module App.Common.View

open FSharp.ViewEngine
open App.Components
open System
open System.Collections.Generic
open System.IO
open System.Text.Json
open type Html
open type Datastar

type PageMetadata =
    { canonicalPath:string
      description:string
      title:string }

module Asset =
    let resolveWithManifest (manifest:IReadOnlyDictionary<string, string>) (path:string) =
        match manifest.TryGetValue path with
        | true, resolvedPath -> resolvedPath
        | false, _ -> path

    let private manifest =
        lazy
            let manifestPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "asset-manifest.json")

            if File.Exists(manifestPath) then
                try
                    JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText manifestPath)
                    :> IReadOnlyDictionary<string, string>
                with _ ->
                    Dictionary<string, string>() :> IReadOnlyDictionary<string, string>
            else
                Dictionary<string, string>() :> IReadOnlyDictionary<string, string>

    let fingerprinted (path:string) =
        resolveWithManifest manifest.Value path

module Navigation =
    let initialize = "window.history.scrollRestoration = 'manual'; window.meierMadeScrollUrl = window.location.pathname + window.location.search"

    // Popstate changes the URL before restored HTML arrives; late scroll events still belong to the outgoing page.
    let saveScroll =
        "window.meierMadeScrollUrl === window.location.pathname + window.location.search && window.history.replaceState(Object.assign({}, window.history.state || {}, {meierMadeScrollX: window.scrollX, meierMadeScrollY: window.scrollY}), '', window.location.href)"

    let action (href:string) =
        let url = JsonSerializer.Serialize href
        let request = $"@get({url}, {{filterSignals: {{include: /^$/}}, headers: {{'X-MeierMade-Navigation': 'push'}}}})"
        $"evt.button === 0 && !evt.ctrlKey && !evt.metaKey && !evt.shiftKey && !evt.altKey && !evt.currentTarget.hasAttribute('download') && (!evt.currentTarget.target || evt.currentTarget.target === '_self') && evt.currentTarget.origin === window.location.origin && (evt.preventDefault(), {url} === window.location.pathname + window.location.search || ({saveScroll}, {request}))"

    let restoreAction =
        "window.articleNavigation && document.querySelector('[data-article-url]')?.dataset.articleUrl === window.location.pathname + window.location.search ? window.articleNavigation.restore() : @get(window.location.pathname + window.location.search, {filterSignals: {include: /^$/}, headers: {'X-MeierMade-Navigation': 'restore'}})"

module PageHead =
    let canonicalUrl (metadata:PageMetadata) =
        $"https://andymeier.dev{metadata.canonicalPath}"

    let titleElement (metadata:PageMetadata) =
        title { _id "document-title"; metadata.title }

    let descriptionElement (metadata:PageMetadata) =
        meta { _id "meta-description"; _name "description"; _content metadata.description }

    let openGraphTitleElement (metadata:PageMetadata) =
        meta { _id "open-graph-title"; _property "og:title"; _content metadata.title }

    let openGraphDescriptionElement (metadata:PageMetadata) =
        meta { _id "open-graph-description"; _property "og:description"; _content metadata.description }

    let openGraphUrlElement (metadata:PageMetadata) =
        meta { _id "open-graph-url"; _property "og:url"; _content (canonicalUrl metadata) }

    let canonicalElement (metadata:PageMetadata) =
        link { _id "canonical-url"; _rel "canonical"; _href (canonicalUrl metadata) }

    let patchableElements metadata =
        [ titleElement metadata
          descriptionElement metadata
          openGraphTitleElement metadata
          openGraphDescriptionElement metadata
          openGraphUrlElement metadata
          canonicalElement metadata ]

module SafeOutput =
    let private imageSchemes = Set.ofList [ Uri.UriSchemeHttp; Uri.UriSchemeHttps ]

    let private tryImageUrl (value:string) =
        match Uri.TryCreate(value, UriKind.Absolute) with
        | true, uri when imageSchemes.Contains(uri.Scheme.ToLowerInvariant()) ->
            uri.GetComponents(UriComponents.AbsoluteUri, UriFormat.UriEscaped) |> Some
        | _ -> None

    let tryBackgroundImageStyle (value:string) =
        value
        |> tryImageUrl
        |> Option.map (fun url ->
            let cssUrl =
                url
                    .Replace("\\", "%5C", StringComparison.Ordinal)
                    .Replace("'", "%27", StringComparison.Ordinal)
                    .Replace("\"", "%22", StringComparison.Ordinal)
                    .Replace("(", "%28", StringComparison.Ordinal)
                    .Replace(")", "%29", StringComparison.Ordinal)

            $"background-image: url('{cssUrl}')")

