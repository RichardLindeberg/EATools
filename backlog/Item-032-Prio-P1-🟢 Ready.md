# Item-032: Migrate Organization Endpoints to Commands

**Status:** 🟢 Ready  
**Priority:** P1 - HIGH  
**Effort:** 16-20 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

Organization endpoints use CRUD operations. Must migrate to commands with cycle detection for parent hierarchy.

---

## Detailed Tasks

### Commands
- [ ] CreateOrganization
- [ ] SetParent (parent_id) - with cycle detection
- [ ] RemoveParent ()
- [ ] UpdateContactInfo (contacts)
- [ ] AddDomain (domain)
- [ ] RemoveDomain (domain)
- [ ] DeleteOrganization

### Events
- [ ] OrganizationCreated
- [ ] ParentAssigned
- [ ] ParentRemoved
- [ ] ContactInfoUpdated
- [ ] DomainAdded
- [ ] DomainRemoved
- [ ] OrganizationDeleted

### Validation
- [ ] SetParent must detect cycles (walk hierarchy)
- [ ] Domain must be valid DNS format
- [ ] Contacts must be valid emails

### API Changes
- [ ] POST /organizations → CreateOrganization
- [ ] POST /organizations/{id}/commands/set-parent
- [ ] POST /organizations/{id}/commands/remove-parent
- [ ] DELETE /organizations/{id} → DeleteOrganization

---

## Acceptance Criteria

- [ ] Cycle detection prevents parent loops
- [ ] All organization changes use commands
- [ ] Domain validation enforced
- [ ] Tests pass

---

## Dependencies

**Depends On:** Item-029, Item-030
