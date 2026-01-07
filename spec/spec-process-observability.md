---
title: Monitoring & Observability Specification
version: 1.0
date_created: 2026-01-07
last_updated: 2026-01-07
owner: EA Platform Team
tags: [observability, logging, metrics, tracing, diagnostics, monitoring, process, opentelemetry, otel]
---

# Monitoring & Observability Specification

## Introduction

This specification defines the observability requirements, best practices, and implementation standards for the EA Tool platform. All observability implementations **MUST be OpenTelemetry (OTel) compliant** to ensure standards-based instrumentation, vendor-neutral signal collection, and interoperability with production-grade observability backends.

The specification encompasses logging, metrics, distributed tracing, and alerting—enabling teams to understand system behavior, diagnose issues, and monitor operational health in production environments.

## 1. Purpose & Scope

**Purpose**: Establish unified OpenTelemetry-based observability standards ensuring:
- Consistent structured logging across all components using OTel logging API
- Production-grade metrics collection via OpenTelemetry Metrics API
- Distributed tracing with OTel trace context propagation (W3C standard)
- Timely alerts and incident detection on aggregated observability signals
- Operational dashboards for system health monitoring
- Vendor-neutral implementation enabling backend switching without code changes

**Scope**: Applies to:
- All backend services (API, event store, projections)
- Integration points and external service calls
- Database operations and performance-critical paths
- Event processing and command handling
- HTTP request/response cycles

**Audience**: Backend developers, DevOps engineers, platform operators, incident responders

**Assumptions**:
- OpenTelemetry .NET SDK is integrated into the application
- Logs are aggregated centrally via OTel collector or direct exporter (ELK, Datadog, CloudWatch)
- Metrics are scraped by a time-series database via OTel Prometheus exporter
- Distributed tracing uses OTel OTLP protocol for trace backend (Jaeger, Tempo, Datadog)
- Observability backend supports W3C Trace Context standard

## 2. Definitions

| Term | Definition |
|------|-----------|
| **OpenTelemetry** | CNCF standard for generating, collecting, and exporting observability signals (logs, metrics, traces) |
| **Instrumentation** | Adding OTel API calls to application code to generate observability signals |
| **Structured Logging** | Machine-readable log format (JSON) with consistent key-value pairs, emitted via OTel logging API |
| **Log Level** | Severity classification per OTel spec: TRACE, DEBUG, INFO, WARN, ERROR, FATAL |
| **Metrics** | Quantitative measurements via OTel Metrics API: counters, gauges, histograms, summaries |
| **Cardinality** | Number of unique label value combinations; high cardinality increases storage costs |
| **Trace** | Complete request flow across service boundaries with contextual spans, per OTel trace model |
| **Span** | Single operation within a trace with timing and metadata, per OTel semantic conventions |
| **Correlation ID** | Unique identifier linking all logs, metrics, spans for a single request (maps to trace ID) |
| **Context Propagation** | Passing trace context via W3C Trace Context headers across service calls |
| **OTLP** | OpenTelemetry Protocol—standard gRPC/HTTP protocol for exporting signals to backend |
| **Semantic Conventions** | OTel-defined standard attribute names for consistent instrumentation across services |
| **Sampler** | Strategy for deciding which traces to collect (100%, error-only, probabilistic) |

## 3. Requirements, Constraints & Guidelines

### 3.1 OpenTelemetry Core Requirements

- **OTL-001**: All observability instrumentation MUST use OpenTelemetry .NET SDK (System.Diagnostics.DiagnosticSource, OpenTelemetry.Api)
- **OTL-002**: All log, metric, and trace exporters MUST be OTel-compliant; direct proprietary APIs are forbidden
- **OTL-003**: Trace context propagation MUST use W3C Trace Context standard (`traceparent` and `tracestate` headers)
- **OTL-004**: All span and metric attributes MUST conform to OTel Semantic Conventions for .NET
- **OTL-005**: The application MUST expose an `/health` endpoint returning OTel-compatible service health status
- **OTL-006**: Configuration of observability backends (exporter URLs, API keys) MUST be externalized and NOT hardcoded

