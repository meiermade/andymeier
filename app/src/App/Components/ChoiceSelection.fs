namespace App.Components

[<NoEquality; NoComparison>]
type internal SelectedChoice = { value:string; label:string }

module internal ChoiceSelection =
    let validateKeys encode options =
        let keys = options |> List.map encode
        if keys |> List.exists isNull then invalidArg (nameof options) "Encoded choice values cannot be null."
        if (keys |> List.distinct |> List.length) <> keys.Length then
            invalidArg (nameof options) "Encoded choice values must be unique."
