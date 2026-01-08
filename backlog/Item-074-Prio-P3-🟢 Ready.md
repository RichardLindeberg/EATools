# Item-074: Contract Drift Detection vs OpenAPI

**Status:** 🟢 Ready  
**Priority:** P3 - LOW  
**Effort:** 3-4 hours  
**Created:** 2026-01-08  
**Owner:** TBD

---

## Problem Statement

There is no automated guard to detect drift between API responses and `openapi.yaml`. Without schema-based validation, response envelopes or field types can diverge silently, breaking clients.

---

## Affected Files

**Create/Modify:**
- `tests/integration/test_contract_drift.py` – add schema-based validation (e.g., jsonschema or schemathesis) against `openapi.yaml`.
- `openapi.yaml` – align definitions if discrepancies are found.

---

## Specifications

- [spec/spec-testing-strategy.md](../spec/spec-testing-strategy.md)
- [spec-tool-api-contract.md](../spec/spec-tool-api-contract.md)

---

## Detailed Tasks

- [ ] Add schema validation tests for key endpoints (health, applications, organizations, servers, relations) using `openapi.yaml`.
- [ ] Report any mismatches; update spec or tests accordingly.
- [ ] Provide marker (e.g., `contract`) to run selectively.

---

## Acceptance Criteria

- [ ] Contract validation tests detect deviations from OpenAPI schemas.
- [ ] Mismatches are documented or fixed in `openapi.yaml` and implementation.

---

## Dependencies

**Depends On:** None

**Related:** Item-016 (Testing Strategy)

---

## Related Items

- [openapi.yaml](../openapi.yaml)
- [spec-tool-api-contract.md](../spec/spec-tool-api-contract.md)

---

## Definition of Done

- [ ] Contract drift tests in place and runnable via pytest marker.
