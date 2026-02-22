[<AutoOpen>]
module App.Infrastructure

open Giraffe
open Microsoft.AspNetCore.Http
open System
open System.Text.Json
open System.Text.Json.Serialization

module Env =
    let variable (key:string) =
        match Environment.GetEnvironmentVariable(key) with
        | value when String.IsNullOrEmpty(value) -> failwith $"Environment variable '{key}' is required"
        | value -> value
    let variableOrDefault (key:string) (defaultValue:string) =
        match Environment.GetEnvironmentVariable(key) with
        | value when String.IsNullOrEmpty(value) -> defaultValue
        | value -> value

module Json =
    let private options =
        JsonFSharpOptions.Default()
            .ToJsonSerializerOptions()

    let serialize<'T> (value: 'T) =
        JsonSerializer.Serialize(value, options)

    let deserialize<'T> (json: string) =
        JsonSerializer.Deserialize<'T>(json, options)

[<AutoOpen>]
module HttpContextExtensions =
    type HttpContext with
        member this.IsDatastar =
            match this.TryGetRequestHeader("Datastar-Request") with
            | Some "true" -> true
            | _ -> false
