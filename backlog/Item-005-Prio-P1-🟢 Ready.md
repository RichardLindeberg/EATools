# Item-005: Fix BusinessCapability `parent_id` Query Parameter Inconsistency

**Status:** 🟢 Ready  
**Priority:** P1 - HIGH  
**Effort:** 1 hour  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

Code extracts and uses `parent_id` query parameter for BusinessCapability filtering, but it's not documented in OpenAPI specification. This creates an undocumented API behavior that could confuse developers.

**Impact:** API parameter works but isn't discoverable through OpenAPI/Swagger UI.

---

## Affected Files

- [openapi.yaml](../../openapi.yaml#L759-L774) - GET /business-capabilities endpoint
- [src/Api/BusinessCapabilitiesEndpoints.fs](../../src/Api/BusinessCapabilitiesEndpoints.fs#L18)

---

## Detailed Tasks

- [ ] Add `parent_id` query parameter to GET /business-capabilities in OpenAPI spec
- [ ] Add parameter description: "Filter by parent business capability"
- [ ] Add parameter example: "cap-parent-id"
- [ ] Add parameter as optional (not required)
- [ ] Verify parameter works as expected
- [ ] Update API documentation to mention filtering by parent_id

---

## Acceptance Criteria

- [ ] `parent_id` query parameter documented in OpenAPI
- [ ] Parameter has clear description
- [ ] Parameter appears in Swagger UI
- [ ] Tests verify filtering works
- [ ] API responses match documented schema

---

## OpenAPI Changes

```yaml
/business-capabilities:
  get:
    tags: [BusinessCapabilities]
    summary: List business capabilities
    parameters:
      - $ref: '#/components/parameters/page'
      - $ref: '#/components/parameters/limit'
      - in: query
        name: parent_id
        schema:
          type: string
        description: Filter by parent business capability ID
        example: cap-parent-uuid
    responses:
      '200':
        description: List business capabilities
```

---

## Dependencies

**Blocks:** None

**Depends On:** None

---

## Related Items

- [Item-013-Prio-P2.md](Item-013-Prio-P2.md) - Standardize query patterns
- [src/Api/BusinessCapabilitiesEndpoints.fs](../../src/Api/BusinessCapabilitiesEndpoints.fs)

---

## Definition of Done

- [x] OpenAPI spec includes parent_id parameter
- [x] Parameter documented with description
- [x] Swagger UI shows the parameter
- [x] Integration test verifies filtering

---

## Notes

- This is a quick fix to align spec with code
- No code changes needed, only OpenAPI documentation
- Part of larger effort to standardize query parameters (Item-013)
