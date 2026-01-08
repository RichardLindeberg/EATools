/// Event store operation metrics
module EATool.Infrastructure.Metrics.EventStoreMetrics

open System.Collections.Generic
open System.Diagnostics.Metrics

/// Record event append operation
let recordAppend (aggregateType: string) (eventCount: int) (durationMs: double) (success: bool) =
    let metrics = MetricsRegistry.getMetrics()
    
    let result = if success then "success" else "failure"
    
    // Record append count
    metrics.EventStoreAppends.Add(
        int64 eventCount,
        KeyValuePair("eatool.aggregate.type", aggregateType :> obj),
        KeyValuePair("eatool.operation.result", result :> obj)
    )
    
    // Record duration
    metrics.EventStoreAppendDuration.Record(
        durationMs,
        KeyValuePair("eatool.aggregate.type", aggregateType :> obj),
        KeyValuePair("eatool.operation.result", result :> obj)
    )

/// Record event read operation
let recordRead (aggregateType: string) (eventCount: int) (durationMs: double) (success: bool) =
    let metrics = MetricsRegistry.getMetrics()
    
    let result = if success then "success" else "failure"
    
    // Record read count
    metrics.EventStoreReads.Add(
        int64 eventCount,
        KeyValuePair("eatool.aggregate.type", aggregateType :> obj),
        KeyValuePair("eatool.operation.result", result :> obj)
    )
    
    // Record duration
    metrics.EventStoreReadDuration.Record(
        durationMs,
        KeyValuePair("eatool.aggregate.type", aggregateType :> obj),
        KeyValuePair("eatool.operation.result", result :> obj)
    )
