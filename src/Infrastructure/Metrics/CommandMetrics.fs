/// Command processing metrics
module EATool.Infrastructure.Metrics.CommandMetrics

open System.Collections.Generic
open System.Diagnostics.Metrics

/// Record command processing metric
let recordCommand (commandType: string) (result: string) (durationMs: double) =
    let metrics = MetricsRegistry.getMetrics()
    
    // Record command count
    metrics.CommandsProcessed.Add(
        1L,
        KeyValuePair("eatool.command.type", commandType :> obj),
        KeyValuePair("eatool.command.result", result :> obj)
    )
    
    // Record duration
    metrics.CommandDuration.Record(
        durationMs,
        KeyValuePair("eatool.command.type", commandType :> obj),
        KeyValuePair("eatool.command.result", result :> obj)
    )

/// Result values for command execution
module CommandResult =
    let success = "success"
    let validationError = "validation_error"
    let concurrencyError = "concurrency_error"
    let unknown = "unknown_error"
