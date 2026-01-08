---
title: Data Architecture Specification
version: 0.1.0
date_created: 2026-01-08
last_updated: 2026-01-08
owner: EA Platform Team
tags: [architecture, data, database, event-sourcing]
---

# Introduction

This specification defines the EA Tool data architecture, covering database choices, read-model schemas, event store design, projection state tracking, indexing strategy, migration approach, deletion semantics, and performance considerations. It is intended for backend engineers, platform engineers, and DBAs responsible for operating and evolving the persistence layer.

## 1. Purpose & Scope

Provide a consistent, documented data architecture for all environments (dev, staging, prod), including schema definitions, migration strategy, and operational guidelines. Scope includes SQLite (current) and MSSQL/PostgreSQL (planned) deployments, covering both event-sourced and non-event-sourced domains.

## 2. Definitions

- **Read Model**: Query-optimized relational tables for API responses.
- **Event Store**: Append-only tables (`events`, `commands`, `snapshots`) capturing domain changes for event-sourced aggregates.
- **Projection State**: Tracker table for projection progress from the event store to read models.
- **Forward-Only Migration**: Migration that is applied once and never rolled back.
- **Soft Delete**: Logical removal using flags/timestamps rather than physical deletion.
- **SQLite**: Embedded relational database used for development/CI and small deployments.
- **MSSQL/PostgreSQL**: Target relational databases for staging/production to support concurrency and operational tooling.

## 3. Requirements, Constraints & Guidelines

- **REQ-001**: Database selection SHALL default to SQLite for development/CI; staging/production SHALL use MSSQL or PostgreSQL.
- **REQ-002**: All timestamps SHALL be stored in ISO 8601 UTC text form.
- **REQ-003**: Primary keys SHALL be UUID (TEXT) across tables.
- **REQ-004**: Migrations SHALL be forward-only, one logical change per file, and idempotent for replays.
- **REQ-005**: Event-sourced domains SHALL persist domain events in the event store and update read models via projections.
- **REQ-006**: Read models SHALL expose indexes for primary query patterns (lookup by id, owner/team, relation endpoints, names).
- **REQ-007**: Projection progress SHALL be tracked per projection to enable replay and monitoring.
- **SEC-001**: No sensitive data SHALL be stored unencrypted beyond necessity; classification fields must be honored by application-layer filtering.
- **CON-001**: SQLite write concurrency is limited; production workloads MUST target MSSQL/PostgreSQL.
- **CON-002**: JSON payloads in SQLite are stored as TEXT; downstream engines MAY use native JSON/JSONB for indexing.
- **GUD-001**: Prefer explicit indexes defined in migrations aligned with access patterns; avoid over-indexing.
- **GUD-002**: Keep foreign-key enforcement light in read models; domain validation and projections ensure integrity.

## 4. Interfaces & Data Contracts

### 4.1 Read Model Schemas

**applications** ([001_create_applications.sql](src/Infrastructure/Migrations/001_create_applications.sql))

| Column | Type | Constraints/Notes |
| --- | --- | --- |
| id | TEXT | PK (UUID) |
| name | TEXT | NOT NULL |
| owner | TEXT | NULL |
| lifecycle | TEXT | NOT NULL |
| lifecycle_raw | TEXT | NOT NULL |
| capability_id | TEXT | NULL |
| data_classification | TEXT | NULL |
| tags | TEXT | NOT NULL (JSON array) |
| created_at | TEXT | NOT NULL, ISO 8601 UTC |
| updated_at | TEXT | NOT NULL, ISO 8601 UTC |
Indexes: idx_applications_name, idx_applications_owner, idx_applications_lifecycle

**organizations** ([002_create_organizations.sql](src/Infrastructure/Migrations/002_create_organizations.sql), [008_add_parent_id_to_organizations.sql](src/Infrastructure/Migrations/008_add_parent_id_to_organizations.sql))

| Column | Type | Constraints/Notes |
| --- | --- | --- |
| id | TEXT | PK (UUID) |
| name | TEXT | NOT NULL |
| domains | TEXT | NOT NULL (JSON array) |
| contacts | TEXT | NOT NULL (JSON array) |
| parent_id | TEXT | NULL (hierarchy root nullable) |
| created_at | TEXT | NOT NULL, ISO 8601 UTC |
| updated_at | TEXT | NOT NULL, ISO 8601 UTC |
Indexes: idx_organizations_name, idx_organizations_parent, UNIQUE idx_organizations_parent_name (parent_id, name)

