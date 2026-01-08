---
title: EA Tool Specifications Index
version: 1.0
date_created: 2026-01-06
last_updated: 2026-01-08
owner: EA Platform Team
tags: [index, overview, specifications]
---

# EA Tool Specifications Index

This document serves as the central index for all EA Tool specifications. All specifications follow a consistent template structure and are designed for both human developers and AI agents.

## Overview

The EA Tool is an API-first enterprise architecture management platform that catalogs IT assets and visualizes them using ArchiMate 3.2 standards. These specifications define the complete system from domain models to API contracts to operational processes.

## Specification Categories

### Domain & Data Model

- **[Domain Model Overview](spec-schema-domain-overview.md)** - High-level domain model, requirements, constraints, guidelines, and rationale
- **[Implementation Status](spec-implementation-status.md)** - Spec vs code alignment tracker, gap analysis, and priorities
- **[Business Layer Entities](spec-schema-entities-business.md)** - Organization and BusinessCapability schemas
- **[Application Layer Entities](spec-schema-entities-application.md)** - Application, ApplicationService, and ApplicationInterface schemas
- **[Infrastructure Entities](spec-schema-entities-infrastructure.md)** - Server and Integration schemas
- **[Data Entities](spec-schema-entities-data.md)** - DataEntity schema and data governance
- **[Meta Entities](spec-schema-entities-meta.md)** - Relation and View schemas
- **[Supporting Entities](spec-schema-entities-supporting.md)** - ImportJob, ExportJob, Webhook, and AuditLog schemas
- **[Validation Rules](spec-schema-validation.md)** - Validation matrix, business rules, and acceptance criteria
- **[Schema Examples](spec-schema-examples.md)** - Complete examples, patterns, and edge cases

### API & Contracts

- **[API Contract](spec-tool-api-contract.md)** - REST API endpoints, request/response patterns, error handling, pagination
- **[API Versioning](spec-tool-api-versioning.md)** *(planned)* - Versioning strategy, deprecation policy, breaking changes
- **[Error Handling](spec-tool-error-handling.md)** - Error codes, structures, trace IDs, rate limiting, client guidance
- **[Query Patterns](spec-tool-query-patterns.md)** - Pagination, filtering, sorting, search, soft-delete toggles, response envelopes

### Architecture & Design

- **[Event Sourcing & Command-Based Architecture](spec-architecture-event-sourcing.md)** - Command design, event store, CQRS, audit requirements, prevention of fat events
- **[System Architecture](spec-architecture-system-design.md)** *(planned)* - Component design, deployment, infrastructure
- **[Data Architecture](spec-architecture-data.md)** - Database schema, migrations, indexing strategy, event store, projections
- **[Integration Architecture](spec-architecture-integration.md)** *(planned)* - External systems, webhooks, SDK generation

### Security & Authorization

- **[Authorization Model](spec-process-authorization.md)** - OPA/Rego policies, RBAC/ABAC, obligations, fail-closed decision flows
- **[Authentication](spec-process-authentication.md)** - OIDC integration, API keys, token management
- **[Security Controls](spec-process-security.md)** *(planned)* - Data classification, PII handling, audit logging

### Processes & Operations

- **[Monitoring & Observability](spec-process-observability.md)** - OpenTelemetry compliance, structured logging, metrics, distributed tracing, alerting
- **[Data Import/Export](spec-process-import-export.md)** *(planned)* - Bulk operations, CSV/Excel formats, job management
- **[Webhook Events](spec-process-webhooks.md)** *(planned)* - Event types, payload formats, retry logic

## Quick Reference

### For Backend Developers
Start with:
1. [Domain Model Overview](spec-schema-domain-overview.md) - Understand requirements and constraints
2. [Implementation Status](spec-implementation-status.md) - See what's implemented vs specified
3. [Entity Schemas](spec-schema-entities-application.md) - Core entity definitions
4. [Validation Rules](spec-schema-validation.md) - Business rules and validation matrix

