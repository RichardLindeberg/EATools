# Item-033: Migrate BusinessCapability Endpoints to Commands

**Status:** 🟢 Ready  
**Priority:** P1 - HIGH  
**Effort:** 16-20 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

BusinessCapability endpoints use CRUD. Need commands with cycle detection.

---

## Detailed Tasks

### Commands
- [ ] CreateCapability
- [ ] SetParent (parent_id) - with cycle detection
- [ ] RemoveParent ()
- [ ] UpdateDescription (description)
- [ ] DeleteCapability

### Events
- [ ] CapabilityCreated
- [ ] ParentAssigned
- [ ] ParentRemoved
- [ ] DescriptionUpdated
- [ ] CapabilityDeleted

### Validation
- [ ] Cycle detection for parent hierarchy
- [ ] Name uniqueness within parent scope

### API Changes
- [ ] POST /business-capabilities → CreateCapability
- [ ] POST /business-capabilities/{id}/commands/set-parent
- [ ] DELETE /business-capabilities/{id} → DeleteCapability

---

## Acceptance Criteria

- [ ] Cycle detection works
- [ ] All changes via commands
- [ ] Tests pass

---

## Dependencies

**Depends On:** Item-029, Item-030
