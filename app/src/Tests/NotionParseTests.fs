module Tests.NotionParseTests

open System
open System.Text.Json
open Domain.Notion
open Expecto

let private defaultAnnotations =
    """{"bold":false,"italic":false,"strikethrough":false,"underline":false,"code":false,"color":"default"}"""

let private richTextJson (text: string) =
    $"""{{
        "type":"text",
        "text":{{"content":"{text}"}},
        "annotations":{defaultAnnotations},
        "plain_text":"{text}",
        "href":null
    }}"""

let private richTextJsonWithHref (text: string) (href: string) =
    $"""{{
        "type":"text",
        "text":{{"content":"{text}","link":{{"url":"{href}"}}}},
        "annotations":{defaultAnnotations},
        "plain_text":"{text}",
        "href":"{href}"
    }}"""

let private richTextJsonBold (text: string) =
    $"""{{
        "type":"text",
        "text":{{"content":"{text}"}},
        "annotations":{{"bold":true,"italic":false,"strikethrough":false,"underline":false,"code":false,"color":"default"}},
        "plain_text":"{text}",
        "href":null
    }}"""

let richTextTests =
    testList "Parse.richText" [
        test "parses plain text" {
            let json = richTextJson "Hello"
            use doc = JsonDocument.Parse(json)
            let rt = Parse.richText doc.RootElement
            Expect.equal rt.plainText "Hello" "plain text"
            Expect.isNone rt.href "no href"
            Expect.isFalse rt.annotations.bold "not bold"
            Expect.isFalse rt.annotations.code "not code"
        }

        test "parses text with href" {
            let json = richTextJsonWithHref "Link" "https://example.com"
            use doc = JsonDocument.Parse(json)
            let rt = Parse.richText doc.RootElement
            Expect.equal rt.plainText "Link" "plain text"
            Expect.equal rt.href (Some "https://example.com") "href"
        }

        test "parses bold annotation" {
            let json = richTextJsonBold "Bold"
            use doc = JsonDocument.Parse(json)
            let rt = Parse.richText doc.RootElement
            Expect.isTrue rt.annotations.bold "bold"
            Expect.isFalse rt.annotations.italic "not italic"
        }
    ]

let propertyValueTests =
    testList "Parse.propertyValue" [
        test "parses title" {
            let json = $"""{{"type":"title","title":[{richTextJson "My Title"}]}}"""
            use doc = JsonDocument.Parse(json)
            let pv = Parse.propertyValue doc.RootElement
            match pv with
            | PropertyValue.Title texts ->
                Expect.equal texts.Length 1 "one item"
                Expect.equal texts.[0].plainText "My Title" "title text"
            | _ -> failtestf "expected Title, got %A" pv
        }

        test "parses rich_text" {
            let json = $"""{{"type":"rich_text","rich_text":[{richTextJson "Some text"}]}}"""
            use doc = JsonDocument.Parse(json)
            let pv = Parse.propertyValue doc.RootElement
            match pv with
            | PropertyValue.RichText texts ->
                Expect.equal texts.[0].plainText "Some text" "text"
            | _ -> failtestf "expected RichText, got %A" pv
        }

        test "parses email" {
            let json = """{"type":"email","email":"test@example.com"}"""
            use doc = JsonDocument.Parse(json)
            let pv = Parse.propertyValue doc.RootElement
            Expect.equal pv (PropertyValue.Email "test@example.com") "email"
        }

        test "parses null email" {
            let json = """{"type":"email","email":null}"""
            use doc = JsonDocument.Parse(json)
            let pv = Parse.propertyValue doc.RootElement
            Expect.equal pv (PropertyValue.Email "") "empty email"
        }

        test "parses checkbox true" {
            let json = """{"type":"checkbox","checkbox":true}"""
            use doc = JsonDocument.Parse(json)
            let pv = Parse.propertyValue doc.RootElement
            Expect.equal pv (PropertyValue.Checkbox true) "checkbox"
        }

        test "parses date" {
            let json = """{"type":"date","date":{"start":"2025-01-15T00:00:00.000+00:00","end":null}}"""
            use doc = JsonDocument.Parse(json)
            let pv = Parse.propertyValue doc.RootElement
            match pv with
            | PropertyValue.Date(Some dt) ->
                Expect.equal dt.Year 2025 "year"
                Expect.equal dt.Month 1 "month"
                Expect.equal dt.Day 15 "day"
            | _ -> failtestf "expected Date, got %A" pv
        }

        test "parses null date" {
            let json = """{"type":"date","date":null}"""
            use doc = JsonDocument.Parse(json)
            let pv = Parse.propertyValue doc.RootElement
            Expect.equal pv (PropertyValue.Date None) "null date"
        }

        test "parses status" {
            let json = """{"type":"status","status":{"name":"Published"}}"""
            use doc = JsonDocument.Parse(json)
            let pv = Parse.propertyValue doc.RootElement
            Expect.equal pv (PropertyValue.Status "Published") "status"
        }

        test "parses null status" {
            let json = """{"type":"status","status":null}"""
            use doc = JsonDocument.Parse(json)
            let pv = Parse.propertyValue doc.RootElement
            Expect.equal pv (PropertyValue.Status "") "null status"
        }

        test "parses select" {
            let json = """{"type":"select","select":{"name":"Option A"}}"""
            use doc = JsonDocument.Parse(json)
            let pv = Parse.propertyValue doc.RootElement
            Expect.equal pv (PropertyValue.Select "Option A") "select"
        }

        test "parses multi_select" {
            let json = """{"type":"multi_select","multi_select":[{"name":"Tag1"},{"name":"Tag2"}]}"""
            use doc = JsonDocument.Parse(json)
            let pv = Parse.propertyValue doc.RootElement
            Expect.equal pv (PropertyValue.MultiSelect [ "Tag1"; "Tag2" ]) "multi_select"
        }

        test "parses unknown type" {
            let json = """{"type":"formula","formula":{"type":"number","number":42}}"""
            use doc = JsonDocument.Parse(json)
            let pv = Parse.propertyValue doc.RootElement
            Expect.equal pv PropertyValue.Unknown "unknown"
        }
    ]