### For API Consumers
Start with:
1. [API Contract](spec-tool-api-contract.md) - Endpoints, request/response patterns, authentication
2. [Schema Examples](spec-schema-examples.md) - Working examples
3. [Authorization Model](spec-process-authorization.md) - Access control patterns

### For Frontend Developers
Start with:
1. [API Contract](spec-tool-api-contract.md) - REST API usage, error handling, pagination
2. [Schema Examples](spec-schema-examples.md) - Request/response patterns
3. [Application Layer Entities](spec-schema-entities-application.md) - Data structures

### For Integration Partners
Start with:
1. [API Contract](spec-tool-api-contract.md) - Integration points, authentication methods
2. [Authentication](spec-process-authentication.md) - API key management
3. [Webhook Events](spec-process-webhooks.md) - Event subscriptions

## Cross-Cutting Concerns

### Time Handling
- **All timestamps MUST be UTC in ISO 8601 format with 'Z' suffix**
- Example: `2026-01-06T10:00:00Z`
- See: [Domain Model Overview](spec-schema-domain-overview.md#time-handling)

### Identifiers
- **All primary keys are UUIDv4**
- Example: `app-123e4567-e89b-12d3-a456-426614174000`
- See: [Domain Model Overview](spec-schema-domain-overview.md#identifiers)

### Data Classification
- **Levels**: `public`, `internal`, `confidential`, `restricted`
- **Inheritance**: Relations inherit classification from entities
- See: [Security Controls](spec-process-security.md#data-classification)

### ArchiMate Alignment
- **Standard**: ArchiMate 3.2
- **Layers**: Business, Application, Technology
- See: [Domain Model Overview](spec-schema-domain-overview.md#archimate-alignment)

## Documentation Standards

All specifications follow this template structure:

1. **Front Matter** - Metadata (title, version, dates, owner, tags)
2. **Introduction** - Purpose and scope
3. **Purpose & Scope** - Clear definition of coverage
4. **Definitions** - Domain terminology
5. **Requirements, Constraints & Guidelines** - Explicit rules (REQ-*, SEC-*, CON-*, GUD-*, PAT-*)
6. **Interfaces & Data Contracts** - Schemas and APIs
7. **Acceptance Criteria** - Testable conditions (AC-*)
8. **Test Automation Strategy** - Testing approach
9. **Rationale & Context** - Design decisions explained
10. **Dependencies & External Integrations** - External requirements (EXT-*, SVC-*, INF-*, DAT-*, PLT-*, COM-*)
11. **Examples & Edge Cases** - Working code/data samples
12. **Validation Criteria** - Compliance checks
13. **Related Specifications** - Cross-references

## Change Management

### Versioning
- Specifications use semantic versioning (major.minor)
- Breaking changes increment major version
- Clarifications and additions increment minor version

### Review Process
1. Draft specification created
2. Technical review by team
3. Implementation validation
4. Published as official spec

### Maintenance
- Specifications reviewed quarterly
- Updated when implementation changes
- Deprecated specs marked clearly

## Related Documentation

- [System Overview](../docs/system-overview.md) - High-level system introduction
- [Entity Guide](../docs/entity-guide.md) - User-facing entity documentation
- [ArchiMate Alignment](../docs/archimate-alignment.md) - ArchiMate mapping guide
- [Relationship Modeling](../docs/relationship-modeling.md) - Relationship patterns
- [OpenAPI Specification](../openapi.yaml) - Machine-readable API contract

## Questions or Feedback

For questions about these specifications:
- Review the [Domain Model Overview](spec-schema-domain-overview.md) for high-level context
- Check [Schema Examples](spec-schema-examples.md) for practical usage
- Consult [Validation Rules](spec-schema-validation.md) for specific constraints
- Refer to related specs in each document's "Related Specifications" section

---

**Last Updated**: 2026-01-06  
**Maintained By**: EA Platform Team  
**Status**: Living Document
