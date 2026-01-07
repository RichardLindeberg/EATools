# Item-062: Created/Modified Timestamp Tracking

**Status:** 🟢 Ready  
**Priority:** P2 - MEDIUM  
**Effort:** 6-8 hours  
**Created:** 2026-01-07  
**Owner:** TBD

---

## Problem Statement

The specification requires created_at and modified_at timestamps (REQ-005) to enable audit trails and timeline analysis, but these are currently missing across all entities. This prevents:

- Auditing when entities were created
- Tracking last modification time
- Analyzing creation/update trends
- GDPR compliance for data lifecycle tracking
- Detecting stale data that hasn't been updated

---

## Affected Files

**Database:**
- Create migration: Add created_at (timestamp, default NOW) to all entity tables
- Create migration: Add modified_at (timestamp, nullable) to all entity tables
- Create migration: Add database trigger to auto-update modified_at on UPDATE

**Repositories:**
- `src/Infrastructure/Repository/*Repository.fs` - All repositories set created_at on insert
- `src/Infrastructure/Repository/*Repository.fs` - All repositories set modified_at on update

**Domain:**
- `src/Domain/Entities/*Entity.fs` - Add created_at field to all domain entities
- `src/Domain/ValueObjects/Timestamps.fs` - Create domain value object for timestamps

**API:**
- `src/Api/Responses/*Response.fs` - Include created_at, modified_at in all entity responses
- Update OpenAPI spec to include created_at and modified_at fields

**Tests:**
- `tests/*CommandTests.fs` - Verify created_at set on creation
- `tests/*CommandTests.fs` - Verify modified_at updated on modification

---

## Specifications

- [spec/spec-schema-domain-overview.md](../spec/spec-schema-domain-overview.md) - REQ-005: Timestamp tracking
- [spec/spec-implementation-status.md](../spec/spec-implementation-status.md) - Gap #15

---

## Detailed Tasks

- [ ] **Create Timestamps value object**:
  - `src/Domain/ValueObjects/Timestamps.fs`
  - Type: CreatedAt (instant, immutable)
  - Type: ModifiedAt (instant option, updated on changes)
  - Constructor: validate timestamps are UTC
  - Serialization to ISO 8601

- [ ] **Update all domain entities**:
  - Add `created_at: CreatedAt` field to:
    - Organization
    - Application
    - Server
    - BusinessCapability
    - DataEntity
    - Integration
    - Relation
  - Add `modified_at: ModifiedAt` field (optional)

- [ ] **Database migrations**:
  - Add created_at (timestamp NOT NULL, default CURRENT_TIMESTAMP) to:
    - organizations
    - applications
    - servers
    - business_capabilities
    - data_entities
    - integrations
    - relations
  - Add modified_at (timestamp, nullable) to same tables
  - Create trigger (if using SQL):
    ```sql
    CREATE TRIGGER update_[table]_modified_at
    BEFORE UPDATE ON [table]
    FOR EACH ROW
    SET NEW.modified_at = CURRENT_TIMESTAMP;
    ```
  - Test migrations: up and down
  - Backfill created_at for existing records (use creation_date if available, or NOW())

- [ ] **Update all repositories**:
  - Insert operations: explicitly set created_at to current UTC timestamp
  - Update operations: set modified_at to current UTC timestamp (or use trigger)
  - Query operations: include created_at and modified_at in all results
  - Mapping: map database timestamps to domain value objects

- [ ] **API responses**:
  - All entity response DTOs include:
    - created_at: ISO 8601 string
    - modified_at: ISO 8601 string (null if never modified)
  - Example:
    ```json
    {
      "id": "org-001",
      "name": "ACME Corp",
      "created_at": "2026-01-01T10:30:00Z",
      "modified_at": "2026-01-05T14:22:15Z"
    }
    ```

- [ ] **Update OpenAPI specification**:
  - `openapi.yaml` - add created_at and modified_at to all entity schemas
  - Type: string (date-time), format: ISO 8601
  - created_at: required, readOnly
  - modified_at: required, readOnly

- [ ] **List/Filter endpoints**:
  - Add query parameters for filtering by timestamp:
    - ?created_after=2026-01-01T00:00:00Z
    - ?created_before=2026-01-31T23:59:59Z
    - ?modified_after=2026-01-01T00:00:00Z
    - ?modified_before=2026-01-31T23:59:59Z
  - Add sort options:
    - ?sort_by=created_at (ascending default)
    - ?sort_by=modified_at
    - ?sort_order=desc

- [ ] **Test coverage**:
  - created_at: verify set to current time on create
  - created_at: immutable (can't be changed after creation)
  - modified_at: NULL immediately after creation
  - modified_at: updated to current time on update
  - modified_at: unchanged when retrieving entity
  - Timestamp filtering: created_after/before work correctly
  - Timestamp filtering: modified_after/before work correctly
  - Timestamp sorting: can sort by created_at and modified_at
  - Timezone: all timestamps in UTC

- [ ] **Documentation**:
  - Update API documentation with timestamp fields
  - Document timestamp filtering capabilities
  - Document that timestamps are immutable (can't set manually)
  - Update schema documentation

---

## Acceptance Criteria

- [ ] created_at column added to all entity tables (migration)
- [ ] modified_at column added to all entity tables (migration)
- [ ] All domain entities include created_at and modified_at fields
- [ ] created_at set automatically on entity creation
- [ ] modified_at set automatically on entity update (trigger or code)
- [ ] All API responses include created_at and modified_at
- [ ] Timestamps in ISO 8601 UTC format
- [ ] Query parameters for filtering by date range work correctly
- [ ] Sort by created_at and modified_at works
- [ ] created_at is immutable (read-only in API)
- [ ] modified_at is read-only in API
- [ ] All tests pass (timestamp tests included)
- [ ] OpenAPI spec updated with timestamp fields
- [ ] Build succeeds with 0 errors, 0 warnings

---

## Dependencies

**Blocks:**
- None

**Depends On:**
- None (can be implemented independently)

**Related:**
- Item-061 - Soft delete (uses created_at for timeline)
- Item-036 - AuditLog (complementary for detailed changes)

---

## Notes

Timestamps are critical for audit trails and compliance. Ensure all timestamps are in UTC to avoid timezone confusion. Database triggers can auto-update modified_at to reduce code complexity. Consider adding triggers for consistency and performance. Make created_at immutable in the domain to prevent accidental modification.