let fileObjectTests =
    testList "Parse.fileObject" [
        test "parses uploaded file" {
            let json = """{"type":"file","file":{"url":"https://s3.amazonaws.com/img.png"}}"""
            use doc = JsonDocument.Parse(json)
            let fo = Parse.fileObject doc.RootElement
            Expect.equal fo (Some { url = "https://s3.amazonaws.com/img.png" }) "uploaded file"
        }

        test "parses external file" {
            let json = """{"type":"external","external":{"url":"https://example.com/img.png"}}"""
            use doc = JsonDocument.Parse(json)
            let fo = Parse.fileObject doc.RootElement
            Expect.equal fo (Some { url = "https://example.com/img.png" }) "external file"
        }

        test "returns None for unknown type" {
            let json = """{"type":"emoji","emoji":"🎉"}"""
            use doc = JsonDocument.Parse(json)
            let fo = Parse.fileObject doc.RootElement
            Expect.isNone fo "no file"
        }
    ]

let blockTests =
    testList "Parse.block" [
        test "parses paragraph" {
            let json =
                $"""{{
                    "object":"block","id":"abc-123","type":"paragraph","has_children":false,
                    "paragraph":{{"rich_text":[{richTextJson "Hello world"}]}}
                }}"""
            use doc = JsonDocument.Parse(json)
            let b = Parse.block doc.RootElement
            Expect.equal b.id "abc-123" "id"
            Expect.isFalse b.hasChildren "no children"
            match b.blockType with
            | BlockType.Paragraph rt ->
                Expect.equal rt.[0].plainText "Hello world" "text"
            | _ -> failtestf "expected Paragraph, got %A" b.blockType
        }

        test "parses heading_1" {
            let json =
                $"""{{
                    "object":"block","id":"h1-id","type":"heading_1","has_children":false,
                    "heading_1":{{"rich_text":[{richTextJson "Title"}]}}
                }}"""
            use doc = JsonDocument.Parse(json)
            let b = Parse.block doc.RootElement
            match b.blockType with
            | BlockType.Heading1 rt -> Expect.equal rt.[0].plainText "Title" "heading text"
            | _ -> failtestf "expected Heading1, got %A" b.blockType
        }

        test "parses heading_2" {
            let json =
                $"""{{
                    "object":"block","id":"h2-id","type":"heading_2","has_children":false,
                    "heading_2":{{"rich_text":[{richTextJson "Subtitle"}]}}
                }}"""
            use doc = JsonDocument.Parse(json)
            let b = Parse.block doc.RootElement
            match b.blockType with
            | BlockType.Heading2 rt -> Expect.equal rt.[0].plainText "Subtitle" "heading text"
            | _ -> failtestf "expected Heading2, got %A" b.blockType
        }

        test "parses heading_3" {
            let json =
                $"""{{
                    "object":"block","id":"h3-id","type":"heading_3","has_children":false,
                    "heading_3":{{"rich_text":[{richTextJson "Section"}]}}
                }}"""
            use doc = JsonDocument.Parse(json)
            let b = Parse.block doc.RootElement
            match b.blockType with
            | BlockType.Heading3 rt -> Expect.equal rt.[0].plainText "Section" "heading text"
            | _ -> failtestf "expected Heading3, got %A" b.blockType
        }

        test "parses bulleted_list_item" {
            let json =
                $"""{{
                    "object":"block","id":"bl-id","type":"bulleted_list_item","has_children":false,
                    "bulleted_list_item":{{"rich_text":[{richTextJson "Item 1"}]}}
                }}"""
            use doc = JsonDocument.Parse(json)
            let b = Parse.block doc.RootElement
            match b.blockType with
            | BlockType.BulletedListItem(rt, children) ->
                Expect.equal rt.[0].plainText "Item 1" "text"
                Expect.isEmpty children "no children initially"
            | _ -> failtestf "expected BulletedListItem, got %A" b.blockType
        }

        test "parses numbered_list_item" {
            let json =
                $"""{{
                    "object":"block","id":"nl-id","type":"numbered_list_item","has_children":false,
                    "numbered_list_item":{{"rich_text":[{richTextJson "Step 1"}]}}
                }}"""
            use doc = JsonDocument.Parse(json)
            let b = Parse.block doc.RootElement
            match b.blockType with
            | BlockType.NumberedListItem(rt, children) ->
                Expect.equal rt.[0].plainText "Step 1" "text"
                Expect.isEmpty children "no children initially"
            | _ -> failtestf "expected NumberedListItem, got %A" b.blockType
        }

        test "parses code block" {
            let json =
                $"""{{
                    "object":"block","id":"code-id","type":"code","has_children":false,
                    "code":{{"rich_text":[{richTextJson "let x = 1"}],"language":"f#"}}
                }}"""
            use doc = JsonDocument.Parse(json)
            let b = Parse.block doc.RootElement
            match b.blockType with
            | BlockType.Code(rt, lang) ->
                Expect.equal rt.[0].plainText "let x = 1" "code text"
                Expect.equal lang "f#" "language"
            | _ -> failtestf "expected Code, got %A" b.blockType
        }

        test "parses image with uploaded file" {
            let json =
                """{"object":"block","id":"img-id","type":"image","has_children":false,
                    "image":{"type":"file","file":{"url":"https://s3.example.com/photo.png"}}}"""
            use doc = JsonDocument.Parse(json)
            let b = Parse.block doc.RootElement
            match b.blockType with
            | BlockType.Image url -> Expect.equal url "https://s3.example.com/photo.png" "image url"
            | _ -> failtestf "expected Image, got %A" b.blockType
        }

        test "parses image with external file" {
            let json =
                """{"object":"block","id":"img-id","type":"image","has_children":false,
                    "image":{"type":"external","external":{"url":"https://example.com/photo.png"}}}"""
            use doc = JsonDocument.Parse(json)
            let b = Parse.block doc.RootElement
            match b.blockType with
            | BlockType.Image url -> Expect.equal url "https://example.com/photo.png" "image url"
            | _ -> failtestf "expected Image, got %A" b.blockType
        }

        test "parses divider" {
            let json = """{"object":"block","id":"div-id","type":"divider","has_children":false,"divider":{}}"""
            use doc = JsonDocument.Parse(json)
            let b = Parse.block doc.RootElement
            Expect.equal b.blockType BlockType.Divider "divider"
        }

        test "parses quote" {
            let json =
                $"""{{
                    "object":"block","id":"q-id","type":"quote","has_children":false,
                    "quote":{{"rich_text":[{richTextJson "A wise quote"}]}}
                }}"""
            use doc = JsonDocument.Parse(json)
            let b = Parse.block doc.RootElement
            match b.blockType with
            | BlockType.Quote rt -> Expect.equal rt.[0].plainText "A wise quote" "quote text"
            | _ -> failtestf "expected Quote, got %A" b.blockType
        }

        test "parses callout" {
            let json =
                $"""{{
                    "object":"block","id":"co-id","type":"callout","has_children":false,
                    "callout":{{"rich_text":[{richTextJson "Note this"}],"icon":{{"type":"emoji","emoji":"💡"}}}}
                }}"""
            use doc = JsonDocument.Parse(json)
            let b = Parse.block doc.RootElement
            match b.blockType with
            | BlockType.Callout rt -> Expect.equal rt.[0].plainText "Note this" "callout text"
            | _ -> failtestf "expected Callout, got %A" b.blockType
        }

        test "parses has_children true" {
            let json =
                $"""{{
                    "object":"block","id":"parent-id","type":"bulleted_list_item","has_children":true,
                    "bulleted_list_item":{{"rich_text":[{richTextJson "Parent"}]}}
                }}"""
            use doc = JsonDocument.Parse(json)
            let b = Parse.block doc.RootElement
            Expect.isTrue b.hasChildren "has children"
        }

        test "parses unsupported block type" {
            let json =
                """{"object":"block","id":"x-id","type":"table_of_contents","has_children":false,"table_of_contents":{}}"""
            use doc = JsonDocument.Parse(json)
            let b = Parse.block doc.RootElement
            Expect.equal b.blockType BlockType.Unsupported "unsupported"
        }
    ]

