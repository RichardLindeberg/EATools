# Item-006: Review and Simplify Lifecycle Storage Strategy

**Status:** 🟢 Ready  
**Priority:** P1 - HIGH  
**Effort:** 3-4 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

Application model stores lifecycle in two fields:
- `Lifecycle: Lifecycle` (discriminated union/enum)
- `LifecycleRaw: string`

This creates redundancy, complexity, and potential for inconsistency. Purpose is unclear and not documented in specifications.

**Impact:** Code complexity, potential data inconsistency, unclear intent.

---

## Affected Files

- [src/Domain/Models.fs](../../src/Domain/Models.fs#L24-L35) - Application type
- [src/Domain/Models.fs](../../src/Domain/Models.fs#L227-L240) - CreateApplicationRequest
- [src/Infrastructure/Migrations/001_create_applications.sql](../../src/Infrastructure/Migrations/001_create_applications.sql)
- [src/Infrastructure/ApplicationRepository.fs](../../src/Infrastructure/ApplicationRepository.fs)
- [src/Infrastructure/Json.fs](../../src/Infrastructure/Json.fs) - Application encoder/decoder

---

## Detailed Tasks

### Investigation Phase
- [ ] Document why both `Lifecycle` and `LifecycleRaw` exist
- [ ] Check if lifecycle_raw is ever different from parsed Lifecycle
- [ ] Check if any consumers rely on lifecycle_raw specifically
- [ ] Check git history for context

### Decision Phase
Choose one of three approaches:

**Option A: Keep Discriminated Union Only**
- [ ] Store as normalized string in DB (planned, active, deprecated, retired)
- [ ] Parse to union on read
- [ ] Validate enum values on write
- [ ] Remove lifecycle_raw field and column

**Option B: Keep String Only**
- [ ] Remove Lifecycle discriminated union
- [ ] Store as string (more flexible)
- [ ] Validate string values in code
- [ ] Remove lifecycle_raw, keep just lifecycle

**Option C: Keep Both (Document Rationale)**
- [ ] Document clearly in spec why both exist
- [ ] Explain intended use cases
- [ ] Document invariants (when they should match/differ)

### Implementation Phase (based on decision)
- [ ] Update domain model
- [ ] Update database schema (if removing column)
- [ ] Update repository code
- [ ] Update JSON encoder/decoder
- [ ] Update tests
- [ ] Update specifications

---

## Acceptance Criteria

- [ ] Single, clear approach chosen and documented
- [ ] No breaking changes to API contract
- [ ] Tests updated and passing
- [ ] Specification updated
- [ ] Code reviewed and approved

---

## Recommendation

**Recommend Option A:** Keep discriminated union only
- Provides type safety
- Forces valid values
- Cleaner code
- Single source of truth
- Easier to refactor later

Implementation:
1. Keep `Lifecycle: Lifecycle` enum
2. Remove `LifecycleRaw: string`
3. Store as string in DB ("planned", "active", etc.)
4. Parse on read, validate on write

---

## Dependencies

**Blocks:** None

**Depends On:** None

---

## Related Items

- [spec/spec-schema-entities-application.md](../../spec/spec-schema-entities-application.md)
- [spec/spec-schema-domain-overview.md](../../spec/spec-schema-domain-overview.md)

---

## Definition of Done

- [x] Decision documented and approved
- [x] Implementation complete
- [x] All tests pass
- [x] No breaking API changes
- [x] Specification updated
- [x] Code reviewed

---

## Notes

- This is a code quality/maintainability issue
- Low risk once approach is chosen
- Should be done before Item-001 sprint to avoid conflicting changes
- Document the decision in spec for future reference
