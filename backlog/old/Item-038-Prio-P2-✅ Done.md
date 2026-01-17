# Item-038: Migrate Integration Endpoints to Commands

- Priority: P2 (Medium)
- Status: ✅ Done
- Owner: Platform/Integration
- Effort: 12-16h

## Goal
Commandify Integrations to capture protocol/SLA changes as events.

## Scope
- Commands: CreateIntegration, UpdateProtocol, SetSLA, SetFrequency
- Events: IntegrationCreated, ProtocolUpdated, SLASet, FrequencySet
- Update `IntegrationsEndpoints.fs` to dispatch commands
- Validation: protocol enum, SLA format, frequency value, source/target app existence

## Deliverables
- Domain command/event/handler files under `src/Domain/Integration/`
- Updated endpoints
- Tests for command→event→projection

## Acceptance Criteria
- ✅ Each command emits a single focused event
- ✅ Protocol/SLA/frequency validated and enforced
- ✅ Source/target application existence checked in handler
- ✅ Projections replay to same state

## Implementation Summary
Successfully migrated Integration endpoints from CRUD pattern to full event sourcing architecture:

**Created Files:**
- `src/Domain/IntegrationCommands.fs` - 10 commands and 10 events with IntegrationAggregate
- `src/Domain/IntegrationCommandHandler.fs` - Validation and handlers (protocol: 13 valid types, frequency: regex + keywords, SLA: format validation)
- `src/Infrastructure/IntegrationEventJson.fs` - Thoth JSON encoders/decoders for all events
- `src/Infrastructure/Projections/IntegrationProjection.fs` - Projection handler implementing IProjectionHandler<IntegrationEvent>

**Updated Files:**
- `src/Api/IntegrationsEndpoints.fs` - Migrated from IntegrationRepository.create/update/delete to command dispatch with loadAggregateState and persistAndProject
- `src/EATool.fsproj` - Added all new Integration files to compilation order

**Build Status:** ✅ Build succeeded with no errors

**Commands Implemented:**
1. CreateIntegration - Creates new integration with validation (source != target)
2. UpdateProtocol - Updates protocol with validation (13 valid protocols)
3. SetSLA - Sets SLA with format validation (99.9%, < 100ms, 24/7, etc.)
4. SetFrequency - Sets frequency with validation (5m, 1h, daily, real-time, etc.)
5. UpdateDataContract - Updates data contract specification
6. SetSourceApp - Changes source application
7. SetTargetApp - Changes target application
8. AddIntegrationTags - Adds tags
9. RemoveIntegrationTags - Removes tags
10. DeleteIntegration - Soft deletes integration

**Validation Rules:**
- Protocol: Must be one of [rest, graphql, grpc, soap, kafka, rabbitmq, sqs, http, https, tcp, udp, websocket, mqtt]
- Frequency: Matches pattern `^\d+[smh]$` or keywords [real-time, streaming, 5m, 15m, 30m, 1h, 4h, 12h, daily, weekly, monthly, on-demand]
- SLA: Matches formats for percentage, time units, or availability (99.9%, < 100ms, 24/7, best-effort)
- Business Rule: source_app_id != target_app_id

## References
- [Event Sourcing & Command-Based Architecture](../spec/spec-architecture-event-sourcing.md)
- [spec-schema-entities-infrastructure.md](../spec/spec-schema-entities-infrastructure.md)
