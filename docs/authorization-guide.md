# Authorization Guide

This guide covers authentication and authorization patterns in the EA Tool API.

## Table of Contents

- [Authentication vs Authorization](#authentication-vs-authorization)
- [Authentication Methods](#authentication-methods)
- [Authorization Models](#authorization-models)
- [RBAC Patterns](#rbac-patterns)
- [ABAC Patterns](#abac-patterns)
- [OPA Policy Configuration](#opa-policy-configuration)
- [Common Scenarios](#common-scenarios)
- [Troubleshooting](#troubleshooting)

## Authentication vs Authorization

### Authentication (Who are you?)

Verifies the identity of the requestor:
- OIDC tokens for users and services
- API keys for development/testing
- Mutual TLS certificates for service-to-service

### Authorization (What are you allowed to do?)

Determines what authenticated users/services can do:
- Role-Based Access Control (RBAC) for simple scenarios
- Attribute-Based Access Control (ABAC) for complex scenarios
- Open Policy Agent (OPA) for policy-driven access control

## Authentication Methods

### 1. OpenID Connect (OIDC) - Recommended Production

#### User Authentication

Users authenticate via OIDC provider:

```
Client → OIDC Provider → Auth Redirect → Token
```

**Configuration:**

```yaml
oidc:
  provider_url: "https://oidc.example.com"
  client_id: "eatool-web"
  client_secret: "secret-key"
  redirect_uri: "https://app.example.com/auth/callback"
  scopes: ["openid", "profile", "email"]
```

**Token Response:**

```json
{
  "access_token": "eyJhbGc...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "id_token": "eyJhbGc..."
}
```

**Token Claims:**

```json
{
  "iss": "https://oidc.example.com",
  "sub": "user-123",
  "email": "john.doe@example.com",
  "name": "John Doe",
  "roles": ["architect", "reviewer"],
  "groups": ["team-infrastructure"],
  "iat": 1630000000,
  "exp": 1630003600
}
```

#### Service Authentication

Services use client credentials flow:

```bash
curl -X POST https://oidc.example.com/token \
  -d grant_type=client_credentials \
  -d client_id=eatool-service \
  -d client_secret=service-secret \
  -d scope=api
```

**Service Token Claims:**

```json
{
  "iss": "https://oidc.example.com",
  "sub": "service:eatool-import",
  "client_id": "eatool-service",
  "aud": "eatool-api",
  "scope": "api",
  "iat": 1630000000,
  "exp": 1630003600
}
```

### 2. API Keys (Development/Testing)

Use API keys for development and testing:

```bash
curl -H "X-API-Key: sk-1234567890abcdef" \
  https://api.example.com/applications
```

**Key Management:**

- Create keys per environment
- Rotate keys regularly
- Disable unused keys
- Never commit keys to version control

**Example:**

```bash
# Create new API key
curl -X POST https://api.example.com/auth/keys \
  -H "Authorization: Bearer admin-token" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Integration Test Key",
    "scopes": ["read:applications", "read:servers"],
    "expires_in": 2592000
  }'
```

### 3. Mutual TLS (Service-to-Service)

For secure service-to-service communication:

```bash
curl --cert client-cert.pem \
  --key client-key.pem \
  --cacert ca-cert.pem \
  https://api.example.com/applications
```

**Certificate Configuration:**

```yaml
mtls:
  enabled: true
  ca_certificate: /etc/eatool/tls/ca.pem
  client_certificate: /etc/eatool/tls/client.pem
  client_key: /etc/eatool/tls/client-key.pem
  verify_client: true
```

## Authorization Models

### RBAC - Role-Based Access Control

Simplest model; users have roles, roles have permissions.

**Components:**

- **Users:** Authenticated principals
- **Roles:** Collections of permissions
- **Permissions:** Specific actions on resources

**Example User Roles:**

```yaml
roles:
  architect:
    description: "Full EA management access"
    permissions:
      - create:applications
      - read:applications
      - update:applications
      - delete:applications
      - create:servers
      - read:servers
      - update:servers
      - delete:servers
      - approve:changes

  reviewer:
    description: "Review and read-only access"
    permissions:
      - read:applications
      - read:servers
      - read:relations
      - comment:changes
      - approve:changes

  viewer:
    description: "Read-only access"
    permissions:
      - read:applications
      - read:servers
      - read:relations
      - read:business-capabilities

  analyst:
    description: "Data analysis access"
    permissions:
      - read:applications
      - read:data-entities
      - read:relations
      - export:data
```

**RBAC Enforcement in Code:**

```fsharp
let requireRole role : HttpHandler =
  fun next ctx ->
    task {
      let userRoles = ctx.User.FindAll("role")
      if userRoles |> Seq.exists (fun c -> c.Value = role) then
        return! next ctx
      else
        ctx.SetStatusCode 403
        return! text "Forbidden" next ctx
    }

// Use in routes
let routes = [
  GET >=> route "/applications" >=> requireRole "viewer" >=> listApplications
  POST >=> route "/applications" >=> requireRole "architect" >=> createApplication
]
```

### ABAC - Attribute-Based Access Control

Attribute-based decisions; policies consider user, resource, action, and environment.

**Components:**

- **Subject Attributes:** User properties (department, role, clearance level)
- **Resource Attributes:** Resource properties (owner, sensitivity, status)
- **Action Attributes:** What operation (create, read, update, delete)
- **Environment Attributes:** Context (time, IP, device)

**Example Policy:**

```
A user can edit an application if:
  - User's department matches application's owner department, OR
  - User has "architect" role, OR
  - User's clearance level ≥ application's sensitivity level AND
  - Request is from corporate network AND
  - It's during business hours
```

**ABAC Attributes:**

```json
{
  "subject": {
    "user_id": "user-123",
    "roles": ["architect"],
    "department": "infrastructure",
    "clearance_level": "secret",
    "office_location": "us-east"
  },
  "resource": {
    "type": "application",
    "id": "app-001",
    "owner_department": "infrastructure",
    "sensitivity": "internal",
    "status": "active"
  },
  "action": "update",
  "environment": {
    "ip_address": "10.0.1.5",
    "is_vpn": true,
    "timestamp": "2025-01-17T14:30:00Z",
    "user_agent": "curl/7.68.0"
  }
}
```

## OPA Policy Configuration

Open Policy Agent (OPA) provides policy-driven authorization.

### OPA Setup

```yaml
# deployment/opa-config.yaml
opa:
  enabled: true
  server: "http://opa:8181"
  decision_path: "/v1/data/eatool/authz"
  cache_ttl: 300
```

### Policy Definition (Rego)

Create policies in Rego language:

```rego
# policies/eatool.rego

package eatool.authz

# Allow if user has architect role
allow {
  input.subject.roles[_] == "architect"
}

# Allow read if user is in viewer group
allow {
  input.action == "read"
  input.subject.groups[_] == "data-team"
}

# Allow if user is resource owner
allow {
  input.action in ["update", "delete"]
  input.subject.user_id == input.resource.owner
}

# Allow if in corporate network and during business hours
allow {
  input.environment.is_vpn == true
  time.now_ns() > time.parse_rfc3339_ns(input.environment.business_start)
  time.now_ns() < time.parse_rfc3339_ns(input.environment.business_end)
}

# Explicitly deny sensitive operations from external IPs
deny {
  input.action in ["delete", "purge"]
  not is_corporate_ip
}

is_corporate_ip {
  startswith(input.environment.ip_address, "10.")
}

# Data policy: deny if sensitivity exceeds clearance
deny {
  input.resource.sensitivity == "confidential"
  input.subject.clearance_level != "secret"
}
```

### Policy Evaluation

```fsharp
type AuthzRequest = {
    subject: Subject
    resource: Resource
    action: string
    environment: Environment
}

let evaluatePolicy (req: AuthzRequest) : Async<bool> = async {
    let json = req |> Json.encode
    let! response = http.PostAsync("http://opa:8181/v1/data/eatool/authz", json)
    let result = response.Content |> Json.decode<OPAResponse>
    return result.allow
}

let requireAuthz (policy: AuthzRequest) : HttpHandler =
    fun next ctx -> task {
        let! allowed = evaluatePolicy policy
        if allowed then
            return! next ctx
        else
            ctx.SetStatusCode 403
            return! text "Access Denied" next ctx
    }
```

## RBAC Patterns

### Pattern 1: Role-Based Endpoint Protection

```fsharp
// Define role checkers
let isArchitect (user: ClaimsPrincipal) =
  user.HasClaim("role", "architect")

let isReviewer (user: ClaimsPrincipal) =
  user.HasClaim("role", "reviewer")

let isViewer (user: ClaimsPrincipal) =
  user.HasClaim("role", "viewer") || isReviewer user || isArchitect user

// Apply to routes
let routes = [
  // Readable by all authenticated users
  GET >=> route "/applications" >=> requireAuthz isViewer >=> listApplications
  
  // Modifiable only by architects
  POST >=> route "/applications" >=> requireAuthz isArchitect >=> createApplication
  PUT >=> route "/applications/%s" >=> requireAuthz isArchitect >=> updateApplication
  
  // Approvable by reviewers
  POST >=> route "/applications/%s/approve" >=> requireAuthz isReviewer >=> approveApplication
]
```

### Pattern 2: Permission-Based Operations

```fsharp
// Fine-grained permissions
let hasPermission (perm: string) (user: ClaimsPrincipal) =
  user.FindAll("permission")
  |> Seq.exists (fun c -> c.Value = perm)

let requirePermission (perm: string) : HttpHandler =
  fun next ctx ->
    task {
      if hasPermission perm ctx.User then
        return! next ctx
      else
        ctx.SetStatusCode 403
        return! text "Insufficient permissions" next ctx
    }

// Routes
let routes = [
  POST >=> route "/applications" >=> requirePermission "create:applications" >=> createApplication
  PUT >=> route "/applications/%s" >=> requirePermission "update:applications" >=> updateApplication
  DELETE >=> route "/applications/%s" >=> requirePermission "delete:applications" >=> deleteApplication
]
```

### Pattern 3: Resource Ownership

```fsharp
// Check if user is resource owner
let isResourceOwner (resourceOwnerId: string) (user: ClaimsPrincipal) =
  user.FindFirst("sub").Value = resourceOwnerId

// Middleware that extracts and checks ownership
let requireOwnership : HttpHandler =
  fun next ctx ->
    task {
      match ctx.TryGetRouteValue "id" with
      | None -> return! requestErrors.BAD_REQUEST "Missing ID" next ctx
      | Some id ->
          match getResourceOwner id with
          | None -> return! requestErrors.NOT_FOUND next ctx
          | Some ownerId ->
              if isResourceOwner ownerId ctx.User then
                return! next ctx
              else
                ctx.SetStatusCode 403
                return! text "Cannot modify resource owned by others" next ctx
    }

// Routes
let routes = [
  DELETE >=> route "/applications/%s" >=> requireOwnership >=> deleteApplication
]
```

## ABAC Patterns

### Pattern 1: Attribute-Based Policy Enforcement

```fsharp
type AuthorizationContext = {
    user: ClaimsPrincipal
    resource: Resource
    action: string
    environment: RequestEnvironment
}

let evaluatePolicy (ctx: AuthorizationContext) : bool =
  // User can edit if they own it or are architect
  let canEdit = 
    (ctx.resource.owner = ctx.user.FindFirst("sub").Value) ||
    ctx.user.HasClaim("role", "architect")
  
  // But not if it's locked for other reasons
  let notBlocked = 
    ctx.resource.status <> "archived" &&
    ctx.resource.status <> "locked"
  
  // And only during business hours or from VPN
  let timeCheck =
    DateTime.Now.Hour >= 8 && DateTime.Now.Hour <= 18 ||
    isVpnConnected ctx.environment
  
  canEdit && notBlocked && timeCheck

let requirePolicy : HttpHandler =
  fun next ctx ->
    task {
      match ctx.TryGetRouteValue "id" with
      | Some id ->
          match getResource id with
          | Some resource ->
              let authCtx: AuthorizationContext = {
                user = ctx.User
                resource = resource
                action = ctx.Request.Method
                environment = {
                  ipAddress = ctx.Connection.RemoteIpAddress.ToString()
                  isVpn = checkVpnStatus ctx
                }
              }
              
              if evaluatePolicy authCtx then
                return! next ctx
              else
                ctx.SetStatusCode 403
                return! text "Access Denied" next ctx
          | None -> return! requestErrors.NOT_FOUND next ctx
      | None -> return! requestErrors.BAD_REQUEST "Missing ID" next ctx
    }
```

### Pattern 2: Department-Based Access

```fsharp
// Allow cross-department reads, but restrict writes
let departmentPolicy (userDept: string) (resourceDept: string) (action: string) =
  match action with
  | "read" -> true  // Anyone can read
  | "update" | "delete" ->
      // Can update own department or be central team
      userDept = resourceDept || userDept = "central-architecture"
  | _ -> false

let requireDepartmentAccess : HttpHandler =
  fun next ctx ->
    task {
      let userDept = ctx.User.FindFirst("department").Value
      match ctx.TryGetRouteValue "id" with
      | Some id ->
          match getResource id with
          | Some resource ->
              if departmentPolicy userDept resource.department ctx.Request.Method then
                return! next ctx
              else
                ctx.SetStatusCode 403
                return! text "Insufficient department access" next ctx
          | None -> return! requestErrors.NOT_FOUND next ctx
      | None -> return! requestErrors.BAD_REQUEST "Missing ID" next ctx
    }
```

## Common Scenarios

### Scenario 1: Multi-Tenant Access

```fsharp
type TenantContext = {
    tenantId: string
    userId: string
    roles: string[]
    resources: string[]
}

// Extract tenant from token or request
let getTenantContext (user: ClaimsPrincipal) : TenantContext option =
  match user.FindFirst("tenant_id") with
  | null -> None
  | tenantClaim ->
      Some {
        tenantId = tenantClaim.Value
        userId = user.FindFirst("sub").Value
        roles = user.FindAll("role") |> Seq.map (fun c -> c.Value) |> Seq.toArray
        resources = user.FindAll("resource") |> Seq.map (fun c -> c.Value) |> Seq.toArray
      }

// Enforce tenant isolation
let requireTenant : HttpHandler =
  fun next ctx ->
    task {
      match getTenantContext ctx.User with
      | None -> return! requestErrors.FORBIDDEN "Missing tenant context" next ctx
      | Some tenant ->
          // Store in context for later use
          ctx.Items["tenant"] <- tenant
          return! next ctx
    }
```

### Scenario 2: Approval Workflows

```fsharp
// Only reviewers can approve
let canApprove (user: ClaimsPrincipal) =
  user.HasClaim("role", "reviewer") || user.HasClaim("role", "architect")

// Only if change is pending approval
let isPendingApproval (changeId: string) =
  match getChange changeId with
  | Some change -> change.status = "pending"
  | None -> false

let approveChange : HttpHandler =
  fun next ctx ->
    task {
      if not (canApprove ctx.User) then
        return! requestErrors.FORBIDDEN "Not a reviewer" next ctx
      
      match ctx.TryGetRouteValue "id" with
      | Some id when isPendingApproval id ->
          // Process approval
          return! next ctx
      | _ -> return! requestErrors.BAD_REQUEST "Not pending approval" next ctx
    }
```

### Scenario 3: Time-Based Access

```fsharp
// Only allow sensitive operations during maintenance windows
let inMaintenanceWindow () =
  let now = DateTime.Now
  let dayOfWeek = now.DayOfWeek
  let hour = now.Hour
  
  // Saturday nights, 10 PM - 6 AM
  (dayOfWeek = DayOfWeek.Saturday && hour >= 22) ||
  (dayOfWeek = DayOfWeek.Sunday && hour < 6)

let requireMaintenanceWindow : HttpHandler =
  fun next ctx ->
    task {
      if inMaintenanceWindow() then
        return! next ctx
      else
        return! requestErrors.FORBIDDEN "Only allowed during maintenance window" next ctx
    }
```

## Troubleshooting

### 401 Unauthorized

**Problem:** Authentication failed

**Causes:**
- Token expired: Check `exp` claim
- Token invalid signature: Verify signing key
- Token not provided: Check `Authorization` header
- Wrong OIDC provider: Verify issuer URL

**Solution:**

```bash
# Decode JWT to inspect claims
jwt-decode "eyJhbGc..."

# Verify token still valid
exp=$(jq -r '.exp' <<< "$(jwt-decode token)")
now=$(date +%s)
if [ $exp -lt $now ]; then
  echo "Token expired"
fi
```

### 403 Forbidden

**Problem:** Access denied

**Causes:**
- Missing required role
- Insufficient permissions
- Resource ownership mismatch
- Policy evaluation failed

**Solution:**

```bash
# Check token claims
curl -H "Authorization: Bearer token" \
  https://api.example.com/me

# Verify roles in token
jq '.resource_access' <<< "$(jwt-decode token)"
```

### Permission Denied on Resource

**Problem:** Cannot modify resource

**Causes:**
- User doesn't own resource
- User's department doesn't match resource department
- Resource is locked/archived
- Policy conditions not met

**Solution:**

```bash
# Check resource details
curl -H "Authorization: Bearer token" \
  https://api.example.com/applications/app-001

# Check user's permissions
curl -H "Authorization: Bearer token" \
  https://api.example.com/me/permissions
```

### Policy Not Evaluating

**Problem:** OPA policy not working

**Causes:**
- OPA server not reachable
- Policy not loaded
- Invalid policy syntax
- Incorrect decision path

**Solution:**

```bash
# Check OPA server health
curl http://opa:8181/health

# Load policy
curl -X PUT http://opa:8181/v1/policies/eatool \
  -H "Content-Type: text/plain" \
  -d @policies/eatool.rego

# Test policy
curl -X POST http://opa:8181/v1/data/eatool/authz \
  -H "Content-Type: application/json" \
  -d '{
    "subject": {...},
    "resource": {...},
    "action": "read"
  }'
```
