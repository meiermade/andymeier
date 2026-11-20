module App.Articles.View

open App.Articles
open App.Articles.Shared
open App.Components
open FSharp.ViewEngine
open App.Common.View
open System
open type Datastar
open type Html

type FilterState =
    { search: string option
      tag: string option
      publishedYear: int option
      sort: ArticleSort }

module FilterState =
    let empty: FilterState = { search = None; tag = None; publishedYear = None; sort = NewestPublished }

    let sortValue = function
        | NewestPublished -> None
        | RecentlyUpdated -> Some "updated"

    let url (filters:FilterState) =
        [ "search", filters.search
          "tag", filters.tag
          "year", filters.publishedYear |> Option.map string
          "sort", sortValue filters.sort ]
        |> List.choose (fun (key, value) ->
            value
            |> Option.map _.Trim()
            |> Option.filter (String.IsNullOrWhiteSpace >> not)
            |> Option.map (fun value -> $"{Uri.EscapeDataString key}={Uri.EscapeDataString value}"))
        |> String.concat "&"
        |> function
            | "" -> "/articles"
            | query -> $"/articles?{query}"

type ArticlesPageState =
    { articles: Article list
      filters: FilterState
      tags: string list
      years: int list }

type FilterOption =
    { label: string
      url: string
      selected: bool }

module FilterControl =
    let menu (name:string) (label:string) (selectedLabel:string) (options:FilterOption list) =
        div {
            _data ("filter-control", name)
            fveDropdownMenu {
                _id $"filter-{name}"
                _class "[&>[role=menu]]:max-h-[min(16rem,calc(100dvh-2rem))] [&>[role=menu]]:overflow-y-auto"
                _alignment DropdownMenuAlignment.Start
                fveDropdownMenuTrigger {
                    fveButton {
                        _ariaLabel label
                        _disabled options.IsEmpty
                        span { _class "min-w-0 truncate"; selectedLabel }
                        fveIcon { _name IconName.ChevronDown; _size Size.Small }
                    }
                }
                if options.IsEmpty then
                    fveDropdownMenuItem { _disabled true; "No options available" }
                for option in options do
                    fveDropdownMenuItem {
                        _href option.url
                        option.label
                        if option.selected then
                            fveDropdownMenuItemTrailing { fveIcon { _name IconName.Check; _size Size.Small } }
                    }
            }
        }

    let active (label:string) (control:HtmlElement) (removeUrl:string) (removeLabel:string) =
        div {
            _class "flex min-w-0 items-center gap-1"
            span { _class "text-sm text-[var(--fve-muted-text)]"; text label }
            control
            fveLink {
                _href removeUrl
                _variant LinkVariant.Ghost
                _color Color.Neutral
                _ariaLabel removeLabel
                fveIcon { _name IconName.XMark; _size Size.Small }
            }
        }

