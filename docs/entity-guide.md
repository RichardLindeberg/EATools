# Entity Guide

This guide describes all domain entities in the EA Tool, their purpose, key attributes, and usage patterns.

## Core Entities

### Organization

**Purpose**: Represents a business organization or organizational unit that owns or manages IT assets.

**ArchiMate Element**: `BusinessActor` or `BusinessRole`

**Key Attributes**:
- `id`: Unique identifier
- `name`: Organization name
- `parent_id`: FK to parent organization (for hierarchical structures like departments within divisions)
- `domains`: DNS domains owned (e.g., `["example.com", "example.org"]`)
- `contacts`: Email addresses of organization contacts
- `created_at`, `updated_at`: Audit timestamps (UTC)

**Usage**:
- Model organizational hierarchies (e.g., departments → divisions → enterprise)
- Link applications and servers to their owning organization via `owns` relations
- Track which teams or departments are responsible for specific IT assets
- Query organizational structures using `parent_id` for reporting and hierarchy views

**Example**:
```json
{
  "id": "org-123",
  "name": "Payments Team",
  "parent_id": "org-100",
  "domains": ["payments.example.com"],
  "contacts": ["payments-lead@example.com"],
  "created_at": "2024-01-05T10:00:00Z",
  "updated_at": "2024-01-05T10:00:00Z"
}
```

**Hierarchical Example**:
```json
[
  {
    "id": "org-100",
    "name": "Engineering Division",
    "parent_id": null,
    "domains": ["engineering.example.com"],
    "contacts": ["eng-vp@example.com"]
  },
  {
    "id": "org-123",
    "name": "Payments Team",
    "parent_id": "org-100",
    "domains": ["payments.example.com"],
    "contacts": ["payments-lead@example.com"]
  }
]
```

---

### Application

**Purpose**: Represents a software application or system component.

**ArchiMate Element**: `ApplicationComponent`

**Key Attributes**:
- `id`: Unique identifier
- `name`: Application name
- `owner`: Team or individual responsible
- `lifecycle`: Current phase (`planned`, `active`, `deprecated`, `retired`)
- `capability_id`: FK to BusinessCapability this app supports
- `data_classification`: Sensitivity level (`public`, `internal`, `confidential`, `restricted`)
- `tags`: Array of labels for filtering and grouping

**Usage**:
- Catalog all applications in the enterprise
- Track lifecycle status for portfolio management
- Link to business capabilities to show coverage
- Model deployment via `deployed_on` relations to servers
- Model data access via `reads`/`writes` relations to data entities
- Model service exposure via `realizes` relations to application services

**Relationships**:
- `application` → `server` (deployed_on, stores_data_on)
- `application` → `business_capability` (supports)
- `application` → `data_entity` (reads, writes)
- `application` → `application_service` (realizes, uses)
- `application` → `application_interface` (exposes)
- `application` → `application` (depends_on, communicates_with, calls)
- `organization` → `application` (owns)

**Example**:
```json
{
  "id": "app-123",
  "name": "Billing Service",
  "owner": "payments-team",
  "lifecycle": "active",
  "capability_id": "cap-payments",
  "data_classification": "confidential",
  "tags": ["payments", "pci-dss"],
  "created_at": "2024-01-05T10:00:00Z",
  "updated_at": "2024-01-05T10:00:00Z"
}
```

---

### ApplicationService

**Purpose**: Represents a business service or capability that applications provide to consumers. Focuses on *what* capability is offered (business perspective).

**ArchiMate Element**: `ApplicationService`

**Key Attributes**:
- `id`: Unique identifier
- `name`: Service name (business-oriented)
- `description`: What the service does
- `business_capability_id`: FK to the capability this service realizes
- `sla`: Service level agreement (e.g., "99.95% uptime")
- `exposed_by_app_ids`: Applications that implement this service
- `consumers`: List of consumer applications or teams
- `tags`: Labels for categorization

**Usage**:
- Model business services independent of implementation
- Map services to business capabilities
- Track which applications realize which services
- Document service consumers and SLAs
- Enable service-oriented architecture (SOA) views

