namespace App.Components

open FSharp.ViewEngine
open type Html

module internal Badge =
    let render attributes children =
        if List.isEmpty children then invalidArg "children" "Badge content is required."
        let variant = ComponentElement.choice "variant" "soft" ["solid"; "soft"; "outline"; "ghost"] attributes
        let color = ComponentElement.value "color" attributes |> Option.defaultValue "neutral"
        let appearance =
            match variant with
            | "solid" -> ComponentColors.solid
            | "outline" -> ComponentColors.outline
            | "ghost" -> ComponentColors.ghost
            | _ when color = "neutral" || color = "custom" -> ComponentColors.soft
            | _ -> "bg-[color-mix(in_oklab,var(--fve-color-soft)_45%,var(--fve-surface))] text-[color-mix(in_oklab,var(--fve-color-text)_65%,var(--fve-text))]"
        ComponentElement.render "span"
            [ _class ("inline-flex items-center gap-1.5 rounded-[var(--fve-radius-control)] px-2 py-1 text-xs font-medium " + appearance)
              _style (ComponentColors.style (ComponentElement.palette attributes) []) ] (ComponentHtml.safeAttributes ["variant"; "color"; "palette"] attributes) children

/// <category>badge</category>
[<AutoOpen>]
module BadgeDsl =
    /// A non-announcing native badge. Content, icons and attributes compose normally.
    let fveBadge = ComponentElementBuilder(Badge.render)
