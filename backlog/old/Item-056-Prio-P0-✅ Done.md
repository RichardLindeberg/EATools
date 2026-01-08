# Item-056: Required Fields Enforcement - Critical Spec Violations

**Status:** 🟢 Ready  
**Priority:** P0 - CRITICAL  
**Effort:** 4-5 hours  
**Created:** 2026-01-07  
**Owner:** TBD

---

## Problem Statement

The specification requires several fields to be mandatory, but the implementation allows them to be optional. This violates the spec contract and allows invalid data to be persisted:

- Applications can be created without an owner (spec: required)
- Applications can be created without data_classification (spec: required)
- Servers can be created without environment (spec: required)
- Servers can be created without criticality (spec: required)

This breaks data integrity and violates CON-APP-001, CON-APP-002, CON-INF-001, CON-INF-002 constraints from the specifications.

---

## Affected Files

**Modify:**
- `src/Api/Requests/CreateApplicationRequest.fs` - Make owner and data_classification required
- `src/Api/Handlers/ApplicationHandlers.fs` - Add validation for required fields
- `src/Infrastructure/Repository/ApplicationRepository.fs` - Enforce required fields at persistence layer
- `src/Api/Requests/CreateServerRequest.fs` - Make environment and criticality required
- `src/Api/Handlers/ServerHandlers.fs` - Add validation for required fields
- `src/Infrastructure/Repository/ServerRepository.fs` - Enforce required fields at persistence layer
- `tests/ApplicationCommandTests.fs` - Add tests for required field validation
- `tests/ServerCommandTests.fs` - Add tests for required field validation

---

## Specifications

- [spec/spec-schema-entities-application.md](../spec/spec-schema-entities-application.md) - Section 2.1: Application entity requires owner and data_classification
- [spec/spec-schema-entities-infrastructure.md](../spec/spec-schema-entities-infrastructure.md) - Section 2.2: Server entity requires environment and criticality
- [spec/spec-implementation-status.md](../spec/spec-implementation-status.md) - Gaps 1-4 (Critical P0 gaps)

---

## Detailed Tasks

- [ ] **Application.owner field**:
  - Remove optionality from CreateApplicationRequest.owner (string option → string)
  - Add validation: owner cannot be empty string, max 255 chars
  - Update ApplicationHandlers.createApplication to require owner
  - Update ApplicationRepository.create to enforce at persistence layer
  - Add test: CreateApplicationRequest without owner returns 400 Bad Request

- [ ] **Application.data_classification field**:
  - Remove optionality from CreateApplicationRequest.data_classification (enum option → enum)
  - Ensure all valid classifications are provided in request
  - Update ApplicationHandlers.createApplication to validate classification
  - Update ApplicationRepository.create to enforce at persistence layer
  - Add test: CreateApplicationRequest with invalid classification returns 400 Bad Request

- [ ] **Server.environment field**:
  - Remove optionality from CreateServerRequest.environment (enum option → enum)
  - Ensure all valid environments (dev, staging, production, etc.) documented
  - Update ServerHandlers.createServer to require environment
  - Update ServerRepository.create to enforce at persistence layer
  - Add test: CreateServerRequest without environment returns 400 Bad Request

- [ ] **Server.criticality field**:
  - Remove optionality from CreateServerRequest.criticality (enum option → enum)
  - Ensure all valid criticality levels (critical, high, medium, low) documented
  - Update ServerHandlers.createServer to require criticality
  - Update ServerRepository.create to enforce at persistence layer
  - Add test: CreateServerRequest with invalid criticality returns 400 Bad Request

- [ ] **Validation layer enforcement**:
  - Ensure validation happens at API layer (request validation)
  - Ensure validation also happens at repository layer (defense in depth)
  - Ensure error messages are clear and spec-aligned

- [ ] **Update API documentation**:
  - Update OpenAPI/Swagger to mark fields as required
  - Update example requests to show required fields

- [ ] **Update tests**:
  - Add tests for each required field enforcement
  - Add integration tests for full create flow
  - Verify error responses include validation error details

---

## Acceptance Criteria

- [ ] CreateApplicationRequest.owner is required (not optional)
- [ ] CreateApplicationRequest.data_classification is required
- [ ] CreateServerRequest.environment is required
- [ ] CreateServerRequest.criticality is required
- [ ] API returns 400 Bad Request when required fields are missing
- [ ] Error response includes field name and validation reason
- [ ] Repository layer enforces required fields (defense in depth)
- [ ] All existing tests still pass
- [ ] New validation tests added and passing (4+ test cases)
- [ ] OpenAPI spec updated to reflect required fields
- [ ] Build succeeds with 0 errors, 0 warnings

---

## Dependencies

**Blocks:**
- None directly, but affects downstream validation items

**Depends On:**
- None (foundational spec enforcement)

**Related:**
- Item-057 - Unique constraints (complementary validation)
- Item-058 - Field format validation (complementary validation)
- Item-021 - Implementation status tracker (gap analysis)

---

## Notes

This is a critical spec compliance fix. These fields are marked as required in the specification (CON-APP-001, CON-APP-002, CON-INF-001, CON-INF-002), and allowing them to be optional breaks the spec contract. This should be done immediately to prevent invalid data from being created.
