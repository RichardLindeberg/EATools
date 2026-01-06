# Item-022: Consider Consolidation of Small Entity Specifications

**Status:** 🟢 Ready  
**Priority:** P2 - MEDIUM  
**Effort:** 2 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

Some entity specifications are very small and could be consolidated:
- [spec-schema-entities-infrastructure.md](../../spec/spec-schema-entities-infrastructure.md) - Only Server + Integration (150 lines)
- [spec-schema-entities-data.md](../../spec/spec-schema-entities-data.md) - Only DataEntity (150 lines)

Could merge into [spec-schema-entities-application.md](../../spec/spec-schema-entities-application.md) since they're all Application Layer.

---

## Affected Files

- [spec/spec-schema-entities-infrastructure.md](../../spec/spec-schema-entities-infrastructure.md)
- [spec/spec-schema-entities-data.md](../../spec/spec-schema-entities-data.md)
- [spec/spec-schema-entities-application.md](../../spec/spec-schema-entities-application.md)
- [spec/spec-index.md](../../spec/spec-index.md)

---

## Detailed Tasks

- [ ] Evaluate consolidation options
- [ ] Decide on approach (consolidate or keep separate)
- [ ] If consolidating: merge files
- [ ] Update spec-index.md
- [ ] Update cross-references

---

## Acceptance Criteria

- [ ] Approach decided
- [ ] Files consolidated (if chosen)
- [ ] Index updated
- [ ] All links work

---

## Recommendation

**Consolidate if:**
- All three specs fit logically in one file (< 500 lines)
- Clear separation by section

**Keep separate if:**
- Files grow significantly
- Different audiences
- Independent evolution

---

## Dependencies

**Depends On:** None

---

## Related Items

- [spec-index.md](../../spec/spec-index.md)
- All entity specs

---

## Definition of Done

- [x] Decision made
- [x] Action taken
- [x] Index updated

---

## Notes

- Balance between organization and consolidation
- Consider file size and readability
