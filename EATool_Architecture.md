# EATool - Architecture Plan

## Executive Summary
EATool is an event-sourced, CQRS-based enterprise architecture registry. It exposes REST endpoints (F#/.NET) for managing domain entities (Applications, Organizations, Business Capabilities, Servers, Integrations, Data Entities) and their Relations. Writes produce domain events persisted to an Event Store; projections update read models for query endpoints. Development uses SQLite; production will adopt MSSQL/PostgreSQL. The architecture prioritizes clarity, consistency, and evolvability.

## System Context
```mermaid
C4Context
title EATool System Context

Person(User, "API Client / Tester")
System(EATool, "EATool API", "Event-sourced registry")
SystemDb(EventStore, "Event Store", "SQLite dev / MSSQL prod")
SystemDb(ReadDB, "Read Model DB", "SQLite dev / MSSQL prod")

Rel(User, EATool, "HTTP/JSON")
Rel(EATool, EventStore, "Append events, idempotent commands")
Rel(EATool, ReadDB, "Projections update read models")
```

Overview: Shows the EATool API interacting with clients and persistence layers.
- Key Components: `EATool API`, `Event Store`, `Read Model DB`.
- Relationships: Clients call HTTP endpoints; API appends events to Event Store; Projection engine updates Read Model DB.
- Design Decisions: Event sourcing for auditability and flexibility; CQRS for separation of reads/writes.
- NFR Considerations:
  - Scalability: Horizontal scale read side; partition event streams.
  - Performance: Fast reads via read models; append-only writes.
  - Security: AuthZ/AuthN (future), least privilege DB access.
  - Reliability: Idempotent commands; versioned events; projection checkpointing.
  - Maintainability: Clear boundaries; schema-evolution via events.
- Trade-offs: Added complexity for event sourcing vs simpler CRUD.
- Risks/Mitigations: Projection lag → checkpoint + replay; event versioning → strict schema evolution.

## Architecture Overview
Pattern: CQRS + Event Sourcing.
- Write path: Commands → Validation/Handlers → Events → Event Store.
- Read path: Projection engine → Read models → Query endpoints.
- Serialization: Thoth.Json for event payloads.
- Migrations: Forward-only SQL files; automated at startup.

## Component Architecture
```mermaid
flowchart LR
  A[API Layer - Giraffe + ASP.NET Core] --> B[Command Router]
  B --> C[Domain Command Handlers]
  C --> D[Event Store]
  D --> E[Projection Engine]
  E --> F[Read Model Repositories]
  F --> G[Query Endpoints]
  C --> H[Validation Matrix - Relations]
  C --> I[JSON Serialization - Thoth]
  J[Migration Runner] --> D
  J --> F
```

Overview: Major components and dependencies.
- Component Responsibilities:
  - API Layer: HTTP routes; envelope parsing; error handling.
  - Command Router: Dispatches to domain handlers.
  - Command Handlers: Validate, produce events, enforce invariants (e.g., relation matrix).
  - Event Store: Persist events/commands/snapshots.
  - Projection Engine: Transforms events into read models.
  - Read Model Repositories: CRUD on read tables.
  - Migration Runner: Applies DB migrations.
- Communication: Synchronous in-process for dev; future async projections possible.
- Design Decisions: Keep handlers pure; write-side isolated from read-side schemas.
- NFRs: Modularity aids maintainability; repositories optimize queries.
- Trade-offs: Projections introduce eventual consistency.
- Risks: Schema drift → use projection_state tracking; serialization migration → event versioning.

## Deployment Architecture
```mermaid
flowchart TB
  subgraph Dev[Development]
    AppDev[EATool Service] --- SQLiteDev[(SQLite File)]
    AppDev --- ProjDev[(Read Models - SQLite)]
  end
  subgraph Prod[Production]
    LB[Ingress / Load Balancer]
    AppPool[EATool App Pool]
    DB1[(MSSQL/PostgreSQL - Event Store)]
    DB2[(MSSQL/PostgreSQL - Read Models)]
    Mon[Monitoring/Logging]
    LB --> AppPool
    AppPool --- DB1
    AppPool --- DB2
    AppPool --- Mon
  end

  Dev --> Prod
```

Overview: Physical/logical deployment in dev vs prod.
- Environments: Dev (local SQLite); Prod (managed DB, containerized app).
- Network Boundaries: Ingress to app; DB in private subnet; role-based access.
- Deployment Strategy: Containers; rolling updates; migrations on startup.
- NFRs:
  - Scalability: Scale AppPool horizontally; isolate read DB for replicas.
  - Performance: Connection pooling; indexes on hot paths.
  - Security: TLS, secrets management, DB user separation.
  - Reliability: Health checks; backups; point-in-time recovery.
  - Maintainability: Infra-as-code; versioned migrations.
- Trade-offs: SQLite simplicity vs limited concurrency.
- Risks: Migration failures → pre-flight checks; connection limits → pooling.

## Data Flow
```mermaid
flowchart LR
  C1[Client POST Create Relation] --> W1[API Validate and Dispatch]
  W1 --> W2[Handler Matrix and Rules Check]
  W2 --> W3[Emit RelationCreated Event]
  W3 --> ES[(Event Store)]
  ES --> PRJ[Projection Engine]
  PRJ --> RM[(Relations Read Model)]
  Q1[Client GET Query Relations] --> RM
```

Overview: End-to-end write and read flows for Relations.
- Data Sources/Sinks: HTTP clients; Event Store; Read Models.
- Transformations: Validation → Event creation → Projection.
- Handling Strategy: Append-only events; projection idempotence; checkpointing.
- NFRs: Fast reads; durable writes; audit trail.
- Trade-offs: Eventual consistency between write and read.
- Risks: Incomplete projections → checkpoint replay; malformed events → strict decoders.

## Key Workflows
```mermaid
sequenceDiagram
  participant C as Client
  participant API as EATool API
  participant H as Command Handler
  participant ES as Event Store
  participant P as Projection Engine
  participant RM as Read Model

  C->>API: POST create relation command
  API->>H: Parse & route
  H->>ES: Append RelationCreated(event)
  ES-->>P: New event
  P->>RM: Upsert relation (bidirectional flag)
  C->>API: GET relations
  API-->>C: Read model results
```

Overview: Create relation sequence.
- Ordering: Command → Event → Projection → Query.
- Request/Response: Synchronous write; eventually consistent read.
- NFRs: Idempotent commands; version checks; projection checkpoints.
- Trade-offs: Immediate read-after-write may lag.
- Risks: Concurrency conflicts → aggregate versioning.

## Additional Diagrams

### ERD (Read Models)
```mermaid
erDiagram
  ORGANIZATIONS {
    TEXT id PK
    TEXT name
    TEXT parent_id
  }
  APPLICATIONS {
    TEXT id PK
    TEXT name
  }
  APPLICATION_SERVICES {
    TEXT id PK
    TEXT name
  }
  APPLICATION_INTERFACES {
    TEXT id PK
    TEXT name
  }
  SERVERS {
    TEXT id PK
    TEXT name
  }
  INTEGRATIONS {
    TEXT id PK
    TEXT name
  }
  DATA_ENTITIES {
    TEXT id PK
    TEXT name
  }
  BUSINESS_CAPABILITIES {
    TEXT id PK
    TEXT name
  }
  RELATIONS {
    TEXT id PK
    TEXT source_id
    TEXT source_type
    TEXT target_id
    TEXT target_type
    TEXT relation_type
    INTEGER bidirectional
  }

  ORGANIZATIONS ||--o{ ORGANIZATIONS : parent
  ORGANIZATIONS ||--o{ APPLICATIONS : owns
  ORGANIZATIONS ||--o{ SERVERS : owns
  APPLICATIONS ||--o{ APPLICATION_SERVICES : realizes
  APPLICATIONS ||--o{ APPLICATION_INTERFACES : exposes
  APPLICATIONS ||--o{ RELATIONS : source
  APPLICATION_SERVICES ||--o{ BUSINESS_CAPABILITIES : realizes
  APPLICATION_INTERFACES ||--o{ APPLICATION_SERVICES : serves
  SERVERS ||--o{ RELATIONS : source
  INTEGRATIONS ||--o{ RELATIONS : source
  APPLICATIONS ||--o{ RELATIONS : target
  APPLICATION_SERVICES ||--o{ RELATIONS : target
  APPLICATION_INTERFACES ||--o{ RELATIONS : target
  SERVERS ||--o{ RELATIONS : target
  DATA_ENTITIES ||--o{ RELATIONS : target
  BUSINESS_CAPABILITIES ||--o{ RELATIONS : target
```

Overview: Key read model entities and relationships.
- Design Decisions: Organization hierarchy via `parent_id`; ownership via org→{app,server}; relations drive cross-entity links.
- NFRs: Indexes on `source_id`, `target_id`, `relation_type` for performance.
- Trade-offs: Minimal FKs for flexibility; rely on projections for integrity.
- Risks: Orphaned references → validation in handlers and projections.

## Phased Development

### Phase 1: Initial Implementation
- Single service; in-process projections; SQLite dev DB.
- Event store and projection_state tables present; snapshots optional.
- Strict relation matrix enforcement.

### Phase 2+: Final Architecture (Target)
- Managed RDBMS (MSSQL/PG) for event store and read models.
- Horizontal scaling; async projection workers; snapshotting strategy.
- Enhanced observability; RBAC; API gateway.

### Migration Path
- Introduce DB abstraction for MSSQL/PG.
- Externalize projection engine with durable queue (if needed).
- Add event upcasting/versioning policies.

## Non-Functional Requirements Analysis

### Scalability
- Scale read side via DB replicas; partition event streams.
- Stateless API instances; container orchestration.

### Performance
- Indexing: `aggregate_id, aggregate_version`, `event_type`, timestamps; on relations: `source_id`, `target_id`, `relation_type`.
- Connection pooling; prepared statements.

### Security
- Planned: OAuth2/OIDC; role-based authorization.
- DB users with least privilege; encrypted connections.

### Reliability
- Event store durability; backups and PITR.
- Health checks; projection checkpoints; retry policies.

### Maintainability
- Modular components; clear schemas; forward-only migrations.
- Documentation-driven schemas and event contracts.

## Risks and Mitigations
- Projection lag: checkpointing + monitoring + replay.
- Event schema evolution: versioning + upcasting.
- DB migration failure: pre-checks + roll-forward fixes.
- Concurrency conflicts: aggregate version checks + retries.

## Technology Stack Recommendations
- Runtime: .NET 8/10 (F#), Giraffe/ASP.NET Core.
- Persistence: SQLite (dev), MSSQL/PostgreSQL (prod).
- Serialization: Thoth.Json (events), JSON over HTTP.
- Observability: OpenTelemetry (future), structured logs.

## Next Steps
- Finalize `spec/spec-architecture-data.md` with detailed schemas.
- Add security and observability roadmap.
- Validate indexing coverage with production datasets.
