module App.Projects.View

open FSharp.ViewEngine
open App.Common.View
open type Html

type ProjectCardProps =
    { title: string
      description: string
      logoSrc: string
      logoAlt: string
      href: string
      label: string }

let private projectCard (props: ProjectCardProps) =
    a {
        _href props.href
        _class "group block p-6 rounded-2xl border border-gray-300/60 dark:border-gray-700/60 transition-colors hover:bg-white dark:hover:bg-gray-800"
        div {
            _class "flex items-center gap-4"
            img {
                _class "h-12 w-12 rounded-xl object-cover bg-gray-100 dark:bg-gray-900 p-1"
                _src props.logoSrc
                _alt props.logoAlt
            }
            h2 { _class "text-2xl font-semibold tracking-tight text-gray-900 dark:text-gray-100"; props.title }
        }
        p { _class "mt-4 text-base text-gray-700 dark:text-gray-300"; props.description }
        p { _class "mt-5 text-sm text-gray-600 dark:text-gray-200 transition-colors group-hover:text-emerald-600 dark:group-hover:text-emerald-400"; props.label }
    }

let page =
    let content =
        div {
            _class "mx-auto max-w-5xl py-10 px-4"
            h1 { _class "text-4xl text-gray-900 dark:text-gray-100 font-medium"; "Projects" }
            p { _class "mt-4 text-lg text-gray-600 dark:text-gray-400"; "Things I am working on." }
            div {
                _class "mt-10 grid gap-8 md:grid-cols-2"
                projectCard {
                    title = "FSharp.ViewEngine"
                    description = "A minimal, fast view engine for F# with a clean computation-expression DSL."
                    logoSrc = Asset.fingerprinted "/images/fsharpviewengine.svg"
                    logoAlt = "FSharp.ViewEngine logo"
                    href = "https://fsharpviewengine.meiermade.com"
                    label = "fsharpviewengine.meiermade.com"
                }
                projectCard {
                    title = "Geldos"
                    description = "A financial operating system for building modern finance and accounting workflows."
                    logoSrc = Asset.fingerprinted "/images/geldos.svg"
                    logoAlt = "Geldos logo"
                    href = "https://geldos.com"
                    label = "geldos.com"
                }
            }
        }
    Page.primary content
