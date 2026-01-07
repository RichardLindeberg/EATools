# Item-055: Observability Testing & Integration

**Status:** 🟢 Ready  
**Priority:** P2 - MEDIUM  
**Effort:** 4-6 hours  
**Created:** 2026-01-07  
**Owner:** TBD

---

## Problem Statement

Observability instrumentation without testing is fragile. Without automated tests:
- OTel instrumentation regresses undetected (broken traces, missing metrics)
- PII can be accidentally leaked in logs/metrics/traces (LOG-004, LOG-011 violations)
- Trace context propagation fails in corner cases (async, error paths)
- Metrics accuracy is not verified (high cardinality slips in)
- Sampling strategy changes break without notice

This violates test automation requirements (Section 6 of spec) and leaves observability brittle.

---

## Affected Files

**Create:**
- `tests/ObservabilityTests.fs` - Unit tests for OTel instrumentation
- `tests/Integration/TracingIntegrationTests.fs` - Integration tests for distributed tracing
- `tests/Integration/MetricsIntegrationTests.fs` - Integration tests for metrics
- `tests/Integration/LoggingIntegrationTests.fs` - Integration tests for structured logging
- `tests/fixtures/MockOTelExporter.fs` - Mock exporter for testing signal collection
- `tests/fixtures/OTelTestHelpers.fs` - Helpers for asserting observability signals

---

## Specifications

- [spec/spec-process-observability.md](../spec/spec-process-observability.md) - Section 6 (test automation strategy)

---

## Detailed Tasks

- [ ] Create mock OTel exporter for testing:
  - Mock implementation of OTLP exporter that collects signals in-memory
  - Expose methods to retrieve collected logs, metrics, traces
  - Support filtering by trace ID, span name, metric name
  - Use in tests instead of sending to real backend

- [ ] Create OTel test helpers in `tests/fixtures/OTelTestHelpers.fs`:
  - Function to assert span exists with attributes
  - Function to assert metric was recorded with value
  - Function to assert log entry contains fields
  - Function to assert PII is not present in signals
  - Function to assert W3C Trace Context format

- [ ] Unit tests for log formatting:
  - Test structured log JSON schema validity (JSON.parse succeeds)
  - Test all required fields present (timestamp, level, logger, message, trace_id, span_id)
  - Test trace ID and span ID are hex format (TRC-002)
  - Test trace flags are valid (01 or 00)
  - Test no PII in context object (email, names redacted)

- [ ] Unit tests for trace ID generation:
  - Test generated trace IDs are 32 hex characters (128 bits)
  - Test span IDs are 16 hex characters (64 bits)
  - Test trace IDs are unique across multiple calls
  - Test W3C Trace Context header format is valid

- [ ] Unit tests for metrics:
  - Test counter increments correctly
  - Test gauge records correct value
  - Test histogram records values (can query percentiles)
  - Test labels are applied correctly
  - Test metric names follow OTel conventions (lowercase, dots)
  - Test no high-cardinality labels (user IDs, request IDs)

- [ ] Unit tests for context propagation:
  - Test correlation ID is created on request entry
  - Test correlation ID is preserved through request lifecycle
  - Test Activity context is preserved across await boundaries (async)
  - Test trace context is available in error handlers

- [ ] Integration tests for full request tracing:
  - Send HTTP request through full stack
  - Assert OTel trace exists with root span
  - Assert root span has HTTP method, path, status attributes
  - Assert child spans exist (handler, command, event store)
  - Assert W3C Trace Context header in response
  - Assert trace is valid per W3C spec

- [ ] Integration tests for metrics emission:
  - Trigger HTTP request
  - Assert `http.server.request.count` metric recorded
  - Assert `http.server.request.duration` metric recorded
  - Assert metric labels correct (method, path, status)
  - Assert no high-cardinality labels
  - Verify /metrics endpoint returns valid Prometheus format

- [ ] Integration tests for logging:
  - Trigger operation that logs
  - Assert log entry collected by mock exporter
  - Assert log contains trace_id and span_id
  - Assert log is valid JSON per OTel schema
  - Assert error logs contain exception details
  - Assert no PII in log fields

- [ ] Integration tests for command processing:
  - Send command through handler
  - Assert command span created with type attribute
  - Assert command metric recorded (success or error)
  - Assert event store append span created
  - Assert end-to-end trace shows full flow

- [ ] Integration tests for projection processing:
  - Process events through projection
  - Assert projection batch span created
  - Assert events processed metric recorded
  - Assert projection lag metric updated
  - Assert no high-cardinality labels

- [ ] PII detection tests:
  - Scan all logs from typical request for email addresses
  - Scan all traces for phone numbers
  - Scan all metrics labels for user IDs
  - Verify known PII patterns are detected and redacted

- [ ] Sampling strategy tests:
  - Verify error rate sampling is 100% (all errors traced)
  - Verify success rate sampling matches configured percentage
  - Verify head-based sampling and tail-based sampling work

- [ ] Performance regression tests:
  - Measure latency with observability enabled vs disabled (5% threshold)
  - Verify metrics emission doesn't block request handling (CON-002)
  - Verify trace sampling reduces overhead proportional to sample rate

- [ ] Add tests to CI/CD:
  - Run observability tests on every PR
  - Run integration tests on main branch
  - Generate coverage report for observability code (80%+ target)
  - Alert if observability tests start failing

---

## Acceptance Criteria

- [ ] Unit tests for all observability components with 80%+ coverage
- [ ] Integration tests for complete request flow (logging, tracing, metrics)
- [ ] Mock OTel exporter allows testing without external backend
- [ ] Trace context propagation works in async operations
- [ ] W3C Trace Context format verified correct
- [ ] Metrics labels verified low-cardinality (no user IDs, request IDs)
- [ ] PII detection tests ensure no sensitive data leaks
- [ ] Sampling strategy tests verify correct behavior
- [ ] Performance tests show <5% latency overhead (CON-001)
- [ ] All tests pass (89+ integration tests + observability tests)
- [ ] Build succeeds with 0 errors, 0 warnings
- [ ] CI/CD configured to run observability tests

---

## Dependencies

**Blocks:**
- None (enhances Items 049-054)

**Depends On:**
- Item-049 - OTel SDK
- Item-050 - Logging
- Item-051 - Tracing
- Item-052 - Metrics

**Related:**
- Item-053 - Event store observability
- Item-054 - Alert rules

---

## Notes

Observability testing is often overlooked but critical. A single PII leak or broken trace can have compliance implications. Focus on integration tests that verify the full end-to-end flow. Mock exporters are key to avoiding external dependencies in tests.
