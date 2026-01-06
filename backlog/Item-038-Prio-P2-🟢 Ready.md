# Item-038: Migrate Integration Endpoints to Commands

- Priority: P2 (Medium)
- Status: 🟢 Ready
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
1) Each command emits a single focused event
2) Protocol/SLA/frequency validated and enforced
3) Source/target application existence checked in handler
4) Projections replay to same state

## References
- [Event Sourcing & Command-Based Architecture](../spec/spec-architecture-event-sourcing.md)
- [spec-schema-entities-infrastructure.md](../spec/spec-schema-entities-infrastructure.md)
