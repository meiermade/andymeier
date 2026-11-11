namespace App.Components

open System
open FSharp.ViewEngine
open type Html

module internal Link =
    let render attributes children =
        let destinations = attributes |> List.filter (fun (attribute:HtmlAttribute) -> String.Equals(attribute.Name, "href", StringComparison.OrdinalIgnoreCase))
        if destinations.Length <> 1 then invalidArg "attributes" "Link requires exactly one href."
        let href = ComponentElement.required "href" attributes
        for attribute in attributes do
            match attribute.Name.ToLowerInvariant(), attribute.Value with
            | ("disabled" | "form" | "formaction" | "formenctype" | "formmethod" | "formnovalidate" | "formtarget"), _
            | "type", ValueSome ("button" | "submit" | "reset") ->
                invalidArg "attributes" "Link does not support button-only attributes; use fveButton for actions."
            | "aria-label", ValueSome value when String.IsNullOrWhiteSpace value ->
                invalidArg "attributes" "An accessible link label cannot be empty."
            | _ -> ()
        let variant = ComponentElement.choice "variant" "text" ["text"; "solid"; "soft"; "outline"; "ghost"] attributes
        let size = ComponentElement.size None attributes
        let appearance =
            match variant with
            | "solid" -> ComponentColors.solid + " hover:bg-[var(--fve-color-hover)] active:bg-[var(--fve-color-active)]"
            | "soft" -> ComponentColors.soft + " hover:bg-[var(--fve-color-soft-hover)] active:bg-[var(--fve-color-soft-active)]"
            | "outline" -> "[--fve-control-border-width:1px] border border-[color-mix(in_srgb,var(--fve-color-border)_20%,transparent)] bg-transparent text-[var(--fve-color-text)] hover:bg-[var(--fve-color-soft-hover)] active:bg-[var(--fve-color-soft-active)]"
            | "ghost" -> ComponentColors.ghost + " hover:bg-[var(--fve-color-soft-hover)] active:bg-[var(--fve-color-soft-active)]"
            | _ -> "font-medium text-[var(--fve-color-text)] underline underline-offset-4 hover:decoration-2 outline-none focus-visible:ring-2 focus-visible:ring-offset-2"
        let classes = ComponentHtml.classes [
            if variant = "text" then
                ActionStyles.iconClasses
                "inline-flex items-center gap-2"
                ComponentHtml.controlSizeClass size
                if size.IsSome then "text-[length:var(--fve-control-font-size)] leading-[var(--fve-control-line-height)]"
            else
                ActionStyles.baseClasses
                ComponentHtml.controlSizeClass size
                ActionStyles.geometryClasses
            appearance
            ComponentColors.focus ]
        ComponentElement.render "a"
            [ _href href
              _data ("fve-group-control", if variant = "outline" then "outline" else "plain")
              _class classes
              _style (ComponentColors.style (ComponentElement.palette (_color Color.Primary :: attributes)) []) ]
            (ComponentHtml.safeAttributes ["href"; "variant"; "size"; "color"; "palette"] attributes) children

/// <summary>A native anchor with ordinary typed presentation and native destination attributes.</summary>
/// <category>link</category>
type LinkBuilder internal () =
    inherit ComponentElementBuilder(Link.render)

/// <category>link</category>
[<AutoOpen>]
module LinkDsl =
    /// Requires href. LinkVariant.Text is the default; other LinkVariant cases provide control-like links.
    let fveLink = LinkBuilder()
