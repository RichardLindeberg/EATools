---
title: EA Tool Domain Model and Data Schema Specification
version: 1.0
date_created: 2026-01-06
last_updated: 2026-01-06
owner: EA Platform Team
tags: [schema, data-model, domain, archimate, entities]
---

# Introduction

This specification defines the complete domain model and data schema for the EA Tool, an API-first enterprise architecture management platform. The domain model aligns with ArchiMate 3.2 standards while supporting practical enterprise architecture management needs including asset cataloging, relationship modeling, and governance.

## 1. Purpose & Scope

**Purpose**: Define all domain entities, their attributes, data types, constraints, and relationships to ensure consistent implementation across the API, database, and client SDKs.

**Scope**: 
- Core domain entities (Organization, Application, ApplicationService, ApplicationInterface, Server, Integration, BusinessCapability, DataEntity, Relation, View)
- Supporting entities (ImportJob, ExportJob, Webhook, AuditLog)
- ArchiMate 3.2 element and relationship mappings
- Validation rules and constraints
- Temporal and hierarchical modeling patterns

**Audience**: Backend developers, API implementers, database architects, and AI agents generating code.

**Assumptions**:
- All timestamps are UTC in ISO 8601 format with 'Z' suffix
- UUIDs are used for primary keys unless otherwise specified
- JSON fields use standard JSON types and are nullable unless marked required
- All text fields are Unicode (UTF-8)

## 2. Definitions

- **Entity**: A domain object with unique identity and lifecycle (e.g., Application, Server)
- **Relation**: A typed, directed edge between two entities with semantic meaning
- **ArchiMate Element**: A concept from ArchiMate 3.2 standard (e.g., ApplicationComponent, ApplicationService)
- **ArchiMate Relationship**: A connection type from ArchiMate 3.2 (e.g., Realization, Serving, Assignment)
- **FK**: Foreign Key - reference to another entity's primary key
- **JSONB**: JSON Binary - efficient storage and indexing of JSON data in PostgreSQL/MSSQL
- **UTC**: Coordinated Universal Time - all timestamps must be in UTC
- **Lifecycle**: The current phase of an asset (planned, active, deprecated, retired)
- **PII**: Personally Identifiable Information

## 3. Requirements, Constraints & Guidelines

### Core Requirements

- **REQ-001**: All entities MUST have a unique identifier (UUID) as primary key
- **REQ-002**: All entities MUST have `created_at` and `updated_at` timestamps in UTC ISO 8601 format with 'Z' suffix
- **REQ-003**: All enum values MUST be lowercase with underscores (e.g., `application_service`, not `ApplicationService`)
- **REQ-004**: All entities MUST support soft delete (retain data with `deleted_at` timestamp) for audit trails
- **REQ-005**: Relations MUST enforce referential integrity between source and target entities
- **REQ-006**: All hierarchical entities (Organization, BusinessCapability) MUST prevent circular references
- **REQ-007**: All text fields MUST support Unicode (UTF-8) encoding
- **REQ-008**: All nullable fields MUST be explicitly marked as optional in OpenAPI schema
- **REQ-009**: Array fields (tags, contacts, domains) MUST support empty arrays as valid values
- **REQ-010**: JSON fields MUST be validated against documented schemas

### Security Requirements

- **SEC-001**: Data classification levels MUST be one of: `public`, `internal`, `confidential`, `restricted`
- **SEC-002**: PII flags MUST be boolean and default to `false`
- **SEC-003**: Sensitive fields (secrets, passwords) MUST never be returned in API responses
- **SEC-004**: Data classification MUST be inherited by related entities unless explicitly overridden

### Validation Constraints

- **CON-001**: Entity names MUST be 1-255 characters, non-empty, trimmed
- **CON-002**: Email addresses MUST match RFC 5322 format
- **CON-003**: URLs MUST be valid HTTP/HTTPS URLs
- **CON-004**: Hostnames MUST match valid DNS hostname format
- **CON-005**: UUIDs MUST be version 4 (random) format
- **CON-006**: Parent-child relationships MUST not create cycles (validated at write time)
- **CON-007**: Lifecycle transitions MUST follow valid paths: planned → active → deprecated → retired
- **CON-008**: Confidence values MUST be float between 0.0 and 1.0 inclusive
- **CON-009**: Timestamps MUST be valid ISO 8601 dates with 'Z' timezone suffix (UTC)
- **CON-010**: Rate limit values MUST be positive integers

