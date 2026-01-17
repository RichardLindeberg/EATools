---
title: Validation Rules and Matrix
version: 1.0
date_created: 2026-01-06
last_updated: 2026-01-06
owner: EA Platform Team
tags: [schema, validation, rules, matrix]
---

# Introduction

This specification defines all validation rules including the relation validation matrix specifying valid combinations of entity types and relation types.

## 1. Purpose & Scope

**Purpose**: Define comprehensive validation rules for all entities and relationships.

**Scope**:
- Relation validation matrix
- Field-level validation rules
- Business rule validation
- Acceptance criteria

**Audience**: Backend developers, QA engineers, API implementers.

## 3. Relation Validation Matrix

Valid (source_type, target_type, relation_type) combinations:

### Comprehensive Validation Matrix

| Source Type | Target Type | Valid Relation Types | ArchiMate Mapping | Notes |
|------------|-------------|---------------------|------------------|-------|
| application | application | depends_on, communicates_with, calls | Serving, Flow | Direct application communication |
| application | application_service | realizes, uses | Realization, Serving | App implements or uses service |
| application | application_interface | exposes | Composition, Assignment | App exposes interface for access |
| application | server | deployed_on, stores_data_on | Assignment | Infrastructure deployment |
| application | data_entity | reads, writes | Access | Data access patterns |
| application | business_capability | supports | Realization, Serving | Business capability realization |
| application_interface | application_service | serves | Serving | Interface provides service |
| application_service | business_capability | realizes, supports | Realization, Serving | Service realizes capability |
| integration | application | communicates_with, publishes_event_to, consumes_event_from | Triggering, Flow | Integration patterns |
| integration | application_interface | calls | Serving | Integration calls interface |
| organization | application | owns | Composition, Assignment | Ownership/stewardship |
| organization | server | owns | Composition, Assignment | Infrastructure ownership |
| organization | organization | part_of | Aggregation, Composition | Organizational hierarchy via relations |
| server | server | connected_to | Association | Network/physical connectivity |

**All other combinations MUST be rejected with 422 validation error.**

### Hierarchical Entities

The following hierarchies are modeled via `parent_id` field (NOT via relations):
- Organization hierarchy: Use `parent_id` field on organization entity for efficient queries
- Business Capability hierarchy: Use `parent_id` field on business_capability entity for tree navigation

Relations may also be used for these hierarchies when additional governance metadata is needed (confidence, effective dates, custom labels).

### Relation Type Semantics

| Relation Type | Direction | Meaning |
|---------------|-----------|---------|
| `depends_on` | Unidirectional | Source requires target to function |
| `communicates_with` | Typically Bidirectional | Applications communicate with each other |
| `calls` | Unidirectional | Source invokes target |
| `realizes` | Unidirectional | Source implements/realizes target |
| `uses` | Unidirectional | Source uses target service/interface |
| `exposes` | Unidirectional | Source makes target available |
| `serves` | Unidirectional | Source provides/serves target |
| `supports` | Unidirectional | Source supports target capability |
| `deployed_on` | Unidirectional | Source is deployed/runs on target |
| `stores_data_on` | Unidirectional | Source stores data on target |
| `reads` | Unidirectional | Source reads target data |
| `writes` | Unidirectional | Source writes target data |
| `owns` | Unidirectional | Source owns/stewards target |
| `connected_to` | Typically Bidirectional | Source connected to target (network/physical) |
| `publishes_event_to` | Unidirectional | Source publishes events to target |
| `consumes_event_from` | Unidirectional | Source consumes events from target |
| `part_of` | Unidirectional | Source is part of target organization |

## 4. Field Validation Rules

### String Fields
- Names: 1-255 characters, trimmed, non-empty
- Descriptions: 0-2000 characters
- URLs: Valid HTTP/HTTPS format
- Hostnames: Valid DNS hostname format
- Emails: RFC 5322 compliant

### Enum Fields
- Lifecycle: planned, active, deprecated, retired
- Data Classification: public, internal, confidential, restricted
- Criticality: low, medium, high, critical
- Environment: dev, staging, prod

### Numeric Fields
- Confidence: float 0.0-1.0 inclusive
- Rate limits: positive integers

### Temporal Fields
- All timestamps: ISO 8601 UTC with 'Z' suffix
- effective_from ≤ effective_to when both present

## 5. Business Rules

### Hierarchical Entities
- MUST prevent circular references
- MUST validate parent existence
- MUST handle orphan prevention on parent deletion

### Lifecycle Transitions
- Valid paths: planned → active → deprecated → retired
- Cannot transition backwards (e.g., active → planned)

### Data Classification Inheritance
- Relations inherit classification from entities unless explicitly set

### Soft Delete
- Deleted entities return 404 unless include_deleted=true
- Deleted entities maintain referential integrity

## 6. Acceptance Criteria

- **AC-VAL-001**: Given invalid (source_type, target_type, relation_type), When creating relation, Then 422 error with matrix reference
- **AC-VAL-002**: Given cycle in organization hierarchy, When updating parent_id, Then 422 error before commit
- **AC-VAL-003**: Given lifecycle transition active→planned, When updated, Then 422 error with valid transitions
- **AC-VAL-004**: Given confidence=1.5, When validated, Then 422 error (must be 0.0-1.0)
- **AC-VAL-005**: Given email "invalid", When validated, Then 422 error with RFC 5322 reference

## 11. Related Specifications

- **[Specification Index](spec-index.md)**
- **[Domain Model Overview](spec-schema-domain-overview.md)**
- **[Business Layer Entities](spec-schema-entities-business.md)**
- **[Application Layer Entities](spec-schema-entities-application.md)**
- **[Schema Examples](spec-schema-examples.md)**
