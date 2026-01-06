---
title: API Contract Specification
version: 1.0
date_created: 2026-01-06
last_updated: 2026-01-06
owner: EA Platform Team
tags: [api, rest, contract, http, endpoints]
---

# Introduction

This specification defines the REST API contract for the EA Tool, including HTTP methods, request/response patterns, error handling, pagination, filtering, headers, and authentication. This is the human-readable companion to the machine-readable [openapi.yaml](../openapi.yaml).

## 1. Purpose & Scope

**Purpose**: Establish consistent REST API patterns, request/response structures, error handling, and conventions for all EA Tool API endpoints.

**Scope**:
- HTTP methods and semantics
- Request/response patterns for all entity operations
- Pagination and collection responses
- Filtering, search, and query parameters
- Error response structure and status codes
- Header conventions
- Authentication and authorization integration
- Versioning strategy

**Out of Scope**: Detailed entity schemas (see entity specifications), authorization policies (see spec-process-authorization.md)

**Audience**: Backend developers, API consumers, integration partners, frontend developers.

## 2. Definitions

- **Resource**: An entity exposed via REST API (e.g., Application, Organization, Server)
- **Collection Endpoint**: Endpoint for listing/creating multiple resources (e.g., `/applications`)
- **Resource Endpoint**: Endpoint for single resource operations (e.g., `/applications/{id}`)
- **Status Code**: HTTP response status indicating outcome (200, 201, 400, 404, etc.)
- **Pagination**: Breaking large result sets into pages for efficient transmission
- **Query Parameter**: URL parameter for filtering, searching, or pagination (e.g., `?page=1&limit=50`)
- **Request Body**: JSON payload sent in POST/PATCH requests
- **Response Body**: JSON payload returned in API responses
- **Error Code**: Application-level error code in error responses (e.g., `validation_error`, `not_found`)
- **Trace ID**: Unique identifier for request tracing and debugging

## 3. Requirements, Constraints & Guidelines

### REST Design Principles

- **REQ-API-001**: All resource URLs MUST use plural nouns (e.g., `/applications`, `/organizations`)
- **REQ-API-002**: All datetime fields MUST use ISO 8601 UTC format with 'Z' suffix (e.g., `2026-01-06T10:00:00Z`)
- **REQ-API-003**: All primary keys MUST be UUIDs (version 4)
- **REQ-API-004**: All JSON property names MUST use snake_case (e.g., `parent_id`, `data_classification`)
- **REQ-API-005**: All endpoints MUST return proper Content-Type: application/json header
- **REQ-API-006**: All list endpoints MUST support pagination with `page` and `limit` parameters
- **REQ-API-007**: All error responses MUST include `code` and `message` fields
- **REQ-API-008**: All successful POST operations MUST return 201 Created with Location header

### Request Validation

- **CON-API-001**: Required fields MUST be validated before processing
- **CON-API-002**: Enum values MUST be lowercase with underscores (e.g., `planned`, `active`, `deprecated`)
- **CON-API-003**: String fields MUST be trimmed and checked for min/max length
- **CON-API-004**: UUID fields MUST be validated for format
- **CON-API-005**: Foreign key references MUST be validated for existence

### Response Design

- **GUD-API-001**: Single resource responses return the entity directly
- **GUD-API-002**: Collection responses MUST include pagination metadata (`items`, `page`, `limit`, `total`)
- **GUD-API-003**: Nullable fields MUST be explicitly set to `null` in JSON (not omitted)
- **GUD-API-004**: Empty arrays MUST be returned as `[]` (not null)
- **GUD-API-005**: Error responses SHOULD include trace_id for debugging

### Header Conventions

- **GUD-API-006**: Include Location header on 201 Created responses
- **GUD-API-007**: Include X-Correlation-Id for request tracing
- **GUD-API-008**: Support If-None-Match for caching (future)

## 4. HTTP Methods

### GET - Retrieve Resources

**Collection Endpoint**: Retrieve multiple resources with pagination

