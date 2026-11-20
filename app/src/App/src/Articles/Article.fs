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

module Article =
    let sort order (articles: Article list) =
        articles
        |> List.sortByDescending (fun article ->
            match order with
            | NewestPublished -> article.createdAt, article.createdAt
            | RecentlyUpdated -> defaultArg article.updatedAt article.createdAt, article.createdAt)

    let create (metadata: ArticleMetadata) (page: HtmlElement) : Article = { ...metadata; page = page }
