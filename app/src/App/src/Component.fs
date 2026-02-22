module App.Component

open FSharp.ViewEngine
open type Html
open type Datastar

module MiniIcon =
    let github =
        raw """
        <svg viewBox="0 0 24 24" aria-hidden="true" class="h-6 w-6" fill="currentColor">
            <path fill-rule="evenodd" clip-rule="evenodd" d="M12 2C6.477 2 2 6.463 2 11.97c0 4.404 2.865 8.14 6.839 9.458.5.092.682-.216.682-.48 0-.236-.008-.864-.013-1.695-2.782.602-3.369-1.337-3.369-1.337-.454-1.151-1.11-1.458-1.11-1.458-.908-.618.069-.606.069-.606 1.003.07 1.531 1.027 1.531 1.027.892 1.524 2.341 1.084 2.91.828.092-.643.35-1.083.636-1.332-2.22-.251-4.555-1.107-4.555-4.927 0-1.088.39-1.979 1.029-2.675-.103-.252-.446-1.266.098-2.638 0 0 .84-.268 2.75 1.022A9.607 9.607 0 0 1 12 6.82c.85.004 1.705.114 2.504.336 1.909-1.29 2.747-1.022 2.747-1.022.546 1.372.202 2.386.1 2.638.64.696 1.028 1.587 1.028 2.675 0 3.83-2.339 4.673-4.566 4.92.359.307.678.915.678 1.846 0 1.332-.012 2.407-.012 2.734 0 .267.18.577.688.48 3.97-1.32 6.833-5.054 6.833-9.458C22 6.463 17.522 2 12 2Z"></path>
        </svg>
        """

    let twitter =
        raw """
        <svg viewBox="0 0 20 20" aria-hidden="true" class="h-5 w-5" fill="currentColor">
            <path d="M6.29 18.251c7.547 0 11.675-6.253 11.675-11.675 0-.178 0-.355-.012-.53A8.348 8.348 0 0 0 20 3.92a8.19 8.19 0 0 1-2.357.646 4.118 4.118 0 0 0 1.804-2.27 8.224 8.224 0 0 1-2.605.996 4.107 4.107 0 0 0-6.993 3.743 11.65 11.65 0 0 1-8.457-4.287 4.106 4.106 0 0 0 1.27 5.477A4.073 4.073 0 0 1 .8 7.713v.052a4.105 4.105 0 0 0 3.292 4.022 4.095 4.095 0 0 1-1.853.07 4.108 4.108 0 0 0 3.834 2.85A8.233 8.233 0 0 1 0 16.407a11.615 11.615 0 0 0 6.29 1.84"></path>
        </svg>
        """

    let linkedIn =
        raw """
        <svg viewBox="0 0 24 24" aria-hidden="true" class="h-6 w-6" fill="currentColor">
            <path d="M18.335 18.339H15.67v-4.177c0-.996-.02-2.278-1.39-2.278-1.389 0-1.601 1.084-1.601 2.205v4.25h-2.666V9.75h2.56v1.17h.035c.358-.674 1.228-1.387 2.528-1.387 2.7 0 3.2 1.778 3.2 4.091v4.715zM7.003 8.575a1.546 1.546 0 01-1.548-1.549 1.548 1.548 0 111.547 1.549zm1.336 9.764H5.666V9.75H8.34v8.589zM19.67 3H4.329C3.593 3 3 3.58 3 4.297v15.406C3 20.42 3.594 21 4.328 21h15.338C20.4 21 21 20.42 21 19.703V4.297C21 3.58 20.4 3 19.666 3h.003z"></path>
        </svg>
        """

    let chevronRight =
        raw """
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" class="w-5 h-5">
          <path fill-rule="evenodd" d="M7.21 14.77a.75.75 0 01.02-1.06L11.168 10 7.23 6.29a.75.75 0 111.04-1.08l4.5 4.25a.75.75 0 010 1.08l-4.5 4.25a.75.75 0 01-1.06-.02z" clip-rule="evenodd" />
        </svg>
        """

    let logo =
        raw """
        <svg xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 32 32" width="18" height="18">
            <path d="M18.598 2.064a1.376 1.376 0 0 0-1.307.975l-3.262 10.656-3.054-5.974a1.376 1.376 0 0 0-2.567.322L3.805 28.256l2.683.611 3.762-16.52 2.877 5.624a1.376 1.376 0 0 0 2.541-.223l2.975-9.72 4.648 14.411a1.376 1.376 0 0 0 2.61.026l2.433-7.084 1.062.365-.96-4.9-.354.306-3.414 2.971 1.062.365-1.09 3.176-4.724-14.646a1.376 1.376 0 0 0-1.318-.954z"/>
        </svg>
        """

    let sun =
        raw """<svg class="h-5 w-5 dark:hidden" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M12 3v2.25m6.364.386-1.591 1.591M21 12h-2.25m-.386 6.364-1.591-1.591M12 18.75V21m-4.773-4.227-1.591 1.591M5.25 12H3m4.227-4.773L5.636 5.636M15.75 12a3.75 3.75 0 1 1-7.5 0 3.75 3.75 0 0 1 7.5 0Z"/></svg>"""

    let moon =
        raw """<svg class="hidden h-5 w-5 dark:block" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M21.752 15.002A9.72 9.72 0 0 1 18 15.75c-5.385 0-9.75-4.365-9.75-9.75 0-1.33.266-2.597.748-3.752A9.753 9.753 0 0 0 3 11.25C3 16.635 7.365 21 12.75 21a9.753 9.753 0 0 0 9.002-5.998Z"/></svg>"""

