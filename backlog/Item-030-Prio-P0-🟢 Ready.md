
# Item-030: Implement CQRS Projection Framework

**Status:** 🟢 Ready  
**Priority:** P0 - CRITICAL  
**Effort:** 12-16 hours  
**Created:** 2026-01-06  
**Owner:** TBD

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
- `tests/integration/test_projections.py` - Integration tests

**Modify:**
- Existing repository files to read from projections

**Reference:**
- [spec-architecture-event-sourcing.md](../spec/spec-architecture-event-sourcing.md) - REQ-028 through REQ-031

---

## Detailed Tasks

### Projection Tracker
- [ ] Create `projection_state` table:
  - projection_name (varchar, PK)
  - last_processed_event_id (uuid)
  - last_processed_at (timestamptz)
  - last_processed_version (bigint)
  - status (enum: active, rebuilding, failed)

- [ ] Implement ProjectionTracker module:
  - `getLastProcessedEvent: ProjectionName -> EventId option`
  - `updateLastProcessed: ProjectionName -> EventId -> Result<unit, string>`
  - `markRebuilding: ProjectionName -> Result<unit, string>`
  - `markActive: ProjectionName -> Result<unit, string>`

### Projection Engine
- [ ] Define projection handler interface:
```fsharp
type IProjectionHandler =
    abstract member ProjectionName: string
    abstract member Handle: EventEnvelope -> Result<unit, string>
    abstract member CanHandle: EventType -> bool
```

- [ ] Implement ProjectionEngine:
  - Subscribe to event stream
  - Route events to registered handlers
  - Update projection state after processing
  - Handle failures (retry, dead letter)
  - Support batch processing for performance

- [ ] Add projection rebuild functionality:
  - Clear projection tables
  - Mark as "rebuilding"
  - Replay all events from beginning
  - Mark as "active" when complete
  - Atomic cutover from old to new projection

### Application Projection
- [ ] Create ApplicationProjection handler:
  - Handle ApplicationCreated → INSERT into applications
  - Handle DataClassificationChanged → UPDATE classification
  - Handle LifecycleTransitioned → UPDATE lifecycle
  - Handle OwnerSet → UPDATE owner
  - Handle TagsAdded → UPDATE tags (array append)
  - Handle TagsRemoved → UPDATE tags (array remove)
  - Handle ApplicationDeleted → UPDATE deleted_at

- [ ] Ensure idempotency (event_id tracking)

### Organization Projection
- [ ] Create OrganizationProjection handler:
  - Handle OrganizationCreated → INSERT
  - Handle ParentAssigned → UPDATE parent_id
  - Handle ParentRemoved → UPDATE parent_id = NULL
  - Handle ContactInfoUpdated → UPDATE contacts
  - Handle DomainAdded → UPDATE domains (array append)
  - Handle DomainRemoved → UPDATE domains (array remove)
  - Handle OrganizationDeleted → UPDATE deleted_at

### Projection Verification
- [ ] Create verification tool:
  - Replay events for aggregate
  - Compare computed state to projection
  - Report discrepancies
  - Useful for debugging projection bugs

### Testing
- [ ] Unit tests for projection handlers
- [ ] Test idempotency (processing same event twice)
- [ ] Test event ordering
- [ ] Test projection rebuild from scratch
- [ ] Integration tests with real event store
- [ ] Performance tests (project 10k events)
- [ ] Test projection verification tool

### Documentation
- [ ] Document projection architecture
- [ ] Document how to add new projections
- [ ] Document projection rebuild procedure
- [ ] Add troubleshooting guide for projection failures

---

## Acceptance Criteria

- [ ] Projection engine processes events and updates read models
- [ ] Application projection correctly reflects all application events
- [ ] Organization projection correctly reflects all organization events
- [ ] Projection state tracks last processed event
- [ ] Rebuild command can regenerate projections from events
- [ ] Projections are idempotent (no duplicate processing)
- [ ] All tests pass (unit + integration)
- [ ] Can project 1000 events in <1 second
- [ ] Verification tool detects projection inconsistencies

---

## Dependencies

**Blocks:**
- Item-031 (Application Commands) - Need projections for queries
- Item-032 (Organization Commands)
- Item-033 (Capability Commands)
- Item-034 (Relation Commands)

**Depends On:**
- Item-028 (Event Store) - Projections read from event store
- Item-029 (Command Framework) - Events produced by commands

---

## Notes

- Projections MUST be eventually consistent (not immediate)
- Current state tables become read-only from API perspective
- Only projection engine writes to read models
- Consider using materialized views for complex queries
- Projection failures should not block command processing
- Dead letter queue for unhandled events

---

## References

- [spec-architecture-event-sourcing.md](../spec/spec-architecture-event-sourcing.md) - CQRS requirements
- Greg Young's CQRS documents: https://cqrs.files.wordpress.com/2010/11/cqrs_documents.pdf
- Event Store projections: https://eventstore.com/
