# Item-008: Create Data Architecture Specification

**Status:** 🟢 Ready  
**Priority:** P1 - HIGH  
**Effort:** 4-6 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

Database schema and data architecture are implemented but lack formal documentation explaining:
- Database choice rationale (SQLite vs MSSQL)
- Table schemas and relationships
- Indexing strategy
- Soft delete approach
- Migration pattern

**Impact:** Database design decisions are implicit; developers cannot understand or modify schema effectively.

---

## Affected Files

**Create:** [spec/spec-architecture-data.md](../../spec/spec-architecture-data.md) (new file)

**Reference:**
- [src/Infrastructure/Migrations/](../../src/Infrastructure/Migrations/) - SQL migration files
- [src/Infrastructure/Database.fs](../../src/Infrastructure/Database.fs)
- [src/Infrastructure/Migrations.fs](../../src/Infrastructure/Migrations.fs)

---

## Detailed Tasks

- [ ] Create spec-architecture-data.md following specification template
- [ ] Document database choice:
  - SQLite for development/small deployments
  - MSSQL/PostgreSQL for production (future)
  - Rationale and trade-offs

- [ ] Document table schemas:
  - For each entity, show: columns, types, constraints, indexes
  - Foreign key relationships
  - Unique constraints

- [ ] Document indexing strategy:
  - Primary keys on id (UUID)
  - Index on common query fields (name, parent_id, owner)
  - Compound indexes for scoped uniqueness (parent_id, name)
  - Why each index exists

- [ ] Document relationships:
  - One-to-many (e.g., Organization parent-child)
  - One-to-many via Relation (e.g., Application to Server)
  - Referential integrity enforcement
  - Cascade behavior (delete, update)

- [ ] Document soft delete:
  - deleted_at timestamp approach
  - How deleted records are filtered
  - Include parameter for queries

- [ ] Document migration strategy:
  - File naming: {number}_{description}.sql
  - One migration per logical change
  - Forward-only migrations (no rollback)
  - Running migrations at startup

- [ ] Document data types:
  - String (TEXT with NOT NULL/nullable)
  - JSON arrays (serialize/deserialize)
  - Timestamps (ISO 8601 UTC)
  - Boolean (integer 0/1 in SQLite)

- [ ] Document performance considerations:
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
