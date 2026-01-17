# Item-080: Implement Entity Detail Pages (All 9 Types)

**Status:** � Blocked  
**Priority:** P1 - HIGH  
**Effort:** 48-64 hours  
**Created:** 2026-01-17  
**Owner:** Frontend Team

---

## Problem Statement

Users need to view detailed information about individual entities, including their properties, relationships with other entities, and audit history. Each of the 9 entity types requires a dedicated detail page.

Detail pages must display all entity properties, related entities (via GET /entities/{id}/relationships), and provide actions for editing and deleting as specified in [spec-ui-entity-workflows.md](../spec/spec-ui-entity-workflows.md).

Without detail pages, users cannot view complete entity information, understand relationships, or access edit/delete actions.

---

## Affected Files

**Create:**
- `frontend/src/pages/applications/ApplicationDetailPage.tsx`
- `frontend/src/pages/servers/ServerDetailPage.tsx`
- `frontend/src/pages/integrations/IntegrationDetailPage.tsx`
- `frontend/src/pages/data-entities/DataEntityDetailPage.tsx`
- `frontend/src/pages/business-capabilities/BusinessCapabilityDetailPage.tsx`
- `frontend/src/pages/organizations/OrganizationDetailPage.tsx`
- `frontend/src/pages/relations/RelationDetailPage.tsx`
- `frontend/src/pages/application-services/ApplicationServiceDetailPage.tsx`
- `frontend/src/pages/application-interfaces/ApplicationInterfaceDetailPage.tsx`
- `frontend/src/components/entity/EntityDetailTemplate.tsx` - Reusable detail template
- `frontend/src/components/entity/EntityHeader.tsx` - Entity header with actions
- `frontend/src/components/entity/PropertyGrid.tsx` - Key-value property display
- `frontend/src/components/entity/RelationshipsTab.tsx` - Relationships viewer
- `frontend/src/components/entity/AuditTab.tsx` - Audit history viewer
- `frontend/src/hooks/useEntityDetail.ts` - Entity detail hook
- `frontend/src/hooks/useEntityRelationships.ts` - Relationships hook

---

## Specifications

- [spec/spec-ui-entity-workflows.md](../spec/spec-ui-entity-workflows.md) - Entity workflow patterns
- [spec/spec-ui-api-integration.md](../spec/spec-ui-api-integration.md) - API integration
- [BACKEND-UI-ALIGNMENT.md](../BACKEND-UI-ALIGNMENT.md) - Backend alignment

---

## Detailed Tasks

### Phase 1: Foundation & Shared Components (12-16 hours)

**Entity Detail API:**
- [ ] Add getEntityById functions to entitiesApi.ts for all 9 types
- [ ] Add getEntityRelationships function (GET /entities/{id}/relationships)
- [ ] Add deleteEntity function for all 9 types
- [ ] Add error handling for 404 (not found), 403 (forbidden), 500

**Shared Components:**
- [ ] Create EntityDetailTemplate (layout with tabs: Overview, Relationships, Audit)
- [ ] Create EntityHeader (breadcrumbs, title, badges, action buttons)
- [ ] Create PropertyGrid (display key-value pairs in responsive grid)
- [ ] Create RelationshipsTab (display related entities grouped by type)
- [ ] Create AuditTab (display event sourcing history)
- [ ] Create DeleteConfirmModal (confirmation before delete)

**Custom Hooks:**
- [ ] Create useEntityDetail hook (fetch entity by ID, handle loading/error)
- [ ] Create useEntityRelationships hook (fetch relationships)
- [ ] Create useEntityDelete hook (delete with confirmation, redirect to list)

### Phase 2: Application Detail Page (5-7 hours)
- [ ] Create ApplicationDetailPage
- [ ] Display properties: ID, Name, Description, Type, Status, Version, Owner, URL, Created, Updated
- [ ] Add tabs: Overview, Relationships, Audit
- [ ] Show relationships: Servers, Integrations, ApplicationServices, ApplicationInterfaces
- [ ] Add actions: Edit, Delete, Archive
- [ ] Handle loading state
- [ ] Handle error state (404, 403, 500)
- [ ] Connect to GET /applications/{id}

### Phase 3: Server Detail Page (5-7 hours)
- [ ] Create ServerDetailPage
- [ ] Display properties: ID, Name, Hostname, IP Address, Environment, Status, OS, CPU, Memory, Owner
- [ ] Add tabs: Overview, Relationships, Audit
- [ ] Show relationships: Applications, Integrations
- [ ] Add actions: Edit, Delete
- [ ] Connect to GET /servers/{id}

