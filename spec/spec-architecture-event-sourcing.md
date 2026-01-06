---
title: Event Sourcing & Command-Based Architecture
version: 1.0
date_created: 2026-01-06
last_updated: 2026-01-06
owner: EA Platform Team
tags: [architecture, event-sourcing, commands, CQRS, audit, data-integrity]
---

# Introduction

This specification mandates Event Sourcing with Command-based architecture as the core pattern for the EA Tool. Rather than generic CRUD operations, the system uses explicit, business-focused **Commands** (AddParent, SetDataClassification, AssignToServer) that produce fine-grained **Events**. This prevents fat events and ensures the domain model captures genuine business operations.

## 1. Purpose & Scope

**Purpose**: Establish Command-based Event Sourcing as the required architectural pattern to ensure:
- Fine-grained, domain-meaningful events (not fat CRUD updates)
- Explicit business operations that are auditable and testable
- Clear domain language that matches business terminology
- Temporal queries and complete audit trails
- Prevention of mixing unrelated concerns in single operations

**Scope**:
- Command design and dispatch
- Event store and immutable event stream
- Aggregate root pattern with command handlers
- Snapshot strategy for performance
- Event projections for queries
- CQRS (Command Query Responsibility Segregation)
- Relationship to REST API (commands may map to REST endpoints)

**Out of Scope**: 
- Event Bus infrastructure implementation details
- Distributed tracing (separate observability spec)
- Saga orchestration (separate process spec)

**Audience**: Architects, backend developers, domain modelers, compliance officers.

**Assumptions**:
- Commands are the entry points for all state changes
- Each command handler validates and produces events
- Events are immutable and time-ordered
- Current state is derived from event projections
- Fat events indicate missing domain concepts (anti-pattern)

## 2. Definitions

- **Command**: An explicit request to perform a specific business operation (e.g., AddParentToCapability, SetDataClassification)
- **Command Handler**: Logic that validates a command and produces events
- **Event**: An immutable record of something that happened (e.g., ParentAdded, DataClassificationSet)
- **Event Sourcing**: Storing state as an immutable sequence of events
- **Event Store**: The authoritative database of all domain events
- **Aggregate**: A consistency boundary (e.g., Application with its Services and Interfaces)
- **Aggregate Root**: Primary entity in an aggregate that handles commands
- **Projection**: Read model derived from events, optimized for queries
- **CQRS**: Command Query Responsibility Segregation - separate write model (commands/events) from read model (projections)
- **Snapshot**: Point-in-time copy of aggregate state to optimize replay
- **Event Envelope**: Wrapper around event with metadata (id, timestamp, actor, causation_id)
- **Compensation Event**: Event that corrects a previous event (for audit trail rather than deletion)

## 3. Requirements, Constraints & Guidelines

### Command Architecture Requirements

- **REQ-001**: ALL state changes MUST be initiated via Commands, never direct database mutations
- **REQ-002**: Commands MUST be named in imperative tense (e.g., AddParent, SetDataClassification, not ParentAdded)
- **REQ-003**: Commands MUST represent specific business operations, never generic updates (REQ-003a: Commands like "UpdateApplication" with arbitrary field changes are forbidden)
- **REQ-004**: Commands MUST be small and focused (typically 2-5 fields for the change being made)
- **REQ-005**: Each Command MUST have a unique command_id (UUID v4) for idempotency
- **REQ-006**: Commands MUST include causation context (actor, source, timestamp, correlation_id)
- **REQ-007**: Commands MUST validate business rules before producing events (fail-fast)
- **REQ-008**: Command handlers MUST be pure functions: (Aggregate, Command) → List<Event>
- **REQ-009**: Commands MUST be idempotent—replaying the same command must be safe (deduplicated by command_id)

### Event Requirements

- **REQ-010**: Events MUST be produced by command handlers, never created directly
- **REQ-011**: Events MUST be named in past tense (e.g., ParentAdded, DataClassificationSet)
- **REQ-012**: Events MUST be fine-grained (one event = one meaningful business fact)
- **REQ-013**: Events MUST NOT be fat or contain unrelated data (anti-pattern: single event with 20+ fields)
- **REQ-014**: Each event MUST have: event_id (uuid), aggregate_id, event_timestamp, actor, source
- **REQ-015**: Events MUST be immutable—never modified or deleted (only new compensation events)
- **REQ-016**: Event sequences MUST support replay to reconstruct any aggregate state

### Audit & Compliance Requirements