let pageTests =
    testList "Parse.page" [
        test "parses page with all property types" {
            let json =
                $"""{{
                    "object":"page",
                    "id":"page-123",
                    "icon":{{"type":"external","external":{{"url":"https://example.com/icon.png"}}}},
                    "cover":{{"type":"file","file":{{"url":"https://s3.example.com/cover.jpg"}}}},
                    "properties":{{
                        "Title":{{"type":"title","title":[{richTextJson "My Article"}]}},
                        "Summary":{{"type":"rich_text","rich_text":[{richTextJson "A summary"}]}},
                        "Email":{{"type":"email","email":"a@b.com"}},
                        "Active":{{"type":"checkbox","checkbox":true}},
                        "Created At":{{"type":"date","date":{{"start":"2025-06-01T00:00:00.000+00:00"}}}},
                        "Status":{{"type":"status","status":{{"name":"Published"}}}},
                        "Category":{{"type":"select","select":{{"name":"Tech"}}}},
                        "Tags":{{"type":"multi_select","multi_select":[{{"name":"F#"}},{{"name":"Web"}}]}}
                    }}
                }}"""
            use doc = JsonDocument.Parse(json)
            let p = Parse.page doc.RootElement
            Expect.equal p.id "page-123" "id"
            Expect.equal p.icon (Some { url = "https://example.com/icon.png" }) "icon"
            Expect.equal p.cover (Some { url = "https://s3.example.com/cover.jpg" }) "cover"
            Expect.equal (Map.count p.properties) 8 "8 properties"
        }

        test "parses page with no icon or cover" {
            let json =
                $"""{{
                    "object":"page",
                    "id":"page-456",
                    "icon":null,
                    "cover":null,
                    "properties":{{
                        "Title":{{"type":"title","title":[{richTextJson "Simple"}]}}
                    }}
                }}"""
            use doc = JsonDocument.Parse(json)
            let p = Parse.page doc.RootElement
            Expect.isNone p.icon "no icon"
            Expect.isNone p.cover "no cover"
        }
    ]

