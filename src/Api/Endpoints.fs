/// API endpoint definitions
namespace EATool.Api

open Giraffe
open Thoth.Json
open EATool.Infrastructure

/// Register basic/health endpoints
module Endpoints =
    
    let routes: HttpHandler list =
        [
            GET >=> route "/health" >=> fun next ctx -> task {
                let json = Encode.object [
                    "status", Encode.string "healthy"
                ]
                return! (Giraffe.Core.json (Encode.toString 0 json)) next ctx
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
                return! (Giraffe.Core.json (Encode.toString 0 json)) next ctx
            }
        ]
