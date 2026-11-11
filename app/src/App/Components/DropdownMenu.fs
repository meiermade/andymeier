namespace App.Components

open System
open FSharp.ViewEngine
open type Html
open type Datastar

/// <category>dropdown-menu</category>
[<RequireQualifiedAccess>]
type DropdownMenuAlignment =
    | Start
    | End

/// <category>dropdown-menu</category>
[<NoEquality; NoComparison>]
type internal DropdownMenuTrigger =
    private
        { control: ComponentControlElement
          attributes: HtmlAttribute list }

/// <category>dropdown-menu</category>
[<RequireQualifiedAccess>]
module internal DropdownMenuTrigger =
    let create attributes children : DropdownMenuTrigger =
        let control, attributes =
            ComponentTrigger.control "fveDropdownMenuTrigger" attributes children

        { control = control
          attributes = attributes }

[<NoEquality; NoComparison>]
type private MenuItemContent =
    { label: string
      leading: HtmlElement option
      trailing: HtmlElement option
      description: HtmlElement option
      shortcut: HtmlElement option
      disabled: bool
      readOnly: bool
      pending: bool
      color: ColorPalette
      body: HtmlElement option
      attributes: HtmlAttribute list
      clickName: string }

/// <category>dropdown-menu</category>
[<NoEquality; NoComparison>]
type internal DropdownMenuItem<'destination> =
    private
    | Link of content: MenuItemContent * destination: 'destination
    | Action of content: MenuItemContent * datastarExpression: string
    | Radio of
        content: MenuItemContent *
        datastarExpression: string *
        isChecked: bool *
        checkedExpression: string option
    | Checkbox of
        content: MenuItemContent *
        datastarExpression: string *
        isChecked: bool *
        checkedExpression: string option
    | Separator of attributes: HtmlAttribute list
    | Group of label: string * items: DropdownMenuItem<'destination> list * attributes: HtmlAttribute list

/// <category>dropdown-menu</category>
[<NoEquality; NoComparison>]
type internal DropdownMenuConfig<'destination> =
    private
        { id: string
          label: string
          items: DropdownMenuItem<'destination> list
          alignment: DropdownMenuAlignment
          trigger: DropdownMenuTrigger option
          containerClass: string option
          attributes: HtmlAttribute list }

type internal DropdownMenuItemElement(item: DropdownMenuItem<string>) =
    inherit HtmlElement()
    member _.Item = item

    override _.Render _ =
        invalidOp "Dropdown-menu items belong inside fveDropdownMenu or fveDropdownMenuGroup."

type internal DropdownMenuTriggerElement(trigger: DropdownMenuTrigger) =
    inherit HtmlElement()
    member _.Trigger = trigger

    override _.Render _ =
        invalidOp "fveDropdownMenuTrigger belongs inside fveDropdownMenu."

