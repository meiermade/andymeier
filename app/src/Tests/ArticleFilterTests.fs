module ArticleFilterTests

open App.Articles
open App.Articles.Handler
open App.Articles.View
open Expecto
open FSharp.ViewEngine
open System
open type Html

let private article (title:string) (summary:string) (tags:string[]) (createdAt:DateTimeOffset) : Article =
    { permalink = title.ToLowerInvariant().Replace(" ", "-")
      title = title
      summary = summary
      cover = "https://assets.meiermade.com/andymeier/articles/shared/cover.webp"
      tags = tags
      createdAt = createdAt
      updatedAt = None
      page = empty }

let private articles =
    [ article "Finance systems" "Notes on dependable operations." [| "Finance"; "Architecture" |] (DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero))
      article "Engineering notes" "Patterns for resilient software." [| "Engineering" |] (DateTimeOffset(2025, 6, 10, 0, 0, 0, TimeSpan.Zero))
      article "Capital planning" "Finance and engineering trade-offs." [| "Capital markets" |] (DateTimeOffset(2025, 1, 15, 0, 0, 0, TimeSpan.Zero)) ]

[<Tests>]
let articleFilterTests =
    testList "Article filters" [
        test "searches title, summary, and tags case-insensitively" {
            let titleMatches = filterArticles { FilterState.empty with search = Some "systems" } articles
            let summaryMatches = filterArticles { FilterState.empty with search = Some "resilient" } articles
            let tagMatches = filterArticles { FilterState.empty with search = Some "architecture" } articles

            Expect.equal (titleMatches |> List.map _.title) [ "Finance systems" ] "Expected title search match"
            Expect.equal (summaryMatches |> List.map _.title) [ "Engineering notes" ] "Expected summary search match"
            Expect.equal (tagMatches |> List.map _.title) [ "Finance systems" ] "Expected tag search match"
        }

        test "combines tag and published-year filters" {
            let filtered = filterArticles { FilterState.empty with search = Some "finance"; tag = Some "capital markets"; publishedYear = Some 2025 } articles

            Expect.equal (filtered |> List.map _.title) [ "Capital planning" ] "Expected filters to combine with AND semantics"
        }

        test "sorts separately by publication and effective update date" {
            let recent = articles[0]
            let revised = { articles[1] with updatedAt = Some(recent.createdAt.AddDays 1.) }
            let tied = { articles[2] with updatedAt = Some recent.createdAt }
            let mixed = [ tied; revised; recent ]

            Expect.equal (Article.sort NewestPublished mixed) [ recent; revised; tied ] "Updates must not change publication order"
            Expect.equal (Article.sort RecentlyUpdated mixed) [ revised; recent; tied ] "Unchanged articles use publication date, which also breaks update ties"
            Expect.equal (filterArticles { FilterState.empty with sort = RecentlyUpdated; publishedYear = Some 2025 } mixed) [ tied; revised ] "Year must still refer to publication"
        }
    ]