### Phase 4: Integration Detail Page (5-7 hours)
- [ ] Create IntegrationDetailPage
- [ ] Display properties: ID, Name, Type, Protocol, Source, Target, Status, Config, Created
- [ ] Add tabs: Overview, Relationships, Audit
- [ ] Show relationships: Source Application, Target Application, DataEntities
- [ ] Add actions: Edit, Delete
- [ ] Connect to GET /integrations/{id}

### Phase 5: DataEntity Detail Page (5-7 hours)
- [ ] Create DataEntityDetailPage
- [ ] Display properties: ID, Name, Type, Format, Classification, Schema, Owner, Created
- [ ] Add tabs: Overview, Relationships, Audit
- [ ] Show relationships: Applications, Integrations
- [ ] Add actions: Edit, Delete
- [ ] Connect to GET /data-entities/{id}

### Phase 6: BusinessCapability Detail Page (5-7 hours)
- [ ] Create BusinessCapabilityDetailPage
- [ ] Display properties: ID, Name, Description, Level, Parent, Owner, Status, Created
- [ ] Add tabs: Overview, Relationships, Audit
- [ ] Show relationships: Child Capabilities, Applications, Organizations
- [ ] Add actions: Edit, Delete
- [ ] Connect to GET /business-capabilities/{id}

### Phase 7: Organization Detail Page (4-6 hours)
- [ ] Create OrganizationDetailPage
- [ ] Display properties: ID, Name, Type, Parent, Owner, Contact, Created
- [ ] Add tabs: Overview, Relationships, Audit
- [ ] Show relationships: Child Organizations, Applications, BusinessCapabilities
- [ ] Add actions: Edit, Delete
- [ ] Connect to GET /organizations/{id}

### Phase 8: Relation Detail Page (3-5 hours)
- [ ] Create RelationDetailPage
- [ ] Display properties: ID, Type, Source Entity, Target Entity, Metadata, Created
- [ ] Add tabs: Overview, Audit (no relationships tab)
- [ ] Add actions: Edit, Delete
- [ ] Connect to GET /relations/{id}

### Phase 9: ApplicationService Detail Page (4-6 hours)
- [ ] Create ApplicationServiceDetailPage
- [ ] Display properties: ID, Name, Application, Protocol, Endpoint, Status, Created
- [ ] Add tabs: Overview, Relationships, Audit
- [ ] Show relationships: Application, ApplicationInterfaces
- [ ] Add actions: Edit, Delete
- [ ] Connect to GET /application-services/{id}

### Phase 10: ApplicationInterface Detail Page (4-6 hours)
- [ ] Create ApplicationInterfaceDetailPage
- [ ] Display properties: ID, Name, Type, Protocol, Source App, Target App, Status, Created
- [ ] Add tabs: Overview, Relationships, Audit
- [ ] Show relationships: Source Application, Target Application, ApplicationServices
- [ ] Add actions: Edit, Delete
- [ ] Connect to GET /application-interfaces/{id}

---

## Acceptance Criteria

**For Each Entity Type:**
- [ ] Detail page displays all entity properties
- [ ] Overview tab shows all relevant information
- [ ] Relationships tab shows all related entities
- [ ] Relationships grouped by entity type
- [ ] Relationships are clickable (navigate to related entity)
- [ ] Audit tab shows event history (if available)
- [ ] Edit button navigates to edit form
- [ ] Delete button shows confirmation modal
- [ ] Delete action removes entity and redirects to list
- [ ] Loading state shown during API call
- [ ] Error state shown for 404 (not found)
- [ ] Error state shown for 403 (forbidden - missing permissions)
- [ ] Error state shown for 500 (server error)
- [ ] Breadcrumbs show correct navigation path
- [ ] Page title includes entity name

**General:**
- [ ] All 9 entity detail pages implemented
- [ ] Consistent UI across all detail pages
- [ ] Responsive design (mobile, tablet, desktop)
- [ ] Accessibility: keyboard navigation, screen readers
- [ ] Performance: <1s page load

---

## Dependencies

**Blocked by:**  
- Item-075 (Frontend project setup)
- Item-076 (Component library - needs Tabs, PropertyGrid, Modal)
- Item-077 (Authentication)
- Item-078 (Routing)
- Item-079 (Entity list pages - navigation from list)

**Blocks:**  
- Item-081 (Entity forms - navigation to edit)

---

## Notes

- Use TanStack Query for data fetching with caching
- Implement breadcrumbs with entity name (e.g., Home > Applications > MyApp)
- Add permission checks before showing Edit/Delete actions
- Consider adding export functionality (export single entity as JSON)
- Add "Copy ID" button for easy reference
- Implement relationship graph visualization (optional, Phase 2)
- Add activity timeline visualization for audit history (optional)
- Consider adding "Related entities" count badges
- Cache entity data for smooth navigation
