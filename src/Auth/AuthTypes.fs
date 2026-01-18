/// Authentication domain types and request/response contracts
namespace EATool.Auth

open System

// ============================================================================
// Request Types
// ============================================================================

/// Login request from client
type LoginRequest = {
    email: string
    password: string
}

/// Refresh token request from client
type RefreshTokenRequest = {
    refreshToken: string
}

/// Logout request from client
type LogoutRequest = {
    refreshToken: string
}

// ============================================================================
// Response Types
// ============================================================================

/// User information in responses (no password)
type UserInfo = {
    id: string
    email: string
    roles: string list
    createdAt: DateTime
}

/// Login response to client
type LoginResponse = {
    accessToken: string
    refreshToken: string
    expiresIn: int  // seconds (900 for 15 min)
    user: UserInfo
}

/// Refresh token response to client
type RefreshTokenResponse = {
    accessToken: string
    expiresIn: int  // seconds (900 for 15 min)
}

// ============================================================================
// Internal Domain Types
// ============================================================================

/// User entity in database
type User = {
    id: string
    email: string
    passwordHash: string
    passwordSalt: string
    roles: string list
    status: string  // "active" | "locked" | "suspended"
    createdAt: DateTime
    updatedAt: DateTime
    lastLoginAt: DateTime option
}

/// Refresh token entity in database
type RefreshToken = {
    id: string
    userId: string
    tokenHash: string  // Hash of token (never store plain text)
    expiresAt: DateTime
    revokedAt: DateTime option
    createdAt: DateTime
}

/// JWT claims extracted from token
type JwtClaims = {
    sub: string         // user id (subject)
    email: string
    roles: string list
    iat: int64         // issued at (unix timestamp)
    exp: int64         // expiration (unix timestamp)
    jti: string        // jwt id (unique identifier)
}

/// Result of token validation
type TokenValidationResult =
    | Valid of JwtClaims
    | Expired
    | Invalid
    | NotFound

// ============================================================================
// Constants
// ============================================================================

module Constants =
    [<Literal>]
    let AccessTokenExpiryMinutes = 15
    
    [<Literal>]
    let RefreshTokenExpiryDays = 7
    
    [<Literal>]
    let PasswordMinLength = 8
    
    [<Literal>]
    let BcryptWorkFactor = 10
