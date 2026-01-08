open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Giraffe
open EATool.Infrastructure
open EATool.Infrastructure.Observability
open EATool.Api

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    
    // Get environment
    let environment = 
        System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        |> Option.ofObj
        |> Option.defaultValue "development"
    
    // Add services
    builder.Services.AddCors(fun options ->
        options.AddDefaultPolicy(fun policy ->
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
            |> ignore))
    |> ignore
    
    builder.Services.AddGiraffe() |> ignore
    
    // Configure OpenTelemetry
    configureOTelTracing builder.Services |> ignore
    configureOTelMetrics builder.Services |> ignore
    
    // Configure logging with OpenTelemetry
    builder.Logging.ClearProviders() |> ignore
    builder.Logging.AddConsole() |> ignore
    configureOTelLogging builder.Logging |> ignore
    
    // Initialize database
    let dbConfig = Database.createConfig environment
    match Database.initializeSchema dbConfig with
    | Ok () ->
        match Migrations.run dbConfig with
        | Ok () -> printfn "[%s] Database migrations applied" environment
        | Error err -> printfn "[%s] Database migration failed: %s" environment err
    | Error err -> printfn "[%s] Database initialization failed: %s" environment err
    
    let app = builder.Build()
    
    // Configure middleware
    app.UseHttpsRedirection() |> ignore
    app.UseCors() |> ignore
    
    // Combine all routes and configure Giraffe
    let allRoutes =
        HealthEndpoint.routes
        @ Endpoints.routes
        @ ApplicationsEndpoints.routes
        @ ApplicationServicesEndpoints.routes
        @ ApplicationInterfacesEndpoints.routes
        @ ServersEndpoints.routes
        @ IntegrationsEndpoints.routes
        @ OrganizationsEndpoints.routes
        @ BusinessCapabilitiesEndpoints.routes
        @ DataEntitiesEndpoints.routes
        @ RelationsEndpoints.routes
    let webApp = choose allRoutes
    
    app.UseGiraffe(webApp)
    
    printfn "[%s] EA Tool API starting on port 8000" environment
    app.Run("http://localhost:8000")
    
    0
