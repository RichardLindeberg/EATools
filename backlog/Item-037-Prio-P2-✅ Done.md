# Item-037: Migrate Server Endpoints to Commands

- Priority: P2 (Medium)
- Status: ✅ Done
- Owner: Platform/Ops
- Effort: 12-16h

## Goal
Replace CRUD for Servers with command handlers and events; keep reads via projections.

## Scope
- Commands: CreateServer, UpdateHostname, SetEnvironment, SetCriticality
- Events: ServerCreated, HostnameUpdated, EnvironmentSet, CriticalitySet
- Update `ServersEndpoints.fs` to dispatch commands
- Validation: DNS hostname, environment enum (dev|staging|prod), criticality enum, uniqueness of hostname

## Deliverables
- ✅ Domain command/event/handler files under `src/Domain/` (ServerCommands.fs, ServerCommandHandler.fs)
- ✅ Event JSON serialization (ServerEventJson.fs)
- ✅ Projection handler (ServerProjection.fs)
- ✅ Updated endpoints (ServersEndpoints.fs with full event sourcing)
- ✅ Updated EATool.fsproj compilation list

## Acceptance Criteria
1) ✅ Command endpoints emit focused events (no fat updates) - Each field change has dedicated command
2) ✅ Hostname uniqueness enforced at handler - Validation in ServerCommandHandler
3) ✅ Environment and criticality enums validated - Using validateEnvironment and validateCriticality
4) ✅ Projections reflect events after replay - ServerProjection updates servers table from event stream

## Implementation Summary

**Created Files:**
- `src/Domain/ServerCommands.fs` - ServerCommand/ServerEvent discriminated unions with 10 commands and 10 events
- `src/Domain/ServerCommandHandler.fs` - Command validation and event generation logic
- `src/Infrastructure/ServerEventJson.fs` - Thoth JSON encoders/decoders for ServerEvent
- `src/Infrastructure/Projections/ServerProjection.fs` - Projection handler implementing IProjectionHandler<ServerEvent>

**Modified Files:**
- `src/Api/ServersEndpoints.fs` - Migrated from CRUD to event sourcing pattern (POST, PATCH, DELETE now use commands)
- `src/EATool.fsproj` - Added new files to compilation order
- Fixed naming conflicts with ApplicationEvent.CriticalitySet by using full qualified types

**Validation:**
- DNS hostname validation via HostnameValidator
- Environment enum: dev|staging|prod
- Criticality enum: low|medium|high|critical
- All validations occur in command handlers before event emission

**Build Status:** ✅ Build succeeded

## References
- [Event Sourcing & Command-Based Architecture](../spec/spec-architecture-event-sourcing.md)
- [spec-schema-entities-infrastructure.md](../spec/spec-schema-entities-infrastructure.md)