```http
GET /applications?page=1&limit=50&search=payment
Authorization: Bearer <token>

Response: 200 OK
{
  "items": [
    {
      "id": "app-123e4567-e89b-12d3-a456-426614174000",
      "name": "Payment Gateway",
      "owner": "payments@example.com",
      "lifecycle": "active",
      "created_at": "2026-01-01T10:00:00Z",
      "updated_at": "2026-01-06T10:00:00Z"
    }
  ],
  "page": 1,
  "limit": 50,
  "total": 1
}
```

**Resource Endpoint**: Retrieve single resource by ID

```http
GET /applications/{id}
Authorization: Bearer <token>

Response: 200 OK
{
  "id": "app-123e4567-e89b-12d3-a456-426614174000",
  "name": "Payment Gateway",
  "owner": "payments@example.com",
  "lifecycle": "active",
  "capability_id": "cap-456",
  "data_classification": "confidential",
  "tags": ["payment", "critical"],
  "created_at": "2026-01-01T10:00:00Z",
  "updated_at": "2026-01-06T10:00:00Z"
}
```

**Status Codes**:
- `200 OK`: Resource(s) found and returned
- `404 Not Found`: Resource with given ID doesn't exist
- `403 Forbidden`: User lacks permission to view resource

### POST - Create Resources

**Create new resource**:

```http
POST /applications
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "New Application",
  "owner": "team@example.com",
  "lifecycle": "planned",
  "data_classification": "internal",
  "tags": ["new"]
}

Response: 201 Created
Location: /applications/app-789
{
  "id": "app-789",
  "name": "New Application",
  "owner": "team@example.com",
  "lifecycle": "planned",
  "data_classification": "internal",
  "tags": ["new"],
  "created_at": "2026-01-06T10:00:00Z",
  "updated_at": "2026-01-06T10:00:00Z"
}
```

**Status Codes**:
- `201 Created`: Resource successfully created
- `400 Bad Request`: Invalid JSON syntax
- `422 Unprocessable Entity`: Validation failed (field-level errors)
- `403 Forbidden`: User lacks permission to create resource

### PATCH - Update Resources

**Partial update of existing resource**:

```http
PATCH /applications/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "lifecycle": "active",
  "tags": ["production", "critical"]
}

Response: 200 OK
{
  "id": "app-789",
  "name": "New Application",
  "owner": "team@example.com",
  "lifecycle": "active",
  "data_classification": "internal",
  "tags": ["production", "critical"],
  "created_at": "2026-01-06T10:00:00Z",
  "updated_at": "2026-01-06T10:05:00Z"
}
```

**Status Codes**:
- `200 OK`: Resource successfully updated
- `400 Bad Request`: Invalid JSON syntax
- `422 Unprocessable Entity`: Validation failed
- `404 Not Found`: Resource doesn't exist
- `403 Forbidden`: User lacks permission to update resource

### DELETE - Remove Resources

**Delete existing resource**:

```http
DELETE /applications/{id}
Authorization: Bearer <token>

Response: 204 No Content
```

**Status Codes**:
- `204 No Content`: Resource successfully deleted
- `404 Not Found`: Resource doesn't exist
- `403 Forbidden`: User lacks permission to delete resource

**Note**: Current implementation performs hard deletes. Soft delete (REQ-004) is planned but not yet implemented.

## 5. Response Structure

### Single Resource Response

All single resource endpoints (GET, POST, PATCH) return the full entity:

```json
{
  "id": "string (uuid)",
  "name": "string",
  "field1": "value",
  "field2": null,
  "tags": [],
  "created_at": "2026-01-06T10:00:00Z",
  "updated_at": "2026-01-06T10:00:00Z"
}
```

**Characteristics**:
- Always includes `id`, `created_at`, `updated_at`
- Nullable fields explicitly show `null` (not omitted)
- Empty arrays shown as `[]`
- Timestamps always in ISO 8601 UTC with 'Z'

### Collection/Paginated Response

All collection endpoints return paginated results:

```json
{
  "items": [
    { "id": "1", "name": "Item 1" },
    { "id": "2", "name": "Item 2" }
  ],
  "page": 1,
  "limit": 50,
  "total": 2
}
```

