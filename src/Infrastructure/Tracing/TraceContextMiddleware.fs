module EATool.Infrastructure.Tracing.TraceContextMiddleware

open System
open System.Diagnostics
open Microsoft.AspNetCore.Http

/// W3C Trace Context header format: 00-{trace-id}-{span-id}-{trace-flags}
type TraceContextHeaders = {
    traceparent: string
    tracestate: string option
}

/// Extract trace context from incoming request headers
let extractTraceContext (request: HttpRequest) =
    let traceparent = 
        match request.Headers.TryGetValue("traceparent") with
        | true, values when values.Count > 0 -> Some (values.[0])
        | _ -> None
    
    let tracestate =
        match request.Headers.TryGetValue("tracestate") with
        | true, values when values.Count > 0 -> Some (values.[0])
        | _ -> None
    
    { traceparent = traceparent |> Option.defaultValue ""; tracestate = tracestate }

/// Create W3C traceparent header from current Activity
let createTraceparent (activity: Activity) =
    if activity <> null && activity.IsAllDataRequested then
        let traceId = activity.TraceId.ToString()
        let spanId = activity.SpanId.ToString()
        let flags = if activity.ActivityTraceFlags.HasFlag(ActivityTraceFlags.Recorded) then "01" else "00"
        $"00-{traceId}-{spanId}-{flags}"
    else
        ""

/// Inject trace context into outgoing response headers
let injectTraceContext (response: HttpResponse) (activity: Activity) =
    let traceparent = createTraceparent activity
    if not (String.IsNullOrWhiteSpace traceparent) then
        response.Headers.Add("traceparent", traceparent)

/// Trace Context Middleware for W3C compliance
type TraceContextMiddleware(next: RequestDelegate) =
    member _.Invoke(context: HttpContext) : System.Threading.Tasks.Task =
        task {
            use activity = ActivitySourceFactory.eaToolActivitySource.StartActivity(
                $"{context.Request.Method} {context.Request.Path}",
                ActivityKind.Server
            )
            
            // Ensure the activity is recorded so traceparent is generated
            if activity <> null then
                // Force activity to be recorded by setting tags
                activity.IsAllDataRequested <- true
                
                // Set standard HTTP semantic attributes
                activity.SetTag("http.method", context.Request.Method) |> ignore
                activity.SetTag("http.target", context.Request.Path.Value) |> ignore
                activity.SetTag("http.url", $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}") |> ignore
                activity.SetTag("http.scheme", context.Request.Scheme) |> ignore
                activity.SetTag("http.host", context.Request.Host.Host) |> ignore
                
                // Extract trace context from request headers if present
                let incomingContext = extractTraceContext context.Request
                if not (String.IsNullOrWhiteSpace incomingContext.traceparent) then
                    activity.SetTag("trace.incoming_context", incomingContext.traceparent) |> ignore
                
                // Store activity in HttpContext items for downstream access
                context.Items.["Activity"] <- activity
                
                // Register response callback to inject trace headers
                context.Response.OnStarting(fun () ->
                    task {
                        injectTraceContext context.Response activity
                        return ()
                    }
                ) |> ignore
            
            try
                do! next.Invoke(context)
                
                // Set response status code attribute
                if activity <> null then
                    activity.SetTag("http.status_code", context.Response.StatusCode) |> ignore
                    let statusOk = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300
                    if not statusOk then
                        activity.SetTag("error.type", "http_error") |> ignore
                        activity.SetTag("http.status_code", context.Response.StatusCode) |> ignore
            with ex ->
                if activity <> null then
                    activity.SetTag("http.status_code", 500) |> ignore
                    activity.SetTag("error.type", ex.GetType().Name) |> ignore
                    activity.SetTag("error.message", ex.Message) |> ignore
                raise ex
        }

/// Get current Activity from HttpContext
let getActivity (context: HttpContext) : Activity =
    match context.Items.TryGetValue("Activity") with
    | true, activity -> activity :?> Activity
    | _ -> Activity.Current

/// Extension methods for HttpContext
[<System.Runtime.CompilerServices.Extension>]
type HttpContextExtensions =
    
    /// Get the Activity (span) for this request
    [<System.Runtime.CompilerServices.Extension>]
    static member GetActivity(context: HttpContext) =
        getActivity context

/// Get trace ID for this request
let getTraceId (context: HttpContext) =
    let activity = getActivity context
    if activity <> null then activity.TraceId.ToString() else ""

/// Get span ID for this request
let getSpanId (context: HttpContext) =
    let activity = getActivity context
    if activity <> null then activity.SpanId.ToString() else ""

/// Get W3C traceparent header value
let getTraceparent (context: HttpContext) =
    let activity = getActivity context
    if activity <> null then createTraceparent activity else ""