module SiteUrl =
    let article (permalink:string) =
        $"/articles/{Uri.EscapeDataString permalink}"

module Brand =
    let logo =
        raw """
        <svg version="1.1" viewBox="0 0 650 650" style="width:100%;height:100%" fill="currentColor" aria-hidden="true"><path d="M321.765 141.729c15.302-2.413 33.5 7.639 41.272 25.18 4.622 10.433 8.885 21.227 13.165 31.908l24.773 62.302 42.843 106.053c6.032 15 14.892 35.276 19.322 51.273.567 2.05-.486 10.69-1.396 13.36-5.92 17.37-14.839 39.343-22.538 55.23-5.926 12.225-18.045 19.073-28.732 21.225-11.206 1.994-28.433-4.294-35.669-15.395-7.077-10.856-14.596-32.455-19.72-45.472l-24.578-62.426-36.478-91.774c-7.363-18.472-14.766-36.918-21.958-55.497-2.558-6.607-4.738-14.643-1.706-21.864 13.688-32.607 18.644-68.773 51.4-74.103m139.643.039c10.591-1.07 28.41 6.264 34.927 16.594 8.982 14.235 18.584 41.208 25.314 57.95l36.856 91.66 34.336 85.245c6.838 16.894 16.943 40.613 22.766 57.852 1.662 4.855 2.568 10.084 2.667 15.39.068 4.278-.37 7.754-1.336 11.768-7.717 32.113-31.838 28.16-52.186 28.128-13.24.097-31.09 2.626-42.209-8.166-9.939-9.647-15.55-28.516-21.255-42.848l-21.718-54.146a15601 15601 0 0 1-50.16-124.246c-6.435-16.112-13.052-32.11-19.344-48.306-2.29-5.897-4.1-13.57-1.444-19.838 14.022-33.084 19.4-63.584 52.786-67.037m-278.687-.016c10.607-.558 18.9 2.317 27.98 9.433 7.124 5.58 10.37 11.225 14.23 20.641 8.102 19.764 15.828 39.756 23.564 59.755l34.291 87.692 23.365 58.809c3.597 9.064 11.492 27.413 13.618 36.933 2.995 13.41-12.94 42.493-17.012 55.087-1.763 5.453-6.203 14.923-8.823 19.104-3.186 5.059-7.119 9.247-11.57 12.32-10.46 7.379-22.782 8.999-34.179 4.495-14.467-5.692-19.556-15.45-26.25-32.046-13.388-33.18-25.976-67.331-39.694-100.243l-4.077 9.484-27.858 69.868c-9.942 24.92-17.987 55.505-43.946 53.41-17.945-1.446-49.04 5.575-63.434-8.616-6.218-6.245-10.202-15.49-11.034-25.609-1.462-17.079 6.65-32.608 12.572-47.276l15.345-38.07 48.355-119.624 25.6-63.251c12.357-30.878 18.545-58.49 48.957-62.296" style="stroke-width:1.13671"/></svg>
        """

module Footer =
    let primary =
        div {
            _class "flex flex-col gap-6 p-6 sm:flex-row sm:p-10 bg-gray-100 border-t border-gray-300 dark:bg-gray-900 dark:border-gray-700 dark:text-gray-300"
            div {
                _class "text-sm space-y-1"
                div { _class "w-12 h-12 text-emerald-600 dark:text-emerald-400"; Brand.logo }
                p { "Andy Meier" }
                p { $"Copyright © {System.DateTime.Now.Year} - All right reserved" }
            }
            div { _class "grow" }
            div {
                _class "text-sm flex flex-col items-start space-y-1"
                fveLink { _href "/articles"; _dataOn ("click", Navigation.action "/articles"); "Articles" }
                fveLink { _href "/privacy"; _dataOn ("click", Navigation.action "/privacy"); "Privacy" }
                fveLink { _href "https://meiermade.com"; _class "whitespace-nowrap"; "Meier Made" }
                fveButton {
                    _id "analytics-settings"
                    _variant ButtonVariant.Ghost
                    _color Color.Primary
                    _class "items-start! p-0! shadow-none! underline underline-offset-4 hover:decoration-2 hover:bg-transparent! active:bg-transparent!"
                    _ariaControls "cookie-consent-banner"
                    _ariaExpanded "false"
                    "Analytics settings"
                }
            }
        }

