---
title: EA Tool Domain Model Overview
version: 1.0
date_created: 2026-01-06
last_updated: 2026-01-06
owner: EA Platform Team
tags: [schema, domain, overview, requirements, archimate]
---

# Introduction

This specification provides a high-level overview of the EA Tool domain model, including core requirements, constraints, design guidelines, and rationale. It serves as the foundation for all detailed entity specifications.

## 1. Purpose & Scope

**Purpose**: Establish the foundational principles, requirements, and constraints for the EA Tool domain model to ensure consistent implementation across all entities and components.

**Scope**:
- Core domain modeling principles
- Global requirements and constraints
- Design guidelines and patterns
- ArchiMate 3.2 alignment strategy
- Rationale for key design decisions

**Out of Scope**: Detailed entity schemas (see entity-specific specifications)

**Audience**: Architects, backend developers, database designers, and AI agents implementing the domain model.

**Assumptions**:
- All timestamps are UTC in ISO 8601 format with 'Z' suffix
- UUIDs (version 4) are used for primary keys
- JSON fields use standard JSON types
- All text fields are Unicode (UTF-8)

## 2. Definitions

- **Entity**: A domain object with unique identity and lifecycle (e.g., Application, Server)
- **Relation**: A typed, directed edge between two entities with semantic meaning
- **ArchiMate Element**: A concept from ArchiMate 3.2 standard (e.g., ApplicationComponent)
- **ArchiMate Relationship**: A connection type from ArchiMate 3.2 (e.g., Realization, Serving)
- **FK**: Foreign Key - reference to another entity's primary key
- **JSONB**: JSON Binary - efficient storage/indexing of JSON data in PostgreSQL/MSSQL
- **UTC**: Coordinated Universal Time - all timestamps must be in UTC
- **Lifecycle**: The current phase of an asset (planned, active, deprecated, retired)
- **PII**: Personally Identifiable Information
- **Soft Delete**: Marking records as deleted without physical removal

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

### Common Fields

All entities share these base fields:

```json
{
  "id": "string (uuid, required)",
  "created_at": "string (ISO 8601 UTC with Z, required)",
  "updated_at": "string (ISO 8601 UTC with Z, required)",
  "deleted_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

### Enum Definitions

#### Lifecycle
```
planned | active | deprecated | retired
```

#### Data Classification
```
public | internal | confidential | restricted
```

#### Criticality
```
low | medium | high | critical
```

#### Environment
```
dev | staging | prod
```

#### Entity Type
```
organization | application | application_service | application_interface | 
server | integration | business_capability | data_entity | view
```

## 5. Acceptance Criteria

- **AC-001**: Given any entity creation, When saved to database, Then `id`, `created_at`, and `updated_at` are automatically generated
- **AC-002**: Given an entity with invalid enum value, When validated, Then a clear error message with valid options is returned
- **AC-003**: Given a hierarchical entity, When parent_id creates a cycle, Then validation fails before database write
- **AC-004**: Given any timestamp field, When returned by API, Then it is formatted as ISO 8601 UTC with 'Z' suffix
- **AC-005**: Given a soft-deleted entity, When queried without include_deleted flag, Then 404 is returned

## 6. Test Automation Strategy

- **Test Levels**: Unit (validation), Integration (database), Contract (OpenAPI)
- **Frameworks**: pytest for Python integration tests, property-based testing for validation rules
- **Test Data Management**: Factory functions for valid entities, fixtures for common scenarios
- **CI/CD Integration**: Run tests on every commit, block merge on failures
- **Coverage Requirements**: 90% for validation logic, 80% overall

## 7. Rationale & Context

### Why UUIDs over sequential IDs?
- Distributed generation without coordination
- No information leakage about record count
- Easier federation across systems
- Prevents enumeration attacks

### Why soft deletes?
- Audit trail preservation
- Enable "undo" functionality
- Support temporal queries (what existed at time T?)
- Compliance with data retention policies

### Why separate Application, ApplicationService, and ApplicationInterface?
- Aligns with ArchiMate 3.2 layered architecture
- Separates business perspective (service) from technical perspective (interface)
- Enables service-oriented views independent of implementation
- Supports multiple applications realizing same service
- Supports multiple interfaces exposing same service (REST + gRPC)

### Why store ArchiMate metadata on relations?
- Enables standards-compliant diagram rendering
- Supports export to ArchiMate tools (Archi, BiZZdesign)
- Provides semantic richness beyond simple graph edges
- Facilitates communication with enterprise architects

### Why confidence and evidence_source fields?
- Not all relations are equally certain (discovered vs manual)
- Support gradual improvement of architecture model quality
- Enable filtering by confidence for high-assurance views
- Track provenance for audit and governance

### Why temporal validity (effective_from/to)?
- Model planned changes before they occur
- Preserve historical architecture states
- Support time-travel queries for impact analysis
- Enable what-if scenario modeling

### ArchiMate Alignment Rationale

The EA Tool domain model is deliberately aligned with ArchiMate 3.2 to:
1. Provide a standard vocabulary understood by enterprise architects
2. Enable interoperability with existing ArchiMate tools
3. Support consistent visualization across tools and teams
4. Leverage 15+ years of ArchiMate best practices and patterns

**Key mappings:**
- Organization → BusinessActor/BusinessRole (who)
- BusinessCapability → BusinessFunction (what business does)
- ApplicationService → ApplicationService (what capability is provided)
- Application → ApplicationComponent (software implementation)
- ApplicationInterface → ApplicationInterface (technical access point)
- DataEntity → DataObject (information assets)
- Server → Node (infrastructure)

## 8. Dependencies & External Integrations

### Technology Platform Dependencies
- **PLT-001**: .NET 10 Runtime - F# application runtime; minimum version 10.0
- **PLT-002**: OpenAPI 3.0.3 - API contract specification format

### Compliance Dependencies
- **COM-001**: GDPR - Personal data handling requires PII flagging and data subject access support
- **COM-002**: ArchiMate 3.2 - Modeling standard alignment for interoperability

## 9. Examples & Edge Cases

See [Schema Examples](spec-schema-examples.md) for detailed examples.

## 10. Validation Criteria

### Schema Validation
- All API requests/responses MUST validate against OpenAPI schema
- All database writes MUST enforce foreign key constraints
- All enum values MUST match defined enums (case-sensitive)

### Business Rule Validation
- Hierarchical cycles MUST be prevented
- Lifecycle transitions MUST follow valid paths
- Self-referencing relations MUST be rejected
- Relation types MUST be valid for entity type pairs

## 11. Related Specifications

- **[Specification Index](spec-index.md)** - All specifications overview
- **[Business Layer Entities](spec-schema-entities-business.md)** - Organization, BusinessCapability
- **[Application Layer Entities](spec-schema-entities-application.md)** - Application, ApplicationService, ApplicationInterface
- **[Infrastructure Entities](spec-schema-entities-infrastructure.md)** - Server, Integration
- **[Data Entities](spec-schema-entities-data.md)** - DataEntity
- **[Meta Entities](spec-schema-entities-meta.md)** - Relation, View
- **[Validation Rules](spec-schema-validation.md)** - Validation matrix and rules
- **[Schema Examples](spec-schema-examples.md)** - Working examples

### External References
- [ArchiMate 3.2 Specification](https://pubs.opengroup.org/architecture/archimate3-doc/)
- [RFC 3339 Date/Time](https://tools.ietf.org/html/rfc3339) - ISO 8601 format
- [RFC 5322 Email Address](https://tools.ietf.org/html/rfc5322)
