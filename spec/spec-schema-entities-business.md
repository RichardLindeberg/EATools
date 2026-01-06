---
title: Business Layer Entities
version: 1.0
date_created: 2026-01-06
last_updated: 2026-01-06
owner: EA Platform Team
tags: [schema, entities, business-layer, organization, capability]
---

# Introduction

This specification defines the Business Layer entities in the EA Tool domain model, aligned with ArchiMate 3.2 Business Layer concepts. These entities represent organizational structures and business capabilities.

## 1. Purpose & Scope

**Purpose**: Define the schema, validation rules, and usage patterns for Business Layer entities that model organizational structures and business capabilities.

**Scope**:
- Organization entity (hierarchical organizational units)
- BusinessCapability entity (business function hierarchy)
- Validation rules specific to these entities
- Usage patterns and examples

**Audience**: Backend developers, API implementers, business analysts.

## 2. Definitions

- **Organization**: A business unit, department, or team that owns or manages IT assets
- **BusinessCapability**: A business function or capability independent of implementation
- **Hierarchical Entity**: An entity that references itself for parent-child relationships
- **Cycle Detection**: Validation to prevent circular parent-child references

## 3. Requirements, Constraints & Guidelines

### Entity-Specific Requirements

- **REQ-BUS-001**: Organizations MUST support unlimited nesting depth via parent_id
- **REQ-BUS-002**: BusinessCapabilities MUST support unlimited nesting depth via parent_id
- **REQ-BUS-003**: Root entities (top-level) MUST have parent_id = null
- **REQ-BUS-004**: Deleting a parent entity MUST handle or prevent child entity orphaning

### Validation Constraints

- **CON-BUS-001**: Organization names MUST be unique within the same parent scope
- **CON-BUS-002**: Organization domains MUST be valid DNS domain names
- **CON-BUS-003**: Organization contacts MUST be valid email addresses (RFC 5322)
- **CON-BUS-004**: BusinessCapability names MUST be unique within the same parent scope
- **CON-BUS-005**: Updating parent_id MUST not create cycles (validated before commit)

### Design Guidelines

- **GUD-BUS-001**: Use Organization for "who" questions - who owns, who manages, who is responsible
- **GUD-BUS-002**: Use BusinessCapability for "what" questions - what does the business do
- **GUD-BUS-003**: Keep Organization hierarchy shallow (3-4 levels max) for practical usability
- **GUD-BUS-004**: Model BusinessCapability as stable capabilities, not organizational structure

## 4. Interfaces & Data Contracts

### Organization

**Purpose**: Represents a business organization or organizational unit that owns or manages IT assets.

**ArchiMate Element**: `BusinessActor` or `BusinessRole`

**Layer**: Business

