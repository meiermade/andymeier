module App.Services.Handler

open App.Infrastructure
open App.ServiceRegistry
open App.Common.Handler
open Giraffe
open Microsoft.AspNetCore.Http
open StarFederation.Datastar.DependencyInjection

let private getPage (services:Services) : HttpHandler =
    fun next ctx -> task {
        use _span = services.telemetry.startActiveSpan "app.services.get_page"
        let page = View.page

        if ctx.IsDatastar then
            let ds = ctx.GetService<IDatastarService>()
            do! patchSignals ds {| selectedNav = "nav-services" |}
            do! patchElement ds page
            do! pushUrl ds "/services"
            return Some ctx
        else
            return! renderPage services page "nav-services" next ctx
    }

let handler (services:Services) : HttpHandler =
    choose [
        route "/services" >=> GET >=> getPage services
    ]
