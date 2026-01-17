# Item-013: Standardize Query Parameter Patterns Across Endpoints

**Status:** 🟢 Ready  
**Priority:** P2 - MEDIUM  
**Effort:** 4-6 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

API endpoints have inconsistent query parameter naming and patterns:
- Some endpoints use `search`, others have entity-specific filters
- Pagination is consistent but some endpoints lack filters
- No standard sort parameter
- Entity-specific filters are undocumented or missing

**Impact:** Poor developer experience; harder to learn and use API.

---

## Affected Files

- [openapi.yaml](../../openapi.yaml) - All endpoint definitions
- All endpoint files in [src/Api/](../../src/Api/)
- All repository files in [src/Infrastructure/](../../src/Infrastructure/)

---

## Detailed Tasks

- [x] Audit all endpoints and document current query parameters
- [x] Design consistent parameter naming convention (see Item-011)
- [x] Update openapi.yaml:
  - [x] Standardize pagination (`page`, `limit`)
  - [x] Add search where missing
  - [x] Add entity-specific filters (owner, lifecycle, etc.)
  - [x] Add sort parameter (optional, nice-to-have)
- [x] Implement in code:
  - [x] Update endpoint handlers to extract standard parameters
  - [x] Update repository layer to apply filters
  - [x] Implement sorting logic (optional)
- [x] Add tests:
  - [x] Test each filter parameter
  - [x] Test pagination with filters
  - [x] Test search with filters
- [x] Update documentation

---

## Proposed Standard Parameters

### All List Endpoints
- `page` (pagination)
- `limit` (pagination)
- `search` (full-text search)
- `sort` (optional, nice-to-have)

### Entity-Specific Filters
- Applications: `owner`, `lifecycle`, `capability_id`
- BusinessCapabilities: `parent_id`
- Organizations: `parent_id`, `search`
- Servers: `environment`, `region`, `criticality`
- DataEntities: `domain`, `classification`, `owner`
- etc.

---

## Acceptance Criteria

- [x] All endpoints use consistent parameter patterns
- [x] OpenAPI spec documents all query parameters
- [x] Pagination consistent across all list endpoints
- [x] Search available on name/text fields
- [x] Entity-specific filters documented
- [x] All tests pass
- [x] Developer documentation updated

---

## Related To

- [Item-011-Prio-P1.md](Item-011-Prio-P1.md) - Query Patterns spec
- [Item-005-Prio-P1.md](Item-005-Prio-P1.md) - parent_id documentation
- [openapi.yaml](../../openapi.yaml)

---

## Definition of Done

- [x] All endpoints use consistent patterns
- [x] OpenAPI spec updated
- [x] Code implements patterns
- [x] All tests passing
- [x] Documentation updated
- [x] Code reviewed

---

## Notes

- Implementation of Item-011 (Query Patterns spec)
- Breaking change potential (be careful with versioning)
- Consider phased rollout by endpoint
