module Domain.Sqlite

open Microsoft.Data.Sqlite
open System
open System.IO

type Config =
    { path: string }

module Config =
    let load () =
        { path = Env.variable "SQLITE_PATH" }

let connectionString (config: Config) =
    let directory = Path.GetDirectoryName(config.path)

    if not (String.IsNullOrWhiteSpace(directory)) then
        Directory.CreateDirectory(directory) |> ignore

    $"Data Source={config.path};Cache=Shared"


type SqliteDataReader with
    member private this.Read<'T>(col: string) = this.GetFieldValue<'T>(this.GetOrdinal(col))
    member private this.ReadOption<'T>(col: string) =
        let idx = this.GetOrdinal(col)
        if this.IsDBNull(idx) then None else Some(this.GetFieldValue<'T>(idx))

    member this.string(col: string) = this.Read<string>(col)
    member this.stringOption(col: string) = this.ReadOption<string>(col)
    member this.dateTimeOffset(col: string) = this.string(col) |> DateTimeOffset.Parse


type SqliteValue =
    | Text of string
    | TextOption of string option


type SqliteParameter = string * SqliteValue

type SqliteCommand =
    { sql: string option
      parameters: SqliteParameter list
      conn: SqliteConnection option }

module SqliteParameter =
    let toSqliteParameter ((name, value): SqliteParameter) : Microsoft.Data.Sqlite.SqliteParameter =
        let p = Microsoft.Data.Sqlite.SqliteParameter()
        p.ParameterName <- name

        match value with
        | Text text -> p.Value <- text
        | TextOption value ->
            match value with
            | Some text -> p.Value <- text
            | None -> p.Value <- DBNull.Value

        p

module SqliteCommand =
    let toSqliteCommand (command: SqliteCommand) =
        match command.conn, command.sql with
        | Some conn, Some sql ->
            let sqliteCommand = new Microsoft.Data.Sqlite.SqliteCommand(sql, conn)

            command.parameters
            |> List.iter (fun p ->
                let sqliteParameter = SqliteParameter.toSqliteParameter p
                sqliteCommand.Parameters.Add(sqliteParameter) |> ignore)

            sqliteCommand
        | None, _ -> failwith "No connection provided"
        | _, None -> failwith "No sql provided"

module Sql =
    let text (value: string) = value |> SqliteValue.Text
    let textOption (value: string option) = value |> SqliteValue.TextOption
    let timestamptz (value: DateTimeOffset) = value.ToString("O") |> SqliteValue.Text

    let connect (conn: SqliteConnection) =
        { sql = None
          parameters = []
          conn = Some conn }

    let sql (sql: string) (command: SqliteCommand) =
        { command with sql = Some sql }

    let parameter (parameter: SqliteParameter) (command: SqliteCommand) =
        { command with parameters = parameter :: command.parameters }

    let parameters (parameters: SqliteParameter list) (command: SqliteCommand) =
        { command with parameters = parameters @ command.parameters }

    let executeNonQuery (command: SqliteCommand) =
        task {
            use sqliteCommand = SqliteCommand.toSqliteCommand command
            let! _ = sqliteCommand.ExecuteNonQueryAsync()
            return ()
        }

    let executeQuery (read: SqliteDataReader -> 'T) (command: SqliteCommand) =
        task {
            use sqliteCommand = SqliteCommand.toSqliteCommand command
            use! reader = sqliteCommand.ExecuteReaderAsync()
            return [ while reader.Read() do read reader ]
        }
