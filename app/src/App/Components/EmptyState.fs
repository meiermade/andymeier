namespace App.Components

open FSharp.ViewEngine
open type Html

module internal EmptyState =
    let part name attributes (children:HtmlElement list) =
        if List.isEmpty children then empty
        else
            let tag, defaults =
                if name = "icon" then
                    "span", [_ariaHidden true; _class "grid size-11 place-items-center rounded-full bg-[var(--fve-neutral-subtle)] text-[var(--fve-neutral-text)] ring-1 ring-inset ring-[var(--fve-neutral-ring)]"]
                else "div", [_class "mt-1 flex flex-wrap justify-center gap-2"]
            ComponentPart("empty-state", name, ComponentElement.render tag defaults attributes children) :> HtmlElement

    let render attributes children =
        let title = ComponentElement.required "label" attributes
        let description = ComponentElement.required "description" attributes
        let parts, body = ComponentParts.split "empty-state" children
        let region name = Map.tryFind name parts |> Option.defaultValue empty
        ComponentElement.render "div"
            [_class "grid justify-items-center gap-3 rounded-[var(--fve-radius-panel)] bg-[var(--fve-surface-subtle)] p-6 text-center ring-1 ring-inset ring-[var(--fve-border)]"] (ComponentHtml.safeAttributes ["label"; "description"] attributes)
            [ region "icon"
              div {
                  _class "max-w-xl"
                  p { _class "font-semibold text-[var(--fve-text)]"; title }
                  p { _class "mt-1 text-sm text-[var(--fve-muted-text)]"; description }
              }
              yield! body
              region "actions" ]

/// <category>empty-state</category>
[<AutoOpen>]
module EmptyStateDsl =
    let fveEmptyState = ComponentElementBuilder(EmptyState.render)
    let fveEmptyStateIcon = ComponentElementBuilder(EmptyState.part "icon")
    let fveEmptyStateActions = ComponentElementBuilder(EmptyState.part "actions")
