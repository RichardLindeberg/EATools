---
title: Authentication Specification
version: 0.1.0
date_created: 2026-01-08
last_updated: 2026-01-08
owner: EA Platform Team
tags: [process, security, authentication, oidc, api-key]
---

# Introduction

This specification defines the authentication model for the EA Tool, covering OpenID Connect (OIDC) with Authorization Code + PKCE as the primary mechanism and API keys for service-to-service use cases. It standardizes token formats, validation requirements, provider configuration, and integration with authorization (OPA).

## 1. Purpose & Scope

Provide a clear, implementable authentication contract for all EA Tool services and clients. Scope includes OIDC/OAuth2 flows, token formats (JWT), API key lifecycle, token validation, claim mapping to authorization inputs, examples, and troubleshooting. Audience: backend engineers, platform engineers, and integrators.

## 2. Definitions

- **OIDC**: OpenID Connect 1.0 built on OAuth 2.0 for user authentication.
- **Authorization Code with PKCE**: OAuth2 flow using code verifier/challenge to protect public clients.
- **API Key**: Static credential for service-to-service access; issued and stored hashed.
- **JWT**: JSON Web Token containing claims; signed with asymmetric keys (RS256 preferred).
- **JWK/JWKS**: JSON Web Key (Set) used to publish public keys for signature verification.
- **Claims**: Token payload attributes (e.g., `sub`, `email`, `roles`, `groups`, `iat`, `exp`, `iss`, `aud`).
- **Scopes**: OAuth2 scope strings controlling access to APIs.
- **Refresh Token**: Long-lived token to obtain new access tokens; stored securely by confidential clients only.
- **Bearer Token**: Access token transmitted via `Authorization: Bearer <token>`.
- **JTI**: Unique token identifier; enables replay detection/blacklist if implemented.

## 3. Requirements, Constraints & Guidelines

- **REQ-001**: Primary authentication method SHALL be OIDC Authorization Code with PKCE for user-facing flows.
- **REQ-002**: Access tokens SHALL be JWTs signed with asymmetric keys (RS256); HS256 is disallowed.
- **REQ-003**: API keys SHALL be allowed only for service-to-service and automation use cases; they MUST be hashed at rest and never logged.
- **REQ-004**: Token validation SHALL verify signature (via JWKS), issuer (`iss`), audience (`aud`), expiration (`exp`), and not-before (`nbf` if present).
- **REQ-005**: Tokens SHALL include subject claims: `sub`, `email`, and optional `roles`, `groups`, `scopes`, `tenant`.
- **REQ-006**: Authorization integration SHALL map token claims to the authorization input envelope: `subject` (claims), `context.scopes`, `context.tenant`.
- **REQ-007**: Clients SHALL send tokens in the `Authorization: Bearer <token>` header; query-string tokens are forbidden.
- **REQ-008**: API keys SHALL be transmitted via `Authorization: ApiKey <key>` or `X-API-Key` header; never via query string.
- **REQ-009**: Key rotation MUST be supported; API keys include `created_at` and `expires_at` metadata.
- **SEC-001**: Fail closed on any token validation error (invalid signature, expired, audience/issuer mismatch, replay detection failure).
- **SEC-002**: Refresh tokens MUST be stored only by confidential clients; frontends use Authorization Code with PKCE and short-lived access tokens.
- **SEC-003**: TLS is mandatory for all authentication flows.
- **SEC-004**: API keys MUST be minimum 32 bytes entropy; display only on creation; store hashed with salt/pepper as applicable.
- **CON-001**: JWKS cache TTL SHOULD be short (e.g., 5-15 minutes) and honor `Cache-Control` from provider.
- **CON-002**: Clock skew tolerance for `exp`/`nbf` checks SHOULD be ≤ 60 seconds.
- **GUD-001**: Prefer `aud` that matches API identifier; avoid wildcard audiences.
- **GUD-002**: Include `jti` to enable revocation lists when required.

## 4. Interfaces & Data Contracts

### 4.1 JWT Access Token (example)

```json
{
  "header": {
    "alg": "RS256",
    "typ": "JWT",
    "kid": "key-2026-01"
  },
  "payload": {
    "iss": "https://auth.example.com/",
    "aud": "eatool-api",
    "sub": "user-123",
    "email": "user@example.com",
    "roles": ["viewer", "team-lead"],
    "groups": ["payments-team"],
    "scopes": ["read:applications", "write:relations"],
    "tenant": "acme",
    "iat": 1767225600,
    "exp": 1767229200,
    "jti": "4e0f4b02-3d65-4e2f-b5b3-1e3d6f8d1234"
  },
  "signature": "base64url-signature"
}
```

### 4.2 API Key Format
- Opaque string, ≥ 43 base64url chars (≈256 bits entropy) or UUIDv4 plus random suffix.
- Stored hashed (e.g., HMAC-SHA256) with per-key salt; only a truncated prefix may be shown for audit listings.

### 4.3 Token Transport
- Access token: `Authorization: Bearer <jwt>`
- API key: `Authorization: ApiKey <key>` (primary) or `X-API-Key: <key>` (legacy fallback)

### 4.4 OIDC Provider Integration
- Discovery: `GET https://<issuer>/.well-known/openid-configuration`
- Authorization endpoint: follows OIDC discovery
- Token endpoint: follows OIDC discovery; client uses code + code_verifier for PKCE
- JWKS endpoint: from discovery for signature verification

