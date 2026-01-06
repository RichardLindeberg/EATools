# Item-034: Migrate Relation Endpoints to Commands

**Status:** 🟢 Ready  
**Priority:** P1 - HIGH  
**Effort:** 16-20 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

Relation endpoints use CRUD. Need commands with validation against relation matrix.

---

## Detailed Tasks

### Commands
- [ ] CreateRelation (source_id, target_id, relation_type, description, classification, confidence, effective_from, effective_to)
- [ ] UpdateConfidence (confidence, evidence_source, last_verified_at)
- [ ] SetEffectiveDates (effective_from, effective_to)
- [ ] UpdateDescription (description)
- [ ] DeleteRelation (reason)

### Events
- [ ] RelationCreated
- [ ] ConfidenceUpdated
- [ ] EffectiveDatesSet
- [ ] DescriptionUpdated
- [ ] RelationDeleted

### Validation
- [ ] CreateRelation must validate (source_type, target_type, relation_type) against matrix
- [ ] Effective dates: from <= to
- [ ] Confidence: 0.0 to 1.0

### API Changes
- [ ] POST /relations → CreateRelation
- [ ] POST /relations/{id}/commands/update-confidence
- [ ] POST /relations/{id}/commands/set-effective-dates
- [ ] DELETE /relations/{id} → DeleteRelation

---

## Acceptance Criteria

- [ ] Relation validation matrix enforced
- [ ] Temporal relations supported
- [ ] All changes via commands
- [ ] Tests pass

---

## Dependencies

**Depends On:** Item-029, Item-030
