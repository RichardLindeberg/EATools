/// Tests for PII detection in observability signals
module EATool.Tests.PIIDetectionTests

open System
open Xunit
open EATool.Tests.Fixtures

[<Fact>]
let ``Email addresses are detected`` () =
    let logContent = "User logged in: john.doe@example.com"
    
    Assert.Throws<System.Exception>(fun () ->
        OTelTestHelpers.assertLogContainsNoPII logContent
    ) |> ignore

[<Fact>]
let ``Email with special characters detected`` () =
    let logContent = "Contact: user+tag@company.co.uk"
    
    Assert.Throws<System.Exception>(fun () ->
        OTelTestHelpers.assertLogContainsNoPII logContent
    ) |> ignore

[<Fact>]
let ``US phone numbers are detected`` () =
    let logContent = "Call support at 555-123-4567"
    
    Assert.Throws<System.Exception>(fun () ->
        OTelTestHelpers.assertLogContainsNoPII logContent
    ) |> ignore

[<Fact>]
let ``Phone with dots are detected`` () =
    let logContent = "Number: 555.123.4567"
    
    Assert.Throws<System.Exception>(fun () ->
        OTelTestHelpers.assertLogContainsNoPII logContent
    ) |> ignore

[<Fact>]
let ``Credit card numbers are detected`` () =
    let logContent = "Card: 4532-1488-0343-6467"
    
    Assert.Throws<System.Exception>(fun () ->
        OTelTestHelpers.assertLogContainsNoPII logContent
    ) |> ignore

[<Fact>]
let ``Credit card without dashes detected`` () =
    let logContent = "Payment: 4532148803436467"
    
    Assert.Throws<System.Exception>(fun () ->
        OTelTestHelpers.assertLogContainsNoPII logContent
    ) |> ignore

[<Fact>]
let ``SSN is detected`` () =
    let logContent = "SSN: 123-45-6789"
    
    Assert.Throws<System.Exception>(fun () ->
        OTelTestHelpers.assertLogContainsNoPII logContent
    ) |> ignore

[<Fact>]
let ``Clean log passes PII check`` () =
    let logContent = "Request processed: GET /api/orgs status=200"
    
    // Should not throw
    OTelTestHelpers.assertLogContainsNoPII logContent

[<Fact>]
let ``Generic email pattern is safe`` () =
    let logContent = "User email field is redacted"
    
    // Should not throw
    OTelTestHelpers.assertLogContainsNoPII logContent

[<Fact>]
let ``Multiple PII detects first occurrence`` () =
    let logContent = "Email: test@example.com and Phone: 555-123-4567"
    
    Assert.Throws<System.Exception>(fun () ->
        OTelTestHelpers.assertLogContainsNoPII logContent
    ) |> ignore

[<Fact>]
let ``PII in JSON log is detected`` () =
    let logContent = """{"email":"user@example.com","action":"login"}"""
    
    Assert.Throws<System.Exception>(fun () ->
        OTelTestHelpers.assertLogContainsNoPII logContent
    ) |> ignore

[<Fact>]
let ``Case sensitivity in email detection`` () =
    let logContent = "User: TEST@EXAMPLE.COM"
    
    // Email pattern should be case-insensitive
    Assert.Throws<System.Exception>(fun () ->
        OTelTestHelpers.assertLogContainsNoPII logContent
    ) |> ignore

[<Fact>]
let ``International email domain detected`` () =
    let logContent = "Contact: admin@company.co.uk"
    
    Assert.Throws<System.Exception>(fun () ->
        OTelTestHelpers.assertLogContainsNoPII logContent
    ) |> ignore

[<Fact>]
let ``Numbers alone pass PII check`` () =
    let logContent = "Count: 12345, Value: 67890"
    
    OTelTestHelpers.assertLogContainsNoPII logContent

[<Fact>]
let ``Placeholder values pass PII check`` () =
    let logContent = "email: [REDACTED], phone: [REDACTED]"
    
    OTelTestHelpers.assertLogContainsNoPII logContent

[<Fact>]
let ``Log without sensitive fields passes`` () =
    let logContent = "Operation: CreateOrganization, Duration: 125ms, Status: OK"
    
    OTelTestHelpers.assertLogContainsNoPII logContent

[<Fact>]
let ``UUID doesn't trigger phone detection`` () =
    let logContent = "Request ID: 550e8400-e29b-41d4-a716-446655440000"
    
    // UUID pattern shouldn't match phone pattern
    OTelTestHelpers.assertLogContainsNoPII logContent

[<Fact>]
let ``IP address doesn't trigger detection`` () =
    let logContent = "Client IP: 192.168.1.1"
    
    // IP pattern shouldn't match phone/credit card pattern
    OTelTestHelpers.assertLogContainsNoPII logContent

[<Fact>]
let ``Timestamp with dashes doesn't trigger detection`` () =
    let logContent = "Timestamp: 2026-01-08-14-30-45"
    
    // Date format shouldn't be flagged as phone
    OTelTestHelpers.assertLogContainsNoPII logContent

[<Fact>]
let ``HTTP method and path are safe`` () =
    let logContent = "GET /api/users/123/profile HTTP/1.1"
    
    OTelTestHelpers.assertLogContainsNoPII logContent

[<Fact>]
let ``XML tags with email detected`` () =
    let logContent = "<email>john@example.com</email>"
    
    Assert.Throws<System.Exception>(fun () ->
        OTelTestHelpers.assertLogContainsNoPII logContent
    ) |> ignore

[<Fact>]
let ``Multiple emails detected`` () =
    let logContent = "Admins: alice@example.com, bob@example.org"
    
    Assert.Throws<System.Exception>(fun () ->
        OTelTestHelpers.assertLogContainsNoPII logContent
    ) |> ignore
