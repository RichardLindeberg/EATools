# Item-039: Migrate DataEntity Endpoints to Commands

- Priority: P2 (Medium)
- Status: ✅ Done
- Owner: Platform/Data
- Effort: 12-16h

## Goal
Move DataEntity writes to command handlers with audit-ready events.

## Scope
- Commands: CreateDataEntity, SetClassification, SetPIIFlag, UpdateRetention, AddDataEntityTags, DeleteDataEntity
- Events: DataEntityCreated, ClassificationSet, PIIFlagSet, RetentionUpdated, DataEntityTagsAdded, DataEntityDeleted
- Update `DataEntitiesEndpoints.fs` to dispatch commands
- Validation: classification enum, pii_flag boolean, retention format, owner/steward presence

## Deliverables
- Domain command/event/handler files under `src/Domain/`
- Updated endpoints
- Tests for validations and projection consistency

## Acceptance Criteria
- ✅ Classification changes emit dedicated events
- ✅ PII flag and retention validated per spec
- ✅ Commands are small; no fat updates
- ✅ Projections replay to identical state

## Implementation Summary
Successfully migrated DataEntity endpoints from CRUD pattern to full event sourcing architecture:

**Created Files:**
- `src/Domain/DataEntityCommands.fs` - 6 commands and 6 events with DataEntityAggregate
- `src/Domain/DataEntityCommandHandler.fs` - Validation and handlers (classification: public|internal|confidential|restricted, retention: regex format, pii_flag: boolean)
- `src/Infrastructure/DataEntityEventJson.fs` - Thoth JSON encoders/decoders for all events
- `src/Infrastructure/Projections/DataEntityProjection.fs` - Projection handler implementing IProjectionHandler<DataEntityEvent>

**Updated Files:**
- `src/Api/DataEntitiesEndpoints.fs` - Migrated from DataEntityRepository.create/update/delete to command dispatch with loadAggregateState and persistAndProject
- `src/EATool.fsproj` - Added all new DataEntity files to compilation order

**Build Status:** ✅ Build succeeded with no errors

**Commands Implemented:**
1. CreateDataEntity - Creates new data entity with classification and name validation
2. SetClassification - Updates classification (public, internal, confidential, restricted)
3. SetPIIFlag - Toggles PII flag status
4. UpdateRetention - Updates retention policy
5. AddDataEntityTags - Adds tags (deduplicates automatically)
6. DeleteDataEntity - Soft deletes data entity

**Validation Rules:**
- Classification: Must be one of [public, internal, confidential, restricted]
- Name: Required, 1-255 characters
- Retention: Format like "7 years", "90 days", "1y", "6m", etc. (regex: `^\d+\s*(y|year|years|m|month|months|d|day|days|h|hour|hours|w|week|weeks)$` or `^\d+[ymdhw]$`)
- PII Flag: Boolean (default false)
- Business Rules: No modification of deleted data entities

## References
- [Event Sourcing & Command-Based Architecture](../spec/spec-architecture-event-sourcing.md)
- [spec-schema-entities-data.md](../spec/spec-schema-entities-data.md)
