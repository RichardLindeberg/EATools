module EATool.Infrastructure.Tracing.ActivitySourceFactory

open System
open System.Diagnostics

/// Central ActivitySource for EATool distributed tracing
let eaToolActivitySource = new ActivitySource("EATool", "1.0.0")

/// Get or create an ActivitySource for a specific module
let getActivitySource (moduleName: string) =
    eaToolActivitySource

/// Start a new activity (span) for an operation
let startActivity (operationName: string) (kind: ActivityKind) =
    eaToolActivitySource.StartActivity(operationName, kind)

/// Set tag on activity (attribute)
let setTag (activity: Activity) (key: string) (value: obj) =
    if activity <> null then 
        activity.SetTag(key, value) |> ignore
    activity

/// Set multiple tags on activity
let setTags (activity: Activity) (tags: (string * obj) list) =
    if activity <> null then
        tags |> List.iter (fun (k, v) -> activity.SetTag(k, v) |> ignore)
    activity

/// Execute operation within an activity span
let traceOperation (operationName: string) (operation: Activity -> 'a) =
    use activity = startActivity operationName ActivityKind.Internal
    try
        let result = operation activity
        result
    with ex ->
        if activity <> null then
            activity.SetTag("error.type", ex.GetType().Name) |> ignore
            activity.SetTag("error.message", ex.Message) |> ignore
        reraise()

/// Execute async operation within an activity span
let traceOperationAsync (operationName: string) (operation: Activity -> Async<'a>) =
    async {
        use activity = startActivity operationName ActivityKind.Internal
        try
            let! result = operation activity
            return result
        with ex ->
            if activity <> null then
                activity.SetTag("error.type", ex.GetType().Name) |> ignore
                activity.SetTag("error.message", ex.Message) |> ignore
            return raise ex
    }

/// Create a span for HTTP request handling
let traceHttpOperation (method: string) (path: string) (operation: Activity -> 'a) =
    let operationName = $"{method} {path}"
    use activity = startActivity operationName ActivityKind.Server
    
    if activity <> null then
        activity.SetTag("http.method", method) |> ignore
        activity.SetTag("http.target", path) |> ignore
    
    try
        let result = operation activity
        result
    with ex ->
        if activity <> null then
            activity.SetTag("error.type", ex.GetType().Name) |> ignore
            activity.SetTag("error.message", ex.Message) |> ignore
        reraise()

/// Create a span for command execution
let traceCommand (commandType: string) (aggregateId: string) (operation: Activity -> 'a) =
    let operationName = $"Command:{commandType}"
    use activity = startActivity operationName ActivityKind.Internal
    
    if activity <> null then
        activity.SetTag("messaging.system", "command") |> ignore
        activity.SetTag("command.type", commandType) |> ignore
        activity.SetTag("entity.id", aggregateId) |> ignore
    
    try
        let result = operation activity
        result
    with ex ->
        if activity <> null then
            activity.SetTag("error.type", ex.GetType().Name) |> ignore
            activity.SetTag("error.message", ex.Message) |> ignore
        reraise()

/// Create a span for database operation
let traceDbOperation (operation: string) (duration: float) (rowCount: int option) =
    let activity = Activity.Current
    if activity <> null then
        activity.SetTag("db.operation", operation) |> ignore
        activity.SetTag("db.duration_ms", duration) |> ignore
        rowCount |> Option.iter (fun count -> activity.SetTag("db.row_count", count) |> ignore)

/// Create a span for event store operation
let traceEventStore (operation: string) (eventCount: int) (totalSize: int64) (duration: float) =
    let activity = Activity.Current
    if activity <> null then
        activity.SetTag("db.system", "event_store") |> ignore
        activity.SetTag("db.operation", operation) |> ignore
        activity.SetTag("db.event_count", eventCount) |> ignore
        activity.SetTag("db.size_bytes", totalSize) |> ignore
        activity.SetTag("db.duration_ms", duration) |> ignore