let paginatedTests =
    testList "Parse.paginated" [
        test "parses paginated pages" {
            let json =
                $"""{{
                    "object":"list",
                    "results":[{{
                        "object":"page","id":"p1",
                        "icon":null,"cover":null,
                        "properties":{{"Title":{{"type":"title","title":[{richTextJson "Page 1"}]}}}}
                    }}],
                    "has_more":true,
                    "next_cursor":"cursor-abc"
                }}"""
            use doc = JsonDocument.Parse(json)
            let res = Parse.paginatedPages doc
            Expect.equal res.results.Length 1 "one result"
            Expect.isTrue res.hasMore "has more"
            Expect.equal res.nextCursor (Some (Cursor.ofString "cursor-abc")) "cursor"
        }

        test "parses paginated pages with no more" {
            let json =
                """{"object":"list","results":[],"has_more":false,"next_cursor":null}"""
            use doc = JsonDocument.Parse(json)
            let res = Parse.paginatedPages doc
            Expect.isEmpty res.results "empty"
            Expect.isFalse res.hasMore "no more"
            Expect.isNone res.nextCursor "no cursor"
        }

        test "parses paginated blocks" {
            let json =
                $"""{{
                    "object":"list",
                    "results":[
                        {{"object":"block","id":"b1","type":"paragraph","has_children":false,
                          "paragraph":{{"rich_text":[{richTextJson "Hello"}]}}}},
                        {{"object":"block","id":"b2","type":"divider","has_children":false,"divider":{{}}}}
                    ],
                    "has_more":false,
                    "next_cursor":null
                }}"""
            use doc = JsonDocument.Parse(json)
            let res = Parse.paginatedBlocks doc
            Expect.equal res.results.Length 2 "two blocks"
            Expect.isFalse res.hasMore "no more"
        }
    ]

[<Tests>]
let tests =
    testList "Notion.Parse" [
        richTextTests
        propertyValueTests
        fileObjectTests
        blockTests
        pageTests
        paginatedTests
    ]
