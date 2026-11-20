namespace App.Components

open System
open FSharp.ViewEngine
open type Html

/// <category>pagination</category>
[<RequireQualifiedAccess>]
type PaginationVariant = Full | NumbersOnly | PreviousNextOnly

/// <category>pagination</category>
[<AutoOpen>]
module PaginationAttributes =
    type Html with
        static member _variant(value:PaginationVariant) = Html._attr("variant", match value with PaginationVariant.Full -> "full" | PaginationVariant.NumbersOnly -> "numbers-only" | PaginationVariant.PreviousNextOnly -> "previous-next-only")
        static member _page(value:int) = Html._attr("page", string value)

type internal PaginationPart(kind:string, pages:(int * bool) list, element:HtmlElement) =
    inherit HtmlElement()
    member _.Kind = kind
    member _.Pages = pages
    override _.Render(writer) = element.Render writer

module internal Pagination =
    let private part kind pages element = PaginationPart(kind, pages, element) :> HtmlElement
    let private linkClasses = "inline-flex min-h-9 items-center justify-center rounded-[var(--fve-radius-control)] px-3 text-sm font-medium text-[var(--fve-text)] no-underline border border-[var(--fve-border)] [&:not([readonly])]:not-disabled:not-aria-disabled:not-aria-busy:hover:bg-[linear-gradient(var(--fve-hover-wash),var(--fve-hover-wash))] [&:not([readonly])]:not-disabled:not-aria-disabled:not-aria-busy:active:bg-[linear-gradient(var(--fve-pressed-wash),var(--fve-pressed-wash))] focus-visible:ring-2 focus-visible:ring-[var(--fve-brand-ring)]"
    let edge kind attributes children =
        let href = ComponentElement.value "href" attributes
        let children = if List.isEmpty children then [text (if kind = "previous" then "Previous" else "Next")] else children
        let element =
            match href with
            | Some href when not (String.IsNullOrWhiteSpace href) -> ComponentElement.render "a" [_class linkClasses] attributes children
            | Some _ -> invalidArg "attributes" "A pagination destination cannot be empty."
            | None -> ComponentElement.render "span" [_ariaDisabled true; _class (linkClasses + " cursor-default opacity-50")] attributes children
        part kind [] element

    let item attributes children =
        let page =
            match ComponentElement.required "page" attributes |> Int32.TryParse with
            | true, page when page > 0 -> page
            | _ -> invalidArg "attributes" "A pagination page number must be positive."
        let current = ComponentElement.value "aria-current" attributes = Some "page"
        let href = ComponentElement.value "href" attributes
        if not current && (href |> Option.forall String.IsNullOrWhiteSpace) then invalidArg "attributes" "A non-current page requires a destination."
        let attrs = ComponentHtml.safeAttributes ["page"; "href"; "aria-current"] attributes
        let children = if List.isEmpty children then [text (string page)] else children
        let element = li {
            if current then
                ComponentElement.render "span" [_ariaCurrent "page"; _ariaLabel $"Page {page}, current page"; _class "inline-flex size-9 items-center justify-center rounded-[var(--fve-radius-control)] bg-[var(--fve-brand-subtle)] text-sm font-semibold text-[var(--fve-brand-text)] border border-[var(--fve-brand-ring)]"] attrs children
            else ComponentElement.render "a" [_href href.Value; _ariaLabel $"Page {page}"; _class (linkClasses + " size-9 px-0")] attrs children
        }
        part "item" [page, current] element

    let ellipsis attributes children =
        let children = if List.isEmpty children then [text "…"] else children
        part "item" [] (li { ComponentElement.render "span" [_ariaHidden true; _class "inline-flex size-9 items-center justify-center text-[var(--fve-muted-text)]"] attributes children })

    let content attributes (children:HtmlElement list) =
        let pages = children |> List.collect (function :? PaginationPart as part when part.Kind = "item" -> part.Pages | :? NoopElement -> [] | _ -> invalidArg "children" "PaginationContent accepts page items and ellipses.")
        part "content" pages (ComponentElement.render "ol" [_class "flex flex-wrap items-center gap-1"] attributes children)

    let summary attributes children = part "summary" [] (ComponentElement.render "div" [_class "min-w-0 text-sm text-[var(--fve-muted-text)]"] attributes children)

    let render attributes (children:HtmlElement list) =
        let parts = children |> List.choose (function :? PaginationPart as part -> Some part | :? NoopElement -> None | _ -> invalidArg "children" "Pagination accepts its semantic child parts.")
        if parts |> List.exists (fun part -> not (List.contains part.Kind ["summary"; "content"; "previous"; "next"])) then invalidArg "children" "Page items must be inside fvePaginationContent."
        if parts |> List.map _.Kind |> List.distinct |> List.length <> parts.Length then invalidArg "children" "Each pagination region may occur only once."
        let pages = parts |> List.collect _.Pages
        if pages |> List.map fst |> List.distinct |> List.length <> pages.Length then invalidArg "children" "Pagination page numbers must be unique."
        let variant = ComponentElement.choice "variant" "full" ["full"; "numbers-only"; "previous-next-only"] attributes
        if variant <> "previous-next-only" && (pages |> List.filter snd |> List.length <> 1) then invalidArg "children" "Pagination requires exactly one current page."
        if ComponentElement.value "aria-label" attributes |> Option.orElseWith (fun () -> ComponentElement.value "aria-labelledby" attributes) |> Option.forall String.IsNullOrWhiteSpace then invalidArg "attributes" "Pagination requires an accessible label."
        let find kind = parts |> List.tryFind (fun part -> part.Kind = kind) |> Option.map (fun part -> part :> HtmlElement)
        ComponentElement.render "nav" [_class "flex flex-wrap items-center justify-between gap-3"] (ComponentHtml.safeAttributes ["variant"; "role"] attributes) [
            match find "summary" with Some summary -> yield summary | None -> ()
            yield div {
                _class "flex flex-wrap items-center gap-1"
                if variant <> "numbers-only" then find "previous" |> Option.defaultWith (fun () -> edge "previous" [] [])
                if variant <> "previous-next-only" then find "content" |> Option.defaultValue empty
                if variant <> "numbers-only" then find "next" |> Option.defaultWith (fun () -> edge "next" [] [])
            } ]

/// <category>pagination</category>
[<AutoOpen>]
module PaginationDsl =
    let fvePagination = ComponentElementBuilder(Pagination.render)
    let fvePaginationContent = ComponentElementBuilder(Pagination.content)
    let fvePaginationItem = ComponentElementBuilder(Pagination.item)
    let fvePaginationEllipsis = ComponentElementBuilder(Pagination.ellipsis)
    let fvePaginationPrevious = ComponentElementBuilder(Pagination.edge "previous")
    let fvePaginationNext = ComponentElementBuilder(Pagination.edge "next")
    let fvePaginationSummary = ComponentElementBuilder(Pagination.summary)
