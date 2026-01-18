# Item-083: Frontend Testing & Quality Assurance

**Status:** � Blocked  
**Priority:** P1 - HIGH  
**Effort:** 48-64 hours  
**Created:** 2026-01-17  
**Owner:** Frontend Team

---

## Problem Statement

The frontend application needs comprehensive testing to ensure reliability, prevent regressions, and maintain code quality. Without testing, bugs will reach production, accessibility requirements may be violated, and refactoring becomes risky.

Testing must cover unit tests, integration tests, accessibility compliance (WCAG 2.1 AA), and performance as specified in [spec-design-ui-architecture.md](../spec/spec-design-ui-architecture.md).

---

## Affected Files

**Create:**
- `frontend/src/components/**/*.test.tsx` - Component unit tests
- `frontend/src/hooks/**/*.test.ts` - Hook unit tests
- `frontend/src/utils/**/*.test.ts` - Utility function tests
- `frontend/src/pages/**/*.test.tsx` - Page integration tests
- `frontend/tests/e2e/auth.spec.ts` - E2E auth tests
- `frontend/tests/e2e/entities.spec.ts` - E2E entity workflow tests
- `frontend/tests/e2e/bulk-operations.spec.ts` - E2E bulk operations tests
- `frontend/tests/accessibility.spec.ts` - Accessibility tests
- `frontend/tests/performance.spec.ts` - Performance tests
- `frontend/.github/workflows/ci.yml` - CI pipeline (if using GitHub)
- `frontend/vitest.config.ts` - Vitest configuration
- `frontend/playwright.config.ts` - Playwright configuration (E2E)
- `frontend/.eslintrc.json` - ESLint configuration with testing rules

**Update:**
- Add test scripts to `frontend/package.json`
- Add coverage thresholds to vitest config

---

## Specifications

- [spec/spec-design-ui-architecture.md](../spec/spec-design-ui-architecture.md) - Testing and quality requirements
- [spec/spec-ui-auth-permissions.md](../spec/spec-ui-auth-permissions.md) - Auth testing scenarios

---

## Detailed Tasks

