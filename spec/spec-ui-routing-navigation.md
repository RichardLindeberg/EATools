---
title: Routing & Navigation Specification
version: 1.0
date_created: 2026-01-17
owner: EA Platform Team
tags: [routing, navigation, url-structure, spa]
---

# Routing & Navigation Specification

## 1. Purpose & Scope

This specification defines the URL structure, route hierarchy, navigation menu, and deep linking patterns for the EA Tool frontend. It ensures consistent, bookmarkable, and SEO-friendly URLs.

## 2. Routing Framework

### Technology Stack
```typescript
// Recommended: React Router v6+
import { BrowserRouter, Routes, Route, useNavigate, useParams } from 'react-router-dom';

// URL scheme: /module/resource/action
// Example: /applications/app-123/edit
```

### Route Configuration Pattern
```typescript
interface RouteConfig {
  path: string;           // URL pattern
  name: string;          // Display name
  component: FC;         // React component
  icon?: IconType;       // Sidebar icon
  permission?: string;   // Required permission
  children?: RouteConfig[]; // Nested routes
  layout?: 'default' | 'minimal'; // Layout type
}
```

## 3. Main Route Structure

### Root Level Routes
```
/ ............................ Dashboard (or redirect to first accessible module)
/auth ......................... Authentication module
  /login ....................... Login page
  /logout ...................... Logout (clear session, redirect)
  /register .................... User registration (if enabled)
  /forgot-password ............. Password reset
  /reset-password/:token ....... Password reset form

/admin ........................ Administration module
  /settings .................... System settings
  /users ....................... User management
  /audit ....................... Audit log viewer
  /integrations-config ......... Integration settings
```

## 4. Main Navigation Menu

### Sidebar Navigation Structure
```typescript
const mainMenu: MenuItem[] = [
  {
    id: 'dashboard',
    label: 'Dashboard',
    icon: 'dashboard',
    path: '/dashboard'
  },
  {
    id: 'applications',
    label: 'Applications',
    icon: 'application',
    path: '/applications',
    children: [
      { id: 'app-list', label: 'All Applications', path: '/applications' },
      { id: 'app-create', label: 'Create', path: '/applications/new' },
      { id: 'app-templates', label: 'Templates', path: '/applications/templates' }
    ]
  },
  {
    id: 'infrastructure',
    label: 'Infrastructure',
    icon: 'server',
    children: [
      { id: 'servers', label: 'Servers', path: '/servers' },
      { id: 'services', label: 'Services', path: '/services' },
      { id: 'interfaces', label: 'Interfaces', path: '/interfaces' }
    ]
  },
  {
    id: 'integrations',
    label: 'Integrations',
    icon: 'link',
    path: '/integrations'
  },
  {
    id: 'data',
    label: 'Data & Capabilities',
    icon: 'database',
    children: [
      { id: 'entities', label: 'Data Entities', path: '/data-entities' },
      { id: 'capabilities', label: 'Business Capabilities', path: '/capabilities' }
    ]
  },
  {
    id: 'organization',
    label: 'Organization',
    icon: 'organization',
    children: [
      { id: 'orgs', label: 'Organizations', path: '/organizations' },
      { id: 'relations', label: 'Relationships', path: '/relationships' }
    ]
  },
  {
    id: 'admin',
    label: 'Administration',
    icon: 'settings',
    permission: 'admin:view',
    path: '/admin'
  },
  {
    id: 'help',
    label: 'Help & Documentation',
    icon: 'help',
    children: [
      { id: 'guide', label: 'User Guide', path: '/help/guide' },
      { id: 'api', label: 'API Documentation', path: '/api/docs' },
      { id: 'about', label: 'About', path: '/help/about' }
    ]
  }
];
```

## 5. Entity Resource Routes

### Applications Routes
```
/applications ........................... List view (with filters/search/sort/pagination)
  ?environment=prod
  ?owner=user@example.com
  ?status=active
  ?page=1&pageSize=20

/applications/new ....................... Create new application
/applications/templates ................. Application templates
/applications/:id ....................... Detail view (Overview tab default)
  /overview ............................ Overview properties
  /architecture ........................ Architecture view
  /integrations ........................ Related integrations
  /servers ............................ Deployed servers
  /services ........................... Application services
  /interfaces ......................... Exposed interfaces
  /audit .............................. Change history

/applications/:id/edit .................. Edit application
/applications/:id/clone ................. Clone existing application
/applications/:id/settings .............. Application-specific settings
```

### Servers Routes
```
/servers ................................ List view
  ?environment=prod
  ?status=running
  ?osType=linux
  ?owner=team

/servers/new ............................ Create new server
/servers/:id ............................ Detail view (Overview tab default)
  /overview ........................... Overview
  /applications ....................... Deployed applications
  /integrations ....................... Connected integrations
  /network ............................ Network configuration
  /performance ........................ Performance metrics
  /audit .............................. Change history

/servers/:id/edit ....................... Edit server
/servers/:id/connect .................... Connect to server (SSH/RDP) - admin only
```

