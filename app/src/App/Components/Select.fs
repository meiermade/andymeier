namespace App.Components

open System
open FSharp.ViewEngine
open type Html
open type Datastar

/// <category>select</category>
[<RequireQualifiedAccess>]
type SelectSearch = Static | Remote of endpoint:string

/// <category>select</category>
[<RequireQualifiedAccess>]
type SelectPosition = SelectedItem | TriggerStart | TriggerEnd

/// <category>select</category>
[<AutoOpen>]
module SelectAttributes =
    type Html with
        static member _search(value:SelectSearch) = Html._attr("search", match value with SelectSearch.Static -> "static" | SelectSearch.Remote endpoint -> if String.IsNullOrWhiteSpace endpoint then invalidArg "endpoint" "A search endpoint is required." else endpoint)
        static member _position(value:SelectPosition) = Html._attr("position", match value with SelectPosition.SelectedItem -> "selected-item" | SelectPosition.TriggerStart -> "trigger-start" | SelectPosition.TriggerEnd -> "trigger-end")
        static member _nativeFallback(value:bool) = Html._attr("native-fallback", if value then "true" else "false")
        static member _query(value:string) = Html._attr("query", value)
        static member _loading(value:bool) = Html._attr("loading", if value then "true" else "false")
        static member _error(value:string) = Html._attr("error", value)
        static member _error(value:string option) = Html._attr("error", value |> Option.defaultValue "")
        static member _retry(value:string) = Html._attr("retry", value)
        static member _emptyMessage(value:string) = Html._attr("empty-message", value)
        static member _loadingMessage(value:string) = Html._attr("loading-message", value)

[<NoEquality; NoComparison>]
type internal SelectChoice = { value:string; label:string; disabled:bool }

type internal SelectPart(kind:string, choices:SelectChoice list, render:string option -> HtmlElement) =
    inherit HtmlElement()
    member _.Kind = kind
    member _.Choices = choices
    member _.RenderOwned(owner:string option) = render owner
    override _.Render(writer) = (render None).Render writer

[<NoEquality; NoComparison>]
type internal SelectConfig =
    { isMultiple:bool; selectedMany:SelectChoice list; search:SelectSearch option; query:string option
      name:string; id:string option; options:SelectChoice list; selected:string option; label:string
      nativeFallback:bool; placeholder:string option; invalid:bool; isRequired:bool; isDisabled:bool
      isPending:bool; isReadOnly:bool; size:Size option; position:SelectPosition
      controlClass:string; attributes:HtmlAttribute list }

