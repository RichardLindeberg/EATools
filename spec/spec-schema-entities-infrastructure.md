---
title: Infrastructure Entities
version: 1.0
date_created: 2026-01-06
last_updated: 2026-01-06
owner: EA Platform Team
tags: [schema, entities, infrastructure, server, integration]
---

# Introduction

This specification defines Infrastructure entities including Technology Layer (Server) and Integration entities for modeling IT infrastructure and application interconnections.

## 1. Purpose & Scope

**Purpose**: Define schemas for infrastructure and integration entities.

**Scope**:
- Server entity (technology infrastructure)
- Integration entity (application connections)

**Audience**: Infrastructure architects, network engineers, integration specialists.

## 4. Interfaces & Data Contracts

### Server

**ArchiMate Element**: `Node`
**Layer**: Technology

**Schema**:
```json
{
  "id": "string (uuid)",
  "hostname": "string (valid DNS hostname, required)",
  "environment": "enum (dev|staging|prod, required)",
  "region": "string (1-100 chars, nullable)",
  "platform": "string (1-50 chars, nullable)",
  "criticality": "enum (low|medium|high|critical, required)",
  "owning_team": "string (1-255 chars, nullable)",
  "tags": ["string"],
  "created_at": "string (ISO 8601 UTC with Z)",
  "updated_at": "string (ISO 8601 UTC with Z)",
  "deleted_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

### Integration

**ArchiMate Element**: `ApplicationInteraction` or `Flow`
**Layer**: Application

**Schema**:
```json
{
  "id": "string (uuid)",
  "source_app_id": "string (uuid, required, FK)",
  "target_app_id": "string (uuid, required, FK)",
  "protocol": "string (1-50 chars, required)",
  "data_contract": "string (1-100 chars, nullable)",
  "sla": "string (0-500 chars, nullable)",
  "frequency": "string (1-50 chars, nullable)",
  "tags": ["string"],
  "created_at": "string (ISO 8601 UTC with Z)",
  "updated_at": "string (ISO 8601 UTC with Z)",
  "deleted_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

## 11. Related Specifications

- **[Specification Index](spec-index.md)**
- **[Domain Model Overview](spec-schema-domain-overview.md)**
- **[Application Layer Entities](spec-schema-entities-application.md)**