**Fields**:
- `items`: Array of entities (may be empty)
- `page`: Current page number (1-based)
- `limit`: Page size (number of items per page)
- `total`: Total count of items matching query

**Characteristics**:
- `page` defaults to 1
- `limit` defaults to 50, maximum 200
- `items` is empty array `[]` when no results
- `total` reflects total count across all pages

### Error Response

All error endpoints return structured error information:

```json
{
  "code": "validation_error",
  "message": "Request validation failed",
  "trace_id": "req-5678efgh",
  "errors": [
    {
      "field": "name",
      "message": "Name is required"
    },
    {
      "field": "lifecycle",
      "message": "Invalid lifecycle value. Must be one of: planned, active, deprecated, retired"
    }
  ]
}
```

**Fields**:
- `code`: Application error code (see Error Codes section)
- `message`: Human-readable error description
- `trace_id`: (optional) Request trace ID for debugging
- `errors`: (optional) Array of field-level validation errors

### Empty Response

DELETE operations return no body:

```http
HTTP/1.1 204 No Content
```

## 6. Status Codes

### Success Status Codes

| Code | Name | Usage | Methods |
|------|------|-------|---------|
| 200 | OK | Resource retrieved or updated successfully | GET, PATCH |
| 201 | Created | Resource created successfully | POST |
| 204 | No Content | Resource deleted successfully (no response body) | DELETE |

### Client Error Status Codes

| Code | Name | Usage | Error Code Example |
|------|------|-------|-------------------|
| 400 | Bad Request | Invalid JSON syntax, malformed request | `bad_request` |
| 403 | Forbidden | User lacks permission for this operation | `forbidden` |
| 404 | Not Found | Resource with given ID doesn't exist | `not_found` |
| 422 | Unprocessable Entity | Validation failed (field-level errors) | `validation_error` |

### Server Error Status Codes

| Code | Name | Usage | Error Code Example |
|------|------|-------|-------------------|
| 500 | Internal Server Error | Unexpected server error | `internal_error` |
| 503 | Service Unavailable | Service temporarily unavailable | `service_unavailable` |

### Status Code Selection Guidelines

- Use **200 OK** for successful GET and PATCH operations
- Use **201 Created** for successful POST operations (include Location header)
- Use **204 No Content** for successful DELETE operations (no body)
- Use **400 Bad Request** for syntactically invalid requests (malformed JSON)
- Use **403 Forbidden** for authorization failures (user lacks permission)
- Use **404 Not Found** when resource ID doesn't exist
- Use **422 Unprocessable Entity** for validation failures (field-level errors)
- Use **500 Internal Server Error** for unexpected server errors
- Use **503 Service Unavailable** for temporary service outages

## 7. Headers

### Request Headers

#### Authorization (Required)

Authentication token for API access:

