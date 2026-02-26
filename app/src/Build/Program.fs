open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO
open Fake.IO.FileSystemOperators
open System
open System.Text.Json
open System.Threading.Tasks

Environment.GetCommandLineArgs()
|> Array.tail
|> Array.toList
|> Context.FakeExecutionContext.Create false "build.fsx"
|> Context.Fake
|> Context.setExecutionContext

let srcDir = Path.getDirectory __SOURCE_DIRECTORY__
let rootDir = Path.getDirectory srcDir
let appDir = srcDir </> "App"

let inline (==>!) x y = x ==> y |> ignore

let pulumi args =
    CreateProcess.fromRawCommand "pulumi" args
    |> CreateProcess.withWorkingDirectory rootDir
    |> CreateProcess.redirectOutput
    |> CreateProcess.ensureExitCode
    |> Proc.start

let getEnvMap () =
    let env = "andrewmeier/local"
    let pulumiEnv = pulumi ["env"; "open"; env]
    let rawPulumiEnvOutput = pulumiEnv.Result.Result.Output
    let pulumiEnvOutput = JsonSerializer.Deserialize<{| environmentVariables:Map<string,string> |}>(rawPulumiEnvOutput)
    pulumiEnvOutput.environmentVariables

let tailwindcss workDir args =
    CreateProcess.fromRawCommand "tailwindcss" args
    |> CreateProcess.withWorkingDirectory workDir
    |> CreateProcess.ensureExitCode
    |> Proc.start

let dotnetEnv workDir env args =
    CreateProcess.fromRawCommand "dotnet" args
    |> CreateProcess.withWorkingDirectory workDir
    |> CreateProcess.withEnvironmentMap env
    |> CreateProcess.ensureExitCode
    |> Proc.start

let docker workDir args =
    CreateProcess.fromRawCommand "docker" args
    |> CreateProcess.withWorkingDirectory workDir
    |> CreateProcess.ensureExitCode
    |> Proc.start

let dotnet workDir args =
    CreateProcess.fromRawCommand "dotnet" args
    |> CreateProcess.withWorkingDirectory workDir
    |> CreateProcess.ensureExitCode
    |> Proc.start

Target.create "StartDeps" <| fun _ ->
    Trace.trace "Starting dependencies (seq, redis)"
    docker rootDir ["compose"; "up"; "-d"; "seq"; "redis"] |> Task.WaitAll

Target.create "EnsureDevCert" <| fun _ ->
    Trace.trace "Ensuring trusted ASP.NET Core HTTPS development certificate"
    dotnet rootDir ["dev-certs"; "https"; "--trust"] |> Task.WaitAll

Target.create "Watch" <| fun _ ->
    let env =
        getEnvMap()
        |> Map.add "ASPNETCORE_ENVIRONMENT" "Development"
        |> Map.add "SERVER_URL" "https://localhost:5000"
        |> EnvMap.ofMap

    let watchCss = tailwindcss appDir ["--input"; "./input.css"; "--output"; "./wwwroot/css/compiled.css"; "--watch"]
    let watchServer = dotnetEnv appDir env ["watch"; "run"; "--no-restore"]
    Task.WaitAny(watchCss, watchServer) |> ignore

Target.create "BuildCss" <| fun _ ->
    let buildCss = tailwindcss appDir ["--input"; "./input.css"; "--output"; "./wwwroot/css/compiled.css"; "--minify"]
    buildCss.Wait()

Target.create "Test" <| fun _ ->
    let tests = dotnet rootDir ["run"; "--project"; "src/Tests/Tests.fsproj"]
    tests.Wait()

Target.create "Publish" <| fun _ ->
    let publish = dotnet appDir [
        "publish"
        "--output"; "./out"
        "--self-contained"; "false"
    ]
    publish.Wait()

Target.create "Default" (fun _ -> Target.listAvailable())

"StartDeps" ==>! "EnsureDevCert"
"EnsureDevCert" ==>! "Watch"

"BuildCss" ==>! "Publish"

Target.runOrDefaultWithArguments "Default"
