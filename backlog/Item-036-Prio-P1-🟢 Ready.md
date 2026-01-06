# Item-036: Create AuditLog as Event Projection

**Status:** 🟢 Ready  
**Priority:** P1 - HIGH  
**Effort:** 8-12 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

AuditLog is currently written separately. Should be projection from event stream.

---

## Detailed Tasks

- [ ] Create AuditLogProjection handler
- [ ] Map events to AuditLog records
- [ ] Remove separate AuditLog write logic
- [ ] Keep existing audit_logs table structure

---

## Acceptance Criteria

- [ ] AuditLog derived from events
- [ ] Existing table structure preserved
- [ ] No separate audit writes needed

---

## Dependencies

**Depends On:** Item-030
