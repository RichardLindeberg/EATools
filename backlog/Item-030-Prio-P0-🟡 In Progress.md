# Item-030: Implement CQRS Projection Framework

**Status:** 🟡 In Progress  
**Priority:** P0 - CRITICAL  
**Effort:** 12-16 hours  
**Created:** 2026-01-06  
**Owner:** GitHub Copilot

---

## Problem Statement

In CQRS architecture, current state tables (applications, organizations, etc.) are projections derived from the event stream. We need a projection engine that listens to events, applies them to read models, and supports projection rebuilds.

**Impact:**
- Cannot query current state without projections
- No mechanism to rebuild projections from events
- Cannot guarantee eventual consistency
- Missing foundation for query optimization

---

## Affected Files

**Create:**
- `src/Infrastructure/ProjectionEngine.fs` - Core projection engine
- `src/Infrastructure/ProjectionTracker.fs` - Track last processed event
- `src/Infrastructure/Projections/ApplicationProjection.fs` - Application read model
- `src/Infrastructure/Projections/OrganizationProjection.fs` - Organization read model

**Reference:**
- [spec/spec-architecture-event-sourcing.md](../spec/spec-architecture-event-sourcing.md) - REQ-028 through REQ-031

---

## Detailed Tasks

### Projection Tracker
- [ ] Create `projection_state` table
- [ ] Implement ProjectionTracker module

### Projection Engine
- [ ] Define projection handler interface
- [ ] Implement ProjectionEngine core
- [ ] Add projection rebuild functionality

### Application Projection
- [ ] Create ApplicationProjection handler

### Organization Projection
- [ ] Create OrganizationProjection handler

### Testing
- [ ] Unit tests for projection handlers
- [ ] Test idempotency and rebuilds

---

## Acceptance Criteria

- [ ] Projection engine processes events and updates read models
- [ ] Projection state tracks last processed event
- [ ] Rebuild command can regenerate projections from events
- [ ] Projections are idempotent
- [ ] All tests pass

---

## Dependencies

**Blocks:**
- Items 031–034 (command migrations)

**Depends On:**
- Item-028 (Event Store) ✅
- Item-029 (Command Framework) ✅

---

## Notes

- Projections MUST be eventually consistent
- Only projection engine writes to read models
- Dead letter queue for unhandled events

---

## References

- [spec-architecture-event-sourcing.md](../spec/spec-architecture-event-sourcing.md)
- Greg Young's CQRS documents
- Event Store projections
