---
title: UI to API Integration Specification
version: 1.0
date_created: 2026-01-17
owner: EA Platform Team
tags: [ui, api, integration, frontend]
---

# UI to API Integration Specification

## 1. Purpose & Scope

This specification defines how the UI (frontend) integrates with the backend API using a **CQRS (Command Query Responsibility Segregation)** pattern. It covers:
- **Queries**: Read-only data retrieval (GET endpoints)
- **Commands**: Write operations (POST, DELETE endpoints with specific command routing)
- Request/response patterns, error handling, caching strategies, and real-time updates
- Per-entity command endpoint routing and approval requirements

**Key Principle**: Queries and Commands are separated. Queries fetch data; Commands dispatch specific business operations.

## 2. API Base Configuration

### Endpoints
```typescript
// Development
BASE_URL = "http://localhost:8000"
API_BASE = `${BASE_URL}/api`

// Production
BASE_URL = "https://api.example.com"
API_BASE = `${BASE_URL}/api`

// Staging
BASE_URL = "https://staging.api.example.com"
API_BASE = `${BASE_URL}/api`
```

### Headers
All requests must include:
```typescript
{
  "Content-Type": "application/json",
  "Authorization": `Bearer ${accessToken}`,
  "X-Request-ID": generateRequestId(),
  "X-Tenant-ID": tenantId,
  "Accept": "application/json"
}
```

## 3. Entity List Endpoint Pattern

### Request
```typescript
GET /api/{entities}?skip=0&take=20&sort=name&filter=environment:prod

Query Parameters:
- skip: Number of items to skip (pagination)
- take: Number of items to return (default 20, max 100)
- sort: Sort column + direction (e.g., "name:asc", "modified:desc")
- filter: Key:value filter pairs (repeatable)
- search: Full-text search term
- select: Specific fields to include (for optimization)
```

### Response (Success - 200)
```json
{
  "data": [
    {
      "id": "app-001",
      "name": "My Application",
      "description": "...",
      "owner": "user@example.com",
      "status": "active",
      "environment": "production",
      "created": "2026-01-17T10:00:00Z",
      "modified": "2026-01-17T11:00:00Z",
      "_links": {
        "self": { "href": "/api/applications/app-001" },
        "edit": { "href": "/api/applications/app-001" },
        "delete": { "href": "/api/applications/app-001", "method": "DELETE" }
      }
    }
  ],
  "pagination": {
    "skip": 0,
    "take": 20,
    "total": 150,
    "page": 1,
    "pages": 8,
    "hasMore": true
  },
  "filters": {
    "environment": ["production", "staging", "development", "test"],
    "status": ["active", "inactive", "deprecated"]
  },
  "timestamp": "2026-01-17T12:30:00Z"
}
```

### Response (Error - 400/500)
```json
{
  "error": {
    "code": "INVALID_REQUEST",
    "message": "Invalid skip parameter",
    "details": {
      "field": "skip",
      "expected": "number >= 0",
      "provided": "invalid"
    },
    "traceId": "trace-123456"
  }
}
```

## 4. Entity Detail Endpoint Pattern

### Request
```typescript
GET /api/{entity}/{id}
GET /api/applications/app-001
```

### Response (Success - 200)
```json
{
  "data": {
    "id": "app-001",
    "name": "My Application",
    "description": "...",
    "owner": "user@example.com",
    "status": "active",
    "environment": "production",
    "created": "2026-01-17T10:00:00Z",
    "modified": "2026-01-17T11:00:00Z",
    "properties": {
      "technology": ["Node.js", "React", "PostgreSQL"],
      "criticality": "high",
      "sla": "99.99%"
    },
    "_embedded": {
      "integrations": [
        { "id": "int-001", "name": "Integration 1" }
      ],
      "servers": [
        { "id": "srv-001", "name": "Server 1" }
      ]
    },
    "_links": {
      "self": { "href": "/api/applications/app-001" },
      "integrations": { "href": "/api/applications/app-001/integrations" },
      "edit": { "href": "/api/applications/app-001" }
    }
  },
  "timestamp": "2026-01-17T12:30:00Z"
}
```

