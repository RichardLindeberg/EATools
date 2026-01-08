/// Test helpers for asserting observability signals
namespace EATool.Tests.Fixtures

open System
open System.Collections.Generic
open System.Text.RegularExpressions

module OTelTestHelpers =

    /// Assert span exists with given name
    let assertSpanExists (exporter: MockOTelExporter) (spanName: string) : unit =
        let spans = exporter.GetSpansByName(spanName)
        if spans.IsEmpty then
            failwith $"Expected span with name '{spanName}' but none found"

    /// Assert span has specific attribute
    let assertSpanHasAttribute (exporter: MockOTelExporter) (spanName: string) (key: string) (value: obj) : unit =
        let spans = exporter.GetSpansByName(spanName)
        let hasAttribute = 
            spans |> List.exists (fun span ->
                span.Attributes.ContainsKey(key) && 
                (match span.Attributes.[key], value with
                 | v1, v2 when obj.Equals(v1, v2) -> true
                 | _ -> false))
        if not hasAttribute then
            failwith $"Expected span '{spanName}' to have attribute {key}={value}"

    /// Assert span count
    let assertSpanCount (exporter: MockOTelExporter) (expectedCount: int) : unit =
        let actualCount = exporter.GetSpans().Length
        if actualCount <> expectedCount then
            failwith $"Expected {expectedCount} spans but found {actualCount}"

    /// Assert trace has hierarchy (parent-child relationships)
    let assertTraceHierarchy (exporter: MockOTelExporter) (traceId: string) : unit =
        let spans = exporter.GetSpansByTraceId(traceId)
        if spans.IsEmpty then
            failwith $"Expected spans with trace ID '{traceId}' but none found"

    /// Assert root span exists (no parent)
    let assertRootSpanExists (exporter: MockOTelExporter) (traceId: string) : unit =
        let spans = exporter.GetSpansByTraceId(traceId)
        let rootSpan = spans |> List.tryFind (fun s -> s.ParentSpanId.IsNone)
        if rootSpan.IsNone then
            failwith $"Expected root span in trace '{traceId}' but none found"

    /// Assert W3C Trace Context format
    let assertW3CTraceContextFormat (traceId: string) (spanId: string) : unit =
        let traceIdPattern = "^[0-9a-f]{32}$"
        let spanIdPattern = "^[0-9a-f]{16}$"
        
        if not (Regex.IsMatch(traceId, traceIdPattern)) then
            failwith $"Invalid trace ID format: '{traceId}' (expected 32 hex characters)"
        if not (Regex.IsMatch(spanId, spanIdPattern)) then
            failwith $"Invalid span ID format: '{spanId}' (expected 16 hex characters)"

    /// Assert log entry exists with all required fields
    let assertLogHasRequiredFields (exporter: MockOTelExporter) : unit =
        let logs = exporter.GetLogs()
        if logs.IsEmpty then
            failwith "Expected at least one log entry but none found"
        
        let requiredFields = ["timestamp"; "level"; "logger"; "message"]
        logs |> List.iter (fun log ->
            requiredFields |> List.iter (fun field ->
                if not (log.ContainsKey(field)) then
                    failwith $"Expected log to have field '{field}' but it's missing"))

    /// Assert log contains no PII patterns
    let assertLogContainsNoPII (logContent: string) : unit =
        let piiPatterns = [
            // Email pattern
            "[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}"
            // Phone pattern (US) - guard against matches inside longer IDs (e.g., UUIDs)
            "(?<!\\d)\\d{3}[-.]?\\d{3}[-.]?\\d{4}(?!\\d)"
            // Credit card pattern
            "\\d{4}[\\s-]?\\d{4}[\\s-]?\\d{4}[\\s-]?\\d{4}"
            // SSN pattern
            "\\d{3}-\\d{2}-\\d{4}"
        ]
        
        piiPatterns |> List.iter (fun pattern ->
            if Regex.IsMatch(logContent, pattern) then
                failwith $"Detected potential PII in log matching pattern: {pattern}")

    /// Assert metric was recorded
    let assertMetricRecorded (exporter: MockOTelExporter) (metricName: string) : unit =
        let metrics = exporter.GetMetrics()
        let metric = metrics |> List.tryFind (fun m -> 
            m.ContainsKey("name") && (m.["name"] :?> string) = metricName)
        if metric.IsNone then
            failwith $"Expected metric '{metricName}' but it was not recorded"

    /// Assert metric value
    let assertMetricValue (exporter: MockOTelExporter) (metricName: string) (expectedValue: float) : unit =
        let metrics = exporter.GetMetrics()
        let metric = metrics |> List.tryFind (fun m -> 
            m.ContainsKey("name") && (m.["name"] :?> string) = metricName)
        match metric with
        | Some m when m.ContainsKey("value") ->
            let value = m.["value"] :?> float
            if not (System.Math.Abs(value - expectedValue) < 0.001) then
                failwith $"Expected metric '{metricName}' to have value {expectedValue} but got {value}"
        | _ -> failwith $"Could not find metric '{metricName}' with value field"

    /// Assert metric has no high-cardinality labels (no user IDs, request IDs)
    let assertMetricLabelsLowCardinality (labels: Map<string, string>) : unit =
        let highCardinalityPatterns = [
            "user_id"
            "request_id"
            "session_id"
            "customer_id"
            "account_id"
        ]
        
        labels |> Map.iter (fun key value ->
            if highCardinalityPatterns |> List.exists (fun pattern -> 
                key.Contains(pattern, StringComparison.OrdinalIgnoreCase)) then
                failwith $"Detected high-cardinality metric label: {key}")

    /// Assert trace ID is valid format
    let assertTraceIdValid (traceId: string) : unit =
        if not (Regex.IsMatch(traceId, "^[0-9a-f]{32}$")) then
            failwith $"Invalid trace ID: '{traceId}' (expected 32 hex characters)"

    /// Assert span ID is valid format
    let assertSpanIdValid (spanId: string) : unit =
        if not (Regex.IsMatch(spanId, "^[0-9a-f]{16}$")) then
            failwith $"Invalid span ID: '{spanId}' (expected 16 hex characters)"

    /// Assert trace flags are valid (00 or 01)
    let assertTraceFlagsValid (flags: string) : unit =
        if not (flags = "00" || flags = "01") then
            failwith $"Invalid trace flags: '{flags}' (expected '00' or '01')"

    /// Assert no duplicate trace IDs in span batch
    let assertNoDuplicateTraceIds (exporter: MockOTelExporter) : unit =
        let spans = exporter.GetSpans()
        let traceIds = spans |> List.map (fun s -> s.TraceId)
        let uniqueTraceIds = traceIds |> List.distinct
        if traceIds.Length <> uniqueTraceIds.Length then
            failwith $"Found duplicate trace IDs in {traceIds.Length} spans"

    /// Get span by name (for further assertions)
    let getSpanByName (exporter: MockOTelExporter) (spanName: string) : CapturedSpan option =
        exporter.GetSpansByName(spanName) |> List.tryHead

    /// Get all child spans for a parent span
    let getChildSpans (exporter: MockOTelExporter) (parentSpanId: string) : CapturedSpan list =
        let allSpans = exporter.GetSpans()
        allSpans |> List.filter (fun s -> 
            match s.ParentSpanId with
            | Some parentId -> parentId = parentSpanId
            | None -> false)
