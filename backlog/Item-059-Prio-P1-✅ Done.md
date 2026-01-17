# Item-059: Application Lifecycle Transitions Validation

**Status:** ✅ Done  
**Priority:** P1 - HIGH  
**Effort:** 3-4 hours  
**Created:** 2026-01-07  
**Completed:** 2026-01-17  
**Owner:** System

---

## Problem Statement

The specification defines valid lifecycle state transitions (CON-APP-005), but the implementation doesn't enforce them. This allows invalid transitions:

- Applications cannot transition from Sunset directly to Active
- Applications cannot transition from Planned to Deprecated
- Invalid transitions violate data integrity and business logic

This is gap #7 from the implementation status tracker.

---

## Affected Files

**Create:**
- `src/Infrastructure/Validation/LifecycleTransitions.fs` - Valid state transition rules

**Modify:**
- `src/Api/Handlers/ApplicationHandlers.fs` - Validate transitions before update
- `src/Infrastructure/Repository/ApplicationRepository.fs` - Validate transitions at persistence layer
- `tests/ApplicationCommandTests.fs` - Test lifecycle transition validation

---

## Specifications

- [spec/spec-schema-entities-application.md](../spec/spec-schema-entities-application.md) - CON-APP-005: Valid lifecycle transitions
- [spec/spec-implementation-status.md](../spec/spec-implementation-status.md) - Gap #7

---

## Detailed Tasks

- [ ] **Create LifecycleTransitions.fs**:
  - Define valid state transitions as a lookup table or decision tree
  - Function: `isValidTransition : fromState -> toState -> bool`
  - Valid transitions (example):
    - Planned → Active, Deprecated
    - Active → Deprecated, Sunset
    - Deprecated → Sunset
    - Sunset → (no valid transitions, end state)
  - Add tests for all valid and invalid transitions

- [ ] **Update ApplicationHandlers.updateApplication**:
  - Before updating, check current lifecycle state
  - Validate proposed new state against current state
  - Call isValidTransition to check validity
  - Error: "Cannot transition from {current} to {proposed}. Valid transitions: {list}"
  - Return 400 Bad Request for invalid transitions

- [ ] **Update ApplicationRepository.update**:
  - Add validation at persistence layer
  - Check transition validity before updating database
  - Provide same error message as handler

- [ ] **Document valid transitions**:
  - Update spec or create transition diagram
  - Document why certain transitions are blocked
  - Example: Sunset is terminal state (no further transitions)

- [ ] **Test coverage**:
  - Test each valid transition (Planned→Active, Active→Deprecated, etc.)
  - Test each invalid transition (Active→Planned, Sunset→Active, etc.)
  - Test error message clarity
  - Test no-op update (same state to same state allowed)
  - Integration test: full update flow with transition validation

---

## Acceptance Criteria

- [x] LifecycleTransitions module defines valid state transitions
- [x] Invalid transitions return 400 Bad Request with clear error
- [x] Error message lists valid transitions from current state
- [x] Handler validates transitions before calling repository
- [x] Repository validates transitions as secondary check
- [x] All valid transitions work correctly
- [x] Transition validation tests pass (10+ test cases)
- [x] No-op updates (same state) are allowed
- [x] Build succeeds with 0 errors, 0 warnings

---

## Dependencies

**Blocks:**
- None

**Depends On:**
- Item-056 - Required fields (complementary validation)

**Related:**
- Item-057 - Unique constraints (complementary validation)
- Item-058 - Format validation (complementary validation)
- Item-021 - Implementation status tracker (gap analysis)

---

## Notes

Lifecycle transitions are a critical business rule. Make sure the transition rules are documented in the spec and align with the business requirements. Consider whether no-op updates (same state to same state) should be allowed or rejected.