### Phase 1: Test Infrastructure Setup (8-10 hours)
- [ ] Configure Vitest for unit/integration tests
- [ ] Configure React Testing Library
- [ ] Configure Playwright for E2E tests
- [ ] Setup test utilities (render helpers, mock data factories)
- [ ] Create MSW (Mock Service Worker) for API mocking with CQRS routes:
      - GET query endpoints for list/detail/relationships
      - Command endpoints (POST /{id}/commands/*, DELETE with approval_id & reason)
- [ ] Configure test coverage reporting (>80% target)
- [ ] Add test scripts to package.json (test, test:watch, test:coverage)
- [ ] Setup CI pipeline for automated testing
- [ ] Configure test environment variables

### Phase 2: Component Unit Tests (12-16 hours)
**Test all components from Item-076:**
- [ ] Button component tests (variants, disabled, onClick)
- [ ] TextInput tests (value, onChange, validation states, error display)
- [ ] Select tests (options, onChange, multi-select)
- [ ] Checkbox/Radio tests (checked state, onChange)
- [ ] Table tests (data display, sorting, row selection)
- [ ] Modal tests (open/close, backdrop click, keyboard ESC)
- [ ] Toast tests (display, auto-dismiss, queue management)
- [ ] Alert tests (variants, dismissible)
- [ ] Pagination tests (page changes, skip/take)
- [ ] Breadcrumbs tests (navigation, dynamic generation)
- [ ] Header/Sidebar tests (navigation, user menu)
- [ ] Form field tests (label, error, help text, required indicator)

**Coverage Target:** >90% for components

### Phase 3: Hook Unit Tests (8-10 hours)
- [ ] useAuth hook tests (login, logout, token refresh, permissions)
- [ ] useEntityList hook tests (fetch, filter, sort, paginate)
- [ ] useEntityDetail hook tests (fetch by ID, loading, error states)
- [ ] useEntityForm hook tests (create via POST, edit via commands/PATCH fallback)
- [ ] useEntityCommand hook tests (command routing, 422 mapping, 403 handling, query invalidation)
- [ ] useAutoSave hook tests (CQRS constraints: no multi-command auto-save)
- [ ] useInfiniteScroll hook tests (load more, pagination)
- [ ] useRetry hook tests (queries retried; commands retried only if idempotent)
- [ ] useQueryParams hook tests (read, write, sync with URL)

**Coverage Target:** >85% for hooks

### Phase 4: Utility Function Tests (6-8 hours)
- [ ] tokenManager tests (get, set, remove, isExpired)
- [ ] formValidation tests (email, URL, IP, JSON validation)
- [ ] formHelpers tests (field mapping, error transformation)
- [ ] routeGuards tests (authentication check, permission check)
- [ ] performanceOptimizations tests (debounce, throttle)

**Coverage Target:** >95% for utilities

### Phase 5: Page Integration Tests (12-16 hours)
**Test user workflows across pages:**
- [ ] Login flow test (enter credentials, submit, redirect to home)
- [ ] Entity list page tests (fetch entities, pagination, filtering, sorting)
- [ ] Entity detail page tests (fetch entity, display, relationships tab)
- [ ] Entity create flow test (fill form, submit via POST, redirect to detail)
- [ ] Entity edit flow test (applications/business-capabilities/organizations use commands; others use PATCH)
- [ ] Verify correct command endpoints are called (classification/lifecycle/owner/parent/description/confidence/effective-dates) with MSW assertions
- [ ] Relations command tests: update-confidence, set-effective-dates, update-description commands verified
- [ ] ApplicationServices command tests: update, set-business-capability, add-consumer commands verified
- [ ] ApplicationInterfaces command tests: update, set-service, deprecate, retire commands verified
- [ ] Entity delete flow test (confirm modal captures approval_id + reason; DELETE called with both; redirect to list)
- [ ] Protected route tests (redirect to login if not authenticated)
- [ ] Permission-based tests (hide/disable actions without permission)
- [ ] Error handling tests (404, 403, 422, 500 responses)

**Coverage Target:** >70% for pages

### Phase 6: E2E Tests with Playwright (12-16 hours)
**Critical user journeys:**
- [ ] E2E: User registration and login
- [ ] E2E: Create application → view detail → edit classification via command → transition lifecycle via command → delete with approval
- [ ] E2E: Create business capability → set parent via command → remove parent → update description via command
- [ ] E2E: Create server → link to application → verify relationship
- [ ] E2E: Search entities → filter results → navigate to detail
- [ ] E2E: Bulk select entities → bulk delete (approval for each) → confirm success
- [ ] E2E: Form validation errors (422) → fix errors → submit successfully
- [ ] E2E: Token expiration → auto-refresh → continue operation
- [ ] E2E: Session timeout → warning modal → extend session
- [ ] E2E: Network error (queries) → retry → success

**Coverage:** 10-15 critical user journeys

### Phase 7: Accessibility Testing (8-10 hours)
- [ ] Install axe-core and jest-axe
- [ ] Test keyboard navigation on all pages
- [ ] Test screen reader compatibility (NVDA, JAWS)
- [ ] Test ARIA attributes on interactive elements
- [ ] Test focus management (modals, dropdowns, forms)
- [ ] Test color contrast ratios (WCAG AA)
- [ ] Test form labels and error associations
- [ ] Test heading hierarchy (h1, h2, h3)
- [ ] Test skip links and landmarks
- [ ] Run automated accessibility audits (Lighthouse, axe)

**Target:** WCAG 2.1 AA compliance (0 violations)

### Phase 8: Performance Testing (6-8 hours)
- [ ] Measure page load times (<3s requirement)
- [ ] Measure First Contentful Paint (<1.5s requirement)
- [ ] Measure Time to Interactive
- [ ] Measure bundle size (<500KB gzipped target)
- [ ] Test with 1000+ entity list (render time, scroll performance)
- [ ] Test form submission time
- [ ] Test API call latency handling
- [ ] Profile React component renders (React DevTools Profiler)
- [ ] Identify and fix performance bottlenecks
- [ ] Run Lighthouse performance audits (score >90)

---

## Acceptance Criteria

**Unit Tests:**
- [ ] >80% overall code coverage
- [ ] >90% coverage for components
- [ ] >85% coverage for hooks
- [ ] >95% coverage for utilities
- [ ] All tests pass consistently

**Integration Tests:**
- [ ] All critical user workflows tested
- [ ] API integration mocked correctly (queries + commands)
- [ ] Error scenarios tested (404, 403, 422, 500)
- [ ] Loading states tested
- [ ] Command dispatch verified: correct endpoints hit for edits/deletes

**E2E Tests:**
- [ ] 10-15 critical user journeys tested end-to-end
- [ ] Tests run in CI pipeline
- [ ] Tests run against local backend
- [ ] Flaky tests identified and fixed

**CQRS Compliance:**
- [ ] Tests assert that applications do not use generic PATCH for classification/lifecycle/owner
- [ ] Tests assert that business capabilities use set-parent, remove-parent, update-description commands
- [ ] Tests assert that organizations use set-parent, remove-parent commands
- [ ] Tests assert that relations use update-confidence, set-effective-dates, update-description commands
- [ ] Tests assert that application-services use update, set-business-capability, add-consumer commands
- [ ] Tests assert that application-interfaces use update, set-service, deprecate, retire commands
- [ ] Delete flows require and pass approval_id + reason parameters
- [ ] Query invalidation occurs after successful commands

**Accessibility:**
- [ ] 0 axe-core violations
- [ ] Keyboard navigation works on all pages
- [ ] Screen reader tested on key workflows
- [ ] WCAG 2.1 AA compliance verified
- [ ] Color contrast ratios meet AA standard

**Performance:**
- [ ] Page load <3s (verified)
- [ ] First Contentful Paint <1.5s (verified)
- [ ] Bundle size <500KB gzipped
- [ ] Lighthouse performance score >90
- [ ] No performance regressions in CI

**CI/CD:**
- [ ] All tests run automatically on PR
- [ ] Coverage report generated
- [ ] Failed tests block merge
- [ ] Performance benchmarks tracked

---

## Dependencies

**Blocked by:**  
- Items 075-082 (Need features to test)

**Blocks:**  
- None (completes Phase 1 MVP)

---

## Notes

- Write tests alongside feature development when possible
- Use TDD (Test-Driven Development) for complex logic
- Mock backend API calls consistently (MSW)
  - Include handlers for command endpoints and DELETE with approval parameters
  - Assert request bodies (commands) and query params (approval_id, reason)
- Test with realistic data volumes
- Run tests in CI on every commit
- Monitor test execution time (optimize slow tests)
- Use snapshot testing sparingly (high maintenance)
- Consider visual regression testing (Percy, Chromatic)
- Add pre-commit hooks to run tests (Husky)
- Document testing patterns and best practices
- Create test data factories for consistent test data
- Use testing-library user-event for realistic interactions
- Test error boundaries and fallback UIs
- Consider mutation testing (Stryker) for test quality