### Design Guidelines

- **GUD-001**: Use composition over inheritance - prefer flat entities with relations over deep type hierarchies
- **GUD-002**: Keep entity models anemic - business logic belongs in service/domain layers, not entity classes
- **GUD-003**: Use descriptive enum values - prefer `deployed_on` over `dep` for readability
- **GUD-004**: Model temporal validity explicitly with `effective_from` and `effective_to` for time-based queries
- **GUD-005**: Use JSONB for semi-structured data that may evolve (tags, metadata, rate_limits)
- **GUD-006**: Denormalize cautiously - prefer relations over duplicated data except for performance-critical paths
- **GUD-007**: Use separate junction tables for many-to-many relationships (e.g., application_servers, application_data)

### ArchiMate Alignment Patterns

- **PAT-001**: Every entity SHOULD map to an ArchiMate 3.2 element type when possible
- **PAT-002**: Relations SHOULD specify both a semantic `relation_type` and an `archimate_relationship`
- **PAT-003**: Layer separation MUST be respected: Business layer (Organization, BusinessCapability), Application layer (Application, ApplicationService, ApplicationInterface, Integration, DataEntity), Technology layer (Server)
- **PAT-004**: Service realization pattern: ApplicationComponent realizes ApplicationService, ApplicationService realizes BusinessCapability
- **PAT-005**: Interface serving pattern: Application exposes ApplicationInterface, ApplicationInterface serves ApplicationService
- **PAT-006**: Deployment pattern: Application assigned to Server via `deployed_on` relation
- **PAT-007**: Data access pattern: Application accesses DataEntity via `reads`/`writes` relations

## 4. Interfaces & Data Contracts

### Entity Schemas

#### Organization

**Purpose**: Business organization or organizational unit that owns/manages IT assets

**ArchiMate Element**: `BusinessActor` or `BusinessRole`

