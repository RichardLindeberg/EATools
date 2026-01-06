# Item-043: Create Data Migration Strategy from CRUD to Event-Sourced

- Priority: P2 (Medium)
- Status: 🟢 Ready
- Owner: Platform/Data
- Effort: 16-24h

## Goal
Migrate existing state tables into the Event Store with synthetic events and runbooks.

## Scope
- Generate initial Created events from current rows
- Backfill best-effort history where possible
- Migration runbook and rollback plan
- Validate projections match pre-migration state

## Deliverables
- Migration plan doc in `docs/`
- Migration scripts (SQL/F#) to emit events from current data
- Verification checklist and automated checks

## Acceptance Criteria
1) Post-migration projections match pre-migration data
2) Event store seeded with Created events for all aggregates
3) Rollback/abort steps documented

## References
- [Event Sourcing & Command-Based Architecture](../spec/spec-architecture-event-sourcing.md)
