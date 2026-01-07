# Item-050: Structured Logging Implementation with OTel ILogger

**Status:** 🟢 Ready  
**Priority:** P1 - HIGH  
**Effort:** 8-10 hours  
**Created:** 2026-01-07  
**Owner:** TBD

---

## Problem Statement

The current application lacks structured logging via OpenTelemetry. Without OTel ILogger integration:
- Log entries don't contain trace IDs and span IDs, making distributed debugging impossible
- Logs are not machine-readable JSON, preventing aggregation and analysis
- No correlation across service boundaries (LOG-001, LOG-002, LOG-003 violations)
- Error logs don't include contextual data for debugging (LOG-005, LOG-006 violations)
- Production support cannot reconstruct request flows from logs

This violates core observability requirements (LOG-001 through LOG-011) and blocks incident response capabilities.

---

## Affected Files

**Create:**
- `src/Infrastructure/Logging/StructuredLogger.fs` - OTel-compliant structured logging helper
- `src/Infrastructure/Logging/LogContext.fs` - Correlation ID and trace context management
- `src/Infrastructure/Logging/JsonLogFormatter.fs` - OTel JSON log schema formatter

**Modify:**
- `src/Program.fs` - Configure ASP.NET Core logging with OTel output
- `src/Api/Handlers.fs` - Replace ad-hoc logging with structured logging calls
- `src/Domain/*.fs` - Add logging to key domain operations
- `src/Infrastructure/Database.fs` - Log database operations with duration and status

---

## Specifications

- [spec/spec-process-observability.md](../spec/spec-process-observability.md) - Section 3.2 & 4.1 (logging requirements and schema)

---

## Detailed Tasks

- [ ] Create `src/Infrastructure/Logging/StructuredLogger.fs` module:
  - Define record types for OTel log attributes (trace_id, span_id, service.name, etc.)
  - Create helper functions: `logInfo`, `logWarn`, `logError` taking structured data
  - Ensure all functions accept correlation ID and current Activity context
  - Map F# logging calls to ILogger with proper OTel attributes

- [ ] Create `src/Infrastructure/Logging/LogContext.fs`:
  - Extension to HttpContext to extract/create correlation IDs
  - Middleware to inject correlation ID into request context
  - Methods to get current Activity (trace context) for logging

- [ ] Configure ASP.NET Core logging in `Program.fs`:
  - Add logging provider that outputs JSON in OTel format
  - Configure log levels per environment (DEBUG in dev, INFO in prod)
  - Ensure trace context (trace_id, span_id) is included in all logs

- [ ] Update all handler logging in `src/Api/Handlers.fs`:
  - Replace `logger.Log()` with structured logging calls from StructuredLogger
  - Include operation name, entity ID, result status for each handler
  - Log errors with exception details and affected aggregate ID (LOG-006)

- [ ] Add logging to key domain operations:
  - Command processing: log command type, aggregate ID, result (LOG-007)
  - Event persistence: log event count, total size, duration (LOG-008)
  - External calls: log HTTP method, target, status, latency (LOG-009)

- [ ] Add database operation logging:
  - Connection pool status (active connections)
  - Query execution time and query count
  - Slow query alerts (>500ms)

- [ ] Verify no PII in any logs:
  - Scan all logging calls for email, names, sensitive data
  - Ensure only user IDs (not user details) are logged (LOG-004)

- [ ] Update runbooks with logging configuration:
  - How to enable DEBUG logs in production for troubleshooting
  - How to search logs by trace ID
  - Common log attributes and their meanings

---

## Acceptance Criteria

- [ ] All logs are emitted via OTel ILogger (no direct console.log or printf)
- [ ] Sample logs from a full request/response cycle are valid JSON per OTel schema
- [ ] Logs contain trace_id, span_id, trace_flags for tracing
- [ ] No PII appears in any log (email, names, secrets redacted)
- [ ] Error logs include exception type, message, stack trace (LOG-006)
- [ ] Correlation ID middleware is active on all HTTP endpoints
- [ ] DEBUG logs do not appear in production logs (only in sampled troubleshooting)
- [ ] Logging overhead <5% of request latency (p99) - CON-001
- [ ] All tests pass (89+ integration tests)
- [ ] Build succeeds with 0 errors, 0 warnings
- [ ] Documentation updated with logging best practices

---

## Dependencies

**Blocks:**
- Item-051 - Command handler instrumentation (depends on logging infrastructure)

**Depends On:**
- Item-049 - OTel SDK integration (must have OTel configured first)

**Related:**
- Item-048 - F# code standards (logging patterns should follow standards)

---

## Notes

This item focuses on structured logging foundation. Metrics and tracing are separate items. Keep logging calls simple and consistent across all modules.
