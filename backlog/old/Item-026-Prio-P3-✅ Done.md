# Item-026: Create Backlog Item Template

**Status:** ✅ Done  
**Priority:** P3 - LOW  
**Effort:** 30 minutes  
**Created:** 2026-01-06  
**Completed:** 2026-01-07  
**Owner:** GitHub Copilot

---

## Problem Statement

Backlog items should follow a consistent template for easier navigation and maintenance. Need to formalize the template structure used in Items 001-025.

---

## Affected Files

**Create:** [backlog/TEMPLATE.md](TEMPLATE.md) (new)

---

## Detailed Tasks

- [x] Create backlog/TEMPLATE.md
- [x] Document all sections
- [x] Provide guidance for each section
- [x] Include examples
- [x] Reference in INDEX.md
- [x] Add file management instructions (move completed items to backlog/old/)
- [x] Update INDEX.md structure to separate active and completed items
- [x] Move all completed items to backlog/old/ directory

---

## Template Structure

```markdown
# Item-{id}: {Title}

**Status:** {🟢 Ready | 🔴 Blocked | 🟡 In Progress}
**Priority:** {P0 | P1 | P2 | P3} - {CRITICAL | HIGH | MEDIUM | LOW}
**Effort:** {estimate} hours
**Created:** {date}
**Owner:** TBD

---

## Problem Statement
[2-3 paragraphs describing the problem and its impact]

---

## Affected Files
[List of files that will be changed or created]

---

## Specifications
[Link to related specifications, if any]

---

## Detailed Tasks
- [ ] Task 1
- [ ] Task 2

---

## Acceptance Criteria
- [ ] Criterion 1
- [ ] Criterion 2

---

## Dependencies
**Blocks:** [Items blocked by this one]
**Depends On:** [Items this depends on]

---

## Related Items
[Links to related backlog items]

---

## Definition of Done
- [x] All tasks complete
- [x] Acceptance criteria met

---

## Notes
[Any additional context or guidance]
```

---

## Acceptance Criteria

- [x] Template created at backlog/TEMPLATE.md
- [x] Clear guidance provided for each section
- [x] Examples included in template
- [x] Linked from INDEX.md
- [x] File management process documented (move to backlog/old/ when done)
- [x] INDEX.md reorganized to show active items separately from completed
- [x] All 12 completed items moved to backlog/old/

---

## Dependencies

**Depends On:** None

---

## Definition of Done

- [x] Template created at backlog/TEMPLATE.md
- [x] Guidance provided for all sections
- [x] File management process documented
- [x] Linked from INDEX.md
- [x] backlog/old/ directory created
- [x] All completed items (12) moved to old/ directory
- [x] INDEX.md reorganized with separate active/completed sections

---

## Notes

- Quick meta-task completed successfully
- Ensures consistency for future items
- Use TEMPLATE.md for new backlog items
- Completed items now organized in backlog/old/ to keep main backlog clean
- INDEX.md now focuses on active work (34 items) with history preserved

**Implementation:**
1. Created comprehensive TEMPLATE.md with all sections and examples
2. Added file management instructions to template
3. Created backlog/old/ directory structure
4. Moved 12 completed items (Items: 001-006, 012, 028-034) to old/
5. Reorganized INDEX.md:
   - Shows only active items in main sections
   - P0 shows "All complete!" since 7/7 done
   - Added "Completed Items" section at end with links to old/
   - Updated progress tracking and navigation

**Benefits:**
- Main backlog is now cleaner and more focused
- Easier to see what work remains
- History is preserved in old/ directory
- Template ensures consistency going forward


## Notes

- Quick meta-task
- Ensures consistency for future items
- Use this template for new backlog items
