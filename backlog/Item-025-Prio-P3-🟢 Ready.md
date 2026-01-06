# Item-025: Create Architecture Decision Records (ADRs)

**Status:** 🟢 Ready  
**Priority:** P3 - LOW  
**Effort:** 4-6 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

Specifications explain "what" but not always "why". Architecture Decision Records (ADRs) document key decisions with context, alternatives, and consequences.

---

## Affected Files

**Create:** [spec/adr/](../../spec/adr/) (new directory)

**Create initial ADRs:**
- adr-001-sqlite-for-development.md
- adr-002-soft-delete-strategy.md
- adr-003-relation-entity-vs-fk.md
- adr-004-archimate-alignment.md
- adr-005-api-first-design.md

---

## Detailed Tasks

- [ ] Create /spec/adr/ directory
- [ ] Create ADR-001: SQLite for development
- [ ] Create ADR-002: Soft delete vs hard delete
- [ ] Create ADR-003: Relation entity vs FK
- [ ] Create ADR-004: ArchiMate alignment
- [ ] Create ADR-005: API-first design
- [ ] Use standard ADR format
- [ ] Link from spec-index.md

---

## Acceptance Criteria

- [ ] ADR directory created
- [ ] Initial ADRs written
- [ ] Standard format used
- [ ] Linked from index

---

## ADR Format

```markdown
# ADR-001: SQLite for Development

## Status
Accepted

## Context
Need to choose database for development/testing...

## Decision
Use SQLite for development, migrations support MSSQL/PostgreSQL for production...

## Rationale
- Simple file-based, no server setup needed
- Good for testing
- Can migrate to larger DB later

## Consequences
- Limited to single connection in production
- Data size limitations
- Upgrade path required to MSSQL/PostgreSQL
```

---

## Dependencies

**Depends On:** None

---

## Related Items

- [spec-index.md](../../spec/spec-index.md)

---

## Definition of Done

- [x] ADR directory created
- [x] Initial ADRs written
- [x] Linked from index

---

## Notes

- ADRs capture historical decisions
- Useful for onboarding
- Consider adding more as system evolves
