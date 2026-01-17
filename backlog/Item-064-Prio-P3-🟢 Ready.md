# Item-064: API Authentication & Authorization Framework

**Status:** 🟢 Ready  
**Priority:** P3 - LOW  
**Effort:** 8-10 hours  
**Created:** 2026-01-07  
**Owner:** TBD

---

## Problem Statement

The specification requires authentication and authorization (REQ-007, REQ-008) for all API endpoints, but currently endpoints are unprotected. This creates:

- Security vulnerabilities (anyone can access/modify data)
- Non-compliance with enterprise security standards
- Lack of audit trails (can't track who made changes)
- No access control (can't restrict by role)
- No authentication for service-to-service calls

Current gaps:
- No authentication mechanism implemented
- No authorization/role-based access control (RBAC)
- No audit trail linking changes to users
- Service accounts not supported for inter-service auth
- No token validation on protected endpoints

---

## Affected Files

**Authentication:**
- Create `src/Api/Auth/AuthenticationMiddleware.fs` - Token validation
- Create `src/Api/Auth/AuthContext.fs` - Authenticated user context
- Create `src/Api/Auth/TokenValidator.fs` - JWT/token validation
- `src/Api/Program.fs` - Register auth middleware

**Authorization:**
- Create `src/Api/Auth/AuthorizationPolicies.fs` - Define RBAC policies
- Create `src/Api/Auth/RoleBasedAccess.fs` - Role-based access control
- Create `src/Api/Handlers/*Handlers.fs` - Add authorization checks

**Domain:**
- Create `src/Domain/Security/Principal.fs` - User principal type
- Create `src/Domain/Security/Role.fs` - Role definitions
- Create `src/Domain/Security/Permission.fs` - Permission model

**API:**
- Create `src/Api/Attributes/AuthorizeAttribute.fs` - Decorate endpoints requiring auth
- `openapi.yaml` - Add security schemes

**Tests:**
- Create `tests/AuthenticationTests.fs` - Test token validation
- Create `tests/AuthorizationTests.fs` - Test access control
- Update command tests to include authentication context

---

## Specifications

- [spec/spec-tool-api-contract.md](../spec/spec-tool-api-contract.md) - REQ-007 & REQ-008: Auth requirements
- [openapi.yaml](../openapi.yaml) - Add security schemes

---

## Detailed Tasks

- [ ] **Define Principal types**:
  - `src/Domain/Security/Principal.fs`
  - Type Principal:
    ```fsharp
    {
      id: UserId
      name: string
      email: string
      roles: Role list
      organization_id: OrganizationId
      is_service_account: bool
      last_login: DateTime option
    }
    ```
  - Type UserId = | UserId of Guid
  - Service principal for inter-service auth

- [ ] **Define role-based access control**:
  - `src/Domain/Security/Role.fs`
  - Roles:
    - Admin - Full access, can manage users and roles
    - DataArchitect - Can manage schema, entities, relationships
    - DataGovernance - Can manage classifications, ownership
    - Viewer - Read-only access to entities
    - ServiceAccount - Can access specific endpoints only
  - Role hierarchy for permission inheritance

- [ ] **Define permissions**:
  - `src/Domain/Security/Permission.fs`
  - Permissions per entity:
    - Create, Read, Update, Delete (CRUD)
    - ViewMetadata, ViewLineage
    - ManageOwnership, ManageClassification
  - Assign permissions to roles
  - Check permissions in handlers

- [ ] **Authentication middleware**:
  - `src/Api/Auth/AuthenticationMiddleware.fs`
  - Extract authentication token from:
    - Authorization: Bearer {token} header
    - X-API-Key header (service accounts)
  - Validate token format and signature
  - Decode principal from token
  - Store in HttpContext.Items for handlers
  - Return 401 Unauthorized if invalid/expired

- [ ] **Token validation**:
  - `src/Api/Auth/TokenValidator.fs`
  - Support JWT tokens
  - Verify token signature using issuer's public key
  - Check token expiration
  - Extract claims: sub (user ID), org (organization ID), roles
  - Cache validated tokens briefly
  - Support refresh tokens for long sessions

- [ ] **Authorization checks**:
  - `src/Api/Auth/RoleBasedAccess.fs`
  - Check role-based permissions
  - Check organization-based access (users only see their org)
  - Check resource ownership (users only modify their resources)
  - DataArchitect can't delete (only soft delete)
  - Admin can perform any action

- [ ] **Decorate protected endpoints**:
  - `src/Api/Attributes/AuthorizeAttribute.fs`
  - [Authorize] - Requires authentication
  - [Authorize("Admin")] - Requires Admin role
  - [Authorize("DataArchitect,DataGovernance")] - Multiple roles
  - Update all command handlers with [Authorize] attributes

- [ ] **Authentication context**:
  - `src/Api/Auth/AuthContext.fs`
  - Type AuthContext:
    ```fsharp
    {
      principal: Principal
      organization_id: OrganizationId
      request_id: string
      timestamp: DateTime
    }
    ```
  - Extract from HttpContext in all handlers
  - Validate user belongs to organization

- [ ] **Audit trail integration**:
  - Include principal ID in event metadata
  - Track who created/updated each entity
  - Link events to user in audit log
  - AuditLog should include user ID and name

- [ ] **API security in OpenAPI**:
  - Add security scheme for JWT:
    ```yaml
    components:
      securitySchemes:
        bearerAuth:
          type: http
          scheme: bearer
          bearerFormat: JWT
    ```
  - Add API Key scheme for service accounts:
    ```yaml
    components:
      securitySchemes:
        apiKey:
          type: apiKey
          in: header
          name: X-API-Key
    ```
  - Mark all protected endpoints with security requirement
  - Document required roles per endpoint

- [ ] **Token management**:
  - Document how to obtain tokens (authentication service)
  - Document token format and claims
  - Support token refresh
  - Support token revocation
  - Service account token provisioning

- [ ] **Test coverage**:
  - Missing token: returns 401
  - Invalid token: returns 401
  - Expired token: returns 401
  - Valid token: succeeds
  - Insufficient role: returns 403
  - Correct role: succeeds
  - User from different org: forbidden
  - Service account token works
  - Request ID tracked with auth context
  - Event metadata includes principal ID

- [ ] **Security documentation**:
  - `docs/authentication.md` - How to authenticate
  - Document token format and lifetime
  - Document roles and permissions
  - Provide client code examples
  - Security best practices (HTTPS only, token storage, etc.)

- [ ] **Local development support**:
  - Create test token generator for development
  - Support unauthenticated mode for local testing
  - Document how to set up test tokens

---

## Acceptance Criteria

- [ ] Principal, Role, Permission types created
- [ ] AuthenticationMiddleware validates tokens
- [ ] [Authorize] attribute enforces auth on endpoints
- [ ] All protected endpoints require valid token
- [ ] RBAC implemented with role-based access
- [ ] User can only access own organization's data
- [ ] DataArchitect can't perform deletes
- [ ] Admin has full access
- [ ] Service account authentication works
- [ ] Audit trail includes principal ID
- [ ] OpenAPI spec includes security schemes
- [ ] All tests pass (auth and authz tests)
- [ ] Unauthenticated requests return 401
- [ ] Insufficient role returns 403
- [ ] Build succeeds with 0 errors, 0 warnings

---

## Dependencies

**Blocks:**
- None (but critical for production)

**Depends On:**
- None

**Related:**
- Item-063 - Error responses (auth failures as 401/403)
- Item-049 - OTel integration (auth events in traces)
- Item-050 - Structured logging (auth failures logged)
- Item-036 - AuditLog (track who changed what)

---

## Notes

Authentication and authorization are critical security components. This should be one of the first items implemented for production systems. Consider:
- Using industry-standard JWT tokens
- Supporting OpenID Connect for enterprise integration
- Implementing API keys for service-to-service communication
- Rate limiting by user/principal
- Audit logging of all privileged operations
- Regular token rotation
- Secure token storage (client-side)

This item should be prioritized before public API deployment.
