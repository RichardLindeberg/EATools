# Item-008: Create Data Architecture Specification

**Status:** 🟢 Ready  
**Priority:** P1 - HIGH  
**Effort:** 4-6 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

Database schema and data architecture are implemented but lack formal documentation explaining:
- Database choice rationale (SQLite vs MSSQL)
- Table schemas and relationships (read models vs event store)
- Indexing strategy
- Event-sourced deletion semantics (handled via events and projections)
- Migration pattern (including event store and projection state)
- Indexing strategy
- Migration pattern

**Impact:** Database design decisions are implicit; developers cannot understand or modify schema effectively.

## Affected Files

**Create:** [spec/spec-architecture-data.md](../../spec/spec-architecture-data.md) (new file)
- [ ] Document table schemas:
  - Read model tables (applications, organizations, business_capabilities, servers, integrations, data_entities, relations): columns, types, constraints, indexes
  - Event store tables (events, commands, snapshots): columns, constraints, indexes
  - Projection state table: columns and purpose
  - Foreign key and referential integrity notes (where enforced or managed via projections)
  - Unique constraints
- [src/Infrastructure/Migrations.fs](../../src/Infrastructure/Migrations.fs)
- [ ] Document deletion semantics in event-sourced domains:
  - Deletion handled via domain events (e.g., RelationDeleted, BusinessCapabilityDeleted)
  - Read model projection behavior (e.g., removing or marking records)
  - No global `deleted_at` pattern in current read models

- [ ] Document migration strategy:
  - File naming: {number}_{description}.sql
  - One migration per logical change
  - Forward-only migrations (no rollback)
  - Running migrations at startup
  - Event store and projection state migrations (008_create_event_store.sql, 009_create_projection_state.sql)

- [ ] Document table schemas:
  - For each entity, show: columns, types, constraints, indexes
  - Foreign key relationships
| id | TEXT PRIMARY KEY | UUID |
| name | TEXT NOT NULL | max 255 |
| parent_id | TEXT NULL | Hierarchy (nullable root) |
| domains | TEXT NOT NULL | JSON array |
| contacts | TEXT NOT NULL | JSON array |
| created_at | TEXT NOT NULL | ISO 8601 UTC |
| updated_at | TEXT NOT NULL | ISO 8601 UTC |
- [ ] Document relationships:
  - One-to-many (e.g., Organization parent-child)
- idx_organizations_name ON name
- idx_organizations_parent ON parent_id
- UNIQUE (parent_id, name)

### event store
Tables supporting event sourcing for select domains (e.g., Relations, BusinessCapabilities):

#### events
| Column | Type | Constraints |
|--------|------|-------------|
| event_id | TEXT PRIMARY KEY | UUID |
| aggregate_id | TEXT NOT NULL | |
| aggregate_type | TEXT NOT NULL | |
| aggregate_version | INTEGER NOT NULL | unique per aggregate_id |
| event_type | TEXT NOT NULL | |
| event_version | INTEGER NOT NULL | default 1 |
| event_timestamp | TEXT NOT NULL | ISO 8601 UTC |
| actor | TEXT NOT NULL | |
| actor_type | TEXT NOT NULL | |
| source | TEXT NOT NULL | |
| causation_id | TEXT NULL | |
| correlation_id | TEXT NULL | |
| data | TEXT NOT NULL | JSON (event payload) |
| metadata | TEXT NULL | JSON |

Indexes:
- UNIQUE (aggregate_id, aggregate_version)
- ix_events_aggregate_id, ix_events_event_type, ix_events_event_timestamp, ix_events_correlation_id

#### commands
| Column | Type | Constraints |
|--------|------|-------------|
| command_id | TEXT PRIMARY KEY | UUID |
| command_type | TEXT NOT NULL | |
| aggregate_id | TEXT NOT NULL | |
| aggregate_type | TEXT NOT NULL | |
| processed_at | TEXT NULL | |
| actor | TEXT NOT NULL | |
| source | TEXT NOT NULL | |
| data | TEXT NOT NULL | JSON |