- **REQ-017**: Every Command execution MUST be recorded as events in the Event Store
- **REQ-018**: Event Store MUST support time-travel queries (state at any point in time)
- **REQ-019**: Events MUST include complete context: who (actor), what (event_type), when (timestamp), where (source), why (correlation_id, business_justification)
- **REQ-020**: Events MUST NOT be deleted or truncated; retention is mandatory
- **REQ-021**: Event retention MUST comply with governance policies (minimum 7 years for audit)
- **REQ-022**: Critical operations (create, delete, classification change) MUST trigger audit alerts

### Validation & Consistency Requirements

- **REQ-023**: Commands MUST validate against current aggregate state (command handler checks preconditions)
- **REQ-024**: Business rules MUST be enforced in command handlers, not in projections
- **REQ-025**: Circular references (parent cycles) MUST be prevented by command validation
- **REQ-026**: Lifecycle transitions MUST follow valid paths (planned → active → deprecated → retired)
- **REQ-027**: Commands MUST fail with descriptive errors if preconditions not met

### CQRS Requirements

- **REQ-028**: Write model (commands/events) and read model (projections) MUST be separate
- **REQ-029**: Projections are derived from events and can be regenerated
- **REQ-030**: Queries MUST read from projections, never directly replay events
- **REQ-031**: Projections MUST be eventually consistent with the event stream

### Snapshot Requirements

- **REQ-032**: Aggregates with >100 events MUST use snapshots
- **REQ-033**: Snapshots MUST be verified by replaying post-snapshot events
- **REQ-034**: Snapshots MUST be automatically created every 100 events

### Security Requirements

- **SEC-001**: Commands with sensitive data (passwords, secrets) MUST be excluded from audit logs
- **SEC-002**: Event timestamps MUST be server-generated, not client-provided
- **SEC-003**: Command dispatch MUST check authorization before handler execution
- **SEC-004**: Compensation events MUST reference original event and approval chain

### Design Guidelines

- **GUD-001**: Design commands to match business terminology (use ubiquitous language)
- **GUD-002**: Each command should represent a single business decision or action
- **GUD-003**: If a command feels like it's doing multiple unrelated things, split it into multiple commands
- **GUD-004**: Include "why" in command context (business_justification, approval_id)
- **GUD-005**: Commands are immutable once dispatched; use new commands for corrections
- **GUD-006**: Events should be self-contained (include all data needed to understand them)
- **GUD-007**: Use event upcasting for schema evolution without losing event fidelity
- **GUD-008**: Snapshots should capture full aggregate state at a point in time
- **GUD-009**: Design for replay—events must be pure transformations of state
- **GUD-010**: Avoid temporal coupling—events must not assume execution order beyond their own sequence

## 4. Interfaces & Data Contracts

### Command Envelope (All commands)

```json
{
  "command_id": "string (uuid v4, required, for idempotency)",
  "command_type": "string (required, e.g., 'AddParentToCapability')",
  "aggregate_id": "string (uuid, required, the entity being modified)",
  "aggregate_type": "string (required, e.g., 'BusinessCapability')",
  "correlation_id": "string (uuid, optional, links related operations)",
  "actor": "string (required, user_id or service_id)",
  "actor_type": "enum (required, 'user'|'service'|'system')",
  "source": "enum (required, 'ui'|'api'|'import'|'webhook'|'system')",
  "command_timestamp": "string (ISO 8601 UTC with Z)",
  "data": "object (required, command-specific payload)",
  "metadata": {
    "ip_address": "string (optional)",
    "correlation_id": "string (optional, links to command that triggered this)",
    "business_justification": "string (optional, why this change is needed)",
    "approval_id": "string (optional, approval workflow reference)"
  }
}
```

### Example Commands

#### AddParentToCapability

```json
{
  "command_id": "cmd-001",
  "command_type": "AddParentToCapability",
  "aggregate_id": "cap-child-uuid",
  "aggregate_type": "BusinessCapability",
  "actor": "user-123",
  "actor_type": "user",
  "source": "api",
  "command_timestamp": "2026-01-06T10:00:00Z",
  "data": {
    "parent_id": "cap-parent-uuid"
  },
  "metadata": {
    "business_justification": "Reorganizing capability hierarchy for Q1 initiative"
  }
}
```

