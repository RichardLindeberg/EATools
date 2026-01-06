---
title: Data Entities
version: 1.0
date_created: 2026-01-06
last_updated: 2026-01-06
owner: EA Platform Team
tags: [schema, entities, data, data-governance]
---

# Introduction

This specification defines DataEntity for modeling significant data assets with data governance metadata.

## 1. Purpose & Scope

**Purpose**: Define schema for data entities and data governance.

**Scope**: DataEntity schema with PII, classification, lineage

**Audience**: Data architects, data stewards, compliance officers.

## 4. Interfaces & Data Contracts

### DataEntity

**ArchiMate Element**: `DataObject` or `BusinessObject`
**Layer**: Application/Business

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

**Relationships**:
- `application` → `data_entity` (reads, writes)

## 11. Related Specifications

- **[Specification Index](spec-index.md)**
- **[Domain Model Overview](spec-schema-domain-overview.md)**
