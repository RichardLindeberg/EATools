## Plan: Simple EA Tool Setup

Drafting an API-first EA tool that catalogs servers/apps and renders ArchiMate 3.2 views for different audiences while keeping the UI swappable and integrable with other systems.

### Steps
1. Backend setup: API-first (contract-first OpenAPI) in F#/.NET 8 (ASP.NET Core minimal API or Giraffe), database (SQLite for dev, MSSQL for staging and prod), auth via OpenID Connect (OIDC) and API keys, versioned endpoints for clients/partners; granular authorization enforced via Rego/OPA.
2. Frontend setup: React with component library (Material-UI/AntD) consuming the published API; keep UI optional so other clients can replace it.
3. Design minimal data model: Organizations, Applications, Servers, Integrations, BusinessCapabilities, DataEntities, Relations with ArchiMate element/type metadata.
4. Build CRUD API for core entities and relations; seed with sample data and auth; publish OpenAPI/SDK clients for integrations.
5. Implement view definitions: saved filtered perspectives (by layer, audience, capability) that map to ArchiMate element styles.
6. Add diagram rendering: client-side graph (e.g., Cytoscape/Dagre) with ArchiMate 3.2 shapes/colors; toggle layers and views.
7. Package deployment (Docker) and basic CI to run lint/tests; expose API docs (Swagger/Redoc) for easy consumption.

### Further Considerations
1. Backend stack: F#/.NET 8 (ASP.NET Core minimal API or Giraffe/Saturn), staging on MSSQL, production on MSSQL; OIDC provider for identity; OPA sidecar or centralized PDP for Rego-based authorization;
2. Hosting constraints on-prem, and SSO needs
3. Need import/export (CSV/Excel) or integration with CMDB (ServiceNow)?
4. SDK generation: use OpenAPI generators to emit TypeScript/Java/Python SDKs; version and publish to internal registries; auto-regenerate on API contract changes via CI.
5. Integration patterns: webhooks for change events (app/server lifecycle), outbound connectors for CMDB/ITSM, CSV/Excel bulk import/export, and scheduled sync jobs; provide API keys and OAuth client creds for partners.

### Initial Data Model
- Organizations: id, name, domains, contacts.
- Applications: id, name, owners, lifecycle (planned/active/retired), business capability mapping, deployment targets, data classifications.
- Servers/Hosts: id, hostname, environment, region, platform, criticality, owning team; relationships to applications (runs_on) and integrations (endpoints).
- Integrations: id, source_app, target_app, protocol, data contracts, SLAs, frequency; optional link to CMDB/ITSM records.
- BusinessCapabilities: id, name, parent_id; mapping to applications for coverage views.
- DataEntities: id, name, domain, classification, retention, owner/steward, source_system, criticality, pii_flag, glossary_terms, lineage pointers.
- Relations: generic typed edges (e.g., serves, depends_on, runs_on, implements, realizes, reads, writes); store ArchiMate element/type metadata for rendering.
- Auditing: timestamps, user, source (UI/API/import) for traceability.

### Initial API Contract (v1)
- Auth: OpenID Connect (OIDC) for identity + JWT bearer on requests; API keys for partners/automation; authorization decisions enforced via Rego/OPA (centralized PDP or sidecar).
- Resources (CRUD): /organizations, /applications, /servers, /integrations, /business-capabilities, /data-entities, /relations.
- Filtering/pagination: standard query params (search, owner, lifecycle, environment, capability, tag); cursor or offset pagination.
- Views: /views (CRUD) to save filters, element sets, and layout hints; /views/{id}/render returns a graph payload (nodes+edges+styling metadata) for any client.
- Import/Export: /imports (upload CSV/Excel/JSON), /exports (on-demand snapshots by filter); async job IDs with status polling.
- Events/Webhooks: /webhooks (register, rotate secret); events for application.updated, server.updated, integration.changed, relation.changed.
- Metadata: /openapi.json and /docs (Swagger/Redoc); SDKs generated from OpenAPI and published per language.
- Time handling: all timestamps are UTC only, ISO 8601 with Z suffix; API contracts and SDKs must document UTC expectations for requests/responses.

