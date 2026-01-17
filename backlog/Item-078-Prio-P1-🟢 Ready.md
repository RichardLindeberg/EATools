# Item-078: Implement Routing & Navigation Structure

**Status:** � Blocked  
**Priority:** P1 - HIGH  
**Effort:** 24-32 hours  
**Created:** 2026-01-17  
**Owner:** Frontend Team

---

## Problem Statement

The EATool application requires a comprehensive routing structure with 25+ routes covering authentication, admin functions, and 9 entity types (each with list, detail, create, edit views). Without proper routing, users cannot navigate between different parts of the application.

The routing implementation must support protected routes, permission-based access control, deep linking, breadcrumb generation, and query parameter handling as specified in [spec-ui-routing-navigation.md](../spec/spec-ui-routing-navigation.md).

---

## Affected Files

**Create:**
- `frontend/src/router/routes.tsx` - Main route configuration
- `frontend/src/router/index.tsx` - Router setup
- `frontend/src/layouts/MainLayout.tsx` - Main application layout
- `frontend/src/layouts/AuthLayout.tsx` - Authentication pages layout
- `frontend/src/layouts/AdminLayout.tsx` - Admin pages layout
- `frontend/src/pages/HomePage.tsx` - Dashboard/home page
- `frontend/src/pages/NotFoundPage.tsx` - 404 error page
- `frontend/src/pages/UnauthorizedPage.tsx` - 403 error page
- `frontend/src/hooks/useQueryParams.ts` - Query parameter hook
- `frontend/src/hooks/useBreadcrumbs.ts` - Breadcrumb generation hook
- `frontend/src/utils/routeGuards.ts` - Permission-based route guards

**Update:**
- `frontend/src/App.tsx` - Integrate router

---

## Specifications

- [spec/spec-ui-routing-navigation.md](../spec/spec-ui-routing-navigation.md) - Complete routing requirements
- [spec/spec-ui-auth-permissions.md](../spec/spec-ui-auth-permissions.md) - Permission-based routing
- [BACKEND-UI-ALIGNMENT.md](../BACKEND-UI-ALIGNMENT.md) - API endpoint alignment

---

## Detailed Tasks

### Router Setup (6-8 hours)
- [ ] Install and configure React Router v6
- [ ] Create router configuration with all 25+ routes
- [ ] Setup route hierarchy (auth, admin, entities)
- [ ] Configure hash vs history mode
- [ ] Add scroll restoration
- [ ] Implement route transition animations (optional)

### Authentication Routes (4-6 hours)
- [ ] `/login` - Login page
- [ ] `/logout` - Logout confirmation
- [ ] `/password-reset` - Password reset request
- [ ] `/password-reset/confirm` - Password reset confirmation
- [ ] `/unauthorized` - 403 error page

### Admin Routes (4-6 hours)
- [ ] `/admin/users` - User management
- [ ] `/admin/roles` - Role management
- [ ] `/admin/permissions` - Permission management
- [ ] `/admin/settings` - Application settings
- [ ] Add admin permission guards (require admin:* permission)

### Entity Routes - 9 Types × 4 Routes Each (36 routes) (8-12 hours)
**Applications:**
- [ ] `/applications` - Application list
- [ ] `/applications/:id` - Application detail
- [ ] `/applications/new` - Create application
- [ ] `/applications/:id/edit` - Edit application

**Servers:**
- [ ] `/servers` - Server list
- [ ] `/servers/:id` - Server detail
- [ ] `/servers/new` - Create server
- [ ] `/servers/:id/edit` - Edit server

**Integrations:**
- [ ] `/integrations` - Integration list
- [ ] `/integrations/:id` - Integration detail
- [ ] `/integrations/new` - Create integration
- [ ] `/integrations/:id/edit` - Edit integration

**Data Entities:**
- [ ] `/data-entities` - Data entity list
- [ ] `/data-entities/:id` - Data entity detail
- [ ] `/data-entities/new` - Create data entity
- [ ] `/data-entities/:id/edit` - Edit data entity

