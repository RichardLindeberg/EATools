/// API endpoint definitions
namespace EATool.Api

open Giraffe

/// Register basic/health endpoints
module Endpoints =
    
    let routes: HttpHandler list =
        [
            GET >=> route "/health" >=> Giraffe.Core.json {| status = "healthy" |}
            GET >=> route "/metadata" >=> Giraffe.Core.json {|
                Name = "EA Tool API"
                Version = "1.0.0"
                Environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            |}
        ]
