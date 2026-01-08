module EATool.Infrastructure.Logging.StructuredLogger

open System
open System.Diagnostics
open Microsoft.Extensions.Logging

/// Log attributes following OpenTelemetry semantic conventions
type LogAttributes = {
    TraceId: string option
    SpanId: string option
    TraceFlags: string option
    ServiceName: string
    ServiceInstanceId: string
    Environment: string
    OperationName: string option
    EntityType: string option
    EntityId: string option
    UserId: string option
    Duration: float option
    StatusCode: int option
    ErrorType: string option
    ErrorMessage: string option
}

/// Create default log attributes from current activity
let createDefaultAttributes () =
    let activity = Activity.Current
    let config = EATool.Infrastructure.Observability.getOTelConfig()
    
    {
        TraceId = activity |> Option.ofObj |> Option.map (fun a -> a.TraceId.ToString())
        SpanId = activity |> Option.ofObj |> Option.map (fun a -> a.SpanId.ToString())
        TraceFlags = activity |> Option.ofObj |> Option.map (fun a -> a.ActivityTraceFlags.ToString())
        ServiceName = config.ServiceInfo.Name
        ServiceInstanceId = config.ServiceInfo.InstanceId
        Environment = config.ServiceInfo.Environment
        OperationName = None
        EntityType = None
        EntityId = None
        UserId = None
        Duration = None
        StatusCode = None
        ErrorType = None
        ErrorMessage = None
    }

/// Update log attributes with operation details
let withOperation (operationName: string) (attrs: LogAttributes) =
    { attrs with OperationName = Some operationName }

/// Update log attributes with entity details
let withEntity (entityType: string) (entityId: string) (attrs: LogAttributes) =
    { attrs with EntityType = Some entityType; EntityId = Some entityId }

/// Update log attributes with user ID
let withUser (userId: string) (attrs: LogAttributes) =
    { attrs with UserId = Some userId }

/// Update log attributes with duration
let withDuration (duration: float) (attrs: LogAttributes) =
    { attrs with Duration = Some duration }

/// Update log attributes with HTTP status code
let withStatusCode (statusCode: int) (attrs: LogAttributes) =
    { attrs with StatusCode = Some statusCode }

/// Update log attributes with error details
let withError (errorType: string) (errorMessage: string) (attrs: LogAttributes) =
    { attrs with ErrorType = Some errorType; ErrorMessage = Some errorMessage }

/// Convert attributes to ILogger state dictionary
let toLoggerState (attrs: LogAttributes) =
    let state = Collections.Generic.Dictionary<string, obj>()
    
    attrs.TraceId |> Option.iter (fun v -> state.["trace_id"] <- v)
    attrs.SpanId |> Option.iter (fun v -> state.["span_id"] <- v)
    attrs.TraceFlags |> Option.iter (fun v -> state.["trace_flags"] <- v)
    state.["service.name"] <- attrs.ServiceName
    state.["service.instance.id"] <- attrs.ServiceInstanceId
    state.["deployment.environment"] <- attrs.Environment
    attrs.OperationName |> Option.iter (fun v -> state.["operation.name"] <- v)
    attrs.EntityType |> Option.iter (fun v -> state.["entity.type"] <- v)
    attrs.EntityId |> Option.iter (fun v -> state.["entity.id"] <- v)
    attrs.UserId |> Option.iter (fun v -> state.["user.id"] <- v)
    attrs.Duration |> Option.iter (fun v -> state.["duration_ms"] <- v)
    attrs.StatusCode |> Option.iter (fun v -> state.["http.status_code"] <- v)
    attrs.ErrorType |> Option.iter (fun v -> state.["error.type"] <- v)
    attrs.ErrorMessage |> Option.iter (fun v -> state.["error.message"] <- v)
    
    state :> Collections.Generic.IReadOnlyDictionary<string, obj>

/// Log an informational message
let logInfo (logger: ILogger) (message: string) (attrs: LogAttributes) =
    let state = toLoggerState attrs
    logger.Log(LogLevel.Information, EventId(0), state, null, fun s ex -> message)

/// Log a warning message
let logWarning (logger: ILogger) (message: string) (attrs: LogAttributes) =
    let state = toLoggerState attrs
    logger.Log(LogLevel.Warning, EventId(0), state, null, fun s ex -> message)

