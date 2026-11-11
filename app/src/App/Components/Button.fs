namespace App.Components

open System
open System.Text.RegularExpressions
open System.Text.Json
open FSharp.ViewEngine
open type Html
open type Datastar

module internal Button =

    let private indicatorSignal (attribute:HtmlAttribute) =
        let parts = attribute.Name.ToLowerInvariant().Split([|"__"|], StringSplitOptions.None)
        let name = parts[0]
        let signal =
            if name.StartsWith("data-indicator:", StringComparison.Ordinal) then
                let camel (value:string) = Regex.Replace(value, "-([a-z])", fun (matched:Match) -> matched.Groups[1].Value.ToUpperInvariant())
                let cases =
                    parts |> Array.tryFindBack (fun part -> part.StartsWith("case.", StringComparison.Ordinal))
                    |> Option.map (fun part -> part.Substring(5).Split('.'))
                    |> Option.defaultValue [|"camel"|]
                (name.Substring("data-indicator:".Length), cases)
                ||> Array.fold (fun value casing ->
                    match casing with
                    | "camel" -> camel value
                    | "snake" -> value.Replace('-', '_')
                    | "pascal" when value.Length > 0 -> value.Substring(0,1).ToUpperInvariant() + camel (value.Substring(1))
                    | _ -> value)
                |> Some
            elif name = "data-indicator" then Some (attribute.Value |> ValueOption.defaultValue "")
            else None
        signal |> Option.map (fun value ->
            if not (Regex.IsMatch(value, @"^[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)*$")) then
                invalidArg "attributes" "A Datastar indicator must name a signal without '$'."
            value)

    let private renderHost (authored:HtmlAttribute list) (children:HtmlElement list) =
        let attributes =
            authored
            |> List.groupBy (fun item -> item.Name.ToLowerInvariant())
            |> List.map (fun (name, items) ->
                if name = "class" || name = "style" then
                    let separator = if name = "style" then ";" else " "
                    Html._attr(name, items |> List.choose (fun item -> ValueOption.toOption item.Value) |> String.concat separator)
                else List.last items)
        let attribute name = attributes |> List.tryFindBack (fun item -> String.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase))
        let value name = attribute name |> Option.bind (fun item -> ValueOption.toOption item.Value)
        for name,allowed in [ "color",["primary";"secondary";"success";"warning";"error";"info";"neutral";"custom"]
                              "variant",["solid";"soft";"outline";"ghost"]
                              "size",["extra-small";"small";"medium";"large";"extra-large"]
                              "type",["button";"submit";"reset"] ] do
            match attribute name with
            | Some { Value = ValueSome actual } when List.contains actual allowed -> ()
            | None -> ()
            | _ -> invalidArg "attributes" $"Unsupported Button {name}."
        if (attribute "href").IsSome then invalidArg "attributes" "Button cannot have href; use fveLink."
        if (attribute "role").IsSome then invalidArg "attributes" "Button supplies one native button role; do not put a second role on its host."
        if (attribute "aria-label").IsSome && (value "aria-label" |> Option.forall String.IsNullOrWhiteSpace) then
            invalidArg "attributes" "An accessible button label cannot be empty."
        if value "color" = Some "custom" && (attribute "palette").IsNone then
            invalidArg "attributes" "A custom Button color requires a palette."
        let indicator = attributes |> List.choose indicatorSignal |> List.tryLast
        let disabledBinding =
            if (attribute "disabled").IsSome && (indicator.IsSome || (attribute "data-attr:disabled").IsSome) then Some "true"
            elif (attribute "data-attr:disabled").IsSome then None
            else indicator |> Option.map (fun signal -> "$" + signal)
        let busyBinding =
            if (attribute "data-attr:aria-busy").IsSome then None
            else indicator |> Option.map (fun signal -> $"${signal} ? 'true' : null")
        (Html.el "fve-button") {
            _attr ("data-preserve-attr", "data-rocket-host data-scope-children")
            match disabledBinding with Some expression -> _dataAttr ("disabled", expression) | None -> ()
            match busyBinding with Some expression -> _dataAttr ("aria-busy", expression) | None -> ()
            for item in ComponentHtml.safeAttributes (
                ["data-preserve-attr"]
                @ (if disabledBinding.IsSome then ["data-attr:disabled"] else [])
                @ (if busyBinding.IsSome then ["data-attr:aria-busy"] else [])) attributes do
                // Caller-authored bindings remain page-owned rather than Rocket-local.
                let family = item.Name.Split([|":";"__"|], StringSplitOptions.None) |> Array.head |> fun name -> name.ToLowerInvariant()
                if List.contains family ["data-bind";"data-computed";"data-indicator";"data-ref"] && not (item.Name.Contains "__root") then
                    { item with Name = item.Name + "__root" }
                else item
            ComponentContent.fragment children
        }

    let render attributes children =
        let element = renderHost attributes children
        ComponentControlElement("fve-button", attributes, (fun attributes -> renderHost attributes children), element) :> HtmlElement

/// <summary>Real HTML attributes and ordinary content for a Rocket-owned native button.</summary>
/// <category>button</category>
type ButtonBuilder internal () =
    inherit ComponentElementBuilder(Button.render)

/// <category>button</category>
[<AutoOpen>]
module ButtonDsl =
    /// Render the custom-element host. Import Assets/button.js once from the application entry.
    let fveButton = ButtonBuilder()
