namespace App.Components

open System
open System.Globalization
open FSharp.ViewEngine
open type Html

/// <summary>Outline, Solid, Mini and Micro artwork. Logos use Solid.</summary>
/// <category>icon</category>
[<RequireQualifiedAccess>]
type IconVariant = Outline | Solid | Mini | Micro

/// <summary>Typed glyph selection and artwork family. Size is an optional rendered-size override.</summary>
/// <category>icon</category>
[<AutoOpen>]
module IconAttributes =
    type Html with
        static member _name(value:IconName) =
            let index = int value
            if index < 0 || index >= IconData.names.Length then invalidArg "value" "Unknown icon name."
            Html._attr("name", IconData.names[index])
        static member _variant(value:IconVariant) =
            Html._attr("variant", match value with IconVariant.Outline -> "outline" | IconVariant.Solid -> "solid" | IconVariant.Mini -> "mini" | IconVariant.Micro -> "micro")

module internal Icon =
    let render attributes (children:HtmlElement list) =
        if children |> List.exists (function :? NoopElement -> false | _ -> true) then
            invalidArg "children" "Icon content comes from its typed name."
        let name = ComponentElement.required "name" attributes
        let index = IconData.names |> Array.tryFindIndex ((=) name) |> Option.defaultWith (fun () -> invalidArg "name" "Unknown icon name.")
        let brand = index >= IconData.outline.Length
        let variant = ComponentElement.choice "variant" (if brand then "solid" else "outline") ["outline"; "solid"; "mini"; "micro"] attributes
        if brand && variant <> "solid" then
            invalidArg "variant" "Logos support Solid artwork; use _size to change their displayed size."
        let nativeSize, defaultSize, artwork =
            if brand then 16, 24, IconData.brand[index - IconData.outline.Length]
            else
                match variant with
                | "solid" -> 24, 24, IconData.solid[index]
                | "mini" -> 20, 20, IconData.mini[index]
                | "micro" -> 16, 16, IconData.micro[index]
                | _ -> 24, 24, IconData.outline[index]
        let dimension =
            match ComponentElement.size None attributes with
            | Some Size.ExtraSmall -> 12
            | Some Size.Small -> 16
            | Some Size.Medium -> 20
            | Some Size.Large -> 24
            | Some Size.ExtraLarge -> 32
            | None -> defaultSize
        let rem = (decimal dimension / 16m).ToString(CultureInfo.InvariantCulture) + "rem"
        ComponentElement.render "svg"
            [ _attr("xmlns", "http://www.w3.org/2000/svg")
              _attr("viewBox", $"0 0 {nativeSize} {nativeSize}")
              _attr("data-fve-icon", name)
              _attr("data-fve-icon-variant", variant)
              _attr("focusable", "false")
              if (ComponentElement.attribute "aria-label" attributes).IsSome then
                  ComponentElement.required "aria-label" attributes |> ignore
                  _role "img"
              else _ariaHidden true
              _class "inline-block shrink-0 align-middle"
              _style $"width:{rem};height:{rem}"
              if variant = "outline" then
                  _attr("fill", "none")
                  _attr("stroke", "currentColor")
                  _attr("stroke-width", "1.5")
              else _attr("fill", "currentColor") ]
            (ComponentHtml.safeAttributes ["name"; "variant"; "size"; "viewBox"; "xmlns"; "fill"; "stroke"; "stroke-width"] attributes)
            [ raw artwork ]

/// <summary>Inline, square SVG. Heroicons default to Outline; logos default to Solid at 24px. Controls own their accessible labels.</summary>
/// <category>icon</category>
[<AutoOpen>]
module IconDsl =
    let fveIcon = ComponentElementBuilder(Icon.render)
