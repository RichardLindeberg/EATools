# Item-082: Implement Advanced UI Patterns & Optimizations

**Status:** � Blocked  
**Priority:** P1 - HIGH  
**Effort:** 40-56 hours  
**Created:** 2026-01-17  
**Owner:** Frontend Team

---

## Problem Statement

The basic read/query and command-based write operations are functional under our CQRS architecture, but the application needs advanced UI patterns to improve user experience, handle complex workflows, and optimize performance. These patterns include dynamic forms, auto-save (with CQRS safeguards), loading states, error recovery, conflict resolution, and performance optimizations.

Without these patterns, users experience slower performance, data loss on errors, confusion during long operations, and poor handling of edge cases as specified in [spec-ui-advanced-patterns.md](../spec/spec-ui-advanced-patterns.md).

---

## Affected Files

**Create:**
- `frontend/src/components/advanced/DynamicForm.tsx` - Conditional field visibility
- `frontend/src/components/advanced/AutoSaveIndicator.tsx` - Auto-save status display
- `frontend/src/components/advanced/SkeletonLoader.tsx` - Loading skeletons
- `frontend/src/components/advanced/ProgressiveLoader.tsx` - Progressive loading
- `frontend/src/components/advanced/InfiniteScroll.tsx` - Infinite scroll wrapper
- `frontend/src/components/advanced/ErrorBoundary.tsx` - React error boundary
- `frontend/src/components/advanced/RetryableComponent.tsx` - Retry on error
- `frontend/src/components/advanced/ConflictResolver.tsx` - Merge conflict UI
- `frontend/src/components/advanced/BulkOperationProgress.tsx` - Bulk op progress
- `frontend/src/components/advanced/MultiStepWizard.tsx` - Multi-step wizard
- `frontend/src/hooks/useAutoSave.ts` - Auto-save hook (500ms debounce)
- `frontend/src/hooks/useInfiniteScroll.ts` - Infinite scroll hook
- `frontend/src/hooks/useOptimisticUpdate.ts` - Optimistic update hook
- `frontend/src/hooks/useRetry.ts` - Retry with exponential backoff
- `frontend/src/utils/performanceOptimizations.ts` - Performance utilities

**Update:**
- Entity forms - Add dynamic field visibility
- Entity lists - Add skeleton screens
- Entity detail pages - Add progressive loading
- API client - Add retry logic with backoff

---

## Specifications

- [spec/spec-ui-advanced-patterns.md](../spec/spec-ui-advanced-patterns.md) - Complete advanced patterns specification
- [spec/spec-design-ui-architecture.md](../spec/spec-design-ui-architecture.md) - Performance requirements

---

## Detailed Tasks

### Phase 1: Dynamic Forms (8-10 hours)
- [ ] Implement conditional field visibility based on field values
- [ ] Create rules engine for show/hide logic
- [ ] Add dynamic array fields (add/remove items)
- [ ] Implement multi-step wizard component
- [ ] Add step validation (prevent next until valid)
- [ ] Add progress indicator for multi-step forms
- [ ] Add save-as-draft functionality between steps
- [ ] Test with complex entity forms (Integration, BusinessCapability)

### Phase 2: Auto-Save (8-10 hours)
- [ ] Create useAutoSave hook with 500ms debounce
- [ ] Implement draft saving to localStorage
- [ ] Add auto-save indicator (Saving..., Saved, Failed)
- [ ] Implement draft recovery on page reload
- [ ] Add "Discard draft" action
- [ ] Handle conflicts between auto-saved drafts and server data
- [ ] Add timestamps to drafts
- [ ] CQRS constraints: disable auto-save for edits that span multiple commands; allow auto-save only when mapping to a single idempotent command or to create drafts client-side
- [ ] Do not auto-dispatch non-idempotent commands during auto-save; require explicit user submit
- [ ] Test with slow network conditions

### Phase 3: Loading States & Skeletons (8-10 hours)
- [ ] Create skeleton screens for all entity list pages
- [ ] Create skeleton screens for all entity detail pages
- [ ] Create skeleton screens for forms
- [ ] Implement progressive loading (load critical data first)
- [ ] Add shimmer animation to skeletons
- [ ] Replace generic spinners with context-appropriate skeletons
- [ ] Implement infinite scroll for long lists
- [ ] Add "Load more" button as fallback
- [ ] Test with simulated slow API

### Phase 4: Error Recovery (8-12 hours)
- [ ] Implement React Error Boundary for component crashes
- [ ] Create error fallback UI with retry button
- [ ] Implement retry logic with exponential backoff (1s, 2s, 4s, 8s) for queries
- [ ] Commands: retry only when idempotency is guaranteed (idempotency key) and safe to repeat; otherwise surface error
- [ ] Add network error detection and recovery
- [ ] Implement offline mode detection
- [ ] Add request queueing for offline operations (queries only). For commands, queue only if idempotency keys are used and business rules allow.
- [ ] Sync queued operations when back online (respect ordering, backoff)
- [ ] Display error messages with actionable recovery options
- [ ] Add error logging (console and optional external service)

