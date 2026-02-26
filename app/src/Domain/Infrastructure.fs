[<AutoOpen>]
module Domain.Infrastructure

open System
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading.Tasks

module Env =
    let variable (key:string) =
        match Environment.GetEnvironmentVariable(key) with
        | value when String.IsNullOrEmpty(value) -> failwith $"Environment variable '{key}' is required"
        | value -> value

    let variableOrDefault (key:string) (defaultValue:string) =
        match Environment.GetEnvironmentVariable(key) with
        | value when String.IsNullOrEmpty(value) -> defaultValue
        | value -> value

[<AutoOpen>]
module TaskExtensions =
    type Task with
        static member Ignore(t:Task) = task { let! _ = t in return () }

module Json =
    let private options =
        JsonFSharpOptions.Default()
            .ToJsonSerializerOptions()

    let serialize<'T> (value: 'T) =
        JsonSerializer.Serialize(value, options)

    let deserialize<'T> (json: string) =
        JsonSerializer.Deserialize<'T>(json, options)
