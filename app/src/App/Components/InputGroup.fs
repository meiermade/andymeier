namespace App.Components

open FSharp.ViewEngine
open type Html

/// <category>input-group</category>
[<RequireQualifiedAccess>]
type InputGroupPosition = InlineStart | InlineEnd | BlockStart | BlockEnd

/// <category>input-group</category>
[<AutoOpen>]
module InputGroupAttributes =
    type Html with
        static member _position(value:InputGroupPosition) =
            Html._attr("position", match value with InputGroupPosition.InlineStart -> "inline-start" | InputGroupPosition.InlineEnd -> "inline-end" | InputGroupPosition.BlockStart -> "block-start" | InputGroupPosition.BlockEnd -> "block-end")

module internal InputGroup =
    let addon attributes children =
        let position = ComponentElement.choice "position" "inline-start" ["inline-start"; "inline-end"; "block-start"; "block-end"] attributes
        let placement =
            match position with
            | "inline-start" -> "order-1"
            | "inline-end" -> "order-3"
            | "block-start" -> "order-0 basis-full py-2"
            | _ -> "order-4 basis-full py-2"
        ComponentPart("input-group", position,
            ComponentElement.render "div"
                [_attr("data-fve-input-group-addon", position)
                 _class ("flex min-w-0 max-w-full flex-wrap items-center gap-2 text-sm text-[var(--fve-muted-text)] [overflow-wrap:anywhere] [&>svg]:size-4 " + placement)]
                (ComponentHtml.safeAttributes ["position"] attributes) children) :> HtmlElement

    let render attributes children =
        let parts, body = ComponentParts.split "input-group" children
        let control =
            match body with
            | [control] -> control
            | _ -> invalidArg "children" "An InputGroup needs one input, textarea or custom control."
        ComponentElement.render "div"
            [_role "group"
             _class "fve-input-group flex min-w-0 flex-wrap items-center gap-x-2 rounded-[var(--fve-radius-control)] border border-[var(--fve-border)] bg-[var(--fve-surface)] px-3 has-[>[data-fve-input-control]:focus-visible]:outline-2 has-[>[data-fve-input-control]:focus-visible]:outline-offset-2 has-[>[data-fve-input-control]:focus-visible]:outline-[var(--fve-brand-ring)] has-[>[data-fve-input-control]:disabled]:opacity-50 has-[>[data-fve-input-control]:read-only]:bg-[var(--fve-neutral-subtle)] has-[>[data-fve-input-control][aria-invalid=true]]:border-[var(--fve-critical-ring)] has-[>[data-fve-input-control][aria-invalid=true]]:outline-[var(--fve-critical-ring)] has-[>[data-fve-input-control]:user-invalid]:border-[var(--fve-critical-ring)] has-[>[data-fve-input-control]:user-invalid]:outline-[var(--fve-critical-ring)] [&>[data-fve-input-control]]:order-2 [&>[data-fve-input-control]]:flex-[1_0_min(8rem,100%)] [&>[data-fve-input-control]]:w-auto [&>[data-fve-input-control]]:min-h-[calc(var(--fve-control-min-height)-2px)] [&>[data-fve-input-control]]:rounded-none [&>[data-fve-input-control]]:border-0 [&>[data-fve-input-control]]:bg-transparent [&>[data-fve-input-control]]:px-0 [&>[data-fve-input-control]]:outline-none"]
            attributes
            [control; yield! parts |> Map.toList |> List.map snd]

/// <category>input-group</category>
[<AutoOpen>]
module InputGroupDsl =
    /// One shared frame. The native control precedes addons in the DOM; position controls visual placement.
    let fveInputGroup = ComponentElementBuilder(InputGroup.render)
    let fveInputGroupAddon = ComponentElementBuilder(InputGroup.addon)
