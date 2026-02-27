[<AutoOpen>]
module App.Infrastructure

open FSharp.ViewEngine
open Giraffe
open type Html
open JetBrains.Annotations
open Markdig
open Microsoft.AspNetCore.Http

[<AutoOpen>]
module HtmlExtensions =
    let private markdownPipeline =
        MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build()

    type Html with
        static member markdown([<LanguageInjection("markdown")>] markdown:string) =
            let markdown' =
                if isNull markdown then ""
                else markdown

            let html = Markdown.ToHtml(markdown', markdownPipeline)

            div {
                _class "prose prose-lg dark:prose-invert max-w-none"
                raw html
            }

[<AutoOpen>]
module HttpContextExtensions =
    type HttpContext with
        member this.IsDatastar =
            match this.TryGetRequestHeader("Datastar-Request") with
            | Some "true" -> true
            | _ -> false
