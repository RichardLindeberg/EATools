module EATool.Api.HealthEndpoint

open System
open System.Diagnostics
open Microsoft.AspNetCore.Http
open Giraffe
open EATool.Infrastructure.Observability

/// Health status response
type HealthStatus = {
    status: string
    service: string
    version: string
    environment: string
    instance_id: string
    uptime_seconds: float
    timestamp: string
    observability: ObservabilityStatus
}

and ObservabilityStatus = {
    tracing_enabled: bool
    metrics_enabled: bool
    otlp_endpoint: string option
}

/// Application start time for uptime calculation
let private startTime = DateTime.UtcNow

/// Get health status
let getHealthStatus () =
    let config = getOTelConfig ()
    let uptime = (DateTime.UtcNow - startTime).TotalSeconds
    
    {
        status = "healthy"
        service = config.ServiceInfo.Name
        version = config.ServiceInfo.Version
        environment = config.ServiceInfo.Environment
        instance_id = config.ServiceInfo.InstanceId
        uptime_seconds = uptime
        timestamp = DateTime.UtcNow.ToString("o")
        observability = {
            tracing_enabled = Activity.DefaultIdFormat = ActivityIdFormat.W3C
            metrics_enabled = true
            otlp_endpoint = config.OtlpEndpoint
        }
    }

/// Health endpoint handler
let healthHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let health = getHealthStatus ()
            return! json health next ctx
        }

/// Health routes
let routes: HttpHandler list = [
    GET >=> route "/health" >=> healthHandler
]