Produces: **ParentAssigned** event
```json
{
  "event_id": "evt-001",
  "event_type": "ParentAssigned",
  "aggregate_id": "cap-child-uuid",
  "aggregate_type": "BusinessCapability",
  "aggregate_version": 2,
  "event_timestamp": "2026-01-06T10:00:00Z",
  "actor": "user-123",
  "causation_id": "cmd-001",
  "data": {
    "parent_id": "cap-parent-uuid",
    "previous_parent_id": null
  }
}
```

#### SetDataClassification

```json
{
  "command_id": "cmd-002",
  "command_type": "SetDataClassification",
  "aggregate_id": "app-uuid-001",
  "aggregate_type": "Application",
  "actor": "security-officer-456",
  "actor_type": "user",
  "source": "api",
  "command_timestamp": "2026-01-06T10:05:00Z",
  "data": {
    "classification": "confidential",
    "reason": "Contains customer payment data"
  },
  "metadata": {
    "business_justification": "Security review identified PII handling"
  }
}
```

Produces: **DataClassificationChanged** event
```json
{
  "event_id": "evt-002",
  "event_type": "DataClassificationChanged",
  "aggregate_id": "app-uuid-001",
  "aggregate_type": "Application",
  "aggregate_version": 3,
  "event_timestamp": "2026-01-06T10:05:00Z",
  "actor": "security-officer-456",
  "causation_id": "cmd-002",
  "data": {
    "previous_classification": "internal",
    "new_classification": "confidential",
    "reason": "Contains customer payment data"
  }
}
```

#### AssignToServer

```json
{
  "command_id": "cmd-003",
  "command_type": "AssignToServer",
  "aggregate_id": "app-uuid-001",
  "aggregate_type": "Application",
  "actor": "platform-ops-789",
  "actor_type": "user",
  "source": "api",
  "command_timestamp": "2026-01-06T10:10:00Z",
  "data": {
    "server_id": "server-prod-001",
    "environment": "prod"
  },
  "metadata": {
    "business_justification": "Deploying to production after testing"
  }
}
```

Produces: **DeployedToServer** event
```json
{
  "event_id": "evt-003",
  "event_type": "DeployedToServer",
  "aggregate_id": "app-uuid-001",
  "aggregate_type": "Application",
  "aggregate_version": 4,
  "event_timestamp": "2026-01-06T10:10:00Z",
  "actor": "platform-ops-789",
  "causation_id": "cmd-003",
  "data": {
    "server_id": "server-prod-001",
    "environment": "prod",
    "previous_server_ids": []
  }
}
```

#### TransitionLifecycle

```json
{
  "command_id": "cmd-004",
  "command_type": "TransitionLifecycle",
  "aggregate_id": "app-uuid-001",
  "aggregate_type": "Application",
  "actor": "architect-user-001",
  "actor_type": "user",
  "source": "api",
  "command_timestamp": "2026-01-06T11:00:00Z",
  "data": {
    "target_lifecycle": "deprecated",
    "sunset_date": "2026-06-30"
  },
  "metadata": {
    "business_justification": "Application being replaced by NextGen Platform"
  }
}
```

Produces: **LifecycleTransitioned** event
```json
{
  "event_id": "evt-004",
  "event_type": "LifecycleTransitioned",
  "aggregate_id": "app-uuid-001",
  "aggregate_type": "Application",
  "aggregate_version": 5,
  "event_timestamp": "2026-01-06T11:00:00Z",
  "actor": "architect-user-001",
  "causation_id": "cmd-004",
  "data": {
    "from_lifecycle": "active",
    "to_lifecycle": "deprecated",
    "sunset_date": "2026-06-30"
  }
}
```

#### RemoveParentFromCapability

```json
{
  "command_id": "cmd-005",
  "command_type": "RemoveParentFromCapability",
  "aggregate_id": "cap-child-uuid",
  "aggregate_type": "BusinessCapability",
  "actor": "user-123",
  "actor_type": "user",
  "source": "api",
  "command_timestamp": "2026-01-06T11:30:00Z",
  "data": {
    "reason": "Restructuring complete, promoting to top-level"
  },
  "metadata": {
    "business_justification": "Organizational restructure"
  }
}
```

Produces: **ParentRemoved** event

### Comparison: Fat Event vs Fine-Grained Events

❌ **BAD: Fat CRUD Update Event**
```json
{
  "event_id": "bad-evt-001",
  "event_type": "ApplicationUpdated",
  "aggregate_id": "app-uuid-001",
  "data": {
    "name": "New Name",
    "owner": "new-owner",
    "lifecycle": "deprecated",
    "data_classification": "confidential",
    "criticality": "high",
    "tags": ["security-review"],
    "deployed_on": ["server-1", "server-2"],
    "business_capability_id": "cap-uuid-002",
    "description": "Updated description",
    "some_other_field": "value"
  }
}
```
**Problem**: Single event mixes 10 unrelated changes. Hard to understand what actually changed. Hard to enforce business rules (e.g., classification change requires different approval than deployment).

