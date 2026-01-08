# Item-066: Define Performance Thresholds & Latency SLOs

**Status:** 🟢 Ready  
**Priority:** P2 - MEDIUM  
**Effort:** 3-5 hours  
**Created:** 2026-01-08  
**Owner:** TBD

---

## Problem Statement

The testing strategy defers concrete latency targets. To ensure consistent performance expectations, we need explicit SLOs and testable thresholds for key endpoints (health, metadata, CRUD commands). Without defined thresholds, CI cannot enforce performance regressions and observability data lacks actionable guardrails.

---

## Affected Files

**Modify:**
- `spec/spec-testing-strategy.md` - Add finalized latency thresholds and update requirements/acceptance criteria.
- `tests/integration/` - Add or update performance-oriented tests/markers to assert thresholds once defined.
- `backlog/INDEX.md` - Update status when complete.

**Create (optional):**
- `tests/integration/test_performance.py` - Performance-focused smoke tests with thresholds.

---

## Specifications

- [spec/spec-testing-strategy.md](../spec/spec-testing-strategy.md) (update with thresholds)
- [backlog/Item-016-Prio-P2-✅ Done.md](old/Item-016-Prio-P2-✅%20Done.md) (originating item)

---

## Detailed Tasks

- [ ] Define SLOs and thresholds for key endpoints (health, metadata, create/update commands): p50, p95, and timeout bounds.
- [ ] Update `spec/spec-testing-strategy.md` to include REQ/AC for latency thresholds and CI enforcement.
- [ ] Add pytest integration timing assertions (e.g., marker `performance`) that fail when thresholds exceeded.
- [ ] Ensure tests record latency metrics without flaky reliance on external load; use single-user baseline.
- [ ] Wire thresholds into CI (e.g., env-configurable defaults) and document in README/testing guidance.

---

## Acceptance Criteria

- [ ] SLO thresholds (p50/p95) are documented for health, metadata, and write paths in `spec/spec-testing-strategy.md`.
- [ ] Integration tests enforce thresholds with clear failure messages and optional markers to include/exclude.
- [ ] CI run fails when thresholds are exceeded and reports timing metrics in artifacts/logs.
- [ ] Documentation updated to describe how to run performance checks locally and in CI.

---

## Dependencies

**Depends On:** None

**Related:**
- Item-016 (Testing Strategy) – original specification work
- Item-054 (Observability Alert Rules) – may consume latency metrics

---

## Related Items

- [spec/spec-testing-strategy.md](../spec/spec-testing-strategy.md)
- [tests/](../tests/)

---

## Definition of Done

- [ ] Latency thresholds defined and documented.
- [ ] Tests added/updated to enforce thresholds.
- [ ] CI integrates performance checks.
- [ ] Backlog and index updated; file moved to `backlog/old/` when complete.

---

## Notes

- Use minimal, deterministic payloads to reduce variance; avoid external dependencies.
- Keep thresholds configurable via environment variables for CI vs. local runs.
- Prefer measuring server-side timing via returned metrics/headers where available.
