/// Comprehensive authentication unit tests
module EATool.Tests.AuthenticationTests

open System
open Xunit
open EATool.Auth
open EATool.Tests.Fixtures.AuthTestHelpers

[<Fact>]
let ``Password strength validation works`` () =
    match PasswordHasher.validatePasswordStrength "Password1" with
    | Ok () -> ()
    | Error errs -> Assert.True(false, sprintf "Expected strong password, got errors: %A" errs)
    match PasswordHasher.validatePasswordStrength "short1" with
    | Ok () -> Assert.True(false, "Expected failure for short password")
    | Error _ -> ()
    match PasswordHasher.validatePasswordStrength "nouppercase1" with
    | Ok () -> Assert.True(false, "Expected failure for missing uppercase")
    | Error _ -> ()
    match PasswordHasher.validatePasswordStrength "NOLOWERCASE1" with
    | Ok () -> Assert.True(false, "Expected failure for missing lowercase")
    | Error _ -> ()
    match PasswordHasher.validatePasswordStrength "NoDigits" with
    | Ok () -> Assert.True(false, "Expected failure for missing digit")
    | Error _ -> ()

[<Fact>]
let ``Password hashing and verification works`` () =
    let pwd = "CorrectHorseBatteryStaple1"
    let (hash, _salt) =
        match PasswordHasher.hashPassword pwd with
        | Ok tuple -> tuple
        | Error err -> Assert.True(false, sprintf "Hashing failed: %s" err); ("", "")
    Assert.True(PasswordHasher.verifyPassword pwd hash)
    Assert.False(PasswordHasher.verifyPassword "WrongPassword" hash)

[<Fact>]
let ``JWT generate and validate returns claims`` () =
    let _ = initializeTestEnv() // ensure JWT env
    let token = JwtTokenService.generateAccessToken "user-123" "user@example.com" ["Viewer"]
    match JwtTokenService.validateAccessToken token with
    | TokenValidationResult.Valid claims ->
        Assert.Equal("user-123", claims.sub)
        Assert.Equal("user@example.com", claims.email)
        Assert.Equal<string list>(["Viewer"], claims.roles)
    | _ -> Assert.True(false, "Expected Valid token")

[<Fact>]
let ``Login with seeded user returns tokens`` () =
    let _ = initializeTestEnv()
    // Seed creates admin@example.com with bcrypt hash for 'password'
    let result = AuthService.login "admin@example.com" "password" |> Async.RunSynchronously
    match result with
    | Ok response ->
        Assert.NotNull(response.accessToken)
        Assert.NotNull(response.refreshToken)
        Assert.True(response.expiresIn > 0)
        Assert.Equal("admin@example.com", response.user.email)
    | Error err -> Assert.True(false, sprintf "Login failed: %s" err)

[<Fact>]
let ``Refresh issues new access token`` () =
    let _ = initializeTestEnv()
    let loginRes = AuthService.login "admin@example.com" "password" |> Async.RunSynchronously
    match loginRes with
    | Ok lr ->
        let refreshRes = AuthService.refresh lr.refreshToken |> Async.RunSynchronously
        match refreshRes with
        | Ok rr ->
            Assert.NotNull(rr.accessToken)
            Assert.True(rr.expiresIn > 0)
        | Error err -> Assert.True(false, sprintf "Refresh failed: %s" err)
    | Error err -> Assert.True(false, sprintf "Login failed: %s" err)

[<Fact>]
let ``Logout revokes token and prevents refresh`` () =
    let _ = initializeTestEnv()
    let loginRes = AuthService.login "admin@example.com" "password" |> Async.RunSynchronously
    match loginRes with
    | Ok lr ->
        let logoutRes = AuthService.logout lr.refreshToken |> Async.RunSynchronously
        match logoutRes with
        | Ok () ->
            let refreshRes = AuthService.refresh lr.refreshToken |> Async.RunSynchronously
            match refreshRes with
            | Ok _ -> Assert.True(false, "Refresh should fail after logout")
            | Error err -> Assert.Contains("revoked", err)
        | Error err -> Assert.True(false, sprintf "Logout failed: %s" err)
    | Error err -> Assert.True(false, sprintf "Login failed: %s" err)
