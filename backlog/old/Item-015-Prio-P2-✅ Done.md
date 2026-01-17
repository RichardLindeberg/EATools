# Item-015: Expand Relation Validation Matrix

**Status:** 🟢 Ready  
**Priority:** P2 - MEDIUM  
**Effort:** 2-3 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

[spec-schema-validation.md](../../spec/spec-schema-validation.md) defines only 12 valid (source, target, relation_type) combinations, but documentation suggests many more patterns. Matrix is incomplete and may miss valid combinations or allow invalid ones.

---

## Affected Files

- [spec/spec-schema-validation.md](../../spec/spec-schema-validation.md#L23-L41)
- [src/Infrastructure/RelationRepository.fs](../../src/Infrastructure/RelationRepository.fs)

---

## Detailed Tasks

- [x] Review [docs/relationship-modeling.md](../../docs/relationship-modeling.md) for all documented patterns
- [x] Map each documented pattern to (source_type, target_type, relation_type)
- [x] Identify missing combinations
- [x] Add hierarchical relations:
  - [x] organization → organization (via parent_id, not relation)
  - [x] business_capability → business_capability (via parent_id, not relation)
- [x] Add missing application relations:
  - [x] application → application_service (uses - not just realizes)
  - [x] integration → application_interface (calls)
- [x] Document why some combinations are excluded
- [x] Add validation code if missing
- [x] Add tests for matrix enforcement

---

## Acceptance Criteria

- [x] Matrix covers all documented patterns
- [x] Invalid combinations rejected with 422
- [x] Valid combinations accepted
- [x] Tests verify matrix enforcement
- [x] Documentation explains each combination

---

## Dependencies

**Depends On:** None

---

## Related Items

- [Item-003-Prio-P0.md](Item-003-Prio-P0.md) - Implementation gaps
- [docs/relationship-modeling.md](../../docs/relationship-modeling.md)
- [spec-schema-validation.md](../../spec/spec-schema-validation.md)

---

## Definition of Done

- [x] Matrix is comprehensive
- [x] All documented patterns included
- [x] Tests verify enforcement
- [x] Spec updated

---

## Notes

- Critical for data integrity
- Should be referenced in API responses when validation fails
- Consider allowing custom relations for extensibility
