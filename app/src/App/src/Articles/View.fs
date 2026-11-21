module App.Articles.View

open App.Articles
open App.Articles.Shared
open App.Components
open FSharp.ViewEngine
open App.Common.View
open type Datastar
open type Html

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
    let chip (name:string) (label:string) (selected:string option) (options:FilterOption list) removeUrl =
        fveButtonGroup {
            _ariaLabel $"{label} filter"
            label
            fveDropdownMenu {
                _id $"filter-{name}"
                _ariaLabel $"{label} filter values"
                _class "[&>[role=menu]]:max-h-[min(16rem,calc(100dvh-2rem))] [&>[role=menu]]:overflow-y-auto"
                _alignment DropdownMenuAlignment.Start
                fveDropdownMenuTrigger {
                    _variant ButtonVariant.Outline
                    _data ("fve-group-control", "outline")
                    fveButton {
                        if selected.IsSome then
                            _class "bg-[var(--fve-brand-subtle)] text-[var(--fve-brand-text)]"
                        span { _class "max-w-[min(9rem,32vw)] truncate"; text (defaultArg selected "Any") }
                        fveIcon { _name IconName.ChevronDown; _size Size.Small }
                    }
                }
                if options.IsEmpty then
                    fveDropdownMenuItem { _disabled true; "No options available" }
                for option in options do
                    fveDropdownMenuItem {
                        _href option.url
                        _data ("article-filter", name)
                        option.label
                        if option.selected then
                            fveDropdownMenuItemTrailing { fveIcon { _name IconName.Check; _size Size.Small } }
                    }
            }
            fveLink {
                _href removeUrl
                _data ("article-filter", name)
                _data ("remove-article-filter", "")
                _variant LinkVariant.Outline
                _ariaLabel $"Remove {label.ToLowerInvariant()} filter"
                fveIcon { _name IconName.XMark; _size Size.Small }
            }
        }

    let nativeSelect name label values selected =
        Html.label {
            _class "flex min-w-0 flex-col gap-1 text-sm"
            text label
            select {
                _class "min-w-0 rounded-lg border border-[var(--fve-border)] bg-[var(--fve-surface)] px-2 py-1"
                _name name
                option { _value ""; "Any" }
                for value in values do
                    option { _value value; _selected ((selected = Some value)); text value }
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
    let yearValues = state.years |> List.map string
    let selectedYear = filters.publishedYear |> Option.map string
    let yearOptions =
        state.years
        |> List.map (fun year ->
            { label = string year
              url = FilterState.url { filters with publishedYear = Some year }
              selected = filters.publishedYear = Some year })

    let searchRequest =
        "@get(el.action + '?' + new URLSearchParams([...new FormData(el), ['tag', el.querySelector('#article-tag').value], ['year', el.querySelector('#article-year').value]]), {headers: {'X-MeierMade-Article-Search': 'live'}, filterSignals: {include: /^$/}, requestCancellation: 'auto', retry: 'never'})"

    let filterRequest =
        ("""const link = evt.target.closest('a[data-article-filter]');
if (link && evt.button === 0 && !evt.metaKey && !evt.ctrlKey && !evt.shiftKey && !evt.altKey) {
    evt.preventDefault();
    const query = new URL(link.href).searchParams;
    const field = link.dataset.articleFilter;
    if (link.hasAttribute('data-remove-article-filter')) {
        if (field === 'tag') $article_tag_visible = false;
        if (field === 'year') $article_year_visible = false;
        requestAnimationFrame(() => el.querySelector('#add-filter-trigger').focus());
    }
    const input = field === 'search' ? el.elements.namedItem(field) : el.querySelector('#article-' + field);
    input.value = query.get(field) ?? '';
    if (field === 'search') input.focus();
""" + searchRequest + ";\n}")

    let clearSearch = span {
        _id "article-clear-search"
        if not hasSearch then _class "hidden"
        fveLink {
            _href clearSearchUrl
            _data ("article-filter", "search")
            _ariaLabel "Clear search"
            _variant LinkVariant.Ghost
            _color Color.Neutral
            fveIcon { _name IconName.XMark; _size Size.Small }
        }
    }

    let addFilter = fveDropdownMenu {
        _id "add-filter"
        _class "noscript:hidden"
        fveDropdownMenuTrigger {
            _variant ButtonVariant.Outline
            _dataAttr ("disabled", "$article_tag_visible && $article_year_visible ? '' : null")
            fveButton {
                fveIcon { _name IconName.Plus; _size Size.Small }
                "Add filter"
            }
        }
        fveDropdownMenuItem {
            _class "group-data-[tag-visible]/article-filters:hidden"
            _dataOn ("click", "$article_tag_visible = true; requestAnimationFrame(() => document.getElementById('filter-tag-trigger').click())")
            "Tag"
        }
        fveDropdownMenuItem {
            _class "group-data-[year-visible]/article-filters:hidden"
            _dataOn ("click", "$article_year_visible = true; requestAnimationFrame(() => document.getElementById('filter-year-trigger').click())")
            "Publication year"
        }
    }

    let sortControl = fveButtonGroup {
        _ariaLabel "Article sorting"
        "Sort by"
        label { _for "article-sort"; _class "sr-only"; "Sort by" }
        fveSelect {
            _id "article-sort"
            _name "sort"
            _ariaLabel "Sort by"
            _value (FilterState.sortValue filters.sort |> Option.defaultValue "published")
            _nativeFallback true
            _position SelectPosition.TriggerEnd
            fveSelectContent {
                fveSelectItem { _value "published"; "Published" }
                fveSelectItem { _value "updated"; "Updated" }
            }
        }
    }

    let activeFilters = div {
        _id "article-active-filters"
        _class "hidden group-data-[tag-visible]/article-filters:flex group-data-[year-visible]/article-filters:flex noscript:hidden! mt-3 flex-wrap items-center gap-2"
        div {
            _class "hidden min-w-0 max-w-full group-data-[tag-visible]/article-filters:block"
            FilterControl.chip "tag" "Tag" filters.tag tagOptions (FilterState.url { filters with tag = None })
        }
        div {
            _class "hidden min-w-0 max-w-full group-data-[year-visible]/article-filters:block"
            FilterControl.chip "year" "Publication year" selectedYear yearOptions (FilterState.url { filters with publishedYear = None })
        }
        fveButton {
            _type "button"
            _variant ButtonVariant.Ghost
            _color Color.Neutral
            _dataOn (
                "click",
                """const form = el.closest('form');
form.querySelector('#article-tag').value = '';
form.querySelector('#article-year').value = '';
$article_tag_visible = false;
$article_year_visible = false;
requestAnimationFrame(() => form.querySelector('#add-filter-trigger').focus());
form.requestSubmit();"""
            )
            "Clear filters"
        }
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
                    _class "group/article-filters border-b border-gray-300/60 py-4 dark:border-gray-700/60"
                    _dataSignals ("article_tag_visible", ["ifmissing"], if filters.tag.IsSome then "true" else "false")
                    _dataSignals ("article_year_visible", ["ifmissing"], if selectedYear.IsSome then "true" else "false")
                    _dataAttr ("data-tag-visible", "$article_tag_visible ? '' : null")
                    _dataAttr ("data-year-visible", "$article_year_visible ? '' : null")
                    _dataOn ("input__debounce.200ms", "evt.target.id === 'article-search' && " + searchRequest)
                    _dataOn ("change", "evt.target.name === 'sort' && " + searchRequest)
                    _dataOn ("submit__prevent", searchRequest)
                    _dataOn ("click", filterRequest)
                    input { _type "hidden"; _id "article-tag"; _value (defaultArg filters.tag "") }
                    input { _type "hidden"; _id "article-year"; _value (defaultArg selectedYear "") }
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
                        }
                    }
                    activeFilters
                    noscript {
                        div {
                            _class "mt-3 flex flex-wrap items-end gap-2"
                            FilterControl.nativeSelect "tag" "Tag" state.tags filters.tag
                            FilterControl.nativeSelect "year" "Publication year" yearValues selectedYear
                            button { _type "submit"; _class "rounded-lg border border-[var(--fve-border)] px-3 py-1"; "Apply" }
                            if hasActiveFilters then fveLink { _href clearFiltersUrl; "Clear filters" }
                        }
                    }
                }
                results
            }
        }

    content, fragment { clearSearch; activeFilters; results }

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
