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
    }

type Article = { ...ArticleMetadata; page: HtmlElement }

module Article =
    let create (metadata: ArticleMetadata) (page: HtmlElement) : Article = { ...metadata; page = page }