### Integrations Routes
```
/integrations ........................... List view
  ?protocol=rest
  ?status=active
  ?frequency=daily
  ?owner=team

/integrations/new ....................... Create integration
/integrations/:id ....................... Detail view (Overview tab default)
  /overview ........................... Overview
  /source-target ....................... Source/Target systems
  /data-contract ....................... Field mapping
  /performance ........................ SLA metrics
  /history ............................ Transaction history
  /audit .............................. Change history

/integrations/:id/edit .................. Edit integration
/integrations/:id/test .................. Test integration
/integrations/:id/monitor ............... Monitor live runs
```

### Data Entities Routes
```
/data-entities .......................... List view
  ?classification=confidential
  ?owner=department
  ?hasPii=true
  ?status=active

/data-entities/new ...................... Create data entity
/data-entities/:id ...................... Detail view (Overview tab default)
  /overview ........................... Overview
  /lineage ............................ Data flow lineage
  /schema ............................. Column definitions
  /retention .......................... Retention policies
  /sensitivity ........................ PII/Encryption status
  /audit .............................. Change history

/data-entities/:id/edit ................. Edit data entity
```

### Business Capabilities Routes
```
/capabilities ........................... List view
  ?status=active
  ?owner=department

/capabilities/new ....................... Create capability
/capabilities/:id ....................... Detail view (Overview tab default)
  /overview ........................... Overview
  /supported-by ........................ Supporting applications
  /processes .......................... Associated processes
  /data ................................ Related data entities
  /roadmap ............................ Strategic timeline
  /audit .............................. Change history

/capabilities/:id/edit .................. Edit capability
```

### Relationships Routes
```
/relationships .......................... List view
  ?type=owns
  ?sourceType=application
  ?targetType=server

/relationships/new ...................... Create relationship
/relationships/:id ...................... Detail view
  Edit/Delete actions

/relationships/:id/impact-analysis ..... Show impact analysis
```

### Organization Routes
```
/organizations .......................... List view
/organizations/new ...................... Create organization
/organizations/:id ...................... Detail view
  /overview ........................... Overview
  /departments ........................ Department hierarchy
  /team-members ....................... Team membership
  /audit .............................. Change history

/organizations/:id/edit ................. Edit organization
```

## 6. Query Parameters Pattern

### Universal Query Parameters
```typescript
// Pagination
?page=1
?pageSize=20
?skip=0&take=20
?cursor=entity-id&direction=forward&limit=20

// Sorting
?sort=name:asc
?sort=modified:desc,name:asc

// Filtering
?filter[environment]=production
?filter[status]=active
?filter[created][gte]=2026-01-01
?filter[created][lte]=2026-01-31

// Search
?search=term

// View options
?view=table|cards|timeline
?columns=name,owner,status

// State preservation
?tab=relationships
?expandAll=true
```

### Example Complex URL
```
/applications?environment=production&owner=team&status=active&sort=modified:desc&page=1&pageSize=50&search=payment
```

## 7. URL State Management

### Bookmarkable Views
```typescript
// Current filters, sort, page, tab = preserved in URL
// User can bookmark and return to exact view state
// Back/forward browser buttons work correctly
// Share link includes all view state

// Implementation pattern:
const [filters, setFilters] = useSearchParams();

const handleFilterChange = (key: string, value: string) => {
  const newParams = new URLSearchParams(filters);
  newParams.set(key, value);
  setFilters(newParams);
};
```

### Deep Linking Examples
```
Share list view with filters:
/applications?environment=prod&status=active

Share detail view with tab:
/applications/app-123#relationships

Share search results:
/integrations?search=database&protocol=rest

Bookmarkable filtered exports:
/servers?environment=staging&export=true&format=csv
```

## 8. Breadcrumb Navigation

### Breadcrumb Generation
```typescript
type BreadcrumbItem = {
  label: string;
  path?: string;
};

// Dynamic breadcrumbs based on route
const generateBreadcrumbs = (pathname: string): BreadcrumbItem[] => {
  const breadcrumbs: BreadcrumbItem[] = [
    { label: 'Home', path: '/' }
  ];

  // Examples:
  // /applications → [Home, Applications]
  // /applications/app-123 → [Home, Applications, Application Name]
  // /applications/app-123/edit → [Home, Applications, Application Name, Edit]
  // /integrations/int-456/data-contract → [Home, Integrations, Integration Name, Data Contract]

  return breadcrumbs;
};
```

### Breadcrumb Behavior
- Fixed at top of main content area
- Last item is non-clickable
- Max 5 items visible, older items collapse into ellipsis menu
- Clicking item navigates to that path
- Responsive: collapses on mobile

## 9. Modal/Dialog Routes (Optional)

