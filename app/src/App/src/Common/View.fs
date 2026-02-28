module App.Common.View

open FSharp.ViewEngine
open Domain.Article
open type Html
open type Datastar

module MiniIcon =
    let github =
        raw """
        <svg viewBox="0 0 24 24" aria-hidden="true" class="h-6 w-6" fill="currentColor">
            <path fill-rule="evenodd" clip-rule="evenodd" d="M12 2C6.477 2 2 6.463 2 11.97c0 4.404 2.865 8.14 6.839 9.458.5.092.682-.216.682-.48 0-.236-.008-.864-.013-1.695-2.782.602-3.369-1.337-3.369-1.337-.454-1.151-1.11-1.458-1.11-1.458-.908-.618.069-.606.069-.606 1.003.07 1.531 1.027 1.531 1.027.892 1.524 2.341 1.084 2.91.828.092-.643.35-1.083.636-1.332-2.22-.251-4.555-1.107-4.555-4.927 0-1.088.39-1.979 1.029-2.675-.103-.252-.446-1.266.098-2.638 0 0 .84-.268 2.75 1.022A9.607 9.607 0 0 1 12 6.82c.85.004 1.705.114 2.504.336 1.909-1.29 2.747-1.022 2.747-1.022.546 1.372.202 2.386.1 2.638.64.696 1.028 1.587 1.028 2.675 0 3.83-2.339 4.673-4.566 4.92.359.307.678.915.678 1.846 0 1.332-.012 2.407-.012 2.734 0 .267.18.577.688.48 3.97-1.32 6.833-5.054 6.833-9.458C22 6.463 17.522 2 12 2Z"></path>
        </svg>
        """

    let xdotcom =
        raw """
        <svg viewBox="0 0 1200 1227" aria-hidden="true" class="h-5 w-5" fill="currentColor">
            <path d="M714.163 519.284L1160.89 0H1055.03L667.137 450.887L357.328 0H0L468.492 681.821L0 1226.37H105.866L515.491 750.218L842.672 1226.37H1200L714.137 519.284H714.163ZM569.165 687.828L521.697 619.934L144.011 79.6944H306.615L611.412 515.685L658.88 583.579L1055.08 1150.3H892.476L569.165 687.854V687.828Z"></path>
        </svg>
        """

    let linkedIn =
        raw """
        <svg viewBox="0 0 24 24" aria-hidden="true" class="h-6 w-6" fill="currentColor">
            <path d="M18.335 18.339H15.67v-4.177c0-.996-.02-2.278-1.39-2.278-1.389 0-1.601 1.084-1.601 2.205v4.25h-2.666V9.75h2.56v1.17h.035c.358-.674 1.228-1.387 2.528-1.387 2.7 0 3.2 1.778 3.2 4.091v4.715zM7.003 8.575a1.546 1.546 0 01-1.548-1.549 1.548 1.548 0 111.547 1.549zm1.336 9.764H5.666V9.75H8.34v8.589zM19.67 3H4.329C3.593 3 3 3.58 3 4.297v15.406C3 20.42 3.594 21 4.328 21h15.338C20.4 21 21 20.42 21 19.703V4.297C21 3.58 20.4 3 19.666 3h.003z"></path>
        </svg>
        """

    let calendar =
        raw """
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" width="16" height="16" class="shrink-0" aria-hidden="true">
          <path stroke-linecap="round" stroke-linejoin="round" d="M6.75 3v2.25M17.25 3v2.25M3 18.75V7.5a2.25 2.25 0 0 1 2.25-2.25h13.5A2.25 2.25 0 0 1 21 7.5v11.25M3 18.75A2.25 2.25 0 0 0 5.25 21h13.5A2.25 2.25 0 0 0 21 18.75M3 18.75v-7.5A2.25 2.25 0 0 1 5.25 9h13.5A2.25 2.25 0 0 1 21 11.25v7.5" />
        </svg>
        """

    let clock =
        raw """
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" width="16" height="16" class="shrink-0" aria-hidden="true">
          <path stroke-linecap="round" stroke-linejoin="round" d="M12 6v6l4 2m5-2a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z" />
        </svg>
        """

    let logo =
        raw """
        <svg version="1.1" viewBox="0 0 650 650" class="w-full h-full" fill="currentColor" aria-hidden="true"><path d="M321.765 141.729c15.302-2.413 33.5 7.639 41.272 25.18 4.622 10.433 8.885 21.227 13.165 31.908l24.773 62.302 42.843 106.053c6.032 15 14.892 35.276 19.322 51.273.567 2.05-.486 10.69-1.396 13.36-5.92 17.37-14.839 39.343-22.538 55.23-5.926 12.225-18.045 19.073-28.732 21.225-11.206 1.994-28.433-4.294-35.669-15.395-7.077-10.856-14.596-32.455-19.72-45.472l-24.578-62.426-36.478-91.774c-7.363-18.472-14.766-36.918-21.958-55.497-2.558-6.607-4.738-14.643-1.706-21.864 13.688-32.607 18.644-68.773 51.4-74.103m139.643.039c10.591-1.07 28.41 6.264 34.927 16.594 8.982 14.235 18.584 41.208 25.314 57.95l36.856 91.66 34.336 85.245c6.838 16.894 16.943 40.613 22.766 57.852 1.662 4.855 2.568 10.084 2.667 15.39.068 4.278-.37 7.754-1.336 11.768-7.717 32.113-31.838 28.16-52.186 28.128-13.24.097-31.09 2.626-42.209-8.166-9.939-9.647-15.55-28.516-21.255-42.848l-21.718-54.146a15601 15601 0 0 1-50.16-124.246c-6.435-16.112-13.052-32.11-19.344-48.306-2.29-5.897-4.1-13.57-1.444-19.838 14.022-33.084 19.4-63.584 52.786-67.037m-278.687-.016c10.607-.558 18.9 2.317 27.98 9.433 7.124 5.58 10.37 11.225 14.23 20.641 8.102 19.764 15.828 39.756 23.564 59.755l34.291 87.692 23.365 58.809c3.597 9.064 11.492 27.413 13.618 36.933 2.995 13.41-12.94 42.493-17.012 55.087-1.763 5.453-6.203 14.923-8.823 19.104-3.186 5.059-7.119 9.247-11.57 12.32-10.46 7.379-22.782 8.999-34.179 4.495-14.467-5.692-19.556-15.45-26.25-32.046-13.388-33.18-25.976-67.331-39.694-100.243l-4.077 9.484-27.858 69.868c-9.942 24.92-17.987 55.505-43.946 53.41-17.945-1.446-49.04 5.575-63.434-8.616-6.218-6.245-10.202-15.49-11.034-25.609-1.462-17.079 6.65-32.608 12.572-47.276l15.345-38.07 48.355-119.624 25.6-63.251c12.357-30.878 18.545-58.49 48.957-62.296" style="stroke-width:1.13671"/></svg>
        """

    let sun =
        raw """<svg class="h-5 w-5 dark:hidden" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M12 3v2.25m6.364.386-1.591 1.591M21 12h-2.25m-.386 6.364-1.591-1.591M12 18.75V21m-4.773-4.227-1.591 1.591M5.25 12H3m4.227-4.773L5.636 5.636M15.75 12a3.75 3.75 0 1 1-7.5 0 3.75 3.75 0 0 1 7.5 0Z"/></svg>"""

    let moon =
        raw """<svg class="hidden h-5 w-5 dark:block" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M21.752 15.002A9.72 9.72 0 0 1 18 15.75c-5.385 0-9.75-4.365-9.75-9.75 0-1.33.266-2.597.748-3.752A9.753 9.753 0 0 0 3 11.25C3 16.635 7.365 21 12.75 21a9.753 9.753 0 0 0 9.002-5.998Z"/></svg>"""

    let hamburger =
        raw """<svg class="h-6 w-6" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5"/></svg>"""

    let close =
        raw """<svg class="h-6 w-6" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M6 18 18 6M6 6l12 12"/></svg>"""

