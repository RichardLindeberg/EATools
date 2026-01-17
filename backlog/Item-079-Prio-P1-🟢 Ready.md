# Item-079: Implement Entity List Pages (All 9 Types)

**Status:** ✅ Done  
**Priority:** P1 - HIGH  
**Effort:** 60-80 hours (Completed in ~8 hours)
**Created:** 2026-01-17  
**Completed:** 2026-01-17  
**Owner:** Frontend Team

---

## Problem Statement

Users need to view, search, filter, and manage lists of all entity types in the EATool system. The application supports 9 entity types (Applications, Servers, Integrations, DataEntities, BusinessCapabilities, Organizations, Relations, ApplicationServices, ApplicationInterfaces), and each needs a dedicated list page.

List pages must support pagination (skip/take), sorting, filtering, search, bulk selection, and bulk actions as specified in [spec-ui-entity-workflows.md](../spec/spec-ui-entity-workflows.md).

Without list pages, users cannot browse entities, perform bulk operations, or navigate to detail/edit pages.

---

## Affected Files

**Create:**
- `frontend/src/pages/applications/ApplicationListPage.tsx`
- `frontend/src/pages/servers/ServerListPage.tsx`
- `frontend/src/pages/integrations/IntegrationListPage.tsx`
- `frontend/src/pages/data-entities/DataEntityListPage.tsx`
- `frontend/src/pages/business-capabilities/BusinessCapabilityListPage.tsx`
- `frontend/src/pages/organizations/OrganizationListPage.tsx`
- `frontend/src/pages/relations/RelationListPage.tsx`
- `frontend/src/pages/application-services/ApplicationServiceListPage.tsx`
- `frontend/src/pages/application-interfaces/ApplicationInterfaceListPage.tsx`
- `frontend/src/components/entity/EntityListTemplate.tsx` - Reusable list template
- `frontend/src/components/entity/EntityTable.tsx` - Reusable table component
- `frontend/src/components/entity/FilterPanel.tsx` - Filter sidebar
- `frontend/src/components/entity/BulkActionBar.tsx` - Bulk action toolbar
- `frontend/src/hooks/useEntityList.ts` - Reusable list hook
- `frontend/src/api/entitiesApi.ts` - Entity CRUD API calls
- `frontend/src/types/entities.ts` - Entity TypeScript types

---

## Specifications

- [spec/spec-ui-entity-workflows.md](../spec/spec-ui-entity-workflows.md) - Entity workflow patterns
- [spec/spec-ui-api-integration.md](../spec/spec-ui-api-integration.md) - API integration requirements
- [spec/spec-ui-advanced-patterns.md](../spec/spec-ui-advanced-patterns.md) - Advanced UI patterns
- [BACKEND-UI-ALIGNMENT.md](../BACKEND-UI-ALIGNMENT.md) - Backend API alignment

---

## Detailed Tasks

### Phase 1: Foundation & Shared Components (16-20 hours)

**Entity Types & API Client:**
- [ ] Define TypeScript interfaces for all 9 entity types
- [ ] Create entitiesApi.ts with CRUD functions (getAll, getById, create, update, delete)
- [ ] Implement pagination support (skip, take query params)
- [ ] Implement sorting support (sort query param)
- [ ] Implement filtering support (filter[key]=value query params)
- [ ] Implement search support (search query param)

**Shared Components:**
- [ ] Create EntityListTemplate component (layout, header, filters, table)
- [ ] Create EntityTable component (sortable columns, row actions)
- [ ] Create FilterPanel component (dynamic filters based on entity type)
- [ ] Create BulkActionBar component (select all, bulk delete, bulk archive)
- [ ] Create EmptyState component (no results, no filters applied)

**Custom Hooks:**
- [ ] Create useEntityList hook (fetches, paginates, filters, sorts)
- [ ] Create useBulkSelection hook (select all, select page, clear)
- [ ] Create useEntityActions hook (delete, archive, export)

### Phase 2: Application List Page (6-8 hours)
- [ ] Create ApplicationListPage with table view
- [ ] Display columns: ID, Name, Description, Type, Status, Owner, Created, Updated
- [ ] Implement sorting on all columns
- [ ] Add filters: Type, Status, Owner, Date range
- [ ] Add search by name/description
- [ ] Add pagination controls (skip/take)
- [ ] Add row actions: View, Edit, Delete
- [ ] Add bulk actions: Delete selected, Archive selected, Export selected
- [ ] Add "Create New" button
- [ ] Connect to GET /applications API endpoint

### Phase 3: Server List Page (6-8 hours)
- [ ] Create ServerListPage with table view
- [ ] Display columns: ID, Name, Hostname, IP Address, Environment, Status, Owner, Created
- [ ] Implement sorting
- [ ] Add filters: Environment, Status, Owner
- [ ] Add search by name/hostname/IP
- [ ] Add pagination, row actions, bulk actions
- [ ] Connect to GET /servers API endpoint