**business_capabilities** ([003_create_business_capabilities.sql](src/Infrastructure/Migrations/003_create_business_capabilities.sql), [008_add_business_capability_description.sql](src/Infrastructure/Migrations/008_add_business_capability_description.sql))

| Column | Type | Constraints/Notes |
| --- | --- | --- |
| id | TEXT | PK (UUID) |
| name | TEXT | NOT NULL |
| parent_id | TEXT | NULL |
| description | TEXT | NULL |
| created_at | TEXT | NOT NULL, ISO 8601 UTC |
| updated_at | TEXT | NOT NULL, ISO 8601 UTC |
Indexes: idx_business_capabilities_name, idx_business_capabilities_parent

**servers** ([004_create_servers.sql](src/Infrastructure/Migrations/004_create_servers.sql))

| Column | Type | Constraints/Notes |
| --- | --- | --- |
| id | TEXT | PK (UUID) |
| hostname | TEXT | NOT NULL |
| environment | TEXT | NULL |
| region | TEXT | NULL |
| platform | TEXT | NULL |
| criticality | TEXT | NULL |
| owning_team | TEXT | NULL |
| tags | TEXT | NOT NULL (JSON array) |
| created_at | TEXT | NOT NULL, ISO 8601 UTC |
| updated_at | TEXT | NOT NULL, ISO 8601 UTC |
Indexes: idx_servers_hostname, idx_servers_environment, idx_servers_region

**integrations** ([005_create_integrations.sql](src/Infrastructure/Migrations/005_create_integrations.sql))

| Column | Type | Constraints/Notes |
| --- | --- | --- |
| id | TEXT | PK (UUID) |
| source_app_id | TEXT | NOT NULL |
| target_app_id | TEXT | NOT NULL |
| protocol | TEXT | NULL |
| data_contract | TEXT | NULL |
| sla | TEXT | NULL |
| frequency | TEXT | NULL |
| tags | TEXT | NOT NULL (JSON array) |
| created_at | TEXT | NOT NULL, ISO 8601 UTC |
| updated_at | TEXT | NOT NULL, ISO 8601 UTC |
Indexes: idx_integrations_source (source_app_id), idx_integrations_target (target_app_id)

**data_entities** ([006_create_data_entities.sql](src/Infrastructure/Migrations/006_create_data_entities.sql))

| Column | Type | Constraints/Notes |
| --- | --- | --- |
| id | TEXT | PK (UUID) |
| name | TEXT | NOT NULL |
| domain | TEXT | NULL |
| classification | TEXT | NOT NULL |
| retention | TEXT | NULL |
| owner | TEXT | NULL |
| steward | TEXT | NULL |
| source_system | TEXT | NULL |
| criticality | TEXT | NULL |
| pii_flag | INTEGER | NOT NULL (0/1) |
| glossary_terms | TEXT | NOT NULL (JSON array) |
| lineage | TEXT | NOT NULL (JSON array) |
| created_at | TEXT | NOT NULL, ISO 8601 UTC |
| updated_at | TEXT | NOT NULL, ISO 8601 UTC |
Indexes: idx_data_entities_name, idx_data_entities_domain, idx_data_entities_classification

**relations** ([007_create_relations.sql](src/Infrastructure/Migrations/007_create_relations.sql))

| Column | Type | Constraints/Notes |
| --- | --- | --- |
| id | TEXT | PK (UUID) |
| source_id | TEXT | NOT NULL |
| target_id | TEXT | NOT NULL |
| source_type | TEXT | NOT NULL |
| target_type | TEXT | NOT NULL |
| relation_type | TEXT | NOT NULL |
| archimate_element | TEXT | NULL |
| archimate_relationship | TEXT | NULL |
| description | TEXT | NULL |
| data_classification | TEXT | NULL |
| criticality | TEXT | NULL |
| confidence | REAL | NULL |
| evidence_source | TEXT | NULL |
| last_verified_at | TEXT | NULL |
| effective_from | TEXT | NULL |
| effective_to | TEXT | NULL |
| label | TEXT | NULL |
| color | TEXT | NULL |
| style | TEXT | NULL |
| bidirectional | INTEGER | NOT NULL (0/1) |
| created_at | TEXT | NOT NULL, ISO 8601 UTC |
| updated_at | TEXT | NOT NULL, ISO 8601 UTC |
Indexes: idx_relations_source (source_id), idx_relations_target (target_id), idx_relations_type (relation_type)

