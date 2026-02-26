module App.Common.Handler

open App.Common.View
open Giraffe
open FSharp.ViewEngine
open Microsoft.AspNetCore.Http
open StarFederation.Datastar.DependencyInjection

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

let renderPage (page:HtmlElement) (selectedNav:string) : HttpHandler =
    fun next ctx -> task {
        let doc = Document.primary(page, selectedNav=selectedNav)
        let html = Render.toHtmlDocString doc
        return! htmlString html next ctx
    }