**Relationships**:
- `application` → `application_service` (realizes, uses)
- `application_service` → `business_capability` (realizes, supports)
- `application_interface` → `application_service` (serves)

**Example**:
```json
{
  "id": "svc-payment",
  "name": "Payment Processing Service",
  "description": "Handles credit card and ACH payment processing with PCI compliance",
  "business_capability_id": "cap-payments",
  "sla": "99.95% uptime, <500ms p95 latency",
  "exposed_by_app_ids": ["app-billing-api"],
  "consumers": ["app-checkout", "app-invoicing"],
  "tags": ["payments", "pci-dss"],
  "created_at": "2024-01-05T10:00:00Z",
  "updated_at": "2024-01-05T10:00:00Z"
}
```

---

### ApplicationInterface

**Purpose**: Represents a technical interface (API endpoint, message queue, etc.) through which applications expose services. Focuses on *where* and *how* services are accessed (technical perspective).

**ArchiMate Element**: `ApplicationInterface`

**Key Attributes**:
- `id`: Unique identifier
- `name`: Interface name (technical)
- `protocol`: Communication protocol (e.g., `https`, `grpc`, `kafka`)
- `endpoint`: URL or connection string
- `specification_url`: Link to API spec (OpenAPI, AsyncAPI, etc.)
- `version`: Interface version (e.g., `v2.1.0`)
- `authentication_method`: Auth mechanism (e.g., `oauth2`, `api-key`, `mtls`)
- `exposed_by_app_id`: FK to the application hosting this interface
- `serves_service_ids`: Application services this interface provides access to
- `rate_limits`: Throttling config (requests/sec, burst, quotas)
- `status`: `active`, `deprecated`, or `retired`

**Usage**:
- Inventory all APIs and technical endpoints
- Track interface versions and deprecation lifecycle
- Document authentication and rate limiting
- Link interfaces to the services they expose
- Model integration dependencies via `calls` or `uses` relations

**Relationships**:
- `application` → `application_interface` (exposes)
- `application_interface` → `application_service` (serves)
- `integration` → `application_interface` (calls, uses)

**Example**:
```json
{
  "id": "intf-payment-api-v2",
  "name": "Payment API v2",
  "protocol": "https",
  "endpoint": "https://api.example.com/v2/payments",
  "specification_url": "https://api.example.com/v2/openapi.yaml",
  "version": "2.1.0",
  "authentication_method": "oauth2",
  "exposed_by_app_id": "app-billing-api",
  "serves_service_ids": ["svc-payment"],
  "rate_limits": {
    "requests_per_second": 100,
    "burst": 200
  },
  "status": "active",
  "tags": ["rest", "public-api"],
  "created_at": "2024-01-05T10:00:00Z",
  "updated_at": "2024-01-05T10:00:00Z"
}
```

---

### Server

**Purpose**: Represents a physical or virtual server, host, or compute instance.

**ArchiMate Element**: `Node` (Technology Layer)

**Key Attributes**:
- `id`: Unique identifier
- `hostname`: Server hostname or FQDN
- `environment`: Deployment environment (`dev`, `staging`, `prod`)
- `region`: Geographic region or data center
- `platform`: OS or platform type (e.g., `linux`, `windows`, `kubernetes`)
- `criticality`: Business impact (`low`, `medium`, `high`, `critical`)
- `owning_team`: Team responsible for maintenance
- `tags`: Labels for filtering

**Usage**:
- Track infrastructure inventory
- Model application deployment via `deployed_on` relations
- Visualize infrastructure topology with `connected_to` relations between servers
- Plan capacity and lifecycle management

**Relationships**:
- `application` → `server` (deployed_on, stores_data_on)
- `server` → `server` (connected_to)
- `organization` → `server` (owns)

**Example**:
```json
{
  "id": "srv-web-01",
  "hostname": "web-prod-01.example.com",
  "environment": "prod",
  "region": "us-east-1",
  "platform": "linux",
  "criticality": "high",
  "owning_team": "platform-team",
  "tags": ["kubernetes", "payments"],
  "created_at": "2024-01-05T10:00:00Z",
  "updated_at": "2024-01-05T10:00:00Z"
}
```

