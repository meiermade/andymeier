namespace App.Components

open FSharp.ViewEngine
open type Html

/// <category>input</category>
[<RequireQualifiedAccess>]
type InputType = Text | Email | Telephone | Password | Number | Search | Url | Date | Time | DateTimeLocal

/// <category>input</category>
[<AutoOpen>]
module InputAttributes =
    type Html with
        static member _type(value:InputType) =
            Html._type (match value with InputType.Text -> "text" | InputType.Email -> "email" | InputType.Telephone -> "tel" | InputType.Password -> "password" | InputType.Number -> "number" | InputType.Search -> "search" | InputType.Url -> "url" | InputType.Date -> "date" | InputType.Time -> "time" | InputType.DateTimeLocal -> "datetime-local")

module internal Input =
    let render attributes children =
        ComponentContent.noChildren "fveInput" children
        let attributes = ComponentElement.attributes attributes
        let kind = ComponentElement.choice "type" "text" ["text"; "email"; "tel"; "password"; "number"; "search"; "url"; "date"; "time"; "datetime-local"] attributes
        input {
            for attribute in TextField.attributes
                [_type kind; _attr("data-fve-input-control", ""); _class (TextField.classes attributes)]
                attributes do
                attribute
        }

/// <category>input</category>
[<AutoOpen>]
module InputDsl =
    /// A native input only. Use InputGroup for decorations and Field for labels and feedback.
    let fveInput = ComponentElementBuilder(Input.render)
