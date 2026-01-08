module EATool.Api.Instrumentation

open System
open System.Diagnostics
open Microsoft.AspNetCore.Http
open EATool.Infrastructure.Tracing

/// Record command processing in trace
let recordCommand (commandType: string) (aggregateId: string) (result: string) =
    let activity = Activity.Current
    if activity <> null then
        activity.SetTag("command.type", commandType) |> ignore
        activity.SetTag("entity.id", aggregateId) |> ignore
        activity.SetTag("entity.type", "aggregate") |> ignore
        activity.SetTag("command.result", result) |> ignore

/// Record event persistence in trace
let recordEventPersistence (eventCount: int) (success: bool) (errorMsg: string option) =
    let activity = Activity.Current
    if activity <> null then
        activity.SetTag("event.count", eventCount) |> ignore
        activity.SetTag("event.persist.success", success) |> ignore
        match errorMsg with
        | Some msg -> activity.SetTag("event.persist.error", msg) |> ignore
        | None -> ()

/// Record projection processing in trace
let recordProjectionProcessing (aggregateType: string) (success: bool) (errorMsg: string option) =
    let activity = Activity.Current
    if activity <> null then
        activity.SetTag("projection.aggregate_type", aggregateType) |> ignore
        activity.SetTag("projection.success", success) |> ignore
        match errorMsg with
        | Some msg -> activity.SetTag("projection.error", msg) |> ignore
        | None -> ()

/// Get current trace context information
let getTraceContext () =
    let activity = Activity.Current
    if activity <> null then
        {|
            TraceId = activity.TraceId.ToString()
            SpanId = activity.SpanId.ToString()
            IsSampled = activity.ActivityTraceFlags.HasFlag(ActivityTraceFlags.Recorded)
        |}
    else
        {|
            TraceId = ""
            SpanId = ""
            IsSampled = false
        |}