### Response (Not Found - 404)
```json
{
  "error": {
    "code": "NOT_FOUND",
    "message": "Application not found",
    "id": "app-001",
    "traceId": "trace-123456"
  }
}
```

## 5. Create Entity Endpoint Pattern

### Request
```typescript
POST /api/{entities}
Content-Type: application/json

{
  "name": "New Application",
  "description": "Application description",
  "owner": "user@example.com",
  "status": "active",
  "environment": "production"
}
```

### Response (Success - 201)
```json
{
  "data": {
    "id": "app-new-001",
    "name": "New Application",
    "description": "Application description",
    "owner": "user@example.com",
    "status": "active",
    "environment": "production",
    "created": "2026-01-17T12:30:00Z",
    "modified": "2026-01-17T12:30:00Z"
  },
  "_links": {
    "self": { "href": "/api/applications/app-new-001" }
  },
  "timestamp": "2026-01-17T12:30:00Z"
}
```

### Response (Validation Error - 400)
```json
{
  "error": {
    "code": "VALIDATION_FAILED",
    "message": "Validation failed for request",
    "validationErrors": [
      {
        "field": "name",
        "message": "Name is required",
        "code": "REQUIRED_FIELD"
      },
      {
        "field": "owner",
        "message": "Owner must be a valid user",
        "code": "INVALID_VALUE"
      }
    ],
    "traceId": "trace-123456"
  }
}
```

## 6. Update Entity Endpoint Pattern (CQRS - Commands)

### Command Routing by Entity Type

**Applications:** Specific commands with business rule enforcement
```typescript
// Set Classification (with justification)
POST /api/applications/{id}/commands/set-classification
{
  "classification": "confidential",
  "reason": "Contains customer financial data"
}

// Transition Lifecycle (state machine)
POST /api/applications/{id}/commands/transition-lifecycle
{
  "target_lifecycle": "deprecated",
  "sunset_date": "2026-06-01"
}

// Set Owner
POST /api/applications/{id}/commands/set-owner
{
  "owner_id": "user-456"
}

// For other fields (name, description, version, type, status, url), use legacy PATCH
PATCH /api/applications/{id}
{
  "name": "Updated Name",
  "description": "New description"
}
```

**BusinessCapabilities & Organizations:** Parent relationship commands
```typescript
// Set Parent (establishes hierarchy)
POST /api/business-capabilities/{id}/commands/set-parent
{
  "parent_id": "bc-parent-001"
}

// Remove Parent
POST /api/business-capabilities/{id}/commands/remove-parent
{}

// Update Description (BusinessCapabilities only)
POST /api/business-capabilities/{id}/commands/update-description
{
  "description": "Updated description"
}

// For other fields, use legacy PATCH
PATCH /api/business-capabilities/{id}
{
  "name": "Updated Name",
  "level": "level-2"
}
```

**Servers, Integrations, DataEntities, Relations, ApplicationServices, ApplicationInterfaces:** Generic PATCH (no specific commands defined)
```typescript
PATCH /api/servers/{id}
{
  "name": "Updated Server Name",
  "hostname": "new-host.example.com"
}
```

### Response (Success - 200)
```json
{
  "data": {
    "id": "app-001",
    "name": "Application Name",
    "status": "active",
    "classification": "confidential",
    "lifecycle": "deprecated",
    "modified": "2026-01-17T13:00:00Z"
  },
  "timestamp": "2026-01-17T13:00:00Z"
}
```

### Response (Validation Error - 422)
Commands may return validation errors. Map to form fields for display.
```json
{
  "error": {
    "code": "VALIDATION_FAILED",
    "message": "Validation failed",
    "validationErrors": [
      {
        "field": "classification",
        "message": "Invalid classification value",
        "code": "INVALID_VALUE"
      }
    ],
    "traceId": "trace-123456"
  }
}
```

