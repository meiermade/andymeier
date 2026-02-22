namespace App

open System

type SeqConfig =
    { endpoint:string
      apiKey:string }

module SeqConfig =
    let load () =
        { endpoint = Env.variableOrDefault "SEQ_ENDPOINT" "http://localhost:5341"
          apiKey = Env.variableOrDefault "SEQ_API_KEY" "" }

type ServerConfig =
    { url:string }

module ServerConfig =
    let load () =
        { url = Env.variableOrDefault "SERVER_URL" "http://localhost:5000" }

type Config =
    { debug:bool
      appName:string
      server:ServerConfig
      seq:SeqConfig }

module Config =
    let load () =
        { debug = Env.variableOrDefault "DEBUG" "false" |> Boolean.Parse
          appName = "AndrewMeier"
          server = ServerConfig.load ()
          seq = SeqConfig.load () }
