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

- [ ] Review [docs/relationship-modeling.md](../../docs/relationship-modeling.md) for all documented patterns
- [ ] Map each documented pattern to (source_type, target_type, relation_type)
- [ ] Identify missing combinations
- [ ] Add hierarchical relations:
  - organization → organization (via parent_id, not relation)
  - business_capability → business_capability (via parent_id, not relation)
- [ ] Add missing application relations:
  - application → application_service (uses - not just realizes)
  - integration → application_interface (calls)
- [ ] Document why some combinations are excluded
- [ ] Add validation code if missing
- [ ] Add tests for matrix enforcement

---

## Acceptance Criteria

- [ ] Matrix covers all documented patterns
- [ ] Invalid combinations rejected with 422
- [ ] Valid combinations accepted
- [ ] Tests verify matrix enforcement
- [ ] Documentation explains each combination

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