### 3.2 Logging Requirements

- **LOG-001**: All logs MUST be emitted via OTel `ILogger` interface (System.Diagnostics.Abstractions)
- **LOG-002**: All logs MUST be structured in JSON format with consistent OTel schema
- **LOG-003**: Every log entry MUST include correlation ID (maps to OTel trace ID) linking related operations
- **LOG-004**: Logs MUST NOT contain personally identifiable information (PII) or sensitive data
- **LOG-005**: All module-level functions MUST log entry and exit points with duration using OTel structured logging
- **LOG-006**: Error logs MUST include stack traces and contextual data for debugging via OTel exception attributes
- **LOG-007**: Command execution MUST be logged with command type, aggregate ID, timestamp, and result
- **LOG-008**: Event persistence MUST be logged with event count, total size, and commit time
- **LOG-009**: External service calls (HTTP, database) MUST log request/response metadata and latency
- **LOG-010**: Log levels MUST be appropriate to message severity; DEBUG logs MUST NOT appear in production without explicit opt-in
- **LOG-011**: Sensitive operational changes MUST be logged to audit trail with appropriate classification

### 3.3 Metrics Requirements

- **MET-001**: All metrics MUST be emitted via OTel `Meter` API (System.Diagnostics.Metrics)
- **MET-002**: Metrics MUST conform to OTel Semantic Conventions for .NET (e.g., `http.server.request.duration`, `db.client.operation.duration`)
- **MET-003**: All HTTP endpoints MUST expose request count, duration, and error rate metrics
- **MET-004**: Metrics MUST use lowercase with underscores; names MUST be descriptive and follow OTel naming conventions
- **MET-005**: High-cardinality labels (user IDs, request IDs) MUST NOT be used in metrics
- **MET-006**: Event store operations MUST track: appends, reads, projection failures, latency via OTel metrics
- **MET-007**: Database operations MUST expose connection pool stats, query latency, slow query counts via OTel
- **MET-008**: Command processing MUST track: commands received, succeeded, failed, processing time
- **MET-009**: Projection updates MUST track: events processed, failures, lag behind event store
- **MET-010**: External integrations MUST track: call count, success rate, latency, timeout/error breakdown
- **MET-011**: Memory and CPU usage MUST be exposed via OTel runtime metrics instrumentation
- **MET-012**: Custom business metrics MUST be defined per domain aggregate using OTel Meter API

### 3.4 Distributed Tracing Requirements

- **TRC-001**: All tracing MUST use OTel Trace API (System.Diagnostics.DiagnosticSource with ActivitySource)
- **TRC-002**: Every request MUST generate an OTel trace with a unique trace ID per W3C Trace Context
- **TRC-003**: Trace context propagation MUST use W3C Trace Context headers (traceparent, tracestate)
- **TRC-004**: Each significant operation MUST create an OTel span with operation name, start/end time, and status
- **TRC-005**: Span attributes MUST follow OTel Semantic Conventions; use standard names (e.g., `http.method`, `db.statement`, `messaging.system`)
- **TRC-006**: Database queries, external API calls, and event operations MUST be instrumented as child spans
- **TRC-007**: Sampling strategy MUST be configurable via OTel sampler; support ParentBasedSampler and ProbabilitySampler
- **TRC-008**: Trace data retention MUST be configurable per environment (7 days prod, 30 days staging)
- **TRC-009**: Span status MUST be set correctly (Unset, Ok, Error) per OTel conventions

### 3.5 Instrumentation Requirements

- **INS-001**: Automatic instrumentation MUST use OTel instrumentations for ASP.NET Core, Entity Framework, HTTP client, and SQL Client
- **INS-002**: Manual instrumentation MUST use `ActivitySource` for custom business operations
- **INS-003**: All external HTTP calls MUST be instrumented with OTel HTTP client instrumentation
- **INS-004**: All database operations MUST be instrumented with OTel SQL instrumentation
- **INS-005**: All async operations MUST preserve trace context across await boundaries via AsyncLocal storage

