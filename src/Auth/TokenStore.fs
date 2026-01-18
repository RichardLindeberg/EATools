/// Refresh token database operations and lifecycle management
namespace EATool.Auth

open System
open Microsoft.Data.Sqlite

module TokenStore =
    
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
    // Token Hashing
    // =========================================================================
    
    /// Hash token using SHA256 for secure storage
    let private hashToken (token: string) : string =
        use sha256 = System.Security.Cryptography.SHA256.Create()
        token
        |> System.Text.Encoding.UTF8.GetBytes
        |> sha256.ComputeHash
        |> Convert.ToBase64String
    
    // =========================================================================
    // Token Persistence
    // =========================================================================
    
    /// Save refresh token to database (hashed)
    let saveRefreshToken (userId: string) (token: string) (expiryDays: int) : Result<unit, string> =
        try
            let tokenHash = hashToken token
            let expiresAt = DateTime.UtcNow.AddDays(float expiryDays)
            let tokenId = Guid.NewGuid().ToString()
            
            use connection = new SqliteConnection(getConnectionString())
            connection.Open()
            
            use command = connection.CreateCommand()
            command.CommandText <-
                "INSERT INTO RefreshTokens (id, user_id, token_hash, expires_at, created_at)
                 VALUES (@id, @user_id, @token_hash, @expires_at, @created_at)"
            
            command.Parameters.AddWithValue("@id", tokenId) |> ignore
            command.Parameters.AddWithValue("@user_id", userId) |> ignore
            command.Parameters.AddWithValue("@token_hash", tokenHash) |> ignore
            command.Parameters.AddWithValue("@expires_at", expiresAt) |> ignore
            command.Parameters.AddWithValue("@created_at", DateTime.UtcNow) |> ignore
            
            command.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Database error saving refresh token: {ex.Message}"
    
    /// Find and validate refresh token
    let findRefreshToken (token: string) : Result<RefreshToken option, string> =
        try
            let tokenHash = hashToken token
            
            use connection = new SqliteConnection(getConnectionString())
            connection.Open()
            
            use command = connection.CreateCommand()
            command.CommandText <-
                "SELECT id, user_id, token_hash, expires_at, revoked_at, created_at
                 FROM RefreshTokens
                 WHERE token_hash = @token_hash"
            
            command.Parameters.AddWithValue("@token_hash", tokenHash) |> ignore
            
            use reader = command.ExecuteReader()
            if reader.Read() then
                let refreshToken = {
                    id = reader.GetString(0)
                    userId = reader.GetString(1)
                    tokenHash = reader.GetString(2)
                    expiresAt = DateTime.Parse(reader.GetString(3))
                    revokedAt = if reader.IsDBNull(4) then None else Some (DateTime.Parse(reader.GetString(4)))
                    createdAt = DateTime.Parse(reader.GetString(5))
                }
                Ok (Some refreshToken)
            else
                Ok None
        with ex ->
            Error $"Database error finding refresh token: {ex.Message}"
    
    /// Revoke a refresh token by marking it revoked
    let revokeToken (token: string) : Result<unit, string> =
        try
            let tokenHash = hashToken token
            
            use connection = new SqliteConnection(getConnectionString())
            connection.Open()
            
            use command = connection.CreateCommand()
            command.CommandText <-
                "UPDATE RefreshTokens SET revoked_at = @revoked_at WHERE token_hash = @token_hash"
            
            command.Parameters.AddWithValue("@revoked_at", DateTime.UtcNow) |> ignore
            command.Parameters.AddWithValue("@token_hash", tokenHash) |> ignore
            
            command.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Database error revoking token: {ex.Message}"
    
    /// Clean up expired tokens (optional maintenance)
    let cleanupExpiredTokens () : Result<int, string> =
        try
            use connection = new SqliteConnection(getConnectionString())
            connection.Open()
            
            use command = connection.CreateCommand()
            command.CommandText <-
                "DELETE FROM RefreshTokens WHERE expires_at < @now"
            
            command.Parameters.AddWithValue("@now", DateTime.UtcNow) |> ignore
            
            let deletedCount = command.ExecuteNonQuery()
            Ok deletedCount
        with ex ->
            Error $"Database error cleaning up tokens: {ex.Message}"
