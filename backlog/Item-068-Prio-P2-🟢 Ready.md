# Item-068: AuthN/AuthZ Integration Test Coverage

**Status:** 🟢 Ready  
**Priority:** P2 - MEDIUM  
**Effort:** 3-4 hours  
**Created:** 2026-01-08  
**Owner:** TBD

---

## Problem Statement

Integration tests currently focus on happy paths and basic validation but lack coverage for authentication and authorization flows (missing/invalid API keys, OIDC tokens). This leaves critical security paths untested and could allow regressions in request handling and error contracts.

---

## Affected Files

**Modify:**
- `tests/integration/` – add pytest cases for API key and OIDC token scenarios across representative endpoints.
- `tests/conftest.py` – extend fixtures if needed for auth variants.
- `spec/spec-testing-strategy.md` – link to new auth coverage where appropriate.

---

## Specifications

- [spec/spec-testing-strategy.md](../spec/spec-testing-strategy.md)
- [spec-process-authentication.md](../spec/spec-process-authentication.md)
- [spec-process-authorization.md](../spec/spec-process-authorization.md)

---

## Detailed Tasks

- [ ] Add tests for missing API key (expect 401/403) and invalid API key (expect 401/403) on at least `/health`, `/applications`, `/organizations`.
- [ ] Add tests for missing/invalid/expired OIDC token on OIDC-protected paths; assert error payload shape.
- [ ] Add precedence test where both API key and bearer token are present; verify expected auth source is used.
- [ ] Document auth test markers/usage in tests/README.md.

---

## Acceptance Criteria

- [ ] AuthN/AuthZ negative-path tests exist and run under `pytest -m integration`.
- [ ] Responses assert correct status codes and error contract fields.
- [ ] README updated with how to run auth scenarios and required env vars.

---

## Dependencies

**Depends On:** None

**Related:** Item-016 (Testing Strategy)

---

## Related Items

- [tests/conftest.py](../tests/conftest.py)
- [spec-process-authentication.md](../spec/spec-process-authentication.md)
- [spec-process-authorization.md](../spec/spec-process-authorization.md)

---

## Definition of Done

- [ ] Auth coverage implemented and documented.
- [ ] Negative cases validated with correct contracts.
