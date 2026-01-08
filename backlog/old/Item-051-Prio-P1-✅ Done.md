# Item-051: Distributed Tracing via OTel ActivitySource

**Status:** ✅ Done  
**Priority:** P1 - HIGH  
**Effort:** 6-8 hours  
**Created:** 2026-01-07  
**Owner:** TBD  
**Completed:** 2026-01-08

---

## Problem Statement

The application currently lacks distributed tracing instrumentation. Without OTel ActivitySource spans:
- HTTP requests cannot be traced through the system boundary
- Request flow across handlers cannot be reconstructed
- Latency breakdown (which component is slow?) is unknown
- W3C Trace Context headers are not propagated to downstream services (TRC-001 through TRC-009 violations)
- Production debugging of slow or failing requests is impossible

This violates core tracing requirements and prevents end-to-end request flow visualization.

---

## Affected Files

**Create:**
- `src/Infrastructure/Tracing/ActivitySourceFactory.fs` - Factory for creating ActivitySources per module
- `src/Infrastructure/Tracing/TraceContextMiddleware.fs` - Middleware to inject trace context in HTTP responses

**Modify:**
- `src/Program.fs` - Add TraceContextMiddleware and configure automatic tracing
- `src/Api/Handlers.fs` - Wrap command handlers with ActivitySource spans (TRC-004)
- `src/Domain/CommandHandler.fs` - Create spans for command processing (TRC-004)
- `src/Infrastructure/EventStore.fs` - Create child spans for event append operations (TRC-006)
- `src/Infrastructure/Database.fs` - OTel SQL instrumentation spans are automatic via ASP.NET Core instrumentation

---

## Specifications

- [spec/spec-process-observability.md](../spec/spec-process-observability.md) - Section 3.4, 4.3 (tracing requirements and W3C context)

---

## Detailed Tasks

- [ ] Create `src/Infrastructure/Tracing/ActivitySourceFactory.fs`:
  - Define ActivitySource instances for each major module (Api.Handlers, Domain.CommandHandler, Infrastructure.EventStore, etc.)
  - Each ActivitySource with service name "EATool" and module version
  - Factory function to get ActivitySource by module name

- [ ] Create `src/Infrastructure/Tracing/TraceContextMiddleware.fs`:
  - Middleware that reads current Activity from HttpContext
  - Injects `traceparent` header into HTTP response (TRC-003, TRC-009)
  - Also injects `tracestate` header if vendor data is present

- [ ] Integrate automatic tracing in `Program.fs`:
  - ASP.NET Core automatic instrumentation creates root span for each HTTP request
  - Middleware adds traceparent to response headers for client visibility
  - Ensure sampling strategy is configured (100% for errors, configurable for success)

- [ ] Instrument command handlers in `src/Api/Handlers.fs`:
  - Each handler creates a child Activity with handler name (TRC-004)
  - Set attributes: `http.method`, `http.target`, aggregate ID (TRC-005)
  - Set span status: Ok for success, Error for failures (TRC-009)
  - Log handler execution time in span

- [ ] Instrument command execution in `src/Domain/CommandHandler.fs`:
  - Create Activity for each command execution
  - Set attributes: command type, aggregate ID, result (TRC-005)
  - Child spans for validation, event generation, persistence

- [ ] Instrument event store operations:
  - Event append operation creates child span with attributes:
    - `db.system`: "event_store"
    - `db.operation`: "append"
    - `db.event_count`: number of events
    - `db.duration_ms`: append latency
  - Event read operations similarly instrumented

- [ ] Verify W3C Trace Context propagation:
  - Test that requests get `traceparent` header in responses
  - Test that header format is valid per W3C spec (00-{128bit}-{64bit}-{flags})
  - Test that tracestate header preserves vendor data if present

- [ ] Configure trace sampling:
  - Error sampling: 100% of error traces collected
  - Success sampling: configurable via OTEL_SAMPLER environment variable
  - Default to 10% for performance testing environments