### 4.2 Event Store Schemas ([008_create_event_store.sql](src/Infrastructure/Migrations/008_create_event_store.sql))

**events**

| Column | Type | Constraints/Notes |
| --- | --- | --- |
| event_id | TEXT | PK (UUID) |
| aggregate_id | TEXT | NOT NULL |
| aggregate_type | TEXT | NOT NULL |
| aggregate_version | INTEGER | NOT NULL; UNIQUE with aggregate_id |
| event_type | TEXT | NOT NULL |
| event_version | INTEGER | NOT NULL DEFAULT 1 |
| event_timestamp | TEXT | NOT NULL, ISO 8601 UTC |
| actor | TEXT | NOT NULL |
| actor_type | TEXT | NOT NULL |
| source | TEXT | NOT NULL |
| causation_id | TEXT | NULL |
| correlation_id | TEXT | NULL |
| data | TEXT | NOT NULL (JSON payload) |
| metadata | TEXT | NULL (JSON) |
Indexes: UNIQUE (aggregate_id, aggregate_version); ix_events_aggregate_id; ix_events_event_type; ix_events_event_timestamp; ix_events_correlation_id

**commands**

| Column | Type | Constraints/Notes |
| --- | --- | --- |
| command_id | TEXT | PK (UUID) |
| command_type | TEXT | NOT NULL |
| aggregate_id | TEXT | NOT NULL |
| aggregate_type | TEXT | NOT NULL |
| processed_at | TEXT | NULL |
| actor | TEXT | NOT NULL |
| source | TEXT | NOT NULL |
| data | TEXT | NOT NULL (JSON payload) |
Indexes: UNIQUE (command_id)

**snapshots**

| Column | Type | Constraints/Notes |
| --- | --- | --- |
| snapshot_id | TEXT | PK (UUID) |
| aggregate_id | TEXT | NOT NULL |
| aggregate_type | TEXT | NOT NULL |
| aggregate_version | INTEGER | NOT NULL |
| snapshot_version | INTEGER | NOT NULL DEFAULT 1 |
| snapshot_timestamp | TEXT | NOT NULL, ISO 8601 UTC |
| state | TEXT | NOT NULL (JSON aggregate state) |
Indexes: UNIQUE (aggregate_id, aggregate_version); ix_snapshots_aggregate_id

### 4.3 Projection State Schema ([009_create_projection_state.sql](src/Infrastructure/Migrations/009_create_projection_state.sql))

| Column | Type | Constraints/Notes |
| --- | --- | --- |
| projection_name | TEXT | PK |
| last_processed_event_id | TEXT | NULL |
| last_processed_at | TEXT | NULL |
| last_processed_version | INTEGER | NOT NULL DEFAULT 0 |
| status | TEXT | NOT NULL DEFAULT 'active' |
Indexes: ix_projection_state_status

### 4.4 Relationships
- Hierarchies: `organizations.parent_id` and `business_capabilities.parent_id` enable tree traversal; uniqueness enforced per parent for organizations.
- Integration endpoints: `integrations.source_app_id` and `integrations.target_app_id` link applications (no FK, enforced via domain validation).
- Relations: `relations.source_id/target_id` reference various entity types; integrity enforced by domain logic and projection rules.
- Event store to read models: projections consume `events` and update read-model tables; progress tracked in `projection_state`.

## 5. Acceptance Criteria

- **AC-001**: Data architecture spec created with canonical sections and front matter.
- **AC-002**: Database choices (SQLite for dev/CI, MSSQL/PostgreSQL for staging/prod) documented with rationale and constraints.
- **AC-003**: All read-model table schemas and indexes documented (applications, organizations, business_capabilities, servers, integrations, data_entities, relations).
- **AC-004**: Event store tables (`events`, `commands`, `snapshots`) and projection_state table documented with indexes and semantics.
- **AC-005**: Relationships and hierarchy behaviors documented (parent-child, integration endpoints, relations graph, projection linkage).
- **AC-006**: Soft delete and deletion semantics documented (event-sourced deletion via events; no global deleted_at in current read models).
- **AC-007**: Migration pattern documented (file naming, forward-only, startup application via DbUp embedded scripts).
- **AC-008**: Performance and indexing considerations documented for primary query paths.
- **AC-009**: Spec linked from the specifications index.

