---
title: Error Handling Specification
version: 0.1.0
date_created: 2026-01-08
last_updated: 2026-01-08
owner: EA Platform Team
tags: [tool, api, error-handling, reliability]
---

# Introduction

This specification defines the error handling model for the EA Tool API. It standardizes error codes, response formats, HTTP status mappings, trace identifiers, rate limiting signals, examples, and client handling expectations.

## 1. Purpose & Scope

Provide a consistent, debuggable, and client-friendly error contract for all EA Tool endpoints. Scope: REST error responses, code taxonomy, validation error shapes, trace IDs, retry/backoff guidance, and rate limiting headers.

## 2. Definitions

- **Error Code**: Machine-readable string indicating error class (e.g., `validation_error`).
- **Trace ID**: Unique request identifier propagated in responses and logs (UUID or ULID).
- **Field Error**: Per-field validation failure entry within `errors` array.
- **Retry-After**: HTTP header specifying when a client may retry (seconds or HTTP-date).
- **Backoff**: Client strategy to delay retries (e.g., exponential with jitter).

## 3. Requirements, Constraints & Guidelines

- **REQ-001**: All non-2xx responses SHALL include a machine-readable `code` and human-readable `message`.
- **REQ-002**: Validation failures SHALL use `validation_error` with status 422 and include an `errors` array with field-level details.
- **REQ-003**: Each error response SHALL include `trace_id` echoed from request context or generated server-side.
- **REQ-004**: Rate limiting responses SHALL use status 429, code `rate_limit_exceeded`, and include `Retry-After` plus `X-RateLimit-*` headers.
- **REQ-005**: Conflict scenarios (e.g., unique constraint violations) SHALL use status 409 and code `conflict`.
- **REQ-006**: `not_found` SHALL map to status 404; `forbidden` to 403; `service_unavailable` to 503; `internal_error` to 500.
- **REQ-007**: Clients SHALL treat 5xx as transient and apply bounded retries with exponential backoff and jitter; 4xx SHOULD NOT be retried except 429 and 409 per policy.
- **SEC-001**: Error payloads MUST NOT leak sensitive data (PII, secrets, stack traces); internal details remain in logs with the same `trace_id`.
- **CON-001**: Error payloads MUST be JSON and conform to the schemas in [openapi.yaml](openapi.yaml).
- **GUD-001**: Prefer concise `message` text suitable for end-user display; reserve technical detail for logs keyed by `trace_id`.
- **GUD-002**: Provide stable `code` values; avoid per-endpoint custom strings.

## 4. Interfaces & Data Contracts

### 4.1 Error Code Taxonomy

| Code | HTTP Status | Meaning |
| --- | --- | --- |
| validation_error | 422 | Request failed schema or business validation |
| not_found | 404 | Resource does not exist |
| forbidden | 403 | Authenticated but not authorized |
| conflict | 409 | Uniqueness or state conflict |
| rate_limit_exceeded | 429 | Too many requests per rate limit |
| internal_error | 500 | Unexpected server error |
| service_unavailable | 503 | Dependency or service unavailable |

### 4.2 Standard Error Response

```json
{
  "code": "internal_error",
  "message": "Unexpected error",
  "trace_id": "req-123e4567-e89b-12d3-a456-426614174000"
}
```

### 4.3 Validation Error Response

```json
{
  "code": "validation_error",
  "message": "Request validation failed",
  "trace_id": "req-123e4567-e89b-12d3-a456-426614174000",
  "errors": [
    {"field": "name", "message": "Field is required", "rule": "required"},
    {"field": "email", "message": "Invalid format", "rule": "email"}
  ]
}
```

### 4.4 Rate Limit Response

```json
{
  "code": "rate_limit_exceeded",
  "message": "Too many requests",
  "trace_id": "req-123e4567-e89b-12d3-a456-426614174000"
}
```

Headers:
- `Retry-After: 15`
- `X-RateLimit-Limit: 1000`
- `X-RateLimit-Remaining: 0`
- `X-RateLimit-Reset: 1704739200`

