---
title: Meta Entities
version: 1.0
date_created: 2026-01-06
last_updated: 2026-01-06
owner: EA Platform Team
tags: [schema, entities, meta, relation, view]
---

# Introduction

This specification defines meta entities (Relation, View) that model relationships between entities and saved architectural views.

## 1. Purpose & Scope

**Purpose**: Define schemas for relations and views.

**Scope**:
- Relation entity (typed edges between entities)
- View entity (saved filtered perspectives)

**Audience**: Backend developers, architects.

## 4. Interfaces & Data Contracts

### Relation

**Purpose**: Typed, directed relationship between entities with ArchiMate semantics.

**Schema**:
```json
{
  "id": "string (uuid)",
  "source_id": "string (uuid, required)",
  "target_id": "string (uuid, required)",
  "source_type": "enum (required)",
  "target_type": "enum (required)",
  "relation_type": "enum (required)",
  "archimate_element": "string (nullable)",
  "archimate_relationship": "string (nullable)",
  "description": "string (0-2000 chars, nullable)",
  "data_classification": "enum (nullable)",
  "criticality": "enum (nullable)",
  "confidence": "float (0.0-1.0, default 1.0)",
  "evidence_source": "string (nullable)",
  "last_verified_at": "string (ISO 8601 UTC with Z, nullable)",
  "effective_from": "string (ISO 8601 UTC with Z, nullable)",
  "effective_to": "string (ISO 8601 UTC with Z, nullable)",
  "label": "string (0-255 chars, nullable)",
  "color": "string (hex #RRGGBB, nullable)",
  "style": "enum (solid|dashed, default solid)",
  "bidirectional": "boolean (default false)",
  "created_at": "string (ISO 8601 UTC with Z)",
  "updated_at": "string (ISO 8601 UTC with Z)",
  "deleted_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

### View

**Purpose**: Saved perspective or filtered view of the architecture.

**Schema**:
```json
{
  "id": "string (uuid)",
  "name": "string (1-255 chars, required)",
  "description": "string (0-2000 chars, nullable)",
  "filter": {
    "capability_id": "string (uuid, nullable)",
    "lifecycle": ["enum"],
    "tags": ["string"],
    "owner": "string (nullable)",
    "environment": "enum (nullable)"
  },
  "layout": {
    "engine": "string",
    "rankdir": "enum (TB|BT|LR|RL)",
    "spacing": "integer",
    "grouping": "object"
  },
  "created_at": "string (ISO 8601 UTC with Z)",
  "updated_at": "string (ISO 8601 UTC with Z)",
  "deleted_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

## 11. Related Specifications

- **[Specification Index](spec-index.md)**
- **[Validation Rules](spec-schema-validation.md)** - Relation validation matrix