---

### Integration

**Purpose**: Represents a connection or data flow between applications.

**ArchiMate Element**: `ApplicationInteraction` or `Flow`

**Key Attributes**:
- `id`: Unique identifier
- `source_app_id`: FK to source application
- `target_app_id`: FK to target application
- `protocol`: Communication protocol (e.g., `https`, `kafka`, `sftp`)
- `data_contract`: Format or schema (e.g., `json`, `xml`, `avro`)
- `sla`: Service level agreement
- `frequency`: Synchronization cadence (`realtime`, `batch`, `hourly`)
- `tags`: Labels

**Usage**:
- Model point-to-point integrations
- Track data flows between applications
- Identify integration complexity (spaghetti diagrams)
- Document protocols and SLAs for operations

**Relationships**:
- `integration` → `application` (communicates_with, publishes_event_to, consumes_event_from)
- `integration` → `application_interface` (calls, uses)

**Example**:
```json
{
  "id": "int-billing-to-crm",
  "source_app_id": "app-billing",
  "target_app_id": "app-crm",
  "protocol": "https",
  "data_contract": "json",
  "sla": "99.9% delivery",
  "frequency": "realtime",
  "tags": ["webhook", "customer-sync"],
  "created_at": "2024-01-05T10:00:00Z",
  "updated_at": "2024-01-05T10:00:00Z"
}
```

---

### BusinessCapability

**Purpose**: Represents a business capability or function independent of how it is implemented.

**ArchiMate Element**: `BusinessFunction` or `BusinessCapability`

**Key Attributes**:
- `id`: Unique identifier
- `name`: Capability name
- `parent_id`: FK to parent capability (for hierarchies)
- `created_at`, `updated_at`: Audit timestamps

**Usage**:
- Build a business capability model (hierarchy)
- Map applications to capabilities to assess coverage
- Identify gaps or overlaps in application portfolio
- Group views by capability for business-oriented perspectives

**Relationships**:
- `business_capability` → `business_capability` (parent-child hierarchy)
- `application` → `business_capability` (supports)
- `application_service` → `business_capability` (realizes, supports)

**Example**:
```json
{
  "id": "cap-payments",
  "name": "Payment Processing",
  "parent_id": "cap-finance",
  "created_at": "2024-01-05T10:00:00Z",
  "updated_at": "2024-01-05T10:00:00Z"
}
```

---

### DataEntity

**Purpose**: Represents a significant data object, dataset, or data asset that applications create, read, update, or delete.

**ArchiMate Element**: `DataObject` or `BusinessObject`

**Key Attributes**:
- `id`: Unique identifier
- `name`: Data entity name (e.g., "Customer Profile", "Order")
- `domain`: Business domain (e.g., "customer", "sales")
- `classification`: Data sensitivity (`public`, `internal`, `confidential`, `restricted`)
- `retention`: Retention policy (e.g., "7 years", "90 days")
- `owner`: Data owner (team or role)
- `steward`: Data steward (responsible for quality/governance)
- `source_system`: System of record
- `criticality`: Business impact (`low`, `medium`, `high`, `critical`)
- `pii_flag`: Boolean indicating personal data
- `glossary_terms`: Array of business glossary terms
- `lineage`: Array of source/derived data entity IDs

**Usage**:
- Catalog important data assets
- Track data lineage and provenance
- Model data access via `reads`/`writes` relations from applications
- Support data governance and compliance (GDPR, CCPA)
- Identify critical data for protection and backup

**Relationships**:
- `application` → `data_entity` (reads, writes)

**Example**:
```json
{
  "id": "data-customer-profile",
  "name": "Customer Profile",
  "domain": "customer",
  "classification": "confidential",
  "retention": "7 years",
  "owner": "customer-data-team",
  "steward": "data-governance",
  "source_system": "crm",
  "criticality": "high",
  "pii_flag": true,
  "glossary_terms": ["customer", "pii", "profile"],
  "lineage": ["data-raw-crm-export"],
  "created_at": "2024-01-05T10:00:00Z",
  "updated_at": "2024-01-05T10:00:00Z"
}
```

