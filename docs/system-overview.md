# System Overview

## Introduction

The EA Tool is an API-first enterprise architecture management platform designed to catalog IT assets (applications, servers, integrations) and visualize them using ArchiMate 3.2 standards. The system prioritizes flexibility, allowing different frontends and integrations while maintaining a consistent, well-defined API contract.

## Design Principles

### 1. API-First (Contract-First)
- OpenAPI 3.0.3 specification defines all endpoints, schemas, and contracts
- API contract is the source of truth
- SDK clients generated automatically from the OpenAPI spec
- Enables frontend flexibility and easy integrations

### 2. ArchiMate 3.2 Alignment
- Domain entities map to ArchiMate elements (ApplicationComponent, ApplicationService, ApplicationInterface, etc.)
- Relationships follow ArchiMate relationship semantics (Realization, Serving, Assignment, Access, etc.)
- Views can be rendered with proper ArchiMate styling and layering

### 3. Separation of Concerns
- Backend provides data management and business logic
- Frontend is swappable/optional - any client can consume the API
- Visualization is client-side, enabling custom rendering

### 4. Comprehensive Authorization
- Authentication via OpenID Connect (OIDC) and API keys
- Authorization enforced via Rego/OPA policies (RBAC/ABAC)
- Fine-grained access control at resource and field level

## Technology Stack

### Backend
- **Language**: F# on .NET 10
- **Framework**: ASP.NET Core minimal API or Giraffe/Saturn
- **Database**: 
  - SQLite (development)
  - MSSQL (staging)
  - MSSQL (production)
- **Auth**: OpenID Connect (OIDC) + JWT bearer tokens
- **Authorization**: Open Policy Agent (OPA) with Rego policies
- **API Contract**: OpenAPI 3.0.3
- **Time Handling**: UTC only, ISO 8601 with Z suffix

### Frontend (Optional)
- **Framework**: React
- **UI Library**: Material-UI or Ant Design
- **Graph Rendering**: Cytoscape.js or similar with Dagre layout
- **State Management**: React Query for API state

### Infrastructure
- **Containerization**: Docker
- **CI/CD**: Automated testing, linting, OpenAPI validation, SDK generation
- **Observability**: Structured logging with correlation IDs, metrics, health checks

## System Architecture

```
┌─────────────────┐
│   Frontend(s)   │  (React, or custom clients)
│   (Optional)    │
└────────┬────────┘
         │ HTTPS + JWT
         ▼
┌─────────────────────────────────────────┐
│         API Gateway / Load Balancer      │
└────────┬────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│      EA Tool API (F#/.NET 10)           │
│  ┌──────────────┐  ┌─────────────────┐ │
│  │  Controllers │  │  Authorization  │ │
│  │  (Endpoints) │◄─┤  (OPA/Rego)     │ │
│  └──────┬───────┘  └─────────────────┘ │
│         │                                │
│  ┌──────▼───────┐  ┌─────────────────┐ │
│  │   Business   │  │   Data Access   │ │
│  │     Logic    │──┤    (Queries)    │ │
│  └──────────────┘  └────────┬────────┘ │
└────────────────────────────────│────────┘
                                │
                                ▼
                    ┌─────────────────────┐
                    │  Database           │
                    │  (SQLite/PG/MSSQL)  │
                    └─────────────────────┘
```

### External Dependencies
- **OIDC Provider**: For authentication (e.g., Keycloak, Auth0, Azure AD)
- **OPA**: Policy Decision Point for authorization
- **CMDB/ITSM**: Optional integrations via webhooks or scheduled sync

## Data Flow

### Read Operations
1. Client sends authenticated request (JWT bearer token or API key)
2. API validates token and extracts subject claims
3. Authorization middleware queries OPA with (subject, action, resource, context)
4. OPA evaluates Rego policies and returns allow/deny + obligations
5. If allowed, API queries database with any field-level filters from obligations
6. Response is serialized and returned to client

### Write Operations
1. Client sends authenticated request with payload
2. API validates token and authorization (same as read)
3. Request body is validated against OpenAPI schema
4. Business logic applies domain rules and constraints
5. For event-sourced domains (e.g., Relations, BusinessCapabilities):
  - Command produces one or more domain events
  - Events are appended to the event store
  - Projections update the read model asynchronously
  For CRUD domains, a database transaction commits changes
6. Audit log records the mutation (actor, action, entity, timestamp)
7. Webhooks fire asynchronously for registered events
8. Response confirms the change

## Environment Configuration

| Environment | Database  | OIDC Provider | OPA Mode    | Purpose                    |
|-------------|-----------|---------------|-------------|----------------------------|
| Development | SQLite    | Mock/Local    | Embedded    | Local development          |
| Staging     | MSSQL     | Staging IdP   | Sidecar     | Pre-production testing     |
| Production  | MSSQL     | Production IdP| Centralized | Live operations            |

## Time Handling

**Critical**: All timestamps in the system are stored and transmitted as UTC in ISO 8601 format with trailing 'Z'.

- Database columns: `TIMESTAMP` or `DATETIME` in UTC
- API requests: ISO 8601 strings ending in 'Z' (e.g., `2024-01-05T10:00:00Z`)
- API responses: ISO 8601 strings ending in 'Z'
- SDKs: Parse/serialize as UTC; let consuming code convert to local time
- Logs: UTC timestamps with timezone explicitly stated

Example:
```json
{
  "created_at": "2024-01-05T10:00:00Z",
  "updated_at": "2024-01-05T14:30:00Z"
}
```

## Security Considerations

- **TLS everywhere**: All API communication over HTTPS
- **Token validation**: JWT signatures verified, expiry checked
- **Fail-closed authorization**: Deny access on OPA errors
- **Rate limiting**: Per-token and per-IP limits
- **Audit logging**: All mutations logged with actor and timestamp
- **Secrets management**: Environment variables or vault, never in code
- **Data classification**: Sensitive fields tagged and encrypted at rest

## Scalability & Performance

- **Stateless API**: Horizontal scaling of API instances
- **Database pooling**: Connection reuse for efficiency
- **Caching**: Short-lived authorization decision cache
- **Pagination**: All list endpoints support cursor or offset pagination
- **Filtering**: Query parameters reduce payload sizes
- **Async jobs**: Long-running operations (import/export) return job IDs

## Next Steps

- [Entity Guide](./entity-guide.md) - Learn about domain entities
- [Relationship Modeling](./relationship-modeling.md) - Model connections between entities
- [API Usage Guide](./api-usage-guide.md) - Integration patterns
