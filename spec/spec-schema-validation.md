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
| server | server | connected_to |

**All other combinations MUST be rejected with 422 validation error.**

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
