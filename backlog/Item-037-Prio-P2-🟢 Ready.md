# Item-037: Migrate Server Endpoints to Commands

- Priority: P2 (Medium)
- Status: 🟢 Ready
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
- Domain command/event/handler files under `src/Domain/Server/`
- Updated endpoints
- Tests (unit + integration) for command→event→projection

## Acceptance Criteria
1) Command endpoints emit focused events (no fat updates)
2) Hostname uniqueness enforced at handler
3) Environment and criticality enums validated
4) Projections reflect events after replay

## References
- [Event Sourcing & Command-Based Architecture](../spec/spec-architecture-event-sourcing.md)
- [spec-schema-entities-infrastructure.md](../spec/spec-schema-entities-infrastructure.md)