### Phase 4: Integration List Page (6-8 hours)
- [ ] Create IntegrationListPage with table view
- [ ] Display columns: ID, Name, Type, Protocol, Source, Target, Status, Created
- [ ] Implement sorting
- [ ] Add filters: Type, Protocol, Status
- [ ] Add search by name
- [ ] Add pagination, row actions, bulk actions
- [ ] Connect to GET /integrations API endpoint

### Phase 5: DataEntity List Page (6-8 hours)
- [ ] Create DataEntityListPage with table view
- [ ] Display columns: ID, Name, Type, Format, Classification, Owner, Created
- [ ] Implement sorting
- [ ] Add filters: Type, Format, Classification, Owner
- [ ] Add search by name
- [ ] Add pagination, row actions, bulk actions
- [ ] Connect to GET /data-entities API endpoint

### Phase 6: BusinessCapability List Page (6-8 hours)
- [ ] Create BusinessCapabilityListPage with table view
- [ ] Display columns: ID, Name, Level, Parent, Owner, Status, Created
- [ ] Implement sorting
- [ ] Add filters: Level, Parent, Status, Owner
- [ ] Add search by name/description
- [ ] Add pagination, row actions, bulk actions
- [ ] Connect to GET /business-capabilities API endpoint

### Phase 7: Organization List Page (4-6 hours)
- [ ] Create OrganizationListPage with table view
- [ ] Display columns: ID, Name, Type, Parent, Owner, Created
- [ ] Implement sorting
- [ ] Add filters: Type, Parent
- [ ] Add search by name
- [ ] Add pagination, row actions, bulk actions
- [ ] Connect to GET /organizations API endpoint

### Phase 8: Relation List Page (4-6 hours)
- [ ] Create RelationListPage with table view
- [ ] Display columns: ID, Type, Source Entity, Target Entity, Created
- [ ] Implement sorting
- [ ] Add filters: Type, Source Type, Target Type
- [ ] Add search
- [ ] Add pagination, row actions, bulk actions
- [ ] Connect to GET /relations API endpoint

### Phase 9: ApplicationService List Page (4-6 hours)
- [ ] Create ApplicationServiceListPage with table view
- [ ] Display columns: ID, Name, Application, Protocol, Status, Created
- [ ] Implement sorting
- [ ] Add filters: Application, Protocol, Status
- [ ] Add search by name
- [ ] Add pagination, row actions, bulk actions
- [ ] Connect to GET /application-services API endpoint

### Phase 10: ApplicationInterface List Page (4-6 hours)
- [ ] Create ApplicationInterfaceListPage with table view
- [ ] Display columns: ID, Name, Type, Protocol, Source App, Target App, Created
- [ ] Implement sorting
- [ ] Add filters: Type, Protocol, Source App, Target App
- [ ] Add search by name
- [ ] Add pagination, row actions, bulk actions
- [ ] Connect to GET /application-interfaces API endpoint

---

## Acceptance Criteria

**For Each Entity Type:**
- [ ] List page displays all entities from API
- [ ] Pagination works correctly (skip/take)
- [ ] Sorting works on all sortable columns
- [ ] Filters work correctly (AND logic)
- [ ] Search works correctly
- [ ] Row actions (View, Edit, Delete) work correctly
- [ ] Bulk selection works (select all, select page, clear)
- [ ] Bulk delete works with confirmation modal
- [ ] "Create New" button navigates to create page
- [ ] Empty state shown when no results
- [ ] Loading state shown during API calls
- [ ] Error state shown on API failures
- [ ] Query parameters sync with URL (deep linking)
- [ ] Page state persists on browser back/forward

**General:**
- [ ] All 9 entity list pages implemented and working
- [ ] Consistent UI across all list pages
- [ ] Responsive design (mobile, tablet, desktop)
- [ ] Accessibility: keyboard navigation, screen readers
- [ ] Performance: <2s initial load, <500ms filter/sort

---

## Dependencies

**Blocked by:**  
- Item-075 (Frontend project setup)
- Item-076 (Component library - needs Table, Pagination, Button, FilterPanel)
- Item-077 (Authentication)
- Item-078 (Routing)

**Blocks:**  
- Item-080 (Entity detail pages - need navigation from list)
- Item-081 (Entity forms - need navigation to create/edit)

---

## Notes

- **Reuse code:** Create generic EntityListTemplate and customize per entity type
- Use TanStack Query for API state management (caching, invalidation)
- Implement optimistic updates for delete operations
- Add confirmation modals for destructive actions (delete, bulk delete)
- Consider adding view switcher (table, grid, card views)
- Add export functionality (CSV, JSON)
- Implement infinite scroll as alternative to pagination (optional)
- Add column customization (show/hide columns) (optional)
- Cache list state in sessionStorage for back navigation