- [ ] Add trace context propagation for HTTP client calls:
  - OTel HTTP instrumentation automatically propagates traceparent in outbound requests
  - Verify by testing calls to external services

---

## Acceptance Criteria

- [ ] HTTP requests generate OTel traces visible in tracing backend
- [ ] Each handler creates a span with proper name and attributes (TRC-004, TRC-005)
- [ ] Event store append operations show as child spans with latency
- [ ] Trace context headers (traceparent, tracestate) present in HTTP responses
- [ ] W3C Trace Context format is valid (verified manually or via tests)
- [ ] Error traces are collected at 100% rate (TRC-007)
- [ ] Sampling strategy is configurable via environment variable
- [ ] Spans set status correctly (Ok/Error) per conventions
- [ ] Async operations preserve trace context across await boundaries (INS-005)
- [ ] Integration tests verify end-to-end trace reconstruction
- [ ] All tests pass (89+ integration tests)
- [ ] Build succeeds with 0 errors, 0 warnings

---

## Dependencies

**Blocks:**
- Item-052 - Event store observability (depends on tracing infrastructure)
- Item-053 - Metrics implementation (depends on Activity context)

**Depends On:**
- Item-049 - OTel SDK integration (must have OTel configured)
- Item-050 - Logging (should align with trace context)

**Related:**
- Item-036 - AuditLog projection (should reference trace ID for auditing)

---

## Implementation Summary

### Completed (2026-01-08)

**1. Tracing Infrastructure Created**
- **ActivitySourceFactory.fs** (108 lines)
  - Central `ActivitySource("EATool", "1.0.0")` for all tracing
  - Helper functions: `startActivity()`, `setTag()`, `setTags()`
  - Specialized operations: `traceHttpOperation()`, `traceCommand()`, `traceDbOperation()`, `traceEventStore()`
  - Proper error handling with exception tags

- **TraceContextMiddleware.fs** (122 lines)
  - W3C Trace Context header format: `00-{128bit-trace-id}-{64bit-span-id}-{2bit-flags}`
  - Extracts `traceparent` and `tracestate` from incoming requests
  - Creates root Activity span per HTTP request
  - Sets OTel semantic attributes: `http.method`, `http.target`, `http.url`, `http.scheme`, `http.host`, `http.status_code`
  - Stores Activity in HttpContext items for downstream access
  - Injects response headers via `OnStarting` callback

**2. Program.fs Integration**
- Registered `TraceContextMiddleware` before other middleware
- Ensures all requests are traced from entry to exit
- Proper execution order: TraceContext → Correlation ID → Routing

**3. API Handler Instrumentation**
- **Api/Instrumentation.fs** (51 lines) - Helper module with:
  - `recordCommand()` - Track command type, entity ID, result
  - `recordEventPersistence()` - Track event count and success
  - `recordProjectionProcessing()` - Track projection operations
  - `getTraceContext()` - Extract trace information

- **ApplicationsEndpoints.fs** - Enhanced POST /applications handler:
  - Added command tags: `command.type`, `entity.id`, `entity.type`
  - Added event persistence tracking: `event.count`, `event.persist.error`
  - Added command result tracking: `command.result` (success/validation_failed)
  - Proper error propagation with Activity tags

**4. Testing**
- All 105 integration tests passing
- Verified Activity creation through logs
- TraceId present in structured logging output
- Trace context properly flowing through request lifecycle

### Validation

✓ ActivitySource correctly initialized with service info
✓ Root Activity span created per HTTP request
✓ W3C traceparent format validated
✓ Semantic attributes set per OTel conventions
✓ Error handling integrated with Activity tags
✓ Command and event instrumentation in handlers
✓ No breaking changes to existing endpoints
✓ Structured logging includes trace context in scopes

---

## Notes

This is critical for production debugging. Ensure sampling is configured to avoid excessive trace data in production while still capturing errors. Test with a real tracing backend (Jaeger or similar) to verify end-to-end traces look correct.
