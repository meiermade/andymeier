namespace App.Components

open FSharp.ViewEngine

/// <summary>Fresh attributes, children, and typed configuration updates for one expression.</summary>
/// <exclude/>
type ComponentState<'config> internal () =
    let attributes = ResizeArray<HtmlAttribute>()
    let children = ResizeArray<HtmlElement>()
    let options = ResizeArray<'config -> 'config>()
    member internal _.Attributes = attributes
    member internal _.Children = children
    member internal _.Options = options

/// <summary>Deferred component construction.</summary>
/// <exclude/>
type ComponentCode<'config> = ComponentState<'config> -> unit

/// <summary>Composition shared by fixed and inferred-type component builders.</summary>
/// <exclude/>
type ComponentExpressionBuilder() =
    member _.Yield(_: unit) : ComponentCode<'config> = ignore

    member _.Yield(attribute: HtmlAttribute) : ComponentCode<'config> =
        fun state ->
            if not (System.String.IsNullOrEmpty attribute.Name) then
                state.Attributes.Add attribute

    member _.Yield(child: HtmlElement) : ComponentCode<'config> = fun state -> state.Children.Add child

    member this.Yield(text: string) : ComponentCode<'config> = this.Yield(Html.text text)

    member _.Yield(children: HtmlElement seq) : ComponentCode<'config> =
        fun state -> state.Children.AddRange children

    member this.YieldFrom(children: HtmlElement seq) : ComponentCode<'config> = this.Yield children

    member _.YieldFrom(attributes: HtmlAttribute seq) : ComponentCode<'config> =
        fun state ->
            for attribute in attributes do
                if not (System.String.IsNullOrEmpty attribute.Name) then
                    state.Attributes.Add attribute

    member _.Zero() : ComponentCode<'config> = ignore

    member _.Combine(first: ComponentCode<'config>, second: ComponentCode<'config>) : ComponentCode<'config> =
        fun state ->
            first state
            second state

    member this.For(code: ComponentCode<'config>, body: unit -> ComponentCode<'config>) =
        this.Combine(code, this.Delay body)

    member _.Delay(body: unit -> ComponentCode<'config>) : ComponentCode<'config> = fun state -> body () state

    member _.For(items: 'item seq, body: 'item -> ComponentCode<'config>) : ComponentCode<'config> =
        fun state ->
            for item in items do
                body item state

    member internal _.Evaluate(code: ComponentCode<'config>, create, attributes, render) : HtmlElement =
        let state = ComponentState<'config>()
        code state

        let config =
            (create (List.ofSeq state.Children), state.Options)
            ||> Seq.fold (fun config option -> option config)

        config |> attributes (List.ofSeq state.Attributes) |> render

type internal ComponentControlElement
    (tagName: string, attributes: HtmlAttribute list, render: HtmlAttribute list -> HtmlElement, element: HtmlElement) =
    inherit HtmlElement()
    member _.TagName = tagName
    member _.Attributes = attributes
    member _.WithAttributes attributes = render attributes
    override _.Render(writer) = element.Render writer

type internal ComponentPart(owner: string, name: string, element: HtmlElement) =
    inherit HtmlElement()
    member _.Owner = owner
    member _.Name = name
    override _.Render(writer) = element.Render writer

module internal ComponentParts =
    let split owner (children: HtmlElement list) =
        let parts, body =
            ((Map.empty, []), children)
            ||> List.fold (fun (parts, body) child ->
                match child with
                | :? ComponentPart as part when part.Owner = owner ->
                    if Map.containsKey part.Name parts then
                        invalidArg "children" $"Only one {owner} {part.Name} is supported."

                    Map.add part.Name child parts, body
                | :? NoopElement -> parts, body
                | _ -> parts, child :: body)

        parts, List.rev body

/// <summary>Ordinary attributes and content for a server-rendered component.</summary>
/// <exclude/>
type ComponentElementBuilder internal (render: HtmlAttribute list -> HtmlElement list -> HtmlElement) =
    inherit ComponentExpressionBuilder()

    member _.Run(code: ComponentCode<unit>) =
        let state = ComponentState<unit>()
        code state
        render (List.ofSeq state.Attributes) (List.ofSeq state.Children)

/// <summary>A trigger accepts a composable control or an explicit native/custom renderer receiving its attributes.</summary>
/// <exclude/>
type ComponentTriggerBuilder internal (render: HtmlAttribute list -> HtmlElement list -> HtmlElement) =
    inherit ComponentElementBuilder(render)

    member _.Yield(renderer: HtmlAttribute list -> HtmlElement) : ComponentCode<unit> =
        fun state -> state.Children.Add(ComponentControlElement("button", [], renderer, NoopElement()))

module internal ComponentContent =
    let fragment children : HtmlElement = FragmentElement(children)

    let plainText componentName (children: HtmlElement list) =
        if
            children
            |> List.exists (function
                | :? TextElement
                | :? NoopElement -> false
                | _ -> true)
        then
            invalidArg (nameof children) $"{componentName} accepts plain text content, not HTML children."

        System.Net.WebUtility.HtmlDecode(Render.toString (fragment children))

    let noChildren componentName children =
        if not (List.isEmpty children) then
            invalidArg (nameof children) $"{componentName} does not accept child content."

    let classes (attributes: HtmlAttribute list) =
        attributes
        |> List.choose (fun attribute ->
            if attribute.Name = "class" then
                ValueOption.toOption attribute.Value
            else
                None)
        |> String.concat " "
