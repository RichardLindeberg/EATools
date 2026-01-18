/// User database operations
namespace EATool.Auth

open System
open Microsoft.Data.Sqlite
open Thoth.Json.Net

module UserStore =
    
    // Optional override for tests to avoid environment cross-talk
    let mutable private testConnectionOverride : string option = None
    let setConnectionStringForTests (conn: string) =
        if System.String.IsNullOrWhiteSpace conn then () else testConnectionOverride <- Some conn
    
    let private getConnectionString () =
        // Prefer test override when set, else SQLITE_CONNECTION_STRING, then CONNECTION_STRING, then default
        match testConnectionOverride with
        | Some s when not (System.String.IsNullOrWhiteSpace s) -> s
        | _ ->
            let sqlite = System.Environment.GetEnvironmentVariable("SQLITE_CONNECTION_STRING")
            let conn = System.Environment.GetEnvironmentVariable("CONNECTION_STRING")
            match (Option.ofObj sqlite, Option.ofObj conn) with
            | Some s, _ when not (System.String.IsNullOrWhiteSpace s) -> s
            | _, Some c when not (System.String.IsNullOrWhiteSpace c) -> c
            | _ -> "Data Source=eatool.db"
    
    // =========================================================================
    // User Queries
    // =========================================================================
    
    /// Find user by email
    let findByEmail (email: string) : Result<User option, string> =
        try
            use connection = new SqliteConnection(getConnectionString())
            connection.Open()
            
            use command = connection.CreateCommand()
            command.CommandText <- 
                "SELECT id, email, password_hash, password_salt, roles, status, created_at, updated_at, last_login_at
                 FROM Users
                 WHERE email = @email"
            
            command.Parameters.AddWithValue("@email", email) |> ignore
            
            use reader = command.ExecuteReader()
            if reader.Read() then
                let rolesJson = reader.GetString(4)
                let roles = 
                    Decode.fromString (Decode.list Decode.string) rolesJson
                    |> function Ok r -> r | Error _ -> []
                
                let user = {
                    id = reader.GetString(0)
                    email = reader.GetString(1)
                    passwordHash = reader.GetString(2)
                    passwordSalt = reader.GetString(3)
                    roles = roles
                    status = reader.GetString(5)
                    createdAt = DateTime.Parse(reader.GetString(6))
                    updatedAt = DateTime.Parse(reader.GetString(7))
                    lastLoginAt = if reader.IsDBNull(8) then None else Some (DateTime.Parse(reader.GetString(8)))
                }
                Ok (Some user)
            else
                Ok None
        with ex ->
            Error $"Database error finding user by email: {ex.Message}"
    
    /// Find user by ID
    let findById (userId: string) : Result<User option, string> =
        try
            use connection = new SqliteConnection(getConnectionString())
            connection.Open()
            
            use command = connection.CreateCommand()
            command.CommandText <-
                "SELECT id, email, password_hash, password_salt, roles, status, created_at, updated_at, last_login_at
                 FROM Users
                 WHERE id = @id"
            
            command.Parameters.AddWithValue("@id", userId) |> ignore
            
            use reader = command.ExecuteReader()
            if reader.Read() then
                let rolesJson = reader.GetString(4)
                let roles = 
                    Decode.fromString (Decode.list Decode.string) rolesJson
                    |> function Ok r -> r | Error _ -> []
                
                let user = {
                    id = reader.GetString(0)
                    email = reader.GetString(1)
                    passwordHash = reader.GetString(2)
                    passwordSalt = reader.GetString(3)
                    roles = roles
                    status = reader.GetString(5)
                    createdAt = DateTime.Parse(reader.GetString(6))
                    updatedAt = DateTime.Parse(reader.GetString(7))
                    lastLoginAt = if reader.IsDBNull(8) then None else Some (DateTime.Parse(reader.GetString(8)))
                }
                Ok (Some user)
            else
                Ok None
        with ex ->
            Error $"Database error finding user by ID: {ex.Message}"
    
    /// Update user's last login timestamp
    let updateLastLogin (userId: string) : Result<unit, string> =
        try
            use connection = new SqliteConnection(getConnectionString())
            connection.Open()
            
            use command = connection.CreateCommand()
            command.CommandText <-
                "UPDATE Users SET last_login_at = @last_login_at WHERE id = @id"
            
            command.Parameters.AddWithValue("@last_login_at", DateTime.UtcNow) |> ignore
            command.Parameters.AddWithValue("@id", userId) |> ignore
            
            command.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Database error updating last login: {ex.Message}"