### Phase 5: Optimistic Updates (6-8 hours)
- [ ] Implement optimistic update pattern for safe, reversible changes (e.g., client UI state)
- [ ] Delete operations: show pending state; do not remove permanently until server confirms `DELETE /entities/{id}?approval_id&reason`
- [ ] Implement optimistic update for status changes that map to a single command and can be rolled back; otherwise use loading indicators
- [ ] Add rollback on failure
- [ ] Show inline loading indicators during optimistic updates
- [ ] Add toast notifications on success/failure
- [ ] Test with forced failures (simulate 500 errors)

### Phase 6: Conflict Resolution (8-12 hours)
- [ ] Implement version conflict detection (ETag or version field)
- [ ] Create conflict resolver modal (last-write-wins, merge, cancel)
- [ ] Implement side-by-side diff viewer (your changes vs server changes)
- [ ] Add merge editor for text fields
- [ ] Implement field-level merge for structured data
- [ ] Add "Keep mine" and "Keep theirs" buttons
- [ ] Test with simulated concurrent edits

### Phase 7: Bulk Operations Progress (4-6 hours)
- [ ] Create progress tracker for bulk operations (delete, update, export)
- [ ] Show progress bar with percentage
- [ ] Display operation count (3 of 50 completed)
- [ ] Add pause/resume functionality
- [ ] Add cancel functionality (stop remaining operations)
- [ ] Show errors inline (failed items)
- [ ] Implement undo for bulk operations (optional)
- [ ] Add export progress for large datasets
- [ ] Prefer backend bulk endpoints when available; otherwise dispatch sequential commands with rate limiting and backoff
- [ ] For deletes requiring approval, ensure each command includes `approval_id` and `reason` and report per-item results

---

## Acceptance Criteria

**CQRS Compliance:**
- [ ] Advanced patterns respect query vs command separation
- [ ] Auto-save never triggers multiple commands silently; non-idempotent commands require explicit submit
- [ ] Retry/backoff applies to queries by default; commands only when idempotent
- [ ] Optimistic updates do not violate business rules; deletes await server confirmation (approval_id + reason)
- [ ] Bulk operations use server bulk APIs when available; otherwise sequential commands with progress and error handling

**Dynamic Forms:**
- [ ] Fields show/hide based on other field values
- [ ] Dynamic arrays allow adding/removing items
- [ ] Multi-step wizards guide users through complex forms
- [ ] Validation prevents progression to next step
- [ ] Draft state saved between steps

**Auto-Save:**
- [ ] Forms auto-save every 500ms after last edit
- [ ] Auto-save indicator shows current status
- [ ] Drafts recovered after browser close/refresh
- [ ] Conflicts handled gracefully

**Loading States:**
- [ ] Skeleton screens shown instead of spinners
- [ ] Skeletons match actual content layout
- [ ] Critical content loads first (progressive loading)
- [ ] Infinite scroll works smoothly
- [ ] No layout shift during loading

**Error Recovery:**
- [ ] Component errors caught and displayed with retry
- [ ] Network errors trigger automatic retry with backoff (queries)
- [ ] Command errors (422/403) display actionable guidance; no automatic retry unless idempotent
- [ ] Offline mode detected and displayed to user
- [ ] Operations queue and sync when back online
- [ ] Error messages actionable and user-friendly

**Optimistic Updates:**
- [ ] UI updates immediately only for safe, reversible updates; commands show pending state when needed
- [ ] Rollback happens on failure
- [ ] Loading indicators subtle
- [ ] Success/failure feedback provided

**Conflict Resolution:**
- [ ] Version conflicts detected
- [ ] Conflict resolver modal shows differences
- [ ] Users can choose resolution strategy
- [ ] Merged data validated before submission

**Bulk Operations:**
- [ ] Progress shown for long operations
- [ ] Users can pause/cancel operations
- [ ] Errors displayed for failed items
- [ ] Success feedback provided on completion
- [ ] Approval-requiring deletes capture and pass approval_id and reason per item

**Performance:**
- [ ] Page load <3s (as per spec)
- [ ] First Contentful Paint <1.5s
- [ ] No jank during scrolling (60fps)
- [ ] Bundle size optimized (<500KB gzipped)

---

## Dependencies

**Blocked by:**  
- Item-075 (Frontend project setup)
- Item-076 (Component library)
- Items 077-081 (Need pages to enhance)

**Blocks:**  
- None (enhances existing features)

---

## Notes

- Focus on patterns that provide immediate value
- Prioritize auto-save for forms (high-value feature)
- Test performance with large datasets (1000+ entities)
- Consider using Web Workers for heavy computations
- Implement service worker for offline support (optional, Phase 2)
- Add bundle size monitoring (webpack-bundle-analyzer)
- Use React.lazy for code splitting
- Consider adding performance monitoring (Web Vitals)
- Test with throttled network (Chrome DevTools)
- Add retry limits to prevent infinite loops
 - Separate concerns: queries (GET) vs commands (POST/DELETE) in all advanced patterns
 - Use idempotency keys for command retries where supported; otherwise avoid auto-retry
 - Invalidate and refetch affected queries after successful commands to sync read models
