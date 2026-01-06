# Item-007: Create Authorization Model Specification

**Status:** 🟢 Ready  
**Priority:** P1 - HIGH  
**Effort:** 4-6 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

Authorization using OPA/Rego policies is mentioned throughout the system (openapi.yaml, docs, code), but lacks a formal specification document explaining:
- How OPA decision flow works
- Policy examples (RBAC, ABAC patterns)
- Decision outputs and obligations
- Failure handling

**Impact:** Developers cannot understand or implement authorization policies without external references.

---

## Affected Files

**Create:** [spec/spec-process-authorization.md](../../spec/spec-process-authorization.md) (new file)

**Reference:**
- [openapi.yaml](../../openapi.yaml#L14-L22) - Authorization description
- [docs/system-overview.md](../../docs/system-overview.md#L48-L64) - Mentions OPA

---

## Detailed Tasks

- [ ] Create spec-process-authorization.md following specification template
- [ ] Document authorization architecture:
  - OPA as Policy Decision Point
  - Input structure (subject, action, resource, context)
  - Decision output (allow, deny, obligations)
  - Failure handling (fail-closed)

- [ ] Document RBAC pattern:
  - Role definition
  - Role assignment
  - Permission checking
  - Example Rego policy

- [ ] Document ABAC pattern:
  - Attribute examples (team, department, environment, tags)
  - Attribute-based rules
  - Example Rego policy

- [ ] Document policy input structure:
  - subject (sub, email, roles, groups, claims)
  - action (read, write, delete)
  - resource (type, id, owner, environment, tags)
  - request context (method, path, scopes, tenant, ip)

- [ ] Document policy output:
  - allow/deny decision
  - obligations (field redactions, filters)
  - error handling

- [ ] Document common patterns:
  - Owner-based access
  - Role-based access
  - Data classification-based access
  - Time-based access

- [ ] Document Rego policy examples:
  - Allow read if viewer role
  - Allow write if owner
  - Deny write to deprecated resources
  - Apply field filters for sensitive data

- [ ] Document policy testing:
  - How to test policies
  - Test data structure
  - Assertions

---

## Acceptance Criteria

- [ ] spec-process-authorization.md created
- [ ] Authorization architecture documented
- [ ] RBAC patterns documented with examples
- [ ] ABAC patterns documented with examples
- [ ] Policy input structure documented
- [ ] Policy output structure documented
- [ ] Example Rego policies included
- [ ] Testing approach documented
- [ ] Linked from spec-index.md

---

## Key Sections

```markdown
# Authorization Model Specification

## 1. Purpose & Scope

## 2. Authorization Architecture

### Decision Flow
1. Client makes authenticated request
2. API extracts authorization context
3. Query OPA with (subject, action, resource, context)
4. OPA evaluates policies
5. Return allow/deny + obligations
6. API enforces decision

### Fail-Closed
- If OPA unavailable: deny access
- If OPA error: deny access
- On timeout: deny access

## 3. RBAC Pattern

Roles: viewer, editor, admin, owner
Rules: Define permissions per role

## 4. ABAC Pattern

Attributes: team, department, environment, classification
Rules: Combine attributes for decisions

## 5. Policy Input Structure

```json
{
  "subject": {
    "sub": "user-id",
    "email": "user@example.com",
    "roles": ["viewer", "team-lead"],
    "groups": ["payments-team"]
  },
  "action": "read",
  "resource": {
    "type": "application",
    "id": "app-123",
    "owner": "payments-team",
    "environment": "prod",
    "tags": ["payments", "pci"]
  }
}
```

## 6. Rego Policy Examples

## 7. Testing Policies

## 8. Related Specifications
```

---

## Dependencies

**Blocks:** None (foundational)

**Depends On:**
- Item-009: Authentication spec (defines subject/claims)

---

## Related Items

- [Item-009-Prio-P1.md](Item-009-Prio-P1.md) - Authentication spec
- [spec-index.md](../../spec/spec-index.md)

---

## Definition of Done

- [x] spec-process-authorization.md created and comprehensive
- [x] Architecture documented
- [x] RBAC and ABAC patterns explained
- [x] Example policies included
- [x] Testing approach documented
- [x] Linked from spec-index.md

---

## Notes

- Critical for implementing role-based and attribute-based access control
- OPA is powerful but Rego has a learning curve
- Examples should be practical and copy-paste ready
- Consider creating example policies in code alongside spec
