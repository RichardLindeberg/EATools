# Item-039: Migrate DataEntity Endpoints to Commands

- Priority: P2 (Medium)
- Status: 🟢 Ready
- Owner: Platform/Data
- Effort: 12-16h

## Goal
Move DataEntity writes to command handlers with audit-ready events.

## Scope
- Commands: CreateDataEntity, SetClassification, SetPIIFlag, UpdateRetention
- Events: DataEntityCreated, DataClassificationChanged, PIIFlagSet, RetentionUpdated
- Update `DataEntitiesEndpoints.fs` to dispatch commands
- Validation: classification enum, pii_flag boolean, retention format, owner/steward presence

## Deliverables
- Domain command/event/handler files under `src/Domain/DataEntity/`
- Updated endpoints
- Tests for validations and projection consistency

## Acceptance Criteria
1) Classification changes emit dedicated events
2) PII flag and retention validated per spec
3) Commands are small; no fat updates
4) Projections replay to identical state

## References
- [Event Sourcing & Command-Based Architecture](../spec/spec-architecture-event-sourcing.md)
- [spec-schema-entities-data.md](../spec/spec-schema-entities-data.md)
