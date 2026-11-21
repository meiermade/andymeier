namespace App.Articles

open FSharp.ViewEngine
open System

type ArticleMetadata =
    {
        permalink: string
        title: string
        summary: string
        cover: string
        tags: string[]
        createdAt: DateTimeOffset
        updatedAt: DateTimeOffset option
    }

type Article = { ...ArticleMetadata; page: HtmlElement }

type ArticleSort =
    | NewestPublished
    | RecentlyUpdated

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

module Article =
    let sort order (articles: Article list) =
        articles
        |> List.sortByDescending (fun article ->
            match order with
            | NewestPublished -> article.createdAt, article.createdAt
            | RecentlyUpdated -> defaultArg article.updatedAt article.createdAt, article.createdAt)

    let create (metadata: ArticleMetadata) (page: HtmlElement) : Article = { ...metadata; page = page }
