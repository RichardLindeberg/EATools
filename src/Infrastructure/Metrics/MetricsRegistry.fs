/// Central registry for OpenTelemetry metrics instruments
module EATool.Infrastructure.Metrics.MetricsRegistry

open System
open System.Collections.Generic
open System.Diagnostics.Metrics

/// Record holding all meter instances for metrics collection
type MetricsRegistry = {
    /// HTTP server metrics
    HttpRequestCount: Counter<int64>
    HttpRequestDuration: Histogram<double>
    
    /// Command processing metrics
    CommandsProcessed: Counter<int64>
    CommandDuration: Histogram<double>
    
    /// Event store metrics
    EventStoreAppends: Counter<int64>
    EventStoreAppendDuration: Histogram<double>
    EventStoreReads: Counter<int64>
    EventStoreReadDuration: Histogram<double>
    
    /// Projection metrics
    ProjectionEventsProcessed: Counter<int64>
    ProjectionFailures: Counter<int64>
    ProjectionLag: ObservableGauge<int64>
    ProjectionBatchDuration: Histogram<double>
    
    /// Business metrics
    ApplicationsCreated: Counter<int64>
    CapabilitiesCreated: Counter<int64>
    ServersCreated: Counter<int64>
    IntegrationsCreated: Counter<int64>
    OrganizationsCreated: Counter<int64>
    RelationsCreated: Counter<int64>
}

/// Central meter for EATool metrics (version aligned with service)
let eaToolMeter = new Meter("EATool", "1.0.0")

/// Initialize all metrics instruments
let initializeMetrics () : MetricsRegistry =
    {
        /// HTTP Server Metrics (OTel Semantic Convention)
        HttpRequestCount = 
            eaToolMeter.CreateCounter<int64>(
                "http.server.request.count",
                unit = "{request}",
                description = "Number of HTTP requests"
            )
        
        HttpRequestDuration = 
            eaToolMeter.CreateHistogram<double>(
                "http.server.request.duration",
                unit = "ms",
                description = "HTTP request latency"
            )
        
        /// Command Processing Metrics
        CommandsProcessed = 
            eaToolMeter.CreateCounter<int64>(
                "eatool.commands.processed",
                unit = "{command}",
                description = "Number of commands processed"
            )
        
        CommandDuration = 
            eaToolMeter.CreateHistogram<double>(
                "eatool.command.duration",
                unit = "ms",
                description = "Command processing duration"
            )
        
        /// Event Store Metrics
        EventStoreAppends = 
            eaToolMeter.CreateCounter<int64>(
                "eatool.eventstore.appends",
                unit = "{event}",
                description = "Number of event append operations"
            )
        
        EventStoreAppendDuration = 
            eaToolMeter.CreateHistogram<double>(
                "eatool.eventstore.append.duration",
                unit = "ms",
                description = "Event append operation duration"
            )
        
        EventStoreReads = 
            eaToolMeter.CreateCounter<int64>(
                "eatool.eventstore.reads",
                unit = "{event}",
                description = "Number of event read operations"
            )
        
        EventStoreReadDuration = 
            eaToolMeter.CreateHistogram<double>(
                "eatool.eventstore.read.duration",
                unit = "ms",
                description = "Event read operation duration"
            )
        
        /// Projection Metrics
        ProjectionEventsProcessed = 
            eaToolMeter.CreateCounter<int64>(
                "eatool.projection.events.processed",
                unit = "{event}",
                description = "Number of events processed by projections"
            )
        
        ProjectionFailures = 
            eaToolMeter.CreateCounter<int64>(
                "eatool.projection.failures",
                unit = "{failure}",
                description = "Number of projection processing failures"
            )
        
        ProjectionLag = 
            eaToolMeter.CreateObservableGauge<int64>(
                "eatool.projection.lag",
                unit = "{event}",
                description = "Projection lag in events",
                observeValue = fun _ -> Measurement<int64>(0L)
            )
        
        ProjectionBatchDuration = 
            eaToolMeter.CreateHistogram<double>(
                "eatool.projection.batch.duration",
                unit = "ms",
                description = "Projection batch processing duration"
            )
        
        /// Business Metrics - Entity Creation Counters
        ApplicationsCreated = 
            eaToolMeter.CreateCounter<int64>(
                "eatool.applications.created",
                unit = "{application}",
                description = "Number of applications created"
            )
        
        CapabilitiesCreated = 
            eaToolMeter.CreateCounter<int64>(
                "eatool.capabilities.created",
                unit = "{capability}",
                description = "Number of capabilities created"
            )
        
        ServersCreated = 
            eaToolMeter.CreateCounter<int64>(
                "eatool.servers.created",
                unit = "{server}",
                description = "Number of servers created"
            )
        
        IntegrationsCreated = 
            eaToolMeter.CreateCounter<int64>(
                "eatool.integrations.created",
                unit = "{integration}",
                description = "Number of integrations created"
            )
        
        OrganizationsCreated = 
            eaToolMeter.CreateCounter<int64>(
                "eatool.organizations.created",
                unit = "{organization}",
                description = "Number of organizations created"
            )
        
        RelationsCreated = 
            eaToolMeter.CreateCounter<int64>(
                "eatool.relations.created",
                unit = "{relation}",
                description = "Number of relations created"
            )
    }

/// Singleton metrics registry instance
let mutable private metricsInstance : MetricsRegistry option = None

/// Get or initialize the metrics registry
let getMetrics () =
    match metricsInstance with
    | Some metrics -> metrics
    | None ->
        let metrics = initializeMetrics()
        metricsInstance <- Some metrics
        metrics

/// Initialize metrics registry (call once at startup)
let initialize () =
    getMetrics () |> ignore
