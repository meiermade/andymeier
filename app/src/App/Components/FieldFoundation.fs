namespace App.Components

open System
open FSharp.ViewEngine
open type Html

module internal TextField =
    let requiredText argument value =
        if String.IsNullOrWhiteSpace value then invalidArg argument "Accessible text is required."
        value

    let stableId value =
        if String.IsNullOrWhiteSpace value || (value |> Seq.exists Char.IsWhiteSpace) then
            invalidArg (nameof value) "A stable ID without whitespace is required."
        value

    let flag name attributes =
        match ComponentElement.attribute name attributes with
        | Some {Value = ValueNone} -> true
        | Some {Value = ValueSome value} -> value <> "false"
        | None -> false

    let size attributes =
        ComponentElement.value "size" attributes |> Option.bind Size.tryParse

    let classes attributes =
        ComponentHtml.classes [
            ComponentHtml.controlSizeClass (size attributes)
            "block w-full min-w-0 min-h-[var(--fve-control-min-height)] rounded-[var(--fve-radius-control)] border border-[var(--fve-border)] bg-[var(--fve-surface)] px-3 py-[max(0px,calc((var(--fve-control-min-height)-var(--fve-control-line-height)-2px)/2))] text-[length:var(--fve-control-font-size)] leading-[var(--fve-control-line-height)] text-[var(--fve-text)] placeholder:text-[var(--fve-muted-text)] focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--fve-brand-ring)] disabled:cursor-not-allowed disabled:opacity-50 read-only:bg-[var(--fve-neutral-subtle)] aria-invalid:border-[var(--fve-critical-ring)] aria-invalid:focus-visible:outline-[var(--fve-critical-ring)] user-invalid:border-[var(--fve-critical-ring)] user-invalid:focus-visible:outline-[var(--fve-critical-ring)]" ]

    let attributes defaults authored =
        let authored =
            if flag "readonly" authored || flag "disabled" authored then ComponentHtml.safeAttributes ["required"] authored
            else authored
        let authored =
            if size authored |> Option.isSome then ComponentHtml.safeAttributes ["size"] authored
            else authored
        ComponentElement.attributes (defaults @ authored)
