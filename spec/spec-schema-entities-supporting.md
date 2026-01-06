---
title: Supporting Entities
version: 1.0
date_created: 2026-01-06
last_updated: 2026-01-06
owner: EA Platform Team
tags: [schema, entities, supporting, import, export, webhook, audit]
---

# Introduction

This specification defines supporting entities for operational concerns: import/export jobs, webhooks, and audit logging.

## 1. Purpose & Scope

**Purpose**: Define schemas for supporting operational entities.

**Scope**:
- ImportJob (async bulk imports)
- ExportJob (async exports)
- Webhook (event subscriptions)
- AuditLog (change tracking)

**Audience**: Backend developers, operations engineers.

## 4. Interfaces & Data Contracts

### ImportJob

```json
{
  "id": "string (uuid)",
  "status": "enum (pending|running|completed|failed, required)",
  "input_type": "string",
  "created_by": "string (required)",
  "created_at": "string (ISO 8601 UTC with Z)",
  "updated_at": "string (ISO 8601 UTC with Z)",
  "error": "string (nullable)"
}
```

### ExportJob

```json
{
  "id": "string (uuid)",
  "status": "enum (pending|running|completed|failed, required)",
  "filter": "object (nullable)",
  "created_by": "string (required)",
  "created_at": "string (ISO 8601 UTC with Z)",
  "updated_at": "string (ISO 8601 UTC with Z)",
  "download_url": "string (nullable)"
}
```

### Webhook

```json
{
  "id": "string (uuid)",
  "url": "string (valid HTTPS URL, required)",
  "secret": "string (write-only)",
  "active": "boolean (default true)",
  "events": ["string"],
  "created_at": "string (ISO 8601 UTC with Z)",
  "updated_at": "string (ISO 8601 UTC with Z)",
  "last_failure_at": "string (ISO 8601 UTC with Z, nullable)"
}
```

### AuditLog

```json
{
  "id": "string (uuid)",
  "actor": "string (required)",
  "action": "string (required)",
  "entity_type": "enum (required)",
  "entity_id": "string (uuid, required)",
  "source": "enum (ui|api|import|system, required)",
  "created_at": "string (ISO 8601 UTC with Z)",
  "metadata": "object (nullable)"
}
```

## 11. Related Specifications

- **[Specification Index](spec-index.md)**
