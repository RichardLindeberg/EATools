# Item-{ID}: {Title}

**Status:** 🟢 Ready  
**Priority:** {P0|P1|P2|P3} - {CRITICAL|HIGH|MEDIUM|LOW}  
**Effort:** {X-Y} hours  
**Created:** YYYY-MM-DD  
**Owner:** TBD

---

## Problem Statement

{2-3 paragraphs describing:
- What is the current problem or gap?
- Why does it need to be addressed?
- What is the business or technical impact?}

Example:
> The current implementation lacks proper validation for X, which leads to Y. This impacts users because Z. Without this fix, we risk A and cannot proceed with B.

---

## Affected Files

**Create:**
- `path/to/new/file.ext` - {Description of what this file will contain}

**Modify:**
- `path/to/existing/file.ext:123-145` - {Description of changes}

**Delete:**
- `path/to/obsolete/file.ext` - {Reason for deletion}

---

## Specifications

{Link to related specifications or design documents}

- [spec/spec-name.md](../spec/spec-name.md)
- [docs/design-doc.md](../docs/design-doc.md)

---

## Detailed Tasks

- [ ] Task 1: {Specific action}
- [ ] Task 2: {Specific action}
- [ ] Task 3: {Specific action}

{Break down the work into concrete, measurable steps}

---

## Acceptance Criteria

- [ ] Criterion 1: {Specific, testable outcome}
- [ ] Criterion 2: {Specific, testable outcome}
- [ ] All tests pass (89+ integration tests)
- [ ] Build succeeds with 0 errors, 0 warnings
- [ ] Documentation updated

{Define what "done" looks like in measurable terms}

---

## Dependencies

**Blocks:** 
- Item-XXX - {Why this blocks that item}

**Depends On:** 
- Item-XXX - {What needs to be completed first}

**Related:**
- Item-XXX - {Related but not blocking}

---

## Related Items

{Links to related backlog items that provide context}

- Item-XXX: {Relation description}
- Item-YYY: {Relation description}

---

## Definition of Done

- [ ] All detailed tasks completed
- [ ] All acceptance criteria met
- [ ] Code reviewed (if applicable)
- [ ] Tests written and passing
- [ ] Documentation updated
- [ ] Changes committed to feature branch
- [ ] Branch merged to main (if applicable)

---

## Notes

{Any additional context, design decisions, or implementation notes}

**Design Decisions:**
- Decision 1 and rationale
- Decision 2 and rationale

**Implementation Notes:**
- Note about approach
- Warning about edge cases

**References:**
- Link to external documentation
- Link to GitHub issue/PR

---

## File Management

### When Status Changes to "✅ Done"
1. Rename file: `Item-{ID}-Prio-{P0-P3}-✅ Done.md`
2. Update **Completed:** date in header
3. Move file to `backlog/old/` directory: `mv Item-{ID}*.md old/`
4. Update [backlog/INDEX.md](INDEX.md):
   - Remove from active items section
   - Add to "Completed Items" section with completion date
5. Keep INDEX.md focused on active work while preserving history

### File Naming Convention
```
Item-{id:3d}-Prio-{P0|P1|P2|P3}-{Status}.md

Examples:
  Item-001-Prio-P0-✅ Done.md        → moves to old/
  Item-026-Prio-P3-🟡 In Progress.md → stays in backlog/
  Item-035-Prio-P1-🟢 Ready.md       → stays in backlog/
```

**Status Values:** 🔴 Blocked | 🟡 In Progress | 🟢 Ready | ✅ Done

---

## Usage Instructions

### Creating a New Backlog Item
1. Copy this template
2. Name it: `Item-{next-number}-Prio-{P0-P3}-🟢 Ready.md`
3. Fill in all sections (remove placeholder text)
4. Add to [INDEX.md](INDEX.md) in appropriate priority section
5. Commit to repository

### Updating During Work
- Change status as work progresses
- Check off tasks and criteria as completed
- Add notes about decisions or issues encountered
- Update affected files list if scope changes

### Completing an Item
- Mark all criteria complete
- Update status to ✅ Done
- Add completion date
- Move to backlog/old/
- Update INDEX.md

---

## Template Version

**Version:** 1.0  
**Last Updated:** 2026-01-07  
**Changelog:**
- v1.0: Initial template with file management instructions
