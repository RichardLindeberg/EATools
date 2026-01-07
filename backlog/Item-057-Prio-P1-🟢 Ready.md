# Item-057: Unique Constraints & Cycle Detection Implementation

**Status:** 🟢 Ready  
**Priority:** P1 - HIGH  
**Effort:** 6-8 hours  
**Created:** 2026-01-07  
**Owner:** TBD

---

## Problem Statement

The specification defines several unique constraints and cycle detection rules, but the implementation is missing them. This allows invalid data:

- Duplicate application names can be created under same organization
- Duplicate business capability names can be created under same parent
- Business capability parent references can form cycles (circular parents)
- These violations break data integrity and spec contract

This affects gaps #6, #8, #9 from the implementation status tracker.

---

## Affected Files

**Create:**
- `src/Infrastructure/Validation/CycleDetection.fs` - Business capability cycle detection logic

**Modify:**
- `src/Infrastructure/Repository/ApplicationRepository.fs` - Add unique constraint enforcement for (organization_id, name)
- `src/Infrastructure/Repository/BusinessCapabilityRepository.fs` - Add unique constraint and cycle detection
- `src/Api/Handlers/ApplicationHandlers.fs` - Validate unique constraint before create/update
- `src/Api/Handlers/BusinessCapabilityHandlers.fs` - Validate unique constraint and cycles before update
- Database migration files - Add unique constraints and composite indexes
- `tests/ApplicationCommandTests.fs` - Test unique name constraint
- `tests/BusinessCapabilityCommandTests.fs` - Test unique name constraint and cycle detection

---

## Specifications

- [spec/spec-schema-entities-application.md](../spec/spec-schema-entities-application.md) - CON-APP-003: Application names unique per organization
- [spec/spec-schema-entities-business.md](../spec/spec-schema-entities-business.md) - CON-BUS-004: Capability names unique per parent, CON-BUS-005: No parent cycles
- [spec/spec-implementation-status.md](../spec/spec-implementation-status.md) - Gaps #6, #8, #9

---

## Detailed Tasks

- [ ] **Application name uniqueness (CON-APP-003)**:
  - Add database unique constraint: UNIQUE(organization_id, name)
  - Create migration to add constraint to applications table
  - Handle existing duplicates (migrate or report error)
  - Add validation in ApplicationRepository before create/update
  - Error response: "Application with name '{name}' already exists in this organization"
  - Add test: Creating duplicate names in same org returns error
  - Add test: Same name allowed in different orgs

- [ ] **BusinessCapability name uniqueness (CON-BUS-004)**:
  - Add database unique constraint: UNIQUE(parent_id, name)
  - Create migration to add constraint to business_capabilities table
  - Handle existing duplicates (migrate or report error)
  - Add validation in BusinessCapabilityRepository before create/update
  - Error response: "Capability with name '{name}' already exists under this parent"
  - Add test: Creating duplicate names under same parent returns error
  - Add test: Same name allowed under different parents

- [ ] **BusinessCapability cycle detection (CON-BUS-005)**:
  - Create CycleDetection.fs with wouldCreateCycle function
  - Implement recursive parent traversal to detect cycles
  - Use same pattern as Organization cycle detection
  - Call detection before any parent_id update
  - Error response: "Cannot set parent: would create circular parent reference"
  - Add test: Creating parent cycle returns error
  - Add test: Deep cycle (A→B→C→A) is detected
  - Add test: Valid parent chains are allowed

- [ ] **Database migrations**:
  - Create migration for Application unique constraint
  - Create migration for BusinessCapability unique constraint
  - Test migrations both up and down
  - Document any existing data that violates constraints

- [ ] **Repository layer enforcement**:
  - ApplicationRepository.create: Check uniqueness before insert
  - ApplicationRepository.update: Check uniqueness before update (excluding current record)
  - BusinessCapabilityRepository.create: Check uniqueness and cycles before insert
  - BusinessCapabilityRepository.update: Check uniqueness and cycles before update
  - Return specific error codes for each violation type

- [ ] **Handler validation**:
  - ApplicationHandlers.createApplication: Validate before calling repository
  - ApplicationHandlers.updateApplication: Validate before calling repository
  - BusinessCapabilityHandlers.createCapability: Validate before calling repository
  - BusinessCapabilityHandlers.updateCapability: Validate cycles when parent_id changes

- [ ] **Test coverage**:
  - Test unique constraint enforcement (duplicate names)
  - Test cycle detection (direct and indirect cycles)
  - Test error messages are clear
  - Test migrations apply correctly
  - Test duplicate checking with various case scenarios
  - Performance test: Cycle detection on deep hierarchies

---

## Acceptance Criteria

- [ ] Application names are unique per organization (UNIQUE constraint exists)
- [ ] BusinessCapability names are unique per parent (UNIQUE constraint exists)
- [ ] BusinessCapability parent cycles are detected and prevented
- [ ] Duplicate application names return 409 Conflict with clear message
- [ ] Duplicate capability names return 409 Conflict with clear message
- [ ] Parent cycles return 400 Bad Request with clear message
- [ ] Database migrations created and tested (up/down)
- [ ] Repository layer validates before persistence
- [ ] Handler layer validates before repository call
- [ ] All tests pass (10+ test cases for constraints/cycles)
- [ ] Build succeeds with 0 errors, 0 warnings
- [ ] Performance acceptable for cycle detection (sub-100ms for deep hierarchies)

---

## Dependencies

**Blocks:**
- None

**Depends On:**
- Item-056 - Required fields (complementary validation)

**Related:**
- Item-058 - Field format validation (complementary validation)
- Item-021 - Implementation status tracker (gap analysis)

---

## Notes

These constraints are essential for data integrity. The cycle detection should use the same algorithm as Organization (which already has this implemented). Focus on making error messages clear so API consumers understand what went wrong.
