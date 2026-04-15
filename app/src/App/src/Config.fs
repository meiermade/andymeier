namespace App

open Domain
open System

type SeqConfig =
    { endpoint:string }

module SeqConfig =
    let load () =
        { endpoint = Env.variableOrDefault "SEQ_ENDPOINT" "http://localhost:5341" }

type ServerConfig =
    { url:string }

module ServerConfig =
    let load () =
        { url = Env.variableOrDefault "SERVER_URL" "https://localhost:5000" }

type GoogleAnalyticsConfig =
    { measurementId:string }

module GoogleAnalyticsConfig =
    let load () =
        { measurementId = Env.variable "GOOGLE_ANALYTICS_MEASUREMENT_ID" }

type Config =
    { debug:bool
      appName:string
      server:ServerConfig
      seq:SeqConfig
      googleAnalytics:GoogleAnalyticsConfig
      sqlite:Sqlite.Config
      notion:Notion.Config }

module Config =
    let load () =
        { debug = Env.variableOrDefault "DEBUG" "false" |> Boolean.Parse
          appName = "andymeier"
          server = ServerConfig.load ()
          seq = SeqConfig.load ()
          googleAnalytics = GoogleAnalyticsConfig.load ()
          sqlite = Sqlite.Config.load ()
          notion = Notion.Config.load () }
