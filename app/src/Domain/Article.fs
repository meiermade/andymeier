module Domain.Article

open FSharp.UMX
open Serilog
open System
open System.Threading.Tasks

// ============================================================
// Types
// ============================================================

type [<Measure>] articleId
type ArticleId = string<articleId>

module ArticleId =
    let toString (a:ArticleId) = UMX.untag a
    let ofString (a:string) : ArticleId = UMX.tag a
    let toBlockId (a:ArticleId) : Notion.BlockId = UMX.cast a

type ArticleProperties =
    { id:string
      permalink:string
      title:string
      summary:string
      icon:string
      iconDescription:string
      cover:string
      coverDescription:string
      tags:string[]
      createdAt:DateTimeOffset
      updatedAt:DateTimeOffset }

type Article =
    { properties:ArticleProperties
      blocks:Notion.Block list }

// ============================================================
// Notion -> Article mapping
// ============================================================

module NotionPage =
    let toProperties (page: Notion.Page) : ArticleProperties =
        { id = page.id
          permalink = page |> Notion.Page.getText "Permalink"
          title = page |> Notion.Page.getTitle "Title"
          summary = page |> Notion.Page.getText "Summary"
          icon = page |> Notion.Page.getIconUrl
          iconDescription = page |> Notion.Page.getText "Icon Description"
          cover = page |> Notion.Page.getCoverUrl
          coverDescription = page |> Notion.Page.getText "Cover Description"
          tags = page |> Notion.Page.getMultiSelect "Tags"
          createdAt = page |> Notion.Page.getDate "Created At"
          updatedAt = page |> Notion.Page.getDate "Updated At" }

// ============================================================
// Redis keys & serialization
// ============================================================

module private Store =
    [<Literal>]
    let ArticleListKey = "articles:list"

    let articleKey (permalink: string) = $"article:{permalink}"

// ============================================================
// Service
// ============================================================

type ListArticles = unit -> Task<ArticleProperties list>
type TryGetArticle = string -> Task<Article option>
type SyncDatabase = unit -> Task<unit>

type Service =
    { listArticles: ListArticles
      tryGetArticle: TryGetArticle
      syncDatabase: SyncDatabase }

module Service =

    // --- Notion helpers (used only by syncDatabase) ---

    let private listBlocks (notion: Notion.Service) (blockId: Notion.BlockId) : Task<Notion.Block list> =
        let rec recurse (blockId: Notion.BlockId) : Task<Notion.Block list> =
            task {
                let blocks = ResizeArray<Notion.Block>()
                let mutable hasMore = true
                let mutable cursor: Notion.Cursor option = None

                while hasMore do
                    let! res = notion.retrieveBlockChildren blockId cursor

                    for block in res.results do
                        if block.hasChildren then
                            let! children = recurse (Notion.BlockId.ofString block.id)

                            let updated =
                                match block.blockType with
                                | Notion.BlockType.BulletedListItem(rt, _) ->
                                    { block with blockType = Notion.BlockType.BulletedListItem(rt, children) }
                                | Notion.BlockType.NumberedListItem(rt, _) ->
                                    { block with blockType = Notion.BlockType.NumberedListItem(rt, children) }
                                | _ -> block

                            blocks.Add(updated)
                        else
                            blocks.Add(block)

                    hasMore <- res.hasMore
                    cursor <- res.nextCursor

                return Seq.toList blocks
            }

        blockId |> recurse

    let private fetchPublishedArticles
        (telemetry: Telemetry.Service)
        (notion: Notion.Service)
        (articlesDatabaseId: Notion.DatabaseId)
        =
        task {
            use _span = telemetry.startActiveSpan "domain.article.fetch_published_articles"
            let mutable hasMore = true
            let mutable cursor: Notion.Cursor option = None
            let pages = ResizeArray()
            let filter = Notion.PropertyFilter.StatusEquals("Status", "Published")

            while hasMore do
                let! res =
                    notion.queryDatabase articlesDatabaseId { filter = Some filter; startCursor = cursor }

                hasMore <- res.hasMore
                cursor <- res.nextCursor

                for page in res.results do
                    pages.Add(page)

            return pages |> Seq.toList
        }

    let private fetchArticle (notion: Notion.Service) (page: Notion.Page) =
        task {
            let properties = NotionPage.toProperties page
            let blockId = ArticleId.ofString page.id |> ArticleId.toBlockId
            let! blocks = listBlocks notion blockId
            return { properties = properties; blocks = blocks }
        }

    // --- Create ---

    let create
        (config: Notion.Config)
        (telemetry: Telemetry.Service)
        (redis: Redis.Service)
        (notion: Notion.Service)
        =
        let listArticles: ListArticles =
            fun () ->
                task {
                    match! redis.tryGetValue Store.ArticleListKey with
                    | Some json ->
                        return Json.deserialize<ArticleProperties list>(json)
                    | None -> return []
                }

        let tryGetArticle: TryGetArticle =
            fun permalink ->
                task {
                    match! redis.tryGetValue (Store.articleKey permalink) with
                    | Some json ->
                        return Some(Json.deserialize<Article>(json))
                    | None -> return None
                }

        let syncDatabase: SyncDatabase =
            fun () ->
                task {
                    Log.Information("Syncing articles from Notion")

                    try
                        let! pages = fetchPublishedArticles telemetry notion config.articlesDatabaseId

                        let summaries =
                            pages
                            |> List.map NotionPage.toProperties
                            |> List.sortByDescending _.createdAt

                        let listJson = Json.serialize summaries
                        do! redis.setValue (Store.ArticleListKey, listJson)
                        Log.Information("Stored {Count} article summaries", summaries.Length)

                        for page in pages do
                            let permalink = page |> Notion.Page.getText "Permalink"
                            try
                                let! article = fetchArticle notion page
                                let json = Json.serialize article
                                do! redis.setValue (Store.articleKey permalink, json)
                                Log.Debug("Stored article {Permalink}", permalink)
                            with ex ->
                                Log.Error(ex, "Failed to store article {Permalink}", permalink)

                        Log.Information("Article sync complete")
                    with ex ->
                        Log.Error(ex, "Article sync failed")
                }

        { listArticles = listArticles
          tryGetArticle = tryGetArticle
          syncDatabase = syncDatabase }

// ============================================================
// Background service
// ============================================================

type SyncBackgroundService(articleService: Service) =
    interface Microsoft.Extensions.Hosting.IHostedService with
        member _.StartAsync(ct) =
            let work () =
                task {
                    while not ct.IsCancellationRequested do
                        do! articleService.syncDatabase ()

                        try
                            do! Task.Delay(TimeSpan.FromMinutes(30.), ct)
                        with :? OperationCanceledException ->
                            ()
                }

            Task.Run(Func<Task>(fun () -> work () :> Task)) |> ignore
            Task.CompletedTask

        member _.StopAsync(_ct) =
            Task.CompletedTask
