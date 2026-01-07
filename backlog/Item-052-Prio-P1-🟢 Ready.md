# Item-052: Metrics Implementation via OTel Meter API

**Status:** 🟢 Ready  
**Priority:** P1 - HIGH  
**Effort:** 8-10 hours  
**Created:** 2026-01-07  
**Owner:** TBD

---

## Problem Statement

The application currently has no metrics instrumentation. Without OTel Metrics API:
- No visibility into request rate, latency, or error breakdown
- Cannot monitor event store performance (appends, reads, lag)
- No alerting capability for operational issues (threshold violations)
- Cannot measure command processing efficiency
- Business metrics (entities created, capabilities defined) are unmeasured
- Operators cannot self-serve observability via dashboards

This violates MET-001 through MET-012 requirements and prevents production monitoring.

---

## Affected Files

**Create:**
- `src/Infrastructure/Metrics/MetricsRegistry.fs` - Central registry for all OTel meters and instruments
- `src/Infrastructure/Metrics/HttpMetrics.fs` - HTTP request/response metrics
- `src/Infrastructure/Metrics/EventStoreMetrics.fs` - Event store operation metrics
- `src/Infrastructure/Metrics/CommandMetrics.fs` - Command processing metrics
- `src/Infrastructure/Metrics/ProjectionMetrics.fs` - Projection update metrics
- `src/Infrastructure/Metrics/BusinessMetrics.fs` - Domain-specific business metrics

**Modify:**
- `src/Program.fs` - Register all metrics instruments in DI container
- `src/Api/Handlers.fs` - Emit HTTP metrics (request count, duration, errors)
- `src/Domain/CommandHandler.fs` - Emit command metrics (success, failure, type)
- `src/Infrastructure/EventStore.fs` - Emit event store metrics
- `src/Infrastructure/Projections/*.fs` - Emit projection metrics

---

## Specifications

- [spec/spec-process-observability.md](../spec/spec-process-observability.md) - Section 3.3, 4.2 (metrics requirements and schema)

---

## Detailed Tasks

- [ ] Create `src/Infrastructure/Metrics/MetricsRegistry.fs`:
  - Initialize OTel Meter with service name "EATool"
  - Create registry of all instruments (counters, gauges, histograms)
  - Expose getters for metrics to be used throughout application
  - Use OTel Semantic Convention names (all lowercase, dot-separated)

- [ ] Create `src/Infrastructure/Metrics/HttpMetrics.fs`:
  - HTTP request counter: `http.server.request.count` (labels: method, path, status)
  - HTTP request latency histogram: `http.server.request.duration` (unit: ms, labels: method, path, status)
  - HTTP request body size: `http.server.request.body.size` (optional, labels: method, path)
  - Emit metrics from middleware or ASP.NET Core instrumentation (automatic)

- [ ] Create `src/Infrastructure/Metrics/CommandMetrics.fs`:
  - Commands processed counter: `eatool.commands.processed` (labels: type, result)
    - result values: "success", "validation_error", "concurrency_error", "unknown_error"
  - Command processing duration histogram: `eatool.command.duration` (unit: ms, labels: type, result)
  - MET-008 compliance: track received, succeeded, failed per command type

- [ ] Create `src/Infrastructure/Metrics/EventStoreMetrics.fs`:
  - Event append counter: `eatool.eventstore.appends` (labels: aggregate_type, result)
  - Event append duration histogram: `eatool.eventstore.append.duration` (unit: ms)
  - Event read counter: `eatool.eventstore.reads` (labels: aggregate_type)
  - Event read duration histogram: `eatool.eventstore.read.duration` (unit: ms)
  - MET-006 compliance: track appends, reads, failures, latency

- [ ] Create `src/Infrastructure/Metrics/ProjectionMetrics.fs`:
  - Events processed counter: `eatool.projection.events.processed` (labels: projection_name, result)
  - Projection failure counter: `eatool.projection.failures` (labels: projection_name)
  - Projection lag gauge: `eatool.projection.lag` (unit: events, labels: projection_name)
  - Batch processing duration histogram: `eatool.projection.batch.duration` (unit: ms)
  - MET-009 compliance: track events processed, failures, lag

- [ ] Create `src/Infrastructure/Metrics/BusinessMetrics.fs`:
  - Applications created counter: `eatool.applications.created`
  - Capabilities defined counter: `eatool.capabilities.defined`
  - Other aggregate creation metrics per domain entities
  - Custom counters/gauges for business-critical events

- [ ] Update `src/Program.fs`:
  - Initialize MetricsRegistry on startup
  - Register all metric instruments in DI container
  - Configure Prometheus exporter for `/metrics` endpoint (MET-003)
  - Ensure metrics provider gracefully handles export failures (CON-002)

- [ ] Integrate metrics in handler execution:
  - HTTP metrics emitted automatically by ASP.NET Core instrumentation
  - If needed, add middleware for low-cardinality path aggregation

- [ ] Integrate metrics in command execution:
  - Emit command counter with type and result after each command
  - Emit duration histogram for command processing

- [ ] Integrate metrics in event store operations:
  - Emit append counter and duration for each event batch
  - Emit read counter and duration for each event store read

- [ ] Integrate metrics in projection processing:
  - Emit events processed counter per projection per batch
  - Emit lag gauge as difference between event store max and projection position

- [ ] Verify no high-cardinality labels:
  - Audit all metrics for user IDs, request IDs, session IDs
  - Use only low-cardinality labels (method, path, type, result)
  - MET-005 compliance check

- [ ] Create Prometheus-format test:
  - Verify metrics output is valid Prometheus format
  - Verify label names follow OTel conventions
  - Verify no high-cardinality values

- [ ] Add dashboard documentation:
  - Sample Grafana dashboard JSON or Prometheus queries
  - Show how to query request rate, latency, error rate
  - Show how to query event store performance
  - Show how to query projection lag

---

## Acceptance Criteria

- [ ] Metrics registry is initialized on startup
- [ ] HTTP metrics emitted for all requests (count, duration, status)
- [ ] Command metrics track success/failure/type per MET-008
- [ ] Event store metrics track appends, reads, latency per MET-006
- [ ] Projection metrics track processed events, failures, lag per MET-009
- [ ] All metrics use OTel Semantic Convention names (lowercase with dots)
- [ ] No high-cardinality labels (user IDs, request IDs excluded) per MET-005
- [ ] `/metrics` endpoint serves Prometheus-format metrics
- [ ] Metrics overhead <5% of request latency (p99) per CON-002
- [ ] Sample Prometheus queries work and return sensible data
- [ ] All tests pass (89+ integration tests)
- [ ] Build succeeds with 0 errors, 0 warnings
- [ ] Documentation updated with metrics reference

---

## Dependencies

**Blocks:**
- Item-054 - Alert rules definition (depends on metrics being available)

**Depends On:**
- Item-049 - OTel SDK integration (must have OTel configured)
- Item-051 - Tracing (metrics context should align with traces)

**Related:**
- Item-037 through Item-044 - Various domain operations (need metrics integration)

---

## Notes

Focus on OTel Semantic Conventions to ensure portability across observability backends. Keep high-cardinality labels out of metrics (can always add them to traces for debugging specific requests).
