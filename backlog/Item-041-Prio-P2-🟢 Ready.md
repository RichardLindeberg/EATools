# Item-041: Add Temporal Query Support

- Priority: P2 (Medium)
- Status: 🟢 Ready
- Owner: Platform
- Effort: 16-20h

## Goal
Enable time-travel queries (state as of timestamp) via event replay/projections.

## Scope
- API pattern: `?asOf=timestamp` for reads
- Projection support for temporal queries or on-the-fly replay to T
- Guard future-effective relations (effective_from > now not shown)
- Tests for reconstructing state at T

## Deliverables
- Temporal query logic in projections or replay module
- API changes to accept `asOf`
- Tests for temporal correctness

## Acceptance Criteria
1) Can return Application state as of a past timestamp
2) Relations respect effective_from/to during temporal queries
3) Temporal results match replay up to T

## References
- [Event Sourcing & Command-Based Architecture](../spec/spec-architecture-event-sourcing.md)
