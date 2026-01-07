# Item-032: Migrate Organization Endpoints to Commands

**Status:** ✅ Done  
**Priority:** P1 - HIGH  
**Effort:** 16-20 hours  
**Created:** 2026-01-06  
**Completed:** 2026-01-07  
**Owner:** GitHub Copilot

---

## Problem Statement

Organization endpoints use CRUD operations. Must migrate to commands with cycle detection for parent hierarchy.

---

## Detailed Tasks

### Commands
- [x] CreateOrganization
- [x] SetParent (parent_id) - with cycle detection
- [x] RemoveParent ()
- [x] UpdateContactInfo (contacts)
- [x] AddDomain (domain)
- [x] RemoveDomain (domain)
- [x] DeleteOrganization

### Events
- [x] OrganizationCreated
- [x] ParentAssigned
- [x] ParentRemoved
- [x] ContactInfoUpdated
- [x] DomainAdded
- [x] DomainRemoved
- [x] OrganizationDeleted

### Validation
- [x] SetParent must detect cycles (walk hierarchy)
- [x] Domain must be valid DNS format
- [x] Contacts must be valid emails

### API Changes
- [x] POST /organizations → CreateOrganization
- [x] POST /organizations/{id}/commands/set-parent
- [x] POST /organizations/{id}/commands/remove-parent
- [x] DELETE /organizations/{id} → DeleteOrganization
- [x] PATCH /organizations/{id} - dispatches to appropriate commands

---

## Acceptance Criteria

- [x] Cycle detection prevents parent loops
- [x] All organization changes use commands
- [x] Domain validation enforced
- [x] Email validation enforced
- [x] Tests pass (19/19 organization tests, 88/88 total)

---

## Implementation Summary

**Files Created:**
- `src/Domain/OrganizationCommands.fs` - Commands, events, and aggregate
- `src/Domain/OrganizationCommandHandler.fs` - Business logic with cycle detection, domain/email validation
- `src/Infrastructure/OrganizationEventJson.fs` - JSON encoders/decoders for events
- Updated `src/Infrastructure/Projections/OrganizationProjection.fs` - Fine-grained event handlers

**Files Modified:**
- `src/Api/OrganizationsEndpoints.fs` - Command-based endpoints with event sourcing
- `src/EATool.fsproj` - Added new modules

**Test Results:**
- ✅ 19/19 organization integration tests passing
- ✅ 88/88 total integration tests passing

---

## Dependencies

**Depends On:** Item-029 ✅, Item-030 ✅