### Modal-based Navigation Pattern
```typescript
// Option 1: Modal in URL params
/applications?modal=edit&id=app-123

// Option 2: Keep context, show modal
/applications/app-123?mode=edit

// Option 3: Separate modal routes
/applications/app-123/modal/edit

// Implementation: Use URL state to control modal visibility
const [params] = useSearchParams();
const showModal = params.get('modal') === 'true';
```

## 10. Authentication & Protected Routes

### Route Protection Pattern
```typescript
interface ProtectedRouteProps {
  component: FC;
  permission?: string;
  path: string;
}

const ProtectedRoute: FC<ProtectedRouteProps> = ({ component: Component, permission, ...rest }) => {
  const { isAuthenticated, hasPermission } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/auth/login" state={{ from: location }} />;
  }

  if (permission && !hasPermission(permission)) {
    return <Navigate to="/unauthorized" />;
  }

  return <Component {...rest} />;
};
```

### Protected Routes
```
/admin ............................ Requires: admin:view permission
/admin/settings ................... Requires: admin:manage
/admin/users ...................... Requires: admin:manage-users
/admin/audit ...................... Requires: admin:audit-view

/applications/:id/edit ............ Requires: app:edit (or owner)
/servers/:id/connect .............. Requires: infrastructure:admin
/integrations/:id/test ............ Requires: integration:manage
/data-entities/:id/edit ........... Requires: data:manage
```

## 11. Error Routes

### Error Handling Routes
```
/error ............................ Generic error page
/404 ............................. Not found
/unauthorized .................... Permission denied (401/403)
/error/:code ...................... Specific error page

// Automatic redirects:
Invalid route → /404
Expired session → /auth/login (with returnUrl)
Permission denied → /unauthorized
```

## 12. Special Routes

### Special Navigation Paths
```
/ ................................ Redirect to /dashboard (or first accessible module)
/help ............................. Help documentation
  /guide .......................... User guide
  /api ............................ API documentation
  /feedback ....................... Send feedback

/profile .......................... User profile
  /settings ....................... Profile settings
  /preferences .................... User preferences
  /password ....................... Change password

/search ........................... Global search results
  ?q=term
  ?types=applications,servers,integrations
```

## 13. Navigation History Management

### Browser History Pattern
```typescript
// Navigation patterns:
- Forward navigation (list → detail): Use navigate('/path')
- Back navigation: Use navigate(-1)
- Conditional navigation: Use useNavigate hook
- Replace state: Use navigate('/path', { replace: true })

// Preserve history:
- Sidebar link: Normal forward history
- Filter/sort change: Replace history (don't clutter back button)
- Search: Replace history
- Tab switch: Replace history
```

## 14. Redirect Rules

### Automatic Redirects
```typescript
const redirectRules = [
  // Old URLs → New URLs
  { from: '/entity', to: '/entities' },
  { from: '/infrastructure', to: '/servers' },
  
  // Root redirects
  { from: '/', to: '/dashboard' },
  
  // Cleanup
  { from: '/admin/*', to: '/admin' }
];
```

## 15. URL Best Practices

### Naming Conventions
- Use lowercase: `/applications` not `/Applications`
- Use hyphens for multi-word: `/data-entities` not `/dataEntities`
- Use resource names (plural): `/applications` not `/app`
- IDs as last segment: `/applications/123` not `/app-123`
- Actions as separate segments: `/applications/123/edit` not `/applications/123?mode=edit` (unless complex)

### Query vs. Path Parameter Decision
```
Use path parameters when:
- Identifying specific resource
- Unique resource identifier required
- RESTful pattern preferred

Use query parameters when:
- Filtering, sorting, pagination
- Optional state
- Multiple independent filters
- View configuration

Examples:
/applications/:id ............. Path parameter (resource ID)
?environment=prod ............. Query parameter (filter)
?page=2 ....................... Query parameter (pagination)
?sort=name:asc ................ Query parameter (sorting)
```

## 16. Mobile Navigation

### Mobile-Specific Routes
```
/mobile-menu ..................... Mobile menu (drawer/hamburger)
  Shows sidebar as overlay
  
Responsive behavior:
- Desktop: Sidebar always visible + main content
- Tablet: Collapsible sidebar + main content
- Mobile: Hamburger menu + full-width content
```

## 17. Validation Criteria

Navigation implementation must support:
- [ ] Bookmarkable URLs for all views
- [ ] Back/forward buttons work correctly
- [ ] Browser history preserved correctly
- [ ] Deep links work from external sources
- [ ] Query parameters preserved on refresh
- [ ] Protected routes enforce permissions
- [ ] Redirects work transparently
- [ ] Mobile navigation responsive
- [ ] Breadcrumbs auto-generated correctly
- [ ] 404 pages handled gracefully

## 18. Related Specifications

- [spec-design-ui-architecture.md](spec-design-ui-architecture.md) - Navigation component specs
- [spec-ui-auth-permissions.md](spec-ui-auth-permissions.md) - Permission-based routing
- [spec-ui-api-integration.md](spec-ui-api-integration.md) - Data fetching for routes