### Response (Authorization Error - 403)
```json
{
  "error": {
    "code": "FORBIDDEN",
    "message": "You do not have permission to transition lifecycle for this entity",
    "traceId": "trace-123456"
  }
}
```

## 7. Delete Entity Endpoint Pattern (CQRS - Commands)

### Request (with Approval)
All deletes require approval context for audit and authorization:
```typescript
DELETE /api/{entity}/{id}?approval_id={approval_id}&reason={reason}

Examples:
DELETE /api/applications/app-001?approval_id=app-123&reason=No longer in use
DELETE /api/servers/srv-001?approval_id=app-456&reason=Decommissioned infrastructure
```

**Required Query Parameters:**
- `approval_id`: ID of the approver or approval context (for audit trail)
- `reason`: User-provided reason for deletion (for compliance)

### Response (Success - 204)
```
No content
```

### Response (Missing Approval - 400)
```json
{
  "error": {
    "code": "MISSING_REQUIRED_PARAMETER",
    "message": "approval_id and reason are required for delete operations",
    "missingParams": ["approval_id", "reason"],
    "traceId": "trace-123456"
  }
}
```

### Response (Conflict - 409)
```json
{
  "error": {
    "code": "ENTITY_HAS_REFERENCES",
    "message": "Cannot delete: entity has active relationships",
    "references": {
      "integrations": 5,
      "servers": 2
    },
    "traceId": "trace-123456"
  }
}
```

### Response (Authorization Error - 403)
```json
{
  "error": {
    "code": "FORBIDDEN",
    "message": "You do not have permission to delete this entity",
    "traceId": "trace-123456"
  }
}
```

## 8. Bulk Operations

### Request
```typescript
POST /api/{entities}/bulk-action
Content-Type: application/json

{
  "action": "delete",
  "ids": ["app-001", "app-002", "app-003"]
}
```

### Response (Success - 200)
```json
{
  "data": {
    "action": "delete",
    "succeeded": 3,
    "failed": 0,
    "total": 3,
    "results": [
      { "id": "app-001", "status": "deleted" },
      { "id": "app-002", "status": "deleted" },
      { "id": "app-003", "status": "deleted" }
    ]
  },
  "timestamp": "2026-01-17T13:00:00Z"
}
```

## 9. Search Endpoint

### Request
```typescript
GET /api/search?q=term&types=applications,servers,integrations&limit=50

Query Parameters:
- q: Search term (required)
- types: Entity types to search (comma-separated)
- limit: Max results per type (default 20, max 100)
```

### Response (Success - 200)
```json
{
  "data": [
    {
      "type": "application",
      "id": "app-001",
      "name": "My Application",
      "score": 0.95
    },
    {
      "type": "server",
      "id": "srv-001",
      "name": "Application Server",
      "score": 0.85
    }
  ],
  "total": 2,
  "timestamp": "2026-01-17T13:00:00Z"
}
```

## 10. Error Handling

### Standard Error Codes
```typescript
enum ErrorCode {
  // Client errors (4xx)
  INVALID_REQUEST = "INVALID_REQUEST",      // 400
  UNAUTHORIZED = "UNAUTHORIZED",            // 401
  FORBIDDEN = "FORBIDDEN",                  // 403
  NOT_FOUND = "NOT_FOUND",                  // 404
  CONFLICT = "CONFLICT",                    // 409
  VALIDATION_FAILED = "VALIDATION_FAILED",  // 422
  
  // Server errors (5xx)
  INTERNAL_ERROR = "INTERNAL_ERROR",        // 500
  SERVICE_UNAVAILABLE = "SERVICE_UNAVAILABLE" // 503
}
```

