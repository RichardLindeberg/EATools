# Item-067: Add CI Coverage Gates & Artifacts

**Status:** 🟢 Ready  
**Priority:** P2 - MEDIUM  
**Effort:** 2-3 hours  
**Created:** 2026-01-08  
**Owner:** TBD

---

## Problem Statement

Coverage expectations in spec-testing-strategy.md (85% F# statement, 70% pytest) are not enforced in CI. Without gates and published artifacts (Cobertura/XML, JUnit), regressions can ship undetected and reports are unavailable for review.

---

## Affected Files

**Modify:**
- `.github/workflows/*` or CI pipeline files – add coverage flags and thresholds for `dotnet test` and `pytest`.
- `tests/README.md` – document coverage commands and artifacts.
- `spec/spec-testing-strategy.md` – reference CI gate implementation if needed.

---

## Specifications

- [spec/spec-testing-strategy.md](../spec/spec-testing-strategy.md)

---

## Detailed Tasks

- [ ] Enable `dotnet test` with Coverlet (Cobertura/XML) and fail if <85% statement coverage for F# assemblies.
- [ ] Enable `pytest --cov --cov-report=xml` and fail if <70% coverage for Python integration modules.
- [ ] Publish coverage and JUnit XML artifacts in CI.
- [ ] Document local commands in tests/README.md.

---

## Acceptance Criteria

- [ ] CI fails when coverage drops below thresholds for F# and pytest suites.
- [ ] Coverage XML and JUnit XML artifacts are uploaded from CI runs.
- [ ] README documents how to run coverage locally and interpret reports.

---

## Dependencies

**Depends On:** None

**Related:** Item-016 (Testing Strategy)

---

## Related Items

- [spec/spec-testing-strategy.md](../spec/spec-testing-strategy.md)
- [tests/README.md](../tests/README.md)

---

## Definition of Done

- [ ] Coverage gates in CI for both stacks.
- [ ] Artifacts published per run.
- [ ] Docs updated.
