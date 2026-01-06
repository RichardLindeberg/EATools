# Item-028: Design and Implement Event Store Infrastructure

**Status:** 🟡 In Progress  
**Priority:** P0 - CRITICAL  
**Effort:** 16-24 hours  
**Created:** 2026-01-06  
**Owner:** GitHub Copilot

---

## Problem Statement

The Event Sourcing specification mandates that ALL state changes must be captured as immutable events in an Event Store. Currently, the system uses direct CRUD operations that mutate database tables without any event history.

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
- `tests/EventStoreTests.fs` - Unit tests for event store

**Reference:**
- [spec/spec-architecture-event-sourcing.md](../spec/spec-architecture-event-sourcing.md) - Requirements

---

## Detailed Tasks

### Schema Design
- [ ] Create `events` table with schema and indexes
- [ ] Create `commands` table for idempotency
- [ ] Create `snapshots` table for performance optimization

### F# Implementation
- [ ] Define `IEventStore` interface
- [ ] Implement `InMemoryEventStore` for unit tests
- [ ] Implement `SqlEventStore` skeleton using `Database.fs`
- [ ] Functions: append, query, idempotency checks
- [ ] Optimistic concurrency via aggregate version

### Testing
- [ ] Unit tests: append, version conflict, idempotency
- [ ] Integration tests (later): with real database

### Documentation
- [ ] Inline docs in EventStore module
- [ ] Schema documented in migration file

---

## Acceptance Criteria

- [ ] Tables exist with proper schema and indexes
- [ ] EventStore module compiles and basic functions work
- [ ] Optimistic concurrency prevents version conflicts
- [ ] Command deduplication prevents duplicate processing
- [ ] Unit tests pass

---

## Dependencies

**Blocks:**
- Item-030 (CQRS Projections)
- Items 031–039 (command migrations)

**Depends On:** None

---

## Notes

- Event store must be immutable - no UPDATE/DELETE
- Server-side timestamps are required for security

---

## References

- [spec-architecture-event-sourcing.md](../spec/spec-architecture-event-sourcing.md)
- Event Store DB: https://eventstore.com/
- Marten: https://martendb.io/
