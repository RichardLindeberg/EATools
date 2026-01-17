# Item-019: Decide on Consolidation of spec-schema-domain-model.md

**Status:** ✅ Done  
**Priority:** P2 - MEDIUM  
**Effort:** 2 hours  
**Created:** 2026-01-06  
**Owner:** System
**Completed:** 2026-01-20

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

## Implementation Summary

**Option 2 Selected:** Remove spec-schema-domain-model.md (consolidation)

### Actions Completed

1. **File Deleted:** spec-schema-domain-model.md removed (881 lines)
2. **spec/README.md Updated:**
   - Removed directory structure entry
   - Updated status table (removed table row)
3. **Verification:** No dangling references in spec directory (only backlog items reference it for historical context)
4. **Specification Consolidation:**
   - spec-schema-domain-overview.md serves as authoritative high-level reference (360 lines)
   - Entity-specific specs provide detailed schemas:
     - spec-schema-entities-business.md
     - spec-schema-entities-application.md
     - spec-schema-entities-infrastructure.md
     - spec-schema-entities-data.md
     - spec-schema-entities-meta.md
     - spec-schema-entities-supporting.md

### Rationale

- **Reduces duplicate content:** Domain overview + entity specs cover all cases
- **Clearer authority:** Entity specs are single source of truth for their domains
- **Lower maintenance:** Changes documented once in entity-specific specs
- **Improved navigation:** spec-index.md guides users to appropriate authoritative spec

---

## Detailed Tasks

- [x] Evaluate current usage of domain-model.md
- [x] Decide on approach (Option 2 selected)
- [x] Remove redundant file
- [x] Update spec-index.md (already clean)
- [x] Update documentation (spec/README.md)
- [x] Verify no dangling references

---

## Acceptance Criteria

- [x] Approach decided and documented
- [x] No duplicate authoritative sources
- [x] Spec-index.md verified (no references to removed file)
- [x] Team approach clear and documented

---

## Recommendation

**✅ COMPLETED - Option 2 Implemented:** Remove domain-model.md in favor of consolidated entity specs
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
- [x] Action taken (file deleted)
- [x] Documentation updated (README.md)
- [x] Verification completed (no dangling references)