module TopNav =
    let private item (id:string, el:HtmlElement, href:string) =
        fveLink {
            _id id
            _href href
            _variant LinkVariant.Ghost
            _color Color.Neutral
            if id = "nav-home" then _ariaLabel "Home"
            _dataAttr ("aria-current", $"$selectedNav == '{id}' ? 'page' : null")
            _class "shadow-none! aria-[current=page]:text-[var(--fve-brand-text)]"
            _dataOn ("click", Navigation.action href)
            el
        }

    let private themeToggle =
        div {
            _attr ("data-signals__ifmissing", "{theme: window.fveColorMode.current()}")
            _dataOn ("fve-color-mode__window", "$theme = window.fveColorMode.current()")
            fveDropdownMenu {
                _id "theme"
                fveDropdownMenuTrigger {
                    fveButton {
                        _id "theme-button"
                        _variant ButtonVariant.Ghost
                        _size Size.Medium
                        _class "shadow-none!"
                        _ariaLabel "Choose theme"
                        span { _class "inline-flex dark:hidden"; fveIcon { _name IconName.Sun; _size Size.Small } }
                        span { _class "hidden dark:inline-flex"; fveIcon { _name IconName.Moon; _size Size.Small } }
                    }
                }
                for mode, label, icon in [ ColorMode.Light, "Light", IconName.Sun; ColorMode.Dark, "Dark", IconName.Moon; ColorMode.System, "System", IconName.ComputerDesktop ] do
                    let value = ColorMode.value mode
                    let initial = mode = ColorMode.System
                    fveDropdownMenuItem {
                        _role "menuitemradio"
                        _ariaChecked initial
                        _dataAttr ("aria-checked", $"$theme == '{value}' ? 'true' : 'false'")
                        _dataOn ("click", $"window.fveColorMode.set('{value}')")
                        fveDropdownMenuItemLeading { fveIcon { _name icon; _size Size.Small } }
                        label
                    }
            }
        }

    let private mobileDropdown =
        div {
            _class "md:hidden"
            fveDropdownMenu {
                _id "navigation"
                fveDropdownMenuTrigger {
                    fveButton {
                        _variant ButtonVariant.Ghost
                        _size Size.Medium
                        _class "shadow-none!"
                        _ariaLabel "Open navigation"
                        fveIcon { _name IconName.Bars3; _size Size.Small }
                    }
                }
                fveDropdownMenuItem {
                    _id "nav-articles-mobile"
                    _href "/articles"
                    _dataAttr ("aria-current", "$selectedNav == 'nav-articles' ? 'page' : null")
                    _class "aria-[current=page]:font-semibold"
                    _dataOn ("click", Navigation.action "/articles")
                    "Articles"
                }
                fveDropdownMenuItem {
                    _href "https://meiermade.com"
                    "Meier Made"
                    fveDropdownMenuItemTrailing { fveIcon { _name IconName.ArrowTopRightOnSquare; _size Size.Small } }
                }
            }
        }

    let primary =
        nav {
            _id "top-nav"
            _class "relative h-14 bg-gray-100/80 py-1.5 px-4 border-b border-gray-300/80 backdrop-blur-md dark:bg-gray-900/80 dark:border-gray-700/80"
            div {
                _class "flex h-full items-center gap-4"
                item("nav-home", div { _class "w-8 h-8 text-emerald-600 dark:text-emerald-400"; Brand.logo }, "/")
                div { _class "grow" }
                div {
                    _class "hidden md:flex items-center gap-4"
                    item("nav-articles", text "Articles", "/articles")
                    fveLink {
                        _href "https://meiermade.com"
                        _variant LinkVariant.Ghost
                        _color Color.Neutral
                        _class "shadow-none!"
                        "Meier Made"
                        fveIcon { _name IconName.ArrowTopRightOnSquare; _size Size.Small }
                    }
                }
                themeToggle
                mobileDropdown
            }
            div {
                _id "article-scroll-progress"
                _role "progressbar"
                _ariaLabel "Article reading progress"
                _attr ("aria-valuemin", "0")
                _attr ("aria-valuemax", "100")
                _attr ("aria-valuenow", "0")
                _class "article-scroll-progress pointer-events-none absolute inset-x-0 -bottom-px h-0.5 origin-left bg-emerald-600 dark:bg-emerald-400"
                _style "transform:scaleX(0)"
            }
        }