### 4.5 Claim Mapping to Authorization Input
- `subject.sub` ← `sub`
- `subject.email` ← `email`
- `subject.roles` ← `roles` (array) or mapped from `realm_access.roles` / `cognito:groups` provider-specific fields
- `subject.groups` ← `groups`
- `context.scopes` ← `scopes` claim or OAuth scope list
- `context.tenant` ← `tenant` claim when present

## 5. Acceptance Criteria

- **AC-001**: Authentication spec created with full template sections and front matter.
- **AC-002**: OIDC Authorization Code with PKCE flow documented, including discovery, endpoints, and redirect URI considerations.
- **AC-003**: JWT token structure, required claims, and validation steps documented.
- **AC-004**: API key generation, storage (hashed), rotation, and transport documented.
- **AC-005**: Token validation rules documented (signature, issuer, audience, expiry, skew) with fail-closed behavior.
- **AC-006**: Authorization integration documented (claim mapping to OPA input envelope).
- **AC-007**: Examples provided for OIDC config, JWT payload, API key usage, and cURL.
- **AC-008**: Troubleshooting guidance included for common auth failures.
- **AC-009**: Spec linked from the specifications index.

## 6. Test Automation Strategy

- **Test Levels**: Unit tests for token validation helpers; integration tests against mock OIDC (static JWKS) and API key verification; end-to-end tests hitting secured endpoints with valid/invalid tokens.
- **Frameworks**: Existing test harness plus JWT libraries; mock JWKS server or static keys; property tests for token parsing edge cases.
- **Test Data Management**: Generate short-lived JWTs signed by test keys; store fixtures under `tests/auth/fixtures`; rotate test keys periodically.
- **CI/CD Integration**: Run auth validation tests in CI; ensure discovery/JWKS fetching is mocked; fail build on lint/validation errors.
- **Coverage Requirements**: Validation paths for signature failure, expired token, wrong audience/issuer, missing required claims, and API key revocation.
- **Performance Testing**: Validate token verification latency with cached JWKS; ensure under 5ms per verification in-process.

## 7. Rationale & Context

- Authorization Code with PKCE provides secure user authentication for SPAs and native apps without exposing client secrets.
- Asymmetric JWT signing allows key rotation without redeploying services (JWKS-driven validation).
- API keys remain necessary for service integrations; hashing and rotation mitigate risk.
- Mapping claims directly into the authorization envelope keeps policy evaluation consistent.

## 8. Dependencies & External Integrations

### External Systems
- **EXT-001**: OIDC Provider (e.g., Keycloak, Auth0, Azure AD) exposing discovery, authorization, token, and JWKS endpoints.

### Third-Party Services
- **SVC-001**: None beyond the chosen OIDC provider; JWT libraries for the runtime.

### Infrastructure Dependencies
- **INF-001**: TLS termination for all auth endpoints; network access to OIDC endpoints.
- **INF-002**: Secure storage for API key hashes and salts (DB) and optional HSM/KMS for signing keys if self-hosted.

### Data Dependencies
- **DAT-001**: API key records (hashed key, created_at, expires_at, owner, scopes/roles if scoped).
- **DAT-002**: JWKS documents cached in-memory with TTL.

### Technology Platform Dependencies
- **PLT-001**: JWT and OIDC libraries compatible with .NET 10.

### Compliance Dependencies
- **COM-001**: Logs must avoid token material; only minimal identifiers (kid, jti) when necessary.

## 9. Examples & Edge Cases

**OIDC Authorization Request (PKCE)**
```
GET https://auth.example.com/authorize?
  response_type=code&
  client_id=eatool-web&
  redirect_uri=https%3A%2F%2Fapp.example.com%2Fcallback&
  scope=openid%20profile%20email%20offline_access&
  code_challenge_method=S256&
  code_challenge=<BASE64URL_SHA256(code_verifier)>&
  state=abc123
```

**Token Exchange**
```
POST https://auth.example.com/token
Content-Type: application/x-www-form-urlencoded

grant_type=authorization_code&
code=<auth_code>&
redirect_uri=https%3A%2F%2Fapp.example.com%2Fcallback&
client_id=eatool-web&
code_verifier=<original_code_verifier>
```

**API Request with JWT**
```
curl -H "Authorization: Bearer <access_token>" \
     https://api.example.com/applications
```

**API Request with API Key**
```
curl -H "Authorization: ApiKey <api_key>" \
     https://api.example.com/applications
```

Edge Cases:
- Expired token (`exp` past now with skew) → deny.
- Audience/issuer mismatch → deny.
- Unknown `kid` and JWKS fetch fails → deny (fail closed) and retry once within backoff.
- API key expired or revoked → deny.
- Missing required claims (`sub`, `email`) → deny.

## 10. Validation Criteria

- Token validation logic enforces signature, issuer, audience, expiry, and required claims with fail-closed defaults.
- API key verification uses hashed storage and denies missing/expired/revoked keys.
- Claim mapping into authorization envelope is covered by integration tests.
- JWKS retrieval caching and rotation paths are exercised in tests.
- Specification is reachable from the spec index and follows the standard template.

## 11. Related Specifications / Further Reading

- [spec-index.md](spec/spec-index.md)
- [spec-process-authorization.md](spec/spec-process-authorization.md)
- [openapi.yaml](openapi.yaml)
- [docs/system-overview.md](docs/system-overview.md)