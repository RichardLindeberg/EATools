# Item-031: Migrate Application Endpoints to Commands

**Status:** ✅ Done  
**Priority:** P1 - HIGH  
**Effort:** 20-24 hours  
**Created:** 2026-01-06  
**Completed:** 2026-01-06  
**Owner:** GitHub Copilot

---

## Problem Statement

Current Application endpoints use direct CRUD operations (POST/PUT/DELETE). Must migrate to command-based architecture with fine-grained commands like CreateApplication, SetDataClassification, TransitionLifecycle, SetOwner.

**Impact:** Fat events, no proper audit trail, cannot enforce different approval workflows per operation.

---

## Detailed Tasks

### Commands to Create
- [x] CreateApplication (name, owner, lifecycle, classification, criticality, tags, description)
- [x] SetDataClassification (classification, reason)
- [x] TransitionLifecycle (target_lifecycle, sunset_date)
- [x] SetOwner (owner, reason)
- [x] AssignToCapability (capability_id)
- [x] RemoveFromCapability ()
- [x] AddTags (tags)
- [x] RemoveTags (tags)
- [x] SetCriticality (criticality, justification)
- [x] UpdateDescription (description)
- [x] DeleteApplication (reason, approval_id)

### Events to Create
- [x] ApplicationCreated
- [x] DataClassificationChanged  
- [x] LifecycleTransitioned
- [x] OwnerSet
- [x] CapabilityAssigned
- [x] CapabilityRemoved
- [x] TagsAdded
- [x] TagsRemoved
- [x] CriticalitySet
- [x] DescriptionUpdated
- [x] ApplicationDeleted

### Command Handlers
- [x] Implement handler for each command with validation
- [x] Lifecycle transition validation (planned→active→deprecated→retired)
- [x] Classification change requires justification
- [x] Deletion requires approval_id

### API Changes
- [x] POST /applications → CreateApplication command
- [x] POST /applications/{id}/commands/set-classification
- [x] POST /applications/{id}/commands/transition-lifecycle
- [x] POST /applications/{id}/commands/set-owner
- [x] DELETE /applications/{id} → DeleteApplication command
- [x] Keep GET endpoints reading from projections

### Testing
- [x] Command handler unit tests (GivenWhenThen)
- [x] Validation tests (lifecycle paths, required fields)
- [x] Integration tests (full command→event→projection)
- [x] API endpoint tests

---

## Acceptance Criteria

- [x] All application state changes go through commands
- [x] Fine-grained events produced (not fat ApplicationUpdated)
- [x] Lifecycle transitions validated in handler
- [x] Classification changes require reason
- [x] All tests pass (25 F# unit tests, 12 Python integration tests)
- [x] API documentation updated (OpenAPI spec)

---

## Implementation Summary

**Files Created/Modified:**
- `src/Domain/ApplicationCommands.fs` - 11 commands, 11 events, ApplicationAggregate
- `src/Domain/ApplicationCommandHandler.fs` - Business logic validation for all commands
- `src/Infrastructure/Projections/ApplicationProjection.fs` - Updated to handle 11 fine-grained events
- `src/Api/ApplicationsEndpoints.fs` - Command-based endpoints with validation
- `openapi.yaml` - Added command endpoint documentation
- `tests/integration/test_applications.py` - Integration tests for command endpoints
- `tests/ApplicationCommandTests.fs` - 12 unit tests for command handlers

**Test Results:**
- ✅ 25/25 F# unit tests passing
- ✅ 12/12 Python integration tests passing

---

## Dependencies

**Depends On:** Item-029 (Commands) ✅, Item-030 (Projections) ✅

---

## References

- [spec-architecture-event-sourcing.md](../spec/spec-architecture-event-sourcing.md)
