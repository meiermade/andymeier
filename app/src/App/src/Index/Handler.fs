module App.Index.Handler

open App.Infrastructure
open App.ServiceRegistry
open App.Common.Handler
open App.Index.View
open Giraffe
open StarFederation.Datastar.DependencyInjection

let private getHomePage (services:Services) : HttpHandler =
    fun next ctx -> task {
        use _span = services.telemetry.startActiveSpan "app.index.get_home_page"
        let! articles = services.article.listArticles ()
        let recentArticles = articles |> List.truncate 3
        let page = homePage recentArticles

        if ctx.IsDatastar then
            let ds = ctx.GetService<IDatastarService>()
            do! patchElement ds page
            do! pushUrl ds "/"
            do! patchSignals ds {| selectedNav = "nav-home" |}
            return Some ctx
        else
            return! renderPage page "nav-home" next ctx
    }

let handler (services:Services) : HttpHandler =
    choose [
        route "/health" >=> GET >=> text "Healthy"
        routex "(/?)" >=> GET >=> getHomePage services
        App.Services.Handler.handler services
        App.Projects.Handler.handler services
        subRoute "/articles" (App.Articles.Handler.handler services)
    ]
