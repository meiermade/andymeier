namespace App.Components

open FSharp.ViewEngine
open type Html

module internal ButtonGroup =
    let render attributes children =
        if List.isEmpty children then invalidArg "children" "A button group requires at least one segment."
        let label = ComponentElement.required "aria-label" attributes
        let orientation = ComponentElement.choice "orientation" "horizontal" ["horizontal"; "vertical"] attributes
        ComponentElement.render (if orientation = "horizontal" then "fve-scroll-region" else "div")
            [ _role "group"
              _ariaLabel label
              _class (ComponentHtml.classes [
                  ControlGroup.classes orientation
                  if orientation = "horizontal" then ComponentHtml.horizontalScrollClasses ])
              if orientation = "horizontal" then yield! ComponentHtml.horizontalScrollAttributes ]
            (ComponentHtml.safeAttributes ["role"; "aria-label"; "tabindex"] attributes) children

/// <summary>Joins native controls without changing their semantics or keyboard behavior.</summary>
/// <category>button-group</category>
type ButtonGroupBuilder internal () =
    inherit ComponentElementBuilder(ButtonGroup.render)
    member this.Yield(text:string) : ComponentCode<unit> =
        base.Yield(span {
            _data ("fve-group-control", "outline")
            _class "inline-flex shrink-0 items-center rounded-[var(--fve-radius-control)] border border-[var(--fve-border)] bg-[var(--fve-surface-subtle)] px-3 min-h-[var(--fve-control-min-height)] text-[length:var(--fve-control-font-size)] leading-[var(--fve-control-line-height)] text-[var(--fve-muted-text)]"
            text
        })

/// <category>button-group</category>
[<AutoOpen>]
module ButtonGroupDsl =
    /// A labelled group of ordinary controls; orientation is a shared typed HTML attribute.
    let fveButtonGroup = ButtonGroupBuilder()