### UI Error Handling Strategy
```typescript
// In UI component

function handleApiError(error: ApiError) {
  switch (error.code) {
    case "VALIDATION_FAILED":
      // Display field errors in form
      displayFormErrors(error.validationErrors);
      break;
      
    case "NOT_FOUND":
      // Redirect to list view
      navigate(`/${error.resource}s`);
      break;
      
    case "CONFLICT":
      // Show conflict resolution modal
      showConflictModal(error.details);
      break;
      
    case "UNAUTHORIZED":
      // Redirect to login
      redirectToLogin();
      break;
      
    case "FORBIDDEN":
      // Show permission error
      showError("You don't have permission to perform this action");
      break;
      
    case "INTERNAL_ERROR":
      // Show generic error with trace ID
      showError(`Server error: ${error.traceId}`);
      break;
  }
}
```

## 11. Request/Response Caching

### Cache Strategy
```typescript
// Cache settings per entity type
const cacheConfig = {
  applications: {
    ttl: 5 * 60 * 1000,  // 5 minutes
    invalidateOn: ["CREATE", "UPDATE", "DELETE"]
  },
  servers: {
    ttl: 10 * 60 * 1000, // 10 minutes
    invalidateOn: ["CREATE", "UPDATE", "DELETE"]
  },
  search: {
    ttl: 2 * 60 * 1000,  // 2 minutes
    invalidateOn: ["CREATE", "UPDATE", "DELETE"]
  }
};

// Optimistic updates
function updateEntity(id: string, data: any) {
  // Update cache immediately
  updateCache(id, data);
  
  // Send to server
  api.patch(`/api/entities/${id}`, data)
    .catch(error => {
      // Revert cache on error
      revertCache(id);
      showError(error.message);
    });
}
```

## 12. Real-time Updates (WebSocket)

### Connection
```typescript
const socket = new WebSocket(`ws://localhost:8000/api/subscribe`);

// Authentication
socket.send(JSON.stringify({
  type: "authenticate",
  token: accessToken
}));
```

### Message Format
```json
{
  "type": "entity-updated",
  "entity": "application",
  "action": "updated",
  "id": "app-001",
  "data": {
    "id": "app-001",
    "name": "Updated Name",
    "modified": "2026-01-17T13:00:00Z"
  }
}
```

### Subscription
```typescript
socket.send(JSON.stringify({
  type: "subscribe",
  entities: ["applications", "servers"],
  actions: ["created", "updated", "deleted"]
}));
```

## 13. Pagination Pattern

### Query Parameters
```typescript
// Page-based pagination
GET /api/applications?page=2&pageSize=20

// Offset-based pagination
GET /api/applications?skip=20&take=20

// Cursor-based pagination
GET /api/applications?cursor=app-020&direction=forward&limit=20
```

### Response
```json
{
  "data": [...],
  "pagination": {
    "current": 2,
    "pageSize": 20,
    "total": 1500,
    "pages": 75,
    "hasNext": true,
    "hasPrev": true,
    "nextCursor": "app-040",
    "prevCursor": "app-001"
  }
}
```

## 14. Filtering & Sorting

### Filter Pattern
```typescript
// Single filter
GET /api/applications?filter[environment]=production

// Multiple filters
GET /api/applications?filter[environment]=production&filter[status]=active

// Range filters
GET /api/applications?filter[created][gte]=2026-01-01&filter[created][lte]=2026-01-31

// Operators: eq, neq, gt, gte, lt, lte, in, nin, contains, startsWith, endsWith
```

### Sort Pattern
```typescript
// Single sort
GET /api/applications?sort=name:asc

// Multiple sorts (priority order)
GET /api/applications?sort=environment:asc,modified:desc
```

## 15. Validation Criteria

API integration must support:
- [ ] Proper request/response formatting
- [ ] Consistent error handling
- [ ] Pagination for large datasets
- [ ] Filtering and sorting
- [ ] Optimistic UI updates
- [ ] Request caching
- [ ] Real-time updates via WebSocket
- [ ] Proper authentication/authorization
- [ ] Rate limiting handling
- [ ] Retry logic for failed requests

## 16. Related Specifications

- [spec-tool-api-contract.md](spec-tool-api-contract.md) - Backend API contract
- [spec-design-ui-architecture.md](spec-design-ui-architecture.md) - UI design patterns
- [spec-tool-error-handling.md](spec-tool-error-handling.md) - Error handling strategies
