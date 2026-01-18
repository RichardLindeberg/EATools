# Item-084: Backend Authentication Endpoints Implementation

**Status:** 🟢 Ready  
**Priority:** P2 - MEDIUM  
**Effort:** 24-32 hours  
**Created:** 2026-01-17  
**Owner:** Backend Team

---

## Problem Statement

The frontend authentication implementation (Item-077) requires backend authentication endpoints that are currently missing. The backend needs POST /auth/login, POST /auth/refresh, and POST /auth/logout endpoints to support the JWT-based authentication flow.

As identified in the FRONTEND-READINESS-REPORT.md, these auth endpoints are a Phase 1 gap but can be implemented after initial frontend development using mock authentication.

Without these endpoints, the frontend cannot perform real authentication and must rely on mock authentication or bypass authentication checks.

---

## Affected Files

**Create:**
- `src/EATool/Auth/AuthTypes.fs` - Authentication domain types
- `src/EATool/Auth/AuthService.fs` - Authentication business logic
- `src/EATool/Auth/AuthEndpoints.fs` - Authentication HTTP endpoints
- `src/EATool/Auth/JwtTokenService.fs` - JWT token generation and validation
- `src/EATool/Auth/PasswordHasher.fs` - Password hashing (bcrypt)
- `src/EATool/Database/UserStore.fs` - User persistence
- `src/EATool/Database/Migrations/004_CreateUsersTable.fs` - Users table migration

**Update:**
- `src/EATool/Program.fs` - Register auth endpoints
- `src/EATool/Api/ErrorHandlers.fs` - Add UNAUTHORIZED error handling

---

## Specifications

- [spec/spec-process-authentication.md](../spec/spec-process-authentication.md) - Authentication process specification
- [spec/spec-ui-auth-permissions.md](../spec/spec-ui-auth-permissions.md) - Frontend auth requirements
- [FRONTEND-READINESS-REPORT.md](../FRONTEND-READINESS-REPORT.md) - Auth gap identification
- [BACKEND-UI-ALIGNMENT.md](../BACKEND-UI-ALIGNMENT.md) - Auth flow alignment

## Architecture & Implementation Guide

📋 **Comprehensive Implementation Architecture**: [Item_084_Authentication_Implementation_Architecture.md](../Item_084_Authentication_Implementation_Architecture.md)

This document provides:
- System context and component architecture diagrams
- Complete data flow diagrams (login, refresh, logout, me endpoints)
- Database schema with migrations
- Detailed specifications for all 7 F# modules
- Configuration and environment variables
- Test strategy and examples
- **8-phase implementation checklist** with 35+ tasks and time estimates
- Security considerations and best practices
- Deployment checklist for production

**Start here for implementation guidance.**

---

## Detailed Tasks

### Phase 1: Foundation (8-10 hours)

**Auth Domain Types:**
- [ ] Define User type (Id, Email, PasswordHash, Roles, Permissions, CreatedAt, UpdatedAt)
- [ ] Define LoginRequest type (Email, Password)
- [ ] Define LoginResponse type (AccessToken, RefreshToken, ExpiresIn, User)
- [ ] Define RefreshTokenRequest type (RefreshToken)
- [ ] Define RefreshTokenResponse type (AccessToken, ExpiresIn)
- [ ] Define Role type (VIEWER, EDITOR, ADMIN, VIEWER_LIMITED)
- [ ] Define Permission type (resource:action format, e.g., app:read, app:delete)

**Database Schema:**
- [ ] Create Users table (Id, Email, PasswordHash, Salt, Roles, CreatedAt, UpdatedAt)
- [ ] Create RefreshTokens table (Id, UserId, Token, ExpiresAt, CreatedAt, RevokedAt)
- [ ] Add indexes on Email (unique), RefreshToken (unique)
- [ ] Create migration script
- [ ] Seed test users (admin@example.com, user@example.com)

### Phase 2: JWT Token Service (6-8 hours)

**JWT Implementation:**
- [ ] Install System.IdentityModel.Tokens.Jwt NuGet package
- [ ] Create JwtTokenService with generateAccessToken function
- [ ] Create generateRefreshToken function (random secure token)
- [ ] Implement validateAccessToken function
- [ ] Implement validateRefreshToken function
- [ ] Configure JWT settings (secret key, issuer, audience, expiry)
- [ ] Access token expiry: 15 minutes
- [ ] Refresh token expiry: 7 days
- [ ] Include user claims in JWT (UserId, Email, Roles, Permissions)

**Token Storage:**
- [ ] Create RefreshTokenStore for persist/retrieve/revoke operations
- [ ] Implement token rotation (invalidate old token when refreshing)
- [ ] Add token revocation on logout
- [ ] Add automatic cleanup of expired tokens

### Phase 3: Password Hashing (4-6 hours)