**Schema**:
```json
{
  "id": "string (uuid)",
  "name": "string (1-255 chars, required)",
  "parent_id": "string (uuid, nullable, FK to organizations.id)",
  "domains": ["string (valid DNS domain)"],
  "contacts": ["string (valid email RFC 5322)"],
  "created_at": "string (ISO 8601 UTC with Z)",
  "updated_at": "string (ISO 8601 UTC with Z)",
  "deleted_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

**Validation Rules**:
- `name` is required, trimmed, 1-255 characters
- `parent_id` must reference existing organization, cannot create cycles
- `domains` must be valid DNS domain names
- `contacts` must be valid email addresses

**Database Indexes**:
- Primary key on `id`
- Index on `parent_id` for hierarchy queries
- Index on `name` for search

---

#### Application

**Purpose**: Software application or system component

**ArchiMate Element**: `ApplicationComponent`

**Schema**:
```json
{
  "id": "string (uuid)",
  "name": "string (1-255 chars, required)",
  "owner": "string (1-255 chars, required)",
  "lifecycle": "enum (planned|active|deprecated|retired, required)",
  "capability_id": "string (uuid, nullable, FK to business_capabilities.id)",
  "data_classification": "enum (public|internal|confidential|restricted, required)",
  "tags": ["string"],
  "created_at": "string (ISO 8601 UTC with Z)",
  "updated_at": "string (ISO 8601 UTC with Z)",
  "deleted_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

**Validation Rules**:
- `name` is required, trimmed, 1-255 characters
- `owner` is required (team or individual identifier)
- `lifecycle` must be valid enum value, defaults to `planned`
- `data_classification` defaults to `internal`
- `tags` is optional array, supports empty array

**Database Indexes**:
- Primary key on `id`
- Index on `lifecycle` for filtering
- Index on `capability_id` for coverage queries
- Full-text index on `name` for search

---

#### ApplicationService

**Purpose**: Business service or capability that applications provide (what capability is offered)

**ArchiMate Element**: `ApplicationService`

**Schema**:
```json
{
  "id": "string (uuid)",
  "name": "string (1-255 chars, required)",
  "description": "string (0-2000 chars, nullable)",
  "business_capability_id": "string (uuid, nullable, FK to business_capabilities.id)",
  "sla": "string (0-500 chars, nullable)",
  "exposed_by_app_ids": ["string (uuid, FK to applications.id)"],
  "consumers": ["string (application id or team name)"],
  "tags": ["string"],
  "created_at": "string (ISO 8601 UTC with Z)",
  "updated_at": "string (ISO 8601 UTC with Z)",
  "deleted_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

**Validation Rules**:
- `name` is required, trimmed, 1-255 characters
- `description` is optional, max 2000 characters
- `exposed_by_app_ids` must reference existing applications
- `consumers` is optional array of identifiers

**Database Indexes**:
- Primary key on `id`
- Index on `business_capability_id` for capability queries
- Full-text index on `name` and `description`

---

#### ApplicationInterface

**Purpose**: Technical interface (API endpoint, message queue) through which applications expose services (where/how services are accessed)

**ArchiMate Element**: `ApplicationInterface`

**Schema**:
```json
{
  "id": "string (uuid)",
  "name": "string (1-255 chars, required)",
  "protocol": "string (1-50 chars, required, e.g., https, grpc, kafka)",
  "endpoint": "string (valid URL or connection string, required)",
  "specification_url": "string (valid HTTPS URL, nullable)",
  "version": "string (semver format, nullable)",
  "authentication_method": "string (1-50 chars, e.g., oauth2, api-key, mtls, nullable)",
  "exposed_by_app_id": "string (uuid, required, FK to applications.id)",
  "serves_service_ids": ["string (uuid, FK to application_services.id)"],
  "rate_limits": {
    "requests_per_second": "integer (positive)",
    "burst": "integer (positive)",
    "quotas": "object (flexible structure)"
  },
  "status": "enum (active|deprecated|retired, required)",
  "tags": ["string"],
  "created_at": "string (ISO 8601 UTC with Z)",
  "updated_at": "string (ISO 8601 UTC with Z)",
  "deleted_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

**Validation Rules**:
- `name` is required, trimmed, 1-255 characters
- `protocol` is required (e.g., https, grpc, kafka, sftp)
- `endpoint` is required, must be valid URL or connection string
- `specification_url` if provided must be valid HTTPS URL
- `version` if provided should follow semver format
- `exposed_by_app_id` is required, must reference existing application
- `serves_service_ids` must reference existing application services
- `rate_limits` values must be positive integers
- `status` defaults to `active`

**Database Indexes**:
- Primary key on `id`
- Index on `exposed_by_app_id` for app queries
- Index on `protocol` for filtering
- Index on `status` for lifecycle queries

---

#### Server

**Purpose**: Physical or virtual server, host, or compute instance

**ArchiMate Element**: `Node` (Technology Layer)

**Schema**:
```json
{
  "id": "string (uuid)",
  "hostname": "string (valid DNS hostname, required)",
  "environment": "enum (dev|staging|prod, required)",
  "region": "string (1-100 chars, nullable)",
  "platform": "string (1-50 chars, nullable, e.g., linux, windows, kubernetes)",
  "criticality": "enum (low|medium|high|critical, required)",
  "owning_team": "string (1-255 chars, nullable)",
  "tags": ["string"],
  "created_at": "string (ISO 8601 UTC with Z)",
  "updated_at": "string (ISO 8601 UTC with Z)",
  "deleted_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

**Validation Rules**:
- `hostname` is required, must be valid DNS hostname
- `environment` is required enum
- `criticality` defaults to `medium`
- `platform` is optional string

**Database Indexes**:
- Primary key on `id`
- Unique index on `hostname` (case-insensitive)
- Index on `environment` for filtering
- Index on `criticality` for risk queries

---

#### Integration

**Purpose**: Connection or data flow between applications

**ArchiMate Element**: `ApplicationInteraction` or `Flow`

**Schema**:
```json
{
  "id": "string (uuid)",
  "source_app_id": "string (uuid, required, FK to applications.id)",
  "target_app_id": "string (uuid, required, FK to applications.id)",
  "protocol": "string (1-50 chars, required)",
  "data_contract": "string (1-100 chars, nullable, e.g., json, xml, avro)",
  "sla": "string (0-500 chars, nullable)",
  "frequency": "string (1-50 chars, nullable, e.g., realtime, batch, hourly)",
  "tags": ["string"],
  "created_at": "string (ISO 8601 UTC with Z)",
  "updated_at": "string (ISO 8601 UTC with Z)",
  "deleted_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

**Validation Rules**:
- `source_app_id` and `target_app_id` are required, must reference existing applications
- `source_app_id` must differ from `target_app_id` (no self-loops)
- `protocol` is required
- `data_contract` is optional
- `frequency` is optional

**Database Indexes**:
- Primary key on `id`
- Index on `source_app_id` for outbound integration queries
- Index on `target_app_id` for inbound integration queries
- Compound index on `(source_app_id, target_app_id)` for duplicate detection

---

#### BusinessCapability

**Purpose**: Business capability or function independent of implementation

**ArchiMate Element**: `BusinessFunction` or `BusinessCapability`

**Schema**:
```json
{
  "id": "string (uuid)",
  "name": "string (1-255 chars, required)",
  "parent_id": "string (uuid, nullable, FK to business_capabilities.id)",
  "created_at": "string (ISO 8601 UTC with Z)",
  "updated_at": "string (ISO 8601 UTC with Z)",
  "deleted_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

**Validation Rules**:
- `name` is required, trimmed, 1-255 characters
- `parent_id` must reference existing capability, cannot create cycles

**Database Indexes**:
- Primary key on `id`
- Index on `parent_id` for hierarchy queries
- Index on `name` for search

---

#### DataEntity

**Purpose**: Significant data object, dataset, or data asset

**ArchiMate Element**: `DataObject` or `BusinessObject`

**Schema**:
```json
{
  "id": "string (uuid)",
  "name": "string (1-255 chars, required)",
  "domain": "string (1-100 chars, nullable)",
  "classification": "enum (public|internal|confidential|restricted, required)",
  "retention": "string (1-100 chars, nullable)",
  "owner": "string (1-255 chars, nullable)",
  "steward": "string (1-255 chars, nullable)",
  "source_system": "string (1-255 chars, nullable)",
  "criticality": "enum (low|medium|high|critical, required)",
  "pii_flag": "boolean (default false)",
  "glossary_terms": ["string"],
  "lineage": ["string (uuid, FK to data_entities.id)"],
  "created_at": "string (ISO 8601 UTC with Z)",
  "updated_at": "string (ISO 8601 UTC with Z)",
  "deleted_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

**Validation Rules**:
- `name` is required, trimmed, 1-255 characters
- `classification` defaults to `internal`
- `criticality` defaults to `medium`
- `pii_flag` defaults to `false`
- `lineage` must reference existing data entities

**Database Indexes**:
- Primary key on `id`
- Index on `classification` for compliance queries
- Index on `pii_flag` for GDPR queries
- Index on `source_system` for lineage queries
- Full-text index on `name` and `glossary_terms`

---

#### Relation

**Purpose**: Typed, directed relationship between entities with ArchiMate semantics and governance metadata

**Schema**:
```json
{
  "id": "string (uuid)",
  "source_id": "string (uuid, required)",
  "target_id": "string (uuid, required)",
  "source_type": "enum (organization|application|application_service|application_interface|server|integration|business_capability|data_entity|view, required)",
  "target_type": "enum (organization|application|application_service|application_interface|server|integration|business_capability|data_entity|view, required)",
  "relation_type": "enum (depends_on|communicates_with|calls|publishes_event_to|consumes_event_from|deployed_on|stores_data_on|reads|writes|owns|supports|implements|realizes|serves|connected_to|exposes|uses, required)",
  "archimate_element": "string (ArchiMate element type, nullable)",
  "archimate_relationship": "string (ArchiMate relationship type, nullable)",
  "description": "string (0-2000 chars, nullable)",
  "data_classification": "enum (public|internal|confidential|restricted, nullable)",
  "criticality": "enum (low|medium|high|critical, nullable)",
  "confidence": "float (0.0-1.0, default 1.0)",
  "evidence_source": "string (1-255 chars, nullable)",
  "last_verified_at": "string (ISO 8601 UTC with Z, nullable)",
  "effective_from": "string (ISO 8601 UTC with Z, nullable)",
  "effective_to": "string (ISO 8601 UTC with Z, nullable)",
  "label": "string (0-255 chars, nullable)",
  "color": "string (hex color #RRGGBB, nullable)",
  "style": "enum (solid|dashed, default solid)",
  "bidirectional": "boolean (default false)",
  "created_at": "string (ISO 8601 UTC with Z)",
  "updated_at": "string (ISO 8601 UTC with Z)",
  "deleted_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

**Validation Rules**:
- `source_id` and `target_id` are required
- `source_type` and `target_type` are required enums
- `relation_type` is required enum
- `source_id` and `target_id` must reference entities of corresponding types
- Relation type must be valid for the (source_type, target_type) pair (see validation matrix below)
- `confidence` must be float between 0.0 and 1.0
- `color` if provided must be valid hex color (#RRGGBB format)
- `effective_from` if provided must be <= `effective_to`

**Database Indexes**:
- Primary key on `id`
- Index on `source_id` for forward traversal
- Index on `target_id` for reverse traversal
- Compound index on `(source_type, relation_type)` for type-specific queries
- Compound index on `(target_type, relation_type)` for reverse type-specific queries
- Index on `effective_from` and `effective_to` for temporal queries

---

#### View

**Purpose**: Saved perspective or filtered view of the architecture

**Schema**:
```json
{
  "id": "string (uuid)",
  "name": "string (1-255 chars, required)",
  "description": "string (0-2000 chars, nullable)",
  "filter": {
    "capability_id": "string (uuid, nullable)",
    "lifecycle": ["enum (planned|active|deprecated|retired)"],
    "tags": ["string"],
    "owner": "string (nullable)",
    "environment": "enum (dev|staging|prod, nullable)"
  },
  "layout": {
    "engine": "string (e.g., dagre, elk, force)",
    "rankdir": "enum (TB|BT|LR|RL)",
    "spacing": "integer (positive)",
    "grouping": "object (flexible structure)"
  },
  "created_at": "string (ISO 8601 UTC with Z)",
  "updated_at": "string (ISO 8601 UTC with Z)",
  "deleted_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

**Validation Rules**:
- `name` is required, trimmed, 1-255 characters
- `filter` is optional JSON object
- `layout` is optional JSON object
- Filter enum values must match entity enum values

**Database Indexes**:
- Primary key on `id`
- Index on `name` for search

---

### Supporting Entities

#### ImportJob

```json
{
  "id": "string (uuid)",
  "status": "enum (pending|running|completed|failed, required)",
  "input_type": "string (e.g., csv, excel, json, s3, upload)",
  "created_by": "string (user id or email, required)",
  "created_at": "string (ISO 8601 UTC with Z)",
  "updated_at": "string (ISO 8601 UTC with Z)",
  "error": "string (error message if failed, nullable)"
}
```

#### ExportJob

```json
{
  "id": "string (uuid)",
  "status": "enum (pending|running|completed|failed, required)",
  "filter": "object (same as View filter structure, nullable)",
  "created_by": "string (user id or email, required)",
  "created_at": "string (ISO 8601 UTC with Z)",
  "updated_at": "string (ISO 8601 UTC with Z)",
  "download_url": "string (pre-signed URL, nullable)"
}
```

#### Webhook

```json
{
  "id": "string (uuid)",
  "url": "string (valid HTTPS URL, required)",
  "secret": "string (HMAC secret, write-only)",
  "active": "boolean (default true)",
  "events": ["string (event type, e.g., application.created, server.updated)"],
  "created_at": "string (ISO 8601 UTC with Z)",
  "updated_at": "string (ISO 8601 UTC with Z)",
  "last_failure_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

#### AuditLog

```json
{
  "id": "string (uuid)",
  "actor": "string (user id or email, required)",
  "action": "string (e.g., create, update, delete, required)",
  "entity_type": "enum (organization|application|..., required)",
  "entity_id": "string (uuid, required)",
  "source": "enum (ui|api|import|system, required)",
  "created_at": "string (ISO 8601 UTC with Z)",
  "metadata": "object (additional context like IP, changes, nullable)"
}
```

## 5. Acceptance Criteria

- **AC-001**: Given a valid entity payload, When POSTed to the API, Then the entity is created with generated UUID and timestamps
- **AC-002**: Given an entity with invalid enum value, When validated, Then a 400 error is returned with clear error message
- **AC-003**: Given a hierarchical entity (Organization, BusinessCapability), When parent_id creates a cycle, Then validation fails with 422 error
- **AC-004**: Given a relation with invalid (source_type, target_type, relation_type) combination, When created, Then validation fails per matrix rules
- **AC-005**: Given entities with timestamps, When returned by API, Then all timestamps are ISO 8601 UTC with 'Z' suffix
- **AC-006**: Given a soft-deleted entity, When queried by ID, Then 404 is returned (unless include_deleted=true parameter)
- **AC-007**: Given an Application with data_classification=confidential, When related DataEntity has no classification, Then relation inherits confidential classification
- **AC-008**: Given a Relation with confidence=0.5, When queried with min_confidence=0.7, Then relation is excluded from results
- **AC-009**: Given a DataEntity with pii_flag=true, When exported, Then PII notice is included in metadata
- **AC-010**: Given a valid ApplicationInterface, When status transitions from active to deprecated, Then consumers are notified via webhook

## 6. Test Automation Strategy

- **Test Levels**: Unit (domain validation), Integration (API endpoints + DB), End-to-End (full workflows)
- **Frameworks**: pytest for API integration tests, property-based testing for validation rules
- **Test Data Management**: Fixtures for common entity graphs, factory functions for valid entities, cleanup between tests
- **CI/CD Integration**: Run tests on every commit, block merge on failures, enforce 80%+ coverage
- **Coverage Requirements**: 80% line coverage minimum, 90% for domain validation logic
- **Contract Testing**: Validate all API responses against OpenAPI schema using openapi-core or similar
- **Performance Testing**: Load test relation queries with 10K+ nodes, measure p95 latency <500ms

## 7. Rationale & Context

### Design Decisions

**Why UUIDs over sequential IDs?**
- Distributed generation without coordination
- No information leakage about record count
- Easier federation across systems

**Why soft deletes?**
- Audit trail preservation
- Enable "undo" functionality
- Support temporal queries (what existed at time T?)

**Why separate Application, ApplicationService, and ApplicationInterface?**
- Aligns with ArchiMate 3.2 layered architecture
- Separates business perspective (service) from technical perspective (interface)
- Enables service-oriented views independent of implementation
- Supports multiple applications realizing same service
- Supports multiple interfaces exposing same service (REST + gRPC)

**Why store ArchiMate metadata on relations?**
- Enables standards-compliant diagram rendering
- Supports export to ArchiMate tools (Archi, BiZZdesign)
- Provides semantic richness beyond simple graph edges

**Why confidence and evidence_source fields?**
- Not all relations are equally certain (discovered vs manual)
- Support gradual improvement of architecture model quality
- Enable filtering by confidence for high-assurance views

**Why temporal validity (effective_from/to)?**
- Model planned changes before they occur
- Preserve historical architecture states
- Support time-travel queries for impact analysis

### ArchiMate Alignment Rationale

The EA Tool domain model is deliberately aligned with ArchiMate 3.2 to:
1. Provide a standard vocabulary understood by enterprise architects
2. Enable interoperability with existing ArchiMate tools
3. Support consistent visualization across tools and teams
4. Leverage 15+ years of ArchiMate best practices and patterns

Key mappings:
- Organization → BusinessActor/BusinessRole (who)
- BusinessCapability → BusinessFunction (what business does)
- ApplicationService → ApplicationService (what capability is provided)
- Application → ApplicationComponent (software implementation)
- ApplicationInterface → ApplicationInterface (technical access point)
- DataEntity → DataObject (information assets)
- Server → Node (infrastructure)

## 8. Dependencies & External Integrations

### External Systems
- **EXT-001**: OIDC Provider - Authentication and identity management; must support standard OIDC discovery and JWT tokens

### Third-Party Services
- **SVC-001**: Open Policy Agent (OPA) - Authorization policy evaluation; requires Rego policy language support; SLA: <50ms p95 latency for authorization decisions

### Infrastructure Dependencies
- **INF-001**: Database - Relational database with JSONB support, transactions, and foreign keys; SQLite for dev, PostgreSQL or MSSQL for staging/prod
- **INF-002**: Container Platform - Docker-compatible runtime for API deployment

### Data Dependencies
- **DAT-001**: CMDB (optional) - Configuration Management Database for periodic synchronization of asset data; format: REST API or CSV export
- **DAT-002**: ITSM (optional) - IT Service Management system for incident/change integration; format: Webhook callbacks

### Technology Platform Dependencies
- **PLT-001**: .NET 10 Runtime - F# application runtime; minimum version 10.0
- **PLT-002**: OpenAPI 3.0.3 - API contract specification format

### Compliance Dependencies
- **COM-001**: GDPR - Personal data handling requires PII flagging and data subject access support
- **COM-002**: ArchiMate 3.2 - Modeling standard alignment for interoperability

## 9. Examples & Edge Cases

### Example: Creating Application with full lifecycle

```json
POST /applications
{
  "name": "Billing API",
  "owner": "payments-team",
  "lifecycle": "planned",
  "capability_id": "cap-payments-uuid",
  "data_classification": "confidential",
  "tags": ["payments", "pci-dss", "critical"]
}

Response 201:
{
  "id": "app-123e4567-e89b-12d3-a456-426614174000",
  "name": "Billing API",
  "owner": "payments-team",
  "lifecycle": "planned",
  "capability_id": "cap-payments-uuid",
  "data_classification": "confidential",
  "tags": ["payments", "pci-dss", "critical"],
  "created_at": "2026-01-06T10:00:00Z",
  "updated_at": "2026-01-06T10:00:00Z"
}
```

### Example: Hierarchical Organizations

```json
// Parent organization
{
  "id": "org-parent",
  "name": "Engineering Division",
  "parent_id": null
}

// Child organization
{
  "id": "org-child",
  "name": "Payments Team",
  "parent_id": "org-parent"  // References parent
}

// Invalid: cycle detection
{
  "id": "org-parent",
  "parent_id": "org-child"  // Would create cycle - validation fails
}
```

### Example: Service Realization Pattern (ArchiMate)

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
  "capability_id": "cap-payments-uuid"
}
// Returns: app-billing-uuid

// Step 4: Create ApplicationInterface
POST /application-interfaces
{
  "name": "Payment REST API v2",
  "protocol": "https",
  "endpoint": "https://api.example.com/v2/payments",
  "exposed_by_app_id": "app-billing-uuid",
  "serves_service_ids": ["svc-payment-uuid"]
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

### Edge Case: Temporal Relations

```json
// Relation valid only in future (planned migration)
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

// Query: GET /relations?effective_at=2026-02-01T00:00:00Z
// Returns: empty (not yet effective)

// Query: GET /relations?effective_at=2026-03-15T00:00:00Z
// Returns: relation (now effective)
```

### Edge Case: Data Classification Inheritance

```json
// Application with confidential classification
POST /applications
{
  "name": "HR System",
  "data_classification": "confidential"
}
// Returns: app-hr-uuid

// Relation to data entity without explicit classification
POST /relations
{
  "source_id": "app-hr-uuid",
  "target_id": "data-employee-uuid",
  "source_type": "application",
  "target_type": "data_entity",
  "relation_type": "writes",
  "data_classification": null  // Inherits from application → confidential
}
```

## 10. Validation Criteria

### Schema Validation
- All API requests/responses MUST validate against OpenAPI schema
- All database writes MUST enforce foreign key constraints
- All enum values MUST match defined enums (case-sensitive)

### Business Rule Validation
- Hierarchical cycles MUST be prevented (organizations, capabilities)
- Lifecycle transitions MUST follow valid paths
- Self-referencing relations MUST be rejected (source_id == target_id)
- Relation types MUST be valid for entity type pairs (see validation matrix)

### Validation Matrix for Relations

Valid (source_type, target_type, relation_type) combinations:

| Source Type | Target Type | Valid Relation Types |
|------------|-------------|---------------------|
| application | server | deployed_on, stores_data_on |
| application | business_capability | supports |
| application | data_entity | reads, writes |
| application | application_service | realizes, uses |
| application | application_interface | exposes |
| application | application | depends_on, communicates_with, calls |
| application_interface | application_service | serves |
| application_service | business_capability | realizes, supports |
| integration | application | communicates_with, publishes_event_to, consumes_event_from |
| organization | application | owns |
| organization | server | owns |
| organization | organization | parent-child (via parent_id, not relation) |
| server | server | connected_to |

All other combinations MUST be rejected with 422 validation error.

## 11. Related Specifications / Further Reading

- [spec-tool-api-contract.md](spec-tool-api-contract.md) - Complete API specification with endpoints, authentication, and error handling
- [spec-architecture-system-design.md](spec-architecture-system-design.md) - System architecture and component design
- [spec-process-authorization.md](spec-process-authorization.md) - Authorization model with OPA/Rego policies
- [ArchiMate 3.2 Specification](https://pubs.opengroup.org/architecture/archimate3-doc/) - Official ArchiMate standard
- [OpenAPI 3.0.3 Specification](https://spec.openapis.org/oas/v3.0.3) - API contract format
- [RFC 3339 Date/Time](https://tools.ietf.org/html/rfc3339) - ISO 8601 timestamp format
- [RFC 5322 Email Address](https://tools.ietf.org/html/rfc5322) - Email validation format
