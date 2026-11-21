module App.Common.Handler

open App.Common.View
open App.ServiceRegistry
open FSharp.ViewEngine
open Giraffe

let renderPage (services:Services) (metadata:PageMetadata) (page:HtmlElement) (selectedNav:string) : HttpHandler =
    fun next ctx -> task {
        let privacyPolicy = App.Privacy.fromRequest ctx
        let doc = Document.primary(metadata, page, services.config.analytics.otelEndpoint, privacyPolicy, selectedNav)
        let html = Render.toHtmlDocString doc
        return! htmlString html next ctx
    }