**Schema**:
```json
{
  "id": "string (uuid, required)",
  "name": "string (1-255 chars, required)",
  "parent_id": "string (uuid, nullable, FK to organizations.id)",
  "domains": ["string (valid DNS domain)"],
  "contacts": ["string (valid email RFC 5322)"],
  "created_at": "string (ISO 8601 UTC with Z, required)",
  "updated_at": "string (ISO 8601 UTC with Z, required)",
  "deleted_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

**Field Descriptions**:

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| id | UUID | Yes | Unique identifier |
| name | String | Yes | Organization name (1-255 chars, trimmed) |
| parent_id | UUID | No | Reference to parent organization for hierarchy |
| domains | Array<String> | No | DNS domains owned by this organization |
| contacts | Array<String> | No | Email addresses of organization contacts |
| created_at | Timestamp | Yes | Creation timestamp (UTC) |
| updated_at | Timestamp | Yes | Last update timestamp (UTC) |
| deleted_at | Timestamp | No | Soft delete timestamp (UTC) |

**Validation Rules**:
- `name` is required, trimmed, 1-255 characters
- `name` must be unique within same parent scope
- `parent_id` if provided must reference existing organization
- `parent_id` cannot equal `id` (no self-reference)
- `parent_id` update must not create cycles
- `domains` must be valid DNS domain names (e.g., "example.com")
- `contacts` must be valid email addresses per RFC 5322

**Database Indexes**:
- Primary key on `id`
- Index on `parent_id` for hierarchy queries
- Index on `name` for search
- Unique compound index on `(parent_id, name)` for scoped uniqueness

**Usage Patterns**:
- Model organizational hierarchies: Enterprise → Division → Department → Team
- Link applications and servers to owning organization via `owns` relation
- Track which teams or departments are responsible for IT assets
- Query organizational structures using recursive CTEs for full tree

**Example**:
```json
{
  "id": "org-123e4567-e89b-12d3-a456-426614174000",
  "name": "Payments Team",
  "parent_id": "org-parent-division-uuid",
  "domains": ["payments.example.com", "billing.example.com"],
  "contacts": ["payments-lead@example.com", "team-payments@example.com"],
  "created_at": "2026-01-06T10:00:00Z",
  "updated_at": "2026-01-06T10:00:00Z",
  "deleted_at": null
}
```

**Relationships**:
- `organization` → `organization` (hierarchical via parent_id, not relation entity)
- `organization` → `application` (owns)
- `organization` → `server` (owns)

---

### BusinessCapability

**Purpose**: Represents a business capability or function independent of how it is implemented.

**ArchiMate Element**: `BusinessFunction` or `Capability`

**Layer**: Business

**Schema**:
```json
{
  "id": "string (uuid, required)",
  "name": "string (1-255 chars, required)",
  "parent_id": "string (uuid, nullable, FK to business_capabilities.id)",
  "created_at": "string (ISO 8601 UTC with Z, required)",
  "updated_at": "string (ISO 8601 UTC with Z, required)",
  "deleted_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

**Field Descriptions**:

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| id | UUID | Yes | Unique identifier |
| name | String | Yes | Capability name (1-255 chars, trimmed) |
| parent_id | UUID | No | Reference to parent capability for hierarchy |
| created_at | Timestamp | Yes | Creation timestamp (UTC) |
| updated_at | Timestamp | Yes | Last update timestamp (UTC) |
| deleted_at | Timestamp | No | Soft delete timestamp (UTC) |

**Validation Rules**:
- `name` is required, trimmed, 1-255 characters
- `name` should be unique within same parent scope (recommended)
- `parent_id` if provided must reference existing capability
- `parent_id` cannot equal `id` (no self-reference)
- `parent_id` update must not create cycles

**Database Indexes**:
- Primary key on `id`
- Index on `parent_id` for hierarchy queries
- Index on `name` for search
- Consider unique compound index on `(parent_id, name)` for scoped uniqueness

**Usage Patterns**:
- Build a business capability model (hierarchy)
- Map applications to capabilities to assess coverage
- Identify gaps or overlaps in application portfolio
- Group views by capability for business-oriented perspectives
- Support capability-based roadmapping and investment analysis

**Example**:
```json
{
  "id": "cap-123e4567-e89b-12d3-a456-426614174000",
  "name": "Payment Processing",
  "parent_id": "cap-parent-finance-uuid",
  "created_at": "2026-01-06T10:00:00Z",
  "updated_at": "2026-01-06T10:00:00Z",
  "deleted_at": null
}
```

**Relationships**:
- `business_capability` → `business_capability` (hierarchical via parent_id)
- `application` → `business_capability` (supports)
- `application_service` → `business_capability` (realizes, supports)

## 5. Acceptance Criteria

- **AC-BUS-001**: Given a valid Organization payload, When created, Then id, created_at, updated_at are auto-generated
- **AC-BUS-002**: Given an Organization with parent_id, When parent is deleted, Then either cascade delete children or prevent deletion
- **AC-BUS-003**: Given an Organization update setting parent_id, When this creates a cycle, Then validation fails with 422 error
- **AC-BUS-004**: Given multiple Organizations with same name under same parent, When created, Then only one succeeds (unique constraint)
- **AC-BUS-005**: Given a BusinessCapability hierarchy, When queried, Then full tree can be retrieved efficiently with recursive query
- **AC-BUS-006**: Given an Organization with invalid email in contacts, When validated, Then 422 error with field-level detail
- **AC-BUS-007**: Given root Organization (parent_id=null), When listed, Then it appears as top-level entity
- **AC-BUS-008**: Given nested BusinessCapabilities (3+ levels), When querying descendants, Then all children returned in single query

## 6. Test Automation Strategy

- **Test Data**: Factory functions for creating valid Organizations and BusinessCapabilities with/without parents
- **Cycle Detection**: Property-based tests generating random hierarchies and verifying cycle prevention
- **Hierarchy Queries**: Tests for recursive CTE performance with deep (10+ levels) hierarchies
- **Unique Constraints**: Tests verifying name uniqueness within parent scope
- **Edge Cases**: Empty parent_id, self-reference attempts, circular reference chains

## 7. Rationale & Context

### Why hierarchical Organizations?
- Real-world enterprises have nested organizational structures
- Enables inheritance of policies, ownership, and permissions
- Supports org chart visualization and reporting roll-ups
- Allows flexible modeling: geo-based, functional, or matrix structures

### Why separate Organization from BusinessCapability?
- Organization changes frequently (reorgs, M&A)
- BusinessCapability should be stable over time
- Organizations answer "who", Capabilities answer "what"
- Separates operational structure from strategic capabilities

### Why allow unlimited nesting depth?
- Different enterprises have different depth needs
- Prevents artificial constraints on modeling
- Application layer can limit display depth for UX

### Why unique names only within parent scope?
- Allows "Engineering" to exist under multiple divisions
- Reduces naming conflicts and bureaucracy
- More natural for users entering data

## 8. Dependencies & External Integrations

### Data Dependencies
- **DAT-BUS-001**: CMDB - May synchronize organizational data from CMDB systems
- **DAT-BUS-002**: HR Systems - May import org structure from HR/identity systems

### Technology Platform Dependencies
- **PLT-BUS-001**: Database - Must support recursive CTEs for hierarchy queries

## 9. Examples & Edge Cases

### Example: Three-Level Organization Hierarchy

```json
[
  {
    "id": "org-enterprise",
    "name": "Acme Corporation",
    "parent_id": null,
    "domains": ["acme.com"],
    "contacts": ["cio@acme.com"]
  },
  {
    "id": "org-engineering",
    "name": "Engineering Division",
    "parent_id": "org-enterprise",
    "domains": ["eng.acme.com"],
    "contacts": ["vp-eng@acme.com"]
  },
  {
    "id": "org-payments",
    "name": "Payments Team",
    "parent_id": "org-engineering",
    "domains": ["payments.acme.com"],
    "contacts": ["payments-lead@acme.com"]
  }
]
```

### Example: BusinessCapability Hierarchy

```json
[
  {
    "id": "cap-finance",
    "name": "Financial Management",
    "parent_id": null
  },
  {
    "id": "cap-payments",
    "name": "Payment Processing",
    "parent_id": "cap-finance"
  },
  {
    "id": "cap-payment-auth",
    "name": "Payment Authorization",
    "parent_id": "cap-payments"
  },
  {
    "id": "cap-payment-settlement",
    "name": "Payment Settlement",
    "parent_id": "cap-payments"
  }
]
```

### Edge Case: Cycle Detection

```json
// Initial state
{
  "id": "org-a",
  "name": "Org A",
  "parent_id": "org-b"
}
{
  "id": "org-b",
  "name": "Org B",
  "parent_id": null
}

// Attempt to create cycle - MUST FAIL
PATCH /organizations/org-b
{
  "parent_id": "org-a"  // Would create: org-a -> org-b -> org-a (cycle)
}

Response 422:
{
  "type": "https://api.example.com/errors/validation",
  "title": "Validation Failed",
  "status": 422,
  "detail": "Updating parent_id would create a circular reference",
  "errors": [
    {
      "field": "parent_id",
      "message": "Cannot set parent_id to org-a as it would create a cycle: org-b -> org-a -> org-b",
      "code": "CIRCULAR_REFERENCE"
    }
  ]
}
```

### Edge Case: Orphan Prevention

```json
// Delete parent with children - two strategies:

// Strategy 1: Prevent deletion
DELETE /organizations/org-parent

Response 409:
{
  "type": "https://api.example.com/errors/conflict",
  "title": "Cannot Delete Organization",
  "status": 409,
  "detail": "Organization has 3 child organizations. Delete or reassign children first.",
  "children": ["org-child-1", "org-child-2", "org-child-3"]
}

// Strategy 2: Cascade soft delete
DELETE /organizations/org-parent?cascade=true

Response 204: (parent and all descendants soft-deleted)
```

## 10. Validation Criteria

- Hierarchical cycles MUST be detected and prevented
- Name uniqueness within parent scope MUST be enforced
- Parent references MUST point to existing entities
- Self-references (parent_id = id) MUST be rejected
- Email addresses MUST validate against RFC 5322
- DNS domains MUST validate against RFC 1035

## 11. Related Specifications

- **[Specification Index](spec-index.md)** - All specifications
- **[Domain Model Overview](spec-schema-domain-overview.md)** - Core requirements and patterns
- **[Application Layer Entities](spec-schema-entities-application.md)** - Application, ApplicationService
- **[Validation Rules](spec-schema-validation.md)** - Validation matrix
- **[Schema Examples](spec-schema-examples.md)** - More examples

### External References
- [ArchiMate 3.2 Business Layer](https://pubs.opengroup.org/architecture/archimate3-doc/chap06.html)
- [RFC 5322 Email Address](https://tools.ietf.org/html/rfc5322)
- [RFC 1035 DNS](https://tools.ietf.org/html/rfc1035)
