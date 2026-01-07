# Data Architecture Specification (EATool)

This specification details the data architecture for EATool, focusing on:
- Event store schema and behavior
- Read model schemas
- Indexing strategy and performance considerations
- Migration approach
- Deletion semantics in event-sourced domains

## 1. Database Choice and Rationale
- Development: SQLite (simplicity, portability for local dev and CI).
- Production: MSSQL or PostgreSQL (concurrency, scaling, operational tooling).
- Trade-offs: SQLite limits concurrent writes; MSSQL/PG require ops complexity.

## 2. Event Store

### 2.1 Tables
- events:
  - event_id (TEXT, PK)
  - aggregate_id (TEXT, NOT NULL)
  - aggregate_type (TEXT, NOT NULL)
  - aggregate_version (INTEGER, NOT NULL, UNIQUE per aggregate_id)
  - event_type (TEXT, NOT NULL)
  - event_version (INTEGER, NOT NULL, default 1)
  - event_timestamp (TEXT, NOT NULL, ISO 8601 UTC)
  - actor (TEXT, NOT NULL)
  - actor_type (TEXT, NOT NULL)
  - source (TEXT, NOT NULL)
  - causation_id (TEXT, NULL)
  - correlation_id (TEXT, NULL)
  - data (TEXT, NOT NULL, JSON payload)
  - metadata (TEXT, NULL, JSON)

- commands:
  - command_id (TEXT, PK, UNIQUE)
  - command_type (TEXT, NOT NULL)
  - aggregate_id (TEXT, NOT NULL)
  - aggregate_type (TEXT, NOT NULL)
  - processed_at (TEXT, NULL)
  - actor (TEXT, NOT NULL)
  - source (TEXT, NOT NULL)
  - data (TEXT, NOT NULL, JSON payload)

- snapshots (optional):
  - snapshot_id (TEXT, PK)
  - aggregate_id (TEXT, NOT NULL)
  - aggregate_type (TEXT, NOT NULL)
  - aggregate_version (INTEGER, NOT NULL)
  - snapshot_version (INTEGER, NOT NULL, default 1)
  - snapshot_timestamp (TEXT, NOT NULL, ISO 8601 UTC)
  - state (TEXT, NOT NULL, JSON aggregate state)

### 2.2 Indexing
- events:
  - UNIQUE (aggregate_id, aggregate_version)
  - ix_events_aggregate_id
  - ix_events_event_type
  - ix_events_event_timestamp
  - ix_events_correlation_id
- commands: UNIQUE (command_id)
- snapshots:
  - UNIQUE (aggregate_id, aggregate_version)
  - ix_snapshots_aggregate_id

### 2.3 Behavior
- Append-only writes; strong ordering per aggregate via version.
- Idempotent command processing via `commands` uniqueness.
- Event versioning for evolution; upcasting policy required for breaking changes.

## 3. Projection State
- projection_state:
  - projection_name (TEXT, PK)
  - last_processed_event_id (TEXT, NULL)
  - last_processed_at (TEXT, NULL)
  - last_processed_version (INTEGER, NOT NULL, default 0)
  - status (TEXT, NOT NULL, default 'active')

Purpose: Track projection progress; support replay and monitoring.

## 4. Read Models
Key tables:
- organizations:
  - id (TEXT, PK)
  - name (TEXT, NOT NULL)
  - parent_id (TEXT, NULL)
  - domains (TEXT, NOT NULL, JSON array)
  - contacts (TEXT, NOT NULL, JSON array)
  - created_at (TEXT, NOT NULL)
  - updated_at (TEXT, NOT NULL)
  - Indexes: name, parent_id, UNIQUE (parent_id, name)

- applications, servers, integrations, data_entities, business_capabilities:
  - id (TEXT, PK)
  - name (TEXT, NOT NULL)
  - created_at/updated_at (TEXT)
  - Additional columns per domain as implemented

- relations:
  - id (TEXT, PK)
  - source_id (TEXT, NOT NULL)
  - source_type (TEXT, NOT NULL)
  - target_id (TEXT, NOT NULL)
  - target_type (TEXT, NOT NULL)
  - relation_type (TEXT, NOT NULL)
  - bidirectional (INTEGER NOT NULL, 0/1)
  - confidence (INTEGER NOT NULL)
  - effective_from (TEXT NULL)
  - effective_to (TEXT NULL)
  - created_at/updated_at (TEXT)
  - Indexes: source_id, target_id, relation_type (and composite where beneficial)

Referential Integrity: Enforced via domain validation and projections; limited FK constraints in read models for flexibility and performance.

## 5. Deletion Semantics (Event-Sourced)
- Event-sourced domains (e.g., Relations, BusinessCapabilities) represent deletion via events (e.g., `RelationDeleted`).
- Projections update read models accordingly (delete rows or mark as inactive depending on domain rules).
- No global `deleted_at` required in these read models; non-event-sourced domains may use explicit deletion fields if needed.

## 6. Migration Pattern
- File naming: `{number}_{description}.sql` (e.g., `008_create_event_store.sql`, `009_create_projection_state.sql`).
- One migration per logical change; forward-only.
- Applied at service startup; ensure idempotency.
- Include indexes within migrations for performance.

## 7. Performance Considerations
- Event Store: Optimize retrieval with `aggregate_id` and time-range indexes.
- Relations: Composite indexes for common queries (`source_id, relation_type`, `target_id, relation_type`).
- Use prepared statements and connection pooling.
- Periodic vacuum/maintenance (SQLite), index maintenance (MSSQL/PG).

## 8. Cross-References
- Architecture overview and diagrams: see `../EATool_Architecture.md`.
- OpenAPI contract: `../openapi.yaml`.

## 9. Acceptance Checklist
- Event store schema documented
- Read model schemas documented
- Indexing strategy documented
- Migration approach documented
- Deletion semantics (event-sourced) documented
