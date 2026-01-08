---
title: Authorization Model Specification
version: 0.1.0
date_created: 2026-01-08
last_updated: 2026-01-08
owner: EA Platform Team
tags: [process, security, authorization, opa, rego]
---

# Introduction

This specification defines the authorization model for the EA Tool using Open Policy Agent (OPA) and Rego policies. It standardizes how authorization inputs are structured, how decisions are produced and enforced, and how RBAC and ABAC patterns are implemented and tested.

## 1. Purpose & Scope

Establish a consistent, fail-closed authorization model for all EA Tool services. Scope covers the policy decision point (OPA), policy inputs/outputs, RBAC and ABAC patterns, obligations, error handling, and policy testing. Audience: backend engineers, platform engineers, and policy authors.

## 2. Definitions

- **OPA**: Open Policy Agent, the policy decision point (PDP).
- **Rego**: Policy language used by OPA.
- **PDP**: Policy Decision Point; evaluates policies.
- **PEP**: Policy Enforcement Point; API middleware that calls the PDP and enforces results.
- **PIP**: Policy Information Point; provides attributes used in decisions (e.g., user directory, metadata service).
- **RBAC**: Role-Based Access Control; decisions based on assigned roles and permissions.
- **ABAC**: Attribute-Based Access Control; decisions based on subject/resource/context attributes.
- **Obligations**: Additional instructions attached to decisions (e.g., field filters, redactions).
- **Fail-Closed**: Default deny behavior on PDP errors or timeouts.

## 3. Requirements, Constraints & Guidelines

- **REQ-001**: Authorization SHALL be centralized via OPA acting as the PDP for all API calls.
- **REQ-002**: All authorization decisions SHALL use the canonical input envelope: `subject`, `action`, `resource`, `context`.
- **REQ-003**: PEPs SHALL include a correlation/trace identifier in every decision request and log entry.
- **REQ-004**: Policies SHALL produce deterministic allow/deny decisions and MAY include obligations.
- **SEC-001**: The system MUST fail closed (deny) on PDP errors, timeouts, or unavailable PDP.
- **SEC-002**: Default decision MUST be deny unless explicitly allowed by policy.
- **SEC-003**: Decision and policy evaluation logs MUST avoid leaking sensitive attributes beyond what is required for audits.
- **SEC-004**: Policies MUST be version-controlled and changes MUST be auditable with author, reason, and timestamp.
- **SEC-005**: Obligations that filter data MUST be applied before data leaves the service boundary.
- **CON-001**: PEP time budget for PDP requests SHOULD be ≤ 200ms p99; timeout => deny.
- **CON-002**: Policy inputs MUST be JSON-serializable and schema-validated before invoking PDP.
- **CON-003**: OPA deployment modes: embedded for development; sidecar or centralized service for staging/production.
- **GUD-001**: Prefer small, composable Rego modules (RBAC, ABAC, ownership, classification, time windows).
- **GUD-002**: Keep policy data (roles, bindings) externalized in data documents, not hard-coded rules.
- **GUD-003**: Provide policy stubs/mocks for local development and automated testing.
- **PAT-001**: Owner-based access pattern MUST allow owners full read/write unless explicitly denied.
- **PAT-002**: RBAC pattern MUST map roles to permissions, then permissions to actions/resources.
- **PAT-003**: ABAC pattern MUST evaluate attributes such as environment, team, department, classification, and tags.
- **PAT-004**: Time-based access pattern MUST support allow/deny within defined time windows (e.g., maintenance only).

## 4. Interfaces & Data Contracts

**Decision Input Envelope**

```json
{
  "subject": {
    "sub": "user-123",
    "email": "user@example.com",
    "roles": ["viewer", "team-lead"],
    "groups": ["payments"],
    "claims": {"department": "payments", "tenant": "acme"}
  },
  "action": "read", // read | write | delete | admin | custom verb
  "resource": {
    "type": "application",
    "id": "app-123",
    "owner": "payments",
    "environment": "prod",
    "classification": "confidential",
    "tags": ["pci", "payments"]
  },
  "context": {
    "method": "GET",
    "path": "/applications/app-123",
    "scopes": ["read:applications"],
    "tenant": "acme",
    "ip": "203.0.113.10",
    "trace_id": "trace-abc123",
    "time": "2026-01-08T10:00:00Z"
  }
}
```

**Decision Output Envelope**

```json
{
  "allow": true,
  "obligations": {
    "fields.deny": ["secrets"],
    "fields.mask": ["credentials"],
    "filters": {
      "classification": "<= internal"
    }
  },
  "trace_id": "trace-abc123",
  "policy_version": "2026-01-08-01"
}
```

**PEP ↔ PDP Interaction**
- Protocol: HTTP POST to OPA `v1/data/authz/allow` (or embedded SDK call), JSON body = input envelope.
- Timeout: 200ms budget; on timeout or transport failure => deny.
- Headers: `X-Trace-Id` propagated from incoming request; `X-Policy-Version` optional.
- Caching: Short-lived positive decision cache MAY be used if obligations are included in cache key.

**Data Contracts for Policy Data (example)**
- Roles document: `{ "roles": { "viewer": ["read"], "editor": ["read", "write"] } }`
- Bindings document: `{ "bindings": { "payments": { "users": ["user-123"], "roles": ["editor"] } } }`
- Classification levels: `public < internal < confidential < restricted`

