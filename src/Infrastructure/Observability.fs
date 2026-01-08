module EATool.Infrastructure.Observability

open System
open System.Collections.Generic
open System.Diagnostics
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open OpenTelemetry
open OpenTelemetry.Trace
open OpenTelemetry.Metrics
open OpenTelemetry.Resources
open OpenTelemetry.Exporter

/// Service information from environment
type ServiceInfo = {
    Name: string
    Version: string
    Environment: string
    InstanceId: string
}

/// OpenTelemetry configuration from environment variables
type OTelConfig = {
    ServiceInfo: ServiceInfo
    OtlpEndpoint: string option
    OtlpHeaders: string option
    EnableConsoleExporter: bool
    TraceSampleRate: float
}

/// Get service information from environment variables
let getServiceInfo () =
    let name = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") |> Option.ofObj |> Option.defaultValue "eatool-api"
    let version = Environment.GetEnvironmentVariable("OTEL_SERVICE_VERSION") |> Option.ofObj |> Option.defaultValue "1.0.0"
    let environment = Environment.GetEnvironmentVariable("OTEL_DEPLOYMENT_ENVIRONMENT") |> Option.ofObj |> Option.defaultValue "development"
    let instanceId = Environment.GetEnvironmentVariable("OTEL_SERVICE_INSTANCE_ID") |> Option.ofObj |> Option.defaultValue (Environment.MachineName)
    
    {
        Name = name
        Version = version
        Environment = environment
        InstanceId = instanceId
    }

/// Get OpenTelemetry configuration from environment
let getOTelConfig () =
    let serviceInfo = getServiceInfo ()
    let otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") |> Option.ofObj
    let otlpHeaders = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_HEADERS") |> Option.ofObj
    let enableConsole = 
        Environment.GetEnvironmentVariable("OTEL_EXPORTER_CONSOLE_ENABLED") 
        |> Option.ofObj 
        |> Option.map (fun s -> s.Equals("true", StringComparison.OrdinalIgnoreCase))
        |> Option.defaultValue false
    
    let sampleRate =
        Environment.GetEnvironmentVariable("OTEL_TRACE_SAMPLE_RATE")
        |> Option.ofObj
        |> Option.bind (fun s -> 
            match Double.TryParse(s) with
            | true, v when v >= 0.0 && v <= 1.0 -> Some v
            | _ -> None)
        |> Option.defaultValue 1.0
    
    {
        ServiceInfo = serviceInfo
        OtlpEndpoint = otlpEndpoint
        OtlpHeaders = otlpHeaders
        EnableConsoleExporter = enableConsole
        TraceSampleRate = sampleRate
    }

/// Create resource attributes for the service
let createResource (serviceInfo: ServiceInfo) =
    ResourceBuilder
        .CreateDefault()
        .AddService(
            serviceName = serviceInfo.Name,
            serviceVersion = serviceInfo.Version,
            serviceInstanceId = serviceInfo.InstanceId)
        .AddAttributes([
            KeyValuePair<string, obj>("deployment.environment", serviceInfo.Environment)
        ])
        .Build()

/// Configure OpenTelemetry tracing
let configureOTelTracing (services: IServiceCollection) =
    let config = getOTelConfig ()
    
    services.AddOpenTelemetry()
        .ConfigureResource(fun rb -> 
            rb.Clear()
              .AddService(
                serviceName = config.ServiceInfo.Name,
                serviceVersion = config.ServiceInfo.Version,
                serviceInstanceId = config.ServiceInfo.InstanceId)
              .AddAttributes([
                KeyValuePair<string, obj>("deployment.environment", config.ServiceInfo.Environment)
              ]) |> ignore)
        .WithTracing(fun builder ->
            // Add standard instrumentations
            builder
                .AddAspNetCoreInstrumentation(fun opts ->
                    opts.RecordException <- true
                    opts.EnrichWithHttpRequest <- fun activity request ->
                        activity.SetTag("http.request_content_length", request.ContentLength) |> ignore
                    opts.EnrichWithHttpResponse <- fun activity response ->
                        activity.SetTag("http.response_content_length", response.ContentLength) |> ignore)
                .AddHttpClientInstrumentation(fun opts ->
                    opts.RecordException <- true
                    opts.EnrichWithHttpRequestMessage <- fun activity request ->
                        activity.SetTag("http.request.method", request.Method.Method) |> ignore
                    opts.EnrichWithHttpResponseMessage <- fun activity response ->
                        activity.SetTag("http.response.status_code", int response.StatusCode) |> ignore)
                .AddSqlClientInstrumentation(fun opts ->
                    opts.SetDbStatementForText <- true
                    opts.RecordException <- true
                    opts.EnableConnectionLevelAttributes <- true)
                |> ignore
            
            // Configure sampler
            let sampler = 
                if config.TraceSampleRate >= 1.0 then
                    new AlwaysOnSampler() :> Sampler
                elif config.TraceSampleRate <= 0.0 then
                    new AlwaysOffSampler() :> Sampler
                else
                    new TraceIdRatioBasedSampler(config.TraceSampleRate) :> Sampler
            
            builder.SetSampler(new ParentBasedSampler(sampler)) |> ignore
            
            // Add exporters
            match config.OtlpEndpoint with
            | Some endpoint ->
                builder.AddOtlpExporter(fun opts ->
                    opts.Endpoint <- Uri(endpoint)
                    opts.Protocol <- OtlpExportProtocol.Grpc
                    match config.OtlpHeaders with
                    | Some headers ->
                        // Parse headers like "key1=value1,key2=value2"
                        headers.Split(',')
                        |> Array.iter (fun header ->
                            match header.Split('=') with
                            | [| key; value |] -> opts.Headers <- opts.Headers + $"{key.Trim()}={value.Trim()}"
                            | _ -> ())
                    | None -> ()) |> ignore
            | None -> ()
            
            if config.EnableConsoleExporter then
                builder.AddConsoleExporter() |> ignore)
        |> ignore
    
    services

