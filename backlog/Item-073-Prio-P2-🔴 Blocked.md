# Item-073: Performance Smoke Hooks (Non-Failing)

**Status:** � Blocked  
**Priority:** P2 - MEDIUM  
**Effort:** 2-3 hours  
**Created:** 2026-01-08  
**Owner:** TBD

---

## Problem Statement

The testing strategy now captures performance metrics but defers SLO thresholds to Item-066. We need lightweight timing capture in tests so latency data is available ahead of enforcing thresholds, without causing flaky failures.

---

## Affected Files

**Modify/Create:**
- `tests/integration/test_performance.py` (new) or add markers in existing suites to record timings.
- `tests/README.md` – document how to run performance hooks and interpret output.

---

## Specifications

- [spec/spec-testing-strategy.md](../spec/spec-testing-strategy.md)
- [backlog/Item-066-Prio-P2-🟢 Ready.md](Item-066-Prio-P2-%F0%9F%9F%A2%20Ready.md)

---

## Detailed Tasks

- [ ] Add timing capture for key endpoints (health, metadata, create/update commands) and log p50/p95 observed values.
- [ ] Keep tests non-failing on latency; only record/report values for now.
- [ ] Structure output so Item-066 can later convert to enforced thresholds.

---

## Acceptance Criteria

- [ ] Performance hooks run with `pytest -m integration` and emit timing stats.
- [ ] No false negatives from timing noise; data available for SLO definition work.

---

## Dependencies

**Depends On:** Item-066 (Latency SLOs)

**Related:** Item-016 (Testing Strategy)

---

## Related Items

- [spec/spec-testing-strategy.md](../spec/spec-testing-strategy.md)
- [backlog/Item-066-Prio-P2-🟢 Ready.md](Item-066-Prio-P2-%F0%9F%9F%A2%20Ready.md)

---

## Definition of Done

- [ ] Performance timing captured in integration runs without enforcing thresholds.