## 5. Acceptance Criteria

- **AC-001**: Given an authenticated request, when the PEP calls OPA with the canonical envelope, then allow/deny and obligations are returned and enforced.
- **AC-002**: Given OPA is unreachable or returns an error, when a request is evaluated, then the PEP denies access and logs the failure with the trace identifier.
- **AC-003**: Given a subject with `viewer` role, when requesting to read an `application`, then the RBAC policy allows and returns no restrictive obligations.
- **AC-004**: Given a resource classified `restricted`, when a user without matching clearance requests access, then the ABAC policy denies with an explicit reason in decision logs.
- **AC-005**: Given a maintenance window policy, when a write occurs outside the allowed time window, then the policy denies.
- **AC-006**: Policy tests (unit and integration) cover RBAC, ABAC, ownership, classification, and time-based patterns with green test results.
- **AC-007**: The specification is linked from the specifications index and follows the standard template sections.

## 6. Test Automation Strategy

- **Test Levels**: Unit (Rego `opa test`), integration (PEP calling embedded/sidecar OPA), end-to-end (API + PEP + PDP).
- **Frameworks**: OPA built-in test runner; `conftest` for contract checks; service integration tests via existing test framework.
- **Test Data Management**: Policy fixtures and JSON inputs stored under `tests/policies/fixtures`; isolated per scenario; include negative cases.
- **CI/CD Integration**: Run `opa test` and `conftest verify` in CI; fail pipeline on policy lint/test failures; publish policy bundle with version metadata.
- **Coverage Requirements**: Policy rules covering RBAC, ABAC, ownership, classification, time windows; target ≥ 90% rule coverage.
- **Performance Testing**: Measure PDP latency (p50/p90/p99) in CI smoke tests; ensure p99 ≤ 200ms under nominal load.

## 7. Rationale & Context

- Centralizing authorization in OPA decouples policy from application code and enables consistent enforcement across services.
- Canonical input/output envelopes simplify integration for all PEPs and improve policy reusability.
- Fail-closed behavior mitigates privilege escalation from PDP outages or errors.
- Obligations allow fine-grained data control (field filters/redactions) without duplicating logic across services.
- RBAC covers coarse-grained access; ABAC and obligations provide fine-grained control for multi-tenant and classified data.

## 8. Dependencies & External Integrations

### External Systems
- **EXT-001**: Identity Provider (OIDC) - Supplies subject claims (sub, email, roles, groups).
- **EXT-002**: Metadata/Directory Service - Optional PIP for team/department attributes.

### Third-Party Services
- **SVC-001**: OPA service or sidecar - PDP execution environment.

### Infrastructure Dependencies
- **INF-001**: Service-to-OPA network path with enforced TLS; configurable timeouts.

### Data Dependencies
- **DAT-001**: Policy bundles stored in versioned artifact storage; pulled by OPA at startup or periodically.

### Technology Platform Dependencies
- **PLT-001**: OPA runtime compatible with Rego policies used; align with API service runtime (.NET 10) for embedding scenarios.

### Compliance Dependencies
- **COM-001**: Audit logging of authorization decisions in compliance with security standards; retention per security policy.

## 9. Examples & Edge Cases

```rego
package authz

default allow = false

# RBAC: viewer can read applications
allow {
  input.action == "read"
  input.resource.type == "application"
  "viewer" in input.subject.roles
}

# Ownership: owners can write their resources
allow {
  input.action == "write"
  input.resource.owner == input.subject.claims.team
}

# ABAC: deny if environment is prod and subject missing prod clearance
deny_reason["missing_prod_clearance"] {
  input.resource.environment == "prod"
  not input.subject.claims.prod_clearance
}

# Time-based deny outside window (UTC)
deny_reason["outside_maintenance_window"] {
  input.action == "write"
  not within_window(input.context.time)
}

within_window(t) {
  # Expect RFC3339 timestamp; maintenance window 01:00-02:00Z
  hour := time.hour(time.parse_rfc3339_ns(t))
  hour >= 1
  hour < 2
}

# Obligations: mask credentials field
obligations["fields.mask"] = ["credentials"] {
  input.resource.type == "application"
  input.action == "read"
  "viewer" in input.subject.roles
}

allow {
  count(deny_reason) == 0
}
```

**Edge Cases**
- Missing required input fields => validation failure; PEP denies before contacting PDP.
- PDP returns obligations requiring filters; PEP must apply filters before data is returned.
- Stale policy version detected => PEP logs warning and optionally retries once; otherwise deny.
- Multi-tenant: `tenant` mismatch between subject and resource => deny.

## 10. Validation Criteria

- Input envelopes conform to the defined JSON structure (required fields present and typed).
- PEP enforces deny on PDP timeout/error and logs trace identifiers.
- Rego policies pass unit tests for RBAC, ABAC, ownership, classification, and time-based rules.
- Obligations are applied server-side before responses are emitted.
- Spec is cross-referenced in spec index and follows the standard template sections.

## 11. Related Specifications / Further Reading

- [spec-index.md](spec/spec-index.md)
- [spec-process-authentication.md](spec/spec-process-authentication.md)
- [spec-process-security.md](spec/spec-process-security.md)
- [spec-schema-domain-overview.md](spec/spec-schema-domain-overview.md)
- [openapi.yaml](openapi.yaml)
- [docs/system-overview.md](docs/system-overview.md)