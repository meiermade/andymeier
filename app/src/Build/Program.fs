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

let execEnv command workDir env args =
    CreateProcess.fromRawCommand command args
    |> CreateProcess.withWorkingDirectory workDir
    |> CreateProcess.withEnvironmentMap env
    |> CreateProcess.ensureExitCode
    |> Proc.start

let exec command workDir args =
    execEnv command workDir EnvMap.empty args
    
let getEnvMap () =
    let env = "andrewmeier/local"
    let pulumiEnv =
        CreateProcess.fromRawCommand "pulumi" ["env"; "open"; env]
        |> CreateProcess.withWorkingDirectory rootDir
        |> CreateProcess.redirectOutput
        |> CreateProcess.ensureExitCode
        |> Proc.start
    let rawPulumiEnvOutput = pulumiEnv.Result.Result.Output
    let pulumiEnvOutput = JsonSerializer.Deserialize<{| environmentVariables:Map<string,string> |}>(rawPulumiEnvOutput)
    pulumiEnvOutput.environmentVariables

Target.create "StartDeps" <| fun _ ->
    Trace.trace "Starting dependencies (seq)"
    exec "docker-compose" rootDir ["up"; "-d"; "seq"] |> Task.WaitAll

Target.create "EnsureDevCert" <| fun _ ->
    Trace.trace "Ensuring trusted ASP.NET Core HTTPS development certificate"
    exec "dotnet" rootDir ["dev-certs"; "https"; "--trust"] |> Task.WaitAll

Target.create "Watch" <| fun _ ->
    let sqlitePath = appDir </> ".data" </> "app.db"

    let env =
        getEnvMap()
        |> Map.add "ASPNETCORE_ENVIRONMENT" "Development"
        |> Map.add "SERVER_URL" "https://localhost:5000"
        |> Map.add "SQLITE_PATH" sqlitePath
        |> EnvMap.ofMap

    let watchCss = exec "tailwindcss" appDir ["--input"; "./input.css"; "--output"; "./wwwroot/css/compiled.css"; "--watch"]
    let watchServer = execEnv "dotnet" appDir env ["watch"; "run"; "--no-restore"]
    Task.WaitAny(watchCss, watchServer) |> ignore

Target.create "BuildCss" <| fun _ ->
    let buildCss = exec "tailwindcss" appDir ["--input"; "./input.css"; "--output"; "./wwwroot/css/compiled.css"; "--minify"]
    buildCss.Wait()

Target.create "Test" <| fun _ ->
    let tests = exec "dotnet" rootDir ["run"; "--project"; "src/Tests/Tests.fsproj"]
    tests.Wait()

Target.create "Publish" <| fun _ ->
    let publish = exec "dotnet" appDir [
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