/// Configure OpenTelemetry metrics
let configureOTelMetrics (services: IServiceCollection) =
    let config = getOTelConfig ()
    
    services.AddOpenTelemetry()
        .ConfigureResource(fun rb -> 
            rb.Clear()
              .AddService(
                serviceName = config.ServiceInfo.Name,
                serviceVersion = config.ServiceInfo.Version,
                serviceInstanceId = config.ServiceInfo.InstanceId)
              .AddAttributes([
                KeyValuePair<string, obj>("deployment.environment", config.ServiceInfo.Environment)
              ]) |> ignore)
        .WithMetrics(fun builder ->
            // Add standard instrumentations
            builder
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                |> ignore
            
            // Add custom meters
            builder.AddMeter("EATool") |> ignore
            builder.AddMeter("EATool.Commands") |> ignore
            builder.AddMeter("EATool.EventStore") |> ignore
            builder.AddMeter("EATool.Projections") |> ignore
            
            // Add exporters
            match config.OtlpEndpoint with
            | Some endpoint ->
                builder.AddOtlpExporter(fun opts ->
                    opts.Endpoint <- Uri(endpoint)
                    opts.Protocol <- OtlpExportProtocol.Grpc
                    match config.OtlpHeaders with
                    | Some headers ->
                        headers.Split(',')
                        |> Array.iter (fun header ->
                            match header.Split('=') with
                            | [| key; value |] -> opts.Headers <- opts.Headers + $"{key.Trim()}={value.Trim()}"
                            | _ -> ())
                    | None -> ()) |> ignore
            | None -> ()
            
            if config.EnableConsoleExporter then
                builder.AddConsoleExporter() |> ignore)
        |> ignore
    
    services

/// Configure OpenTelemetry logging integration
let configureOTelLogging (loggingBuilder: ILoggingBuilder) =
    let config = getOTelConfig ()
    
    loggingBuilder
        .AddOpenTelemetry(fun opts ->
            opts.IncludeFormattedMessage <- true
            opts.IncludeScopes <- true
            opts.ParseStateValues <- true)
        |> ignore
    
    loggingBuilder

/// Activity source for custom instrumentation
let activitySource = new ActivitySource("EATool", "1.0.0")

/// Create a traced span for an operation
let traceOperation (name: string) (attributes: (string * obj) list) (operation: unit -> 'a) =
    use activity = activitySource.StartActivity(name, ActivityKind.Internal)
    
    match activity with
    | null -> 
        // Tracing not enabled, just run the operation
        operation ()
    | act ->
        // Add attributes
        attributes |> List.iter (fun (key, value) -> act.SetTag(key, value) |> ignore)
        
        try
            let result = operation ()
            act.SetStatus(ActivityStatusCode.Ok) |> ignore
            result
        with ex ->
            act.SetStatus(ActivityStatusCode.Error, ex.Message) |> ignore
            act.RecordException(ex) |> ignore
            reraise ()

/// Start an activity for async operations
let startActivity (name: string) (attributes: (string * obj) list) =
    let activity = activitySource.StartActivity(name, ActivityKind.Internal)
    match activity with
    | null -> None
    | act ->
        attributes |> List.iter (fun (key, value) -> act.SetTag(key, value) |> ignore)
        Some act

/// Complete an activity with success
let completeActivity (activity: Activity option) =
    match activity with
    | Some act ->
        act.SetStatus(ActivityStatusCode.Ok) |> ignore
        act.Dispose()
    | None -> ()

/// Complete an activity with an error
let completeActivityWithError (activity: Activity option) (ex: Exception) =
    match activity with
    | Some act ->
        act.SetStatus(ActivityStatusCode.Error, ex.Message) |> ignore
        act.RecordException(ex) |> ignore
        act.Dispose()
    | None -> ()
