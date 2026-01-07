# Item-061: Soft Delete Implementation Across All Entities

**Status:** 🟢 Ready  
**Priority:** P2 - MEDIUM  
**Effort:** 10-12 hours  
**Created:** 2026-01-07  
**Owner:** TBD

---

## Problem Statement

The specification requires soft delete capability (REQ-004) to preserve audit trails and enable recovery, but hard deletes are currently implemented. This prevents:

- Recovering accidentally deleted entities
- Maintaining complete audit trails
- Checking deletion history
- GDPR-compliant data management with deletion dates

Soft delete (deleted_at field) is missing for all entities: Organization, Application, Server, BusinessCapability, DataEntity, Integration, Relation.

---

## Affected Files

**Database:**
- Create migration: Add deleted_at (nullable timestamp) to all entity tables
- Create migration: Add index on deleted_at for soft-delete queries

**Repositories:**
- `src/Infrastructure/Repository/OrganizationRepository.fs` - Implement soft delete
- `src/Infrastructure/Repository/ApplicationRepository.fs` - Implement soft delete
- `src/Infrastructure/Repository/ServerRepository.fs` - Implement soft delete
- `src/Infrastructure/Repository/BusinessCapabilityRepository.fs` - Implement soft delete
- `src/Infrastructure/Repository/DataEntityRepository.fs` - Implement soft delete
- `src/Infrastructure/Repository/IntegrationRepository.fs` - Implement soft delete
- `src/Infrastructure/Repository/RelationRepository.fs` - Implement soft delete
- Create `src/Infrastructure/Repository/SoftDeleteRepository.fs` - Base soft delete patterns

**API:**
- `src/Api/Handlers/*Handlers.fs` - Update delete to soft delete
- `src/Api/Requests/*.fs` - Add ?include_deleted query parameter for retrieval endpoints

**Tests:**
- `tests/*CommandTests.fs` - Test soft delete for each entity
- Create integration test for soft delete recovery

---

## Specifications

- [spec/spec-schema-domain-overview.md](../spec/spec-schema-domain-overview.md) - REQ-004: Soft delete requirement
- [spec/spec-implementation-status.md](../spec/spec-implementation-status.md) - Gap #14

---

## Detailed Tasks

- [ ] **Create SoftDeleteRepository.fs base module**:
  - Define soft delete patterns to reuse across repositories
  - Helper function: `softDelete : id -> repository -> Result<unit, error>`
  - Helper function: `permanentDelete : id -> repository -> Result<unit, error>` (admin only)
  - Query filters to exclude soft-deleted items by default
  - Query filters to include soft-deleted items when requested

- [ ] **Database migrations**:
  - Add deleted_at (nullable timestamp, ISO 8601 UTC) to:
    - organizations
    - applications
    - servers
    - business_capabilities
    - data_entities
    - integrations
    - relations
  - Add index on deleted_at for query performance
  - Test migrations: up and down
  - Existing data: set deleted_at to NULL (not deleted)

- [ ] **Update all repositories**:
  - All Get/List queries filter WHERE deleted_at IS NULL by default
  - Add parameter: ?include_deleted to include soft-deleted items
  - Delete operation: UPDATE table SET deleted_at = NOW() instead of DELETE
  - Permanent delete: actual DELETE (admin only, separate endpoint)
  - Cascade soft deletes: when parent deleted, soft delete children

- [ ] **API delete endpoints**:
  - DELETE /entities/{id} → soft delete (sets deleted_at)
  - Response: 204 No Content or 200 OK with updated entity
  - DELETE /entities/{id}?permanent=true → permanent delete (admin only)
  - Add warning header if attempting permanent delete of soft-deleted item

- [ ] **API retrieval endpoints**:
  - Add optional query parameter: ?include_deleted=true
  - Default behavior: exclude soft-deleted items
  - If include_deleted=true, return all items including soft-deleted
  - Soft-deleted items: mark in response with "deleted_at" timestamp

- [ ] **Restore capability (optional enhancement)**:
  - PATCH /entities/{id}/restore → restore soft-deleted item (sets deleted_at to NULL)
  - Only works on soft-deleted items (deleted_at NOT NULL)
  - Response: 200 OK with restored entity

- [ ] **Validation & constraints**:
  - Relations to soft-deleted entities should fail validation
  - Prevent operations on soft-deleted entities (can't update deleted entity)
  - Can still view soft-deleted entities with include_deleted=true

- [ ] **Test coverage**:
  - Soft delete: verify deleted_at is set, item excluded from default queries
  - Include deleted: verify soft-deleted items returned with include_deleted=true
  - Cascade: when parent soft-deleted, children soft-deleted
  - Restore: soft-deleted items can be restored
  - Validation: operations on soft-deleted items fail appropriately
  - Permanent delete: admin operation, removes row completely
  - Query performance: index on deleted_at improves soft-delete filtering

- [ ] **Documentation**:
  - Update API spec to document delete behavior (soft vs permanent)
  - Document include_deleted query parameter
  - Document restore endpoint (if implemented)
  - Update runbook for data recovery procedures

---

## Acceptance Criteria

- [ ] deleted_at column added to all entity tables (migration created)
- [ ] All repositories filter soft-deleted items by default
- [ ] ?include_deleted=true parameter includes soft-deleted items
- [ ] Delete operations set deleted_at timestamp (soft delete)
- [ ] Soft-deleted items excluded from normal queries
- [ ] Admin can permanently delete items (separate endpoint)
- [ ] Cascade soft deletes when parent deleted
- [ ] Relations to soft-deleted entities fail validation
- [ ] All tests pass (including soft delete tests)
- [ ] Restore functionality works (if implemented)
- [ ] Database index on deleted_at exists for query performance
- [ ] API documentation updated
- [ ] Build succeeds with 0 errors, 0 warnings

---

## Dependencies

**Blocks:**
- None

**Depends On:**
- Item-056 - Required fields (complementary validation)

**Related:**
- Item-021 - Implementation status tracker (gap analysis)
- Item-036 - AuditLog as event projection (complementary for audit)

---

## Notes

Soft delete is a complex change affecting all repositories and API endpoints. Plan for careful migration of existing data. Consider whether cascade soft deletes should be automatic or require admin confirmation. Document the difference between soft delete (reversible) and permanent delete (irreversible).
