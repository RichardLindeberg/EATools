module EATool.Infrastructure.Logging.LogContext

open System
open System.Runtime.CompilerServices
open System.Diagnostics
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging

/// Correlation ID key for HttpContext items
let private CorrelationIdKey = "CorrelationId"

/// Get or create correlation ID for the current request
let getOrCreateCorrelationId (context: HttpContext) =
    match context.Items.TryGetValue(CorrelationIdKey) with
    | true, value -> value :?> string
    | false, _ ->
        // Try to get from Activity (trace ID)
        match Activity.Current with
        | null ->
            // Generate new correlation ID
            let correlationId = Guid.NewGuid().ToString("N")
            context.Items.[CorrelationIdKey] <- correlationId
            correlationId
        | activity ->
            // Use trace ID as correlation ID
            let correlationId = activity.TraceId.ToString()
            context.Items.[CorrelationIdKey] <- correlationId
            correlationId

/// Get correlation ID from HttpContext if it exists
let tryGetCorrelationId (context: HttpContext) =
    match context.Items.TryGetValue(CorrelationIdKey) with
    | true, value -> Some (value :?> string)
    | false, _ -> None

/// Middleware to add correlation ID to all requests
type CorrelationIdMiddleware(next: RequestDelegate, logger: ILogger<CorrelationIdMiddleware>) =
    member _.InvokeAsync(context: HttpContext) =
        task {
            // Get or create correlation ID
            let correlationId = getOrCreateCorrelationId context
            
            // Add to response headers for clients
            context.Response.Headers.["X-Correlation-ID"] <- correlationId
            
            // Check if there's an incoming correlation ID header
            match context.Request.Headers.TryGetValue("X-Correlation-ID") with
            | true, values when values.Count > 0 ->
                // Use incoming correlation ID
                let incomingId = values.[0]
                context.Items.[CorrelationIdKey] <- incomingId
                context.Response.Headers.["X-Correlation-ID"] <- incomingId
            | _ -> ()
            
            // Log request start
            logger.LogInformation(
                "Request started: {Method} {Path} [CorrelationId: {CorrelationId}]",
                context.Request.Method,
                context.Request.Path,
                correlationId)
            
            try
                do! next.Invoke(context)
                
                // Log request completed
                logger.LogInformation(
                    "Request completed: {Method} {Path} {StatusCode} [CorrelationId: {CorrelationId}]",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    correlationId)
            with ex ->
                // Log request failed
                logger.LogError(
                    ex,
                    "Request failed: {Method} {Path} [CorrelationId: {CorrelationId}]",
                    context.Request.Method,
                    context.Request.Path,
                    correlationId)
                raise ex
        }

/// Extension methods for HttpContext to access correlation ID
type HttpContextExtensions =
    [<Extension>]
    static member GetCorrelationId(context: HttpContext) =
        getOrCreateCorrelationId context
    
    [<Extension>]
    static member TryGetCorrelationId(context: HttpContext) =
        tryGetCorrelationId context