✅ **GOOD: Fine-Grained Domain Events**
```json
[
  {"event_type": "ParentCapabilityAssigned", "data": {"capability_id": "cap-uuid-002"}},
  {"event_type": "DataClassificationChanged", "data": {"new_classification": "confidential"}},
  {"event_type": "LifecycleTransitioned", "data": {"to_lifecycle": "deprecated"}},
  {"event_type": "CriticalitySet", "data": {"criticality": "high"}},
  {"event_type": "DeployedToServer", "data": {"server_id": "server-1"}},
  {"event_type": "TagsAdded", "data": {"tags": ["security-review"]}},
  {"event_type": "DescriptionUpdated", "data": {"description": "Updated description"}}
]
```
**Benefits**: Each event is clear and focused. Different events can have different audit requirements. Easy to understand what changed and why.

## 5. Acceptance Criteria

- **AC-001**: Given a command is dispatched, When it passes validation, Then exactly one or more focused events MUST be produced and persisted
- **AC-002**: Given an aggregate state, When a command is replayed with the same command_id, Then no duplicate events MUST be created (idempotency)
- **AC-003**: Given a parent-child command would create a cycle, When the command handler validates, Then it MUST reject with specific error
- **AC-004**: Given an AddParent command, When successful, Then a single ParentAssigned event (not fat ApplicationUpdated) MUST be created
- **AC-005**: Given a SetDataClassification command, When successful, Then exactly one DataClassificationChanged event MUST be produced
- **AC-006**: Given multiple commands for same aggregate, When each is processed, Then events MUST be recorded in temporal order with sequential aggregate_version
- **AC-007**: Given a command without business_justification, When dispatched, Then validation MUST fail for sensitive operations (classification, deletion)
- **AC-008**: Given events are replayed, When aggregated from event_version 1, Then final state MUST match current projection exactly
- **AC-009**: Given a fat UpdateApplication command, When code review occurs, Then it MUST be rejected as violating REQ-003a
- **AC-010**: Given event sourcing is implemented, When audit is queried, Then each change MUST trace back to a specific command with actor and justification

## 6. Test Automation Strategy

- **Test Levels**: Unit (command handlers), Integration (full command→event→projection flow), End-to-End
- **Frameworks**: XUnit/NUnit with domain-driven design test libraries (GivenWhenThen pattern)
- **Command Handler Tests**: Test each handler with valid/invalid commands, boundary conditions
- **Event Tests**: Verify events are immutable, can be replayed, produce correct state
- **Projection Tests**: Verify projections match replayed events, handle out-of-order events
- **Idempotency Tests**: Replay same command_id multiple times, verify single event produced
- **CI/CD Integration**: Automated command/event tests in GitHub Actions
- **Coverage Requirements**: 
  - Command handlers: 90%+
  - Projections: 85%+
  - Event handlers: 90%+
- **Property-Based Testing**: Use QuickCheck to verify replay properties (idempotency, commutativity where applicable)

## 7. Rationale & Context

**Why Commands Instead of CRUD?**

Commands are explicit business operations:
- "SetDataClassification" is more meaningful than "UpdateApplication(fields: {...})"
- Each command can have different audit requirements, approval workflows, validation rules
- Commands prevent mixing unrelated concerns (setting classification ≠ deploying server)
- Fat events are a code smell indicating missing domain concepts

**Why Event Sourcing with Commands?**

1. **Immutable Audit Trail**: Every command produces auditable events
2. **Temporal Queries**: Reconstruct state at any point in time
3. **Compliance**: Complete context (who, what, when, why) for each change
4. **Scalability**: Event store is append-only; projections scale independently
5. **Testability**: Commands and events are pure functions, easily testable

**How Commands Avoid Fat Events**

- Command: SetDataClassification(classification: "confidential")
- Event: DataClassificationChanged(from: "internal", to: "confidential")
- Single responsibility: One event = one state change

Compare to:
- Command: UpdateApplication(fields: {classification, owner, lifecycle, server_id, ...})
- Event: ApplicationUpdated(all 10+ fields)
- Violates single responsibility

**Relationship to REST API**

