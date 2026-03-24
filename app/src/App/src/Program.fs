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
        .AddAspNetCoreInstrumentation(fun opts ->
            opts.Filter <- fun ctx -> ctx.Request.Path.Value <> "/health")
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(fun opts ->
            opts.Endpoint <- Uri(config.seq.endpoint + "/ingest/otlp/v1/traces")
            opts.Protocol <- OtlpExportProtocol.HttpProtobuf)
        .Build()

let configureLogger (config: Config) =
    let initialLogLevel =
        if config.debug then LogEventLevel.Debug
        else LogEventLevel.Information

    let levelSwitch = LoggingLevelSwitch(initialLogLevel)

    let logger =
        LoggerConfiguration()
            .MinimumLevel.ControlledBy(levelSwitch)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .WriteTo.Console()
            .WriteTo.Seq(serverUrl = config.seq.endpoint, controlLevelSwitch = levelSwitch)
            .CreateLogger()

    Log.Logger <- logger

let configureServices (serviceCollection: IServiceCollection) (tracerProvider: TracerProvider) (services: Services) =
    serviceCollection
        .AddSerilog()
        .AddSingleton(tracerProvider)
        .AddHostedService(fun _ -> Article.SyncBackgroundService(services.article))
        .AddDatastar()
        .AddGiraffe()
    |> ignore

let configureApp (services: Services) (app: WebApplication) =
    app
        .UseSerilogRequestLogging(fun opts ->
            opts.GetLevel <- fun ctx _ _ ->
                if ctx.Request.Path.Value = "/health" then LogEventLevel.Verbose
                else LogEventLevel.Information)
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
            configureServices builder.Services tracerProvider services
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
