/// HTTP endpoint handlers for authentication
namespace EATool.Api

open Giraffe
open Thoth.Json.Net
open EATool.Auth
open Microsoft.AspNetCore.Http

module AuthEndpoints =
    
    // =========================================================================
    // POST /auth/login - User login endpoint
    // =========================================================================
    
    let loginHandler : HttpHandler = fun next ctx -> task {
        try
            let! body = ctx.BindJsonAsync<LoginRequest>()
            
            // Validate input
            if System.String.IsNullOrWhiteSpace body.email || System.String.IsNullOrWhiteSpace body.password then
                ctx.SetStatusCode 400
                let errorJson = 
                    Encode.object [
                        "code", Encode.string "validation_error"
                        "message", Encode.string "Email and password are required"
                    ]
                return! json errorJson next ctx
            else
                // Call auth service
                let! result = AuthService.login body.email body.password
                
                match result with
                | Ok response ->
                    ctx.SetStatusCode 200
                    let responseJson =
                        Encode.object [
                            "accessToken", Encode.string response.accessToken
                            "refreshToken", Encode.string response.refreshToken
                            "expiresIn", Encode.int response.expiresIn
                            "user", Encode.object [
                                "id", Encode.string response.user.id
                                "email", Encode.string response.user.email
                                "roles", Encode.list (List.map Encode.string response.user.roles)
                                "createdAt", Encode.datetime response.user.createdAt
                            ]
                        ]
                    return! json responseJson next ctx
                    
                | Error err ->
                    ctx.SetStatusCode 401
                    let errorJson =
                        Encode.object [
                            "code", Encode.string "unauthorized"
                            "message", Encode.string err
                        ]
                    return! json errorJson next ctx
        with ex ->
            ctx.SetStatusCode 500
            let errorJson =
                Encode.object [
                    "code", Encode.string "internal_error"
                    "message", Encode.string "An error occurred during login"
                ]
            return! json errorJson next ctx
    }
    
    // =========================================================================
    // POST /auth/refresh - Refresh access token
    // =========================================================================
    
    let refreshHandler : HttpHandler = fun next ctx -> task {
        try
            let! body = ctx.BindJsonAsync<RefreshTokenRequest>()
            
            if System.String.IsNullOrWhiteSpace body.refreshToken then
                ctx.SetStatusCode 400
                let errorJson =
                    Encode.object [
                        "code", Encode.string "validation_error"
                        "message", Encode.string "Refresh token is required"
                    ]
                return! json errorJson next ctx
            else
                // Call auth service
                let! result = AuthService.refresh body.refreshToken
                
                match result with
                | Ok response ->
                    ctx.SetStatusCode 200
                    let responseJson =
                        Encode.object [
                            "accessToken", Encode.string response.accessToken
                            "expiresIn", Encode.int response.expiresIn
                        ]
                    return! json responseJson next ctx
                    
                | Error err ->
                    ctx.SetStatusCode 401
                    let errorJson =
                        Encode.object [
                            "code", Encode.string "unauthorized"
                            "message", Encode.string err
                        ]
                    return! json errorJson next ctx
        with ex ->
            ctx.SetStatusCode 500
            let errorJson =
                Encode.object [
                    "code", Encode.string "internal_error"
                    "message", Encode.string "An error occurred during refresh"
                ]
            return! json errorJson next ctx
    }
    
    // =========================================================================
    // POST /auth/logout - Logout and revoke refresh token
    // =========================================================================
    
    let logoutHandler : HttpHandler = fun next ctx -> task {
        try
            let! body = ctx.BindJsonAsync<LogoutRequest>()
            
            if System.String.IsNullOrWhiteSpace body.refreshToken then
                ctx.SetStatusCode 400
                let errorJson =
                    Encode.object [
                        "code", Encode.string "validation_error"
                        "message", Encode.string "Refresh token is required"
                    ]
                return! json errorJson next ctx
            else
                // Call auth service
                let! result = AuthService.logout body.refreshToken
                
                match result with
                | Ok () ->
                    ctx.SetStatusCode 200
                    let responseJson = Encode.object []
                    return! json responseJson next ctx
                    
                | Error err ->
                    ctx.SetStatusCode 400
                    let errorJson =
                        Encode.object [
                            "code", Encode.string "bad_request"
                            "message", Encode.string err
                        ]
                    return! json errorJson next ctx
        with ex ->
            ctx.SetStatusCode 500
            let errorJson =
                Encode.object [
                    "code", Encode.string "internal_error"
                    "message", Encode.string "An error occurred during logout"
                ]
            return! json errorJson next ctx
    }
    
    // =========================================================================
    // GET /auth/me - Get current user info
    // =========================================================================
    
    let meHandler : HttpHandler = fun next ctx -> task {
        try
            // Extract Authorization header
            let authHeader = ctx.Request.Headers.["Authorization"].ToString()
            
            if System.String.IsNullOrWhiteSpace authHeader then
                ctx.SetStatusCode 401
                let errorJson =
                    Encode.object [
                        "code", Encode.string "unauthorized"
                        "message", Encode.string "Authorization header required"
                    ]
                return! json errorJson next ctx
            else
                // Extract bearer token
                let token = 
                    if authHeader.StartsWith("Bearer ") then
                        authHeader.Substring("Bearer ".Length).Trim()
                    else
                        ""
                
                if System.String.IsNullOrWhiteSpace token then
                    ctx.SetStatusCode 401
                    let errorJson =
                        Encode.object [
                            "code", Encode.string "unauthorized"
                            "message", Encode.string "Invalid authorization header"
                        ]
                    return! json errorJson next ctx
                else
                    // Validate token and extract claims
                    match JwtTokenService.validateAccessToken token with
                    | TokenValidationResult.Valid claims ->
                        let! result = AuthService.getUser claims.sub
                        
                        match result with
                        | Ok (Some user) ->
                            ctx.SetStatusCode 200
                            let responseJson =
                                Encode.object [
                                    "id", Encode.string user.id
                                    "email", Encode.string user.email
                                    "roles", Encode.list (List.map Encode.string user.roles)
                                    "createdAt", Encode.datetime user.createdAt
                                ]
                            return! json responseJson next ctx
                        | Ok None ->
                            ctx.SetStatusCode 404
                            let errorJson =
                                Encode.object [
                                    "code", Encode.string "not_found"
                                    "message", Encode.string "User not found"
                                ]
                            return! json errorJson next ctx
                        | Error err ->
                            ctx.SetStatusCode 500
                            let errorJson =
                                Encode.object [
                                    "code", Encode.string "internal_error"
                                    "message", Encode.string err
                                ]
                            return! json errorJson next ctx
                    
                    | TokenValidationResult.Expired ->
                        ctx.SetStatusCode 401
                        let errorJson =
                            Encode.object [
                                "code", Encode.string "unauthorized"
                                "message", Encode.string "Token has expired"
                            ]
                        return! json errorJson next ctx
                    
                    | TokenValidationResult.Invalid | TokenValidationResult.NotFound ->
                        ctx.SetStatusCode 401
                        let errorJson =
                            Encode.object [
                                "code", Encode.string "unauthorized"
                                "message", Encode.string "Invalid token"
                            ]
                        return! json errorJson next ctx
        with ex ->
            ctx.SetStatusCode 500
            let errorJson =
                Encode.object [
                    "code", Encode.string "internal_error"
                    "message", Encode.string "An error occurred"
                ]
            return! json errorJson next ctx
    }
    
    // =========================================================================
    // Route Registration
    // =========================================================================
    
    let routes : HttpHandler list = [
        POST >=> route "/auth/login" >=> loginHandler
        POST >=> route "/auth/refresh" >=> refreshHandler
        POST >=> route "/auth/logout" >=> logoutHandler
        GET >=> route "/auth/me" >=> meHandler
    ]
