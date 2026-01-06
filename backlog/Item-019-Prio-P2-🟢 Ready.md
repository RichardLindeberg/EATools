# Item-019: Decide on Consolidation of spec-schema-domain-model.md

**Status:** 🟢 Ready  
**Priority:** P2 - MEDIUM  
**Effort:** 2 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

[spec-schema-domain-model.md](../../spec/spec-schema-domain-model.md) is a 881-line file that duplicates content now split across entity-specific specs. This creates maintenance burden and confusion about which spec is authoritative.

**Options:**
1. Keep as "single comprehensive reference" (deprecate entity specs)
2. Remove it entirely (use entity specs as authoritative)
3. Keep both with clear role definition

---

## Affected Files

- [spec/spec-schema-domain-model.md](../../spec/spec-schema-domain-model.md)
- [spec/spec-index.md](../../spec/spec-index.md)
- All entity-specific spec files

---

## Detailed Tasks

- [ ] Evaluate current usage of domain-model.md
- [ ] Decide on approach (1, 2, or 3)
- [ ] If keeping: mark as reference, link to entity specs
- [ ] If removing: update spec-index.md
- [ ] If both: clearly separate concerns
- [ ] Update documentation

---

## Acceptance Criteria

- [ ] Approach decided and documented
- [ ] No duplicate authoritative sources
- [ ] Spec-index.md updated
- [ ] Team agrees on approach

---

## Recommendation

**Recommend Option 2:** Remove domain-model.md
- Reason: Entity-specific specs are more maintainable
- Keep domain-overview.md as high-level reference
- Point to entity specs for details
- Reduces duplicate content

---

## Dependencies

**Depends On:** None

---

## Related Items

- [spec-index.md](../../spec/spec-index.md)
- All entity specs

---

## Definition of Done

- [x] Approach decided
- [x] Action taken
- [x] Documentation updated
- [x] Team aligned

---

## Notes

- Maintenance burden vs. single-source-of-truth trade-off
- Consider deprecation period if removing
