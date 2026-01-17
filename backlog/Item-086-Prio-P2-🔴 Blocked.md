# Item-086: Implement Advanced Authorization with OPA/Rego

**Status:** � Blocked  
**Priority:** P2 - MEDIUM  
**Effort:** 40-56 hours  
**Created:** 2026-01-17  
**Owner:** Backend Team

---

## Problem Statement

The current RBAC (Role-Based Access Control) system provides basic authentication and permission checks, but enterprise environments require more sophisticated authorization including attribute-based access control (ABAC), field-level redaction, dynamic permissions, and centralized policy management.

Open Policy Agent (OPA) with Rego policies provides a flexible, centralized way to define and enforce complex authorization rules as specified in [spec-process-authorization.md](../spec/spec-process-authorization.md).

This is a Phase 2 advanced feature identified in the FRONTEND-READINESS-REPORT.md.

---

## Affected Files

**Backend:**
- `src/EATool/Authorization/OpaClient.fs` - OPA HTTP client
- `src/EATool/Authorization/PolicyEvaluator.fs` - Evaluate OPA policies
- `src/EATool/Authorization/PolicyTypes.fs` - Authorization types
- `src/EATool/Authorization/FieldRedaction.fs` - Field-level redaction
- `src/EATool/Middleware/OpaAuthorizationMiddleware.fs` - Authorization middleware
- `policies/applications.rego` - Application entity policies
- `policies/servers.rego` - Server entity policies
- `policies/common.rego` - Shared policy functions
- `policies/rbac.rego` - RBAC rules
- `docker-compose.yml` - Add OPA service

**Frontend:**
- `frontend/src/utils/fieldRedaction.ts` - Handle redacted fields in UI
- `frontend/src/components/entity/RedactedField.tsx` - Display redacted fields

---

## Specifications

- [spec/spec-process-authorization.md](../spec/spec-process-authorization.md) - Authorization process specification
- [spec/spec-ui-auth-permissions.md](../spec/spec-ui-auth-permissions.md) - UI permission requirements
- [FRONTEND-READINESS-REPORT.md](../FRONTEND-READINESS-REPORT.md) - Phase 2 feature identification

---

## Detailed Tasks

### Phase 1: OPA Infrastructure Setup (8-10 hours)

**OPA Installation:**
- [ ] Add OPA service to docker-compose.yml
- [ ] Configure OPA HTTP API (default port 8181)
- [ ] Mount policy files directory (/policies)
- [ ] Add OPA health check
- [ ] Configure OPA logging

**OPA Client:**
- [ ] Create OpaClient in F# to communicate with OPA service
- [ ] Implement policy evaluation API call (POST /v1/data/eatool/allow)
- [ ] Implement field redaction query (POST /v1/data/eatool/redact)
- [ ] Add error handling for OPA unavailable
- [ ] Add caching for policy evaluation results (optional, with TTL)
- [ ] Add fallback to basic RBAC if OPA unavailable

### Phase 2: Rego Policy Development (12-16 hours)

**Common Policies (policies/common.rego):**
- [ ] Define user context structure (user_id, roles, permissions, attributes)
- [ ] Define resource structure (type, id, owner, sensitivity)
- [ ] Create role helper functions (is_admin, is_editor, is_viewer)
- [ ] Create permission helper functions (has_permission)
- [ ] Create attribute functions (same_department, same_organization)

**RBAC Policies (policies/rbac.rego):**
- [ ] Implement role-based access rules
- [ ] VIEWER: read-only access to public entities
- [ ] EDITOR: read/write access to owned entities
- [ ] ADMIN: full access to all entities
- [ ] VIEWER_LIMITED: read-only access to specific department entities

**Application Policies (policies/applications.rego):**
- [ ] Allow read if user has app:read permission
- [ ] Allow create if user has app:create permission
- [ ] Allow update if user has app:update permission AND (is owner OR is admin)
- [ ] Allow delete if user has app:delete permission AND (is owner OR is admin)
- [ ] Redact sensitive fields (API keys, passwords) for non-admins
- [ ] Redact owner details if different department

**Server Policies (policies/servers.rego):**
- [ ] Allow read if user has server:read permission
- [ ] Allow create if user has server:create permission AND same environment
- [ ] Allow update if user has server:update permission AND (is owner OR is admin)
- [ ] Allow delete if user has server:delete permission AND is admin (prevent accidental deletion)
- [ ] Redact IP addresses for VIEWER_LIMITED role
- [ ] Redact credentials for non-admins

**Integration Policies (policies/integrations.rego):**
- [ ] Similar pattern for integrations
- [ ] Redact connection strings, API tokens

**General Pattern:**
- [ ] Create policies for all 9 entity types
- [ ] Consistent structure across all policies
- [ ] Test policies with OPA REPL

