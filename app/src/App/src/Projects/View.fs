module App.Projects.View

open FSharp.ViewEngine
open App.Common.View
open type Html

let private projectCard (title:string, description:string, logoSrc:string, logoAlt:string, href:string, label:string) =
    a {
        _href href
        _class "group block p-6 rounded-2xl border border-slate-400/20 transition-colors hover:bg-white dark:hover:bg-gray-800"
        div {
            _class "flex items-center gap-4"
            img {
                _class "h-12 w-12 rounded-xl object-cover bg-gray-100 dark:bg-gray-900 p-1"
                _src logoSrc
                _alt logoAlt
            }
            h2 { _class "text-2xl font-semibold tracking-tight text-gray-900 dark:text-gray-100"; title }
        }
        p { _class "mt-4 text-base text-gray-700 dark:text-gray-300"; description }
        p { _class "mt-5 text-sm text-gray-600 dark:text-gray-200 transition-colors group-hover:text-emerald-600 dark:group-hover:text-emerald-400"; label }
    }

let page =
    let content =
        div {
            _class "mx-auto max-w-5xl py-10 px-4"
            h1 { _class "text-4xl text-gray-900 dark:text-gray-100 font-medium"; "Projects" }
            p { _class "mt-4 text-lg text-gray-600 dark:text-gray-400"; "Things I am working on." }
            div {
                _class "mt-10 grid gap-8 md:grid-cols-2"
                projectCard(
                    "FSharp.ViewEngine",
                    "A minimal, fast view engine for F# with a clean computation-expression DSL.",
                    "/images/projects/fsharp-viewengine.svg",
                    "FSharp.ViewEngine logo",
                    "https://fsharpviewengine.meiermade.com",
                    "fsharpviewengine.meiermade.com"
                )
                projectCard(
                    "Geldos",
                    "A financial operating system for building modern finance and accounting workflows.",
                    "/images/projects/geldos.png",
                    "Geldos logo",
                    "https://geldos.com",
                    "geldos.com"
                )
            }
        }
    Page.primary content