module Page =
    let primary (page:HtmlElement) =
        div { _id "page-content"; _tabindex -1; _class "min-h-screen bg-gray-100 dark:bg-gray-900"; page }

module Analytics =
    let banner (policy:App.Privacy.BrowserPolicy) =
        let title, description =
            match policy.analytics with
            | App.Privacy.AnalyticsMode.OptIn ->
                "Optional analytics",
                "Optional browser analytics starts only if you accept."
            | App.Privacy.AnalyticsMode.DefaultOn ->
                "Analytics settings",
                "Limited first-party analytics may run by default in your region."

        div {
            _id "cookie-consent-banner"
            _role "dialog"
            _ariaLabelledby "analytics-consent-title"
            _ariaDescribedby "analytics-consent-description"
            _class "pointer-events-none fixed inset-x-0 bottom-0 z-50 hidden px-4 pb-4 sm:px-6 sm:pb-6"
            div {
                _class "pointer-events-auto ml-auto max-w-xl rounded-2xl border border-gray-200 bg-white p-5 shadow-2xl shadow-gray-950/15 sm:p-6 dark:border-gray-700 dark:bg-gray-900 dark:shadow-black/30"
                h2 {
                    _id "analytics-consent-title"
                    _tabindex -1
                    _class "text-base font-semibold text-gray-950 outline-none dark:text-gray-50"
                    title
                }
                p {
                    _id "analytics-consent-description"
                    _class "mt-2 text-sm/6 text-gray-600 dark:text-gray-300"
                    description
                    " It helps me understand traffic sources, site usage, performance, and errors; declining does not affect the site. A first-party cookie remembers an explicit choice for six months. Read the "
                    fveLink { _href "/privacy#analytics"; "privacy page" }
                    "."
                }
                p {
                    _id "analytics-consent-error"
                    _ariaLive "polite"
                    _class "mt-3 hidden text-sm/6 font-medium text-red-700 dark:text-red-300"
                }
                div {
                    _class "mt-5 flex flex-col gap-3 sm:flex-row sm:justify-end"
                    fveButton { _id "analytics-reject"; "Decline analytics" }
                    fveButton { _id "analytics-accept"; "Accept analytics" }
                }
            }
        }

type Document =
    static member primary (metadata:PageMetadata, page:HtmlElement, otelEndpoint:string option, privacyPolicy:App.Privacy.BrowserPolicy, ?selectedNav:string) =
        let selectedNav = defaultArg selectedNav ""

        html {
            _lang "en"
            _class (ComponentsTheme.emerald |> ComponentsTheme.className)
            head {
                PageHead.titleElement metadata
                meta { _charset "UTF-8" }
                meta { _name "viewport"; _content "width=device-width, initial-scale=1.0" }
                PageHead.descriptionElement metadata
                PageHead.openGraphTitleElement metadata
                PageHead.openGraphDescriptionElement metadata
                meta { _property "og:type"; _content "website" }
                PageHead.openGraphUrlElement metadata
                PageHead.canonicalElement metadata
                ColorMode.assets "theme" ColorMode.System
                script {
                    _id "privacy-controls"
                    _type "module"
                    _src (Asset.fingerprinted "/scripts/privacy.js")
                    _data ("analytics-mode", App.Privacy.analyticsModeValue privacyPolicy)
                    match otelEndpoint with
                    | Some endpoint ->
                        _data ("otel-endpoint", endpoint)
                        _data ("telemetry-src", Asset.fingerprinted "/scripts/telemetry.js")
                    | None -> ()
                }
                link { _href (Asset.fingerprinted "/css/compiled.css"); _rel "stylesheet" }
                link { _href (Asset.fingerprinted "/css/prism.css"); _rel "stylesheet" }
                script { _type "module"; _src (Asset.fingerprinted "/scripts/components.js") }
            }
            body {
                _dataInit Navigation.initialize
                _dataOn ("scroll__window__throttle.100ms.trailing", Navigation.saveScroll)
                _dataOn ("popstate__window", Navigation.restoreAction)
                _dataSignals $"{{selectedNav: '{selectedNav}'}}"
                _class "bg-gray-200 dark:bg-gray-950"
                div {
                    _class "mx-auto max-w-7xl has-[[data-article-page]]:max-w-[88rem]"
                    TopNav.primary
                    page
                    Footer.primary
                }
                Analytics.banner privacyPolicy
            }
        }
