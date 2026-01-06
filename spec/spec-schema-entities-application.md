---
title: Application Layer Entities
version: 1.0
date_created: 2026-01-06
last_updated: 2026-01-06
owner: EA Platform Team
tags: [schema, entities, application-layer, application, service, interface]
---

# Introduction

This specification defines the Application Layer entities aligned with ArchiMate 3.2 Application Layer concepts. These entities represent software applications, the services they provide, and the interfaces through which services are accessed.

## 1. Purpose & Scope

**Purpose**: Define schemas, validation rules, and usage patterns for Application Layer entities that model software systems and their service-oriented architecture.

**Scope**:
- Application entity (software components)
- ApplicationService entity (business services provided)
- ApplicationInterface entity (technical access points)
- Service realization and interface serving patterns

**Audience**: Backend developers, solution architects, API designers.

## 2. Definitions

- **Application**: A software application or system component (the "how")
- **ApplicationService**: A business service or capability that applications provide (the "what")
- **ApplicationInterface**: A technical interface through which services are accessed (the "where/how")
- **Service Realization**: Pattern where Application realizes ApplicationService
- **Interface Serving**: Pattern where ApplicationInterface serves ApplicationService

## 3. Requirements, Constraints & Guidelines

### Entity-Specific Requirements

- **REQ-APP-001**: Every Application SHOULD have at least one associated ApplicationService (via realizes relation)
- **REQ-APP-002**: ApplicationService MAY be realized by multiple Applications
- **REQ-APP-003**: ApplicationInterface MUST be exposed by exactly one Application
- **REQ-APP-004**: ApplicationInterface MAY serve multiple ApplicationServices
- **REQ-APP-005**: Application lifecycle transitions MUST follow: planned → active → deprecated → retired

### Validation Constraints

- **CON-APP-001**: Application names MUST be unique across the system
- **CON-APP-002**: ApplicationInterface endpoint MUST be valid URL or connection string
- **CON-APP-003**: ApplicationInterface version SHOULD follow semantic versioning
- **CON-APP-004**: ApplicationInterface exposed_by_app_id MUST reference existing application
- **CON-APP-005**: ApplicationService exposed_by_app_ids MUST all reference existing applications

### Design Guidelines

- **GUD-APP-001**: Model ApplicationService for business perspective, ApplicationInterface for technical perspective
- **GUD-APP-002**: Use ApplicationService to decouple business capabilities from implementation
- **GUD-APP-003**: Create separate ApplicationInterfaces for different protocols (REST, gRPC, etc.)
- **GUD-APP-004**: Track interface versions to manage deprecation and client compatibility

## 4. Interfaces & Data Contracts

### Application

**Purpose**: Software application or system component.

**ArchiMate Element**: `ApplicationComponent`

**Layer**: Application