module OutlineIcon =
    let logo =
        raw """
        <svg xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 32 32" width="24" height="24">
            <path d="M18.598 2.064a1.376 1.376 0 0 0-1.307.975l-3.262 10.656-3.054-5.974a1.376 1.376 0 0 0-2.567.322L3.805 28.256l2.683.611 3.762-16.52 2.877 5.624a1.376 1.376 0 0 0 2.541-.223l2.975-9.72 4.648 14.411a1.376 1.376 0 0 0 2.61.026l2.433-7.084 1.062.365-.96-4.9-.354.306-3.414 2.971 1.062.365-1.09 3.176-4.724-14.646a1.376 1.376 0 0 0-1.318-.954z"/>
        </svg>
        """

module Footer =
    let primary =
        div {
            _class "flex p-10 bg-gray-50 border-t border-gray-300 dark:bg-gray-900 dark:border-gray-700 dark:text-gray-300"
            div {
                _class "text-sm space-y-1"
                div { _class "w-12 h-12 text-emerald-800 dark:text-emerald-400"; OutlineIcon.logo }
                p { "Andrew Meier" }
                p { $"Copyright © {System.DateTime.Now.Year} - All right reserved" }
            }
            div { _class "grow" }
            div {
                _class "text-sm flex flex-col space-y-1"
                a { _class "underline cursor-pointer hover:text-emerald-600 dark:hover:text-emerald-400"; _dsOn ("click", "@get('/articles')"); "Articles" }
                a { _class "underline cursor-pointer hover:text-emerald-600 dark:hover:text-emerald-400"; _dsOn ("click", "@get('/about')"); "About" }
            }
        }

module TopNav =
    let private item (id:string, el:HtmlElement, href:string) =
        a {
            _id id
            _class "p-2 text-sm font-semibold text-gray-800 cursor-pointer hover:text-emerald-600 dark:text-gray-200 dark:hover:text-emerald-400"
            _dsOn ("click", $"@get('{href}')")
            el
        }

    let private themeToggle =
        button {
            _id "theme-toggle"
            _type "button"
            _class [
                "p-2 rounded-md text-gray-600 hover:text-emerald-600 hover:bg-gray-100"
                "dark:text-gray-400 dark:hover:text-emerald-400 dark:hover:bg-gray-800"
            ]
            { Name = "onclick"; Value = ValueSome "toggleTheme()" }
            MiniIcon.sun
            MiniIcon.moon
        }

    let primary =
        nav {
            _class "bg-gray-50 py-2 px-4 lg:px-6 flex items-center gap-4 border-b border-gray-300 dark:bg-gray-900 dark:border-gray-700"
            item("nav-home", OutlineIcon.logo, "/")
            div { _class "grow" }
            item("nav-articles", text "Articles", "/articles")
            item("nav-about", text "About", "/about")
            themeToggle
        }

module Page =
    let primary (page:HtmlElement) =
        div { _id "page"; _class "min-h-screen bg-gray-50 dark:bg-gray-900"; page }

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
                script { _type "module"; _src "https://cdn.jsdelivr.net/gh/starfederation/datastar@1.0.0-RC.6/bundles/datastar.js" }
            }
            body {
                _dsSignals ("selectedNav", $"'{selectedNav}'")
                _class "bg-gray-100 dark:bg-gray-950"
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