### 3.6 Alerting Requirements

- **ALR-001**: Alert rules MUST be defined for all critical paths: health checks, error rates, latency anomalies
- **ALR-002**: Alert thresholds MUST be environment-specific (dev, staging, prod)
- **ALR-003**: Alerts MUST route to appropriate teams with severity classification (critical, high, medium, low)
- **ALR-004**: Repeated alerts MUST be deduplicated within a time window to prevent notification fatigue
- **ALR-005**: Alert conditions MUST require sustained threshold breach (e.g., 5+ minute window) to avoid flaps

### 3.7 Constraints

- **CON-001**: Logging must not impact request latency by more than 5% in 99th percentile
- **CON-002**: Metrics emission must be asynchronous to prevent blocking request handling
- **CON-003**: Trace sampling must limit overhead for production systems; 100% sampling only for errors or troubleshooting
- **CON-004**: PII, secrets, and sensitive data must NEVER be logged, metricated, or traced
- **CON-005**: Log retention must comply with data governance and compliance requirements (per environment)
- **CON-006**: OTel exporter must not interfere with application startup; export failures MUST not block service availability

### 3.8 Guidelines

- **GUD-001**: Use correlation IDs (map to OTel trace IDs) to trace the complete lifecycle of user requests across services
- **GUD-002**: Log at INFO level for business events; DEBUG for developer diagnostics
- **GUD-003**: Include context in error logs: what operation, on which entity, with what input
- **GUD-004**: Use OTel Semantic Convention names for all spans and metrics
- **GUD-005**: Label metrics cautiously; avoid unbounded dimensions (use aggregation on the query side)
- **GUD-006**: Trace distributed flows end-to-end; start traces at request boundary, propagate through all calls
- **GUD-007**: Create dashboards for service owners to self-serve operational visibility
- **GUD-008**: Alert on symptoms (latency, error rate), not causes (CPU), for better signal-to-noise ratio
- **GUD-009**: Review and tune alerts quarterly; acknowledge and update rules based on incident patterns
- **GUD-010**: Document metric meanings and dashboard logic in runbooks for on-call engineers
- **GUD-011**: Use ActivitySource for all custom business instrumentation; avoid direct DiagnosticListener usage

## 4. Interfaces & Data Contracts

### 4.1 Structured Log Schema (OTel Compliant)

All logs MUST conform to this JSON schema (OTel log format):

```json
{
  "timestamp": "2026-01-07T10:30:45.123Z",
  "level": "INFO",
  "logger": "EATool.Api.Handlers",
  "message": "Application created successfully",
  "trace_id": "4bf92f3577b34da6a3ce929d0e0e4736",
  "span_id": "00f067aa0ba902b7",
  "trace_flags": "01",
  "service.name": "eatool-api",
  "service.instance.id": "api-pod-01",
  "deployment.environment": "production",
  "user.id": "user-550e8400-e29b-41d4-a716-446655440003",
  "http.request.id": "http-550e8400",
  "otel.library.name": "EATool.Api.Handlers",
  "otel.library.version": "1.0.0",
  "attributes": {
    "http.method": "POST",
    "http.target": "/api/applications",
    "http.status_code": 201,
    "http.response_content_length": 256,
    "db.operation.duration_ms": 45,
    "db.system": "postgresql",
    "application.name": "SAP ERP",
    "domain": "business",
    "aggregate.id": "app-550e8400-e29b-41d4-a716-446655440004",
    "aggregate.type": "Application"
  },
  "error": null
}
```

**Field Definitions** (OTel-compliant):
- `timestamp`: ISO 8601 UTC timestamp when log was created
- `level`: Log level per OTel spec (TRACE, DEBUG, INFO, WARN, ERROR, FATAL)
- `logger`: Fully qualified class/module name
- `message`: Human-readable message describing the event
- `trace_id`: OTel trace ID (32 hex characters)
- `span_id`: OTel span ID (16 hex characters)
- `trace_flags`: OTel trace flags (sampled: 01, not sampled: 00)
- `service.name`: OTel semantic convention for service name
- `service.instance.id`: OTel semantic convention for instance identifier
- `deployment.environment`: OTel semantic convention for environment
- `user.id`: Unique identifier for user (no PII)
- `http.request.id`: HTTP request ID
- `attributes`: Object with OTel semantic convention attributes

