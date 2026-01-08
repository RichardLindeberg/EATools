---
title: Query Patterns Specification
version: 0.1.0
date_created: 2026-01-08
last_updated: 2026-01-08
owner: EA Platform Team
tags: [tool, api, query, pagination, filtering, sorting, search]
---

# Introduction

This specification defines standard query patterns for EA Tool APIs, including pagination, filtering, sorting, search, soft-delete handling, response envelopes, and consistency rules for list endpoints.

## 1. Purpose & Scope

Provide a consistent contract for query parameters and list responses across all endpoints. Scope: pagination, filtering, sorting, search, soft-delete toggles, response metadata, examples, and client guidance. Applies to all list endpoints in the API.

## 2. Definitions

- **Pagination**: `page` (1-based) and `limit` parameters controlling result windows.
- **Filtering**: Field-specific query parameters (e.g., `owner`, `environment`, `tag`).
- **Sorting**: `sort` parameter with ascending/descending indicators.
- **Search**: Free-text matching across selected fields per entity.
- **Soft Delete**: Optional inclusion of logically deleted records via `include_deleted`.
- **Has More**: Boolean flag indicating additional pages beyond current window.

## 3. Requirements, Constraints & Guidelines

- **REQ-001**: All collection/list endpoints SHALL support pagination with `page` (default 1) and `limit` (default 50, max 200).
- **REQ-002**: All list endpoints SHALL accept `search` for text lookup; matching is case-insensitive and substring-based unless stated otherwise.
- **REQ-003**: Filtering parameters SHALL use clear field names (`owner`, `environment`, `lifecycle`, `tag`, `parent_id`, etc.) and allow multi-valued parameters via repeated keys.
- **REQ-004**: Sorting SHALL use `sort` with `field` (asc), `+field` (asc), or `-field` (desc); multiple sorts allowed via comma separation in a single value.
- **REQ-005**: Responses SHALL include `items`, `page`, `limit`, `total`; `has_more` MAY be included for forward pagination convenience.
- **REQ-006**: Validation errors for query params SHALL return `validation_error` (422) per the error handling spec.
- **REQ-007**: Soft-deleted records SHALL be excluded by default; `include_deleted=true` MAY include them where the domain supports soft delete.
- **REQ-008**: Parameter names SHALL be consistent across entities (e.g., `owner` not `owner_id`; `environment` not `env`).
- **GUD-001**: Prefer indexed fields for filters to keep queries performant.
- **GUD-002**: Document entity-specific filters alongside the OpenAPI schema.

## 4. Interfaces & Data Contracts

### 4.1 Standard Query Parameters

- `page` (int, optional): 1-based page number; default 1; min 1.
- `limit` (int, optional): page size; default 50; min 1; max 200.
- `search` (string, optional): case-insensitive substring search over entity-specific fields (typically name/description/tags).
- `sort` (string, optional): comma-separated list of fields; prefix with `-` for descending, `+` for explicit ascending; default per entity (commonly `-created_at`).
- `include_deleted` (bool, optional): default false; when true, include soft-deleted items where supported.

### 4.2 Filtering Patterns (examples)

- Single value: `?owner=team-id`
- Multiple values (repeat param): `?tag=payments&tag=pci`
- Null filter: `?parent_id=null` (roots only)
- Equality filters use direct parameter names; avoid custom operators in query string.

### 4.3 Sorting Patterns

- Ascending: `?sort=name` or `?sort=+name`
- Descending: `?sort=-created_at`
- Multiple: `?sort=+priority,-created_at`

### 4.4 Pagination Response Envelope

```json
{
  "items": [ /* array of resources */ ],
  "page": 2,
  "limit": 10,
  "total": 150,
  "has_more": true
}
```

### 4.5 Search Behavior
- Case-insensitive substring match.
- Applied to common text fields per entity (e.g., `name`, `description`, tags where available).
- Non-indexed search MAY be slower in SQLite; consider full-text in future engines.

