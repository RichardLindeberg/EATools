# Item-030: Implement CQRS Projection Framework

**Status:** ✅ Done  
**Priority:** P0 - CRITICAL  
**Effort:** 12-16 hours  
**Created:** 2026-01-06  
**Completed:** 2026-01-06  
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
- [x] Create `projection_state` table
- [x] Implement ProjectionTracker module

### Projection Engine
- [x] Define projection handler interface
- [x] Implement ProjectionEngine core
- [x] Add projection rebuild functionality

### Application Projection
- [x] Create ApplicationProjection handler

### Organization Projection
- [x] Create OrganizationProjection handler

### Testing
- [x] Unit tests for projection handlers
- [x] Test idempotency and rebuilds

---

## Acceptance Criteria

- [x] Projection engine processes events and updates read models
- [x] Projection state tracks last processed event
- [x] Rebuild command can regenerate projections from events
- [x] Projections are idempotent
- [x] All tests pass

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