**Schema**:
```json
{
  "id": "string (uuid, required)",
  "name": "string (1-255 chars, required)",
  "owner": "string (1-255 chars, required)",
  "lifecycle": "enum (planned|active|deprecated|retired, required)",
  "capability_id": "string (uuid, nullable, FK to business_capabilities.id)",
  "data_classification": "enum (public|internal|confidential|restricted, required)",
  "tags": ["string"],
  "created_at": "string (ISO 8601 UTC with Z, required)",
  "updated_at": "string (ISO 8601 UTC with Z, required)",
  "deleted_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

**Relationships**:
- `application` → `server` (deployed_on, stores_data_on)
- `application` → `business_capability` (supports)
- `application` → `data_entity` (reads, writes)
- `application` → `application_service` (realizes, uses)
- `application` → `application_interface` (exposes)
- `application` → `application` (depends_on, communicates_with, calls)
- `organization` → `application` (owns)

---

### ApplicationService

**Purpose**: Business service or capability that applications provide (what capability is offered).

**ArchiMate Element**: `ApplicationService`

**Layer**: Application

**Schema**:
```json
{
  "id": "string (uuid, required)",
  "name": "string (1-255 chars, required)",
  "description": "string (0-2000 chars, nullable)",
  "business_capability_id": "string (uuid, nullable, FK to business_capabilities.id)",
  "sla": "string (0-500 chars, nullable)",
  "exposed_by_app_ids": ["string (uuid, FK to applications.id)"],
  "consumers": ["string (application id or team name)"],
  "tags": ["string"],
  "created_at": "string (ISO 8601 UTC with Z, required)",
  "updated_at": "string (ISO 8601 UTC with Z, required)",
  "deleted_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

**Relationships**:
- `application` → `application_service` (realizes, uses)
- `application_service` → `business_capability` (realizes, supports)
- `application_interface` → `application_service` (serves)

---

### ApplicationInterface

**Purpose**: Technical interface through which applications expose services (where/how services are accessed).

**ArchiMate Element**: `ApplicationInterface`

**Layer**: Application

**Schema**:
```json
{
  "id": "string (uuid, required)",
  "name": "string (1-255 chars, required)",
  "protocol": "string (1-50 chars, required, e.g., https, grpc, kafka)",
  "endpoint": "string (valid URL or connection string, required)",
  "specification_url": "string (valid HTTPS URL, nullable)",
  "version": "string (semver format, nullable)",
  "authentication_method": "string (1-50 chars, nullable)",
  "exposed_by_app_id": "string (uuid, required, FK to applications.id)",
  "serves_service_ids": ["string (uuid, FK to application_services.id)"],
  "rate_limits": {
    "requests_per_second": "integer (positive)",
    "burst": "integer (positive)",
    "quotas": "object"
  },
  "status": "enum (active|deprecated|retired, required)",
  "tags": ["string"],
  "created_at": "string (ISO 8601 UTC with Z, required)",
  "updated_at": "string (ISO 8601 UTC with Z, required)",
  "deleted_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

**Relationships**:
- `application` → `application_interface` (exposes)
- `application_interface` → `application_service` (serves)
- `integration` → `application_interface` (calls, uses)

## 5. Acceptance Criteria

- **AC-APP-001**: Given Application with lifecycle=planned, When updated to active, Then transition succeeds
- **AC-APP-002**: Given Application with lifecycle=active, When updated to planned, Then validation fails (invalid transition)
- **AC-APP-003**: Given ApplicationInterface with invalid endpoint URL, When validated, Then 422 error returned
- **AC-APP-004**: Given ApplicationInterface, When exposed_by_app_id references non-existent app, Then 422 error returned
- **AC-APP-005**: Given ApplicationService realized by multiple Applications, When queried, Then all realizing apps returned

## 6. Test Automation Strategy

- **Lifecycle Testing**: Test all valid and invalid lifecycle transitions
- **Reference Integrity**: Test FK validation for all referenced entities
- **URL Validation**: Test endpoint and specification_url validation
- **Version Management**: Test semver parsing and comparison

## 7. Rationale & Context

### Why separate Application, ApplicationService, and ApplicationInterface?

**Application** (Component):
- Represents the software artifact/deployment
- Answers "what software implements this?"
- Has technical attributes (owner, lifecycle, classification)

**ApplicationService** (Service):
- Represents the business capability offered
- Answers "what business service is provided?"
- Decouples business view from implementation
- Enables service-oriented architecture modeling

**ApplicationInterface** (Interface):
- Represents the technical access point
- Answers "where and how do I access the service?"
- Supports multiple interfaces per service (REST + gRPC)
- Tracks versioning and deprecation independently

This separation enables:
- Multiple applications implementing same service
- Multiple interfaces exposing same service
- Independent versioning and lifecycle management
- Business views independent of implementation changes

## 8. Dependencies & External Integrations

### Data Dependencies
- **DAT-APP-001**: CMDB - Application inventory synchronization
- **DAT-APP-002**: APM Tools - Application performance and health data

## 9. Examples & Edge Cases

### Example: Complete Service Realization Pattern

```json
// 1. Create Application
POST /applications
{
  "name": "Billing API",
  "owner": "payments-team",
  "lifecycle": "active",
  "capability_id": "cap-payments",
  "data_classification": "confidential"
}
// Returns: app-billing-uuid

// 2. Create ApplicationService
POST /application-services
{
  "name": "Payment Processing Service",
  "description": "Handles payment processing with PCI compliance",
  "business_capability_id": "cap-payments",
  "sla": "99.95% uptime",
  "exposed_by_app_ids": ["app-billing-uuid"]
}
// Returns: svc-payment-uuid

// 3. Create ApplicationInterface (REST)
POST /application-interfaces
{
  "name": "Payment REST API v2",
  "protocol": "https",
  "endpoint": "https://api.example.com/v2/payments",
  "specification_url": "https://api.example.com/v2/openapi.yaml",
  "version": "2.1.0",
  "authentication_method": "oauth2",
  "exposed_by_app_id": "app-billing-uuid",
  "serves_service_ids": ["svc-payment-uuid"],
  "status": "active"
}

// 4. Create ApplicationInterface (gRPC)
POST /application-interfaces
{
  "name": "Payment gRPC API v2",
  "protocol": "grpc",
  "endpoint": "grpc://api.example.com:50051",
  "specification_url": "https://api.example.com/proto/payment.proto",
  "version": "2.1.0",
  "authentication_method": "mtls",
  "exposed_by_app_id": "app-billing-uuid",
  "serves_service_ids": ["svc-payment-uuid"],
  "status": "active"
}

// 5. Create Relations
POST /relations
[
  {
    "source_id": "app-billing-uuid",
    "target_id": "svc-payment-uuid",
    "source_type": "application",
    "target_type": "application_service",
    "relation_type": "realizes",
    "archimate_relationship": "Realization"
  }
]
```

### Edge Case: Interface Deprecation

```json
// Deprecate old interface version
PATCH /application-interfaces/intf-payment-api-v1
{
  "status": "deprecated"
}

// Consumers receive webhook notification
Webhook Payload:
{
  "event": "application_interface.deprecated",
  "entity_id": "intf-payment-api-v1",
  "entity_type": "application_interface",
  "timestamp": "2026-01-06T10:00:00Z",
  "data": {
    "name": "Payment REST API v1",
    "sunset_date": "2026-07-01T00:00:00Z",
    "replacement": "intf-payment-api-v2"
  }
}
```

## 10. Validation Criteria

- Application names MUST be unique
- Lifecycle transitions MUST follow valid paths
- ApplicationInterface endpoints MUST be valid URLs
- Foreign key references MUST point to existing entities
- Enum values MUST match defined enums

## 11. Related Specifications

- **[Specification Index](spec-index.md)**
- **[Domain Model Overview](spec-schema-domain-overview.md)**
- **[Business Layer Entities](spec-schema-entities-business.md)**
- **[Infrastructure Entities](spec-schema-entities-infrastructure.md)**
- **[Validation Rules](spec-schema-validation.md)**
- **[Schema Examples](spec-schema-examples.md)**

### External References
- [ArchiMate 3.2 Application Layer](https://pubs.opengroup.org/architecture/archimate3-doc/chap08.html)