```http
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Alternative**: API key authentication

```http
X-Api-Key: api-key-12345
```

**Requirements**:
- All endpoints require authentication (except health/status endpoints)
- Bearer tokens are OIDC JWT tokens with claims (sub, email, roles, groups)
- API keys are alternative for service-to-service authentication

#### Content-Type (Required for POST/PATCH)

```http
Content-Type: application/json
```

**Requirements**:
- MUST be `application/json` for all POST and PATCH requests
- Server validates Content-Type header

#### X-Correlation-Id (Optional)

Client-provided correlation ID for request tracing:

```http
X-Correlation-Id: client-correlation-12345
```

**Guidelines**:
- Used for distributed tracing across services
- If not provided, server generates trace_id
- Useful for debugging and log correlation

### Response Headers

#### Content-Type

```http
Content-Type: application/json; charset=utf-8
```

**All JSON responses include this header**

#### Location (on 201 Created)

```http
HTTP/1.1 201 Created
Location: /applications/app-123e4567-e89b-12d3-a456-426614174000
```

**Requirements**:
- MUST be included on all 201 Created responses
- Contains full path to newly created resource
- Allows clients to immediately access new resource

#### X-RateLimit-* (Future)

Planned rate limiting headers:

```http
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1704556800
```

**Status**: Not yet implemented (see backlog)

## 8. Pagination

### Query Parameters

All collection endpoints support pagination:

| Parameter | Type | Default | Min | Max | Description |
|-----------|------|---------|-----|-----|-------------|
| `page` | integer | 1 | 1 | - | 1-based page number |
| `limit` | integer | 50 | 1 | 200 | Items per page |

### Example Request

```http
GET /applications?page=2&limit=25
```

### Example Response

```json
{
  "items": [ /* 25 items */ ],
  "page": 2,
  "limit": 25,
  "total": 100
}
```

### Pagination Behavior

- **Default page**: If `page` parameter omitted, defaults to 1
- **Default limit**: If `limit` parameter omitted, defaults to 50
- **Maximum limit**: If `limit` > 200, clamped to 200
- **Invalid page**: If `page` < 1, defaults to 1
- **Empty results**: If page exceeds total pages, returns empty items array with correct total
- **Total count**: `total` field shows total items across all pages

### Calculating Pages

- **Total pages**: `ceil(total / limit)`
- **Has next page**: `page * limit < total`
- **Has previous page**: `page > 1`

### Example Navigation

Given `total: 125`, `limit: 50`:

```
Page 1: GET /applications?page=1&limit=50  → items 1-50
Page 2: GET /applications?page=2&limit=50  → items 51-100
Page 3: GET /applications?page=3&limit=50  → items 101-125
Page 4: GET /applications?page=4&limit=50  → items: []
```

## 9. Filtering & Search

### Search Parameter

Full-text search across common fields:

```http
GET /applications?search=payment
```

**Behavior**:
- Searches across: `name` and common text fields
- Case-insensitive partial match
- Combines with other filters using AND logic

### Field-Specific Filters

Entity-specific query parameters:

```http
GET /applications?owner=team@example.com
GET /applications?lifecycle=active
GET /applications?data_classification=confidential
GET /organizations?parent_id=org-123
GET /servers?environment=prod
```

**Common Filters by Entity**:

**Applications**:
- `owner`: Filter by owner email
- `lifecycle`: Filter by lifecycle (planned, active, deprecated, retired)
- `data_classification`: Filter by classification level
- `capability_id`: Filter by business capability

**Organizations**:
- `parent_id`: Filter by parent organization (hierarchical queries)
- Special value: `parent_id=null` returns root organizations

**Servers**:
- `environment`: Filter by environment (dev, staging, prod)
- `criticality`: Filter by criticality level

**BusinessCapabilities**:
- `parent_id`: Filter by parent capability

### Multi-Value Filters (Future)

Planned support for multiple values:

```http
GET /applications?tags=payment,critical
GET /servers?environment=prod,staging
```

**Status**: Not yet implemented

### Date Range Filters (Future)

Planned temporal filtering:

```http
GET /relations?effective_from_gte=2026-01-01T00:00:00Z
GET /relations?effective_to_lte=2026-12-31T23:59:59Z
```

**Status**: Not yet implemented

### Combining Filters

Multiple filters combine with AND logic:

```http
GET /applications?lifecycle=active&data_classification=confidential&search=payment
```

Returns applications that are:
- Active lifecycle
- AND Confidential classification
- AND Name contains "payment"

## 10. Error Handling

### Error Response Structure

All errors return consistent structure:

```json
{
  "code": "error_code",
  "message": "Human-readable error message",
  "trace_id": "req-1234abcd",
  "errors": [ /* optional field-level errors */ ]
}
```

### Error Codes

| Code | Status | Description | Example |
|------|--------|-------------|---------|
| `bad_request` | 400 | Invalid request syntax | Malformed JSON |
| `validation_error` | 422 | Field validation failed | Missing required field |
| `not_found` | 404 | Resource doesn't exist | Unknown ID |
| `forbidden` | 403 | Access denied | Insufficient permissions |
| `conflict` | 409 | Resource conflict | Duplicate name |
| `internal_error` | 500 | Server error | Database unavailable |

### Validation Error Response

Field-level validation errors:

```json
{
  "code": "validation_error",
  "message": "Request validation failed",
  "trace_id": "req-5678efgh",
  "errors": [
    {
      "field": "name",
      "message": "Name is required and must be 1-255 characters"
    },
    {
      "field": "lifecycle",
      "message": "Invalid lifecycle value. Must be one of: planned, active, deprecated, retired"
    },
    {
      "field": "parent_id",
      "message": "Parent organization not found"
    }
  ]
}
```

**Characteristics**:
- `errors` array contains one object per validation failure
- Each error has `field` (JSON property path) and `message` (description)
- All validation errors returned together (not fail-fast)

### Not Found Error

Resource doesn't exist:

```json
{
  "code": "not_found",
  "message": "Application not found",
  "trace_id": "req-abcd1234"
}
```

### Forbidden Error

Authorization failure:

```json
{
  "code": "forbidden",
  "message": "You do not have access to this resource",
  "trace_id": "req-xyz789"
}
```

### Conflict Error

Resource conflict (duplicate, circular reference):

```json
{
  "code": "conflict",
  "message": "Organization name already exists under this parent",
  "trace_id": "req-conflict123"
}
```

```json
{
  "code": "validation_error",
  "message": "Cannot set parent_id: would create circular reference",
  "trace_id": "req-cycle456"
}
```

### Internal Server Error

Unexpected error:

```json
{
  "code": "internal_error",
  "message": "An unexpected error occurred. Please contact support with trace_id.",
  "trace_id": "req-error999"
}
```

**Note**: Sensitive error details are NOT exposed to clients

## 11. Authentication & Authorization

### Authentication Methods

#### OIDC Bearer Tokens (Primary)

```http
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Token Structure**: JWT with claims
- `sub`: User ID
- `email`: User email
- `roles`: User roles (e.g., `["viewer", "editor"]`)
- `groups`: User groups (e.g., `["payments-team"]`)

