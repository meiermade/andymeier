module App.ServiceRegistry

open Domain
open OpenTelemetry.Trace
open System
open System.Net.Http

type Services =
    { config: Config
      telemetry: Telemetry.Service
      article: Article.Service }

module Services =
    let create (config: Config) (tracer: Tracer) =
        let telemetry = Telemetry.Service.create tracer
        let redis = Redis.Service.create config.redis telemetry
        let socketsHandler = new SocketsHttpHandler(PooledConnectionLifetime = TimeSpan.FromHours(1.))
        let httpClient = new HttpClient(socketsHandler)
        let notion = Notion.Service.create config.notion telemetry httpClient
        let article = Article.Service.create config.notion telemetry redis notion

        { config = config
          telemetry = telemetry
          article = article }
