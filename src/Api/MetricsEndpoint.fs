module EATool.Api.MetricsEndpoint

open System
open System.Diagnostics.Metrics
open System.Text
open Microsoft.AspNetCore.Http
open Giraffe
open EATool.Infrastructure.Metrics

/// Format Prometheus metric line in text format
/// Format: metric_name{label1="value1",label2="value2"} value timestamp
let private formatPrometheusLine (metricName: string) (labels: (string * string) list) (value: string) =
    if List.isEmpty labels then
        sprintf "%s %s" metricName value
    else
        let labelStr = 
            labels
            |> List.map (fun (k, v) -> sprintf "%s=\"%s\"" k (v.Replace("\"", "\\\"")))
            |> String.concat ","
        sprintf "%s{%s} %s" metricName labelStr value

/// Export metrics in Prometheus text format
let exportMetrics () =
    try
        // Note: System.Diagnostics.Metrics doesn't provide built-in Prometheus export
        // For now, return a simple placeholder that indicates metrics are available
        // In production, use OpenTelemetry PrometheusHttpListener or similar
        let sb = StringBuilder()
        sb.AppendLine("# HELP eatool_up EATool service is up and running") |> ignore
        sb.AppendLine("# TYPE eatool_up gauge") |> ignore
        sb.AppendLine("eatool_up 1") |> ignore
        sb.AppendLine() |> ignore
        
        // Get the metrics registry to show what's available
        let metrics = MetricsRegistry.getMetrics()
        
        // Note: Direct metric value export would require iterating through instrument measurements
        // This is provided by OTel exporters. For now, show that metrics infrastructure is initialized.
        sb.AppendLine("# Metrics registry initialized with instruments:") |> ignore
        sb.AppendLine("# - http.server.request.count") |> ignore
        sb.AppendLine("# - http.server.request.duration") |> ignore
        sb.AppendLine("# - eatool.commands.processed") |> ignore
        sb.AppendLine("# - eatool.commands.duration") |> ignore
        sb.AppendLine("# - eatool.eventstore.appends") |> ignore
        sb.AppendLine("# - eatool.eventstore.append.duration") |> ignore
        sb.AppendLine("# - eatool.eventstore.reads") |> ignore
        sb.AppendLine("# - eatool.eventstore.read.duration") |> ignore
        sb.AppendLine("# - eatool.projections.events_processed") |> ignore
        sb.AppendLine("# - eatool.projections.failures") |> ignore
        sb.AppendLine("# - eatool.projections.lag") |> ignore
        sb.AppendLine("# - eatool.projections.batch.duration") |> ignore
        sb.AppendLine("# - eatool.applications.created") |> ignore
        sb.AppendLine("# - eatool.capabilities.created") |> ignore
        sb.AppendLine("# - eatool.servers.created") |> ignore
        sb.AppendLine("# - eatool.integrations.created") |> ignore
        sb.AppendLine("# - eatool.organizations.created") |> ignore
        sb.AppendLine("# - eatool.relations.created") |> ignore
        
        sb.ToString()
    with ex ->
        sprintf "# Error exporting metrics: %s\n" ex.Message

/// Metrics endpoint handler
let metricsHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        ctx.Response.ContentType <- "text/plain; version=0.0.4; charset=utf-8"
        return! text (exportMetrics ()) next ctx
    }

/// Routes
let routes: HttpHandler list = [
    GET >=> route "/metrics" >=> metricsHandler
]
