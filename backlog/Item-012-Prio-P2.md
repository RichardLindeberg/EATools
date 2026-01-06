# Item-012: Add `parent_id` Filter to Organizations Endpoint

**Status:** 🔴 Blocked  
**Priority:** P2 - MEDIUM  
**Effort:** 2 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

Organizations support hierarchies but lack query parameter to filter by parent_id, making it difficult for users to retrieve organizational structure and navigate hierarchies.

**Impact:** Users must fetch all organizations and filter client-side; poor UX for large organizations.

---

## Affected Files

- [openapi.yaml](../../openapi.yaml#L59-L80) - GET /organizations endpoint
- [src/Api/OrganizationsEndpoints.fs](../../src/Api/OrganizationsEndpoints.fs#L16-L19)
- [src/Infrastructure/OrganizationRepository.fs](../../src/Infrastructure/OrganizationRepository.fs#L38-L49)

---

## Detailed Tasks

- [ ] Add `parent_id` query parameter to GET /organizations in OpenAPI
- [ ] Add parameter description: "Filter by parent organization (null for roots)"
- [ ] Update OrganizationsEndpoints to extract parent_id parameter
- [ ] Update OrganizationRepository.buildFilters to support parent_id WHERE clause
- [ ] Support `parent_id=null` to query root organizations
- [ ] Add integration test for filtering by parent_id
- [ ] Update API documentation

---

## Acceptance Criteria

- [ ] Can filter organizations by parent_id parameter
- [ ] Can query for root organizations (parent_id=null)
- [ ] OpenAPI spec documents the parameter
- [ ] Integration tests verify filtering works correctly
- [ ] Response includes parent_id in each organization

---

## OpenAPI Changes

```yaml
/organizations:
  get:
    tags: [Organizations]
    parameters:
      - $ref: '#/components/parameters/page'
      - $ref: '#/components/parameters/limit'
      - $ref: '#/components/parameters/search'
      - in: query
        name: parent_id
        schema:
          type: string
          nullable: true
        description: Filter by parent organization ID (null for roots)
        example: org-parent-uuid
```

---

## Code Changes

**OrganizationsEndpoints.fs:**
```fsharp
let parentId = ctx.TryGetQueryStringValue "parent_id" 
  |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))

let result = OrganizationRepository.getAll pageParam limitParam search parentId
```

**OrganizationRepository.fs:**
```fsharp
let buildFilters (search: string option) (parentId: string option) =
  let clauses = List<string>()
  let parameters = List<SqliteParameter>()
  
  // ... existing search clause ...
  
  match parentId with
  | Some "null" -> clauses.Add("parent_id IS NULL")
  | Some id -> 
    clauses.Add("parent_id = $parent_id")
    parameters.Add(SqliteParameter("$parent_id", id))
  | None -> ()
```

---

## Dependencies

**Blocks:** None

**Depends On:**
- Item-001: Organization parent_id implementation (must be done first)

---

## Related Items

- [Item-001-Prio-P0.md](Item-001-Prio-P0.md) - Must complete first
- [Item-011-Prio-P1.md](Item-011-Prio-P1.md) - Query patterns spec

---

## Definition of Done

- [x] OpenAPI spec includes parent_id parameter
- [x] Code extracts and uses parameter
- [x] Integration test verifies filtering
- [x] Null handling for root organizations
- [x] Documentation updated

---

## Notes

- Quick follow-up to Item-001 (parent_id implementation)
- Similar pattern to Item-005 (BusinessCapability parent_id)
- Enables hierarchical navigation