/// Log an error message
let logError (logger: ILogger) (message: string) (ex: Exception option) (attrs: LogAttributes) =
    let state = toLoggerState attrs
    let exn = ex |> Option.defaultValue null
    logger.Log(LogLevel.Error, EventId(0), state, exn, fun s ex -> message)

/// Log a debug message
let logDebug (logger: ILogger) (message: string) (attrs: LogAttributes) =
    let state = toLoggerState attrs
    logger.Log(LogLevel.Debug, EventId(0), state, null, fun s ex -> message)

/// Time an operation and log the duration
let logTimedOperation (logger: ILogger) (operationName: string) (operation: unit -> 'a) =
    let stopwatch = Diagnostics.Stopwatch.StartNew()
    let attrs = createDefaultAttributes() |> withOperation operationName
    
    try
        let result = operation()
        stopwatch.Stop()
        let finalAttrs = attrs |> withDuration stopwatch.Elapsed.TotalMilliseconds
        logInfo logger $"Operation '{operationName}' completed successfully" finalAttrs
        result
    with ex ->
        stopwatch.Stop()
        let finalAttrs = 
            attrs 
            |> withDuration stopwatch.Elapsed.TotalMilliseconds
            |> withError (ex.GetType().Name) ex.Message
        logError logger $"Operation '{operationName}' failed" (Some ex) finalAttrs
        raise ex

/// Time an async operation and log the duration
let logTimedOperationAsync (logger: ILogger) (operationName: string) (operation: unit -> Async<'a>) =
    async {
        let stopwatch = Diagnostics.Stopwatch.StartNew()
        let attrs = createDefaultAttributes() |> withOperation operationName
        
        try
            let! result = operation()
            stopwatch.Stop()
            let finalAttrs = attrs |> withDuration stopwatch.Elapsed.TotalMilliseconds
            logInfo logger $"Operation '{operationName}' completed successfully" finalAttrs
            return result
        with ex ->
            stopwatch.Stop()
            let finalAttrs = 
                attrs 
                |> withDuration stopwatch.Elapsed.TotalMilliseconds
                |> withError (ex.GetType().Name) ex.Message
            logError logger $"Operation '{operationName}' failed" (Some ex) finalAttrs
            return raise ex
    }

/// Log command execution
let logCommand (logger: ILogger) (commandType: string) (aggregateId: string) (result: Result<'a, string>) =
    let attrs = 
        createDefaultAttributes() 
        |> withOperation $"Command:{commandType}"
        |> withEntity "Command" aggregateId
    
    match result with
    | Ok _ ->
        logInfo logger $"Command '{commandType}' executed successfully for aggregate {aggregateId}" attrs
    | Error err ->
        let errorAttrs = attrs |> withError "CommandError" err
        logError logger $"Command '{commandType}' failed for aggregate {aggregateId}: {err}" None errorAttrs

/// Log event persistence
let logEventPersistence (logger: ILogger) (eventCount: int) (totalSize: int64) (duration: float) =
    let attrs = 
        createDefaultAttributes() 
        |> withOperation "EventStore:PersistEvents"
        |> withDuration duration
    
    let state = toLoggerState attrs
    let mutableState = Collections.Generic.Dictionary<string, obj>(state)
    mutableState.["event_count"] <- eventCount
    mutableState.["total_size_bytes"] <- totalSize
    
    logger.Log(
        LogLevel.Information, 
        EventId(0), 
        mutableState :> Collections.Generic.IReadOnlyDictionary<string, obj>, 
        null, 
        fun s ex -> $"Persisted {eventCount} events ({totalSize} bytes) in {duration:F2}ms")

/// Log database query
let logDatabaseQuery (logger: ILogger) (queryType: string) (duration: float) (rowCount: int option) =
    let attrs = 
        createDefaultAttributes() 
        |> withOperation $"Database:{queryType}"
        |> withDuration duration
    
    let state = toLoggerState attrs
    let mutableState = Collections.Generic.Dictionary<string, obj>(state)
    rowCount |> Option.iter (fun count -> mutableState.["row_count"] <- count)
    
    let message = 
        match rowCount with
        | Some count -> $"Database {queryType} completed in {duration:F2}ms, returned {count} rows"
        | None -> $"Database {queryType} completed in {duration:F2}ms"
    
    logger.Log(
        LogLevel.Debug, 
        EventId(0), 
        mutableState :> Collections.Generic.IReadOnlyDictionary<string, obj>, 
        null, 
        fun s ex -> message)