Commands may map 1:1 to REST endpoints:
- POST /applications/{id}/commands/set-classification (body: {classification, reason})
- POST /capabilities/{id}/commands/add-parent (body: {parent_id})
- POST /applications/{id}/commands/deploy (body: {server_id})

Or REST could be a convenience layer that dispatches multiple commands:
- PUT /applications/{id} (body: {classification, lifecycle, owner, ...}) → dispatches multiple commands

## 8. Dependencies & External Integrations

### Infrastructure Dependencies

- **INF-001**: Event Store - PostgreSQL with EventStoreDB or MSSQL with custom schema
- **INF-002**: Projection Store - Read-optimized database (PostgreSQL, MSSQL, Redis)
- **INF-003**: Command Dispatcher - In-process (Mediatr, MessageBus) or distributed (RabbitMQ, Service Bus)
- **INF-004**: Snapshot Store - Same as Event Store or dedicated fast store (Redis)

### Technology Platform Dependencies

- **PLT-001**: Event Sourcing Library - Marten (.NET), EventStoreDB, or custom implementation
- **PLT-002**: Domain Event Publishing - Mediatr, custom in-process bus
- **PLT-003**: Schema Evolution - Event upcasting library

### Compliance Dependencies

- **COM-001**: GDPR - Event retention, right to erasure (compensation events vs deletion)
- **COM-002**: HIPAA - Audit controls, immutability enforcement
- **COM-003**: SOX - Change management, segregation of duties, audit trail integrity

## 9. Examples & Edge Cases

### Edge Case: Validation Error in Command Handler

Command rejected before events produced:

```csharp
// Pseudocode
public async Task<Result<List<Event>>> Handle(AddParentToCapability cmd, Aggregate agg)
{
    if (agg.ParentId == cmd.ParentId) 
        return Error("Already has this parent");
    
    if (WouldCreateCycle(agg.Id, cmd.ParentId))
        return Error("Would create cycle: X → Y → Z → X");
    
    // Command is valid, produce event
    return Success(new[] { new ParentAssigned(agg.Id, cmd.ParentId) });
}
```

### Edge Case: Compensation for Erroneous Command

Correct a mistake by producing compensation events:

```json
{
  "command_id": "cmd-correction-001",
  "command_type": "CorrectionCommand",
  "aggregate_id": "app-uuid-001",
  "aggregate_type": "Application",
  "actor": "admin-user",
  "source": "system",
  "data": {
    "corrects_event_id": "evt-002",
    "correction_reason": "Wrong classification assigned, should be 'internal' not 'confidential'",
    "corrected_value": "internal"
  }
}
```

Produces: **DataClassificationCorrected** event (references original, doesn't delete it)

### Edge Case: Multi-Step Command Sequence

User performs multiple operations on one aggregate:

```json
// Step 1: Create application
{"command_type": "CreateApplication", "data": {...}} → ApplicationCreated event

// Step 2: Assign parent capability
{"command_type": "AssignCapability", "data": {...}} → CapabilityAssigned event

// Step 3: Set classification
{"command_type": "SetDataClassification", "data": {...}} → DataClassificationChanged event

// Step 4: Deploy to server
{"command_type": "DeployToServer", "data": {...}} → DeployedToServer event
```

Each command produces one event. Events are recorded in order. Aggregate version increments with each event.

## 10. Validation Criteria

1. ✅ Commands are specific business operations (not generic updates)
2. ✅ Events are fine-grained (one event = one meaningful change)
3. ✅ No fat events with 10+ unrelated fields
4. ✅ Command handlers validate preconditions before producing events
5. ✅ Events include complete audit context (actor, timestamp, justification)
6. ✅ Event replay reconstructs correct state
7. ✅ Commands are idempotent (by command_id)
8. ✅ Projections are derived from events and can be regenerated
9. ✅ Circular references prevented by command validation
10. ✅ Each state change is auditable and traceable to a command

## 11. Related Specifications

- **[Domain Model Overview](spec-schema-domain-overview.md)** - Requirements for audit trails and temporal queries
- **[Supporting Entities - AuditLog](spec-schema-entities-supporting.md)** - AuditLog as projection of event stream
- **[API Contract](spec-tool-api-contract.md)** - How commands map to REST endpoints
- **[Data Architecture](spec-architecture-data.md)** *(planned)* - Event store and projection databases
- **[System Architecture](spec-architecture-system-design.md)** *(planned)* - Overall component design
- **[Monitoring & Observability](spec-process-observability.md)** *(planned)* - Monitoring event flow and projections

