# Item-031: Migrate Application Endpoints to Commands

**Status:** 🟢 Ready  
**Priority:** P1 - HIGH  
**Effort:** 20-24 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

Current Application endpoints use direct CRUD operations (POST/PUT/DELETE). Must migrate to command-based architecture with fine-grained commands like CreateApplication, SetDataClassification, TransitionLifecycle, SetOwner.

**Impact:** Fat events, no proper audit trail, cannot enforce different approval workflows per operation.

---

## Detailed Tasks

### Commands to Create
- [ ] CreateApplication (name, owner, lifecycle, classification, criticality, tags, description)
- [ ] SetDataClassification (classification, reason)
- [ ] TransitionLifecycle (target_lifecycle, sunset_date)
- [ ] SetOwner (owner, reason)
- [ ] AssignToCapability (capability_id)
- [ ] RemoveFromCapability ()
- [ ] AddTags (tags)
- [ ] RemoveTags (tags)
- [ ] SetCriticality (criticality, justification)
- [ ] UpdateDescription (description)
- [ ] DeleteApplication (reason, approval_id)

### Events to Create
- [ ] ApplicationCreated
- [ ] DataClassificationChanged  
- [ ] LifecycleTransitioned
- [ ] OwnerSet
- [ ] CapabilityAssigned
- [ ] CapabilityRemoved
- [ ] TagsAdded
- [ ] TagsRemoved
- [ ] CriticalitySet
- [ ] DescriptionUpdated
- [ ] ApplicationDeleted

### Command Handlers
- [ ] Implement handler for each command with validation
- [ ] Lifecycle transition validation (planned→active→deprecated→retired)
- [ ] Classification change requires justification
- [ ] Deletion requires approval_id

### API Changes
- [ ] POST /applications → CreateApplication command
- [ ] POST /applications/{id}/commands/set-classification
- [ ] POST /applications/{id}/commands/transition-lifecycle
- [ ] POST /applications/{id}/commands/set-owner
- [ ] DELETE /applications/{id} → DeleteApplication command
- [ ] Keep GET endpoints reading from projections

### Testing
- [ ] Command handler unit tests (GivenWhenThen)
- [ ] Validation tests (lifecycle paths, required fields)
- [ ] Integration tests (full command→event→projection)
- [ ] API endpoint tests

---

## Acceptance Criteria

- [ ] All application state changes go through commands
- [ ] Fine-grained events produced (not fat ApplicationUpdated)
- [ ] Lifecycle transitions validated in handler
- [ ] Classification changes require reason
- [ ] All tests pass
- [ ] API documentation updated

---

## Dependencies

**Depends On:** Item-029 (Commands), Item-030 (Projections)

---

## References

- [spec-architecture-event-sourcing.md](../spec/spec-architecture-event-sourcing.md)
