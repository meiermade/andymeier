module Domain.Redis

open StackExchange.Redis
open System
open System.Threading.Tasks

type Config =
    { connectionString:string }

module Config =
    let load () =
        { connectionString = Env.variableOrDefault "REDIS_CONNECTION_STRING" "localhost:6379,abortConnect=false" }

type TryGetValue = string -> Task<string option>
type SetValue = string * string -> Task<unit>
type SetValueExp = string * string * TimeSpan -> Task<unit>
type Remove = string -> Task<unit>

type Service =
    { tryGetValue: TryGetValue
      setValue: SetValue
      setValueExp: SetValueExp
      remove: Remove }

module Service =

    let private tryGetValue (telemetry: Telemetry.Service) (conn: ConnectionMultiplexer) : TryGetValue =
        fun (key: string) ->
            task {
                use span = telemetry.startActiveSpan "domain.redis.try_get_value"
                span.SetAttribute("key", key) |> ignore
                let db = conn.GetDatabase()
                let! cached = db.StringGetAsync(RedisKey.op_Implicit key)

                if cached.HasValue then
                    return Some(cached.ToString())
                else
                    return None
            }

    let private setValue (telemetry: Telemetry.Service) (conn: ConnectionMultiplexer) : SetValue =
        fun (key, value) ->
            task {
                use span = telemetry.startActiveSpan "domain.redis.set_value"
                span.SetAttribute("key", key) |> ignore
                let db = conn.GetDatabase()
                do! db.StringSetAsync(RedisKey.op_Implicit key, RedisValue.op_Implicit value) |> Task.Ignore
            }

    let private setValueExp (telemetry: Telemetry.Service) (conn: ConnectionMultiplexer) : SetValueExp =
        fun (key, value, expiration) ->
            task {
                use span = telemetry.startActiveSpan "domain.redis.set_value_exp"
                span.SetAttribute("key", key) |> ignore
                let db = conn.GetDatabase()

                do!
                    db.StringSetAsync(RedisKey.op_Implicit key, RedisValue.op_Implicit value, expiration)
                    |> Task.Ignore
            }

    let private remove (telemetry: Telemetry.Service) (conn: ConnectionMultiplexer) : Remove =
        fun (key: string) ->
            task {
                use span = telemetry.startActiveSpan "domain.redis.remove"
                span.SetAttribute("key", key) |> ignore
                let db = conn.GetDatabase()
                do! db.KeyDeleteAsync(RedisKey.op_Implicit key) |> Task.Ignore
            }

    let create (config: Config) (telemetry: Telemetry.Service) =
        let conn = ConnectionMultiplexer.Connect(config.connectionString)

        { tryGetValue = tryGetValue telemetry conn
          setValue = setValue telemetry conn
          setValueExp = setValueExp telemetry conn
          remove = remove telemetry conn }
