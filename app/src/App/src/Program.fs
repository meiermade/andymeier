open App
open App.ServiceRegistry
open Domain
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open OpenTelemetry
open OpenTelemetry.Exporter
open OpenTelemetry.Resources
open OpenTelemetry.Trace
open Serilog
open Serilog.Core
open Serilog.Events
open StarFederation.Datastar.DependencyInjection
open System

let configureTracerProvider (config: Config) =
    Sdk
        .CreateTracerProviderBuilder()
        .AddSource(config.appName)
        .ConfigureResource(fun resourceBuilder ->
            resourceBuilder.AddService(serviceName = config.appName) |> ignore)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(fun opts ->
            opts.Endpoint <- Uri(config.seq.endpoint + "/ingest/otlp/v1/traces")
            opts.Protocol <- OtlpExportProtocol.HttpProtobuf
            opts.Headers <- $"X-Seq-ApiKey={config.seq.apiKey}")
        .Build()

let configureLogger (config: Config) =
    let initialLogLevel =
        if config.debug then LogEventLevel.Debug
        else LogEventLevel.Information

    let levelSwitch = LoggingLevelSwitch(initialLogLevel)

    let logger =
        LoggerConfiguration()
            .MinimumLevel.ControlledBy(levelSwitch)
            .WriteTo.Console()
            .WriteTo.Seq(serverUrl = config.seq.endpoint, apiKey = config.seq.apiKey, controlLevelSwitch = levelSwitch)
            .CreateLogger()

    Log.Logger <- logger

let configureServices (services: IServiceCollection) =
    services
        .AddSerilog()
        .AddDatastar()
        .AddGiraffe()
    |> ignore

let configureApp (services: Services) (app: WebApplication) =
    app
        .UseStaticFiles()
        .UseGiraffe(Index.Handler.handler services)

[<EntryPoint>]
let main _args =
    let config = Config.load ()
    configureLogger config

    try
        try
            let tracerProvider = configureTracerProvider config
            let tracer = tracerProvider.GetTracer(config.appName)
            let services = Services.create config tracer

            let builder = WebApplication.CreateBuilder()
            builder.Services.AddSerilog() |> ignore
            builder.Services.AddSingleton(tracerProvider) |> ignore
            builder.Services.AddHostedService(fun _ -> Article.SyncBackgroundService(services.article)) |> ignore
            configureServices builder.Services
            let app = builder.Build()

            configureApp services app
            Log.Information("Starting {AppName}", config.appName)
            app.Run(config.server.url)
            0
        with ex ->
            Log.Fatal(ex, "Application start-up failed")
            1
    finally
        Log.CloseAndFlush()