module ArticleCard =
    let private tag (text:string) =
        span {
            _class "inline-flex items-center rounded-md bg-gray-50 px-2 py-1 text-xs font-medium text-gray-600 ring-1 ring-inset ring-gray-500/10 dark:bg-gray-800 dark:text-gray-300 dark:ring-gray-600"
            text
        }

    let summary (article':Article) =
        let url = $"/articles/{article'.permalink}"
        article {
            _id article'.permalink
            _class "py-6 border-b border-gray-200 dark:border-gray-800"
            div {
                _class "flex items-center flex-wrap gap-x-4 gap-y-1 text-sm text-gray-400 dark:text-gray-500"
                div {
                    _class "inline-flex items-center whitespace-nowrap"
                    span { _class "mr-1.5"; MiniIcon.calendar }
                    time {
                        _datetime (article'.createdAt.ToString("yyyy-MM-dd"))
                        article'.createdAt.ToString("MMMM d, yyyy")
                    }
                }
            }
            h2 {
                _class "mt-2 text-xl font-semibold tracking-tight text-gray-900 dark:text-gray-100"
                a {
                    _href url
                    _dsOn ("click", $"@get('{url}')")
                    _class "hover:text-emerald-600 dark:hover:text-emerald-400"
                    article'.title
                }
            }
            p { _class "mt-2 text-base text-gray-600 dark:text-gray-400"; article'.summary }
            div {
                _class "mt-4 flex flex-wrap gap-2"
                for t in article'.tags do tag t
            }
        }

module Footer =
    let primary =
        div {
            _class "flex p-10 bg-gray-50 border-t border-gray-300 dark:bg-gray-900 dark:border-gray-700 dark:text-gray-300"
            div {
                _class "text-sm space-y-1"
                div { _class "w-12 h-12 text-emerald-600 dark:text-emerald-400"; MiniIcon.logo }
                p { "Andrew Meier" }
                p { $"Copyright © {System.DateTime.Now.Year} - All right reserved" }
            }
            div { _class "grow" }
            div {
                _class "text-sm flex flex-col space-y-1"
                a { _class "underline cursor-pointer hover:text-emerald-600 dark:hover:text-emerald-400"; _dsOn ("click", "@get('/articles')"); "Articles" }
                a { _class "underline cursor-pointer hover:text-emerald-600 dark:hover:text-emerald-400"; _dsOn ("click", "@get('/services')"); "Services" }
                a { _class "underline cursor-pointer hover:text-emerald-600 dark:hover:text-emerald-400"; _dsOn ("click", "@get('/projects')"); "Projects" }
            }
        }

module TopNav =
    let private item (id:string, el:HtmlElement, href:string) =
        let baseClass, inactiveLightClass, inactiveDarkClass =
            if id = "nav-home" then
                "p-2 text-base font-bold tracking-tight cursor-pointer hover:text-emerald-600 dark:hover:text-emerald-400", "text-gray-900", "dark:text-gray-100"
            else
                "p-2 text-sm font-semibold cursor-pointer hover:text-emerald-600 dark:hover:text-emerald-400", "text-gray-800", "dark:text-gray-200"

        a {
            _id id
            _class baseClass
            _dsClass ("text-emerald-600", $"$selectedNav == '{id}'")
            _dsClass ("dark:text-emerald-400", $"$selectedNav == '{id}'")
            _dsClass (inactiveLightClass, $"$selectedNav != '{id}'")
            _dsClass (inactiveDarkClass, $"$selectedNav != '{id}'")
            _dsOn ("click", $"@get('{href}')")
            el
        }

    let private mobileItem (id:string, label:string, href:string) =
        a {
            _id $"{id}-mobile"
            _class "block p-3 text-base font-semibold cursor-pointer hover:text-emerald-600 hover:bg-gray-200 dark:hover:text-emerald-400 dark:hover:bg-gray-800 rounded-md"
            _dsClass ("text-emerald-600", $"$selectedNav == '{id}'")
            _dsClass ("dark:text-emerald-400", $"$selectedNav == '{id}'")
            _dsClass ("text-gray-800", $"$selectedNav != '{id}'")
            _dsClass ("dark:text-gray-200", $"$selectedNav != '{id}'")
            _dsOn ("click", $"$menuOpen = false; @get('{href}')")
            text label
        }

    let private themeToggle =
        button {
            _id "theme-toggle"
            _type "button"
            _class [
                "p-2 rounded-md text-gray-600 hover:text-emerald-600 hover:bg-gray-100"
                "dark:text-gray-400 dark:hover:text-emerald-400 dark:hover:bg-gray-800"
                "hover:cursor-pointer"
            ]
            _onclick "toggleTheme()"
            MiniIcon.sun
            MiniIcon.moon
        }

    let private hamburgerButton =
        button {
            _id "menu-toggle"
            _type "button"
            _class [
                "md:hidden p-2 rounded-md text-gray-600 hover:text-emerald-600 hover:bg-gray-100"
                "dark:text-gray-400 dark:hover:text-emerald-400 dark:hover:bg-gray-800"
                "hover:cursor-pointer"
            ]
            _dsOn ("click", "$menuOpen = !$menuOpen")
            div {
                _dsShow "!$menuOpen"
                MiniIcon.hamburger
            }
            div {
                _dsShow "$menuOpen"
                MiniIcon.close
            }
        }

    let primary =
        nav {
            _id "top-nav"
            _class "relative bg-gray-100 py-2 px-4 border-b border-gray-300 dark:bg-gray-900 dark:border-gray-700"
            _dsSignals ("menuOpen", "false")
            div {
                _class "flex items-center gap-4"
                item("nav-home", div { _class "w-8 h-8 text-emerald-600 dark:text-emerald-400"; MiniIcon.logo }, "/")
                div { _class "grow" }
                div {
                    _class "hidden md:flex items-center gap-4"
                    item("nav-articles", text "Articles", "/articles")
                    item("nav-projects", text "Projects", "/projects")
                    item("nav-services", text "Services", "/services")
                }
                themeToggle
                hamburgerButton
            }
            div {
                _id "mobile-menu"
                _class "md:hidden absolute left-0 right-0 top-full z-50 bg-gray-100 border-b border-gray-300 dark:bg-gray-900 dark:border-gray-700 px-4 pt-2 pb-1 shadow-lg"
                _dsShow "$menuOpen"
                mobileItem("nav-articles", "Articles", "/articles")
                mobileItem("nav-projects", "Projects", "/projects")
                mobileItem("nav-services", "Services", "/services")
            }
        }

module Page =
    let primary (page:HtmlElement) =
        div { _id "page"; _class "min-h-screen bg-gray-100 dark:bg-gray-900"; page }

type Document =
    static member primary (page:HtmlElement, ?selectedNav:string) =
        let selectedNav = defaultArg selectedNav ""
        html {
            _lang "en"
            head {
                title "Andrew Meier"
                meta { _charset "UTF-8" }
                meta { _name "viewport"; _content "width=device-width, initial-scale=1.0" }
                script { js "let t=localStorage.getItem('theme');if(t==='dark'||(!t||t==='system')&&window.matchMedia('(prefers-color-scheme: dark)').matches){document.documentElement.classList.add('dark')}" }
                link { _href "/css/compiled.css"; _rel "stylesheet" }
                link { _href "/css/prism.css"; _rel "stylesheet" }
                script { _type "module"; _src "/scripts/datastar.1.0.0-RC.6.js" }
            }
            body {
                _dsSignals ("selectedNav", $"'{selectedNav}'")
                _class "bg-gray-200 dark:bg-gray-950"
                div {
                    _class "mx-auto max-w-7xl"
                    TopNav.primary
                    page
                    Footer.primary
                }
                script { _src "/scripts/prism.1.29.0.js" }
                script { js "function toggleTheme(){var d=document.documentElement,t=d.classList.contains('dark')?'light':'dark';localStorage.setItem('theme',t);d.classList.toggle('dark',t==='dark')}" }
            }
        }
