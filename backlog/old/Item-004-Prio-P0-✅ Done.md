# Item-004: Create API Contract Specification

**Status:** ✅ Done  
**Priority:** P0 - CRITICAL  
**Effort:** 6-8 hours  
**Created:** 2026-01-06  
**Completed:** 2026-01-06  
**Owner:** EA Platform Team

---

## Problem Statement

OpenAPI specification exists (openapi.yaml) but lacks a formal specification document explaining:
- REST API design patterns
- Request/response patterns
- Pagination and filtering conventions
- Error handling and status codes
- Header conventions
- Versioning strategy

**Impact:** Developers lack comprehensive API design documentation; API consumers have incomplete guidance.

---

## Affected Files

**Create:** [spec/spec-tool-api-contract.md](../../spec/spec-tool-api-contract.md) (new file)

**Reference:**
- [openapi.yaml](../../openapi.yaml)
- [docs/api-usage-guide.md](../../docs/api-usage-guide.md) (missing, see Item-014)

---

## Detailed Tasks

- [x] Create spec-tool-api-contract.md following specification template
- [x] Document REST endpoint patterns:
  - Collection endpoints: GET, POST
  - Resource endpoints: GET, PATCH, DELETE
  - Status codes: 200, 201, 204, 400, 403, 404, 422
  - Error response structure

- [x] Document request/response patterns:
  - Standard header requirements (Authorization, Content-Type)
  - Timestamp format (ISO 8601 UTC with Z)
  - ID format (UUID v4)
  - Data types for all schemas

- [x] Document pagination:
  - page (1-based)
  - limit (default 50, max 200)
  - total count in response
  - Response structure with items/page/limit/total

- [x] Document filtering and search:
  - Search parameter (full-text on common fields)
  - Field-specific filters
  - Date range filters
  - Multi-value filters (tags)

- [x] Document error handling:
  - Error code taxonomy (validation_error, not_found, forbidden, etc.)
  - Field-level error structure
  - Trace ID for debugging
  - HTTP status code meanings

- [x] Document response structure patterns:
  - Single entity responses
  - Collection/paginated responses
  - Error responses
  - Async job responses (202 Accepted)

- [x] Document headers:
  - Authorization: Bearer <token> or X-Api-Key
  - Correlation-Id for request tracing
  - X-RateLimit-* headers
  - Location header on POST (201 Created)

- [x] Document authentication integration:
  - OIDC token format
  - API key format
  - JWT claims structure

- [x] Expose openapi.yaml and the api specification document available from the backend
  - /API should give the api specification document with a link to /OpenApiSpecification
  - /OpenApiSpecification should expose the openapi.yaml with approriate viewer

---

## Acceptance Criteria

- [x] spec-tool-api-contract.md is comprehensive
- [x] All HTTP methods documented
- [x] All response patterns documented
- [x] Error codes with examples
- [x] Headers with purposes documented
- [x] Status codes explain when to use each
- [x] Examples for each pattern
- [x] Links to openapi.yaml for machine-readable version
- [x] Linked from spec-index.md

---

## Template Sections

```markdown
# API Contract Specification

## 1. Purpose & Scope

## 2. Definitions

## 3. Requirements & Guidelines

### REST Design
- REQ-API-001: All resources use plural nouns in URLs
- REQ-API-002: All datetime fields use ISO 8601 UTC with Z
- etc.

### Request/Response Patterns
- GUD-API-001: Error responses always include code and message
- GUD-API-002: All list endpoints support pagination
- etc.

## 4. HTTP Methods

### GET - Retrieve resources
- Single: GET /resource/{id} → 200 + entity
- List: GET /resource?page=1&limit=50 → 200 + {items, page, limit, total}

### POST - Create resources
- POST /resource + body → 201 + entity + Location header

### PATCH - Update resources
- PATCH /resource/{id} + body → 200 + entity

### DELETE - Delete resources
- DELETE /resource/{id} → 204 No Content

## 5. Response Structure

### Success Responses
- Single resource
- Paginated collection
- Empty (204)

### Error Responses
- Validation errors (field-level)
- Not found
- Forbidden
- Server errors

## 6. Status Codes

| Code | Meaning | When |
|------|---------|------|
| 200 | OK | GET, PATCH |
| 201 | Created | POST |
| 204 | No Content | DELETE |
| 400 | Bad Request | Invalid request syntax |
| 403 | Forbidden | Access denied |
| 404 | Not Found | Resource doesn't exist |
| 422 | Unprocessable Entity | Validation failed |
| 500 | Server Error | Unexpected error |

## 7. Headers

- Authorization
- Content-Type
- X-Correlation-Id
- X-RateLimit-*
- Location (on POST)

## 8. Pagination

## 9. Filtering & Search

## 10. Error Handling

## 11. Examples
```

---

## Dependencies

**Blocks:** None (foundational)

**Depends On:** None

---

## Related Items

- [Item-010-Prio-P1.md](Item-010-Prio-P1.md) - Error Handling spec (subset)
- [Item-011-Prio-P1.md](Item-011-Prio-P1.md) - Query Patterns spec (subset)
- [openapi.yaml](../../openapi.yaml) - Machine-readable contract

---

## Definition of Done

- [x] spec-tool-api-contract.md created and comprehensive
- [x] All REST patterns documented
- [x] Error handling documented
- [x] Headers documented
- [x] Examples for each pattern
- [x] Linked from spec-index.md
- [x] Reviewed by team

---

## Notes

- This specification is the human-readable version of openapi.yaml
- Should stay in sync with OpenAPI file
- Developers should read this first, then check openapi.yaml for details
- Basis for Item-014 (API Usage Guide) which provides practical examples

## Resolution

Created comprehensive API contract specification at [spec/spec-tool-api-contract.md](../../spec/spec-tool-api-contract.md):

**Documentation Created** (19 sections, ~750 lines):
- HTTP Methods: GET, POST, PATCH, DELETE with examples
- Response structures: Single resource, paginated collections, errors, empty
- Status codes: Complete matrix with usage guidelines (200, 201, 204, 400, 403, 404, 422, 500, 503)
- Headers: Authorization, Content-Type, Location, X-Correlation-Id, X-RateLimit-*
- Pagination: page/limit parameters, default behavior, navigation patterns
- Filtering & Search: Full-text search, field-specific filters, hierarchical queries
- Error handling: Structured errors with code/message/trace_id, field-level validation
- Authentication: OIDC Bearer tokens, API keys, JWT claims structure
- Versioning strategy: Current state and future options
- 6 complete examples covering common API operations

**Implementation Added**:
- GET /API endpoint serving spec-tool-api-contract.md as text/markdown
- GET /OpenApiSpecification endpoint serving openapi.yaml as application/x-yaml
- Updated src/Api/Endpoints.fs with new routes

**Linked from**:
- spec-index.md in API & Contracts section (removed "planned" status)
- Quick reference guides for API Consumers, Frontend Developers, Integration Partners

**Implementation Status Section**:
- Documents what's currently implemented vs planned
- Identifies partial implementations (Location header, trace_id)
- Lists not yet implemented features (rate limiting, soft delete, versioning)

This establishes the foundation for consistent API design and serves as authoritative reference for all stakeholders.
