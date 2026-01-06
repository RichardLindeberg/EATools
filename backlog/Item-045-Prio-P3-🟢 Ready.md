# Item-045: Update API Documentation for Command Endpoints

- Priority: P3 (Low)
- Status: 🟢 Ready
- Owner: Docs/API
- Effort: 8-12h

## Goal
Document command-based endpoints and patterns in OpenAPI and markdown.

## Scope
- Update OpenAPI with command endpoints (POST /{aggregate}/commands/*)
- Document command envelopes, examples, and error patterns
- Add guidance for clients on splitting fat updates into commands

## Deliverables
- Updated `openapi.yaml`
- Docs in `docs/` or `spec/` as appropriate

## Acceptance Criteria
1) OpenAPI includes new command endpoints with schemas
2) Examples show fine-grained commands (e.g., set-classification)
3) Guidance provided for clients migrating from CRUD

## References
- [Event Sourcing & Command-Based Architecture](../spec/spec-architecture-event-sourcing.md)
