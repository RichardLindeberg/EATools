# Item-072: Data Cleanup & Idempotency Tests

**Status:** 🟢 Ready  
**Priority:** P2 - MEDIUM  
**Effort:** 2-3 hours  
**Created:** 2026-01-08  
**Owner:** TBD

---

## Problem Statement

Integration tests create resources but cleanup is inconsistent, and idempotency of repeated commands is not validated. This risks state bleed between tests and undefined behavior for retried requests.

---

## Affected Files

**Modify:**
- `tests/integration/test_applications.py`, `test_organizations.py`, `test_application_services.py`, `test_servers.py`, `test_relations.py` – add cleanup and idempotency cases.
- `tests/README.md` – document cleanup/idempotency guidance.

---

## Specifications

- [spec/spec-testing-strategy.md](../spec/spec-testing-strategy.md)

---

## Detailed Tasks

- [ ] Ensure each create/update test cleans up created entities (DELETE or command delete) or uses fixture finalizers.
- [ ] Add idempotency tests: re-run same create/command where supported and assert stable response or defined conflict code.
- [ ] Add safeguards to avoid leaking entities between tests (namespacing, teardown).

---

## Acceptance Criteria

- [ ] All integration suites leave a clean state after execution.
- [ ] Idempotency behavior is asserted for key commands/endpoints.

---

## Dependencies

**Depends On:** None

**Related:** Item-016 (Testing Strategy)

---

## Related Items

- [tests/README.md](../tests/README.md)
- [spec/spec-testing-strategy.md](../spec/spec-testing-strategy.md)

---

## Definition of Done

- [ ] Cleanup and idempotency validated across integration tests.
