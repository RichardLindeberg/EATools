---
title: Authentication & Permission-Based UI Specification
version: 1.0
date_created: 2026-01-17
owner: EA Platform Team
tags: [auth, security, permissions, rbac, ui]
---

# Authentication & Permission-Based UI Specification

## 1. Purpose & Scope

This specification defines authentication flows, permission model, and how the UI enforces authorization at component and route levels. It ensures secure and consistent access control across the application.

## 2. Authentication Flows

### Login Flow
```
1. User navigates to /auth/login
2. Login form displayed:
   - Email/Username field
   - Password field
   - "Remember me" checkbox (optional)
   - "Forgot password?" link
   - Submit button

3. Form submission:
   - Validate fields client-side
   - POST /api/auth/login { email, password, rememberMe }
   
4. Server response (success):
   - Returns: { accessToken, refreshToken, user }
   - Store tokens in:
     * accessToken: Memory + localStorage (or session storage)
     * refreshToken: Secure httpOnly cookie
   - Store user info in app state
   
5. Success handling:
   - Clear form
   - Show success toast
   - Redirect to /dashboard (or returnUrl)
   
6. Error handling:
   - Display specific error: "Invalid credentials", "Account locked", etc.
   - Keep user on login page
   - Preserve email field for retry
```

### Logout Flow
```
1. User clicks "Logout" in header menu
2. Confirmation dialog (optional):
   "Are you sure you want to logout?"

3. On confirm:
   - POST /api/auth/logout (with refresh token)
   - Clear localStorage/sessionStorage
   - Clear app state
   - Clear cookies
   - Redirect to /auth/login
   
4. On error:
   - Still clear local state
   - Show toast: "Logged out locally"
   - Redirect to /auth/login
```

### Session Management
```
Access Token: JWT in memory + localStorage
- 15 minutes expiry
- Sent in Authorization header

Refresh Token: Secure HttpOnly cookie
- 7 days expiry
- Auto-refreshed on token expiry
- Used to obtain new access token
- Revoked on logout

Session State:
- User info: name, email, roles, permissions
- Last activity timestamp
- CSRF token (if needed)
```

### Token Refresh
```
1. API request returns 401 (token expired)
2. Automatically call:
   POST /api/auth/refresh { refreshToken }

3. Server returns new accessToken
4. Retry original request with new token
5. If refresh fails:
   - Clear auth state
   - Redirect to /auth/login
   - Show message: "Session expired, please login again"
```

### Session Timeout
```
Idle timeout: 30 minutes (configurable)

1. Track user activity (mouse, keyboard, clicks)
2. On timeout:
   - Show modal warning: "Your session will expire in 5 minutes"
   - Offer "Continue Session" button
   
3. If user continues:
   - Extend session
   - Clear warning

4. If timeout completes:
   - Clear auth state
   - Redirect to /auth/login
   - Preserve current URL for post-login redirect
```

## 3. Permission Model

### Permission Types

#### Resource-Based Permissions
```
Format: resource:action

Examples:
- app:read ........................ View applications
- app:create ..................... Create new application
- app:edit ....................... Edit application (own or all)
- app:delete ..................... Delete application
- app:export ..................... Export applications
- app:bulk-edit .................. Bulk modify

- server:read .................... View servers
- server:create .................. Create servers
- server:admin ................... Connect/configure servers

- integration:manage ............. Full integration management
- integration:monitor ............ View integration metrics

- data:manage .................... Manage data entities
- data:pii-access ................ View PII data

- report:view .................... View reports
- report:export .................. Export reports

- admin:view ..................... Access admin section
- admin:manage ................... Modify system settings
- admin:audit .................... View audit logs
- admin:user-manage .............. Manage user accounts
```

#### Role-Based Permissions
```typescript
interface Role {
  id: string;
  name: string;
  description: string;
  permissions: string[];
  isSystem: boolean; // Cannot be deleted
}

const predefinedRoles = {
  VIEWER: {
    name: 'Viewer',
    permissions: [
      'app:read', 'server:read', 'integration:read',
      'data:read', 'report:view'
    ]
  },
  EDITOR: {
    name: 'Editor',
    permissions: [
      'app:read', 'app:create', 'app:edit',
      'server:read', 'server:create', 'server:edit',
      'integration:read', 'integration:create', 'integration:edit',
      'data:read', 'data:create', 'data:edit',
      'report:view', 'report:export'
    ]
  },
  ADMIN: {
    name: 'Administrator',
    permissions: [
      'app:*', 'server:*', 'integration:*',
      'data:*', 'report:*', 'admin:*'
    ]
  },
  VIEWER_LIMITED: {
    name: 'Viewer (Limited)',
    permissions: [
      'app:read', 'report:view'
    ]
  }
};
```

