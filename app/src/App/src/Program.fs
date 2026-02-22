open App
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Localization
open Microsoft.Extensions.DependencyInjection
open OpenTelemetry
open OpenTelemetry.Trace
open OpenTelemetry.Resources
open OpenTelemetry.Exporter
open Serilog
open Serilog.Core
open Serilog.Events
open StarFederation.Datastar.DependencyInjection
open System
open System.Collections.Generic
open System.Globalization
open System.IO

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
        if config.debug then
            LogEventLevel.Debug
        else
            LogEventLevel.Information

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
        .AddHealthChecks()
        .Services.AddGiraffe()
    |> ignore

let configureApp (articleService: Article.Service) (articlesDir: string) (app: WebApplication) =
    let enUSCultureInfo = CultureInfo("en-US")
    let supportedCultures = List([ enUSCultureInfo ])

    let requestLocalizationOptions =
        RequestLocalizationOptions(
            SupportedCultures = supportedCultures,
            DefaultRequestCulture = RequestCulture(enUSCultureInfo)
        )

    app.UseStaticFiles().UseRequestLocalization(requestLocalizationOptions)
    |> ignore

    app.MapHealthChecks("/healthz") |> ignore
    app.UseGiraffe(Handler.Index.handler articleService articlesDir)

[<EntryPoint>]
let main _args =
    let config = Config.load ()
    configureLogger config

    try
        try
            let tracerProvider = configureTracerProvider config

            let articlesDir = Path.Combine(AppContext.BaseDirectory, "articles")
            let articleService = Article.Service.create articlesDir

            let builder = WebApplication.CreateBuilder()
            builder.Services.AddSerilog() |> ignore
            builder.Services.AddSingleton(tracerProvider) |> ignore
            configureServices builder.Services
            let app = builder.Build()

            configureApp articleService articlesDir app
            Log.Information("Starting {AppName}", config.appName)
            app.Run(config.server.url)
            0
        with ex ->
            Log.Fatal(ex, "Application start-up failed")
            1
    finally
        Log.CloseAndFlush()
