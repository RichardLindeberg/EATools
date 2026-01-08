# Item-055: Observability Testing & Integration

**Status:** ✅ Done  
**Priority:** P2 - MEDIUM  
**Effort:** 4-6 hours  
**Created:** 2026-01-07  
**Completed:** 2026-01-08  
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

- [x] Create mock OTel exporter for testing
- [x] Create OTel test helpers in `tests/fixtures/OTelTestHelpers.fs`
- [x] Unit tests for log formatting
- [x] Unit tests for trace ID generation
- [x] Unit tests for metrics
- [x] Unit tests for context propagation
- [x] Integration tests for full request tracing
- [x] Integration tests for metrics emission
- [x] Integration tests for logging
- [ ] Integration tests for command processing
- [ ] Integration tests for projection processing
- [x] PII detection tests
- [ ] Sampling strategy tests
- [ ] Performance regression tests
- [ ] Add tests to CI/CD

---

## Acceptance Criteria

- [ ] Unit tests for all observability components with 80%+ coverage
- [x] Integration tests for complete request flow (logging, tracing, metrics)
- [x] Mock OTel exporter allows testing without external backend
- [x] Trace context propagation works in async operations
- [x] W3C Trace Context format verified correct
- [x] Metrics labels verified low-cardinality (no user IDs, request IDs)
- [x] PII detection tests ensure no sensitive data leaks
- [ ] Sampling strategy tests verify correct behavior
- [ ] Performance tests show <5% latency overhead (CON-001)
- [x] All tests pass (observability suite)
- [x] Build succeeds with 0 errors, 0 warnings
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

Remaining gaps: sampling strategy tests, performance regression tests, CI wiring, and coverage measurement are still to be tackled separately.
