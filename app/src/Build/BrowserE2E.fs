module BrowserE2E

open System
open System.IO
open System.Net.Http
open System.Text.Json
open System.Threading
open System.Threading.Tasks

[<RequireQualifiedAccess>]
type AnalyticsMode =
    | DefaultOn
    | OptIn

let modeValue = function
    | AnalyticsMode.DefaultOn -> "default-on"
    | AnalyticsMode.OptIn -> "opt-in"

let playwrightPackageVersion (packageJson:string) =
    use document = JsonDocument.Parse packageJson
    document.RootElement
        .GetProperty("devDependencies")
        .GetProperty("@playwright/test")
        .GetString()
    |> Option.ofObj
    |> Option.defaultWith (fun () -> invalidOp "@playwright/test must have an explicit version.")

let verifyPlaywrightImage packageVersion (image:string) =
    if not (image.Contains($":v{packageVersion}-", StringComparison.Ordinal)) then
        invalidOp $"Playwright image {image} does not match @playwright/test {packageVersion}."

let npmInstallCommand e2eDirectory =
    { BuildProcess.create "npm" [ "ci" ] with
        workingDirectory = e2eDirectory
        timeout = TimeSpan.FromMinutes 5. }

let playwrightCommand e2eDirectory image baseUrl scope expectedMode analyticsEnabled serverContainer =
    let environment =
        [ "--env"; "CI=true"
          "--env"; $"E2E_ANALYTICS_ENABLED={analyticsEnabled}"
          "--env"; $"E2E_SCOPE={scope}"
          "--env"; $"SITE_E2E_BASE_URL={baseUrl}" ]
        @ (expectedMode
           |> Option.map (fun mode -> [ "--env"; $"E2E_EXPECTED_ANALYTICS_MODE={modeValue mode}" ])
           |> Option.defaultValue [])

    { BuildProcess.create "docker" (
        [ "run"
          "--rm"
          "--init"
          "--ipc=host" ]
        @ (serverContainer
           |> Option.map (fun name -> [ "--network"; $"container:{name}" ])
           |> Option.defaultValue [])
        @ environment
        @ [ "--volume"; $"{Path.GetFullPath e2eDirectory}:/work"
            "--workdir"; "/work"
            image
            "npm"; "test"; "--"
            "--project=firefox"
            "--retries=0" ]) with
        workingDirectory = e2eDirectory }

let countryFromEdgeTrace (content:string) =
    content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
    |> Array.tryPick (fun line ->
        let parts = line.Trim().Split('=', 2)
        if parts.Length = 2 && parts[0] = "loc" then Some parts[1]
        else None)

let private runLogged log command =
    let result = BuildProcess.runChecked BuildProcess.run command
    if not (String.IsNullOrWhiteSpace result.standardOutput) then log result.standardOutput
    if not (String.IsNullOrWhiteSpace result.standardError) then log result.standardError
    result

let private waitForEndpoint log (url:string) =
    use client = new HttpClient(Timeout = TimeSpan.FromSeconds 5.)
    let mutable ready = false
    let mutable attempt = 1

    while not ready && attempt <= 60 do
        try
            use response = client.GetAsync(url).GetAwaiter().GetResult()
            ready <- response.IsSuccessStatusCode
        with
        | :? HttpRequestException -> ()
        | :? TaskCanceledException -> ()

        if not ready then Thread.Sleep 1000
        attempt <- attempt + 1

    if not ready then invalidOp $"Endpoint did not become ready at {url}."
    log $"Endpoint ready at {url}."

let buildImageCommand rootDirectory imageTag =
    { BuildProcess.create "docker" [
        "build"
        "--tag"; imageTag
        "--file"; "app/Dockerfile"
        "app" ] with
        workingDirectory = rootDirectory }

let localServerCommand imageTag containerName baseUrl analyticsEnabled =
    let port = Uri(baseUrl).Port
    BuildProcess.create "docker" [
        "run"
        "--detach"
        "--name"; containerName
        "--publish"; $"127.0.0.1:{port}:5000"
        "--env"; $"ANALYTICS_ENABLED={analyticsEnabled}"
        "--env"; "ASPNETCORE_ENVIRONMENT=Production"
        "--env"; "OTEL_EXPORTER_OTLP_ENDPOINT=http://127.0.0.1:4318"
        "--env"; "PUBLIC_OTEL_EXPORTER_OTLP_ENDPOINT=https://otel.test"
        "--env"; "SERVER_URL=http://0.0.0.0:5000"
        imageTag ]

let private cleanup log arguments =
    let result = BuildProcess.run (BuildProcess.create "docker" arguments)
    if result.exitCode <> 0 then log result.standardError

let runLocal log rootDirectory e2eDirectory image baseUrl stateDirectory =
    let runId = Guid.NewGuid().ToString("N")
    let candidateImage = $"andymeier-e2e:{runId}"
    Directory.CreateDirectory stateDirectory |> ignore
    runLogged log (npmInstallCommand e2eDirectory) |> ignore

    try
        runLogged log (buildImageCommand rootDirectory candidateImage) |> ignore
        for analyticsEnabled in [ "false"; "true" ] do
            let containerName = $"andymeier-e2e-{runId}-{analyticsEnabled}"
            let logPath = Path.Combine(stateDirectory, $"server-analytics-{analyticsEnabled}.log")

            try
                runLogged log (localServerCommand candidateImage containerName baseUrl analyticsEnabled) |> ignore
                waitForEndpoint log $"{baseUrl}/health"
                // Sharing the server network keeps acceptance requests on loopback, including
                // the trusted forwarded-scheme check, on both local Docker and Linux CI.
                playwrightCommand e2eDirectory image "http://127.0.0.1:5000" "local" None analyticsEnabled (Some containerName)
                |> runLogged log
                |> ignore
            finally
                let logs = BuildProcess.run (BuildProcess.create "docker" [ "logs"; containerName ])
                File.WriteAllText(logPath, logs.standardOutput + Environment.NewLine + logs.standardError)
                cleanup log [ "rm"; "--force"; containerName ]
    finally
        cleanup log [ "image"; "rm"; candidateImage ]

let private verifyActualUsContext log baseUrl =
    use client = new HttpClient(Timeout = TimeSpan.FromSeconds 15.)
    let trace = client.GetStringAsync($"{baseUrl}/cdn-cgi/trace").GetAwaiter().GetResult()
    match countryFromEdgeTrace trace with
    | Some "US" -> log "Cloudflare confirmed the deployed browser runner is in the U.S."
    | country ->
        let actual = country |> Option.defaultValue "unknown"
        invalidOp $"Expected a U.S. deployed browser context, received {actual}."

let runPublished log e2eDirectory image baseUrl =
    waitForEndpoint log $"{baseUrl}/health"
    runLogged log (npmInstallCommand e2eDirectory) |> ignore
    use client = new HttpClient(Timeout = TimeSpan.FromSeconds 15.)
    (RegionalAnalytics.verify log client baseUrl).GetAwaiter().GetResult()
    verifyActualUsContext log baseUrl
    playwrightCommand e2eDirectory image baseUrl "deployed" (Some AnalyticsMode.DefaultOn) "true" None
    |> runLogged log
    |> ignore
