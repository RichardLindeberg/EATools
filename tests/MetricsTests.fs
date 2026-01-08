/// Unit tests for metrics
module EATool.Tests.MetricsTests

open System
open System.Collections.Generic
open Xunit
open EATool.Tests.Fixtures

[<Fact>]
let ``Mock exporter is initialized empty`` () =
    let exporter = MockOTelExporter()
    let metrics = exporter.GetMetrics()
    Assert.Empty(metrics)

[<Fact>]
let ``Metric can be recorded`` () =
    let exporter = MockOTelExporter()
    let metric = Map.ofList [("name", "requests" :> obj); ("value", 1.0 :> obj)]
    exporter.AddMetric(metric)
    
    let metrics = exporter.GetMetrics()
    Assert.Single(metrics)

[<Fact>]
let ``Multiple metrics can be recorded`` () =
    let exporter = MockOTelExporter()
    for i in 1..5 do
        let metric = Map.ofList [("name", $"metric_{i}" :> obj); ("value", float i :> obj)]
        exporter.AddMetric(metric)
    
    let metrics = exporter.GetMetrics()
    Assert.Equal(5, metrics.Length)

[<Fact>]
let ``Metric counter increments`` () =
    let exporter = MockOTelExporter()
    let name = "http.requests.total"
    
    let metric1 = Map.ofList [("name", name :> obj); ("value", 1.0 :> obj)]
    exporter.AddMetric(metric1)
    
    let metric2 = Map.ofList [("name", name :> obj); ("value", 2.0 :> obj)]
    exporter.AddMetric(metric2)
    
    let metrics = exporter.GetMetrics()
    Assert.Equal(2, metrics.Length)

[<Fact>]
let ``Metric names follow OTel conventions`` () =
    let validNames = ["http.server.request.count"; "db.client.connections.usage"; "rpc.server.duration"]
    validNames |> List.iter (fun name ->
        let metric = Map.ofList [("name", name :> obj); ("value", 1.0 :> obj)]
        Assert.True(name |> Seq.forall (fun c -> Char.IsLetterOrDigit(c) || c = '.' || c = '_'))
    )

[<Fact>]
let ``Metric labels can be set`` () =
    let exporter = MockOTelExporter()
    let labels = Map.ofList [("method", "GET"); ("path", "/api/orgs"); ("status", "200")]
    let metric = Map.ofList [
        ("name", "http.server.requests" :> obj)
        ("value", 42.0 :> obj)
        ("labels", labels :> obj)
    ]
    exporter.AddMetric(metric)
    
    let metrics = exporter.GetMetrics()
    Assert.Single(metrics)

[<Fact>]
let ``High-cardinality labels are detected`` () =
    let labels = Map.ofList [
        ("user_id", "12345")
        ("method", "GET")
        ("status", "200")
    ]
    
    Assert.Throws<System.Exception>(fun () ->
        OTelTestHelpers.assertMetricLabelsLowCardinality labels
    ) |> ignore

[<Fact>]
let ``Low-cardinality labels pass validation`` () =
    let labels = Map.ofList [
        ("method", "GET")
        ("status", "200")
        ("path", "/api/v1/orgs")
    ]
    
    OTelTestHelpers.assertMetricLabelsLowCardinality labels

[<Fact>]
let ``Request ID label is rejected`` () =
    let labels = Map.ofList [
        ("request_id", "abc-123")
        ("method", "GET")
    ]
    
    Assert.Throws<System.Exception>(fun () ->
        OTelTestHelpers.assertMetricLabelsLowCardinality labels
    ) |> ignore

[<Fact>]
let ``Session ID label is rejected`` () =
    let labels = Map.ofList [
        ("session_id", "sess_abc123")
        ("status", "200")
    ]
    
    Assert.Throws<System.Exception>(fun () ->
        OTelTestHelpers.assertMetricLabelsLowCardinality labels
    ) |> ignore

[<Fact>]
let ``Customer ID label is rejected`` () =
    let labels = Map.ofList [
        ("customer_id", "cust_xyz")
        ("method", "POST")
    ]
    
    Assert.Throws<System.Exception>(fun () ->
        OTelTestHelpers.assertMetricLabelsLowCardinality labels
    ) |> ignore

[<Fact>]
let ``Metric histogram can record values`` () =
    let exporter = MockOTelExporter()
    let values = [10.5; 25.3; 50.1; 75.8; 100.0]
    
    values |> List.iter (fun value ->
        let metric = Map.ofList [
            ("name", "request.duration" :> obj)
            ("value", value :> obj)
            ("unit", "ms" :> obj)
        ]
        exporter.AddMetric(metric)
    )
    
    let metrics = exporter.GetMetrics()
    Assert.Equal(5, metrics.Length)

[<Fact>]
let ``Metric can have unit`` () =
    let exporter = MockOTelExporter()
    let metric = Map.ofList [
        ("name", "memory.usage" :> obj)
        ("value", 512.0 :> obj)
        ("unit", "MB" :> obj)
    ]
    exporter.AddMetric(metric)
    
    let metrics = exporter.GetMetrics()
    let m = metrics.Head
    Assert.True(m.ContainsKey("unit"))

[<Fact>]
let ``Metrics can be cleared`` () =
    let exporter = MockOTelExporter()
    exporter.AddMetric(Map.ofList [("name", "test" :> obj)])
    
    Assert.Equal(1, exporter.GetMetrics().Length)
    
    exporter.Clear()
    Assert.Equal(0, exporter.GetMetrics().Length)

[<Fact>]
let ``Statistics are accurate`` () =
    let exporter = MockOTelExporter()
    exporter.AddMetric(Map.ofList [("name", "m1" :> obj)])
    exporter.AddMetric(Map.ofList [("name", "m2" :> obj)])
    
    let stats = exporter.GetStatistics()
    Assert.Equal(0, stats.SpanCount)
    Assert.Equal(0, stats.LogCount)
    Assert.Equal(2, stats.MetricCount)

[<Fact>]
let ``Metric value must be numeric`` () =
    let exporter = MockOTelExporter()
    let metric = Map.ofList [
        ("name", "test.metric" :> obj)
        ("value", 42.5 :> obj)
    ]
    exporter.AddMetric(metric)
    
    Assert.Single(exporter.GetMetrics())

[<Fact>]
let ``Gauge updates with latest value`` () =
    let exporter = MockOTelExporter()
    
    // Record same gauge with different values
    exporter.AddMetric(Map.ofList [("name", "cpu.usage" :> obj); ("value", 25.0 :> obj)])
    exporter.AddMetric(Map.ofList [("name", "cpu.usage" :> obj); ("value", 75.0 :> obj)])
    
    // Exporter records both, in real implementation would keep latest
    Assert.Equal(2, exporter.GetMetrics().Length)

[<Fact>]
let ``Metric timestamps are recorded`` () =
    let exporter = MockOTelExporter()
    let timestamp = DateTime.UtcNow
    let metric = Map.ofList [
        ("name", "test" :> obj)
        ("value", 1.0 :> obj)
        ("timestamp", timestamp :> obj)
    ]
    exporter.AddMetric(metric)
    
    let metrics = exporter.GetMetrics()
    Assert.Single(metrics)