module internal Select =
    let private flag name attributes =
        match ComponentElement.attribute name attributes with
        | Some {Value = ValueNone} -> true
        | Some {Value = ValueSome value} -> value <> "false"
        | None -> false

    let private partAttributes protectedNames attributes = ComponentHtml.safeAttributes ("class" :: protectedNames) attributes
    let private actionClasses = ComponentHtml.classes [ComponentHtml.popupControlClasses; "fve-popup-control inline-flex min-h-8 items-center rounded-[var(--fve-radius-control)] px-2 text-xs leading-4 font-medium text-[var(--fve-muted-text)] hover:text-[var(--fve-text)] disabled:cursor-not-allowed disabled:opacity-50"]

    let internal renderItem (choice:SelectChoice) attributes (children:HtmlElement list) =
        button {
            _type "button"; _role "option"; _tabindex -1
            _disabled choice.disabled; _ariaDisabled choice.disabled; _ariaSelected false
            _dataPreserveAttr "id aria-selected aria-disabled disabled data-active"
            if choice.disabled then _attr("data-fve-option-disabled", "")
            _attr("data-fve-option-label", choice.label.ToLowerInvariant())
            _attr("data-fve-option-value", choice.value)
            _attr("data-fve-option-text", choice.label)
            _class (ComponentHtml.classes [ComponentHtml.popupItemClasses; "fve-popup-item flex w-full items-center justify-between gap-3 px-2 py-[var(--fve-control-padding-block)] text-left text-[length:var(--fve-control-font-size)] leading-[var(--fve-control-line-height)] font-normal text-[var(--fve-text)] disabled:cursor-not-allowed disabled:opacity-50"; ComponentContent.classes attributes])
            for attribute in partAttributes ["data-preserve-attr"; "type"; "role"; "tabindex"; "disabled"; "aria-disabled"; "aria-selected"; "data-fve-option-disabled"; "data-fve-option-label"; "data-fve-option-value"; "data-fve-option-text"] attributes do attribute
            span {
                _class "min-w-0 [overflow-wrap:anywhere]"
                if List.isEmpty children then choice.label else for child in children do child
            }
            span { _ariaHidden true; _hidden true; _attr("data-fve-select-check", ""); _class "shrink-0 font-semibold"; "✓" }
        }

    // Parts remain server-rendered light DOM. The owning Select initializes their IDs and behavior.
    let internal renderContent ownerId attributes (children:HtmlElement list) searchText isLoading (failure:string option) (retryEndpoint:string option) emptyMessage loadingMessage =
        let ownerId =
            ownerId |> Option.orElseWith (fun () ->
                ComponentElement.value "id" attributes |> Option.map (fun id ->
                    let stem = if id.EndsWith("-popup", StringComparison.Ordinal) then id[..id.Length - 7] elif id.EndsWith("-content", StringComparison.Ordinal) then id[..id.Length - 9] else id
                    if stem.StartsWith("fve-select-", StringComparison.Ordinal) then stem else "fve-select-" + ComponentHtml.signalToken stem))
        div {
            _attr("data-fve-select-content", ""); _attr("data-fve-select-query", searchText)
            if isLoading then _attr("data-fve-select-loading", "")
            _popover "auto"; _role "group"; _tabindex -1
            _style "inset:auto; margin:0.25rem 0; position-area:block-end span-inline-end; position-try-fallbacks:flip-block, flip-inline, flip-block flip-inline; width:anchor-size(width)"
            _class (ComponentHtml.classes [ComponentHtml.popupClasses; "fve-popup fixed z-30 open:flex max-h-60 flex-col overflow-hidden rounded-[var(--fve-radius-control)] py-1"; ComponentContent.classes attributes])
            for attribute in partAttributes ["popover"; "role"; "tabindex"; "aria-label"; "style"; "data-fve-select-content"; "data-fve-select-query"; "data-fve-select-loading"] attributes do attribute
            div {
                _hidden true; _attr("data-fve-select-search-frame", ""); _class "relative shrink-0 p-1"
                input {
                    _type "search"; _autocomplete "off"; _placeholder "Search options"; _value searchText
                    match ownerId with Some id -> _id(id + "-search"); _ariaControls(id + "-options") | None -> ()
                    _class (ComponentHtml.classes [ComponentHtml.popupFieldClasses; "block min-h-[var(--fve-control-min-height)] w-full rounded-[var(--fve-radius-control)] border border-[var(--fve-border)] bg-[var(--fve-surface)] px-10 py-[max(0px,calc(var(--fve-control-padding-block)-1px))] [&::-webkit-search-cancel-button]:appearance-none text-[length:var(--fve-control-font-size)] leading-[var(--fve-control-line-height)] text-[var(--fve-text)]"])
                }
                raw """<svg viewBox="0 0 20 20" fill="currentColor" class="pointer-events-none absolute left-4 top-1/2 size-4 -translate-y-1/2 text-[var(--fve-muted-text)]" aria-hidden="true"><path fill-rule="evenodd" d="M8.5 3.5a5 5 0 1 0 0 10 5 5 0 0 0 0-10ZM2 8.5a6.5 6.5 0 1 1 11.596 4.047l3.928 3.929a.75.75 0 1 1-1.06 1.06l-3.929-3.928A6.5 6.5 0 0 1 2 8.5Z" clip-rule="evenodd"/></svg>"""
                button {
                    _type "button"; _tabindex 0; _hidden true; _attr("data-fve-select-action", "clear-search")
                    _class "absolute right-2 top-1/2 grid size-8 -translate-y-1/2 place-items-center rounded-[var(--fve-radius-control)] text-[var(--fve-muted-text)] hover:text-[var(--fve-text)] focus-visible:outline-2 focus-visible:outline-[var(--fve-brand-ring)]"
                    raw """<svg viewBox="0 0 20 20" fill="currentColor" class="size-4" aria-hidden="true"><path d="M5.22 5.22a.75.75 0 0 1 1.06 0L10 8.94l3.72-3.72a.75.75 0 1 1 1.06 1.06L11.06 10l3.72 3.72a.75.75 0 1 1-1.06 1.06L10 11.06l-3.72 3.72a.75.75 0 0 1-1.06-1.06L8.94 10 5.22 6.28a.75.75 0 0 1 0-1.06Z"/></svg>"""
                }
            }
            div {
                _hidden true; _attr("data-fve-select-multiple-actions", ""); _class "flex shrink-0 items-center justify-between px-2 py-1"
                button { _type "button"; _tabindex 0; _attr("data-fve-select-action", "all"); _class actionClasses; "Select all" }
                button { _type "button"; _tabindex 0; _attr("data-fve-select-action", "clear"); _class (actionClasses + " ml-auto"); "Clear selection" }
            }
            div {
                _attr("data-fve-select-options", ""); _role "listbox"; _tabindex 0
                match ownerId with Some id -> _id(id + "-options") | None -> ()
                _dataPreserveAttr "id aria-label aria-multiselectable aria-required aria-busy aria-activedescendant"
                _class "min-h-0 overflow-auto outline-none"
                if failure.IsNone then for child in children do child
            }
            p {
                _role "status"; _hidden (not isLoading); _attr("data-fve-select-busy", "")
                _class "flex shrink-0 items-center justify-center gap-2 px-3 py-4 text-center text-sm text-[var(--fve-muted-text)]"
                ComponentHtml.loadingGlyph Size.Small
                Html.text loadingMessage
            }
            match failure with
            | Some message ->
                div {
                    _attr("data-fve-select-error", ""); _class "grid shrink-0 justify-items-center gap-2 px-3 py-4 text-center"
                    p { _role "alert"; _class "text-sm text-[var(--fve-critical-text)]"; message }
                    match retryEndpoint with
                    | Some endpoint ->
                        button {
                            _type "button"; _tabindex 0; _attr("data-fve-select-action", "retry"); _attr("data-fve-select-endpoint", endpoint)
                            _class (ComponentHtml.classes [ComponentHtml.popupControlClasses; "fve-popup-control inline-flex min-h-[var(--fve-control-min-height)] items-center justify-center rounded-[var(--fve-radius-control)] border border-[var(--fve-brand-ring)] px-3 py-[max(0px,calc(var(--fve-control-padding-block)-1px))] text-[length:var(--fve-control-font-size)] leading-[var(--fve-control-line-height)] font-medium text-[var(--fve-brand-text)] hover:bg-[var(--fve-brand-subtle)]"])
                            "Retry"
                        }
                    | None -> ()
                }
            | None -> p { _role "status"; _hidden true; _attr("data-fve-select-empty", ""); _class "shrink-0 px-3 py-4 text-center text-sm text-[var(--fve-muted-text)]"; Html.text emptyMessage }
        }

    let internal renderRoot (children:HtmlElement list) (config:SelectConfig) =
        let triggerId = config.id |> Option.defaultValue config.name
        let instanceId = ComponentHtml.signalToken triggerId
        let fieldId = "fve-select-" + instanceId
        let unavailable = config.isDisabled || config.isReadOnly
        let initial = config.selectedMany |> List.map (fun choice -> {| value = choice.value; label = choice.label |})
        let placeholder = config.placeholder |> Option.defaultValue (if config.isMultiple then "Select options" else "Select an option")
        let selectedLabel = if initial.IsEmpty then (if config.search.IsSome && not config.isMultiple then "" else placeholder) else initial |> List.map _.label |> String.concat ", "
        let search, endpoint = match config.search with None -> "none", "" | Some SelectSearch.Static -> "static", "" | Some(SelectSearch.Remote endpoint) -> "remote", endpoint
        let position = if config.position = SelectPosition.TriggerEnd then "block-end span-inline-start" else "block-end span-inline-end"
        let settings =
            System.Text.Json.JsonSerializer.Serialize
                {| multiple = config.isMultiple
                   search = search
                   endpoint = endpoint
                   unavailable = unavailable
                   disabled = config.isDisabled
                   required = config.isRequired
                   label = config.label
                   position = position
                   aligned = config.position = SelectPosition.SelectedItem && not config.isMultiple && config.search.IsNone
                   blocked = config.options |> List.filter _.disabled |> List.map _.value
                   name = config.name
                   placeholder = placeholder
                   initial = initial
                   query = config.query |> Option.defaultValue "" |}
        let describedBy = ComponentElement.value "aria-describedby" config.attributes |> Option.defaultValue ""
        let triggerClasses = ComponentHtml.classes [ComponentHtml.popupFieldClasses; "fve-popup-field fve-popup-control group flex min-h-[var(--fve-control-min-height)] w-full items-center justify-between gap-3 rounded-[var(--fve-radius-control)] border bg-[var(--fve-surface)] px-3 py-[max(0px,calc(var(--fve-control-padding-block)-1px))] text-left text-[length:var(--fve-control-font-size)] leading-[var(--fve-control-line-height)] font-normal text-[var(--fve-text)] outline-none [&:not([readonly])]:not-disabled:not-aria-disabled:not-aria-busy:hover:bg-[linear-gradient(var(--fve-hover-wash),var(--fve-hover-wash))] [&:not([readonly])]:not-disabled:not-aria-disabled:not-aria-busy:active:bg-[linear-gradient(var(--fve-pressed-wash),var(--fve-pressed-wash))] disabled:cursor-not-allowed disabled:opacity-50"; if config.invalid then "border-[var(--fve-critical-ring)]" else "border-[var(--fve-border)]"]
        (Html.el "fve-select") {
            _id(fieldId + "-field"); _attr("settings", settings)
            _attr("data-fve-select", instanceId); _attr("data-fve-select-field", fieldId); _attr("data-fve-select-trigger", triggerId)
            _class (ComponentHtml.classes [ComponentHtml.controlSizeClass config.size; "relative grid min-w-0 grid-cols-1 content-start gap-1.5"])
            for attribute in config.attributes do
                if attribute.Name.StartsWith("data-", StringComparison.OrdinalIgnoreCase) && not (attribute.Name.StartsWith("data-bind", StringComparison.OrdinalIgnoreCase)) then attribute
            if config.nativeFallback then
                div {
                    _attr("data-fve-select-native-frame", ""); _class "relative"
                    select {
                        _id triggerId; _attr("data-fve-select-native", ""); _data("fve-group-control", "outline"); _name config.name
                        if describedBy <> "" then _ariaDescribedby describedBy
                        _ariaInvalid config.invalid; _required config.isRequired; _disabled unavailable
                        _class (triggerClasses + " appearance-none pr-9")
                        if config.selected.IsNone then option { _value ""; _selected true; _disabled true; placeholder }
                        for choice in config.options do option { _value choice.value; _selected(config.selected = Some choice.value); _disabled choice.disabled; choice.label }
                    }
                    raw """<svg viewBox="0 0 20 20" fill="currentColor" class="pointer-events-none absolute right-3 top-1/2 size-4 -translate-y-1/2 text-[var(--fve-muted-text)]" aria-hidden="true"><path fill-rule="evenodd" d="M5.22 7.22a.75.75 0 0 1 1.06 0L10 10.94l3.72-3.72a.75.75 0 1 1 1.06 1.06l-4.25 4.25a.75.75 0 0 1-1.06 0L5.22 8.28a.75.75 0 0 1 0-1.06Z" clip-rule="evenodd"/></svg>"""
                }
            div {
                _attr("data-fve-select-values", ""); _class "hidden"; _dataIgnoreMorph
                if not config.isMultiple then
                    input { _id(fieldId + "-value"); _type "hidden"; _name config.name; _value(config.selected |> Option.defaultValue ""); _disabled(config.isDisabled || (config.nativeFallback && not config.isReadOnly)) }
                else for item in initial do input { _type "hidden"; _name config.name; _value item.value; _disabled config.isDisabled }
            }
            button {
                _id(if config.nativeFallback then fieldId + "-enhanced" else triggerId)
                _type "button"; _tabindex 0; _attr("data-fve-select-trigger-control", ""); _data("fve-group-control", "outline")
                _hidden config.nativeFallback
                if config.search.IsNone then _popovertarget(fieldId + "-popup")
                if config.search.IsNone && not config.isMultiple then _role "combobox"
                _ariaHaspopup(if config.search.IsSome then "dialog" else "listbox")
                _ariaControls(fieldId + (if config.search.IsSome then "-popup" else "-options")); _ariaExpanded false
                if config.search.IsNone && not config.isMultiple then _ariaRequired config.isRequired
                _ariaDisabled unavailable; _ariaInvalid config.invalid; _disabled unavailable
                if config.isPending then _ariaBusy true
                if config.search.IsNone && config.isMultiple then _ariaDescribedby(String.concat " " ([triggerId + "-selection"] @ if describedBy = "" then [] else [describedBy]))
                elif describedBy <> "" then _ariaDescribedby describedBy
                _class (ComponentHtml.classes [triggerClasses; config.controlClass])
                for attribute in partAttributes ["id"; "type"; "name"; "value"; "disabled"; "required"; "role"; "aria-haspopup"; "aria-controls"; "aria-describedby"; "aria-invalid"; "aria-expanded"; "aria-activedescendant"; "aria-required"; "aria-disabled"; "aria-busy"; "data-bind:"; "data-attr:"; "data-on:"; "data-signals"] config.attributes do attribute
                span {
                    if config.isMultiple then _id(triggerId + "-selection")
                    _attr("data-fve-select-label", ""); _class "min-w-0 truncate text-left"; selectedLabel
                }
                if config.isPending && config.search.IsNone then ComponentHtml.loadingGlyph Size.Small
                else raw """<svg viewBox="0 0 20 20" fill="currentColor" class="size-4 shrink-0 text-[var(--fve-muted-text)]" aria-hidden="true"><path fill-rule="evenodd" d="M5.22 7.22a.75.75 0 0 1 1.06 0L10 10.94l3.72-3.72a.75.75 0 1 1 1.06 1.06l-4.25 4.25a.75.75 0 0 1-1.06 0L5.22 8.28a.75.75 0 0 1 0-1.06Z" clip-rule="evenodd"/></svg>"""
            }
            for child in children do child
            if config.isMultiple then output { _role "status"; _attr("data-fve-select-announcement", ""); _class "sr-only"; $"{initial.Length} selected" }
        }

    let private choices (children:HtmlElement list) = children |> List.collect (function :? SelectPart as part -> part.Choices | _ -> [])
    let private itemChoice attributes (children:HtmlElement list) =
        let value = match ComponentElement.value "value" attributes with Some value when not(isNull value) -> value | _ -> invalidArg "attributes" "A Select item requires a string value."
        let label =
            match ComponentElement.value "label" attributes with
            | Some label when not(String.IsNullOrWhiteSpace label) -> label
            | _ ->
                if children |> List.exists (function :? TextElement | :? NoopElement -> false | _ -> true) then invalidArg "attributes" "Rich Select items require a plain label attribute for search and selected text."
                let text = System.Net.WebUtility.HtmlDecode(Render.toString(ComponentContent.fragment children))
                if String.IsNullOrWhiteSpace text then invalidArg "children" "A Select item needs text or an explicit label."
                text
        { value = value; label = label; disabled = flag "disabled" attributes }

    let private ownedChildren owner (children:HtmlElement list) = children |> List.map (function :? SelectPart as part -> part.RenderOwned owner | child -> child)
    let item attributes children =
        let choice = itemChoice attributes children
        SelectPart("item", [choice], fun owner ->
            let attributes = match owner, ComponentElement.attribute "id" attributes with Some owner, None -> _id(owner + "-option-" + ComponentHtml.optionToken choice.value) :: attributes | _ -> attributes
            renderItem choice (ComponentHtml.safeAttributes ["value"; "label"] attributes) children) :> HtmlElement
    let selectedItem attributes children = SelectPart("selected", [itemChoice attributes children], fun _ -> empty) :> HtmlElement
    let content attributes children =
        let items = choices children
        ChoiceSelection.validateKeys _.value items
        let optional name = ComponentElement.value name attributes |> Option.filter(String.IsNullOrWhiteSpace >> not)
        SelectPart("content", items, fun owner ->
            let attributes = match owner, ComponentElement.attribute "id" attributes with Some owner, None -> _id(owner + "-popup") :: attributes | _ -> attributes
            renderContent owner (ComponentHtml.safeAttributes ["query"; "loading"; "error"; "retry"; "empty-message"; "loading-message"] attributes) (ownedChildren owner children)
                (optional "query" |> Option.defaultValue "") (flag "loading" attributes) (optional "error") (optional "retry")
                (optional "empty-message" |> Option.defaultValue "No matching options") (optional "loading-message" |> Option.defaultValue "Loading options")) :> HtmlElement
    let group attributes children =
        let label = ComponentElement.required "label" attributes
        SelectPart("group", choices children, fun owner ->
            ComponentElement.render "div" [_role "group"; _ariaLabel label; _class "mt-1 first:mt-0"] (ComponentHtml.safeAttributes ["label"] attributes)
                [ div { _ariaHidden true; _class "px-2 py-1 text-xs font-semibold text-[var(--fve-muted-text)]"; label }; yield! ownedChildren owner children ]) :> HtmlElement

    let root isMultiple attributes (children:HtmlElement list) =
        let attributes = ComponentElement.attributes attributes
        let name = ComponentElement.required "name" attributes
        let controlId = ComponentElement.value "id" attributes |> Option.defaultValue name
        if String.IsNullOrWhiteSpace controlId || controlId |> Seq.exists Char.IsWhiteSpace then invalidArg "attributes" "A stable Select ID without whitespace is required."
        let contentParts = children |> List.choose (function :? SelectPart as part when part.Kind = "content" -> Some part | _ -> None)
        if contentParts.Length <> 1 then invalidArg "children" "Select requires exactly one fveSelectContent."
        let items = contentParts.Head.Choices
        let selectedItems = children |> List.collect (function :? SelectPart as part when part.Kind = "selected" -> part.Choices | _ -> [])
        ChoiceSelection.validateKeys _.value items
        ChoiceSelection.validateKeys _.value selectedItems
        if (ComponentElement.attribute (if isMultiple then "value" else "values") attributes).IsSome then invalidArg "attributes" "Use value for Select and values for MultiSelect."
        let selection =
            if isMultiple then
                match ComponentElement.value "values" attributes with
                | None -> []
                | Some json ->
                    try
                        let values = System.Text.Json.JsonSerializer.Deserialize<string array>(json)
                        if isNull values || values |> Array.exists isNull then invalidArg "attributes" "Selected values cannot be null."
                        values |> Array.toList |> List.distinct
                    with :? System.Text.Json.JsonException -> invalidArg "attributes" "MultiSelect values must be a JSON array of strings."
            else ComponentElement.value "value" attributes |> Option.toList
        let allItems = items @ (selectedItems |> List.filter(fun selected -> items |> List.exists(fun item -> item.value = selected.value) |> not))
        let selected = selection |> List.map(fun value -> allItems |> List.tryFind(fun item -> item.value = value) |> Option.defaultWith(fun () -> invalidArg "attributes" "Every selected value needs an item or fveSelectSelectedItem label."))
        let search = ComponentElement.value "search" attributes |> Option.map(function "static" -> SelectSearch.Static | endpoint when not(String.IsNullOrWhiteSpace endpoint) -> SelectSearch.Remote endpoint | _ -> invalidArg "attributes" "A search mode or endpoint is required.")
        let nativeFallback = flag "native-fallback" attributes
        if nativeFallback && (isMultiple || search.IsSome) then invalidArg "attributes" "Native fallback is available only for ordinary single selects."
        let config =
            { isMultiple = isMultiple; selectedMany = selected; search = search; query = ComponentElement.value "query" attributes
              name = name; id = Some controlId; options = allItems; selected = if isMultiple then None else selection |> List.tryHead
              label = ComponentElement.value "aria-label" attributes |> Option.defaultValue ""; nativeFallback = nativeFallback
              placeholder = ComponentElement.value "placeholder" attributes; invalid = flag "aria-invalid" attributes
              isRequired = flag "required" attributes; isDisabled = flag "disabled" attributes; isPending = flag "aria-busy" attributes; isReadOnly = flag "readonly" attributes
              size = ComponentElement.size None attributes; position = match ComponentElement.choice "position" "selected-item" ["selected-item"; "trigger-start"; "trigger-end"] attributes with "trigger-start" -> SelectPosition.TriggerStart | "trigger-end" -> SelectPosition.TriggerEnd | _ -> SelectPosition.SelectedItem
              controlClass = ComponentContent.classes attributes
              attributes = ComponentHtml.safeAttributes ["class"; "size"; "search"; "position"; "native-fallback"; "query"; "values"; "value"; "readonly"; "placeholder"] attributes }
        let body = children |> List.filter(function :? SelectPart as part when part.Kind = "selected" -> false | _ -> true)
        renderRoot (ownedChildren (Some("fve-select-" + ComponentHtml.signalToken controlId)) body) config

/// <category>select</category>
[<AutoOpen>]
module SelectDsl =
    /// Single string-valued form control. Domain values are serialized by the application.
    let fveSelect = ComponentElementBuilder(Select.root false)
    /// Repeated-name form values. Selected values use _values.
    let fveMultiSelect = ComponentElementBuilder(Select.root true)
    let fveSelectContent = ComponentElementBuilder(Select.content)
    let fveSelectItem = ComponentElementBuilder(Select.item)
    let fveSelectGroup = ComponentElementBuilder(Select.group)
    /// Selected labels omitted from a remote result page. Does not render an option.
    let fveSelectSelectedItem = ComponentElementBuilder(Select.selectedItem)
