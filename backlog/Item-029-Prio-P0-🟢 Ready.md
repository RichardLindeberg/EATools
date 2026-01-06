# Item-029: Create Command Framework and Base Types

**Status:** 🟢 Ready  
**Priority:** P0 - CRITICAL  
**Effort:** 12-16 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

Event Sourcing requires a command-based architecture where all state changes are initiated via explicit Commands (not generic CRUD). Commands validate business rules and produce Events. Currently, there's no framework for commands, handlers, or validation.

**Impact:**
- Cannot implement fine-grained, auditable operations
- Fat events will occur without command structure
- No idempotency support
- No standardized validation framework

---

## Affected Files

**Create:**
- `src/Domain/Commands.fs` - Command envelope and base types
- `src/Domain/Events.fs` - Event envelope and base types
- `src/Domain/CommandHandler.fs` - Command handler abstraction
- `src/Domain/CommandDispatcher.fs` - Command dispatch with idempotency
- `tests/unit/test_commands.fs` - Unit tests

**Reference:**
- [spec-architecture-event-sourcing.md](../spec/spec-architecture-event-sourcing.md) - REQ-001 through REQ-009

---

## Detailed Tasks

### Command Types
- [ ] Define Command envelope type:
```fsharp
type CommandEnvelope<'TData> = {
    CommandId: Guid
    CommandType: string
    AggregateId: Guid
    AggregateType: string
    CorrelationId: Guid option
    Actor: string
    ActorType: ActorType
    Source: Source
    CommandTimestamp: DateTime
    Data: 'TData
    Metadata: CommandMetadata
}

type CommandMetadata = {
    IpAddress: string option
    BusinessJustification: string option
    ApprovalId: string option
}

type ActorType = User | Service | System
type Source = UI | API | Import | Webhook | System
```

- [ ] Create command validation result type:
```fsharp
type CommandResult<'T> =
    | Success of 'T
    | ValidationError of string list
    | BusinessRuleViolation of string
    | ConcurrencyConflict of string
```

### Event Types
- [ ] Define Event envelope type:
```fsharp
type EventEnvelope<'TData> = {
    EventId: Guid
    EventType: string
    EventVersion: int
    EventTimestamp: DateTime
    AggregateId: Guid
    AggregateType: string
    AggregateVersion: int
    CausationId: Guid option  // command_id that caused this
    CorrelationId: Guid option
    Actor: string
    ActorType: ActorType
    Source: Source
    Data: 'TData
    Metadata: EventMetadata option
}

type EventMetadata = {
    Tags: string list option
    BusinessJustification: string option
}
```

### Command Handler Interface
- [ ] Define command handler interface:
```fsharp
type ICommandHandler<'TCommand, 'TAggregate> =
    abstract member Handle: 'TCommand -> 'TAggregate -> CommandResult<EventEnvelope list>
    abstract member ValidateCommand: 'TCommand -> Result<unit, string list>
    abstract member ValidateBusinessRules: 'TCommand -> 'TAggregate -> Result<unit, string>
```

- [ ] Create base command handler with template method:
```fsharp
type CommandHandlerBase<'TCommand, 'TAggregate>() =
    member this.Execute(cmd: CommandEnvelope<'TCommand>, agg: 'TAggregate) =
        // 1. Check idempotency (command_id already processed?)
        // 2. Validate command structure
        // 3. Validate business rules against aggregate
        // 4. Produce events
        // 5. Record command as processed
```

### Command Dispatcher
- [ ] Implement command dispatcher:
  - Route command to appropriate handler
  - Check idempotency (query commands table)
  - Load current aggregate state
  - Call handler
  - Append events to event store
  - Record command in commands table
  - Return success/failure

- [ ] Add retry logic for concurrency conflicts

### Validation Framework
- [ ] Create validation helpers:
  - `validateRequired: string -> Result<unit, string>`
  - `validateLength: min -> max -> string -> Result<unit, string>`
  - `validateEmail: string -> Result<unit, string>`
  - `validateUrl: string -> Result<unit, string>`
  - `validateUuid: string -> Result<unit, string>`
  - `validateEnum: 'T list -> 'T -> Result<unit, string>`

- [ ] Create validation combinator:
```fsharp
let (<*>) f1 f2 = 
    match f1, f2 with
    | Ok (), Ok () -> Ok ()
    | Error e1, Error e2 -> Error (e1 @ e2)
    | Error e, _ | _, Error e -> Error e
```

### Testing
- [ ] Unit tests for command envelope serialization
- [ ] Unit tests for event envelope serialization
- [ ] Unit tests for validation helpers
- [ ] Test command dispatcher idempotency
- [ ] Test concurrency conflict handling
- [ ] Test validation error aggregation

---

## Acceptance Criteria

- [ ] Command and Event envelope types defined
- [ ] Command handler interface created
- [ ] Command dispatcher routes and executes commands
- [ ] Idempotency check prevents duplicate command processing
- [ ] Validation framework provides reusable validators
- [ ] All unit tests pass
- [ ] Documentation explains command/event lifecycle
- [ ] REQ-001 through REQ-009 from spec satisfied

---

## Dependencies

**Blocks:**
- Item-031 (Application Commands)
- Item-032 (Organization Commands)
- Item-033 (Capability Commands)
- Item-034 (Relation Commands)

**Depends On:**
- Item-028 (Event Store) - Commands must record to event store

---

## Notes

- Commands are named in imperative tense (AddParent, SetClassification)
- Events are named in past tense (ParentAdded, ClassificationSet)
- Command handlers MUST be pure functions
- Fail-fast validation before event production
- Consider MediatR library for command dispatch (optional)

---

## References

- [spec-architecture-event-sourcing.md](../spec/spec-architecture-event-sourcing.md) - Requirements
- F# Result type: https://fsharpforfunandprofit.com/posts/recipe-part2/