Index:
- UNIQUE (command_id)

#### snapshots (optional)
| Column | Type | Constraints |
|--------|------|-------------|
| snapshot_id | TEXT PRIMARY KEY | UUID |
| aggregate_id | TEXT NOT NULL | |
| aggregate_type | TEXT NOT NULL | |
| aggregate_version | INTEGER NOT NULL | |
| snapshot_version | INTEGER NOT NULL | default 1 |
| snapshot_timestamp | TEXT NOT NULL | ISO 8601 UTC |
| state | TEXT NOT NULL | JSON (aggregate state) |

Indexes:
- UNIQUE (aggregate_id, aggregate_version)
- ix_snapshots_aggregate_id

### projection_state
Tracks progress of projections updating read models from events.

| Column | Type | Constraints |
|--------|------|-------------|
| projection_name | TEXT PRIMARY KEY | |
| last_processed_event_id | TEXT NULL | |
| last_processed_at | TEXT NULL | |
| last_processed_version | INTEGER NOT NULL | default 0 |
| status | TEXT NOT NULL | default 'active' |
- [ ] Document soft delete:
  - deleted_at timestamp approach
  - How deleted records are filtered
  - Include parameter for queries

- [ ] Document migration strategy:
  - File naming: {number}_{description}.sql
  - One migration per logical change
  - Forward-only migrations (no rollback)
  - Common queries and their performance
  - Recommended indexes
  - Query optimization patterns

---

## Acceptance Criteria

- [ ] spec-architecture-data.md created
- [ ] Database choice documented with rationale
- [ ] All table schemas documented
- [ ] Relationships documented
- [ ] Indexing strategy documented
- [ ] Soft delete approach documented
- [ ] Migration pattern documented
- [ ] Performance considerations documented
- [ ] Linked from spec-index.md

---

## Key Sections

```markdown
# Data Architecture Specification

## 1. Purpose & Scope

## 2. Database Choice

### Current: SQLite
- Development and small deployments
- File-based, easy to backup
- Limitations at scale

### Future: MSSQL / PostgreSQL
- Production environments
- Scaling considerations
- JSON/JSONB support

## 3. Table Schemas

### organizations
| Column | Type | Constraints |
|--------|------|-------------|
| id | TEXT PRIMARY KEY | UUID |
| name | TEXT NOT NULL | max 255 |
| parent_id | TEXT NULL | FK |
| domains | TEXT NOT NULL | JSON array |
| contacts | TEXT NOT NULL | JSON array |
| created_at | TEXT NOT NULL | ISO 8601 UTC |
| updated_at | TEXT NOT NULL | ISO 8601 UTC |
| deleted_at | TEXT NULL | Soft delete |

Indexes:
- idx_organizations_name ON name
- idx_organizations_parent ON parent_id
- UNIQUE (parent_id, name)

## 4. Relationships

## 5. Soft Delete Strategy

## 6. Migration Pattern

## 7. Performance Optimization

## 8. Related Specifications
```

---

## Dependencies

**Blocks:** None

**Depends On:**
- Item-017: Data Migration Strategy spec (related)

---

## Related Items

- [Item-017-Prio-P2.md](Item-017-Prio-P2.md) - Migration strategy
- [src/Infrastructure/Migrations/](../../src/Infrastructure/Migrations/)
- [spec-index.md](../../spec/spec-index.md)

---

## Definition of Done

- [x] spec-architecture-data.md created
- [x] All tables documented
- [x] Relationships documented
- [x] Indexing strategy documented
- [x] Migration pattern documented
- [x] Performance considerations documented
- [x] Linked from spec-index.md

---

## Notes

- Critical for database design and optimization
- Serves as reference for developers adding new entities
- Should document both current SQLite and future RDBMS
- Consider including ER diagram (ASCII or Mermaid)
