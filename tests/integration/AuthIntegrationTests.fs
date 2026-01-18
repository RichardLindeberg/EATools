/// Integration tests for authentication endpoints using TestServer
module EATool.Tests.Integration.AuthIntegrationTests

open System
open System.Net.Http
open System.Net.Http.Headers
open System.Text
open System.Text.Json
open System.Threading.Tasks
open Xunit
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.TestHost
open Giraffe
open EATool.Infrastructure
open EATool.Infrastructure.Observability
open EATool.Infrastructure.Logging.LogContext
open EATool.Infrastructure.Tracing
open EATool.Infrastructure.Metrics
open EATool.Api
open EATool.Tests.Fixtures.AuthTestHelpers

let private buildTestClient () : HttpClient =
    // Configure environment variables for app startup
    let conn = initializeTestEnv() // sets JWT_* and CONNECTION_STRING (for stores)
    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "staging") |> ignore
    Environment.SetEnvironmentVariable("SQLITE_CONNECTION_STRING", conn) |> ignore

    // Build a test web application
    let builder = WebApplication.CreateBuilder()
    builder.WebHost.UseTestServer() |> ignore

    // Services (mirror Program.fs)
    builder.Services.AddCors(fun options ->
        options.AddDefaultPolicy(fun policy ->
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader() |> ignore)) |> ignore
    builder.Services.AddGiraffe() |> ignore

    // Observability
    configureOTelTracing builder.Services |> ignore
    configureOTelMetrics builder.Services |> ignore
    builder.Logging.ClearProviders() |> ignore
    builder.Logging.AddConsole() |> ignore
    builder.Logging.AddJsonConsole(fun options ->
        options.IncludeScopes <- true
        options.TimestampFormat <- "yyyy-MM-dd'T'HH:mm:ss.fff'Z'"
        options.UseUtcTimestamp <- true) |> ignore
    configureOTelLogging builder.Logging |> ignore
    builder.Logging.SetMinimumLevel(LogLevel.Debug) |> ignore

    // Initialize database + migrations
    let dbConfig = Database.createConfig "staging"
    match Database.initializeSchema dbConfig with
    | Ok () ->
        match Migrations.run dbConfig with
        | Ok () -> ()
        | Error err -> failwithf "Database migration failed: %s" err
    | Error err -> failwithf "Database initialization failed: %s" err

    // Build app and middleware
    let app = builder.Build()
    app.UseMiddleware<EATool.Api.Middleware.ErrorHandlingMiddleware>() |> ignore
    app.UseMiddleware<TraceContextMiddleware.TraceContextMiddleware>() |> ignore
    app.UseMiddleware<CorrelationIdMiddleware>() |> ignore
    app.UseCors() |> ignore

    let allRoutes =
        HealthEndpoint.routes
        @ MetricsEndpoint.routes
        @ Endpoints.routes
        @ AuthEndpoints.routes
        @ ApplicationsEndpoints.routes
        @ ApplicationServicesEndpoints.routes
        @ ApplicationInterfacesEndpoints.routes
        @ ServersEndpoints.routes
        @ DocumentationEndpoints.routes
        @ IntegrationsEndpoints.routes
        @ OrganizationsEndpoints.routes
        @ BusinessCapabilitiesEndpoints.routes
        @ DataEntitiesEndpoints.routes
        @ RelationsEndpoints.routes
    let webApp = choose allRoutes
    app.UseGiraffe(webApp)

    app.StartAsync().GetAwaiter().GetResult()
    let client = app.GetTestClient()
    client

let private postJson (client: HttpClient) (path: string) (jsonBody: string) : Task<HttpResponseMessage> =
    let content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
    client.PostAsync(path, content)

let private readJson (resp: HttpResponseMessage) : JsonDocument =
    let str = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult()
    JsonDocument.Parse(str)

let private await<'T> (t: Task<'T>) : 'T =
    t.GetAwaiter().GetResult()

[<Fact>]
let ``Login succeeds and returns tokens`` () =
    let client = buildTestClient()
    let resp = postJson client "/auth/login" "{\"email\":\"admin@example.com\",\"password\":\"password\"}" |> await
    Assert.Equal(200, int resp.StatusCode)
    let doc = readJson resp
    let access = doc.RootElement.GetProperty("accessToken").GetString()
    let refresh = doc.RootElement.GetProperty("refreshToken").GetString()
    Assert.False(String.IsNullOrWhiteSpace access)
    Assert.False(String.IsNullOrWhiteSpace refresh)

[<Fact>]
let ``Login fails with wrong password`` () =
    let client = buildTestClient()
    let resp = postJson client "/auth/login" "{\"email\":\"admin@example.com\",\"password\":\"wrong\"}" |> await
    Assert.Equal(401, int resp.StatusCode)

[<Fact>]
let ``GET /auth/me returns user with Bearer token`` () =
    let client = buildTestClient()
    let login = postJson client "/auth/login" "{\"email\":\"admin@example.com\",\"password\":\"password\"}" |> await
    let doc = readJson login
    let access = doc.RootElement.GetProperty("accessToken").GetString()
    client.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Bearer", access)
    let meResp = client.GetAsync("/auth/me").GetAwaiter().GetResult()
    Assert.Equal(200, int meResp.StatusCode)
    let meDoc = readJson meResp
    Assert.Equal("admin@example.com", meDoc.RootElement.GetProperty("email").GetString())

[<Fact>]
let ``Refresh issues new access token`` () =
    let client = buildTestClient()
    let login = postJson client "/auth/login" "{\"email\":\"admin@example.com\",\"password\":\"password\"}" |> await
    let doc = readJson login
    let refresh = doc.RootElement.GetProperty("refreshToken").GetString()
    let refreshResp = postJson client "/auth/refresh" (sprintf "{\"refreshToken\":\"%s\"}" refresh) |> await
    Assert.Equal(200, int refreshResp.StatusCode)
    let rrDoc = readJson refreshResp
    let newAccess = rrDoc.RootElement.GetProperty("accessToken").GetString()
    Assert.False(String.IsNullOrWhiteSpace newAccess)

[<Fact>]
let ``Logout revokes token and blocks refresh`` () =
    let client = buildTestClient()
    let login = postJson client "/auth/login" "{\"email\":\"admin@example.com\",\"password\":\"password\"}" |> await
    let doc = readJson login
    let refresh = doc.RootElement.GetProperty("refreshToken").GetString()
    let logoutResp = postJson client "/auth/logout" (sprintf "{\"refreshToken\":\"%s\"}" refresh) |> await
    Assert.Equal(200, int logoutResp.StatusCode)
    let refreshResp = postJson client "/auth/refresh" (sprintf "{\"refreshToken\":\"%s\"}" refresh) |> await
    Assert.Equal(401, int refreshResp.StatusCode)
