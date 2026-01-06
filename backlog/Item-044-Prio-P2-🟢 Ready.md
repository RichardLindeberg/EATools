# Item-044: Create Event Sourcing Test Suite

- Priority: P2 (Medium)
- Status: 🟢 Ready
- Owner: QA/Platform
- Effort: 20-24h

## Goal
Comprehensive automated tests for commands, events, projections, snapshots, and replay.

## Scope
- Property-based tests for replay/idempotency/commutativity where applicable
- Command handler unit tests (Given-When-Then)
- Projection correctness tests
- Snapshot verification tests
- Integration tests for command→event→projection flows

## Deliverables
- Test suites under `tests/` covering above areas
- CI integration running on PRs

## Acceptance Criteria
1) Replay produces identical state to projections
2) Duplicate command_id does not create duplicate events
3) Snapshots verified by replaying post-snapshot events
4) Temporal queries covered

## References
- [Event Sourcing & Command-Based Architecture](../spec/spec-architecture-event-sourcing.md)
