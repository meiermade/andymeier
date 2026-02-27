module Tests.NotionPageTests

open System
open Domain.Notion
open Expecto

let private mkRichText text =
    { plainText = text
      href = None
      annotations =
        { bold = false
          italic = false
          strikethrough = false
          underline = false
          code = false
          color = "default" } }

let private mkPage properties =
    { id = "page-1"
      icon = None
      cover = None
      properties = properties |> Map.ofList }

let getterTests =
    testList "Page helpers" [
        test "getTitle returns first rich text plain text" {
            let page = mkPage [ "Title", PropertyValue.Title [ mkRichText "Hello" ] ]
            Expect.equal (Page.getTitle "Title" page) "Hello" "title"
        }

        test "getTitle returns empty for missing key" {
            let page = mkPage []
            Expect.equal (Page.getTitle "Title" page) "" "empty"
        }

        test "getText returns first rich text plain text" {
            let page = mkPage [ "Summary", PropertyValue.RichText [ mkRichText "A summary" ] ]
            Expect.equal (Page.getText "Summary" page) "A summary" "text"
        }

        test "getCheckbox returns value" {
            let page = mkPage [ "Active", PropertyValue.Checkbox true ]
            Expect.isTrue (Page.getCheckbox "Active" page) "true"
        }

        test "getCheckbox returns false for missing" {
            let page = mkPage []
            Expect.isFalse (Page.getCheckbox "Active" page) "false"
        }

        test "getDate returns date" {
            let dt = DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero)
            let page = mkPage [ "Created", PropertyValue.Date(Some dt) ]
            Expect.equal (Page.getDate "Created" page) dt "date"
        }

        test "getDate returns MinValue for None" {
            let page = mkPage [ "Created", PropertyValue.Date None ]
            Expect.equal (Page.getDate "Created" page) DateTimeOffset.MinValue "min"
        }

        test "getStatus returns name" {
            let page = mkPage [ "Status", PropertyValue.Status "Published" ]
            Expect.equal (Page.getStatus "Status" page) "Published" "status"
        }

        test "getSelect returns name" {
            let page = mkPage [ "Category", PropertyValue.Select "Tech" ]
            Expect.equal (Page.getSelect "Category" page) "Tech" "select"
        }

        test "getMultiSelect returns names" {
            let page = mkPage [ "Tags", PropertyValue.MultiSelect [ "A"; "B" ] ]
            Expect.equal (Page.getMultiSelect "Tags" page) [| "A"; "B" |] "multi_select"
        }

        test "getMultiSelect returns empty for missing" {
            let page = mkPage []
            Expect.equal (Page.getMultiSelect "Tags" page) Array.empty "empty"
        }

        test "getIconUrl returns url" {
            let page =
                { id = "p1"
                  icon = Some { url = "https://example.com/icon.png" }
                  cover = None
                  properties = Map.empty }
            Expect.equal (Page.getIconUrl page) "https://example.com/icon.png" "icon"
        }

        test "getIconUrl returns empty for None" {
            let page = mkPage []
            Expect.equal (Page.getIconUrl page) "" "no icon"
        }

        test "getCoverUrl returns url" {
            let page =
                { id = "p1"
                  icon = None
                  cover = Some { url = "https://example.com/cover.jpg" }
                  properties = Map.empty }
            Expect.equal (Page.getCoverUrl page) "https://example.com/cover.jpg" "cover"
        }
    ]

[<Tests>]
let tests = testList "Notion.Page" [ getterTests ]
