# Item-003: Document Implementation Gaps from Specifications

**Status:** 🟢 Ready  
**Priority:** P0 - CRITICAL  
**Effort:** 2-3 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

Specifications define features and validation rules that are not fully implemented in the codebase. Need to track and document these gaps to maintain spec/code alignment.

**Impact:** Specifications become unreliable if not kept in sync with implementation.

---

## Affected Files

**Create:** [spec/spec-implementation-status.md](../../spec/spec-implementation-status.md) (new file)

**Reference:** All specification files and implementation files

---

## Detailed Tasks

- [ ] Create spec-implementation-status.md with comprehensive matrix
- [ ] Track Organization.parent_id (spec defines, code missing)
- [ ] Track soft delete (REQ-004 defines, not implemented)
- [ ] Track field-level requirements vs reality:
  - Application.owner (spec: required, code: optional)
  - Application.data_classification (spec: required, code: optional)
  - Server.environment (spec: required, code: optional)
  - Server.criticality (spec: required, code: optional)
- [ ] Track unique constraints (spec defines, needs DB verification)
- [ ] Track validation rules (spec: comprehensive, code: partial)
- [ ] Document prioritization of fixes

---

## Acceptance Criteria

- [ ] spec-implementation-status.md created with all gaps documented
- [ ] Matrix includes: Entity, Field, Spec vs Code, Priority, Status
- [ ] All implementation gaps have priority assigned
- [ ] Each gap links to backlog item if creating new task
- [ ] Document is linked from spec-index.md

---

## Template for Status File

```markdown
# Implementation Status Tracker

## Overview
Tracks alignment between specifications (spec/ folder) and implementation (src/, tests/).

## Status Matrix

| Entity | Field | Specification | Implementation | Gap | Priority | Backlog |
|--------|-------|---|---|---|---|---|
| Organization | parent_id | Required | Missing | CRITICAL | P0 | Item-001 |
| Application | owner | Required | Optional | MEDIUM | P1 | - |
| ...

## Gap Categories

### Critical (P0)
- Missing features defined in specs
- Breaking spec contract

### High (P1)  
- Field-level mismatches
- Validation rule gaps

### Medium (P2)
- Documentation gaps
- Soft delete missing

### Low (P3)
- Nice-to-have features
- Enhancements

## Maintenance

This document should be reviewed:
- When updating specifications
- When implementing new features
- Monthly as part of sprint planning
```

---

## Key Gaps to Document

### Feature Gaps
1. **Organization.parent_id** - Spec defines, code missing → Item-001
2. **Soft Delete** - Spec REQ-004, not implemented
3. **Lifecycle Transitions** - Spec CON-007, validation not in code

### Validation Gaps
1. **Field Requirements** - Owner, classification, environment, criticality
2. **Unique Constraints** - (parent_id, name) compound uniqueness
3. **Lifecycle Validation** - Prevent backwards transitions

### Data Model Gaps
1. **LifecycleRaw Redundancy** - Not documented in spec
2. **AuditLog Entity** - Spec defines, no implementation
3. **Deleted timestamps** - Spec REQ-004, not used

---

## Dependencies

**Blocks:** 
- Item-021: Implementation Status Tracker (related but different scope)

**Depends On:** None

---

## Related Items

- [Item-021-Prio-P2.md](Item-021-Prio-P2.md) - Create full tracker
- [spec/](../../spec/) - All specification files
- [src/](../../src/) - Implementation files

---

## Definition of Done

- [x] spec-implementation-status.md created
- [x] All major gaps documented
- [x] Each gap has priority and ownership
- [x] Linked from spec-index.md
- [x] Used in sprint planning

---

## Notes

- This document becomes the source of truth for spec/code alignment
- Review during sprint planning and code reviews
- Update when specifications change
- Link new issues/items to this tracker
