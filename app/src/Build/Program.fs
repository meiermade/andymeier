open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO
open Fake.IO.FileSystemOperators
open System
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

let tailwindcss workDir args =
    CreateProcess.fromRawCommand "tailwindcss" args
    |> CreateProcess.withWorkingDirectory workDir
    |> CreateProcess.ensureExitCode
    |> Proc.start

let dotnet workDir args =
    CreateProcess.fromRawCommand "dotnet" args
    |> CreateProcess.withWorkingDirectory workDir
    |> CreateProcess.ensureExitCode
    |> Proc.start

Target.create "Watch" <| fun _ ->
    let watchCss = tailwindcss appDir ["--input"; "./input.css"; "--output"; "./wwwroot/css/compiled.css"; "--watch"]
    let watchServer = dotnet appDir ["watch"; "run"; "--no-restore"]
    Task.WaitAny(watchCss, watchServer) |> ignore

Target.create "BuildCss" <| fun _ ->
    let buildCss = tailwindcss appDir ["--input"; "./input.css"; "--output"; "./wwwroot/css/compiled.css"; "--minify"]
    buildCss.Wait()

Target.create "Publish" <| fun _ ->
    let publish = dotnet appDir [
        "publish"
        "--output"; "./out"
        "--self-contained"; "false"
    ]
    publish.Wait()

Target.create "Default" (fun _ -> Target.listAvailable())

"BuildCss" ==>! "Publish"

Target.runOrDefaultWithArguments "Default"
