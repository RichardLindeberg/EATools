/// HTTP request and response metrics
module EATool.Infrastructure.Metrics.HttpMetrics

open System.Collections.Generic
open System.Diagnostics.Metrics
open Microsoft.AspNetCore.Http

/// Record HTTP request metrics
let recordRequest (method: string) (path: string) (statusCode: int) (durationMs: double) =
    let metrics = MetricsRegistry.getMetrics()
    
    let status = statusCode.ToString()
    let normalizedPath = if path = "/" then "/" else path.TrimEnd('/')
    
    // Record request count with low-cardinality labels
    metrics.HttpRequestCount.Add(
        1L,
        KeyValuePair("http.method", method :> obj),
        KeyValuePair("http.url.path", normalizedPath :> obj),
        KeyValuePair("http.response.status_code", status :> obj)
    )
    
    // Record duration with same labels
    metrics.HttpRequestDuration.Record(
        durationMs,
        KeyValuePair("http.method", method :> obj),
        KeyValuePair("http.url.path", normalizedPath :> obj),
        KeyValuePair("http.response.status_code", status :> obj)
    )

/// Middleware-friendly function to extract and record metrics
let recordHttpMetric (ctx: HttpContext) (method: string) (path: string) (durationMs: double) =
    let statusCode = ctx.Response.StatusCode
    recordRequest method path statusCode durationMs
