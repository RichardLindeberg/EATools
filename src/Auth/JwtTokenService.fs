/// JWT token generation and validation
namespace EATool.Auth

open System
open System.IdentityModel.Tokens.Jwt
open Microsoft.IdentityModel.Tokens
open System.Text
open System.Security.Claims

module JwtTokenService =
    
    let private getJwtSecret () =
        System.Environment.GetEnvironmentVariable("JWT_SECRET")
        |> Option.ofObj
        |> Option.defaultValue "dev-secret-key-minimum-32-characters-change-in-production"
    
    let private getAccessTokenExpiry () =
        System.Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES")
        |> Option.ofObj
        |> Option.bind (fun s -> 
            match Int32.TryParse s with 
            | (true, v) -> Some v 
            | _ -> None)
        |> Option.defaultValue Constants.AccessTokenExpiryMinutes
    
    let private createSecurityKey () =
        let secret = getJwtSecret()
        let key = Encoding.ASCII.GetBytes(secret)
        SymmetricSecurityKey(key)
    
    let generateAccessToken (userId: string) (email: string) (roles: string list) : string =
        let securityKey = createSecurityKey()
        let credentials = SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
        let now = DateTime.UtcNow
        let expiry = now.AddMinutes(float (getAccessTokenExpiry()))
        
        let baseClaims = [
            Claim(ClaimTypes.NameIdentifier, userId)
            Claim(ClaimTypes.Email, email)
            Claim("jti", Guid.NewGuid().ToString())
        ]
        
        let roleClaims = roles |> List.map (fun role -> Claim(ClaimTypes.Role, role))
        let allClaims = baseClaims @ roleClaims
        
        let token = JwtSecurityToken(
            issuer = "eatool",
            audience = "eatool-api",
            claims = allClaims,
            notBefore = now,
            expires = expiry,
            signingCredentials = credentials
        )
        
        JwtSecurityTokenHandler().WriteToken(token)
    
    let generateRefreshToken () : string =
        let randomBytes = Array.zeroCreate 32
        use rng = System.Security.Cryptography.RandomNumberGenerator.Create()
        rng.GetBytes(randomBytes)
        Convert.ToBase64String(randomBytes)
    
    let validateAccessToken (token: string) : EATool.Auth.TokenValidationResult =
        try
            let handler = JwtSecurityTokenHandler()
            let securityKey = createSecurityKey()
            
            let validationParameters = TokenValidationParameters(
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateIssuer = true,
                ValidIssuer = "eatool",
                ValidateAudience = true,
                ValidAudience = "eatool-api",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(60.0)
            )
            
            let (principal, _) = handler.ValidateToken(token, validationParameters)
            
            let sub = principal.FindFirst(ClaimTypes.NameIdentifier)
            let email = principal.FindFirst(ClaimTypes.Email)
            let roleClaims = principal.FindAll(ClaimTypes.Role)
            let roles = roleClaims |> Seq.map (fun c -> c.Value) |> Seq.toList
            
            if sub <> null && email <> null then
                let claims = {
                    sub = sub.Value
                    email = email.Value
                    roles = roles
                    iat = int64 (DateTime.UtcNow.Subtract(DateTime(1970,1,1)).TotalSeconds)
                    exp = int64 0
                    jti = ""
                }
                TokenValidationResult.Valid claims
            else
                TokenValidationResult.Invalid
        with
            | :? SecurityTokenExpiredException -> TokenValidationResult.Expired
            | :? SecurityTokenValidationException -> TokenValidationResult.Invalid
            | _ -> TokenValidationResult.Invalid
