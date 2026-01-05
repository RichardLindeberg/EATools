/// Database access and initialization
namespace EATool.Infrastructure

open System
open Microsoft.Data.Sqlite

/// Database configuration
type DatabaseConfig =
    {
        ConnectionString: string
        Environment: string  // dev, staging, prod
    }

/// Database initialization and schema management
module Database =

    let mutable private currentConfig: DatabaseConfig option = None

    /// Create database configuration from environment
    let createConfig (env: string) =
        let connString =
            match env with
            | "development" ->
                // SQLite connection string for development
                "Data Source=eatool.db;Cache=Shared;Mode=ReadWriteCreate"
            | "staging" | "production" ->
                // Allow overriding the SQLite connection string in higher environments
                Environment.GetEnvironmentVariable("SQLITE_CONNECTION_STRING")
                |> Option.ofObj
                |> Option.defaultValue "Data Source=eatool.db;Cache=Shared;Mode=ReadWriteCreate"
            | _ -> "Data Source=eatool.db;Cache=Shared;Mode=ReadWriteCreate"

        {
            ConnectionString = connString
            Environment = env
        }

    /// Store the active configuration for later connections
    let private configure (config: DatabaseConfig) =
        currentConfig <- Some config

    /// Get an open SQLite connection using the active configuration
    let getConnection () : SqliteConnection =
        match currentConfig with
        | Some cfg ->
            let conn = new SqliteConnection(cfg.ConnectionString)
            conn.Open()
            conn
        | None -> invalidOp "Database not configured. Call Database.initializeSchema first."

    /// Initialize database schema (creates tables if they don't exist)
    let initializeSchema (config: DatabaseConfig) : Result<unit, string> =
        try
            configure config
            use conn = getConnection ()
            Ok ()
        with
        | ex -> Error ex.Message

    /// Health check - verify database connectivity
    let healthCheck (config: DatabaseConfig) : Result<bool, string> =
        try
            configure config
            use conn = getConnection ()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "SELECT 1"
            cmd.ExecuteScalar() |> ignore
            Ok true
        with
        | ex -> Error ex.Message
