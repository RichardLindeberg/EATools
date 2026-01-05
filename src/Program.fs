open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Giraffe
open EATool.Infrastructure
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
    
    // Initialize database
    let dbConfig = Database.createConfig environment
    match Database.initializeSchema dbConfig with
    | Ok () -> printfn "[%s] Database initialized successfully" environment
    | Error err -> printfn "[%s] Database initialization failed: %s" environment err
    
    let app = builder.Build()
    
    // Configure middleware
    app.UseHttpsRedirection() |> ignore
    app.UseCors() |> ignore
    
    // Combine all routes and configure Giraffe
    let allRoutes = Endpoints.routes @ OrganizationsEndpoints.routes
    let webApp = choose allRoutes
    
    app.UseGiraffe(webApp)
    
    printfn "[%s] EA Tool API starting on port 8000" environment
    app.Run("http://localhost:8000")
    
    0
