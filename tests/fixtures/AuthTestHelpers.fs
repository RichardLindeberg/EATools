/// Helpers for authentication tests: database setup and environment config
namespace EATool.Tests.Fixtures

open System
open System.IO
open Xunit
open EATool.Infrastructure
open EATool.Auth

module AuthTestHelpers =
    /// Create a unique SQLite connection string for isolation per test
    let createTestConnectionString () : string =
        let dbName = sprintf "eatool_test_%s.db" (Guid.NewGuid().ToString("N"))
        let dbPath = Path.Combine(Directory.GetCurrentDirectory(), dbName)
        sprintf "Data Source=%s;Cache=Shared;Mode=ReadWriteCreate" dbPath

    /// Configure JWT environment variables for deterministic tests
    let configureJwtEnv () =
        Environment.SetEnvironmentVariable("JWT_SECRET", "test-secret-change-me-32-chars-1234567890")
        Environment.SetEnvironmentVariable("JWT_EXPIRY_MINUTES", "15")
        Environment.SetEnvironmentVariable("JWT_REFRESH_EXPIRY_DAYS", "7")
        ()

    /// Run database migrations against provided connection string
    let runMigrations (connString: string) =
        let cfg : DatabaseConfig = { ConnectionString = connString; Environment = "test" }
        match Migrations.run cfg with
        | Ok () ->
            // Seed test users programmatically to avoid DbUp variable substitution issues
            use conn = new Microsoft.Data.Sqlite.SqliteConnection(connString)
            conn.Open()
            // Ensure RefreshTokens table exists to avoid race conditions in tests
            let ensureCmd = conn.CreateCommand()
            ensureCmd.CommandText <- "CREATE TABLE IF NOT EXISTS RefreshTokens (\n    id TEXT PRIMARY KEY,\n    user_id TEXT NOT NULL,\n    token_hash TEXT NOT NULL UNIQUE,\n    expires_at TEXT NOT NULL,\n    revoked_at TEXT,\n    created_at TEXT NOT NULL,\n    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE\n);\nCREATE INDEX IF NOT EXISTS idx_refresh_tokens_user_id ON RefreshTokens(user_id);\nCREATE INDEX IF NOT EXISTS idx_refresh_tokens_expires_at ON RefreshTokens(expires_at);\nCREATE INDEX IF NOT EXISTS idx_refresh_tokens_revoked_at ON RefreshTokens(revoked_at);"
            ensureCmd.ExecuteNonQuery() |> ignore
            // Admin user
            let (adminHash, adminSalt) =
                match PasswordHasher.hashPassword "password" with
                | Ok (h, s) -> (h, s)
                | Error err -> failwithf "Failed to hash admin password: %s" err
            let cmd1 = conn.CreateCommand()
            cmd1.CommandText <- "INSERT OR IGNORE INTO Users (id, email, password_hash, password_salt, roles, status, created_at, updated_at) VALUES (@id, @email, @hash, @salt, @roles, 'active', datetime('now'), datetime('now'))"
            cmd1.Parameters.AddWithValue("@id", "user-admin-dev-001") |> ignore
            cmd1.Parameters.AddWithValue("@email", "admin@example.com") |> ignore
            cmd1.Parameters.AddWithValue("@hash", adminHash) |> ignore
            cmd1.Parameters.AddWithValue("@salt", adminSalt) |> ignore
            cmd1.Parameters.AddWithValue("@roles", "[\"Admin\"]") |> ignore
            cmd1.ExecuteNonQuery() |> ignore
            // Viewer user
            let (userHash, userSalt) =
                match PasswordHasher.hashPassword "password" with
                | Ok (h, s) -> (h, s)
                | Error err -> failwithf "Failed to hash user password: %s" err
            let cmd2 = conn.CreateCommand()
            cmd2.CommandText <- "INSERT OR IGNORE INTO Users (id, email, password_hash, password_salt, roles, status, created_at, updated_at) VALUES (@id, @email, @hash, @salt, @roles, 'active', datetime('now'), datetime('now'))"
            cmd2.Parameters.AddWithValue("@id", "user-viewer-dev-001") |> ignore
            cmd2.Parameters.AddWithValue("@email", "user@example.com") |> ignore
            cmd2.Parameters.AddWithValue("@hash", userHash) |> ignore
            cmd2.Parameters.AddWithValue("@salt", userSalt) |> ignore
            cmd2.Parameters.AddWithValue("@roles", "[\"Viewer\"]") |> ignore
            cmd2.ExecuteNonQuery() |> ignore
        | Error err -> failwithf "Failed to run migrations: %s" err

    /// Set the CONNECTION_STRING env var so UserStore/TokenStore use test DB
    let setConnectionStringEnv (connString: string) =
        Environment.SetEnvironmentVariable("CONNECTION_STRING", connString) |> ignore
        Environment.SetEnvironmentVariable("SQLITE_CONNECTION_STRING", connString) |> ignore

    /// Initialize full test environment: unique DB + migrations + JWT env
    let initializeTestEnv () : string =
        let conn = createTestConnectionString()
        setConnectionStringEnv conn
        runMigrations conn
        configureJwtEnv()
        // Ensure stores use the exact test DB even if env changes later
        UserStore.setConnectionStringForTests conn
        TokenStore.setConnectionStringForTests conn
        conn