### 4.6 Soft Delete Handling
- Default: exclude soft-deleted records.
- `include_deleted=true` MAY include them where the domain implements soft delete.
- If unsupported for an entity, the parameter is ignored or returns validation error per API contract.

## 5. Acceptance Criteria

- **AC-001**: Query patterns spec created with template sections and front matter.
- **AC-002**: Pagination pattern documented (params, defaults, limits, response envelope).
- **AC-003**: Filtering pattern documented, including multi-value and null filters.
- **AC-004**: Sorting pattern documented with multi-field support.
- **AC-005**: Search pattern documented (case-insensitive substring).
- **AC-006**: Soft delete handling documented with defaults and toggles.
- **AC-007**: Examples provided for pagination, filtering, search, sorting, and combined usage.
- **AC-008**: Consistency rules documented for parameter names across endpoints.
- **AC-009**: Spec linked from the specifications index.

## 6. Test Automation Strategy

- **Test Levels**: API integration tests for list endpoints asserting pagination metadata, sorting order, filter correctness, search matching, and soft-delete toggling where applicable.
- **Frameworks**: Existing API test harness with table-driven cases; property tests for pagination bounds.
- **Test Data Management**: Seed datasets with varied owners, environments, tags, parent/child hierarchies, and soft-deleted samples.
- **CI/CD Integration**: Run query-pattern tests in CI against SQLite; future runs against MSSQL/PG for collation/search parity.
- **Coverage Requirements**: Positive and negative cases for each parameter; boundary tests for `page`, `limit`, and sort parsing.
- **Performance Testing**: Ensure filtered/sorted queries use indexes; monitor p99 latency for search and tag filters.

## 7. Rationale & Context

- Consistent parameter names reduce client friction and code duplication.
- Standard envelopes simplify pagination handling in SDKs and UIs.
- Multi-value filters and predictable sorting enable flexible querying without bespoke endpoints.

## 8. Dependencies & External Integrations

### External Systems
- **EXT-001**: None.

### Third-Party Services
- **SVC-001**: None.

### Infrastructure Dependencies
- **INF-001**: Database indexes to support common filters and sorts.

### Data Dependencies
- **DAT-001**: None beyond entity tables; search behavior depends on available text indexes.

### Technology Platform Dependencies
- **PLT-001**: HTTP framework supporting query parsing and validation.

### Compliance Dependencies
- **COM-001**: None specific; adhere to general API privacy/PII handling.

## 9. Examples & Edge Cases

- Pagination: `GET /applications?page=2&limit=10`
- Filtering: `GET /applications?owner=team-123&lifecycle=active`
- Search: `GET /applications?search=payment`
- Sorting: `GET /applications?sort=-created_at`
- Combined: `GET /applications?page=1&limit=10&lifecycle=active&sort=-created_at&search=payment`
- Null filter: `GET /organizations?parent_id=null`
- Multi-value tags: `GET /applications?tag=payments&tag=pci`

Edge Cases:
- `limit` > max → validation error (422) with details per error handling spec.
- `page` < 1 → validation error (422).
- Invalid sort field → validation error (422) or ignored per endpoint contract (documented per API contract).
- `include_deleted=true` on entity without soft delete → validation error (422) if enforced, else no-op.

## 10. Validation Criteria

- List responses include `items`, `page`, `limit`, `total`, and optional `has_more` per this spec.
- Query parameters enforce documented bounds and naming across endpoints.
- Filters, sorting, and search yield consistent results in integration tests.
- Soft delete toggle behaves as documented for supporting entities.
- Spec is reachable from the index and aligns with the standard template.

## 11. Related Specifications / Further Reading

- [spec-index.md](spec/spec-index.md)
- [spec-tool-api-contract.md](spec/spec-tool-api-contract.md)
- [spec-tool-error-handling.md](spec/spec-tool-error-handling.md)
- [spec-architecture-data.md](spec/spec-architecture-data.md)