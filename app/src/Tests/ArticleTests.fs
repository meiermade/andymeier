module ArticleTests

open Domain
open Expecto

[<Tests>]
let domainTests =
    testList "Domain" [
        test "ArticleId round-trips" {
            let value = "abc123"
            let id = Article.ArticleId.ofString value
            let roundTripped = Article.ArticleId.toString id
            Expect.equal roundTripped value "ArticleId should round-trip"
        }

        test "ArticleId converts to Notion BlockId" {
            let value = "abc123"
            let id = Article.ArticleId.ofString value
            let blockId = Article.ArticleId.toBlockId id
            Expect.equal (Notion.BlockId.toString blockId) value "BlockId conversion should preserve value"
        }
    ]
