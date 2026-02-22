module App.Article

open Markdig
open Serilog
open System
open System.IO
open System.Text.RegularExpressions

// ============================================================
// Types
// ============================================================

type ArticleProperties =
    { permalink:string
      title:string
      summary:string
      thumbnail:string
      cover:string
      tags:string[]
      createdAt:DateTimeOffset }

type Article =
    { properties:ArticleProperties
      contentHtml:string }

// ============================================================
// Directory Name Parsing
// ============================================================

module private DirName =
    let private pattern = Regex(@"^(\d{4}-\d{2}-\d{2})-(.+)$")

    let parse (dirName:string) =
        let m = pattern.Match(dirName)
        if m.Success then
            match DateTimeOffset.TryParse(m.Groups.[1].Value) with
            | true, dt -> Some (dt, m.Groups.[2].Value)
            | _ -> None
        else None

// ============================================================
// Markdown Parsing
// ============================================================

module private Markdown =

    let private pipeline =
        MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build()

    let toHtml (markdown:string) =
        Markdig.Markdown.ToHtml(markdown, pipeline)

    type Frontmatter =
        { title:string
          summary:string
          thumbnail:string
          cover:string
          tags:string[] }

    let parseFrontmatter (lines:string[]) : Frontmatter * int =
        let mutable i = 0
        if lines.Length = 0 || lines.[0].Trim() <> "---" then
            failwith "Expected frontmatter starting with ---"
        i <- 1
        let mutable title = ""
        let mutable summary = ""
        let mutable thumbnail = ""
        let mutable cover = ""
        let mutable tags = [||]

        while i < lines.Length && lines.[i].Trim() <> "---" do
            let line = lines.[i].Trim()
            let colonIdx = line.IndexOf(':')
            if colonIdx > 0 then
                let key = line.Substring(0, colonIdx).Trim()
                let value = line.Substring(colonIdx + 1).Trim()
                match key with
                | "title" -> title <- value
                | "summary" -> summary <- value
                | "thumbnail" -> thumbnail <- value
                | "cover" -> cover <- value
                | "tags" -> tags <- value.Split(',') |> Array.map _.Trim()
                | _ -> ()
            i <- i + 1

        if i < lines.Length then i <- i + 1 // skip closing ---
        { title = title; summary = summary; thumbnail = thumbnail; cover = cover; tags = tags }, i

// ============================================================
// Service
// ============================================================

type Service =
    { listArticles: unit -> ArticleProperties list
      tryGetArticle: string -> Article option }

module Service =

    let private assetUrl (dirName:string) (filename:string) =
        $"/articles/{dirName}/assets/{filename}"

    let private loadArticle (articlesDir:string) (dirName:string) : Article option =
        match DirName.parse dirName with
        | None ->
            Log.Warning("Skipping directory with invalid name format: {DirName}", dirName)
            None
        | Some (createdAt, _permalink) ->
            let dir = Path.Combine(articlesDir, dirName)
            let mdFile = Path.Combine(dir, "index.md")
            if not (File.Exists(mdFile)) then None
            else
                try
                    let content = File.ReadAllText(mdFile)
                    let lines = content.Split('\n')
                    let frontmatter, bodyStart = Markdown.parseFrontmatter lines
                    let body = lines.[bodyStart..] |> String.concat "\n"

                    let bodyWithPaths =
                        body.Replace("](./", $"](/articles/{dirName}/assets/")
                            .Replace("src=\"./", $"src=\"/articles/{dirName}/assets/")

                    let contentHtml = Markdown.toHtml bodyWithPaths

                    let properties =
                        { permalink = dirName
                          title = frontmatter.title
                          summary = frontmatter.summary
                          thumbnail =
                              if String.IsNullOrEmpty(frontmatter.thumbnail) then ""
                              else assetUrl dirName frontmatter.thumbnail
                          cover =
                              if String.IsNullOrEmpty(frontmatter.cover) then ""
                              else assetUrl dirName frontmatter.cover
                          tags = frontmatter.tags
                          createdAt = createdAt }

                    Some { properties = properties; contentHtml = contentHtml }
                with ex ->
                    Log.Error(ex, "Failed to load article {DirName}", dirName)
                    None

    let create (articlesDir:string) : Service =
        let mutable cachedArticles : Article list = []

        let loadAll () =
            if Directory.Exists(articlesDir) then
                let dirs = Directory.GetDirectories(articlesDir) |> Array.map Path.GetFileName
                cachedArticles <-
                    dirs
                    |> Array.choose (loadArticle articlesDir)
                    |> Array.sortByDescending _.properties.createdAt
                    |> Array.toList
                Log.Information("Loaded {Count} articles from {Dir}", cachedArticles.Length, articlesDir)
            else
                Log.Warning("Articles directory not found: {Dir}", articlesDir)
                cachedArticles <- []

        loadAll ()

        { listArticles = fun () -> cachedArticles |> List.map _.properties
          tryGetArticle = fun permalink ->
              cachedArticles |> List.tryFind (fun a -> a.properties.permalink = permalink) }
