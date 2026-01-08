/// Mock OpenTelemetry exporter for testing signal collection
namespace EATool.Tests.Fixtures

open System
open System.Collections.Generic
open System.Diagnostics
// Lightweight mock; no dependency on OpenTelemetry SDK

/// Captured trace span for testing
type CapturedSpan = {
    TraceId: string
    SpanId: string
    ParentSpanId: string option
    Name: string
    StartTimeUtc: DateTime
    EndTimeUtc: DateTime
    Status: ActivityStatusCode
    Attributes: Dictionary<string, obj>
    Events: (string * DateTime * Dictionary<string, obj>) list
}

/// Mock collector that stores spans/logs/metrics in memory
type MockOTelExporter() =
    let spans = List<CapturedSpan>()
    let logs = List<Map<string, obj>>()
    let metrics = List<Map<string, obj>>()

    /// Add a captured activity/span to the in-memory store
    member this.AddActivity(activity: Activity) : unit =
        let parentSpanId =
            if activity.ParentSpanId = ActivitySpanId() then None
            else Some (activity.ParentSpanId.ToString())

        let capturedSpan: CapturedSpan = {
            TraceId = activity.TraceId.ToString()
            SpanId = activity.SpanId.ToString()
            ParentSpanId = parentSpanId
            Name = activity.DisplayName
            StartTimeUtc = activity.StartTimeUtc
            EndTimeUtc = activity.StartTimeUtc.Add(activity.Duration)
            Status = activity.Status
            Attributes = Dictionary<string, obj>()
            Events = []
        }

        // Copy attributes
        activity.TagObjects
        |> Seq.iter (fun kvp -> capturedSpan.Attributes.Add(kvp.Key, kvp.Value))

        spans.Add(capturedSpan)

    /// Get all captured spans
    member this.GetSpans() : CapturedSpan list =
        spans |> Seq.toList

    /// Get span by name
    member this.GetSpansByName(name: string) : CapturedSpan list =
        spans |> Seq.filter (fun s -> s.Name = name) |> Seq.toList

    /// Get span by trace ID
    member this.GetSpansByTraceId(traceId: string) : CapturedSpan list =
        spans |> Seq.filter (fun s -> s.TraceId = traceId) |> Seq.toList

    /// Get span with specific attribute
    member this.GetSpansWithAttribute(key: string, value: obj) : CapturedSpan list =
        spans 
        |> Seq.filter (fun s -> 
            s.Attributes.ContainsKey(key) && 
            (match s.Attributes.[key], value with
             | v1, v2 when obj.Equals(v1, v2) -> true
             | _ -> false))
        |> Seq.toList

    /// Get all captured logs
    member this.GetLogs() : Map<string, obj> list =
        logs |> Seq.toList

    /// Add captured log
    member internal this.AddLog(log: Map<string, obj>) : unit =
        logs.Add(log)

    /// Get all captured metrics
    member this.GetMetrics() : Map<string, obj> list =
        metrics |> Seq.toList

    /// Add captured metric
    member internal this.AddMetric(metric: Map<string, obj>) : unit =
        metrics.Add(metric)

    /// Clear all captured signals
    member this.Clear() : unit =
        spans.Clear()
        logs.Clear()
        metrics.Clear()

    /// Get summary statistics
    member this.GetStatistics() : {| SpanCount: int; LogCount: int; MetricCount: int |} =
        {|
            SpanCount = spans.Count
            LogCount = logs.Count
            MetricCount = metrics.Count
        |}