**Token Validation**:
- Signature verified against OIDC provider public keys
- Expiration checked (`exp` claim)
- Issuer validated (`iss` claim)

#### API Keys (Service-to-Service)

```http
X-Api-Key: api-key-12345
```

**Use Cases**:
- CI/CD pipelines
- Service-to-service authentication
- Scheduled jobs and automation

### Authorization Model

**Policy Engine**: Open Policy Agent (OPA) with Rego policies

**Authorization Inputs**:
```json
{
  "subject": {
    "sub": "user-123",
    "email": "user@example.com",
    "roles": ["editor"],
    "groups": ["payments-team"]
  },
  "action": "read",
  "resource": {
    "type": "application",
    "id": "app-456",
    "owner": "payments@example.com",
    "data_classification": "confidential"
  },
  "request": {
    "method": "GET",
    "path": "/applications/app-456"
  }
}
```

**Authorization Decision**:
```json
{
  "allow": true,
  "obligations": {
    "redact_fields": [],
    "filters": {}
  },
  "trace_id": "authz-789"
}
```

**Decision Outcomes**:
- `allow: true`: Request permitted
- `allow: false`: Request denied (403 Forbidden)
- Obligations: Field redactions, filters to apply
- Fail-closed: PDP errors result in deny

### Authorization Errors

When authorization fails:

```http
HTTP/1.1 403 Forbidden
Content-Type: application/json

{
  "code": "forbidden",
  "message": "You do not have access to this resource",
  "trace_id": "req-xyz789"
}
```

## 12. Versioning Strategy

### Current Versioning

**API Version**: 1.0.0

**No explicit version in URL**: All endpoints at root path
- `/applications` (not `/v1/applications`)
- `/organizations` (not `/v1/organizations`)

### Future Versioning Strategy

When breaking changes are needed:

**Option 1: URL Path Versioning** (Recommended)
```
/v1/applications  → Version 1
/v2/applications  → Version 2 (breaking changes)
```

**Option 2: Header Versioning**
```http
Accept: application/vnd.eatool.v2+json
```

**Option 3: Query Parameter**
```
/applications?api_version=2
```

### Breaking vs Non-Breaking Changes

