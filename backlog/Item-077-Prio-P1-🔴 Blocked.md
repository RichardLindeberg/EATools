# Item-077: Implement Authentication Pages & Token Management

**Status:** � Blocked  
**Priority:** P1 - HIGH  
**Effort:** 32-40 hours  
**Created:** 2026-01-17  
**Owner:** Frontend Team

---

## Problem Statement

Users need a way to authenticate with the EATool application. The backend API uses JWT tokens for authentication, but currently there are no frontend pages for login, token management, or session handling.

Without authentication pages, users cannot access protected features, and the application cannot enforce role-based permissions defined in the specification.

This implementation must follow the authentication flow specified in [spec-ui-auth-permissions.md](../spec/spec-ui-auth-permissions.md), including JWT token storage, automatic refresh, and session timeout handling.

---

## Affected Files

**Create:**
- `frontend/src/pages/auth/LoginPage.tsx` - Login page
- `frontend/src/pages/auth/LogoutPage.tsx` - Logout confirmation
- `frontend/src/pages/auth/PasswordResetPage.tsx` - Password reset request
- `frontend/src/pages/auth/PasswordResetConfirmPage.tsx` - Password reset confirmation
- `frontend/src/hooks/useAuth.ts` - Authentication hook
- `frontend/src/contexts/AuthContext.tsx` - Auth context provider
- `frontend/src/utils/tokenManager.ts` - Token storage and refresh logic
- `frontend/src/api/authApi.ts` - Authentication API calls
- `frontend/src/components/auth/ProtectedRoute.tsx` - Protected route wrapper
- `frontend/src/components/auth/SessionTimeout.tsx` - Session timeout handler
- `frontend/src/types/auth.ts` - Authentication TypeScript types

**Update:**
- `frontend/src/App.tsx` - Wrap with AuthProvider
- `frontend/src/api/client.ts` - Add auth interceptors

---

## Specifications

- [spec/spec-ui-auth-permissions.md](../spec/spec-ui-auth-permissions.md) - Authentication and authorization requirements
- [spec/spec-ui-routing-navigation.md](../spec/spec-ui-routing-navigation.md) - Auth routing requirements
- [BACKEND-UI-ALIGNMENT.md](../BACKEND-UI-ALIGNMENT.md) - Backend auth alignment

---

## Detailed Tasks

### Token Management (8-10 hours)
- [ ] Create tokenManager utility for localStorage operations
- [ ] Implement JWT access token storage (15 min expiration)
- [ ] Implement JWT refresh token storage (7 day expiration)
- [ ] Create token refresh logic with automatic retry
- [ ] Implement token expiration detection
- [ ] Create token validation utility
- [ ] Add security measures (HttpOnly consideration, XSS protection)

### Auth Context & Hooks (8-10 hours)
- [ ] Create AuthContext with user state, permissions, loading states
- [ ] Implement useAuth hook for consuming auth context
- [ ] Create login() function with API integration
- [ ] Create logout() function with token cleanup
- [ ] Create refreshAccessToken() function
- [ ] Implement user permissions checker
- [ ] Add session timeout tracking (idle + absolute timeout)

### API Integration (6-8 hours)
- [ ] Create authApi.ts with login, logout, refresh endpoints
- [ ] Update Axios client with request interceptor (inject access token)
- [ ] Update Axios client with response interceptor (handle 401, auto-refresh)
- [ ] Implement retry logic for failed requests after token refresh
- [ ] Add error handling for auth failures
- [ ] Create API call queue during token refresh

### Login Page (6-8 hours)
- [ ] Create LoginPage component with email/password form
- [ ] Add form validation (required fields, email format)
- [ ] Implement login submission handler
- [ ] Add loading state during authentication
- [ ] Display authentication errors
- [ ] Add "Remember me" checkbox (optional)
- [ ] Add "Forgot password?" link
- [ ] Implement redirect to original destination after login

### Protected Routes (4-6 hours)
- [ ] Create ProtectedRoute component wrapper
- [ ] Implement authentication check before rendering
- [ ] Redirect to /login if not authenticated
- [ ] Store original destination for post-login redirect
- [ ] Add permission-based route protection
- [ ] Create role-based route guards

### Logout & Session Management (4-6 hours)
- [ ] Create LogoutPage with confirmation
- [ ] Implement logout logic (clear tokens, clear context)
- [ ] Create SessionTimeout component for idle detection
- [ ] Add warning modal before session expires
- [ ] Implement auto-logout on token expiration
- [ ] Add "Extend session" functionality

### Password Reset (Optional - 6-8 hours)
- [ ] Create PasswordResetPage for email input
- [ ] Create PasswordResetConfirmPage for new password
- [ ] Integrate with backend password reset API (if available)
- [ ] Add email validation
- [ ] Add password strength indicator
- [ ] Display success/error messages

---

## Acceptance Criteria

- [ ] Users can log in with email and password
- [ ] Access tokens stored securely in localStorage
- [ ] Refresh tokens stored securely in localStorage
- [ ] Access token automatically refreshed before expiration
- [ ] 401 responses trigger automatic token refresh and retry
- [ ] Users redirected to login page if not authenticated
- [ ] Users redirected to original destination after successful login
- [ ] Protected routes check authentication before rendering
- [ ] Session timeout warning displayed 2 minutes before expiration
- [ ] Users automatically logged out after session expires
- [ ] Logout clears all tokens and auth state
- [ ] Auth state persists across page refreshes
- [ ] All auth flows tested and working
- [ ] Loading states displayed during auth operations
- [ ] Error messages displayed for auth failures

---

## Dependencies

**Blocked by:**  
- Item-075 (Frontend project setup)
- Item-076 (Component library - needs Button, TextInput, Modal)

**Blocks:**  
- Item-078 (Routing with protected routes)
- Items 079-084 (All entity pages require authentication)

---

## Notes

- **Important:** Backend auth endpoints (POST /auth/login, POST /auth/refresh) are not yet implemented (identified in FRONTEND-READINESS-REPORT.md). This item may need to wait for backend auth implementation or use mock authentication for initial development.
- Store tokens in localStorage (consider httpOnly cookies for production)
- Access token expiry: 15 minutes
- Refresh token expiry: 7 days
- Use secure flag on cookies in production
- Consider implementing CSRF protection
- Test with backend /health endpoint initially if auth endpoints unavailable
- Follow OWASP authentication best practices
- Consider adding 2FA support in Phase 2
