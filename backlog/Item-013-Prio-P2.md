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

- [ ] Audit all endpoints and document current query parameters
- [ ] Design consistent parameter naming convention (see Item-011)
- [ ] Update openapi.yaml:
  - Standardize pagination (`page`, `limit`)
  - Add search where missing
  - Add entity-specific filters (owner, lifecycle, etc.)
  - Add sort parameter (optional, nice-to-have)
- [ ] Implement in code:
  - Update endpoint handlers to extract standard parameters
  - Update repository layer to apply filters
  - Implement sorting logic (optional)
- [ ] Add tests:
  - Test each filter parameter
  - Test pagination with filters
  - Test search with filters
- [ ] Update documentation

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

- [ ] All endpoints use consistent parameter patterns
- [ ] OpenAPI spec documents all query parameters
- [ ] Pagination consistent across all list endpoints
- [ ] Search available on name/text fields
- [ ] Entity-specific filters documented
- [ ] All tests pass
- [ ] Developer documentation updated

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