**Non-Breaking Changes** (no version bump needed):
- Adding new endpoints
- Adding optional fields to requests
- Adding fields to responses
- Adding query parameters
- Relaxing validation rules

**Breaking Changes** (require version bump):
- Removing endpoints
- Removing fields from responses
- Making optional fields required
- Changing field types
- Changing error response structure
- Changing status codes
- Renaming fields

### Deprecation Policy (Future)

When deprecating APIs:
1. Announce deprecation with 6-month notice
2. Add `Deprecation` and `Sunset` headers to responses
3. Maintain old version for deprecation period
4. Remove after sunset date

```http
Deprecation: true
Sunset: Wed, 01 Jul 2026 00:00:00 GMT
Link: </v2/applications>; rel="successor-version"
```

## 13. API Documentation Endpoints

### GET /API

Returns this API contract specification (human-readable):

```http
GET /API
Accept: text/markdown

Response: 200 OK
Content-Type: text/markdown

# API Contract Specification
...entire markdown document...
```

**Includes**:
- Link to OpenAPI specification: `/OpenApiSpecification`
- Human-readable API documentation
- Examples and patterns

### GET /OpenApiSpecification

Returns OpenAPI 3.0 specification (machine-readable):

```http
GET /OpenApiSpecification
Accept: application/json

Response: 200 OK
Content-Type: application/json

{
  "openapi": "3.0.3",
  "info": {
    "title": "EA Tool API",
    "version": "1.0.0"
  },
  ...
}
```

**Includes**:
- Machine-readable API contract
- Can be used with Swagger UI, Postman, code generators
- Kept in sync with this specification

## 14. Examples

### Example 1: Create Organization with Hierarchy

```http
POST /organizations
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Payments Team",
  "parent_id": "org-enterprise-123",
  "domains": ["payments.example.com"],
  "contacts": ["payments-lead@example.com"]
}

Response: 201 Created
Location: /organizations/org-456

{
  "id": "org-456",
  "name": "Payments Team",
  "parent_id": "org-enterprise-123",
  "domains": ["payments.example.com"],
  "contacts": ["payments-lead@example.com"],
  "created_at": "2026-01-06T10:00:00Z",
  "updated_at": "2026-01-06T10:00:00Z"
}
```

### Example 2: Query with Pagination and Search

```http
GET /applications?page=2&limit=25&search=gateway
Authorization: Bearer <token>

Response: 200 OK

{
  "items": [
    {
      "id": "app-123",
      "name": "Payment Gateway",
      "lifecycle": "active",
      "created_at": "2026-01-01T10:00:00Z",
      "updated_at": "2026-01-05T10:00:00Z"
    }
  ],
  "page": 2,
  "limit": 25,
  "total": 48
}
```

### Example 3: Update with Validation Error

```http
PATCH /applications/app-123
Authorization: Bearer <token>
Content-Type: application/json

{
  "lifecycle": "invalid",
  "owner": ""
}

Response: 422 Unprocessable Entity

{
  "code": "validation_error",
  "message": "Request validation failed",
  "trace_id": "req-error-567",
  "errors": [
    {
      "field": "lifecycle",
      "message": "Invalid lifecycle value. Must be one of: planned, active, deprecated, retired"
    },
    {
      "field": "owner",
      "message": "Owner must be 1-255 characters if provided"
    }
  ]
}
```

### Example 4: Hierarchical Query

```http
GET /organizations?parent_id=org-enterprise-123
Authorization: Bearer <token>

Response: 200 OK

{
  "items": [
    {
      "id": "org-456",
      "name": "Payments Team",
      "parent_id": "org-enterprise-123",
      "created_at": "2026-01-06T10:00:00Z",
      "updated_at": "2026-01-06T10:00:00Z"
    },
    {
      "id": "org-789",
      "name": "Risk Team",
      "parent_id": "org-enterprise-123",
      "created_at": "2026-01-05T10:00:00Z",
      "updated_at": "2026-01-05T10:00:00Z"
    }
  ],
  "page": 1,
  "limit": 50,
  "total": 2
}
```