### Phase 3: Backend Integration (10-14 hours)

**Authorization Middleware:**
- [ ] Create OpaAuthorizationMiddleware
- [ ] Extract user context from JWT token
- [ ] Extract resource context from request (entity type, entity ID)
- [ ] Call OPA to evaluate policy (allow/deny)
- [ ] Return 403 FORBIDDEN if denied
- [ ] Apply middleware to all entity endpoints

**Field Redaction:**
- [ ] After entity fetched, call OPA for field redaction policy
- [ ] OPA returns list of fields to redact for this user
- [ ] Replace redacted field values with "[REDACTED]" or null
- [ ] Return redacted entity to client
- [ ] Log redaction events (audit trail)

**Dynamic Permissions:**
- [ ] Move permission checks from code to OPA policies
- [ ] Replace hard-coded permission checks with OPA calls
- [ ] Support attribute-based access control (ABAC)
- [ ] Example: User can only access applications in their department

### Phase 4: Frontend Integration (6-8 hours)

**Redacted Field Handling:**
- [ ] Detect "[REDACTED]" values in API responses
- [ ] Display redacted fields with lock icon and tooltip
- [ ] Hide redacted fields from edit forms
- [ ] Show "Insufficient permissions" message on hover

**Permission-Based UI:**
- [ ] Query backend for user permissions on page load (GET /auth/permissions)
- [ ] Hide/disable UI elements based on permissions
- [ ] Show permission denied messages when actions unavailable
- [ ] Update UI dynamically if permissions change

### Phase 5: Testing & Validation (8-12 hours)

**Policy Testing:**
- [ ] Write OPA policy unit tests (test/*.rego)
- [ ] Test all role combinations (ADMIN, EDITOR, VIEWER, VIEWER_LIMITED)
- [ ] Test attribute-based rules (same_department, same_organization)
- [ ] Test field redaction for each entity type
- [ ] Test edge cases (no owner, null values)

**Integration Testing:**
- [ ] Test authorization on all entity endpoints
- [ ] Test with different user roles
- [ ] Test field redaction in API responses
- [ ] Test 403 responses for unauthorized actions
- [ ] Test fallback to RBAC if OPA unavailable

**Frontend Testing:**
- [ ] Test redacted field display
- [ ] Test permission-based UI hiding/disabling
- [ ] Test with different user roles
- [ ] Test graceful handling of denied actions

---

## Acceptance Criteria

**OPA Infrastructure:**
- [ ] OPA service running and accessible (http://localhost:8181)
- [ ] Policies loaded and active
- [ ] OPA health check passing
- [ ] Backend can communicate with OPA

**Policies:**
- [ ] All 9 entity types have Rego policies
- [ ] RBAC rules implemented (ADMIN, EDITOR, VIEWER, VIEWER_LIMITED)
- [ ] ABAC rules implemented (department, organization)
- [ ] Field redaction rules defined for sensitive fields
- [ ] Policies tested with OPA REPL and unit tests

**Backend Integration:**
- [ ] All entity endpoints check authorization via OPA
- [ ] 403 FORBIDDEN returned for unauthorized actions
- [ ] Field redaction applied to API responses
- [ ] User context correctly passed to OPA
- [ ] Fallback to basic RBAC if OPA unavailable

**Frontend Integration:**
- [ ] Redacted fields displayed with lock icon
- [ ] UI elements hidden/disabled based on permissions
- [ ] Permission denied messages shown
- [ ] Graceful handling of 403 responses

**Security:**
- [ ] No sensitive data leaked to unauthorized users
- [ ] Field redaction cannot be bypassed
- [ ] Authorization decisions logged (audit trail)
- [ ] OPA policy changes reflected immediately

**Performance:**
- [ ] OPA authorization adds <50ms latency to API calls
- [ ] Policy evaluation results cached (if implemented)
- [ ] No noticeable impact on page load times

---

## Dependencies

**Blocked by:**  
- Item-084 (Authentication endpoints - need JWT for user context)

**Blocks:** None (enhances existing authorization)

---

## Notes

- **Phase 2 Feature:** Can be implemented after MVP Phase 1
- OPA is industry-standard for policy-as-code
- Rego has a learning curve, provide examples and documentation
- Consider using OPA bundles for policy distribution in production
- Add OPA policy versioning and rollback capability
- Test performance with complex policies (nested rules)
- Consider caching policy evaluation results (cache key: user + resource + action)
- Add OPA policy linting (opa check, opa test)
- Document policy authoring guidelines for team
- Consider using OPA decision logs for audit trail
- Add OPA monitoring (policy evaluation metrics)
- Test with large user base (1000+ users)
- Consider partial evaluation for performance (compile policies)
