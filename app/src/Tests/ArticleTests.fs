module ArticleTests

open Expecto
open System
open System.IO

[<Tests>]
let articleTests =
    testList "Article" [
        test "Service loads articles from directory with date prefix" {
            let tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            let articleDir = Path.Combine(tempDir, "2025-01-15-test-article")
            Directory.CreateDirectory(articleDir) |> ignore

            let markdown = """---
title: Test Article
summary: A test summary
cover: cover.jpg
thumbnail: thumb.jpg
tags: fsharp, dotnet
---

# Hello World

This is a test article.
"""
            File.WriteAllText(Path.Combine(articleDir, "index.md"), markdown)

            let service = App.Article.Service.create tempDir
            let articles = service.listArticles ()

            Expect.equal articles.Length 1 "Should have one article"
            Expect.equal articles.[0].title "Test Article" "Title should match"
            Expect.equal articles.[0].summary "A test summary" "Summary should match"
            Expect.equal articles.[0].permalink "2025-01-15-test-article" "Permalink should be full dir name"
            Expect.equal articles.[0].tags [| "fsharp"; "dotnet" |] "Tags should match"
            Expect.equal (articles.[0].createdAt.Date) (DateTime(2025, 1, 15)) "CreatedAt date should be parsed from dir name"
            Expect.stringContains articles.[0].thumbnail "thumb.jpg" "Thumbnail should be set"
            Expect.stringContains articles.[0].cover "cover.jpg" "Cover should be set"

            let article = service.tryGetArticle "2025-01-15-test-article"
            Expect.isSome article "Should find article by permalink"
            Expect.stringContains article.Value.contentHtml "Hello World" "Content should contain heading"

            Directory.Delete(tempDir, true)
        }

        test "Service handles missing directory gracefully" {
            let service = App.Article.Service.create "/nonexistent/path"
            let articles = service.listArticles ()
            Expect.equal articles.Length 0 "Should have no articles"
        }

        test "Service skips directories without date prefix" {
            let tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            let articleDir = Path.Combine(tempDir, "no-date-prefix")
            Directory.CreateDirectory(articleDir) |> ignore
            File.WriteAllText(Path.Combine(articleDir, "index.md"), "---\ntitle: Bad\n---\nContent")

            let service = App.Article.Service.create tempDir
            let articles = service.listArticles ()
            Expect.equal articles.Length 0 "Should skip dir without date prefix"

            Directory.Delete(tempDir, true)
        }
    ]
