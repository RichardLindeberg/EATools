# Item-060: Range & Temporal Constraint Validation

**Status:** 🟢 Ready  
**Priority:** P2 - MEDIUM  
**Effort:** 2-3 hours  
**Created:** 2026-01-07  
**Owner:** TBD

---

## Problem Statement

The specification defines range and temporal constraints that aren't enforced:

- Relation.confidence must be between 0.0 and 1.0 (probability score)
- Relation.effective_from must be <= Relation.effective_to (temporal validity)

Without validation, invalid data can be persisted that violates business logic.

This is gap #15 and #16 from the implementation status tracker.

---

## Affected Files

**Modify:**
- `src/Api/Requests/CreateRelationRequest.fs` - Add confidence range validation
- `src/Api/Requests/UpdateRelationRequest.fs` - Add confidence range and temporal validation
- `src/Api/Handlers/RelationHandlers.fs` - Validate before create/update
- `src/Infrastructure/Repository/RelationRepository.fs` - Validate at persistence layer
- `tests/RelationCommandTests.fs` - Test range and temporal validation

---

## Specifications

- [spec/spec-schema-entities-meta.md](../spec/spec-schema-entities-meta.md) - Relation entity constraints
- [spec/spec-schema-validation.md](../spec/spec-schema-validation.md) - Validation rules for Relation
- [spec/spec-implementation-status.md](../spec/spec-implementation-status.md) - Gaps #15, #16

---

## Detailed Tasks

- [ ] **Relation.confidence range validation**:
  - Validate confidence is between 0.0 and 1.0 (inclusive)
  - Add validation in RelationHandlers.createRelation
  - Add validation in RelationHandlers.updateRelation
  - Add validation in RelationRepository.create/update
  - Error: "Confidence must be between 0.0 and 1.0, received: {value}"
  - Test boundary values: 0.0, 1.0, 0.5, -0.1 (invalid), 1.1 (invalid)

- [ ] **Relation temporal constraint validation**:
  - Validate effective_from <= effective_to when both are present
  - Allow null/None for both fields (optional)
  - Allow only effective_from set (no end date)
  - Allow only effective_to set (no start date)
  - Invalid: effective_from > effective_to
  - Add validation in RelationHandlers.updateRelation
  - Add validation in RelationRepository.update
  - Error: "effective_from ({from}) must be <= effective_to ({to})"

- [ ] **Validation layering**:
  - Handler layer: Validate before calling repository
  - Repository layer: Validate as secondary check
  - Return 400 Bad Request for validation failures

- [ ] **Test coverage**:
  - Confidence: Valid values (0.0, 0.5, 1.0), invalid values (-0.1, 1.1)
  - Temporal: Valid (from before to), invalid (from after to)
  - Temporal: Null combinations (both null, only from, only to)
  - Error message clarity and detail
  - Integration test: full relation update with validation

---

## Acceptance Criteria

- [ ] Relation.confidence validates range 0.0-1.0
- [ ] Relation.effective_from validates <= effective_to
- [ ] Invalid confidence returns 400 Bad Request with clear error
- [ ] Invalid temporal range returns 400 Bad Request with clear error
- [ ] Null/None confidence and temporal fields handled correctly
- [ ] Handler layer validates before repository
- [ ] Repository layer validates as secondary check
- [ ] All validation tests pass (8+ test cases)
- [ ] Build succeeds with 0 errors, 0 warnings

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

These are relatively straightforward validations. The temporal constraint (from <= to) is a common business rule. Make sure to handle null/None values correctly for optional fields.
