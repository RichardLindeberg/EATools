/// Projection processing metrics
module EATool.Infrastructure.Metrics.ProjectionMetrics

open System.Collections.Generic
open System.Diagnostics.Metrics

/// Record events processed by projection
let recordEventsProcessed (projectionName: string) (eventCount: int) (result: string) =
    let metrics = MetricsRegistry.getMetrics()
    
    metrics.ProjectionEventsProcessed.Add(
        int64 eventCount,
        KeyValuePair("eatool.projection.name", projectionName :> obj),
        KeyValuePair("eatool.projection.result", result :> obj)
    )

/// Record projection processing failure
let recordFailure (projectionName: string) (errorType: string) =
    let metrics = MetricsRegistry.getMetrics()
    
    metrics.ProjectionFailures.Add(
        1L,
        KeyValuePair("eatool.projection.name", projectionName :> obj),
        KeyValuePair("eatool.failure.type", errorType :> obj)
    )

/// Record projection batch processing duration
let recordBatchDuration (projectionName: string) (durationMs: double) (eventCount: int) =
    let metrics = MetricsRegistry.getMetrics()
    
    metrics.ProjectionBatchDuration.Record(
        durationMs,
        KeyValuePair("eatool.projection.name", projectionName :> obj),
        KeyValuePair("eatool.batch.event_count", (eventCount.ToString()) :> obj)
    )

/// Projection result values
module ProjectionResult =
    let success = "success"
    let failure = "failure"
    let skipped = "skipped"