### OpenAPI Skeleton (v1)
- Info: title, version, contact, terms; servers: prod, staging, local.
- SecuritySchemes: oauth2 clientCredentials (tokenUrl), apiKey (header: X-Api-Key), bearerAuth (JWT).
- Components/Schemas (representative): Organization, Application, Server, Integration, BusinessCapability, DataEntity, Relation, View, Graph (nodes, edges, styling), ImportJob, ExportJob, Webhook, Error.
- Paths (examples):
	- /organizations: get (list with filters), post; /organizations/{id}: get, patch, delete.
	- /applications: get, post; /applications/{id}: get, patch, delete.
	- /servers: get, post; /servers/{id}: get, patch, delete.
	- /integrations: get, post; /integrations/{id}: get, patch, delete.
	- /business-capabilities: get, post; /business-capabilities/{id}: get, patch, delete.
	- /data-entities: get, post; /data-entities/{id}: get, patch, delete.
	- /relations: get, post; /relations/{id}: get, patch, delete.
	- /views: get, post; /views/{id}: get, patch, delete; /views/{id}/render: get returns Graph.
	- /imports: post (create job, upload via pre-signed URL or multipart); /imports/{jobId}: get status.
	- /exports: post (create job with filter payload); /exports/{jobId}: get status and download link.
	- /webhooks: get, post; /webhooks/{id}: patch, delete; /webhooks/test: post.
- Global: pagination params, tracing headers (request-id), rate-limit headers.
- Errors: standard problem+json with trace id and code.

### Database Schema (proposed)
- organizations(id pk, name, domains jsonb, contacts jsonb, created_at, updated_at)
- applications(id pk, name, owner, lifecycle, capability_id fk business_capabilities.id, data_classification, tags jsonb, created_at, updated_at)
- servers(id pk, hostname, environment, region, platform, criticality, owning_team, tags jsonb, created_at, updated_at)
- application_servers(app_id fk applications.id, server_id fk servers.id, primary key(app_id, server_id))
- integrations(id pk, source_app_id fk applications.id, target_app_id fk applications.id, protocol, data_contract, sla, frequency, tags jsonb, created_at, updated_at)
- business_capabilities(id pk, name, parent_id fk business_capabilities.id, created_at, updated_at)
- data_entities(id pk, name, domain, classification, retention, owner, steward, source_system, criticality, pii_flag boolean, glossary_terms jsonb, created_at, updated_at)
- application_data(app_id fk applications.id, data_id fk data_entities.id, usage enum(reads,writes,creates,deletes), primary key(app_id, data_id, usage))
- relations(id pk, source_id, target_id, source_type, target_type, relation_type, archimate_element, archimate_relationship, created_at, updated_at)
- views(id pk, name, description, filter jsonb, layout jsonb, created_at, updated_at)
- imports(id pk, status, input_type, created_by, created_at, updated_at, error)
- exports(id pk, status, filter jsonb, created_by, created_at, updated_at, download_url)
- webhooks(id pk, url, secret, active, events jsonb, created_at, updated_at, last_failure_at)
- audit_log(id pk, actor, action, entity_type, entity_id, source, created_at, metadata jsonb)

### Diagram/View Payloads
- Graph model: nodes (id, type, archimate_element, label, attrs, data refs), edges (id, source, target, archimate_relationship, relation_type, label, attrs), layout hints (rankdir, spacing, groups), legends (color/shape per element/relationship), and filters used; include data-entity nodes/edges for lineage (reads/writes).
- View render response (/views/{id}/render): returns graph payload plus audience tags and suggested layer toggles (business/application/technology).
- Client rendering: Dagre/ELK layout for clarity; ArchiMate 3.2 styling map on client; allow toggling layers, highlighting capability coverage, and filtering by lifecycle/owner.
- Export: support PNG/SVG/PDF render via headless client; include data timestamp and filters in metadata.

### Delivery, Ops, and NFRs
- Environments: dev (SQLite), staging (Postgres), prod (MSSQL). Migrations via a tool that supports Postgres and MSSQL (e.g., Prisma/Alembic); seed scripts for demo data.
- CI: lint, tests, type-check, OpenAPI diff guard, SDK generation, Docker build; fail on breaking API changes unless version bumped.
- CD: build/publish API container and SDK packages; deploy via container platform; run migrations pre-start.
- Observability: request logging with correlation ids, metrics (latency, error rates), health/readiness endpoints, audit logging for mutating calls; ensure all logged timestamps are UTC.
- Security: OpenID Connect for identity, Rego/OPA for authorization (RBAC/ABAC policies), API keys for partners/automation, rate limiting, secrets management, TLS everywhere.
- Data protection: tag sensitive fields, encrypt secrets at rest, backup/restore plan.

### Authorization (OPA/Rego)
- Policy inputs: subject (sub, email, roles, groups/teams), action (verb like read/write/delete), resource (type, id, owner, environment, tags), and request context (method, path, tenant, auth scopes).
- Decision contract: allow/deny plus optional obligations (e.g., redactions, field-level filters, required tags); include trace id for audits.
- Evaluation: centralized PDP or sidecar OPA; cache short-lived decisions; fail-closed on policy/OPA errors.
- Example Rego (simplified):
	- package authz
	- default allow = false
	- allow {
			input.action == "read"
			input.resource.type == "application"
			"viewer" in input.subject.roles
	}