let private articleIndex (state:ArticlesPageState) =
    let filters = state.filters
    let hasSearch = filters.search.IsSome
    let hasActiveFilters = filters.tag.IsSome || filters.publishedYear.IsSome
    let hasCriteria = hasSearch || hasActiveFilters
    let clearSearchUrl = FilterState.url { filters with search = None }
    let clearFiltersUrl = FilterState.url { filters with tag = None; publishedYear = None }
    let tagOptions =
        state.tags
        |> List.map (fun tag ->
            { label = tag
              url = FilterState.url { filters with tag = Some tag }
              selected = filters.tag = Some tag })
    let yearOptions =
        state.years
        |> List.map (fun year ->
            { label = string year
              url = FilterState.url { filters with publishedYear = Some year }
              selected = filters.publishedYear = Some year })

    let searchRequest =
        "@get(el.action + '?' + new URLSearchParams(new FormData(el)), {headers: {'X-MeierMade-Article-Search': 'live'}, filterSignals: {include: /^$/}, requestCancellation: 'cleanup', retry: 'never'})"

    let clearSearch = span {
        _id "article-clear-search"
        if not hasSearch then _class "hidden"
        fveLink {
            _href clearSearchUrl
            _ariaLabel "Clear search"
            _variant LinkVariant.Ghost
            _color Color.Neutral
            fveIcon { _name IconName.XMark; _size Size.Small }
        }
    }

    let addFilter = div {
        _id "article-add-filter"
        if filters.tag.IsSome && filters.publishedYear.IsSome then _class "hidden"
        if filters.tag.IsNone || filters.publishedYear.IsNone then
            fvePopover {
                _id "add-filter"
                _alignment PopoverAlignment.End
                fvePopoverTrigger {
                    fveButton {
                        fveIcon { _name IconName.Plus; _size Size.Small }
                        "Add filter"
                    }
                }
                fvePopoverContent {
                    _class "grid gap-3"
                    if filters.tag.IsNone then
                        div {
                            _class "grid gap-1"
                            span { _class "text-sm font-medium"; "Tag" }
                            FilterControl.menu "tag" "Tag filter" "Select tag" tagOptions
                        }
                    if filters.publishedYear.IsNone then
                        div {
                            _class "grid gap-1"
                            span { _class "text-sm font-medium"; "Published" }
                            FilterControl.menu "year" "Published year filter" "Select year" yearOptions
                        }
                }
            }
    }

    let sortControl = fveButtonGroup {
        _ariaLabel "Article sorting"
        "Sort by"
        Html.select {
            _id "article-sort"
            _name "sort"
            _ariaLabel "Sort by"
            _data ("fve-group-control", "outline")
            _class "min-h-[var(--fve-control-min-height)] cursor-pointer rounded-[var(--fve-radius-control)] border border-[var(--fve-border)] bg-[var(--fve-surface)] py-0 pl-3 pr-8 text-[length:var(--fve-control-font-size)] text-[var(--fve-text)] focus-visible:outline-2 focus-visible:-outline-offset-2 focus-visible:outline-[var(--fve-brand-ring)]"
            for sort, label in [ NewestPublished, "Published"; RecentlyUpdated, "Updated" ] do
                option {
                    _value (FilterState.sortValue sort |> Option.defaultValue "")
                    _selected (filters.sort = sort)
                    text label
                }
        }
    }

    let activeFilters = div {
        _id "article-active-filters"
        if hasActiveFilters then
            _class "mt-3 flex flex-wrap items-center gap-2"
            match filters.tag with
            | Some tag ->
                FilterControl.active
                    "Tag"
                    (FilterControl.menu "tag" "Tag filter" tag tagOptions)
                    (FilterState.url { filters with tag = None })
                    "Remove tag filter"
            | None -> ()
            match filters.publishedYear with
            | Some year ->
                FilterControl.active
                    "Published"
                    (FilterControl.menu "year" "Published year filter" (string year) yearOptions)
                    (FilterState.url { filters with publishedYear = None })
                    "Remove published year filter"
            | None -> ()
            fveLink { _href clearFiltersUrl; "Clear filters" }
    }

    let results = div {
        _id "article-results"
        if state.articles.IsEmpty then
            _class "py-8"
            fveEmptyState {
                _label "No articles found"
                _description (if hasCriteria then "Try changing or clearing your search and filters." else "Articles will appear here when they are published.")
                fveEmptyStateIcon { fveIcon { _name IconName.MagnifyingGlass } }
            }
        else
            _class "flex flex-col"
            for article in state.articles do ArticleCard.summary article
    }

    let content =
        div {
            _class "mx-auto max-w-5xl px-4 py-10"
            header {
                h1 { _class "text-4xl font-medium text-gray-900 dark:text-gray-100"; "Articles" }
                p {
                    _class "mt-4 max-w-2xl text-lg leading-7 text-gray-600 dark:text-gray-400"
                    "Personal notes by Andy Meier. Unless otherwise noted, they reflect my own views—not those of clients, collaborators, or organizations I work with."
                }
            }
            section {
                _ariaLabel "Article search and filters"
                _class "mt-10"
                form {
                    _method "get"
                    _action "/articles"
                    _class "border-b border-gray-300/60 py-4 dark:border-gray-700/60"
                    _dataOn ("input__debounce.200ms", "evt.target.id === 'article-search' && " + searchRequest)
                    _dataOn ("change", "evt.target.id === 'article-sort' && " + searchRequest)
                    _dataOn ("submit__prevent", searchRequest)
                    match filters.tag with
                    | Some tag -> input { _type "hidden"; _name "tag"; _value tag }
                    | None -> ()
                    match filters.publishedYear with
                    | Some year -> input { _type "hidden"; _name "year"; _value (string year) }
                    | None -> ()
                    div {
                        _class "flex flex-col gap-2 sm:flex-row sm:items-center"
                        div {
                            _class "min-w-0 flex-1"
                            fveInputGroup {
                                _ariaLabel "Article search"
                                fveInputGroupAddon {
                                    fveIcon { _name IconName.MagnifyingGlass; _size Size.Small }
                                }
                                fveInput {
                                    _id "article-search"
                                    _type InputType.Text
                                    _role "searchbox"
                                    _name "search"
                                    _ariaLabel "Search articles"
                                    _value (filters.search |> Option.defaultValue "")
                                    _placeholder "Search articles"
                                }
                                fveInputGroupAddon {
                                    _position InputGroupPosition.InlineEnd
                                    _class "has-[.hidden]:hidden"
                                    clearSearch
                                }
                            }
                        }
                        div {
                            _class "flex flex-wrap items-center gap-2 sm:justify-end"
                            addFilter
                            sortControl
                            noscript { fveButton { _type "submit"; "Apply" } }
                        }
                    }
                    activeFilters
                }
                results
            }
        }

    content, fragment { clearSearch; addFilter; activeFilters; results }

let articlesPage state = articleIndex state |> fst |> Page.primary

let articleSearchResults state = articleIndex state |> snd

let notFoundPage =
    let content =
        div {
            _class "flex flex-col items-center"
            h1 { _class "text-3xl text-gray-800 dark:text-gray-100"; "Could not find page." }
            p { _class "mt-2 text-md text-gray-600 dark:text-gray-400"; "Something went wrong. Try refreshing the page." }
        }
    Page.primary content
