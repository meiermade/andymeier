module App.Projects.Handler

open App.Infrastructure
open App.Common.Handler
open App.ServiceRegistry
open App.Projects
open Giraffe
open Microsoft.AspNetCore.Http
open StarFederation.Datastar.DependencyInjection

let private getPage (services:Services) : HttpHandler =
    fun next ctx -> task {
        use _span = services.telemetry.startActiveSpan "app.projects.get_page"
        let page = View.page
        if ctx.IsDatastar then
            let ds = ctx.GetService<IDatastarService>()
            do! patchElement ds page
            do! pushUrl ds "/projects"
            do! patchSignals ds {| selectedNav = "nav-projects" |}
            return Some ctx
        else
            return! renderPage page "nav-projects" next ctx
    }

let handler (services:Services) : HttpHandler =
    route "/projects" >=> GET >=> getPage services
