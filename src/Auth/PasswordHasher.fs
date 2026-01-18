/// Password hashing and validation using bcrypt
namespace EATool.Auth

open System
open BCrypt.Net

module PasswordHasher =
    
    /// Hash a password with bcrypt
    let hashPassword (password: string) : Result<string * string, string> =
        try
            let salt = BCrypt.GenerateSalt(Constants.BcryptWorkFactor)
            let hash = BCrypt.HashPassword(password, salt)
            Ok (hash, salt)
        with ex ->
            Error $"Password hashing failed: {ex.Message}"
    
    /// Verify password against hash
    let verifyPassword (password: string) (hash: string) : bool =
        try
            BCrypt.Verify(password, hash)
        with
            | _ -> false
    
    /// Validate password strength
    let validatePasswordStrength (password: string) : Result<unit, string list> =
        let errors = [
            if String.IsNullOrWhiteSpace password then
                "Password cannot be empty"
            if password.Length < Constants.PasswordMinLength then
                $"Password must be at least {Constants.PasswordMinLength} characters"
            if not (password |> Seq.exists Char.IsUpper) then
                "Password must contain at least one uppercase letter"
            if not (password |> Seq.exists Char.IsLower) then
                "Password must contain at least one lowercase letter"
            if not (password |> Seq.exists Char.IsDigit) then
                "Password must contain at least one digit"
        ]
        
        if List.isEmpty errors then Ok ()
        else Error errors