**Error Object** (when level is WARN or ERROR):
```json
{
  "error": {
    "type": "InvalidArgumentException",
    "message": "Application name cannot be empty",
    "stack_trace": "at EATool.Api.Handlers.createApplication(...)",
    "error_code": "ERR-001-INVALID-ARG"
  }
}
```

### 4.2 Metrics Schema (OTel Semantic Conventions)

All metrics MUST use OTel Semantic Convention names. Examples:

```
http.server.request.duration
http.server.request.body.size
http.client.request.duration
db.client.operation.duration
messaging.publish.duration
rpc.server.duration
```

**OTel Metric Types**:
- `Counter`: Monotonically increasing value (e.g., request count, error count)
- `Gauge`: Current value that can increase or decrease (e.g., active connections, memory usage)
- `Histogram`: Distribution of values (e.g., request latency, message size)

**Example Metric Declaration (F#)**:
```fsharp
let meter = new Meter("EATool.Api", "1.0.0")
let httpRequestCounter = meter.CreateCounter<long>("http.server.request.count")
let httpDurationHistogram = meter.CreateHistogram<double>("http.server.request.duration", "ms")
let dbConnectionGauge = meter.CreateObservableGauge<int>("db.client.connections.current", fun () -> ...)
```

**Standard Label/Attribute Names** (OTel Semantic Conventions):
- `http.method`: GET, POST, PUT, DELETE, PATCH
- `http.target`: Request path and query
- `http.status_code`: Response status code
- `db.system`: postgresql, mysql, mssql, etc.
- `db.statement`: SQL query (redacted of values)
- `messaging.system`: kafka, rabbitmq, etc.
- `rpc.system`: grpc, etc.
- `error.type`: Exception type name

### 4.3 Trace Context Propagation (W3C Standard)

Use W3C Trace Context format in HTTP headers (OTel standard):

```
traceparent: 00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01
tracestate: vendor_specific_data
```

Format: `00-{trace_id}-{parent_span_id}-{flags}`
- `trace_id`: 32 hex characters (128 bits)
- `parent_span_id`: 16 hex characters (64 bits)
- `flags`: Sampled (01) or not sampled (00)

**OTel Implementation**:
```fsharp
// Automatic propagation via OTel HttpClient instrumentation
let client = new HttpClient()
// OTel automatically injects traceparent header

// Manual propagation if needed:
let propagator = new TraceContextPropagator()
propagator.Inject(httpRequestMessage.Headers, activity)
```

## 5. Acceptance Criteria

- **AC-001**: Given a user creates an application via REST API, When the operation completes, Then an OTel trace exists with trace ID, span ID, and all operations linked with W3C Trace Context
- **AC-002**: Given an error occurs during command processing, When the error is logged, Then OTel logs include error type, message, stack trace, and affected aggregate ID
- **AC-003**: Given multiple services handle a request, When logs are aggregated, Then all logs are linked by trace ID and can be viewed as a single OTel trace
- **AC-004**: Given an HTTP endpoint is called, When the request completes, Then OTel metrics exist for `http.server.request.count`, `http.server.request.duration`, and status code breakdown
- **AC-005**: Given a distributed request spans multiple services, When the request completes, Then an OTel trace exists showing all service calls with W3C Trace Context propagation
- **AC-006**: Given production logs are queried, When DEBUG logs are retrieved, Then no logs appear unless explicitly sampled for troubleshooting
- **AC-007**: Given a metric query for high-cardinality labels (e.g., user IDs), When attempted, Then the query is rejected or the label is filtered
- **AC-008**: Given a critical error occurs, When OTel logging is executed, Then the log entry contains no PII or sensitive secrets
- **AC-009**: Given OTel exporter is misconfigured, When the application starts, Then it logs a warning but continues running (export failures don't block startup)
- **AC-010**: Given an OTel-compliant backend is configured, When traces are exported, Then all traces conform to OTLP protocol and are correctly received by the backend

## 6. Test Automation Strategy

- **Test Levels**: Unit, Integration, End-to-End
- **Unit Tests**:
  - OTel log message formatting and JSON validity
  - Trace ID generation and W3C compliance
  - OTel metrics counter/gauge correctness
  - Span attribute conformance to semantic conventions
  - OTel context propagation across async boundaries
- **Integration Tests**:
  - Full request flow with OTel trace verification
  - Trace context propagation via W3C headers across HTTP calls
  - OTel metrics emission to Prometheus exporter
  - Alert rule evaluation against OTel metric data
  - OTel exporter mock backends for validation
- **End-to-End Tests**:
  - Production-like scenarios with OTel enabled
  - Trace query and reconstruction in test observability backend
  - Metrics visualization in OTel dashboards
  - Alert firing based on OTel metrics
- **Frameworks**: NUnit/xUnit for .NET, OpenTelemetry test exporters
- **CI/CD Integration**: Automated tests run on PR; OTel integration tests run post-deployment
- **Coverage Requirements**: 80% for OTel instrumentation code
- **Performance Testing**: Verify OTel overhead <5% on p99 latency, trace sampling at 10% for load tests

## 7. Rationale & Context

**Why OpenTelemetry?**
- OTel is a CNCF standard with broad industry adoption; switching backends requires no code changes
- OTel Semantic Conventions ensure consistent naming across services and organizations
- OTel automatic instrumentation reduces boilerplate for common patterns (HTTP, database, messaging)
- OTel supports logs, metrics, and traces in a unified API and protocol
- OTel is vendor-neutral; avoids vendor lock-in to specific observability platforms

**Why W3C Trace Context?**
- W3C Trace Context is the standard for distributed tracing across services
- Trace context headers enable automatic trace reconstruction without centralized context storage
- W3C format is human-readable and debuggable in HTTP headers

**Why Semantic Conventions?**
- Standard names enable cross-service analysis and dashboarding
- Tools and observability backends understand semantic convention names automatically
- Consistent naming reduces cognitive load on operators

**Event-Sourced Systems Observability:**
- Log commands received, processed, and their results for audit trail
- Log events persisted to event store with count, timestamp, and aggregate
- Log projection updates and failures to track CQRS consistency
- Log event replay operations for debugging and recovery scenarios
- Use OTel span attributes to link commands, events, and projections

## 8. Dependencies & External Integrations

### External Systems
- **EXT-001**: OpenTelemetry Collector - OTLP-compatible endpoint for receiving logs, metrics, and traces
- **EXT-002**: Centralized Logging Platform (ELK, Datadog, CloudWatch) - MUST accept logs via OTLP protocol
- **EXT-003**: Time-Series Metrics Database (Prometheus, InfluxDB, TimescaleDB) - MUST accept OTel Prometheus format or OTLP
- **EXT-004**: Distributed Tracing Backend (Jaeger, Tempo, Datadog APM) - MUST accept OTLP protocol

### Third-Party Services
- **SVC-001**: OpenTelemetry .NET SDK - Core OTel API for .NET: System.Diagnostics.DiagnosticSource, OpenTelemetry.Api
- **SVC-002**: OpenTelemetry Instrumentations - Auto-instrumentation for ASP.NET Core, HTTP, SQL, Entity Framework
- **SVC-003**: OpenTelemetry Exporters - OTLP exporter, Prometheus exporter, Console exporter for debugging

### Infrastructure Dependencies
- **INF-001**: OpenTelemetry Collector cluster for receiving OTLP signals
- **INF-002**: Log aggregation cluster accepting OTLP or JSON logs with 30-day retention for production
- **INF-003**: Prometheus or compatible metrics store with daily cardinality monitoring
- **INF-004**: Trace storage with 7-day retention for production, 30-day for staging; OTLP protocol support
- **INF-005**: Alerting pipeline with incident creation and on-call routing

### Compliance Dependencies
- **COM-001**: Data Residency - Logs, metrics, and traces MUST be stored within specified geographic regions
- **COM-002**: Data Retention - Logs retained per compliance window; metrics aggregated after 1 year
- **COM-003**: PII Redaction - All OTel signals MUST be scanned for and redacted of PII before export

## 9. Examples & Edge Cases

### Example 1: OTel Instrumentation in F# Command Handler

**Using OTel ActivitySource**:
```fsharp
module EATool.Api.Handlers

open System.Diagnostics

let activitySource = new ActivitySource("EATool.Api.Handlers", "1.0.0")

let createApplication cmd (traceContext: Activity option) =
  use activity = activitySource.StartActivity("createApplication")
  
  // OTel automatically links to parent trace
  activity.SetAttribute("app.create.input.name", cmd.Name) |> ignore
  activity.SetAttribute("app.create.input.domain", cmd.Domain) |> ignore
  
  let startTime = System.DateTime.UtcNow
  
  try
    let appId = generateUuid()
    let event = ApplicationCreated { Id = appId; Name = cmd.Name; ... }
    
    // Span for event store append
    use appendSpan = activitySource.StartActivity("eventstore.append")
    appendSpan.SetAttribute("db.system", "event_store") |> ignore
    appendSpan.SetAttribute("db.operation", "append") |> ignore
    
    let appendDurationMs = float (System.DateTime.UtcNow - startTime).TotalMilliseconds
    appendSpan.SetAttribute("db.duration_ms", appendDurationMs) |> ignore
    
    eventStore.Append [event]
    appendSpan.SetStatus(ActivityStatusCode.Ok) |> ignore
    
    activity.SetAttribute("app.create.result", "success") |> ignore
    activity.SetAttribute("app.create.id", appId) |> ignore
    activity.SetStatus(ActivityStatusCode.Ok) |> ignore
    
    // OTel metrics
    commandCounter.Add(1L, KeyValuePair("command.type", "CreateApplication"), 
                           KeyValuePair("command.result", "success"))
    
    Ok appId
    
  with exn ->
    activity.SetAttribute("exception.type", exn.GetType().Name) |> ignore
    activity.SetAttribute("exception.message", exn.Message) |> ignore
    activity.SetAttribute("exception.stacktrace", exn.StackTrace) |> ignore
    activity.SetStatus(ActivityStatusCode.Error, exn.Message) |> ignore
    
    commandCounter.Add(1L, KeyValuePair("command.type", "CreateApplication"), 
                           KeyValuePair("command.result", "error"))
    
    Error exn.Message
```

### Example 2: OTel HTTP Request Tracing

**ASP.NET Core with OTel HttpServer instrumentation** (automatic):
```fsharp
// Startup configuration
open OpenTelemetry
open OpenTelemetry.Trace

let configureOTel (builder: WebApplicationBuilder) =
  builder.Services
    .AddOpenTelemetry()
    .WithTracing(fun tracerProviderBuilder ->
      tracerProviderBuilder
        .AddAspNetCoreInstrumentation()      // Automatic HTTP tracing
        .AddHttpClientInstrumentation()      // Automatic HTTP client tracing
        .AddSqlClientInstrumentation()       // Automatic SQL tracing
        .AddOtlpExporter(fun options ->
          options.Endpoint <- new Uri("http://localhost:4317")  // OTel Collector
        )
      |> ignore
    )
    |> ignore
    
  builder.Services
    .AddOpenTelemetry()
    .WithMetrics(fun meterProviderBuilder ->
      meterProviderBuilder
        .AddAspNetCoreInstrumentation()      // HTTP metrics
        .AddHttpClientInstrumentation()      // HTTP client metrics
        .AddRuntimeInstrumentation()         // Runtime metrics
        .AddPrometheusExporter()             // Prometheus metrics endpoint
      |> ignore
    )
    |> ignore
```

**W3C Trace Context Propagation** (automatic in OTel):
```
GET /api/applications HTTP/1.1
traceparent: 00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01
tracestate: vendor=data

HTTP/1.1 200 OK
traceparent: 00-4bf92f3577b34da6a3ce929d0e0e4736-xxxxxxxxxxxxxxxx-01
```

### Example 3: Edge Case - High-Volume Event Processing with OTel

**Problem**: Processing 10k events/second with full OTel instrumentation

**Solution** - Use batch operations and sampling:
```fsharp
let processEvents events batchId =
  use activity = activitySource.StartActivity("projection.processBatch")
  activity.SetAttribute("batch.id", batchId) |> ignore
  activity.SetAttribute("batch.event_count", events.Length) |> ignore
  
  let startTime = System.DateTime.UtcNow
  let mutable successCount = 0
  let mutable errorCount = 0
  
  // Batch-level span, not per-event
  events |> List.iter (fun evt ->
    try
      updateProjection evt
      successCount <- successCount + 1
    with exn ->
      errorCount <- errorCount + 1
      // Log only errors, not every success
      use errorSpan = activitySource.StartActivity("projection.event.failed")
      errorSpan.SetAttribute("event.id", evt.EventId) |> ignore
      errorSpan.SetAttribute("error.type", exn.GetType().Name) |> ignore
      errorSpan.SetStatus(ActivityStatusCode.Error, exn.Message) |> ignore
  )
  
  let durationMs = (System.DateTime.UtcNow - startTime).TotalMilliseconds
  activity.SetAttribute("batch.success_count", successCount) |> ignore
  activity.SetAttribute("batch.error_count", errorCount) |> ignore
  activity.SetAttribute("batch.duration_ms", durationMs) |> ignore
  
  // OTel metrics (aggregated, low cardinality)
  projectionGauge.Record(float successCount, KeyValuePair("result", "success"))
  projectionGauge.Record(float errorCount, KeyValuePair("result", "error"))
  projectionDuration.Record(durationMs)
```

### Example 4: Edge Case - Avoiding PII in OTel Signals

**Wrong**:
```fsharp
activity.SetAttribute("user.email", "john.doe@company.com") // PII!
activity.SetAttribute("request.body", jsonString) // May contain PII!
```

**Correct**:
```fsharp
activity.SetAttribute("user.id", "user-550e8400-e29b-41d4-a716-446655440000")
activity.SetAttribute("http.method", "POST")
activity.SetAttribute("http.target", "/api/applications")
// Don't log raw request/response bodies; use summary only
```

## 10. Validation Criteria

- [ ] All instrumentation uses OTel APIs; no direct use of DiagnosticListener
- [ ] All logs conform to OTel log schema with trace_id, span_id, trace_flags
- [ ] All metrics use OTel Semantic Convention names (reviewed against OTel spec)
- [ ] All spans set status correctly (Ok, Error, Unset) per OTel conventions
- [ ] W3C Trace Context headers (traceparent, tracestate) present in HTTP responses
- [ ] All OTel signals export to OTLP endpoint without errors
- [ ] No PII in any OTel signal (logs, metrics, traces); scan with data loss prevention
- [ ] Trace sampling is configurable; matches sampler configuration
- [ ] OTel exporter failures don't block application startup
- [ ] Dashboards query OTel semantic convention metric names
- [ ] Runbooks document OTel instrumentation points and observability backend setup

## 11. Related Specifications

- [Event Sourcing & Command-Based Architecture](spec-architecture-event-sourcing.md) - Commands, events, and audit trail requirements
- [Security Controls](spec-process-security.md) - PII handling and audit logging compliance
- [API Contract](spec-tool-api-contract.md) - HTTP response codes and error response format
- [Domain Model Overview](spec-schema-domain-overview.md) - Core entity definitions and timestamp formats

## References

- [OpenTelemetry .NET Getting Started](https://github.com/open-telemetry/opentelemetry-dotnet#getting-started)
- [OpenTelemetry Semantic Conventions](https://opentelemetry.io/docs/specs/semconv/)
- [W3C Trace Context](https://www.w3.org/TR/trace-context/)
- [Microsoft: Observability with OpenTelemetry](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel)
- [OTel Protocol (OTLP) Specification](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/protocol/exporter.md)
