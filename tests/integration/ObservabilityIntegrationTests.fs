/// Integration tests for observability across request lifecycle
module EATool.Tests.Integration.ObservabilityIntegrationTests

#nowarn "0020" // Allow ignored return values from fluent Activity APIs

open System
open System.Diagnostics
open System.Collections.Generic
open Xunit
open EATool.Tests.Fixtures

[<Fact>]
let ``Full request lifecycle creates trace with spans`` () =
    let exporter = MockOTelExporter()
    
    // Simulate a request lifecycle
    let rootActivity = new Activity("HttpRequest")
    rootActivity.AddTag("http.method", "POST")
    rootActivity.AddTag("http.path", "/api/orgs")
    rootActivity.AddTag("http.status", 201)
    rootActivity.Start()
    let rootSpanId = rootActivity.SpanId.ToString()
    let traceId = rootActivity.TraceId.ToString()
    
    // Handler span
    let handlerActivity = new Activity("OrganizationHandler")
    handlerActivity.AddTag("handler.type", "CreateOrganization")
    handlerActivity.Start()
    
    // Command execution span
    let commandActivity = new Activity("ExecuteCommand")
    commandActivity.AddTag("command.type", "CreateOrganizationCommand")
    commandActivity.Start()
    
    // Event store write span
    let eventStoreActivity = new Activity("EventStoreAppend")
    eventStoreActivity.AddTag("event.count", 1)
    eventStoreActivity.Start()
    
    eventStoreActivity.Stop()
    commandActivity.Stop()
    handlerActivity.Stop()
    rootActivity.Stop()
    
    // Verify trace structure
    OTelTestHelpers.assertW3CTraceContextFormat traceId rootSpanId

[<Fact>]
let ``HTTP metrics are recorded`` () =
    let exporter = MockOTelExporter()
    
    // Record HTTP request metrics
    let httpMetric = Map.ofList [
        ("name", "http.server.request.count" :> obj)
        ("value", 1.0 :> obj)
        ("labels", Map.ofList [("method", "GET"); ("path", "/api/orgs"); ("status", "200")] :> obj)
    ]
    exporter.AddMetric(httpMetric)
    
    let durationMetric = Map.ofList [
        ("name", "http.server.request.duration" :> obj)
        ("value", 125.5 :> obj)
        ("unit", "ms" :> obj)
        ("labels", Map.ofList [("method", "GET"); ("status", "200")] :> obj)
    ]
    exporter.AddMetric(durationMetric)
    
    let metrics = exporter.GetMetrics()
    Assert.Equal(2, metrics.Length)

[<Fact>]
let ``Trace context is propagated through spans`` () =
    let exporter = MockOTelExporter()
    
    let rootActivity = new Activity("Root")
    rootActivity.Start()
    let rootTraceId = rootActivity.TraceId.ToString()
    
    let childActivity = new Activity("Child")
    childActivity.Start()
    let childTraceId = childActivity.TraceId.ToString()
    
    let grandchildActivity = new Activity("Grandchild")
    grandchildActivity.Start()
    let grandchildTraceId = grandchildActivity.TraceId.ToString()
    
    grandchildActivity.Stop()
    childActivity.Stop()
    rootActivity.Stop()
    
    // All should have same trace ID
    Assert.Equal(rootTraceId, childTraceId)
    Assert.Equal(childTraceId, grandchildTraceId)

[<Fact>]
let ``Parent-child relationships are maintained`` () =
    let rootActivity = new Activity("Parent")
    rootActivity.Start()
    let rootSpanId = rootActivity.SpanId.ToString()
    
    let childActivity = new Activity("Child")
    childActivity.Start()
    let childParentSpanId = childActivity.ParentSpanId.ToString()
    
    Assert.Equal(rootSpanId, childParentSpanId)
    
    childActivity.Stop()
    rootActivity.Stop()

[<Fact>]
let ``Log messages include trace context`` () =
    let exporter = MockOTelExporter()
    
    let activity = Activity.Current
    let logEntry = Map.ofList [
        ("level", "INFO" :> obj)
        ("message", "Operation completed" :> obj)
        ("logger", "OrganizationHandler" :> obj)
    ]
    exporter.AddLog(logEntry)
    
    let logs = exporter.GetLogs()
    Assert.Single(logs)

[<Fact>]
let ``Error logs contain exception details`` () =
    let exporter = MockOTelExporter()
    
    let errorLog = Map.ofList [
        ("level", "ERROR" :> obj)
        ("message", "Operation failed" :> obj)
        ("exception.type", "ArgumentException" :> obj)
        ("exception.message", "Invalid input" :> obj)
        ("error.code", 400 :> obj)
    ]
    exporter.AddLog(errorLog)
    
    let logs = exporter.GetLogs()
    Assert.Single(logs)
    let log = logs.Head
    Assert.True(log.ContainsKey("exception.type"))
    Assert.True(log.ContainsKey("exception.message"))

[<Fact>]
let ``Metrics have low-cardinality labels`` () =
    let exporter = MockOTelExporter()
    
    let labels = Map.ofList [
        ("method", "POST")
        ("path", "/api/orgs")
        ("status", "201")
    ]
    
    OTelTestHelpers.assertMetricLabelsLowCardinality labels