### Permission Checking
```typescript
// User object in state
interface User {
  id: string;
  email: string;
  name: string;
  roles: Role[];
  permissions: string[];
  department?: string;
  manager?: string;
  resourceOwnership: {
    applications: string[];    // app IDs user owns
    servers: string[];
    integrations: string[];
    dataEntities: string[];
  };
}

// Permission check functions
const hasPermission = (user: User, permission: string): boolean => {
  // Wildcard support: 'admin:*' grants all admin:* permissions
  return user.permissions.some(p => 
    p === permission || 
    p === `${permission.split(':')[0]}:*`
  );
};

const hasAnyPermission = (user: User, permissions: string[]): boolean => {
  return permissions.some(p => hasPermission(user, p));
};

const canManageEntity = (user: User, entity: any): boolean => {
  // Admin can manage all
  if (hasPermission(user, `${entity.type}:edit`)) return true;
  
  // Owner can manage own resources
  return user.resourceOwnership[`${entity.type}s`]?.includes(entity.id);
};
```

## 4. UI Permission Enforcement

### Route-Level Protection
```typescript
const ProtectedRoute = ({ path, permission, component: Component }) => {
  const { user } = useAuth();
  
  if (!user) {
    return <Navigate to="/auth/login" />;
  }
  
  if (permission && !hasPermission(user, permission)) {
    return <Navigate to="/unauthorized" />;
  }
  
  return <Component />;
};

// Usage
<Route path="/admin" element={<ProtectedRoute permission="admin:view" component={Admin} />} />
<Route path="/admin/users" element={<ProtectedRoute permission="admin:user-manage" component={UserMgmt} />} />
```

### Component-Level Hiding
```typescript
// Permission-based rendering wrapper
const IfAllowed: FC<{ permission: string | string[]; children: ReactNode }> = 
  ({ permission, children }) => {
  const { user } = useAuth();
  const permissions = Array.isArray(permission) ? permission : [permission];
  
  if (!hasAnyPermission(user, permissions)) {
    return null; // Hide component entirely
  }
  
  return <>{children}</>;
};

// Usage
<IfAllowed permission="app:create">
  <Button onClick={openCreateForm}>+ Create Application</Button>
</IfAllowed>

<IfAllowed permission={["app:edit", "app:delete"]}>
  <DropdownMenu items={editDeleteActions} />
</IfAllowed>
```

### Button/Action Disabling
```typescript
const ActionButton: FC<{ permission: string; onClick: () => void }> = 
  ({ permission, onClick, children }) => {
  const { user } = useAuth();
  const allowed = hasPermission(user, permission);
  
  return (
    <Tooltip 
      content={!allowed ? "You don't have permission" : undefined}
      disabled={allowed}
    >
      <Button disabled={!allowed} onClick={onClick}>
        {children}
      </Button>
    </Tooltip>
  );
};

// Usage
<ActionButton permission="app:delete" onClick={handleDelete}>
  Delete
</ActionButton>
```

### Field-Level Disabling
```typescript
const ProtectedFormField = ({ permission, ...fieldProps }) => {
  const { user } = useAuth();
  const allowed = hasPermission(user, permission);
  
  return (
    <FormField 
      {...fieldProps}
      disabled={!allowed || fieldProps.disabled}
      hint={!allowed ? "You don't have permission to modify this field" : fieldProps.hint}
    />
  );
};

// Usage - Editor cannot edit "critical" flag
<ProtectedFormField 
  permission="app:admin" 
  name="critical" 
  label="Critical Application" 
  type="checkbox"
/>
```

### Row Action Visibility
```typescript
const ListViewActions: FC<{ row: Entity }> = ({ row }) => {
  const { user } = useAuth();
  
  const actions = [
    {
      label: 'Edit',
      icon: 'edit',
      onClick: () => handleEdit(row),
      visible: canManageEntity(user, row)
    },
    {
      label: 'Delete',
      icon: 'trash',
      onClick: () => handleDelete(row),
      visible: hasPermission(user, `${row.type}:delete`) && canManageEntity(user, row),
      variant: 'danger'
    },
    {
      label: 'Export',
      icon: 'download',
      onClick: () => handleExport(row),
      visible: hasPermission(user, `${row.type}:export`)
    }
  ];
  
  return (
    <DropdownMenu 
      items={actions.filter(a => a.visible)}
    />
  );
};
```

### Menu Item Visibility
```typescript
const menuItems = computed(() => {
  return [
    {
      id: 'dashboard',
      label: 'Dashboard',
      path: '/dashboard',
      visible: true // Always visible
    },
    {
      id: 'applications',
      label: 'Applications',
      path: '/applications',
      visible: hasPermission(user, 'app:read')
    },
    {
      id: 'admin',
      label: 'Administration',
      path: '/admin',
      visible: hasPermission(user, 'admin:view')
    }
  ].filter(item => item.visible);
});
```

## 5. Data Filtering by Permission

