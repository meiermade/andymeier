module App.Handler

open App.Component
open Giraffe
open FSharp.ViewEngine
open Microsoft.AspNetCore.Http
open StarFederation.Datastar.DependencyInjection
open System.IO

let patchElement (ds:IDatastarService) (element:HtmlElement) = task {
    let html = Render.toString element
    do! ds.PatchElementsAsync(html)
}

let inline patchSignals (ds:IDatastarService) (signals:'T) = task {
    do! ds.PatchSignalsAsync(signals)
}

let pushUrl (ds:IDatastarService) (url:string) = task {
    // language=javascript
    let js = $"""window.history.pushState(null, '', '{url}');"""
    do! ds.ExecuteScriptAsync(js)
}

let private renderPage (page:HtmlElement) (selectedNav:string) : HttpHandler =
    fun next ctx -> task {
        let doc = Document.primary(page, selectedNav=selectedNav)
        let html = Render.toHtmlDocString doc
        return! htmlString html next ctx
    }

module Article =
    let private getArticlesPage (articleService:Article.Service) : HttpHandler =
        fun next ctx -> task {
            let articles = articleService.listArticles ()
            let page = View.Article.articlesPage articles
            if ctx.IsDatastar then
                let ds = ctx.GetService<IDatastarService>()
                do! patchElement ds page
                do! pushUrl ds "/articles"
                do! patchSignals ds {| selectedNav = "nav-articles" |}
                return Some ctx
            else
                return! renderPage page "nav-articles" next ctx
        }

    let private getArticlePage (articleService:Article.Service) (id:string) : HttpHandler =
        fun next ctx -> task {
            match articleService.tryGetArticle id with
            | Some article ->
                let page = View.Article.articlePage article
                let url = $"/articles/{article.properties.permalink}"
                if ctx.IsDatastar then
                    let ds = ctx.GetService<IDatastarService>()
                    do! patchElement ds page
                    do! pushUrl ds url
                    do! patchSignals ds {| selectedNav = "nav-articles" |}
                    return Some ctx
                else
                    return! renderPage page "nav-articles" next ctx
            | None ->
                let page = View.Article.notFoundPage
                if ctx.IsDatastar then
                    let ds = ctx.GetService<IDatastarService>()
                    do! patchElement ds page
                    return Some ctx
                else
                    return! renderPage page "nav-articles" next ctx
        }

    let private getArticleAsset (articlesDir:string) (permalink:string) (filename:string) : HttpHandler =
        fun _next ctx -> task {
            let filePath = Path.Combine(articlesDir, permalink, filename)
            if File.Exists(filePath) then
                return! ctx.WriteFileStreamAsync(true, filePath, None, None)
            else
                return None
        }

    let handler (articleService:Article.Service) (articlesDir:string) : HttpHandler =
        choose [
            routex "(/?)" >=> GET >=> getArticlesPage articleService
            routef "/%s/assets/%s" (fun (permalink, filename) ->
                GET >=> getArticleAsset articlesDir permalink filename)
            routef "/%s" (fun id -> GET >=> getArticlePage articleService id)
        ]

module Index =
    let getHomePage : HttpHandler =
        fun next ctx -> task {
            let page = View.Index.aboutPage
            if ctx.IsDatastar then
                let ds = ctx.GetService<IDatastarService>()
                do! patchElement ds page
                do! pushUrl ds "/"
                do! patchSignals ds {| selectedNav = "nav-home" |}
                return Some ctx
            else
                return! renderPage page "nav-home" next ctx
        }

    let getAboutPage : HttpHandler =
        fun next ctx -> task {
            let page = View.Index.aboutPage
            if ctx.IsDatastar then
                let ds = ctx.GetService<IDatastarService>()
                do! patchElement ds page
                do! pushUrl ds "/about"
                do! patchSignals ds {| selectedNav = "nav-about" |}
                return Some ctx
            else
                return! renderPage page "nav-about" next ctx
        }

    let handler (articleService:Article.Service) (articlesDir:string) : HttpHandler =
        choose [
            route "/healthz" >=> GET >=> text "Healthy"
            routex "(/?)" >=> GET >=> getHomePage
            route "/about" >=> GET >=> getAboutPage
            subRoute "/articles" (Article.handler articleService articlesDir)
        ]