**Password Security:**
- [ ] Install BCrypt.Net NuGet package
- [ ] Create PasswordHasher with hashPassword function
- [ ] Create verifyPassword function
- [ ] Configure bcrypt work factor (10-12 rounds)
- [ ] Add password strength validation (min 8 chars, uppercase, lowercase, number)

### Phase 4: Authentication Endpoints (8-10 hours)

**POST /auth/login:**
- [ ] Accept LoginRequest (email, password)
- [ ] Validate email format
- [ ] Look up user by email
- [ ] Verify password hash
- [ ] Generate access token (JWT, 15 min expiry)
- [ ] Generate refresh token (secure random, 7 day expiry)
- [ ] Store refresh token in database
- [ ] Return LoginResponse (tokens, user info)
- [ ] Return 401 if credentials invalid
- [ ] Return 400 if validation fails
- [ ] Log authentication attempts (success/failure)

**POST /auth/refresh:**
- [ ] Accept RefreshTokenRequest (refreshToken)
- [ ] Validate refresh token format
- [ ] Look up refresh token in database
- [ ] Check token not expired
- [ ] Check token not revoked
- [ ] Generate new access token
- [ ] Optionally rotate refresh token
- [ ] Return RefreshTokenResponse (new access token)
- [ ] Return 401 if token invalid/expired/revoked

**POST /auth/logout:**
- [ ] Accept refresh token in request body or header
- [ ] Revoke refresh token in database
- [ ] Return 200 OK
- [ ] Optional: Invalidate all user sessions

**GET /auth/me:**
- [ ] Require valid access token in Authorization header
- [ ] Extract user ID from JWT claims
- [ ] Fetch user from database
- [ ] Return user information (without password)
- [ ] Return 401 if token invalid/expired

### Phase 5: Authentication Middleware (4-6 hours)

**JWT Validation Middleware:**
- [ ] Create Giraffe middleware to validate JWT tokens
- [ ] Extract token from Authorization header (Bearer {token})
- [ ] Validate token signature and expiration
- [ ] Extract claims and add to HttpContext
- [ ] Return 401 if token missing/invalid/expired
- [ ] Apply middleware to protected routes

**Permission Checking:**
- [ ] Create requirePermission middleware
- [ ] Check user has required permission (from JWT claims)
- [ ] Return 403 if permission missing
- [ ] Apply to routes requiring specific permissions

---

## Acceptance Criteria

**Login Endpoint:**
- [ ] POST /auth/login accepts email and password
- [ ] Returns access token, refresh token, expiry, user info on success
- [ ] Returns 401 for invalid credentials
- [ ] Returns 400 for validation errors (invalid email format)
- [ ] Access token is valid JWT with 15 min expiry
- [ ] Refresh token stored in database with 7 day expiry
- [ ] JWT includes user ID, email, roles, permissions

**Refresh Endpoint:**
- [ ] POST /auth/refresh accepts refresh token
- [ ] Returns new access token on success
- [ ] Returns 401 for invalid/expired/revoked token
- [ ] Old refresh token optionally revoked (token rotation)

**Logout Endpoint:**
- [ ] POST /auth/logout accepts refresh token
- [ ] Revokes refresh token in database
- [ ] Returns 200 OK
- [ ] Subsequent refresh attempts fail with 401

**Me Endpoint:**
- [ ] GET /auth/me requires valid access token
- [ ] Returns user information (no password)
- [ ] Returns 401 if token missing/invalid

**Security:**
- [ ] Passwords hashed with bcrypt (never stored plain text)
- [ ] JWT secret key stored securely (environment variable)
- [ ] Refresh tokens cryptographically secure random
- [ ] Expired tokens cannot be used
- [ ] Revoked tokens cannot be used
- [ ] Rate limiting on login endpoint (prevent brute force) - optional
- [ ] Login attempts logged (audit trail)

**Integration:**
- [ ] Frontend can authenticate using these endpoints
- [ ] Protected entity endpoints require valid JWT
- [ ] Permission checks work correctly on entity endpoints
- [ ] Token refresh works automatically before expiry

---

## Dependencies

**Blocked by:** None (backend is functional, auth is addition)  
**Blocks:** Item-077 (Frontend auth - currently using mock)

---

## Notes

- **Priority P2 (Medium):** Frontend can develop using mock authentication initially
- Consider using existing library like AspNetCore.Authentication.JwtBearer (if applicable in F#)
- Store JWT secret in environment variable, never commit to repo
- Consider adding refresh token rotation for extra security
- Add rate limiting to prevent brute force attacks (optional)
- Consider adding 2FA support in Phase 2
- Log all authentication events for security auditing
- Consider adding password reset functionality (separate item)
- Test with frontend authentication flow
- Add integration tests for auth endpoints
- Consider CORS configuration for auth endpoints
- Add refresh token cleanup job (remove expired tokens periodically)
- Follow OWASP authentication guidelines