### Server-Side Filtering
```typescript
// Backend: Filter data based on user permissions
GET /api/applications?filterByPermission=true

Returns only:
- Applications user can read
- Filtered by user's department (if applicable)
- Exclude: applications in restricted departments

GET /api/servers?includeOwn=true

Returns:
- All servers user can read (if admin)
- Only user's owned servers (if regular user)
```

### UI-Side Filtering
```typescript
const getVisibleEntities = (entities: Entity[], user: User): Entity[] => {
  return entities.filter(entity => {
    // Has read permission
    if (!hasPermission(user, `${entity.type}:read`)) return false;
    
    // Check department restrictions
    if (entity.departmentRestricted && entity.department !== user.department) {
      return false;
    }
    
    // Check PII access
    if (entity.hasPii && !hasPermission(user, 'data:pii-access')) {
      return false;
    }
    
    return true;
  });
};
```

## 6. Unauthorized States

### Permission Denied UI
```
When user lacks permission:
- Hide action/button (preferred)
- Disable with tooltip: "You don't have permission"
- Show "/unauthorized" page for protected routes

Elements:
- Locked icon
- Explanatory text
- Link to admin/help

Example:
┌────────────────────────────────┐
│ 🔒 Permission Denied            │
│                                 │
│ You don't have permission to    │
│ access this feature.            │
│                                 │
│ Required permission: admin:view │
│                                 │
│ [Contact Administrator] [Help]  │
└────────────────────────────────┘
```

### Expired Permission UI
```
If user's permissions change during session:
- Detect via API response (403)
- Refresh user state
- Hide previously-accessible features
- Show warning toast:
  "Your permissions have changed. Some features are now hidden."
```

## 7. Password Management

### Change Password Flow
```
Route: /profile/password

1. Display form:
   - Current Password
   - New Password
   - Confirm Password

2. Validation:
   - Current password correct (call POST /api/auth/verify-password)
   - New password != current password
   - Passwords match
   - Password strength: 8+ chars, mixed case, numbers/symbols

3. On submit:
   - POST /api/auth/change-password { currentPassword, newPassword }
   
4. Success:
   - Show toast: "Password changed successfully"
   - Logout user (require re-login with new password)
   - Redirect to /auth/login

5. Error:
   - Show specific error: "Current password is incorrect"
```

### Forgot Password Flow
```
Route: /auth/forgot-password

1. Display form:
   - Email field

2. On submit:
   - POST /api/auth/forgot-password { email }
   - Server sends reset link to email
   
3. Response:
   - Show message: "Check your email for password reset link"
   - Don't confirm/deny whether email exists (security)

4. User clicks link in email:
   - Navigate to /auth/reset-password/:token
   
5. Reset form:
   - New Password field
   - Confirm Password field
   - Submit button

6. On submit:
   - POST /api/auth/reset-password { token, newPassword }
   
7. Success:
   - Clear form
   - Show message: "Password reset successful"
   - Redirect to /auth/login with message
```

## 8. API Security Headers

### Request Headers
```typescript
// Every API request includes:
{
  'Authorization': `Bearer ${accessToken}`,
  'X-Request-ID': generateUUID(),
  'X-CSRF-Token': csrfToken,  // If using form submissions
  'Content-Type': 'application/json'
}

// Tokens expire and refresh automatically
```

### Response Handling
```typescript
// 401 Unauthorized
- Access token expired or invalid
- Attempt token refresh
- If refresh fails: redirect to /auth/login

// 403 Forbidden
- Permission denied
- Navigate to /unauthorized
- Show error message

// 419 Session Expired
- Session timeout
- Redirect to /auth/login with timeout message
```

## 9. Audit Logging

### Permission-Related Logging
```
Log events:
- Login attempt (success/failure)
- Logout
- Permission denied action
- Token refresh
- Session timeout
- Permission change (by admin)
- Password change

API endpoint:
GET /api/audit-logs?action=login&user=user@example.com

Shown in:
- /admin/audit page (admin only)
- User's /profile/activity (own activity only)
```

## 10. Validation Criteria

Authentication & authorization must support:
- [ ] Secure token storage (no XSS exposure)
- [ ] Token refresh automation
- [ ] Session timeout enforcement
- [ ] Permission checking at routes and components
- [ ] Graceful permission denied handling
- [ ] Field-level permission control
- [ ] Row action filtering by permission
- [ ] Menu item visibility by permission
- [ ] Password security requirements
- [ ] Audit logging of security events
- [ ] CSRF protection
- [ ] Rate limiting on auth endpoints

## 11. Related Specifications

- [spec-ui-routing-navigation.md](spec-ui-routing-navigation.md) - Protected routes
- [spec-ui-api-integration.md](spec-ui-api-integration.md) - API error handling (401/403)
- [spec-design-ui-architecture.md](spec-design-ui-architecture.md) - Unauthorized state UI
