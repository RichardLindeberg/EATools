# Item-069: Observability Contract Tests for Write Paths

**Status:** 🟢 Ready  
**Priority:** P2 - MEDIUM  
**Effort:** 3-4 hours  
**Created:** 2026-01-08  
**Owner:** TBD

---

## Problem Statement

Trace context and metrics are asserted for /health and /metrics, but create/update/delete flows lack observability contract tests. Without coverage, regressions in `traceparent` headers, span linkage, and low-cardinality labels on write paths may go unnoticed.

---

## Affected Files

**Modify:**
- `tests/integration/test_trace_context.py` – add W3C traceparent assertions for write responses.
- `tests/integration/` – per-entity tests (applications, application-services, relations, servers) to include observability checks.
- `spec/spec-testing-strategy.md` – reference coverage addition if needed.

---

## Specifications

- [spec/spec-testing-strategy.md](../spec/spec-testing-strategy.md)
- [spec-process-observability.md](../spec/spec-process-observability.md)

---

## Detailed Tasks

- [ ] Add tests that create/update/delete entities and assert `traceparent` header format on responses.
- [ ] Validate low-cardinality metric labels for these operations (method, path template, status) are present/clean.
- [ ] Ensure structured logs (if exposed) include correlation/trace IDs for these flows in mock/exporter checks.

---

## Acceptance Criteria

- [ ] Observability assertions exist for write-path endpoints across core entities.
- [ ] W3C traceparent format validated; no high-cardinality labels in metrics.
- [ ] Tests run under `pytest -m integration` without external collectors.

---

## Dependencies

**Depends On:** None

**Related:** Item-016 (Testing Strategy), Item-054 (Observability Alert Rules)

---

## Related Items

- [tests/integration/test_trace_context.py](../tests/integration/test_trace_context.py)
- [spec-process-observability.md](../spec/spec-process-observability.md)

---

## Definition of Done

- [ ] Observability contract tests cover write paths.
- [ ] Trace/metric expectations enforced.
