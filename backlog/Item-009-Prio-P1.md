# Item-009: Create Authentication Specification

**Status:** 🟢 Ready  
**Priority:** P1 - HIGH  
**Effort:** 3-4 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

Authentication via OIDC and API keys is mentioned in openapi.yaml and docs but lacks formal specification explaining:
- OIDC integration setup
- API key management
- JWT token structure and claims
- Token validation
- Credential refresh

**Impact:** Developers cannot implement authentication or configure OIDC provider without external guidance.

---

## Affected Files

**Create:** [spec/spec-process-authentication.md](../../spec/spec-process-authentication.md) (new file)

**Reference:**
- [openapi.yaml](../../openapi.yaml#L28-L41) - Security schemes
- [docs/system-overview.md](../../docs/system-overview.md) - Mentions OIDC

---

## Detailed Tasks

- [ ] Create spec-process-authentication.md following specification template
- [ ] Document OIDC authentication:
  - OpenID Connect flow (Authorization Code with PKCE)
  - OIDC provider configuration
  - Token endpoint
  - Discovery endpoint (.well-known/openid-configuration)
  - Redirection URI setup

- [ ] Document JWT tokens:
  - Token format (header.payload.signature)
  - Payload claims: sub, email, roles, groups, iat, exp
  - Token validation (signature, expiration)
  - Token storage and refresh

- [ ] Document API key authentication:
  - API key format
  - API key generation and management
  - API key storage (secure, hashed)
  - API key rotation policy
  - Revocation

- [ ] Document token validation:
  - Signature verification
  - Expiration check
  - Issuer verification
  - Claim validation

- [ ] Document authorization integration:
  - How tokens flow to OPA
  - Claim mapping to subject attributes
  - Role extraction from token claims

- [ ] Document examples:
  - OIDC configuration examples (Keycloak, Auth0, Azure AD)
  - JWT token example
  - API key usage example
  - cURL requests with auth

- [ ] Document troubleshooting:
  - Invalid signature errors
  - Token expiration handling
  - OIDC provider issues
  - API key issues

---

## Acceptance Criteria

- [ ] spec-process-authentication.md created
- [ ] OIDC flow documented
- [ ] JWT token structure documented
- [ ] API key management documented
- [ ] Token validation documented
- [ ] Authorization integration documented
- [ ] Examples for each auth method
- [ ] Troubleshooting guide included
- [ ] Linked from spec-index.md

---

## Key Sections

```markdown
# Authentication Specification

## 1. Purpose & Scope

## 2. Authentication Methods

### OIDC (Primary)
- Recommended for user-facing applications
- Federated authentication
- Token refresh automatically handled

### API Key (Integration)
- For service-to-service authentication
- For testing and development
- Static credential

## 3. OIDC Flow

1. User initiates login
2. Redirect to OIDC provider
3. User authenticates (username/password, MFA, etc.)
4. Provider redirects with authorization code
5. Backend exchanges code for tokens (using client_secret)
6. Return ID token + access token to client
7. Client stores tokens and includes in API requests

## 4. JWT Token Structure

```json
{
  "header": {
    "alg": "RS256",
    "typ": "JWT"
  },
  "payload": {
    "sub": "user-id",
    "email": "user@example.com",
    "roles": ["viewer", "team-lead"],
    "groups": ["payments-team"],
    "iat": 1704547200,
    "exp": 1704633600
  },
  "signature": "base64-encoded-signature"
}
```

## 5. API Key Management

## 6. Token Validation

## 7. Authorization Integration

## 8. OIDC Provider Examples

### Keycloak
### Auth0
### Azure AD

## 9. Troubleshooting

## 10. Related Specifications
```

---

## Dependencies

**Blocks:** None

**Depends On:** None

---

## Related Items

- [Item-007-Prio-P1.md](Item-007-Prio-P1.md) - Authorization spec (uses auth)
- [openapi.yaml](../../openapi.yaml#L28-L41)
- [spec-index.md](../../spec/spec-index.md)

---

## Definition of Done

- [x] spec-process-authentication.md created
- [x] OIDC flow documented
- [x] JWT structure documented
- [x] API key management documented
- [x] Examples for each method
- [x] Troubleshooting guide included
- [x] Linked from spec-index.md

---

## Notes

- Critical for security and user access
- OIDC examples should be copy-paste ready
- Include both user-facing and service-to-service patterns
- Should reference security best practices (PKCE, etc.)
