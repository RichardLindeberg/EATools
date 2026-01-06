# Item-014: Add Missing API Documentation Files

**Status:** 🟢 Ready  
**Priority:** P2 - MEDIUM  
**Effort:** 4-6 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

[docs/README.md](../../docs/README.md) references two documentation files that don't exist:
- `docs/api-usage-guide.md`
- `docs/authorization-guide.md`

This creates broken links and documentation gaps.

---

## Affected Files

**Create:**
- [docs/api-usage-guide.md](../../docs/api-usage-guide.md) (new)
- [docs/authorization-guide.md](../../docs/authorization-guide.md) (new)

---

## Detailed Tasks

### API Usage Guide
- [ ] Create docs/api-usage-guide.md with:
  - Authentication setup (OIDC, API key)
  - Making requests (curl, Postman, SDKs)
  - Request/response examples
  - Pagination patterns
  - Filtering and search patterns
  - Error handling
  - Rate limiting
  - Best practices
  - Troubleshooting

### Authorization Guide
- [ ] Create docs/authorization-guide.md with:
  - RBAC patterns
  - ABAC patterns
  - OPA policy configuration
  - Permission model
  - Common authorization scenarios
  - Troubleshooting access issues

---

## Acceptance Criteria

- [ ] api-usage-guide.md is comprehensive
- [ ] authorization-guide.md is comprehensive
- [ ] Content is accurate
- [ ] Examples are tested
- [ ] Links in README work
- [ ] Both files follow docs style

---

## Dependencies

**Depends On:**
- Item-004: API Contract spec
- Item-007: Authorization spec
- Item-009: Authentication spec
- Item-010: Error Handling spec
- Item-011: Query Patterns spec

---

## Related Items

- [docs/README.md](../../docs/README.md)
- [Item-004-Prio-P0.md](Item-004-Prio-P0.md)
- [Item-007-Prio-P1.md](Item-007-Prio-P1.md)

---

## Definition of Done

- [x] Both files created
- [x] Comprehensive and practical
- [x] Examples provided
- [x] Links verified
- [x] Style consistent with other docs

---

## Notes

- These are user-facing guides (not formal specs)
- Should be practical and code-example heavy
- Assume developer audience familiar with APIs
