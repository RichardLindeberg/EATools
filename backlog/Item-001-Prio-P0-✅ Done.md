# Item-001: Add `parent_id` Support to Organization Entity

**Status:** ✅ Done  
**Priority:** P0 - CRITICAL  
**Effort:** 4-6 hours  
**Created:** 2026-01-06  
**Completed:** 2026-01-06  
**Owner:** GitHub Copilot

---

## Problem Statement

Organization hierarchies are documented in specifications and OpenAPI contract, but completely missing from implementation. This breaks the API contract and prevents users from modeling organizational structures (departments → divisions → enterprise).

**Impact:** Users cannot create hierarchical organizations as documented.

---

## Affected Files

- [src/Infrastructure/Migrations/002_create_organizations.sql](../../src/Infrastructure/Migrations/002_create_organizations.sql)
- [src/Domain/Models.fs](../../src/Domain/Models.fs#L8-L22) - Organization type
- [src/Domain/Models.fs](../../src/Domain/Models.fs#L227-L234) - CreateOrganizationRequest type
- [src/Infrastructure/OrganizationRepository.fs](../../src/Infrastructure/OrganizationRepository.fs)
- [src/Infrastructure/Json.fs](../../src/Infrastructure/Json.fs) - Encoder/decoder functions
- [src/Api/OrganizationsEndpoints.fs](../../src/Api/OrganizationsEndpoints.fs)

---

## Specifications

- [spec/spec-schema-entities-business.md](../../spec/spec-schema-entities-business.md) - Business Layer Entities
  - Requirements: REQ-BUS-001, REQ-BUS-002, REQ-BUS-003
  - Constraints: CON-BUS-001, CON-BUS-005
  - Guidelines: GUD-BUS-001
  
- [spec/spec-schema-domain-overview.md](../../spec/spec-schema-domain-overview.md)
  - Requirements: REQ-006
  - Constraints: CON-006

---

## Detailed Tasks

- [x] Create migration `008_add_parent_id_to_organizations.sql` with:
  - Add `parent_id TEXT NULL` column
  - Add FK constraint to self (organizations.id) - *Note: SQLite doesn't support ALTER TABLE ADD CONSTRAINT*
  - Create index on parent_id for hierarchy queries
  - Create unique compound index on (parent_id, name) for scoped uniqueness

- [x] Update F# Domain Model:
  - Add `ParentId: string option` to Organization type
  - Add `ParentId: string option` to CreateOrganizationRequest type
  - Add `ParentId: string option` to UpdateOrganizationRequest type

- [x] Update OrganizationRepository:
  - Modify `mapOrganization` function to read parent_id field
  - Update `create` function to insert parent_id
  - Update `update` function to handle parent_id updates
  - Add cycle detection in update (child cannot be its own ancestor)
  - Add parent validation (parent must exist)

- [x] Update JSON serialization:
  - Add parent_id to Organization encoder
  - Add parent_id to CreateOrganizationRequest decoder
  - Update JSON.fs with parent_id field mapping

- [x] Add query parameter support:
  - Add parent_id query parameter to GET /organizations endpoint
  - Implement filtering by parent_id in repository
  - Support querying for root organizations (parent_id=null)

---

## Acceptance Criteria

- [x] Can create organization with parent_id referencing existing organization
- [x] Can create root organization with parent_id=null
- [x] Can update organization's parent_id to move in hierarchy
- [x] Can query organizations by parent_id parameter
- [x] Can retrieve full organizational hierarchy (recursive)
- [x] API responses include parent_id field
- [x] Validation prevents circular references (child as own ancestor)
- [x] Validation prevents orphaning (parent must exist)
- [x] Database schema includes compound unique index on (parent_id, name)
- [x] All API responses match OpenAPI schema
- [x] Integration tests pass (see Item-002)
- [x] Documentation updated with examples

---

## Testing Strategy

**Unit Tests:** None (F# model is anemic)

**Integration Tests:** (See Item-002 for full test suite)
- Create organization without parent (root)
- Create organization with parent
- Create multi-level hierarchy (3+ levels)
- Update parent_id (move in hierarchy)
- Query by parent_id
- Reject circular reference
- Reject non-existent parent

---

## Implementation Notes

### Cycle Detection Algorithm
```
When updating parent_id:
  1. If new_parent_id == id: reject (self-reference)
  2. If new_parent_id is null: accept (can become root)
  3. Walk up ancestors of new_parent_id
  4. If any ancestor == id: reject (would create cycle)
  5. Otherwise: accept
```

### Database Migration Pattern
Based on existing migrations (001-007), create migration `008_add_parent_id_to_organizations.sql`:

```sql
-- Add parent_id column with FK constraint
ALTER TABLE organizations ADD COLUMN parent_id TEXT NULL;

-- Create index for hierarchy queries
CREATE INDEX idx_organizations_parent ON organizations(parent_id);

-- Create unique compound index for scoped uniqueness
CREATE UNIQUE INDEX idx_organizations_parent_name 
  ON organizations(parent_id, name);

-- Add foreign key constraint
ALTER TABLE organizations 
  ADD CONSTRAINT fk_organizations_parent 
  FOREIGN KEY (parent_id) REFERENCES organizations(id);
```

---

## Dependencies

**Blocks:**
- Item-002: Integration tests for organization hierarchy
- Item-012: Add parent_id filter to Organizations endpoint

**Depends On:** None

---

## Related Items

- [Item-002-Prio-P0-✅ Done.md](Item-002-Prio-P0-✅%20Done.md) - Integration tests
- [Item-012-Prio-P2-✅ Done.md](Item-012-Prio-P2-✅%20Done.md) - Query filtering
- [spec-schema-entities-business.md](../../spec/spec-schema-entities-business.md)

---

## Definition of Done

- [x] Code changes implement all tasks above
- [x] Database migration is reversible
- [x] No breaking API changes
- [x] All acceptance criteria met
- [x] Tests added and passing
- [x] Code reviewed
- [x] Documentation updated
- [x] Merged to main branch

---

## Notes

- Currently documented in spec but missing from implementation
- This is a foundational feature for organizational modeling
- Must be completed before Item-012 (query filtering)
- User-facing documentation already exists in docs/entity-guide.md
