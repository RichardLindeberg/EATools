# Item-028: Design and Implement Event Store Infrastructure

**Status:** 🟢 Ready  
**Priority:** P0 - CRITICAL  
**Effort:** 16-24 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

The Event Sourcing specification (spec-architecture-event-sourcing.md) mandates that ALL state changes must be captured as immutable events in an Event Store. Currently, the system uses direct CRUD operations that mutate database tables without any event history.

**Impact:** 
- No audit trail of changes
- Cannot reconstruct historical state
- Violates compliance requirements (7-year retention)
- Cannot implement temporal queries
- Missing foundation for command-based architecture

---

## Affected Files

**Create:**
- `src/Infrastructure/EventStore.fs` - Event store module
- `src/Infrastructure/Migrations/008_create_event_store.sql` - Event store schema
- `tests/integration/test_event_store.py` - Integration tests

**Reference:**
- [spec/spec-architecture-event-sourcing.md](../spec/spec-architecture-event-sourcing.md) - Requirements

---

## Detailed Tasks

### Schema Design
- [ ] Create `events` table with schema:
  - event_id (uuid, PK)
  - aggregate_id (uuid, indexed)
  - aggregate_type (varchar, indexed)
  - aggregate_version (integer)
  - event_type (varchar, indexed)
  - event_version (integer, default 1)
  - event_timestamp (timestamptz, indexed)
  - actor (varchar)
  - actor_type (enum: user, service, system)
  - source (enum: ui, api, import, webhook, system)
  - causation_id (uuid, nullable - references command_id)
  - correlation_id (uuid, nullable, indexed)
  - data (jsonb)
  - metadata (jsonb)
  - UNIQUE constraint on (aggregate_id, aggregate_version)

- [ ] Create `commands` table for idempotency:
  - command_id (uuid, PK)
  - command_type (varchar)
  - aggregate_id (uuid, indexed)
  - aggregate_type (varchar)
  - processed_at (timestamptz)
  - actor (varchar)
  - source (enum)
  - data (jsonb)
  - UNIQUE constraint on command_id

- [ ] Create `snapshots` table:
  - snapshot_id (uuid, PK)
  - aggregate_id (uuid, indexed)
  - aggregate_type (varchar)
  - aggregate_version (integer)
  - snapshot_version (integer, default 1)
  - snapshot_timestamp (timestamptz)
  - state (jsonb)
  - UNIQUE constraint on (aggregate_id, aggregate_version)

- [ ] Create indexes:
  - events: (aggregate_id, aggregate_version)
  - events: (event_timestamp DESC)
  - events: (event_type)
  - events: (correlation_id) WHERE correlation_id IS NOT NULL
  - commands: (command_id)
  - snapshots: (aggregate_id, aggregate_version DESC)

### F# Implementation
- [ ] Create EventStore module with functions:
  - `appendEvent: Event -> Result<unit, string>` - Append event with version check
  - `getEvents: AggregateId -> Event list` - Get all events for aggregate
  - `getEventsSince: AggregateId -> Version -> Event list` - Get events after version
  - `getEventsByType: EventType -> Event list` - Query by event type
  - `getEventsInTimeRange: DateTime -> DateTime -> Event list` - Temporal queries
  - `checkCommandProcessed: CommandId -> bool` - Idempotency check
  - `recordCommand: Command -> Result<unit, string>` - Record command for deduplication

- [ ] Implement optimistic concurrency control:
  - Check aggregate_version before append
  - Return error if version conflict
  - Client must reload and retry

- [ ] Add event envelope type:
```fsharp
type EventEnvelope = {
    EventId: Guid
    AggregateId: Guid
    AggregateType: string
    AggregateVersion: int
    EventType: string
    EventVersion: int
    EventTimestamp: DateTime
    Actor: string
    ActorType: ActorType
    Source: Source
    CausationId: Guid option
    CorrelationId: Guid option
    Data: Json
    Metadata: Json
}
```

### Testing
- [ ] Unit tests for EventStore functions
- [ ] Test optimistic concurrency control (version conflicts)
- [ ] Test idempotency (duplicate command_id)
- [ ] Integration tests with real database
- [ ] Performance tests (append 10k events, query by aggregate)
- [ ] Test event ordering guarantees

### Documentation
- [ ] Add inline documentation to EventStore module
- [ ] Document schema in migration file
- [ ] Add usage examples in tests

---

## Acceptance Criteria

- [ ] Events table exists with proper schema and indexes
- [ ] Commands table exists for deduplication
- [ ] Snapshots table exists for performance optimization
- [ ] EventStore module compiles and all functions work
- [ ] Optimistic concurrency prevents version conflicts
- [ ] Command deduplication prevents duplicate processing
- [ ] All tests pass (unit + integration)
- [ ] Can append 1000 events and query in <100ms
- [ ] Migration 008 runs successfully on SQLite and MSSQL

---

## Dependencies

**Blocks:**
- Item-029 (Command Framework)
- Item-030 (CQRS Projections)
- All command migration items (031-039)

**Depends On:** None (foundational infrastructure)

---

## Notes

- This is the foundational piece for Event Sourcing architecture
- All subsequent items depend on this being completed first
- Consider using Marten library for .NET to simplify implementation (optional)
- Event store must be immutable - no UPDATE or DELETE operations
- Server-side timestamp generation is critical for security (SEC-002)

---

## References

- [spec-architecture-event-sourcing.md](../spec/spec-architecture-event-sourcing.md) - Requirements REQ-001 through REQ-021
- Event Store DB patterns: https://eventstore.com/
- Marten documentation: https://martendb.io/
