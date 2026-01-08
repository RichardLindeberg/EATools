# Item-070: Metrics Label Cardinality & PII Guardrails

**Status:** 🟢 Ready  
**Priority:** P2 - MEDIUM  
**Effort:** 2-3 hours  
**Created:** 2026-01-08  
**Owner:** TBD

---

## Problem Statement

Current `/metrics` tests only assert presence and format. They do not verify low-cardinality label policies or absence of PII-like patterns. This risks telemetry bloat or sensitive data leakage in metrics.

---

## Affected Files

**Modify:**
- `tests/integration/test_metrics.py` – add label set validation and PII guard checks.
- `spec/spec-testing-strategy.md` – reference expanded metric validation if needed.

---

## Specifications

- [spec/spec-testing-strategy.md](../spec/spec-testing-strategy.md)
- [spec-process-observability.md](../spec/spec-process-observability.md)

---

## Detailed Tasks

- [ ] Validate allowed label keys per metric family; fail on high-cardinality labels (IDs, emails, timestamps).
- [ ] Add simple PII pattern detection (email/UUID) to metrics text scrape assertions.
- [ ] Keep tests deterministic and offline (no external collectors required).

---

## Acceptance Criteria

- [ ] Metrics tests fail when unexpected/high-cardinality labels or PII-like values appear.
- [ ] Coverage includes at least core HTTP and domain metrics emitted by the API.

---

## Dependencies

**Depends On:** None

**Related:** Item-069 (Observability on write paths)

---

## Related Items

- [tests/integration/test_metrics.py](../tests/integration/test_metrics.py)
- [spec-process-observability.md](../spec/spec-process-observability.md)

---

## Definition of Done

- [ ] Metrics label cardinality and PII guardrails enforced in tests.
