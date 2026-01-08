# Item-049: OpenTelemetry SDK Integration & Configuration

**Status:** ✅ Done  
**Priority:** P1 - HIGH  
**Effort:** 6-8 hours  
**Created:** 2026-01-07  
**Completed:** 2026-01-08  
**Owner:** TBD

---

## Problem Statement

The EA Tool currently lacks production-grade observability instrumentation. Without OpenTelemetry integration, we cannot:
- Trace distributed requests across service boundaries
- Collect structured metrics for monitoring and alerting
- Generate logs with correlation IDs for debugging production issues
- Export observability signals to standard backends (Jaeger, Datadog, ELK, Prometheus)

This blocks all observability work and violates the requirement to be OTel-compliant per the Monitoring & Observability specification.

---

## Affected Files

**Create:**
- `src/Infrastructure/Observability.fs` - OTel configuration and setup utilities
- `src/Infrastructure/Observability/ActivitySourceFactory.fs` - Factory for creating OTel ActivitySources per module

**Modify:**
- `src/Program.fs` - Add OTel builder configuration in WebApplication setup
- `EATool.fsproj` - Add NuGet dependencies: OpenTelemetry, OpenTelemetry.Exporter.Console, OpenTelemetry.Exporter.OpenTelemetryProtocol, OpenTelemetry.Instrumentation.AspNetCore, OpenTelemetry.Instrumentation.Http, OpenTelemetry.Instrumentation.SqlClient

---

## Specifications

- [spec/spec-process-observability.md](../spec/spec-process-observability.md) - Complete observability requirements and OTel compliance rules
- [spec/spec-tool-api-contract.md](../spec/spec-tool-api-contract.md) - HTTP contract for health/metrics endpoints

---

## Detailed Tasks

- [x] Add OpenTelemetry .NET NuGet packages to EATool.fsproj:
  - `OpenTelemetry` (core API)
  - `OpenTelemetry.Api` (semantic conventions)
  - `OpenTelemetry.Exporter.OpenTelemetryProtocol` (OTLP exporter)
  - `OpenTelemetry.Exporter.Console` (for debugging)
  - `OpenTelemetry.Instrumentation.AspNetCore` (automatic HTTP tracing)
  - `OpenTelemetry.Instrumentation.Http` (HTTP client tracing)
  - `OpenTelemetry.Instrumentation.SqlClient` (SQL tracing)
  - `System.Diagnostics.DiagnosticSource` (ActivitySource support)

- [x] Create `src/Infrastructure/Observability.fs` module with:
  - `configureOTelTracing`: Function to configure TracerProvider with OTLP exporter
  - `configureOTelMetrics`: Function to configure MeterProvider with OTLP exporter
  - `configureOTelLogging`: Function to configure ILogger integration
  - Exporter configuration from environment variables (OTEL_EXPORTER_OTLP_ENDPOINT, OTEL_EXPORTER_OTLP_HEADERS)
  - Sampling strategy configuration (ParentBasedSampler with configurable error sampling)

- [x] Integrate OTel configuration into `src/Program.fs`:
  - Call observability configuration functions in WebApplicationBuilder setup
  - Ensure exporter failures don't block application startup (graceful degradation)
  - Add environment variable reading for OTEL_SERVICE_NAME, OTEL_DEPLOYMENT_ENVIRONMENT

- [x] Create `/health` endpoint that returns:
  - Service name, version, environment
  - OTel readiness status (exporter connected or failed gracefully)
  - Application startup time and uptime
  - Response format: JSON per OTel health check conventions

- [x] Add configuration documentation:
  - Environment variable reference (OTEL_EXPORTER_OTLP_ENDPOINT, OTEL_EXPORTER_OTLP_HEADERS, OTEL_DEPLOYMENT_ENVIRONMENT, etc.)
  - How to connect to local OTel Collector vs cloud backends (Datadog, Jaeger, Tempo)
  - Sampling strategy tuning guidelines

---

## Acceptance Criteria

- [x] EATool.fsproj builds successfully with all OTel dependencies added
- [x] `/health` endpoint responds with OTel-compatible JSON within 100ms
- [x] Environment variable configuration is read correctly (no hardcoded exporter URLs)
- [x] TracerProvider, MeterProvider, and Logger are properly initialized in DI container
- [x] Application logs warning but continues if OTLP exporter is unreachable (CON-006)
- [x] Metrics endpoint `/metrics` exposes Prometheus-format output (for future use)
- [x] All tests pass (101 integration tests)
- [x] Build succeeds with 0 errors, 0 warnings
- [x] Documentation updated in OpenAPI spec

---

## Dependencies

**Blocks:**
- Item-050 - Structured logging implementation
- Item-051 - Command handler instrumentation
- Item-052 - Event store observability

**Depends On:**
- None (foundational)

**Related:**
- spec-process-observability.md (specification this implements)

---

## Notes

This is the foundational item; all subsequent observability work depends on it. Prioritize getting the basic OTel SDK wired up correctly with graceful fallback for export failures.

---

## History

| Date | Action | Description |
|------|--------|-------------|
| 2026-01-08 | ✅ Completed | Implemented OpenTelemetry SDK integration with tracing, metrics, logging, and /health endpoint. All 101 tests passing. |
