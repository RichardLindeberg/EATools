# EATool - Project Backlog

> **Last Updated:** 2026-01-06  
> **Status Key:** 🔴 Blocked | 🟡 In Progress | 🟢 Ready | ✅ Done

---

## 🔥 P0 - Critical (Immediate Action Required)

### 1. Add `parent_id` Support to Organization Entity
**Priority:** P0 - CRITICAL  
**Status:** 🟢 Ready  
**Effort:** 4-6 hours  

**Problem:**
Organization hierarchies are documented and specified in OpenAPI but completely missing from implementation. This breaks the API contract and prevents users from modeling organizational structures (departments → divisions → enterprise).

**Affected Files:**
- [src/Infrastructure/Migrations/002_create_organizations.sql](src/Infrastructure/Migrations/002_create_organizations.sql)
- [src/Domain/Models.fs](src/Domain/Models.fs#L8-L22) - Organization type
- [src/Domain/Models.fs](src/Domain/Models.fs#L227-L234) - CreateOrganizationRequest type
- [src/Infrastructure/OrganizationRepository.fs](src/Infrastructure/OrganizationRepository.fs)
- [src/Infrastructure/Json.fs](src/Infrastructure/Json.fs) - Encoder/decoder functions

**Tasks:**
- [ ] Create migration `008_add_parent_id_to_organizations.sql`
- [ ] Add `ParentId: string option` to Organization domain model
- [ ] Add `ParentId: string option` to CreateOrganizationRequest
- [ ] Update `OrganizationRepository.mapOrganization` to read parent_id
- [ ] Update `OrganizationRepository.create` to insert parent_id
- [ ] Update `OrganizationRepository.update` to update parent_id
- [ ] Add query parameter support for filtering by parent_id
- [ ] Update JSON encoder/decoder for parent_id field
- [ ] Add index on parent_id column for performance

**Acceptance Criteria:**
- [ ] Can create organization with parent_id
- [ ] Can update organization's parent_id
- [ ] Can query organizations by parent_id
- [ ] Can retrieve full organizational hierarchy
- [ ] API responses match OpenAPI schema (including parent_id)
- [ ] Integration tests pass

**Dependencies:** None

---

### 2. Add Integration Tests for Organization Hierarchy
**Priority:** P0 - CRITICAL  
**Status:** 🔴 Blocked (by task #1)  
**Effort:** 2-3 hours  

**Problem:**
No test coverage for the documented hierarchical organization feature. Tests would currently fail because the feature is not implemented.

**Affected Files:**
- [tests/integration/test_organizations.py](tests/integration/test_organizations.py)

**Tasks:**
- [ ] Test creating organization with parent_id
- [ ] Test creating multi-level hierarchy (grandparent → parent → child)
- [ ] Test updating parent_id (moving org in hierarchy)
- [ ] Test querying organizations by parent_id
- [ ] Test circular reference prevention (child cannot be its own ancestor)
- [ ] Test deleting parent organization (cascade behavior or constraint)
- [ ] Test null parent_id for root organizations

**Acceptance Criteria:**
- [ ] All hierarchy tests pass
- [ ] Edge cases covered (null parent, circular refs, orphans)
- [ ] Test data cleanup works correctly

**Dependencies:** Task #1 (Organization parent_id implementation)

---

## 🔸 P1 - High Priority

### 3. Fix BusinessCapability `parent_id` Query Parameter Inconsistency
**Priority:** P1 - HIGH  
**Status:** 🟢 Ready  
**Effort:** 1 hour  

**Problem:**
Code extracts and uses `parent_id` query parameter but it's not documented in OpenAPI spec. This creates an undocumented API behavior.

**Affected Files:**
- [openapi.yaml](openapi.yaml#L759-L774) - GET /business-capabilities endpoint
- [src/Api/BusinessCapabilitiesEndpoints.fs](src/Api/BusinessCapabilitiesEndpoints.fs#L18)

**Tasks:**
- [ ] Add `parent_id` query parameter to OpenAPI spec (GET /business-capabilities)
- [ ] Add description and example
- [ ] Verify parameter works as expected
- [ ] Update API documentation

**Acceptance Criteria:**
- [ ] OpenAPI spec includes parent_id parameter
- [ ] Parameter documented with description and example
- [ ] Swagger UI shows the parameter

**Dependencies:** None

---

### 4. Review and Simplify Lifecycle Storage Strategy
**Priority:** P1 - HIGH  
**Status:** 🟢 Ready  
**Effort:** 3-4 hours  

**Problem:**
Application model stores lifecycle in two fields: `Lifecycle: Lifecycle` (discriminated union) and `LifecycleRaw: string`. This creates redundancy, complexity, and potential for inconsistency. Purpose is unclear.

**Affected Files:**
- [src/Domain/Models.fs](src/Domain/Models.fs#L24-L35) - Application type
- [src/Domain/Models.fs](src/Domain/Models.fs#L227-L240) - CreateApplicationRequest
- [src/Infrastructure/Migrations/001_create_applications.sql](src/Infrastructure/Migrations/001_create_applications.sql)
- [src/Infrastructure/ApplicationRepository.fs](src/Infrastructure/ApplicationRepository.fs)
- [src/Infrastructure/Json.fs](src/Infrastructure/Json.fs) - Application encoder/decoder

**Investigation Needed:**
- [ ] Document why both fields exist
- [ ] Determine if both are actually needed
- [ ] Check if any consumers rely on lifecycle_raw specifically

**Options:**
1. **Keep discriminated union only** - Store as normalized string in DB, parse to union on read
2. **Keep string only** - Remove discriminated union, validate string values
3. **Keep both** - Document the rationale clearly

**Tasks:**
- [ ] Investigate usage patterns
- [ ] Choose approach based on findings
- [ ] Implement chosen solution
- [ ] Update tests
- [ ] Add documentation explaining the decision

**Acceptance Criteria:**
- [ ] Single source of truth for lifecycle state
- [ ] Clear documentation of approach
- [ ] No breaking changes to API contract
- [ ] Tests updated and passing

**Dependencies:** None

---

## 🔹 P2 - Medium Priority

### 5. Add `parent_id` Filter to Organizations Endpoint
**Priority:** P2 - MEDIUM  
**Status:** 🔴 Blocked (by task #1)  
**Effort:** 2 hours  

**Problem:**
Organizations support hierarchies but lack query parameter to filter by parent_id, making it difficult to query organizational structure.

**Affected Files:**
- [openapi.yaml](openapi.yaml#L59-L80) - GET /organizations
- [src/Api/OrganizationsEndpoints.fs](src/Api/OrganizationsEndpoints.fs#L16-L19)
- [src/Infrastructure/OrganizationRepository.fs](src/Infrastructure/OrganizationRepository.fs#L38-L49)

**Tasks:**
- [ ] Add parent_id query parameter to OpenAPI spec
- [ ] Update OrganizationsEndpoints to extract parent_id param
- [ ] Update OrganizationRepository.buildFilters to support parent_id
- [ ] Add integration test for filtering by parent_id
- [ ] Update documentation

**Acceptance Criteria:**
- [ ] Can filter organizations by parent_id
- [ ] Can query for root organizations (parent_id=null)
- [ ] OpenAPI spec documents the parameter
- [ ] Tests verify filtering works correctly

**Dependencies:** Task #1 (Organization parent_id implementation)

---

### 6. Standardize Query Parameter Patterns Across Endpoints
**Priority:** P2 - MEDIUM  
**Status:** 🟢 Ready  
**Effort:** 4-6 hours  

**Problem:**
Inconsistent query parameter naming and filtering patterns across endpoints. Some use `search`, some use entity-specific filters, creating poor developer experience.

**Affected Files:**
- [openapi.yaml](openapi.yaml) - All endpoint definitions
- All endpoint files in [src/Api/](src/Api/)
- All repository files in [src/Infrastructure/](src/Infrastructure/)

**Tasks:**
- [ ] Audit all endpoints and document current query parameters
- [ ] Design consistent parameter naming convention
- [ ] Define standard filters (search, sort, filter patterns)
- [ ] Update OpenAPI spec with standardized parameters
- [ ] Implement changes across all endpoints
- [ ] Update integration tests
- [ ] Document query parameter standards

**Proposed Standards:**
- `search` - Full-text search across multiple fields
- `sort` - Field name with optional +/- prefix for direction
- `filter[field]` - Exact match on specific field
- `page`, `limit` - Pagination (already consistent)

**Acceptance Criteria:**
- [ ] All endpoints follow consistent query parameter patterns
- [ ] OpenAPI spec documents all query parameters
- [ ] Developer documentation explains query patterns
- [ ] All tests updated and passing

**Dependencies:** None

---

### 7. Add Missing API Documentation Files
**Priority:** P2 - MEDIUM  
**Status:** 🟢 Ready  
**Effort:** 4-6 hours  

**Problem:**
README references documentation files that don't exist, creating broken links and gaps in documentation.

**Affected Files:**
- [docs/README.md](docs/README.md)

**Missing Files:**
- `docs/api-usage-guide.md`
- `docs/authorization-guide.md`

**Tasks:**
- [ ] Create `api-usage-guide.md` with:
  - Authentication setup
  - Making requests (curl/Postman examples)
  - Pagination patterns
  - Filtering and search
  - Error handling
  - Rate limiting
  - Best practices
- [ ] Create `authorization-guide.md` with:
  - OpenID Connect configuration
  - JWT token structure
  - API key management
  - OPA/Rego policy examples
  - Role-based access control
  - Attribute-based access control
  - Common authorization patterns
- [ ] Add code examples
- [ ] Add troubleshooting sections
- [ ] Link to OpenAPI spec

**Acceptance Criteria:**
- [ ] Both documentation files exist
- [ ] Content is comprehensive and accurate
- [ ] Examples are tested and working
- [ ] Links in README work correctly

**Dependencies:** None

---

## 🔷 P3 - Low Priority

### 8. Fix Agent Configuration Errors
**Priority:** P3 - LOW  
**Status:** 🟢 Ready  
**Effort:** 30 minutes  

**Problem:**
Agent configuration file references unknown tools and non-existent directories.

**Affected Files:**
- [.github/agents/specification.agent.md](.github/agents/specification.agent.md#L3)

**Tasks:**
- [ ] Remove unknown tool references: `findTestFiles`, `microsoft.docs.mcp`, `github`
- [ ] Fix or remove reference to `/spec/` directory
- [ ] Verify agent configuration is valid
- [ ] Test agent functionality

**Acceptance Criteria:**
- [ ] No validation errors in agent configuration
- [ ] Agent tools are all valid
- [ ] Directory references are correct

**Dependencies:** None

---

### 9. Document Dual Database Environment Strategy
**Priority:** P3 - LOW  
**Status:** 🟢 Ready  
**Effort:** 1 hour  

**Problem:**
Documentation mentions SQLite, PostgreSQL, and MSSQL for different environments, but actual implementation only uses SQLite. This may confuse developers.

**Affected Files:**
- [docs/system-overview.md](docs/system-overview.md#L32-L35)
- [src/Infrastructure/Database.fs](src/Infrastructure/Database.fs#L19-L34)

**Tasks:**
- [ ] Clarify actual database support in documentation
- [ ] Document migration path from SQLite to MSSQL if planned
- [ ] Update system-overview.md to match current implementation
- [ ] Add database configuration guide
- [ ] Document connection string formats

**Acceptance Criteria:**
- [ ] Documentation accurately reflects current implementation
- [ ] Future database plans (if any) are documented separately
- [ ] Configuration examples provided

**Dependencies:** None

---

## 📋 Backlog Items (Not Yet Prioritized)

### Future Enhancements
- [ ] Add cascading delete options for parent-child relationships
- [ ] Implement soft delete for all entities
- [ ] Add bulk operations (bulk create, update, delete)
- [ ] Add GraphQL endpoint as alternative to REST
- [ ] Implement real-time updates via WebSocket/SSE
- [ ] Add data export formats (CSV, Excel, ArchiMate XML)
- [ ] Implement full-text search with advanced query syntax
- [ ] Add audit logging for all operations
- [ ] Implement versioning/history for entities

### Technical Debt
- [ ] Evaluate moving from SQLite to PostgreSQL for production
- [ ] Add comprehensive logging and observability
- [ ] Implement caching layer (Redis)
- [ ] Add performance benchmarks and load testing
- [ ] Security audit and penetration testing
- [ ] Implement rate limiting at API level
- [ ] Add API request/response validation middleware
- [ ] Containerize application (Dockerfile)
- [ ] Set up CI/CD pipeline

---

## 📊 Progress Tracking

### By Priority
- **P0 (Critical):** 0/2 complete (0%)
- **P1 (High):** 0/2 complete (0%)
- **P2 (Medium):** 0/3 complete (0%)
- **P3 (Low):** 0/2 complete (0%)

### By Status
- 🟢 Ready: 7
- 🔴 Blocked: 2
- 🟡 In Progress: 0
- ✅ Done: 0

---

## 🎯 Next Sprint Recommendations

**Sprint Goal:** Fix critical data model inconsistencies and establish test coverage

**Suggested Tasks:**
1. Task #1 - Add parent_id to Organization (P0)
2. Task #2 - Integration tests for organization hierarchy (P0)
3. Task #3 - Fix BusinessCapability parent_id query parameter (P1)
4. Task #8 - Fix agent configuration (P3, quick win)

**Estimated Effort:** 8-12 hours

---

## 📝 Notes

- All file paths are relative to project root: `/home/richard/Projects/EATool`
- Breaking changes should be documented in CHANGELOG.md
- OpenAPI spec changes require version bump
- Database migrations must be backwards compatible
- Integration tests should run against clean database state

---

## 🤝 Contributing

When working on backlog items:
1. Update status to 🟡 In Progress
2. Create feature branch: `feature/task-{number}-brief-description`
3. Reference task number in commits: `[Task #1] Add parent_id to Organization`
4. Update acceptance criteria as you complete them
5. Move to ✅ Done when all acceptance criteria met
6. Update progress tracking metrics
