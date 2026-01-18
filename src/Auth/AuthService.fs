/// Authentication business logic - orchestration of services
namespace EATool.Auth

open System

module AuthService =
    
    // =========================================================================
    // Login - Verify credentials and issue tokens
    // =========================================================================
    
    let login (email: string) (password: string) : Async<Result<LoginResponse, string>> = async {
        // Validate inputs
        if System.String.IsNullOrWhiteSpace email || System.String.IsNullOrWhiteSpace password then
            return Error "Email and password are required"
        else
            // Find user by email
            match! async {
                try
                    let result = UserStore.findByEmail email
                    return result
                with ex ->
                    return Error ex.Message
            } with
            | Error err -> return Error err
            | Ok None -> return Error "Invalid credentials"
            | Ok (Some foundUser) ->
                // Verify password
                if not (PasswordHasher.verifyPassword password foundUser.passwordHash) then
                    return Error "Invalid credentials"
                else
                    // Check account status
                    if foundUser.status <> "active" then
                        return Error $"Account is {foundUser.status}"
                    else
                        // Update last login
                        match UserStore.updateLastLogin foundUser.id with
                        | Error _ -> () // Log but continue
                        | Ok () -> ()
                        
                        // Generate tokens
                        let accessToken = JwtTokenService.generateAccessToken foundUser.id foundUser.email foundUser.roles
                        let refreshToken = JwtTokenService.generateRefreshToken()
                        
                        // Save refresh token to database
                        match TokenStore.saveRefreshToken foundUser.id refreshToken Constants.RefreshTokenExpiryDays with
                        | Error err -> return Error err
                        | Ok () ->
                            let response = {
                                accessToken = accessToken
                                refreshToken = refreshToken
                                expiresIn = Constants.AccessTokenExpiryMinutes * 60  // Convert to seconds
                                user = {
                                    id = foundUser.id
                                    email = foundUser.email
                                    roles = foundUser.roles
                                    createdAt = foundUser.createdAt
                                }
                            }
                            return Ok response
    }
    
    // =========================================================================
    // Refresh - Issue new access token using refresh token
    // =========================================================================
    
    let refresh (refreshToken: string) : Async<Result<RefreshTokenResponse, string>> = async {
        if System.String.IsNullOrWhiteSpace refreshToken then
            return Error "Refresh token is required"
        else
            match! async {
                try
                    let result = TokenStore.findRefreshToken refreshToken
                    return result
                with ex ->
                    return Error ex.Message
            } with
            | Error err -> return Error err
            | Ok None -> return Error "Invalid token"
            | Ok (Some token) ->
                // Check if token is revoked
                if token.revokedAt.IsSome then
                    return Error "Token has been revoked"
                // Check if token is expired
                elif token.expiresAt < DateTime.UtcNow then
                    return Error "Token has expired"
                else
                    // Get user info
                    match! async {
                        try
                            let result = UserStore.findById token.userId
                            return result
                        with ex ->
                            return Error ex.Message
                    } with
                    | Error err -> return Error err
                    | Ok None -> return Error "User not found"
                    | Ok (Some user) ->
                        let newAccessToken = JwtTokenService.generateAccessToken user.id user.email user.roles
                        let response = {
                            accessToken = newAccessToken
                            expiresIn = Constants.AccessTokenExpiryMinutes * 60  // Convert to seconds
                        }
                        return Ok response
    }
    
    // =========================================================================
    // Logout - Revoke refresh token
    // =========================================================================
    
    let logout (refreshToken: string) : Async<Result<unit, string>> = async {
        if System.String.IsNullOrWhiteSpace refreshToken then
            return Error "Refresh token is required"
        else
            match! async {
                try
                    let result = TokenStore.revokeToken refreshToken
                    return result
                with ex ->
                    return Error ex.Message
            } with
            | Error err -> return Error err
            | Ok () -> return Ok ()
    }
    
    let getUser (userId: string) : Async<Result<UserInfo option, string>> = async {
        if System.String.IsNullOrWhiteSpace userId then
            return Error "User ID is required"
        else
            match! async {
                try
                    let result = UserStore.findById userId
                    return result
                with ex ->
                    return Error ex.Message
            } with
            | Error err -> return Error err
            | Ok None -> return Ok None
            | Ok (Some user) ->
                let userInfo = {
                    id = user.id
                    email = user.email
                    roles = user.roles
                    createdAt = user.createdAt
                }
                return Ok (Some userInfo)
    }
