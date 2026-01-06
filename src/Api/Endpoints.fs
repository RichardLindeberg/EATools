/// API endpoint definitions
namespace EATool.Api

open Giraffe
open Thoth.Json.Net
open EATool.Infrastructure
open System.IO

/// Register basic/health endpoints
module Endpoints =
    
    let routes: HttpHandler list =
        [
            GET >=> route "/health" >=> fun next ctx -> task {
                let json = Encode.object [
                    "status", Encode.string "healthy"
                ]
                return! (Giraffe.Core.json json) next ctx
            }
            
            GET >=> route "/metadata" >=> fun next ctx -> task {
                let env = 
                    System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                    |> Option.ofObj
                    |> Option.defaultValue "production"
                let json = Encode.object [
                    "Name", Encode.string "EA Tool API"
                    "Version", Encode.string "1.0.0"
                    "Environment", Encode.string env
                ]
                return! (Giraffe.Core.json json) next ctx
            }
            
            GET >=> route "/API" >=> fun next ctx -> task {
                let specPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "spec", "spec-tool-api-contract.md")
                if File.Exists(specPath) then
                    let content = File.ReadAllText(specPath)
                    ctx.SetContentType "text/markdown; charset=utf-8"
                    return! ctx.WriteStringAsync content
                else
                    ctx.SetStatusCode 404
                    let errorJson = Json.encodeErrorResponse "not_found" "API specification not found"
                    return! (Giraffe.Core.json errorJson) next ctx
            }
            
            GET >=> route "/OpenApiSpecification" >=> fun next ctx -> task {
                let openapiPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "openapi.yaml")
                if File.Exists(openapiPath) then
                    let content = File.ReadAllText(openapiPath)
                    ctx.SetContentType "application/x-yaml; charset=utf-8"
                    ctx.SetHttpHeader("Content-Disposition", "inline; filename=\"openapi.yaml\"")
                    return! ctx.WriteStringAsync content
                else
                    ctx.SetStatusCode 404
                    let errorJson = Json.encodeErrorResponse "not_found" "OpenAPI specification not found"
                    return! (Giraffe.Core.json errorJson) next ctx
            }
        ]

