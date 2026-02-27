module Domain.Article

open FSharp.UMX
open Microsoft.Data.Sqlite
open Sqlite
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

type Article =
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
      updatedAt:DateTimeOffset
      blocks:Notion.Block list
      syncedAt:DateTimeOffset }

// ============================================================
// Notion -> Article mapping
// ============================================================

module NotionPage =
    let toArticle (syncedAt: DateTimeOffset) (page: Notion.Page) : Article =
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
          updatedAt = page |> Notion.Page.getDate "Updated At"
          blocks = []
          syncedAt = syncedAt }

// ============================================================
// Service
// ============================================================

type ListArticles = unit -> Task<Article list>
type TryGetArticle = string -> Task<Article option>
type SyncDatabase = unit -> Task<unit>

type Service =
    { listArticles: ListArticles
      tryGetArticle: TryGetArticle
      syncDatabase: SyncDatabase }

module Service =

    let private ensureSchema (conn: SqliteConnection) =
        task {
            // language=sqlite
            let sql =
                """
                CREATE TABLE IF NOT EXISTS articles (
                    id TEXT PRIMARY KEY,
                    permalink TEXT NOT NULL UNIQUE,
                    title TEXT NOT NULL,
                    summary TEXT NOT NULL,
                    icon TEXT,
                    icon_description TEXT,
                    cover TEXT,
                    cover_description TEXT,
                    tags_json TEXT NOT NULL,
                    created_at_utc TEXT NOT NULL,
                    updated_at_utc TEXT NOT NULL,
                    blocks_json TEXT NOT NULL,
                    synced_at_utc TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS idx_articles_created_at ON articles(created_at_utc DESC);
                """

            do!
                Sql.connect conn
                |> Sql.sql sql
                |> Sql.executeNonQuery
        }

    let private upsertArticle (conn: SqliteConnection) (article: Article) (syncedAt: DateTimeOffset) =
        task {
            // language=sqlite
            let sql =
                """
                INSERT INTO articles (
                    id, permalink, title, summary, icon, icon_description, cover, cover_description,
                    tags_json, created_at_utc, updated_at_utc, blocks_json, synced_at_utc
                )
                VALUES (
                    $id, $permalink, $title, $summary, $icon, $icon_description, $cover, $cover_description,
                    $tags_json, $created_at_utc, $updated_at_utc, $blocks_json, $synced_at_utc
                )
                ON CONFLICT(id) DO UPDATE SET
                    permalink = excluded.permalink,
                    title = excluded.title,
                    summary = excluded.summary,
                    icon = excluded.icon,
                    icon_description = excluded.icon_description,
                    cover = excluded.cover,
                    cover_description = excluded.cover_description,
                    tags_json = excluded.tags_json,
                    created_at_utc = excluded.created_at_utc,
                    updated_at_utc = excluded.updated_at_utc,
                    blocks_json = excluded.blocks_json,
                    synced_at_utc = excluded.synced_at_utc;
                """

            do!
                Sql.connect conn
                |> Sql.sql sql
                |> Sql.parameters [
                    "$id", Sql.text article.id
                    "$permalink", Sql.text article.permalink
                    "$title", Sql.text article.title
                    "$summary", Sql.text article.summary
                    "$icon", Sql.text article.icon
                    "$icon_description", Sql.text article.iconDescription
                    "$cover", Sql.text article.cover
                    "$cover_description", Sql.text article.coverDescription
                    "$tags_json", Sql.text (Json.serialize article.tags)
                    "$created_at_utc", Sql.timestamptz article.createdAt
                    "$updated_at_utc", Sql.timestamptz article.updatedAt
                    "$blocks_json", Sql.text (Json.serialize article.blocks)
                    "$synced_at_utc", Sql.timestamptz syncedAt
                ]
                |> Sql.executeNonQuery
        }

    let private deleteStaleArticles (conn: SqliteConnection) (ids: string list) =
        task {
            match ids with
            | [] ->
                do!
                    Sql.connect conn
                    |> Sql.sql "DELETE FROM articles"
                    |> Sql.executeNonQuery
            | _ ->
                let placeholders = ids |> List.mapi (fun i _ -> $"$id{i}")
                let joinedPlaceholders = String.Join(",", placeholders)

                let parameters =
                    ids
                    |> List.mapi (fun i id -> ($"$id{i}", Sql.text id))

                do!
                    Sql.connect conn
                    |> Sql.sql $"DELETE FROM articles WHERE id NOT IN ({joinedPlaceholders})"
                    |> Sql.parameters parameters
                    |> Sql.executeNonQuery
        }

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

    let private fetchArticle (notion: Notion.Service) (syncedAt: DateTimeOffset) (page: Notion.Page) =
        task {
            let article = NotionPage.toArticle syncedAt page
            let blockId = ArticleId.ofString page.id |> ArticleId.toBlockId
            let! blocks = listBlocks notion blockId
            return { article with blocks = blocks }
        }

    // --- Create ---

    let create
        (config: Notion.Config)
        (telemetry: Telemetry.Service)
        (sqliteConfig: Sqlite.Config)
        (notion: Notion.Service)
        =
        let connectionString = Sqlite.connectionString sqliteConfig

        let listArticles: ListArticles =
            fun () ->
                task {
                    use span = telemetry.startActiveSpan "domain.sqlite.list_articles"
                    use conn = new SqliteConnection(connectionString)
                    do! conn.OpenAsync() |> Task.Ignore

                    // language=sqlite
                    let sql =
                        """
                        SELECT
                            id, permalink, title, summary, icon, icon_description, cover, cover_description,
                            tags_json, created_at_utc, updated_at_utc, blocks_json, synced_at_utc
                        FROM articles
                        ORDER BY created_at_utc DESC
                        """

                    let! articles =
                        Sql.connect conn
                        |> Sql.sql sql
                        |> Sql.executeQuery (fun reader ->
                            { id = reader.string "id"
                              permalink = reader.string "permalink"
                              title = reader.string "title"
                              summary = reader.string "summary"
                              icon = reader.stringOption "icon" |> Option.defaultValue ""
                              iconDescription = reader.stringOption "icon_description" |> Option.defaultValue ""
                              cover = reader.stringOption "cover" |> Option.defaultValue ""
                              coverDescription = reader.stringOption "cover_description" |> Option.defaultValue ""
                              tags = reader.string "tags_json" |> Json.deserialize<string[]>
                              createdAt = reader.dateTimeOffset "created_at_utc"
                              updatedAt = reader.dateTimeOffset "updated_at_utc"
                              blocks = reader.string "blocks_json" |> Json.deserialize<Notion.Block list>
                              syncedAt = reader.dateTimeOffset "synced_at_utc" })

                    span.SetAttribute("count", articles.Length) |> ignore
                    return articles
                }

        let tryGetArticle: TryGetArticle =
            fun permalink ->
                task {
                    use span = telemetry.startActiveSpan "domain.sqlite.try_get_article"
                    span.SetAttribute("permalink", permalink) |> ignore

                    use conn = new SqliteConnection(connectionString)
                    do! conn.OpenAsync() |> Task.Ignore

                    // language=sqlite
                    let sql =
                        """
                        SELECT
                            id, permalink, title, summary, icon, icon_description, cover, cover_description,
                            tags_json, created_at_utc, updated_at_utc, blocks_json, synced_at_utc
                        FROM articles
                        WHERE permalink = $permalink
                        LIMIT 1
                        """

                    let! rows =
                        Sql.connect conn
                        |> Sql.sql sql
                        |> Sql.parameter ("$permalink", Sql.text permalink)
                        |> Sql.executeQuery (fun reader ->
                            { id = reader.string "id"
                              permalink = reader.string "permalink"
                              title = reader.string "title"
                              summary = reader.string "summary"
                              icon = reader.stringOption "icon" |> Option.defaultValue ""
                              iconDescription = reader.stringOption "icon_description" |> Option.defaultValue ""
                              cover = reader.stringOption "cover" |> Option.defaultValue ""
                              coverDescription = reader.stringOption "cover_description" |> Option.defaultValue ""
                              tags = reader.string "tags_json" |> Json.deserialize<string[]>
                              createdAt = reader.dateTimeOffset "created_at_utc"
                              updatedAt = reader.dateTimeOffset "updated_at_utc"
                              blocks = reader.string "blocks_json" |> Json.deserialize<Notion.Block list>
                              syncedAt = reader.dateTimeOffset "synced_at_utc" })

                    return rows |> List.tryHead
                }

        let syncDatabase: SyncDatabase =
            fun () ->
                task {
                    Log.Information("Syncing articles from Notion")

                    try
                        let! pages = fetchPublishedArticles telemetry notion config.articlesDatabaseId
                        let syncedAt = DateTimeOffset.UtcNow

                        let articles = ResizeArray<Article>()

                        for page in pages do
                            let permalink = page |> Notion.Page.getText "Permalink"

                            try
                                let! article = fetchArticle notion syncedAt page
                                articles.Add(article)
                                Log.Debug("Fetched article {Permalink}", permalink)
                            with ex ->
                                Log.Error(ex, "Failed to fetch article {Permalink}", permalink)

                        use conn = new SqliteConnection(connectionString)
                        do! conn.OpenAsync() |> Task.Ignore
                        do! ensureSchema conn

                        for article in articles do
                            do! upsertArticle conn article syncedAt

                        let ids = articles |> Seq.map _.id |> List.ofSeq
                        do! deleteStaleArticles conn ids

                        Log.Information("Stored {Count} articles", articles.Count)
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
