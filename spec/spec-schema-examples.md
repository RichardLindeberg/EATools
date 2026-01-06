---
title: Schema Examples and Patterns
version: 1.0
date_created: 2026-01-06
last_updated: 2026-01-06
owner: EA Platform Team
tags: [schema, examples, patterns, workflows]
---

# Introduction

This specification provides complete working examples, patterns, and edge cases for the EA Tool domain model.

## 1. Purpose & Scope

**Purpose**: Provide practical examples demonstrating entity creation, relationship modeling, and common patterns.

**Scope**:
- Complete workflow examples
- ArchiMate pattern implementations
- Edge case handling
- Multi-entity scenarios

**Audience**: All developers, architects, integration partners.

## Examples

### Example 1: Complete Service Realization (ArchiMate Pattern)

```json
// Step 1: Create BusinessCapability
POST /business-capabilities
{
  "name": "Payment Processing",
  "parent_id": null
}
// Returns: cap-payments-uuid

// Step 2: Create ApplicationService
POST /application-services
{
  "name": "Payment Service",
  "business_capability_id": "cap-payments-uuid"
}
// Returns: svc-payment-uuid

// Step 3: Create Application
POST /applications
{
  "name": "Billing API",
  "owner": "payments-team",
  "lifecycle": "active",
  "capability_id": "cap-payments-uuid",
  "data_classification": "confidential"
}
// Returns: app-billing-uuid

// Step 4: Create ApplicationInterface
POST /application-interfaces
{
  "name": "Payment REST API v2",
  "protocol": "https",
  "endpoint": "https://api.example.com/v2/payments",
  "exposed_by_app_id": "app-billing-uuid",
  "serves_service_ids": ["svc-payment-uuid"],
  "status": "active"
}
// Returns: intf-payment-api-uuid

// Step 5: Create Relations
POST /relations
[
  {
    "source_id": "svc-payment-uuid",
    "target_id": "cap-payments-uuid",
    "source_type": "application_service",
    "target_type": "business_capability",
    "relation_type": "realizes",
    "archimate_relationship": "Realization"
  },
  {
    "source_id": "app-billing-uuid",
    "target_id": "svc-payment-uuid",
    "source_type": "application",
    "target_type": "application_service",
    "relation_type": "realizes",
    "archimate_relationship": "Realization"
  },
  {
    "source_id": "app-billing-uuid",
    "target_id": "intf-payment-api-uuid",
    "source_type": "application",
    "target_type": "application_interface",
    "relation_type": "exposes",
    "archimate_relationship": "Composition"
  },
  {
    "source_id": "intf-payment-api-uuid",
    "target_id": "svc-payment-uuid",
    "source_type": "application_interface",
    "target_type": "application_service",
    "relation_type": "serves",
    "archimate_relationship": "Serving"
  }
]
```

### Example 2: Hierarchical Organizations

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

### Example 3: Temporal Relations (Future Planning)

```json
// Plan future deployment
POST /relations
{
  "source_id": "app-new-uuid",
  "target_id": "srv-k8s-uuid",
  "source_type": "application",
  "target_type": "server",
  "relation_type": "deployed_on",
  "effective_from": "2026-03-01T00:00:00Z",
  "effective_to": null,
  "confidence": 0.8,
  "evidence_source": "architecture-decision-123"
}

// Query current state (before March 1)
GET /relations?effective_at=2026-02-01T00:00:00Z
// Returns: empty (relation not yet effective)

// Query future state (after March 1)
GET /relations?effective_at=2026-03-15T00:00:00Z
// Returns: relation (now effective)
```

### Edge Case: Cycle Detection

```json
// Initial state: org-a → org-b → null
PATCH /organizations/org-b
{
  "parent_id": "org-a"  // Would create cycle
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
      "message": "Cannot set parent_id to org-a as it would create cycle: org-b → org-a → org-b",
      "code": "CIRCULAR_REFERENCE"
    }
  ]
}
```

### Edge Case: Data Classification Inheritance

```json
// Application with confidential classification
{
  "id": "app-hr",
  "name": "HR System",
  "data_classification": "confidential"
}

// Relation without explicit classification
{
  "source_id": "app-hr",
  "target_id": "data-employee",
  "source_type": "application",
  "target_type": "data_entity",
  "relation_type": "writes",
  "data_classification": null  // Inherits "confidential" from app
}

// Queried relation will show inherited classification
GET /relations/rel-123
Response:
{
  "id": "rel-123",
  "source_id": "app-hr",
  "target_id": "data-employee",
  "relation_type": "writes",
  "data_classification": "confidential"  // Inherited
}
```

## 11. Related Specifications

- **[Specification Index](spec-index.md)**
- **[Domain Model Overview](spec-schema-domain-overview.md)**
- **[Business Layer Entities](spec-schema-entities-business.md)**
- **[Application Layer Entities](spec-schema-entities-application.md)**
- **[Infrastructure Entities](spec-schema-entities-infrastructure.md)**
- **[Validation Rules](spec-schema-validation.md)**