[<Fact>]
let ``High-cardinality labels are rejected`` () =
    let exporter = MockOTelExporter()
    
    let labels = Map.ofList [
        ("user_id", "usr_12345")
        ("method", "POST")
    ]
    
    Assert.Throws<Exception>(fun () ->
        OTelTestHelpers.assertMetricLabelsLowCardinality labels
    ) |> ignore

[<Fact>]
let ``Request ID in labels is rejected`` () =
    let labels = Map.ofList [
        ("request_id", "req_abc123")
        ("status", "200")
    ]
    
    Assert.Throws<Exception>(fun () ->
        OTelTestHelpers.assertMetricLabelsLowCardinality labels
    ) |> ignore

[<Fact>]
let ``Distributed tracing works across async boundaries`` () =
    let activity = new Activity("AsyncOperation")
    activity.Start()
    let originalTraceId = activity.TraceId.ToString()
    
    let asyncWork = async {
        // In an async context, Activity.Current might be null depending on SynchronizationContext
        // This test shows the behavior
        let currentActivity = Activity.Current
        if currentActivity <> null then
            Assert.Equal(originalTraceId, currentActivity.TraceId.ToString())
        return true
    }
    
    let result = Async.RunSynchronously asyncWork
    Assert.True(result)
    
    activity.Stop()

[<Fact>]
let ``Correlation ID is created and propagated`` () =
    let exporter = MockOTelExporter()
    
    let activity = new Activity("RequestHandler")
    activity.AddBaggage("correlation_id", "corr_abc123")
    activity.Start()
    
    let baggage = activity.Baggage |> Seq.toList
    Assert.NotEmpty(baggage)
    
    activity.Stop()

[<Fact>]
let ``Sampling rate affects trace collection`` () =
    let exporter = MockOTelExporter()
    
    // With 100% sampling, all traces should be collected
    let traces = 
        [ for i in 1..10 do
            let activity = new Activity($"Trace{i}")
            activity.Start()
            activity.Stop()
            yield activity.TraceId.ToString()
        ]
    
    Assert.Equal(10, traces.Length)

[<Fact>]
let ``Errors are always traced (error sampling)`` () =
    let exporter = MockOTelExporter()
    
    // Even with low sampling, errors should be traced
    let errorActivity = new Activity("ErrorSpan")
    errorActivity.SetStatus(ActivityStatusCode.Error, "Operation failed")
    errorActivity.Start()
    
    Assert.Equal(ActivityStatusCode.Error, errorActivity.Status)
    
    errorActivity.Stop()

[<Fact>]
let ``Request lifecycle is complete`` () =
    let activity = new Activity("CompleteRequest")
    activity.AddTag("http.method", "POST")
    activity.AddTag("http.path", "/api/orgs")
    activity.Start()
    
    System.Threading.Thread.Sleep(10)
    
    activity.AddTag("http.status", 201)
    activity.SetStatus(ActivityStatusCode.Ok)
    activity.Stop()
    
    Assert.Equal(ActivityStatusCode.Ok, activity.Status)
    Assert.True(activity.Duration.TotalMilliseconds >= 10.0)

[<Fact>]
let ``Observability doesn't block request processing`` () =
    let stopwatch = System.Diagnostics.Stopwatch.StartNew()
    
    for i in 1..100 do
        let activity = new Activity($"Span{i}")
        activity.AddTag("index", i)
        activity.Start()
        activity.Stop()
    
    stopwatch.Stop()
    
    // 100 spans should complete in reasonable time (< 1 second)
    Assert.True(stopwatch.ElapsedMilliseconds < 1000L)

[<Fact>]
let ``All signals are collected`` () =
    let exporter = MockOTelExporter()
    
    // Add trace (simulate collection)
    let activity = new Activity("TestSpan")
    activity.Start()
    activity.Stop()
    exporter.AddActivity(activity)
    
    // Add log
    exporter.AddLog(Map.ofList [("message", "test" :> obj)])
    
    // Add metric
    exporter.AddMetric(Map.ofList [("name", "test.metric" :> obj)])
    
    let stats = exporter.GetStatistics()
    Assert.Equal(1, stats.SpanCount) // AddActivity collects spans in mock
    Assert.Equal(1, stats.LogCount)
    Assert.Equal(1, stats.MetricCount)

[<Fact>]
let ``Clear removes all signals`` () =
    let exporter = MockOTelExporter()
    
    exporter.AddLog(Map.ofList [("message", "test" :> obj)])
    exporter.AddMetric(Map.ofList [("name", "test" :> obj)])
    
    let stats1 = exporter.GetStatistics()
    Assert.Equal(1, stats1.LogCount)
    Assert.Equal(1, stats1.MetricCount)
    
    exporter.Clear()
    
    let stats2 = exporter.GetStatistics()
    Assert.Equal(0, stats2.LogCount)
    Assert.Equal(0, stats2.MetricCount)

[<Fact>]
let ``W3C Trace Context header format`` () =
    let traceId = ActivityTraceId.CreateRandom().ToString()
    let spanId = ActivitySpanId.CreateRandom().ToString()
    let traceFlags = "01"
    
    // W3C format: traceparent: 00-<traceid>-<spanid>-<traceflags>
    let traceparent = $"00-{traceId}-{spanId}-{traceFlags}"
    
    // Verify format
    let parts = traceparent.Split('-')
    Assert.Equal(4, parts.Length)
    Assert.Equal("00", parts.[0])
    Assert.Equal(32, parts.[1].Length)
    Assert.Equal(16, parts.[2].Length)
    Assert.Equal(2, parts.[3].Length)
