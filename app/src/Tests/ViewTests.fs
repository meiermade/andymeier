module ViewTests

open App.Common.View
open Expecto
open FSharp.ViewEngine
open System.Collections.Generic
open type Html

[<Tests>]
let tests =
    testList "View" [
        testList "Asset" [
            test "uses fingerprinted path from manifest when present" {
                let manifest = Dictionary<string, string>()
                manifest.Add("/css/compiled.css", "/css/compiled.abc123.css")

                let path = Asset.resolveWithManifest manifest "/css/compiled.css"

                Expect.equal path "/css/compiled.abc123.css" "Expected manifest fingerprinted path"
            }

            test "falls back to original path when manifest entry is missing" {
                let manifest = Dictionary<string, string>()
                manifest.Add("/css/other.css", "/css/other.abc123.css")

                let path = Asset.resolveWithManifest manifest "/css/compiled.css"

                Expect.equal path "/css/compiled.css" "Expected original path when manifest entry is missing"
            }
        ]

        testList "Document" [
            test "includes consent banner and delayed google analytics loading" {
                let doc = Document.primary(div { "Hello" }, "G-TEST123", "nav-home")

                let html = Render.toHtmlDocString doc

                Expect.stringContains html "<title>Andy Meier</title>" "Expected page to render"
                Expect.stringContains html "selectedNav: 'nav-home'" "Expected nav signal to render"
                Expect.stringContains html "cookie-consent-banner" "Expected consent banner"
                Expect.stringContains html "Reject" "Expected reject action"
                Expect.stringContains html "Accept" "Expected accept action"
                Expect.stringContains html "gtag('consent','default',{analytics_storage:'denied'" "Expected denied-by-default consent mode"
                Expect.stringContains html "localStorage.setItem('analytics-consent',v)" "Expected consent to be persisted"
                Expect.stringContains html "https://www.googletagmanager.com/gtag/js?id=G-TEST123" "Expected deferred gtag script source"
                Expect.stringContains html "gtag('config','G-TEST123');" "Expected GA config call after consent"
            }
        ]
    ]