### Example 5: Delete Resource

```http
DELETE /applications/app-123
Authorization: Bearer <token>

Response: 204 No Content
```

### Example 6: Not Found Error

```http
GET /applications/app-nonexistent
Authorization: Bearer <token>

Response: 404 Not Found

{
  "code": "not_found",
  "message": "Application not found",
  "trace_id": "req-notfound-890"
}
```

## 15. Acceptance Criteria

- **AC-API-001**: Given valid credentials, When calling GET /applications, Then return 200 with paginated response
- **AC-API-002**: Given valid request body, When calling POST /applications, Then return 201 with Location header
- **AC-API-003**: Given invalid field value, When calling POST, Then return 422 with field-level errors
- **AC-API-004**: Given missing required field, When calling POST, Then return 422 with error for that field
- **AC-API-005**: Given non-existent ID, When calling GET /{id}, Then return 404 with not_found error code
- **AC-API-006**: Given no authorization, When calling any endpoint, Then return 403 with forbidden error code
- **AC-API-007**: Given page=2 and limit=25, When calling GET /applications, Then return page 2 with correct pagination metadata
- **AC-API-008**: Given search parameter, When calling GET /applications?search=payment, Then return filtered results
- **AC-API-009**: Given valid ID, When calling DELETE /{id}, Then return 204 No Content
- **AC-API-010**: Given all error responses, When examining structure, Then all include code, message, and optional trace_id

## 16. Implementation Status

### Currently Implemented

- ✅ GET, POST, PATCH, DELETE methods for all entities
- ✅ Pagination with page/limit parameters
- ✅ Search parameter on collection endpoints
- ✅ Field-specific filters (owner, lifecycle, parent_id, environment)
- ✅ Error response structure with code/message
- ✅ 200, 201, 204, 400, 403, 404, 422 status codes
- ✅ Content-Type and Authorization headers
- ✅ ISO 8601 timestamps with Z suffix
- ✅ UUID primary keys

### Partially Implemented

- ⚠️ Location header on 201 Created (not consistently added)
- ⚠️ Trace ID in error responses (not always included)
- ⚠️ Field-level validation errors (some validation missing)

### Not Yet Implemented

- ❌ /API endpoint to serve this specification
- ❌ /OpenApiSpecification endpoint
- ❌ X-Correlation-Id header support
- ❌ X-RateLimit-* headers
- ❌ Multi-value filters
- ❌ Date range filters
- ❌ Soft delete support
- ❌ API versioning in URL path

## 17. Related Specifications

- **[Specification Index](spec-index.md)** - All specifications
- **[Domain Model Overview](spec-schema-domain-overview.md)** - Entity definitions and requirements
- **[Validation Rules](spec-schema-validation.md)** - Field validation and business rules
- **[Authorization Model](spec-process-authorization.md)** *(planned)* - OPA/Rego policies
- **[Authentication](spec-process-authentication.md)** *(planned)* - OIDC integration details
- **[Error Handling Specification](spec-process-error-handling.md)** *(planned, see Item-010)* - Detailed error handling patterns
- **[Query Patterns Specification](spec-process-query-patterns.md)** *(planned, see Item-011)* - Advanced filtering and search

## 18. Machine-Readable Contract

The machine-readable OpenAPI specification is maintained at:

**File**: [../openapi.yaml](../openapi.yaml)

**Usage**:
- Import into Swagger UI for interactive documentation
- Generate client SDKs (TypeScript, Python, Java, etc.)
- Validate API requests/responses
- Generate mock servers for testing

**Keeping in Sync**:
- This specification (spec-tool-api-contract.md) is human-readable narrative
- openapi.yaml is machine-readable contract
- Both should stay synchronized
- Changes to API require updates to both documents

## 19. Document History

| Date | Version | Changes | Author |
|------|---------|---------|--------|
| 2026-01-06 | 1.0 | Initial creation with comprehensive API contract documentation | EA Platform Team |

---

**Status**: Living Document  
**Maintained By**: EA Platform Team  
**Review Frequency**: Quarterly or when API changes
