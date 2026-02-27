module App.Articles.Handler

open App.Infrastructure
open App.ServiceRegistry
open App.Articles.View
open App.Common.Handler
open Giraffe
open Microsoft.AspNetCore.Http
open StarFederation.Datastar.DependencyInjection

let private getArticlesPage (services:Services) : HttpHandler =
    fun next ctx -> task {
        use _span = services.telemetry.startActiveSpan "app.articles.get_articles_page"
        let! articles = services.article.listArticles ()
        let page = articlesPage articles

        if ctx.IsDatastar then
            let ds = ctx.GetService<IDatastarService>()
            do! patchElement ds page
            do! pushUrl ds "/articles"
            do! patchSignals ds {| selectedNav = "nav-articles" |}
            return Some ctx
        else
            return! renderPage page "nav-articles" next ctx
    }

let private getArticlePage (services:Services) (id:string) : HttpHandler =
    fun next ctx -> task {
        use _span = services.telemetry.startActiveSpan "app.articles.get_article_page"
        match! services.article.tryGetArticle id with
        | Some article ->
            let page = articlePage article
            let url = $"/articles/{article.permalink}"

            if ctx.IsDatastar then
                let ds = ctx.GetService<IDatastarService>()
                do! patchElement ds page
                do! pushUrl ds url
                do! patchSignals ds {| selectedNav = "nav-articles" |}
                return Some ctx
            else
                return! renderPage page "nav-articles" next ctx
        | None ->
            let page = notFoundPage
            if ctx.IsDatastar then
                let ds = ctx.GetService<IDatastarService>()
                do! patchElement ds page
                return Some ctx
            else
                return! renderPage page "nav-articles" next ctx
    }

let handler (services:Services) : HttpHandler =
    choose [
        routex "(/?)" >=> GET >=> getArticlesPage services
        routef "/%s" (fun id -> GET >=> getArticlePage services id)
    ]
