# Item-011: Create Query Patterns Specification

**Status:** 🟢 Ready  
**Priority:** P1 - HIGH  
**Effort:** 2-3 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

API has inconsistent query parameters across endpoints (search, owner, lifecycle, parent_id, etc.) but lacks formal specification defining:
- Pagination patterns
- Filtering patterns
- Sorting patterns
- Search patterns
- Response structures

**Impact:** API consumers struggle with inconsistent query parameter naming; developers implement inconsistently.

---

## Affected Files

**Create:** [spec/spec-tool-query-patterns.md](../../spec/spec-tool-query-patterns.md) (new file)

**Reference:**
- [openapi.yaml](../../openapi.yaml) - All endpoint definitions
- All endpoint files in [src/Api/](../../src/Api/)

---

## Detailed Tasks

- [ ] Create spec-tool-query-patterns.md following specification template
- [ ] Document pagination pattern:
  - `page` (1-based, default 1)
  - `limit` (default 50, max 200)
  - Response structure: items, page, limit, total
  - has_more flag (optional)

- [ ] Document filtering pattern:
  - Field-specific filters: `?field=value`
  - Examples: `owner=team-id`, `environment=prod`
  - Support for multiple values: `?tag=payments&tag=pci`
  - Null filters: `?parent_id=null` for roots

- [ ] Document sorting pattern:
  - `sort=field` (ascending)
  - `sort=+field` (ascending, explicit)
  - `sort=-field` (descending)
  - Multiple sorts: `sort=+priority,-created_at`
  - Default sort order (created_at DESC)

- [ ] Document search pattern:
  - `search=term` (full-text across name, description, etc.)
  - Case-insensitive
  - Partial matching (contains)
  - Examples: `search=payment`, `search=prod`

- [ ] Document soft delete handling:
  - `include_deleted=true` to see deleted records
  - Default: exclude deleted records
  - Examples

- [ ] Document response structure:
  - Single resource (GET /resource/{id})
  - Collection (GET /resource?page=1&limit=50)
  - Pagination metadata (page, limit, total)
  - Item structure

- [ ] Document examples:
  - Paginate: `GET /applications?page=2&limit=10`
  - Filter: `GET /applications?owner=team-id&lifecycle=active`
  - Search: `GET /applications?search=payment`
  - Sort: `GET /applications?sort=-created_at`
  - Combined: `GET /applications?page=1&limit=10&lifecycle=active&sort=-created_at`

- [ ] Document consistency rules:
  - All endpoints support pagination
  - All list endpoints support search
  - Entity-specific filters (owner, lifecycle, etc.)
  - Standard parameter names

---

## Acceptance Criteria

- [ ] spec-tool-query-patterns.md created
- [ ] Pagination pattern documented
- [ ] Filtering pattern documented
- [ ] Sorting pattern documented
- [ ] Search pattern documented
- [ ] Response structures documented
- [ ] Examples for each pattern
- [ ] Consistency rules documented
- [ ] Linked from spec-index.md

---

## Key Sections

```markdown
# Query Patterns Specification

## 1. Purpose & Scope

## 2. Standard Parameters

### Pagination
- `page` (int, default 1, min 1)
- `limit` (int, default 50, min 1, max 200)

### Search & Filter
- `search` (string, optional)
- `sort` (string, optional)
- Entity-specific filters (owner, lifecycle, etc.)

### Soft Delete
- `include_deleted` (bool, default false)

## 3. Pagination

Request: `GET /applications?page=2&limit=10`

Response:
```json
{
  "items": [...],
  "page": 2,
  "limit": 10,
  "total": 150,
  "has_more": true
}
```

## 4. Filtering

Field-specific: `?owner=team-id&lifecycle=active`
Multiple values: `?tag=payments&tag=pci`
Null values: `?parent_id=null` (for roots)

## 5. Sorting

Ascending: `?sort=name`
Descending: `?sort=-created_at`
Multiple: `?sort=+priority,-created_at`

## 6. Search

Full-text: `?search=payment`
Case-insensitive partial matching

## 7. Examples

## 8. Consistency Rules

## 9. Related Specifications
```

---

## Dependencies

**Blocks:**
- Item-013: Standardize Query Patterns (larger implementation)

**Depends On:** None

---

## Related Items

- [Item-013-Prio-P2.md](Item-013-Prio-P2.md) - Implementation of patterns
- [Item-005-Prio-P1.md](Item-005-Prio-P1.md) - parent_id parameter
- [spec-index.md](../../spec/spec-index.md)

---

## Definition of Done

- [x] spec-tool-query-patterns.md created
- [x] All query patterns documented
- [x] Response structures documented
- [x] Examples for each pattern
- [x] Consistency rules documented
- [x] Linked from spec-index.md

---

## Notes

- This specification provides the foundation for Item-013 (implementation)
- Critical for API usability
- Should be finalized before major API changes
- Examples should be copy-paste ready
- Consider adding to API documentation/Swagger