### 4.5 Field-Level Errors
- `field`: JSON path or parameter name.
- `message`: Human-readable description of the validation failure.
- `rule`: Optional short code for the violated rule (e.g., `required`, `max_length`, `pattern`).

## 5. Acceptance Criteria

- **AC-001**: Error handling spec created with full template sections and front matter.
- **AC-002**: Error code taxonomy documented with HTTP status mappings.
- **AC-003**: Error and validation response structures documented including `trace_id` and `errors` array.
- **AC-004**: HTTP status usage documented for 400, 403, 404, 409, 422, 429, 500, 503.
- **AC-005**: Trace ID generation/propagation documented for debugging.
- **AC-006**: Rate limiting behavior and headers documented with retry guidance.
- **AC-007**: Examples provided for validation, not found, forbidden, rate limit, and internal errors.
- **AC-008**: Client handling guidance documented (retry/backoff vs fail-fast).
- **AC-009**: Spec linked from the specifications index.

## 6. Test Automation Strategy

- **Test Levels**: Unit tests for error mappers; integration tests asserting response bodies, headers, and status codes for failure scenarios; contract tests against OpenAPI error schemas.
- **Frameworks**: Existing API test harness plus OpenAPI-based validators; property tests for validation error arrays.
- **Test Data Management**: Fixture requests intentionally violating schemas and causing conflicts; simulated rate limit to trigger 429.
- **CI/CD Integration**: Run contract and negative-path tests in CI; block releases on schema mismatch or missing headers.
- **Coverage Requirements**: Tests per error code taxonomy entry and per mapped HTTP status.
- **Performance Testing**: Ensure error paths include trace IDs and headers without materially impacting latency.

## 7. Rationale & Context

- Stable error codes and formats reduce client-side branching and improve observability.
- Trace IDs connect client-visible errors to server logs for faster root-cause analysis.
- Explicit rate limit signals enable polite client retry behavior.
- Clear validation detail reduces support burden and improves developer UX.

## 8. Dependencies & External Integrations

### External Systems
- **EXT-001**: None.

### Third-Party Services
- **SVC-001**: None.

### Infrastructure Dependencies
- **INF-001**: Logging/trace infrastructure capable of emitting `trace_id` correlated with request context.

### Data Dependencies
- **DAT-001**: None beyond error payloads.

### Technology Platform Dependencies
- **PLT-001**: HTTP framework support for consistent error serialization and header injection.

### Compliance Dependencies
- **COM-001**: Error payloads must avoid leaking sensitive data; adhere to privacy requirements.

## 9. Examples & Edge Cases

**Not Found**
```json
{
  "code": "not_found",
  "message": "Application not found",
  "trace_id": "req-123e4567-e89b-12d3-a456-426614174000"
}
```

**Forbidden**
```json
{
  "code": "forbidden",
  "message": "Access denied",
  "trace_id": "req-123e4567-e89b-12d3-a456-426614174000"
}
```

**Conflict**
```json
{
  "code": "conflict",
  "message": "Name already exists",
  "trace_id": "req-123e4567-e89b-12d3-a456-426614174000"
}
```

Edge Cases:
- Missing `trace_id` in context → generate a new one and include in response and logs.
- Upstream dependency outage → return `service_unavailable` (503) with minimal message; client should retry with backoff.
- Malformed JSON payload → respond with 400 `validation_error` if schema parsing fails before field-level validation.

## 10. Validation Criteria

- Error responses conform to documented structures and OpenAPI schemas for all failure paths.
- Rate limit responses emit `Retry-After` and `X-RateLimit-*` headers with code `rate_limit_exceeded`.
- Trace IDs are present in all error responses and correlate with server logs.
- Validation errors include `errors` array entries with field and rule where applicable.
- Spec is reachable from the index and follows the standard template.

## 11. Related Specifications / Further Reading

- [spec-index.md](spec/spec-index.md)
- [openapi.yaml](openapi.yaml)
- [spec-tool-api-contract.md](spec/spec-tool-api-contract.md)
- [spec-process-authorization.md](spec/spec-process-authorization.md)