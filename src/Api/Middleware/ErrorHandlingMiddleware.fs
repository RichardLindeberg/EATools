/// Error handling middleware for standardized error responses
namespace EATool.Api.Middleware

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Thoth.Json.Net
open EATool.Api.ErrorResponse
open EATool.Api.ErrorCodes

type ErrorHandlingMiddleware(next: RequestDelegate, logger: ILogger<ErrorHandlingMiddleware>) =
    
    /// Get or generate request ID from headers
    let getRequestId (ctx: HttpContext) =
        match ctx.Request.Headers.TryGetValue("X-Request-ID") with
        | true, values when values.Count > 0 -> values.[0]
        | _ -> 
            match ctx.Request.Headers.TryGetValue("X-Correlation-ID") with
            | true, values when values.Count > 0 -> values.[0]
            | _ -> Guid.NewGuid().ToString()
    
    /// Get request path
    let getPath (ctx: HttpContext) =
        ctx.Request.Path.Value
    
    /// Convert ErrorResponse to JSON
    let encodeErrorResponse (error: ErrorResponse) =
        let fieldErrorEncoder (fe: FieldError) =
            Encode.object [
                "field", Encode.string fe.Field
                "code", Encode.string fe.Code
                "message", Encode.string fe.Message
            ]
        
        Encode.object [
            "error_code", Encode.string error.ErrorCode
            "message", Encode.string error.Message
            "details", (match error.Details with | Some d -> Encode.string d | None -> Encode.nil)
            "request_id", Encode.string error.RequestId
            "timestamp", Encode.string (error.Timestamp.ToString("o"))
            "path", Encode.string error.Path
            "field_errors", (match error.FieldErrors with | Some errors -> Encode.list (List.map fieldErrorEncoder errors) | None -> Encode.nil)
        ]
        |> Encode.toString 2
    
    /// Handle exception and return error response
    let handleException (ctx: HttpContext) (ex: Exception) = task {
        let requestId = getRequestId ctx
        let path = getPath ctx
        
        // Log the exception
        logger.LogError(ex, "Unhandled exception in request {RequestId} to {Path}", requestId, path)
        
        // Determine error response based on exception type
        let (statusCode, errorResponse) =
            match ex with
            | :? InvalidOperationException as ioe when ioe.Message.Contains("Version conflict") ->
                // Optimistic concurrency conflict
                (409, conflictError "Concurrent modification detected" ioe.Message requestId path)
            
            | :? InvalidOperationException as ioe ->
                // General validation/state error
                (400, create VALIDATION_ERROR ioe.Message requestId path)
            
            | :? ArgumentException as ae ->
                // Argument validation error
                (400, create VALIDATION_ERROR ae.Message requestId path)
            
            | :? UnauthorizedAccessException ->
                // Authentication/authorization error
                (401, create UNAUTHORIZED "Authentication required" requestId path)
            
            | _ ->
                // Unexpected internal error - don't expose details to client
                (500, internalError "An unexpected error occurred. Please contact support." requestId path)
        
        // Set response
        ctx.Response.StatusCode <- statusCode
        ctx.Response.ContentType <- "application/json; charset=utf-8"
        
        let json = encodeErrorResponse errorResponse
        do! ctx.Response.WriteAsync(json)
    }
    
    member _.InvokeAsync(ctx: HttpContext) : Task =
        task {
            try
                do! next.Invoke(ctx)
            with ex ->
                do! handleException ctx ex
        } :> Task