**Business Capabilities:**
- [ ] `/business-capabilities` - Business capability list
- [ ] `/business-capabilities/:id` - Business capability detail
- [ ] `/business-capabilities/new` - Create business capability
- [ ] `/business-capabilities/:id/edit` - Edit business capability

**Organizations:**
- [ ] `/organizations` - Organization list
- [ ] `/organizations/:id` - Organization detail
- [ ] `/organizations/new` - Create organization
- [ ] `/organizations/:id/edit` - Edit organization

**Relations:**
- [ ] `/relations` - Relation list
- [ ] `/relations/:id` - Relation detail
- [ ] `/relations/new` - Create relation
- [ ] `/relations/:id/edit` - Edit relation

**Application Services:**
- [ ] `/application-services` - Application service list
- [ ] `/application-services/:id` - Application service detail
- [ ] `/application-services/new` - Create application service
- [ ] `/application-services/:id/edit` - Edit application service

**Application Interfaces:**
- [ ] `/application-interfaces` - Application interface list
- [ ] `/application-interfaces/:id` - Application interface detail
- [ ] `/application-interfaces/new` - Create application interface
- [ ] `/application-interfaces/:id/edit` - Edit application interface

### Route Guards & Protection (4-6 hours)
- [ ] Implement ProtectedRoute wrapper (check authentication)
- [ ] Implement PermissionRoute wrapper (check specific permissions)
- [ ] Create route guard for entity routes (e.g., app:read for /applications)
- [ ] Redirect to /login if not authenticated
- [ ] Redirect to /unauthorized if missing permissions
- [ ] Store original destination for post-login redirect

### Layouts (4-6 hours)
- [ ] Create MainLayout with Header, Sidebar, Breadcrumbs
- [ ] Create AuthLayout (no sidebar, centered content)
- [ ] Create AdminLayout (additional admin sidebar)
- [ ] Implement responsive layout (mobile, tablet, desktop)
- [ ] Add layout transition animations

### Query Parameters & Deep Linking (4-6 hours)
- [ ] Create useQueryParams hook for reading/writing query params
- [ ] Support pagination params (skip, take)
- [ ] Support sort params (sort=field.asc or sort=field.desc)
- [ ] Support filter params (filter[key]=value)
- [ ] Support search param (search=query)
- [ ] Support view param (view=list|grid|card)
- [ ] Support export param (export=csv|json)
- [ ] Implement query param persistence across navigation

### Breadcrumbs (4-6 hours)
- [ ] Create useBreadcrumbs hook for dynamic generation
- [ ] Generate breadcrumbs from route hierarchy
- [ ] Include entity names in breadcrumbs (e.g., Home > Applications > App123)
- [ ] Add breadcrumb navigation in MainLayout
- [ ] Style breadcrumbs with separators

---

## Acceptance Criteria

- [ ] All 25+ routes configured and accessible
- [ ] Protected routes require authentication
- [ ] Permission routes check user permissions
- [ ] Unauthenticated users redirected to /login
- [ ] Unauthorized users redirected to /unauthorized
- [ ] Deep links work correctly (e.g., /applications?skip=20&take=10)
- [ ] Query parameters preserved during navigation
- [ ] Breadcrumbs generated correctly for all routes
- [ ] Breadcrumbs include entity names where applicable
- [ ] 404 page shown for invalid routes
- [ ] Browser back/forward buttons work correctly
- [ ] Layouts render correctly (MainLayout, AuthLayout, AdminLayout)
- [ ] Sidebar highlights active route
- [ ] Mobile navigation works (hamburger menu)

---

## Dependencies

**Blocked by:**  
- Item-075 (Frontend project setup)
- Item-076 (Component library - needs Header, Sidebar, Breadcrumbs)
- Item-077 (Authentication - needs ProtectedRoute)

**Blocks:**  
- Items 079-084 (All page implementations need routing)

---

## Notes

- Use React Router v6 with data router pattern
- Consider using route-based code splitting for better performance
- Ensure all routes are type-safe with TypeScript
- Test deep linking thoroughly
- Consider adding route prefetching for better UX
- Implement route analytics tracking (optional)
- Follow RESTful URL conventions
- Add meta tags for SEO (optional)
