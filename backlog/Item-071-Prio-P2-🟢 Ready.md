# Item-071: Error Contract & Validation Coverage

**Status:** 🟢 Ready  
**Priority:** P2 - MEDIUM  
**Effort:** 3-4 hours  
**Created:** 2026-01-08  
**Owner:** TBD

---

## Problem Statement

Negative-path coverage is incomplete across endpoints. We lack systematic tests for validation errors (bad enums, malformed UUIDs, missing required fields), and error payload shape is not asserted uniformly. This risks contract drift from OpenAPI and inconsistent client behavior.

---

## Affected Files

**Modify:**
- `tests/integration/` per-entity files – add negative-path cases and payload shape assertions.
- `openapi.yaml` – verify/align schemas if discrepancies are found.

---

## Specifications

- [spec/spec-testing-strategy.md](../spec/spec-testing-strategy.md)
- [spec-tool-error-handling.md](../spec/spec-tool-error-handling.md)
- [spec-tool-query-patterns.md](../spec/spec-tool-query-patterns.md)

---

## Detailed Tasks

- [ ] Add negative-path tests (invalid enums, malformed UUIDs, missing required fields, type violations) for: `/applications`, `/application-services`, `/application-interfaces`, `/organizations`, `/servers`, `/relations`, `/integrations`, `/data-entities`, `/business-capabilities`.
- [ ] For each endpoint, assert error response contract fields: `code`, `message`, `details`, `trace_id` (per spec-tool-error-handling).
- [ ] Include query/command variants where applicable (e.g., command endpoints for applications/application-services, relation creation, server updates).
- [ ] Align tests with OpenAPI schemas; flag and document any mismatches for remediation in `openapi.yaml` and specs.

---

## Acceptance Criteria

- [ ] Each listed endpoint has negative-path coverage for bad enums, malformed/missing IDs, missing required fields, and type violations.
- [ ] Error responses across these endpoints include expected contract fields and correct status codes (400/422 for validation, 404 for not found).
- [ ] OpenAPI/spec discrepancies discovered by the tests are documented and scheduled for fix (or fixed as part of this item).

---

## Dependencies

**Depends On:** None

**Related:** Item-016 (Testing Strategy)

---

## Related Items

- [spec-tool-error-handling.md](../spec/spec-tool-error-handling.md)
- [openapi.yaml](../openapi.yaml)

---

## Definition of Done

- [ ] Negative-path and error contract coverage implemented for all listed endpoints.
