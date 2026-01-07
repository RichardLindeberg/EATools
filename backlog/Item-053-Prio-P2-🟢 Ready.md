# Item-053: Event Store Observability Instrumentation

**Status:** 🟢 Ready  
**Priority:** P2 - MEDIUM  
**Effort:** 6-8 hours  
**Created:** 2026-01-07  
**Owner:** TBD

---

## Problem Statement

Event sourcing is at the heart of the EA Tool, but event store operations have no observability:
- Cannot monitor event append latency or throughput
- Event store failures are not visible until user-facing errors occur
- Projection lag relative to event store is unknown
- Event replay performance cannot be measured
- Audit trail of what happened and when (per LOG-007, LOG-008) lacks tracing linkage

This violates logging requirements LOG-007, LOG-008 and tracing requirements TRC-006, and leaves event store issues invisible to operators.

---

## Affected Files

**Create:**
- `src/Infrastructure/EventStore/ObservableEventStore.fs` - Decorator pattern wrapper for event store with full instrumentation
- `docs/observability-eventstore.md` - Documentation of event store observability signals

**Modify:**
- `src/Infrastructure/EventStore.fs` - Integrate ObservableEventStore decorator
- `src/Infrastructure/Projections/*.fs` - Add lag tracking and metrics
- `src/Domain/CommandHandler.fs` - Link events to command trace for auditing

---

## Specifications

- [spec/spec-process-observability.md](../spec/spec-process-observability.md) - Section 3.2, 3.3, 3.4 (event-sourced systems observability)
- [spec/spec-architecture-event-sourcing.md](../spec/spec-architecture-event-sourcing.md) - Event store architecture

---

## Detailed Tasks

- [ ] Create `src/Infrastructure/EventStore/ObservableEventStore.fs` decorator:
  - Wrap IEventStore interface with observability
  - Log event append with: event types, count, total size, aggregate ID (LOG-008)
  - Create child span for append operation with OTel attributes
  - Measure and record append latency
  - Log any append errors with exception details and affected aggregate

- [ ] Instrument event append in decorator:
  - Pre-append: log "appending N events to aggregate {id}"
  - Append span attributes: `db.system="event_store"`, `db.operation="append"`, `event.count=N`, `aggregate.id={id}`
  - Post-append: log success with duration and metrics
  - On error: log error with exception, stack trace, attempted aggregate ID (LOG-006)
  - Emit metric: event append counter and latency histogram

- [ ] Instrument event read operations:
  - Span name: "eventstore.read"
  - Attributes: `db.operation="read"`, `aggregate.id={id}`, `event.count={resultCount}`
  - Log reads at DEBUG level (to avoid log spam in production)
  - Emit metric: event read counter and latency histogram

- [ ] Instrument event replay operations:
  - Span name: "eventstore.replay"
  - Attributes: `db.operation="replay"`, `aggregate.id={id}`, `from_version={v}`, `to_version={v}`
  - Log replay with count and duration (useful for recovery scenarios)
  - Emit metric: replay latency histogram

- [ ] Track and expose projection lag:
  - For each projection, track: `projection_current_position` and `event_store_max_position`
  - Calculate lag = `max_position - current_position`
  - Emit lag as gauge metric: `eatool.projection.lag` (unit: events, per projection)
  - Log lag as metric periodically (every 10 minutes) if lag > threshold

- [ ] Link events to command trace for audit:
  - When appending events from a command, include trace ID in event metadata
  - Allows reconstructing: command → trace → events → projections
  - Trace ID becomes searchable in audit trail (useful for compliance)

- [ ] Implement event store performance instrumentation:
  - Track separate metrics for different aggregate types
  - Monitor append latency percentiles (p50, p95, p99)
  - Alert on abnormal latency (requires Item-054)

- [ ] Add observability documentation:
  - Document what each event store log message means
  - Document metrics: what they measure, labels, how to interpret
  - Document projection lag: what it means, acceptable thresholds
  - Show example queries for event store health dashboards

- [ ] Create integration tests:
  - Verify log entries are created for append/read operations
  - Verify metrics are emitted with correct values and labels
  - Verify trace spans are created and linked correctly
  - Verify lag calculation is accurate

---

## Acceptance Criteria

- [ ] Event append operations create OTel spans with proper attributes
- [ ] Logs contain event count, total size, aggregate ID per LOG-008
- [ ] Append errors include exception details and affected aggregate per LOG-006
- [ ] Event read operations are instrumented (at DEBUG level for low log volume)
- [ ] Projection lag is calculated and exposed as a metric
- [ ] Events are linked to command trace ID for audit trail
- [ ] Event store metrics are emitted (append count, duration, lag per MET-006, MET-009)
- [ ] Projection lag metric updates are correct (matches event store - projection position)
- [ ] Documentation clearly explains all observability signals
- [ ] Integration tests verify instrumentation works end-to-end
- [ ] All tests pass (89+ integration tests)
- [ ] Build succeeds with 0 errors, 0 warnings

---

## Dependencies

**Blocks:**
- Item-054 - Alert rules (needs event store metrics available first)

**Depends On:**
- Item-049 - OTel SDK integration
- Item-050 - Logging (uses logging infrastructure)
- Item-051 - Tracing (creates child spans)
- Item-052 - Metrics (emits metrics)

**Related:**
- Item-036 - AuditLog projection (should reference trace ID for audits)
- Item-035 - Snapshot system (may need observability too)

---

## Notes

Event store observability is critical for production systems. Ensure every operation (append, read, replay) is visible. Projection lag tracking is especially important for CQRS consistency monitoring. Be careful with log volume; use DEBUG level for high-frequency reads.