/// <category>dropdown-menu</category>
[<RequireQualifiedAccess>]
module internal DropdownMenuItem =
    let private content label =
        if String.IsNullOrWhiteSpace label then
            invalidArg (nameof label) "A menu item label is required."

        { label = label
          leading = None
          trailing = None
          description = None
          shortcut = None
          disabled = false
          readOnly = false
          pending = false
          color = ComponentColors.neutral
          body = None
          attributes = []
          clickName = "data-on:click" }

    let flag name attributes =
        match ComponentElement.attribute name attributes with
        | Some { Value = ValueNone } -> true
        | Some { Value = ValueSome value } -> value <> "false"
        | None -> false

    let elements (children: HtmlElement list) =
        children
        |> List.choose (function
            | :? DropdownMenuItemElement as element -> Some element.Item
            | :? NoopElement -> None
            | _ -> invalidArg "children" "Use dropdown-menu items, groups and separators.")

    let element authored children =
        let attributes = ComponentElement.attributes authored
        let parts, children = ComponentParts.split "DropdownMenuItem" children

        let label =
            match ComponentElement.value "label" attributes with
            | Some label when not (String.IsNullOrWhiteSpace label) -> label
            | Some _ -> invalidArg "label" "A menu-item label is required."
            | None -> ComponentContent.plainText "Rich dropdown-menu items without _label" children

        let defaults = content label
        let click = ComponentElement.attribute "data-on:click" attributes

        let description =
            match ComponentElement.value "description" attributes, Map.tryFind "description" parts with
            | Some _, Some _ -> invalidArg "description" "Supply one menu-item description."
            | Some text, None when not (String.IsNullOrWhiteSpace text) -> Some(Html.text text)
            | Some _, None -> invalidArg "description" "A visible description is required."
            | None, description -> description

        let content =
            { defaults with
                leading = Map.tryFind "leading" parts
                trailing = Map.tryFind "trailing" parts
                description = description
                shortcut = Map.tryFind "shortcut" parts
                body =
                    Some(
                        if children.IsEmpty then
                            Html.text label
                        else
                            ComponentContent.fragment children
                    )
                disabled = flag "disabled" attributes
                readOnly = flag "readonly" attributes || flag "aria-disabled" attributes
                pending = flag "aria-busy" attributes
                color = ComponentElement.palette attributes
                attributes = ComponentHtml.safeAttributes [ "label"; "description"; "color"; "palette" ] attributes
                clickName = click |> Option.map _.Name |> Option.defaultValue "data-on:click" }

        let action =
            click
            |> Option.bind (fun attribute -> ValueOption.toOption attribute.Value)
            |> Option.defaultValue "void 0"

        let checked' =
            ComponentElement.choice "aria-checked" "false" [ "true"; "false" ] attributes = "true"

        let role =
            ComponentElement.choice "role" "menuitem" [ "menuitem"; "menuitemradio"; "menuitemcheckbox" ] attributes

        if
            role = "menuitem"
            && ((ComponentElement.attribute "aria-checked" attributes |> Option.isSome)
                || (ComponentElement.attribute "data-attr:aria-checked" attributes |> Option.isSome))
        then
            invalidArg "aria-checked" "Only radio and checkbox menu items have a checked state."

        ComponentElement.choice "type" "button" [ "button"; "submit"; "reset" ] attributes
        |> ignore

        let item =
            match ComponentElement.value "href" attributes, role with
            | Some href, "menuitem" -> Link(content, href)
            | Some _, _ -> invalidArg "href" "Radio and checkbox menu items use native buttons, not destinations."
            | None, "menuitemradio" -> Radio(content, action, checked', None)
            | None, "menuitemcheckbox" -> Checkbox(content, action, checked', None)
            | _ -> Action(content, action)

        DropdownMenuItemElement(item) :> HtmlElement

    let groupElement attributes children =
        let label = ComponentElement.required "label" attributes
        let items = elements children

        if List.isEmpty items then
            invalidArg (nameof children) "A menu group requires at least one item."

        if
            items
            |> List.exists (function
                | Group _ -> true
                | _ -> false)
        then
            invalidArg (nameof children) "Menu groups cannot be nested."

        DropdownMenuItemElement(Group(label, items, ComponentHtml.safeAttributes [ "label" ] attributes)) :> HtmlElement

    let separatorElement attributes children =
        ComponentContent.noChildren "fveDropdownMenuSeparator" children
        DropdownMenuItemElement(Separator attributes) :> HtmlElement

/// <category>dropdown-menu</category>
[<RequireQualifiedAccess>]
module internal DropdownMenu =
    let internal empty: DropdownMenuConfig<'destination> =
        { id = ""
          label = ""
          items = []
          alignment = DropdownMenuAlignment.End
          trigger = None
          containerClass = None
          attributes = [] }

    let withTrigger trigger (config: DropdownMenuConfig<'destination>) = { config with trigger = Some trigger }

    let render resolve config =
        let trigger =
            config.trigger
            |> Option.defaultWith (fun () -> invalidOp "DropdownMenu.withTrigger is required before rendering.")

        if List.isEmpty config.items then
            invalidOp "DropdownMenu.withContent is required before rendering."

        let readOnly =
            DropdownMenuItem.flag "readonly" trigger.attributes
            || DropdownMenuItem.flag "aria-disabled" trigger.attributes

        let disabled = DropdownMenuItem.flag "disabled" trigger.attributes

        let triggerId =
            ComponentElement.value "id" trigger.attributes
            |> Option.defaultValue $"{config.id}-trigger"

        if
            String.IsNullOrWhiteSpace triggerId
            || (triggerId |> Seq.exists Char.IsWhiteSpace)
        then
            invalidArg "id" "A menu-trigger ID must be stable and contain no whitespace."

        let menuId = $"{config.id}-menu"

        let positionArea =
            match config.alignment with
            | DropdownMenuAlignment.Start -> "block-end span-inline-end"
            | DropdownMenuAlignment.End -> "block-end span-inline-start"

        let itemContent (content: MenuItemContent) =
            [ if content.pending then
                  ComponentHtml.loadingGlyph Size.Small
              else
                  match content.leading with
                  | Some leading ->
                      span {
                          _ariaHidden true
                          _class "flex size-4 shrink-0 items-center justify-center"
                          leading
                      }
                  | None -> ()
              span {
                  _class "min-w-0 flex-1"

                  span {
                      _class "block truncate"
                      content.body |> Option.defaultWith (fun () -> Html.text content.label)
                  }

                  match content.description with
                  | Some description ->
                      span {
                          _class "block truncate text-xs text-[var(--fve-muted-text)]"
                          description
                      }
                  | None -> ()
              }
              match content.trailing with
              | Some trailing ->
                  span {
                      _ariaHidden true
                      _class "ml-auto flex size-4 shrink-0 items-center justify-center"
                      trailing
                  }
              | None -> ()
              match content.shortcut with
              | Some shortcut ->
                  kbd {
                      _ariaHidden true

                      _class
                          "ml-auto shrink-0 rounded-[var(--fve-radius-control)] bg-[var(--fve-surface-subtle)] px-2 py-1 text-xs font-semibold text-[var(--fve-muted-text)]"

                      shortcut
                  }
              | None -> () ]

        let itemClasses unavailable =
            ComponentHtml.classes
                [ ComponentHtml.popupItemClasses
                  "group fve-popup-item flex w-full items-center gap-3 rounded-none px-3 py-[var(--fve-control-padding-block)] text-left text-[length:var(--fve-control-font-size)] leading-[var(--fve-control-line-height)] font-normal text-[var(--fve-color-text)]"
                  if unavailable then
                      "cursor-not-allowed opacity-50" ]

        let nativeAttributes (content: MenuItemContent) =
            ComponentHtml.safeAttributes
                [ "id"
                  "class"
                  "style"
                  "type"
                  "role"
                  "href"
                  "disabled"
                  "readonly"
                  "aria-disabled"
                  "aria-busy"
                  "aria-checked"
                  "data-on:click" ]
                content.attributes
            |> List.filter (fun attribute ->
                not (content.disabled || content.readOnly)
                || not (
                    attribute.Name.StartsWith("data-on:click", StringComparison.OrdinalIgnoreCase)
                    || attribute.Name.StartsWith("data-on:keydown", StringComparison.OrdinalIgnoreCase)
                ))

        let rec renderEntry path item =
            let authoredId =
                match item with
                | Link(content, _)
                | Action(content, _)
                | Radio(content, _, _, _)
                | Checkbox(content, _, _, _) -> ComponentElement.value "id" content.attributes
                | _ -> None

            let entryId = authoredId |> Option.defaultValue $"{menuId}-entry-{path}"

            match item with
            | Link(content, destination) ->
                let unavailable = content.disabled || content.readOnly

                a {
                    _id entryId

                    if unavailable |> not then
                        _href (resolve destination)

                    _role "menuitem"
                    _tabindex -1
                    _ariaDisabled unavailable

                    if content.pending then
                        _ariaBusy true

                    _attr ("data-fve-menu-label", content.label.ToLowerInvariant())

                    if unavailable |> not then
                        let action =
                            ComponentElement.value content.clickName content.attributes
                            |> Option.defaultValue "void 0"

                        _attr (content.clickName, action)

                    _style (ComponentColors.style content.color content.attributes)

                    _class (
                        ComponentHtml.classes [ itemClasses unavailable; ComponentContent.classes content.attributes ]
                    )

                    for attribute in nativeAttributes content do
                        attribute

                    itemContent content
                }
            | Action(content, expression)
            | Radio(content, expression, _, _)
            | Checkbox(content, expression, _, _) ->
                let unavailable = content.disabled || content.readOnly

                button {
                    _id entryId
                    _type (ComponentElement.value "type" content.attributes |> Option.defaultValue "button")

                    match item with
                    | Radio(_, _, isChecked, checkedExpression) ->
                        _role "menuitemradio"
                        _ariaChecked isChecked

                        match checkedExpression with
                        | Some expression -> _dataAttr ("aria-checked", $"({expression}) ? 'true' : 'false'")
                        | None -> ()
                    | Checkbox(_, _, isChecked, checkedExpression) ->
                        _role "menuitemcheckbox"
                        _ariaChecked isChecked

                        match checkedExpression with
                        | Some expression -> _dataAttr ("aria-checked", $"({expression}) ? 'true' : 'false'")
                        | None -> ()
                    | _ -> _role "menuitem"

                    _tabindex -1
                    _disabled content.disabled
                    _ariaDisabled unavailable

                    if content.pending then
                        _ariaBusy true

                    _attr ("data-fve-menu-label", content.label.ToLowerInvariant())

                    if unavailable |> not then
                        _attr (content.clickName, expression)

                    _style (ComponentColors.style content.color content.attributes)

                    _class (
                        ComponentHtml.classes [ itemClasses unavailable; ComponentContent.classes content.attributes ]
                    )

                    for attribute in nativeAttributes content do
                        attribute

                    itemContent content

                    match item with
                    | Radio(_, _, isChecked, checkedExpression)
                    | Checkbox(_, _, isChecked, checkedExpression) ->
                        span {
                            _ariaHidden true
                            _class "ml-auto size-4 shrink-0 invisible group-aria-checked:visible"

                            raw
                                """<svg viewBox="0 0 20 20" fill="currentColor" aria-hidden="true"><path fill-rule="evenodd" d="M16.704 4.153a.75.75 0 0 1 .143 1.051l-8 10.5a.75.75 0 0 1-1.127.075l-4.5-4.5a.75.75 0 0 1 1.06-1.06l3.894 3.893 7.479-9.816a.75.75 0 0 1 1.051-.143Z" clip-rule="evenodd"/></svg>"""
                        }
                    | _ -> ()
                }
            | Separator attributes ->
                ComponentElement.render
                    "div"
                    [ _id entryId; _role "separator"; _class "my-1 h-px bg-[var(--fve-border)]" ]
                    (ComponentHtml.safeAttributes [ "role" ] attributes)
                    []
            | Group(label, items, attributes) ->
                let labelId = $"{entryId}-label"

                ComponentElement.render
                    "div"
                    [ _id entryId; _role "group"; _ariaLabelledby labelId ]
                    (ComponentHtml.safeAttributes [ "role"; "aria-labelledby" ] attributes)
                    [ yield
                          div {
                              _id labelId

                              _class
                                  "px-3 py-1 text-xs font-semibold uppercase tracking-wide text-[var(--fve-muted-text)]"

                              label
                          }
                      for index, child in items |> List.indexed do
                          yield renderEntry $"{path}-{index}" child ]

        (Html.el "fve-dropdown-menu") {
            _class (ComponentHtml.classes [ "relative inline-flex"; config.containerClass |> Option.defaultValue "" ])
            _attr ("alignment", if config.alignment = DropdownMenuAlignment.Start then "start" else "end")

            for attribute in ComponentHtml.safeAttributes [ "class"; "alignment" ] config.attributes do
                attribute

            ComponentTrigger.render
                trigger.control
                [ yield!
                      trigger.attributes
                      |> ComponentHtml.safeAttributes
                          [ "id"
                            "type"
                            "role"
                            "popovertarget"
                            "aria-haspopup"
                            "aria-expanded"
                            "aria-controls"
                            "aria-label"
                            "aria-disabled"
                            "readonly"
                            "data-attr:aria-expanded" ]
                  yield _id triggerId
                  yield _type "button"
                  yield _popovertarget menuId
                  yield _ariaHaspopup "menu"
                  yield _ariaExpanded false
                  if not (String.IsNullOrWhiteSpace config.label) then
                      yield _ariaLabel config.label
                  yield _ariaControls menuId
                  yield _disabled disabled
                  yield _ariaDisabled (disabled || readOnly)
                  ]

            div {
                _id menuId
                _popover "auto"
                _role "menu"
                _tabindex -1
                _ariaLabelledby triggerId
                _attr ("data-fve-position-area", positionArea)

                _style
                    $"inset: auto; margin: 0.5rem 0; position-area: {positionArea}; position-try-fallbacks: flip-block, flip-inline, flip-block flip-inline; width: min(16rem, calc(100vw - 2rem))"

                _class (
                    ComponentHtml.classes
                        [ ComponentHtml.popupClasses
                          "fve-popup fixed z-30 overflow-hidden rounded-[var(--fve-radius-control)] py-1" ]
                )

                for index, item in config.items |> List.indexed do
                    renderEntry (string index) item
            }
        }

    let internal dslAttributes attributes (config: DropdownMenuConfig<'destination>) =
        let config =
            (config, attributes)
            ||> List.fold (fun config attribute ->
                match attribute.Name, attribute.Value with
                | "id", ValueSome id ->
                    if String.IsNullOrWhiteSpace id || (id |> Seq.exists Char.IsWhiteSpace) then
                        invalidArg (nameof attributes) "A stable menu ID without whitespace is required."

                    { config with id = id }
                | "class", ValueSome value ->
                    { config with
                        containerClass =
                            Some(ComponentHtml.classes [ config.containerClass |> Option.defaultValue ""; value ]) }
                | _ -> config)

        { config with
            attributes = config.attributes @ attributes }

type internal DropdownMenuElement(config: DropdownMenuConfig<string>) =
    inherit HtmlElement()
    member _.Config = config

    override _.Render output =
        (DropdownMenu.render id config).Render output

/// <category>dropdown-menu</category>
[<AutoOpen>]
module DropdownMenuAttributes =
    type Html with
        static member _alignment(value: DropdownMenuAlignment) =
            Html._attr (
                "alignment",
                if value = DropdownMenuAlignment.Start then
                    "start"
                else
                    "end"
            )

module internal DropdownMenuElements =
    let trigger authored children =
        DropdownMenuTriggerElement(DropdownMenuTrigger.create authored children) :> HtmlElement

    let render authored (children: HtmlElement list) =
        let attributes = ComponentElement.attributes authored
        let id = ComponentElement.required "id" attributes

        if id |> Seq.exists Char.IsWhiteSpace then
            invalidArg "id" "A stable menu ID without whitespace is required."

        let triggers, items =
            children
            |> List.fold
                (fun (triggers, items) child ->
                    match child with
                    | :? DropdownMenuTriggerElement as element -> element.Trigger :: triggers, items
                    | :? DropdownMenuItemElement as element -> triggers, element.Item :: items
                    | :? NoopElement -> triggers, items
                    | _ ->
                        invalidArg
                            "children"
                            "Use fveDropdownMenuTrigger and dropdown-menu items, groups or separators.")
                ([], [])

        let trigger =
            match triggers with
            | [ trigger ] -> trigger
            | _ -> invalidArg "children" "Exactly one fveDropdownMenuTrigger is required."

        if items.IsEmpty then
            invalidArg "children" "A dropdown menu requires at least one item."

        let label =
            ComponentElement.value "aria-label" trigger.attributes
            |> Option.orElse (ComponentElement.value "aria-label" attributes)

        match label with
        | Some text when String.IsNullOrWhiteSpace text ->
            invalidArg "aria-label" "A supplied menu label cannot be empty."
        | _ -> ()

        let labelledby =
            ComponentElement.value "aria-labelledby" trigger.attributes
            |> Option.orElse (ComponentElement.value "aria-labelledby" attributes)

        let triggerAttributes =
            ComponentHtml.safeAttributes [ "aria-label"; "aria-labelledby" ] trigger.attributes
            @ (labelledby |> Option.map Html._ariaLabelledby |> Option.toList)

        let config =
            { DropdownMenu.empty with
                id = id
                label = label |> Option.defaultValue ""
                trigger =
                    Some
                        { trigger with
                            attributes = triggerAttributes }
                items = List.rev items
                alignment =
                    if ComponentElement.choice "alignment" "end" [ "start"; "end" ] attributes = "start" then
                        DropdownMenuAlignment.Start
                    else
                        DropdownMenuAlignment.End }
            |> DropdownMenu.dslAttributes (
                ComponentHtml.safeAttributes [ "aria-label"; "aria-labelledby"; "alignment" ] attributes
            )

        DropdownMenuElement(config) :> HtmlElement

    let part role attributes children =
        let content =
            if List.isEmpty attributes then
                ComponentContent.fragment children
            else
                ComponentElement.render "span" [] attributes children

        ComponentPart("DropdownMenuItem", role, content) :> HtmlElement

/// <category>dropdown-menu</category>
[<AutoOpen>]
module DropdownMenuDsl =
    let fveDropdownMenu = ComponentElementBuilder(DropdownMenuElements.render)
    let fveDropdownMenuTrigger = ComponentTriggerBuilder(DropdownMenuElements.trigger)
    let fveDropdownMenuItem = ComponentElementBuilder(DropdownMenuItem.element)
    let fveDropdownMenuGroup = ComponentElementBuilder(DropdownMenuItem.groupElement)

    let fveDropdownMenuSeparator =
        ComponentElementBuilder(DropdownMenuItem.separatorElement)

    let fveDropdownMenuItemLeading =
        ComponentElementBuilder(DropdownMenuElements.part "leading")

    let fveDropdownMenuItemTrailing =
        ComponentElementBuilder(DropdownMenuElements.part "trailing")

    let fveDropdownMenuItemDescription =
        ComponentElementBuilder(DropdownMenuElements.part "description")

    let fveDropdownMenuItemShortcut =
        ComponentElementBuilder(DropdownMenuElements.part "shortcut")