## 6. Test Automation Strategy

- **Test Levels**: Migration smoke tests (DbUp dry run) and integration tests that apply migrations against SQLite; future staging tests against MSSQL/PostgreSQL.
- **Frameworks**: DbUp for migration execution; integration tests via existing test harness to ensure projections and repositories operate on migrated schema.
- **Test Data Management**: Seed fixtures per test using minimal inserts; clean database per test run (ephemeral SQLite files in CI).
- **CI/CD Integration**: Run migrations at startup in CI; fail build on migration errors; optional schema diff checks for drift.
- **Coverage Requirements**: Each migration exercised at least once in automated tests; repositories and projections cover their key query paths.
- **Performance Testing**: Track query latency for hot paths (relations by source/target, applications by owner) and ensure indexes are used via EXPLAIN in staging.

## 7. Rationale & Context

- SQLite offers fast, dependency-light development while embedded migrations keep the schema close to code.
- Forward-only, idempotent migrations reduce operational risk and keep deployment pipelines simple.
- Event store plus projections support event-sourced domains while read models stay query-friendly.
- JSON stored as TEXT keeps compatibility across SQLite and future engines; native JSON/JSONB can be adopted in MSSQL/PG without contract changes.

## 8. Dependencies & External Integrations

### External Systems
- **EXT-001**: None currently; database runs locally or in managed service per environment.

### Third-Party Services
- **SVC-001**: DbUp library for migration execution.

### Infrastructure Dependencies
- **INF-001**: Storage for SQLite files in dev/CI; managed MSSQL/PostgreSQL instances for staging/prod.
- **INF-002**: Application startup permissions to run migrations (DDL) in target environment.

### Data Dependencies
- **DAT-001**: Migration scripts embedded in assembly ([src/Infrastructure/Migrations.fs](src/Infrastructure/Migrations.fs)); scripts under [src/Infrastructure/Migrations/](src/Infrastructure/Migrations/).

### Technology Platform Dependencies
- **PLT-001**: .NET 10 runtime compatible with DbUp and chosen DB providers.

### Compliance Dependencies
- **COM-001**: Audit logging of data mutations handled at application layer; schema stores timestamps and actor context via event store payloads.

## 9. Examples & Edge Cases

```sql
-- Example: fetch application relations by source with index use
SELECT id, target_id, relation_type
FROM relations
WHERE source_id = 'app-123'
  AND relation_type = 'Serving';

-- Example: retrieve projection checkpoint
SELECT last_processed_event_id, last_processed_version
FROM projection_state
WHERE projection_name = 'relations_projection';
```

Edge Cases:
- Event store replays require projection_state reset; ensure idempotent projections or compensating logic.
- Missing foreign keys can allow orphaned rows; projections and domain validation must guard against this.
- SQLite lacks native JSON indexing; consider virtual columns or migrate to MSSQL/PG for JSON indexing when needed.
- Concurrency in SQLite is limited; long-running writes can block readers—mitigate via short transactions and move heavy workloads to MSSQL/PG.

## 10. Validation Criteria

- Migrations execute successfully via DbUp against a clean SQLite database.
- Read-model tables contain expected columns and indexes after migration run.
- Event store tables enforce uniqueness on `aggregate_id` + `aggregate_version` and `command_id`.
- Projection state table is populated and updated during projection runs in tests.
- Data architecture spec is reachable from the spec index and conforms to the specification template.

## 11. Related Specifications / Further Reading

- [spec-index.md](spec/spec-index.md)
- [spec-architecture-event-sourcing.md](spec/spec-architecture-event-sourcing.md)
- [spec-implementation-status.md](spec/spec-implementation-status.md)
- [docs/system-overview.md](docs/system-overview.md)
- [openapi.yaml](openapi.yaml)
*** End Patch# Data Architecture Specification (EATool)

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
