# Item-040: Implement Event Schema Versioning (Upcasting)

- Priority: P2 (Medium)
- Status: 🟢 Ready
- Owner: Platform
- Effort: 12-16h

## Goal
Support event_version and upcasting to evolve event schemas safely.

## Scope
- Store event_version on all events
- Upcaster pipeline to transform old events to current shape
- Tests for forward-compatibility and deserialization
- Tooling to detect unknown versions

## Deliverables
- Upcasting module (e.g., `src/Infrastructure/EventUpcaster.fs`)
- Unit tests covering multiple versions
- Guidelines for adding new versions

## Acceptance Criteria
1) Events with older versions load via upcaster without runtime errors
2) Unknown version surfaces clear error
3) Upcaster covered by tests for at least two version transitions

## References
- [Event Sourcing & Command-Based Architecture](../spec/spec-architecture-event-sourcing.md)
