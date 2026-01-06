# Item-042: Implement Compensation Events

- Priority: P2 (Medium)
- Status: 🟢 Ready
- Owner: Platform
- Effort: 8-12h

## Goal
Allow corrections without deleting events via compensation events.

## Scope
- Command pattern for corrections referencing original event_id
- Events like DataClassificationCorrected, NameCorrected, etc.
- Store link to corrected event and reason
- Ensure projections apply corrections deterministically

## Deliverables
- Correction command/handler abstractions
- At least one implemented correction flow (e.g., classification)
- Tests for projection correctness after corrections

## Acceptance Criteria
1) Corrections do not delete original events
2) Projections reflect corrected state
3) Audit trace links correction to original event

## References
- [Event Sourcing & Command-Based Architecture](../spec/spec-architecture-event-sourcing.md)
