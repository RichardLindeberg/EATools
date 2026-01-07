# Item-029: Create Command Framework and Base Types

**Status:** ✅ Done  
**Priority:** P0 - CRITICAL  
**Effort:** 12-16 hours  
**Created:** 2026-01-06  
**Completed:** 2026-01-06  
**Owner:** GitHub Copilot

---

## Problem Statement

Event Sourcing requires a command-based architecture where all state changes are initiated via explicit Commands (not generic CRUD). Commands validate business rules and produce Events. Previously, there was no framework for commands, handlers, or validation.

**Impact:**
- Enables fine-grained, auditable operations
- Prevents fat events without command structure
- Establishes idempotency checks
- Provides standardized validation framework

---

## Affected Files

- [src/Domain/Types.fs](../../src/Domain/Types.fs)
- [src/Domain/Validation.fs](../../src/Domain/Validation.fs)
- [src/Domain/Commands.fs](../../src/Domain/Commands.fs)
- [src/Domain/Events.fs](../../src/Domain/Events.fs)
- [src/Domain/CommandHandler.fs](../../src/Domain/CommandHandler.fs)
- [src/Domain/CommandDispatcher.fs](../../src/Domain/CommandDispatcher.fs)
- [tests/CommandFrameworkTests.fs](../tests/CommandFrameworkTests.fs)
- [tests/EATool.Tests.fsproj](../tests/EATool.Tests.fsproj)

---

## Specifications

- [spec-architecture-event-sourcing.md](../spec/spec-architecture-event-sourcing.md) - REQ-001 through REQ-009

---

## Detailed Tasks

### Command Types
- [x] Define `CommandEnvelope<'TData>` and `CommandMetadata`
- [x] Define `ActorType` and `Source` DUs
- [x] Define `CommandResult<'T>` DU and mapping helper

### Event Types
- [x] Define `EventEnvelope<'TData>` and `EventMetadata`

### Command Handler Interface
- [x] Define `ICommandHandler<'TCommand, 'TAggregate, 'TEvent>` with `ValidateCommand`, `ValidateBusinessRules`, and `Handle`
- [x] Add base orchestration via dispatcher to execute validations and handle results

### Command Dispatcher
- [x] Implement dispatcher to route command to handler
- [x] Check idempotency (via `IsProcessed` and `MarkProcessed` dependencies)
- [x] Load aggregate, validate, handle, and persist events via `SaveEvents`
- [ ] Add retry logic for concurrency conflicts (deferred)

### Validation Framework
- [x] Create validation helpers (`validateRequired`, `validateLength`, etc.)
- [x] Create validation combinator `(<*>)` to aggregate errors

### Testing
- [x] Unit tests for validation helpers (combinator aggregation)
- [x] Unit tests for dispatcher idempotency and success path
- [ ] Unit tests for envelope serialization (deferred)
- [ ] Unit tests for concurrency conflict handling (deferred)

---

## Acceptance Criteria

- [x] Command and Event envelope types defined
- [x] Command handler interface created
- [x] Command dispatcher routes and executes commands
- [x] Idempotency check prevents duplicate command processing
- [x] Validation framework provides reusable validators
- [x] All current unit tests pass
- [ ] Documentation explains command/event lifecycle (deferred to Item-030/docs)
- [ ] REQ-001 through REQ-009 from spec satisfied (partially; persistence integration depends on Item-028)

---

## Dependencies

**Blocks:**
- Item-031 (Application Commands)
- Item-032 (Organization Commands)
- Item-033 (Capability Commands)
- Item-034 (Relation Commands)

**Depends On:**
- Item-028 (Event Store) - Full persistence-backed idempotency and event append

---

## Outcome

The command framework is implemented and unit-tested. The dispatcher provides idempotency checks and orchestrates validation and event emission via dependency-injected persistence functions. Full event store integration and concurrency retry logic are deferred to Item-028. Follow-on items will migrate concrete endpoints to commands using this framework (Items 031–039).

---

## References

- [spec-architecture-event-sourcing.md](../spec/spec-architecture-event-sourcing.md) - Requirements
- F# Result type: https://fsharpforfunandprofit.com/posts/recipe-part2/