---

### Relation

**Purpose**: Models typed, directed relationships between entities with ArchiMate semantics and governance metadata.

**Key Attributes**:
- `id`: Unique identifier
- `source_id`, `target_id`: Entity IDs
- `source_type`, `target_type`: Entity types (enum: organization, application, application_service, application_interface, server, integration, business_capability, data_entity, view)
- `relation_type`: Semantic relationship (enum: depends_on, communicates_with, calls, publishes_event_to, consumes_event_from, deployed_on, stores_data_on, reads, writes, owns, supports, implements, realizes, serves, connected_to, exposes, uses)
- `archimate_element`: Optional ArchiMate element type for source
- `archimate_relationship`: ArchiMate relationship type (Assignment, Realization, Serving, Access, Flow, etc.)
- `description`: Human-readable explanation
- `data_classification`: Sensitivity of the relationship/data flow
- `criticality`: Business impact
- `confidence`: Float 0-1 indicating certainty
- `evidence_source`: Where the relationship was discovered (e.g., "cmdb", "manual")
- `last_verified_at`: When the relationship was last confirmed
- `effective_from`, `effective_to`: Temporal validity
- `label`, `color`, `style`, `bidirectional`: Rendering hints

**Usage**:
- Connect entities with semantic meaning
- Enforce valid relationship pairs (validation matrix)
- Add governance metadata (confidence, evidence, verification)
- Support temporal modeling (effective dates)
- Enable rich visualization with styling hints

**See**: [Relationship Modeling Guide](./relationship-modeling.md) for detailed usage patterns

---

### View

**Purpose**: Represents a saved perspective or filtered view of the architecture for specific audiences or purposes.

**Key Attributes**:
- `id`: Unique identifier
- `name`: View name
- `description`: Purpose and audience
- `filter`: JSON object with filter criteria (tags, lifecycle, capability, etc.)
- `layout`: Layout engine config (e.g., `{"engine": "dagre", "rankdir": "LR"}`)

**Usage**:
- Save common perspectives (e.g., "Payments Landscape", "Retired Apps")
- Share views across teams
- Generate architecture diagrams on demand via `/views/{id}/render`
- Support different audiences (executives, architects, ops)

**Example**:
```json
{
  "id": "view-payments",
  "name": "Payments Landscape",
  "description": "All applications and services supporting payment capabilities",
  "filter": {
    "capability_id": "cap-payments",
    "lifecycle": ["active", "deprecated"]
  },
  "layout": {
    "engine": "dagre",
    "rankdir": "LR"
  },
  "created_at": "2024-01-05T10:00:00Z",
  "updated_at": "2024-01-05T10:00:00Z"
}
```

---

## Supporting Entities

### ImportJob

**Purpose**: Represents an asynchronous bulk import operation (CSV, Excel, JSON).

**Key Attributes**:
- `id`: Job identifier
- `status`: `pending`, `running`, `completed`, `failed`
- `input_type`: Source type (e.g., `s3`, `upload`)
- `created_by`: User who initiated the import

**Usage**: Track progress of large bulk imports, allow async processing.

---

### ExportJob

**Purpose**: Represents an asynchronous export operation.

**Key Attributes**:
- `id`: Job identifier
- `status`: `pending`, `running`, `completed`, `failed`
- `filter`: Filter criteria for what to export
- `download_url`: Pre-signed URL for completed export

**Usage**: Generate snapshots for reporting or backup, filter by criteria.

---

### Webhook

**Purpose**: Represents a registered webhook endpoint for event notifications.

**Key Attributes**:
- `id`: Unique identifier
- `url`: Target endpoint URL
- `secret`: Shared secret for HMAC signature verification
- `active`: Boolean indicating if webhook is enabled
- `events`: Array of event types to subscribe to

**Usage**: Enable integrations, notify external systems of changes.

---

## Next Steps

- [Relationship Modeling Guide](./relationship-modeling.md) - Learn how to connect entities
- [ArchiMate Alignment](./archimate-alignment.md) - Understand the ArchiMate mapping
- [API Usage Guide](./api-usage-guide.md) - Interact with the API
