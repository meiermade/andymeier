module App.Articles.View

open Domain.Article
open FSharp.ViewEngine
open App.Common.View
open type Datastar
open type Html

let articlesPage (articles:Article list) =
    let content =
        div {
            _class "mx-auto max-w-5xl py-10 px-4"
            header {
                h1 { _class "text-4xl text-gray-900 dark:text-gray-100 font-medium"; "Articles" }
                p { _class "mt-4 text-lg text-gray-600 dark:text-gray-400"; "My thoughts on finance and technology" }
            }
            div {
                _class "mt-10"
                div {
                    div {
                        _class "flex flex-col space-y-4"
                        for a in articles do ArticleCard.summary a
                    }
                }
            }
        }
    Page.primary content

module RichTextView =
    let private notionBlockLinkRegex = System.Text.RegularExpressions.Regex(@"^/[0-9a-f]{32}#([0-9a-f]{32})$")

    let private toAnchorHref (href:string) =
        let m = notionBlockLinkRegex.Match(href)
        if m.Success then $"#{m.Groups.[1].Value}"
        else href

    let toHtml (t: Domain.Notion.RichText) =
        let inner =
            if t.annotations.code then
                code { _class "language-none"; t.plainText }
            else
                span {
                    _class [
                        if t.annotations.bold then "font-bold"
                        if t.annotations.italic then "italic"
                        if t.annotations.underline then "underline"
                        if t.annotations.strikethrough then "line-through"
                    ]
                    t.plainText
                }

        match t.href with
        | Some href -> a { _href (toAnchorHref href); inner }
        | None -> inner

module Block =
    let (|Bulleted|Numbered|Other|) (block:Domain.Notion.Block) =
        match block.blockType with
        | Domain.Notion.BlockType.BulletedListItem _ -> Bulleted
        | Domain.Notion.BlockType.NumberedListItem _ -> Numbered
        | _ -> Other

    let rec toHtml (block:Domain.Notion.Block) : HtmlElement =
        let cleanId = block.id.Replace("-", "")

        match block.blockType with
        | Domain.Notion.BlockType.Heading1 richText ->
            h2 {
                _class "mt-8"; _id cleanId
                for t in richText do RichTextView.toHtml t
            }
        | Domain.Notion.BlockType.Heading2 richText ->
            h3 {
                _class "mt-6"; _id cleanId
                for t in richText do RichTextView.toHtml t
            }
        | Domain.Notion.BlockType.Heading3 richText ->
            h4 {
                _class "mt-4"; _id cleanId
                for t in richText do RichTextView.toHtml t
            }
        | Domain.Notion.BlockType.Paragraph richText ->
            div {
                if List.isEmpty richText then br
                else for t in richText do RichTextView.toHtml t
            }
        | Domain.Notion.BlockType.BulletedListItem(richText, children) ->
            li {
                for t in richText do RichTextView.toHtml t
                for child in children do toHtml child
            }
        | Domain.Notion.BlockType.NumberedListItem(richText, children) ->
            li {
                for t in richText do RichTextView.toHtml t
                for child in children do toHtml child
            }
        | Domain.Notion.BlockType.Code(richText, language) ->
            let language =
                match language with
                | "f#" -> "fsharp"
                | "JSON" -> "json"
                | "TOML" -> "toml"
                | other -> other
            pre {
                _class $"language-{language}"
                code {
                    _class $"language-{language}"
                    for t in richText do RichTextView.toHtml t
                }
            }
        | Domain.Notion.BlockType.Image url ->
            img { _class "drop-shadow-xl rounded"; _src url }
        | Domain.Notion.BlockType.Divider ->
            div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
        | Domain.Notion.BlockType.Quote richText ->
            blockquote { for t in richText do RichTextView.toHtml t }
        | Domain.Notion.BlockType.Callout richText ->
            div {
                _class "bg-gray-200 dark:bg-gray-800 rounded p-2"
                for t in richText do RichTextView.toHtml t
            }
        | Domain.Notion.BlockType.Unsupported -> empty

module Content =
    let toHtml (blocks:Domain.Notion.Block list) =
        let elements = ResizeArray<HtmlElement>()
        let bulletedListItems = ResizeArray<HtmlElement>()
        let numberedListItems = ResizeArray<HtmlElement>()

        let flushBulletedListItems () =
            let children = List.ofSeq bulletedListItems
            let unorderedList = ul { _class "list-disc"; for c in children do c }
            elements.Add(unorderedList)
            bulletedListItems.Clear()

        let flushNumberedListItems () =
            let children = List.ofSeq numberedListItems
            let orderedList = ol { _class "list-decimal"; for c in children do c }
            elements.Add(orderedList)
            numberedListItems.Clear()

        for block in blocks do
            match block with
            | Block.Bulleted ->
                if numberedListItems.Count > 0 then flushNumberedListItems()
                bulletedListItems.Add(Block.toHtml block)
            | Block.Numbered ->
                if bulletedListItems.Count > 0 then flushBulletedListItems()
                numberedListItems.Add(Block.toHtml block)
            | Block.Other ->
                if numberedListItems.Count > 0 then flushNumberedListItems()
                if bulletedListItems.Count > 0 then flushBulletedListItems()
                elements.Add(Block.toHtml block)

        if numberedListItems.Count > 0 then flushNumberedListItems()
        if bulletedListItems.Count > 0 then flushBulletedListItems()
        List.ofSeq elements

let articlePage (article':Article) =
    let content =
        div {
            div {
                _class "bg-cover bg-no-repeat bg-center bg-blend-overlay bg-gray-800"
                _style $"background-image: url('{article'.cover}')"
                div {
                    _class "pt-28 pb-20 px-4 mx-auto max-w-5xl flex flex-col justify-end items-start text-gray-50"
                    time {
                        _class "text-base text-gray-50 border-l border-gray-300 pl-2"
                        _datetime (article'.createdAt.ToString("yyyy-MM-dd"))
                        article'.createdAt.ToString("MMMM d, yyyy")
                    }
                    h1 {
                        _class "mt-4 text-4xl font-bold tracking-tight text-gray-50"
                        article'.title
                    }
                }
            }
            article {
                _class "mx-auto max-w-5xl px-4"
                div {
                    _class "mt-8 prose prose-lg dark:prose-invert prose-code:before:hidden prose-code:after:hidden max-w-none"
                    _dataInit "highlightCode($el)"
                    for el in Content.toHtml article'.blocks do el
                }
            }
            script { _src (Asset.fingerprinted "/scripts/prism.1.29.0.js") }
            script { js "function highlightCode(el){if(el?.querySelectorAll)Prism.highlightAllUnder(el)}" }
        }
    Page.primary content

let notFoundPage =
    let content =
        div {
            _class "flex flex-col items-center"
            h1 { _class "text-3xl text-gray-800 dark:text-gray-100"; "Could not find page." }
            p { _class "mt-2 text-md text-gray-600 dark:text-gray-400"; "Something went wrong. Try refreshing the page." }
        }
    Page.primary content
