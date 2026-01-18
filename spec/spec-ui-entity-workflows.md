---
title: Entity Management UI Workflows Specification
version: 1.0
date_created: 2026-01-17
owner: EA Platform Team
tags: [ui, workflows, entities, patterns]
---

# Entity Management UI Workflows Specification

## 1. Purpose & Scope

This specification defines the UI workflows for managing each entity type in the EA Tool system using a **CQRS-driven architecture**. It covers:
- **List workflows**: Query-based entity retrieval and filtering
- **Detail workflows**: Query-based entity viewing with relationships
- **Create workflows**: Command-based entity creation (POST)
- **Edit workflows**: Command-based updates routed to specific command endpoints per entity
- **Delete workflows**: Command-based deletion with approval requirements

Each workflow includes list views, detail views, create/edit forms, and related entity management.

## 2. Common Workflow Elements

### List View Components
All entity list views include:
- **Search bar**: Global search within entity type
- **Filter controls**: By status, owner, environment, type, etc.
- **Sort options**: Default sort + customizable columns
- **View toggle**: Table vs. card view
- **Bulk actions**: Select multiple and perform actions
- **Create button**: Launch create form
- **Export option**: Export filtered results

### Detail View Components
All entity detail views include:
- **Header**: Entity name, type badge, status indicator
- **Tabs**: Overview, Relationships, Audit, Settings
- **Action bar**: Edit, Delete, More actions
- **Properties panel**: Display all properties
- **Related entities**: Show connections
- **Audit trail**: Show change history

### Form Pattern
All create/edit forms follow CQRS patterns:
- **Create**: POST to collection endpoint (e.g., `POST /applications`)
- **Edit**: Route to specific command endpoints where defined; fallback to PATCH for other fields
  - **Applications**: classification, lifecycle, owner → commands; others → PATCH
  - **BusinessCapabilities & Organizations**: parent, description → commands; others → PATCH
  - **Other entities**: All edits → PATCH (no specific commands defined)
- **Delete**: Modal captures approval_id + reason, dispatches `DELETE /entity/{id}?approval_id={id}&reason={reason}`
- **Required field marker**: * for required fields
- **Field validation**: Real-time validation with error messages
- **Type-specific fields**: Per entity type
- **Relationship selector**: For entity references
- **Save/Cancel buttons**: Standard form actions
- **Autosave indicator**: Show save status (respects CQRS: no auto-dispatch for multi-command edits)

## 3. Application Management Workflow

### List View: Applications
```
Header: Applications (N)
[Search] [Filter] [Sort] [+ Create] [View]

Column Headers: Select | Name | Owner | Environment | Status | Modified
Row Actions: Click to open | Dropdown menu

Filters:
- Environment: Production, Staging, Development, Test
- Status: Active, Inactive, Deprecated
- Owner: User filter
- Technology: Filter by tech stack
```

### Detail View: Application
```
[Home] > [Applications] > [Application Name]

Header: [Edit] [Delete] [More ▼]

Tabs:
- Overview (default): Display all properties
- Architecture: Show diagram view
- Integrations: List related integrations
- Servers: List deployed servers
- Services: List application services
- Interfaces: List exposed interfaces
- Audit: Change history

Overview Tab:
- Name, Description, Owner, Status
- Technology Stack, Environment
- Department, Business Owner
- Created Date, Last Modified
- Related count badges
```

### Create/Edit Form: Application
```
Name *                [Text input]
Description           [Textarea]
Owner *              [User select]
Status *             [Active/Inactive/Deprecated radio]
Environment *        [Prod/Staging/Dev/Test select]
Type *               [Select]
Technology Stack     [Multi-select tags]
Department           [Select]
Business Owner       [User select]
Critical Flag        [Checkbox]
URL                  [Text input, URL validation]

Create Flow:
- POST /applications with form data
- On 201: redirect to detail page
- On 422: display validation errors per field

Edit Flow:
- Load entity via GET /applications/{id}
- User edits and clicks Save
- For fields: classification, lifecycle, owner → dispatch command endpoints
- For other fields → PATCH /applications/{id}
- On 200: refetch entity, display success toast
- On 422: display validation errors
- On 403: show permission denied error

Delete Flow:
- Delete button → confirmation modal
- Modal prompts for:
  - Approval ID (context/approver reference)
  - Reason (user-provided deletion reason)
- On confirm: DELETE /applications/{id}?approval_id={id}&reason={reason}
- On 204: redirect to applications list
- On 409: show conflict error (has references)
- On 403: show permission error

Validation:
- Name: Required, 2-100 chars, unique
- Owner: Required, valid user
- Environment: Required, one of enum
- Status: Required, one of enum
- URL: If provided, valid URL format

[Cancel] [Save] [+ Save & Create Another]
```

**Command Dispatch (Edit Mode):**
```
When user modifies:
- "Classification" → POST /applications/{id}/commands/set-classification
- "Lifecycle" → POST /applications/{id}/commands/transition-lifecycle
- "Owner" → POST /applications/{id}/commands/set-owner
- All others → PATCH /applications/{id}
```

## 4. Server Management Workflow

### List View: Servers
```
Filters:
- Environment: Production, Staging, Dev, Test
- Status: Running, Stopped, Maintenance, Error
- OS Type: Linux, Windows, macOS, Cloud
- Owner: Team filter
```

### Detail View: Server
```
Tabs:
- Overview: Server properties, connection info
- Applications: Deployed applications
- Integrations: Connected integrations
- Network: IP, DNS, ports information
- Performance: Resource usage, monitoring
- Audit: Change history

Properties:
- Name, Host, IP Address, Port
- OS Type, OS Version
- Environment, Status
- Owner, Backup Schedule
- Description, Tags
```

### Create/Edit Form: Server
```
Name *               [Text input]
Host *              [Text input]
IP Address          [IP input]
Environment *       [Select]
OS Type *          [Select: Linux/Windows/macOS/Cloud]
OS Version         [Text input]
Owner *            [User select]
Backup Schedule    [Select: Never/Daily/Weekly/Monthly]
Description        [Textarea]
Tags               [Multi-select]

Create Flow:
- POST /servers with form data
- On 201: redirect to detail page
- On 422: display validation errors per field

Edit Flow:
- Load entity via GET /servers/{id}
- User edits and clicks Save
- All fields → PATCH /servers/{id} (no specific server commands)
- On 200: refetch entity, display success toast
- On 422: display validation errors
- On 403: show permission denied error

Delete Flow:
- Delete button → confirmation modal
- Modal prompts for:
  - Approval ID (context/approver reference)
  - Reason (user-provided deletion reason)
- On confirm: DELETE /servers/{id}?approval_id={id}&reason={reason}
- On 204: redirect to servers list
- On 409: show conflict error (has applications deployed)
- On 403: show permission error

Validation:
- Name: Required, 2-100 chars, unique
- Host: Required, valid hostname
- IP: Optional, valid IPv4/IPv6
- Environment: Required, one of enum
- OS Type: Required, one of enum
- Owner: Required, valid user

[Cancel] [Save] [+ Save & Create Another]

**Command Dispatch (Edit Mode):**
```
When user modifies:
- All fields → PATCH /servers/{id}
- (Servers have no specific command endpoints)
```

## 5. Integration Management Workflow

### List View: Integrations
```
Filters:
- Protocol: REST, SOAP, GraphQL, Database, File, Message
- Status: Active, Inactive, Testing
- Frequency: Real-time, Hourly, Daily, Weekly, Monthly
- Owner: Team filter
```

### Detail View: Integration
```
Tabs:
- Overview: Basic properties
- Source & Target: Connection endpoints
- Data Contract: Mapping and transformation
- Performance: SLA, latency, error rates
- History: Transaction history
- Audit: Change log

Key Data:
- Source System, Target System
- Protocol, Frequency, Direction
- SLA %, Error Rate Threshold
- Last Run, Next Scheduled Run
- Data Volume Metrics
```

### Create/Edit Form: Integration
```
Name *                   [Text input]
Description              [Textarea]
Source System *         [System select]
Target System *         [System select]
Protocol *              [Select enum]
Frequency *             [Select: RT/1h/1d/1w/1m]
SLA *                   [Number: 99.0-99.99%]
Direction *             [Unidirectional/Bidirectional]
Owner *                 [User select]
Data Classification *   [Enum: Public/Internal/Confidential]
Error Threshold %       [Number]
Retry Policy            [Select]
Tags                    [Multi-select]

Create Flow:
- POST /integrations with form data
- On 201: redirect to detail page
- On 422: display validation errors per field

Edit Flow:
- Load entity via GET /integrations/{id}
- User edits and clicks Save
- All fields → PATCH /integrations/{id} (no specific integration commands)
- On 200: refetch entity, display success toast
- On 422: display validation errors
- On 403: show permission denied error

Delete Flow:
- Delete button → confirmation modal
- Modal prompts for:
  - Approval ID (context/approver reference)
  - Reason (user-provided deletion reason)
- On confirm: DELETE /integrations/{id}?approval_id={id}&reason={reason}
- On 204: redirect to integrations list
- On 409: show conflict error (integration in use)
- On 403: show permission error

Data Contract:
+ Add Mapping
  Source Field > Target Field
  Transformation Rule

Validation:
- Name: Required, 2-100 chars, unique
- Source System: Required, valid system
- Target System: Required, valid system
- Protocol: Required, one of enum
- Frequency: Required, one of enum
- Owner: Required, valid user

[Cancel] [Save] [+ Save & Create Another]

**Command Dispatch (Edit Mode):**
```
When user modifies:
- All fields → PATCH /integrations/{id}
- (Integrations have no specific command endpoints)
```

## 6. DataEntity Management Workflow

### List View: Data Entities
```
Filters:
- Classification: Public, Internal, Confidential, Restricted
- Owner: Department filter
- PII Flag: Has PII, No PII
- Status: Active, Deprecated
- System: Filter by owning system
```

### Detail View: DataEntity
```
Tabs:
- Overview: Basic properties
- Lineage: Data flow diagram
- Schema: Column definitions, types
- Retention: Retention policies
- Sensitivity: PII, encryption status
- Audit: Change history

Data:
- Name, Owner, Description
- Classification, Sensitivity Level
- Has PII Flag, Encrypted Flag
- Retention Period
- System of Record
- Related Systems
- Last Modified
```

### Create/Edit Form: DataEntity
```
Name *                    [Text input]
Description               [Textarea]
Owner *                  [Department select]
Classification *         [Enum: Public/Internal/Confidential/Restricted]
Sensitivity Level        [Enum: Low/Medium/High]
Has PII *               [Checkbox]
Encrypted *             [Checkbox]
Retention Period *      [Select: 1y/3y/5y/7y/Indefinite]
Retention Unit          [Years/Months/Days]
System of Record *      [System select]
Related Systems         [Multi-select systems]
Backup Required         [Checkbox]
Backup Frequency        [Select]
Compliance Rules        [Multi-select]
Tags                    [Multi-select]

Create Flow:
- POST /data-entities with form data
- On 201: redirect to detail page
- On 422: display validation errors per field

Edit Flow:
- Load entity via GET /data-entities/{id}
- User edits and clicks Save
- All fields → PATCH /data-entities/{id} (no specific data-entity commands)
- On 200: refetch entity, display success toast
- On 422: display validation errors
- On 403: show permission denied error

Delete Flow:
- Delete button → confirmation modal
- Modal prompts for:
  - Approval ID (context/approver reference)
  - Reason (user-provided deletion reason)
- On confirm: DELETE /data-entities/{id}?approval_id={id}&reason={reason}
- On 204: redirect to data-entities list
- On 409: show conflict error (data entity in use by systems)
- On 403: show permission error

Schema Editor:
+ Add Column
  Name | Type | Nullable | Description | Sensitive

Validation:
- Name: Required, 2-100 chars, unique
- Owner: Required, valid department
- Classification: Required, one of enum
- System of Record: Required, valid system
- Retention Period: Required, positive integer

[Cancel] [Save] [+ Save & Create Another]

**Command Dispatch (Edit Mode):**
```
When user modifies:
- All fields → PATCH /dataentities/{id}
- (DataEntities have no specific command endpoints)
```

## 7. Business Capability Management Workflow

### List View: Business Capabilities
```
Filters:
- Status: Planned, Building, Active, Retiring
- Owner: Department filter
- Architecture Style: Microservice, Monolith, etc.
```

### Detail View: Business Capability
```
Tabs:
- Overview: Capability properties
- Supported By: Supporting applications
- Processes: Associated processes
- Data: Related data entities
- Roadmap: Strategic timeline
- Audit: History

Data:
- Name, Description
- Owner, Strategic Value
- Current State, Target State
- Supporting Applications
- Key Performance Indicators
- Timeline, Status
```

### Create/Edit Form: Business Capability
```
Name *                    [Text input]
Description               [Textarea]
Owner *                  [Department select]
Status *                 [Enum: Planned/Building/Active/Retiring]
Strategic Value          [Enum: Critical/High/Medium/Low]
Architecture Style       [Enum: Microservice/Monolith/Hybrid]
Current State            [Text input]
Target State             [Text input]
Performance KPI          [Text input]
Timeline                 [Date range]
Supporting Applications  [Multi-select]

Create Flow:
- POST /business-capabilities with form data
- On 201: redirect to detail page
- On 422: display validation errors per field

Edit Flow:
- Load entity via GET /business-capabilities/{id}
- User edits and clicks Save
- For fields: parent, description → dispatch command endpoints (set-parent, remove-parent, update-description)
- For other fields → PATCH /business-capabilities/{id}
- On 200: refetch entity, display success toast
- On 422: display validation errors
- On 403: show permission denied error

Delete Flow:
- Delete button → confirmation modal
- Modal prompts for:
  - Approval ID (context/approver reference)
  - Reason (user-provided deletion reason)
- On confirm: DELETE /business-capabilities/{id}?approval_id={id}&reason={reason}
- On 204: redirect to business-capabilities list
- On 409: show conflict error (supporting applications exist)
- On 403: show permission error

Validation:
- Name: Required, 2-100 chars, unique
- Owner: Required, valid department
- Status: Required, one of enum

[Cancel] [Save] [+ Save & Create Another]

**Command Dispatch (Edit Mode):**
```
When user modifies:
- "Parent" (relationship) → POST /business-capabilities/{id}/commands/set-parent
- "Parent" removal → POST /business-capabilities/{id}/commands/remove-parent
- "Description" → POST /business-capabilities/{id}/commands/update-description
- All others → PATCH /business-capabilities/{id}
```

## 8. Organization Management Workflow

### List View: Organizations
```
Filters:
- Status: Active, Inactive, Pending
- Owner: Parent org filter
- Type: Department, Team, etc.
```

### Detail View: Organization
```
Tabs:
- Overview: Organization properties
- Members: Team members
- Applications: Owned applications
- Relationships: Parent/child organizations
- Audit: History

Data:
- Name, Description
- Owner, Parent Organization
- Status, Type
- Member Count
- Owned Applications Count
```

### Create/Edit Form: Organization
```
Name *                    [Text input]
Description               [Textarea]
Owner *                  [User select]
Status *                 [Enum: Active/Inactive/Pending]
Type                     [Enum: Department/Team/Division]
Parent Organization      [Org select - for hierarchy]
Tags                     [Multi-select]

Create Flow:
- POST /organizations with form data
- On 201: redirect to detail page
- On 422: display validation errors per field

Edit Flow:
- Load entity via GET /organizations/{id}
- User edits and clicks Save
- For fields: parent → dispatch command endpoints (set-parent, remove-parent)
- For other fields → PATCH /organizations/{id}
- On 200: refetch entity, display success toast
- On 422: display validation errors
- On 403: show permission denied error

Delete Flow:
- Delete button → confirmation modal
- Modal prompts for:
  - Approval ID (context/approver reference)
  - Reason (user-provided deletion reason)
- On confirm: DELETE /organizations/{id}?approval_id={id}&reason={reason}
- On 204: redirect to organizations list
- On 409: show conflict error (has members or applications)
- On 403: show permission error

Validation:
- Name: Required, 2-100 chars, unique
- Owner: Required, valid user
- Status: Required, one of enum

[Cancel] [Save] [+ Save & Create Another]

**Command Dispatch (Edit Mode):**
```
When user modifies:
- "Parent Organization" → POST /organizations/{id}/commands/set-parent
- "Parent Organization" removal → POST /organizations/{id}/commands/remove-parent
- All others → PATCH /organizations/{id}
```

## 9. Application Service Management Workflow

### List View: Application Services
```
Filters:
- Status: Available, Unavailable, Deprecated
- Application: Filter by parent application
- Type: Synchronous, Asynchronous, Webhook
- Owner: Team filter
```

### Detail View: Application Service
```
Tabs:
- Overview: Service properties
- Contract: Service contract/interface
- Consumers: Systems consuming service
- Performance: SLA, latency metrics
- Audit: History

Data:
- Name, Description
- Application, Type
- Status, Owner
- Service Contract
- Consumer Count
```

### Create/Edit Form: Application Service
```
Name *                    [Text input]
Description               [Textarea]
Application *            [Application select]
Type *                   [Enum: Synchronous/Asynchronous/Webhook]
Status *                 [Enum: Available/Unavailable/Deprecated]
Owner *                  [User select]
Service Contract         [Text/Code editor]
SLA %                    [Number: 99.0-99.99%]
Timeout (ms)             [Number input]
Retry Policy             [Select]
Tags                     [Multi-select]

Create Flow:
- POST /application-services with form data
- On 201: redirect to detail page
- On 422: display validation errors per field

Edit Flow:
- Load entity via GET /application-services/{id}
- User edits and clicks Save
- For fields: type, business-capability → dispatch command endpoints (update, set-business-capability)
- For other fields → PATCH /application-services/{id}
- On 200: refetch entity, display success toast
- On 422: display validation errors
- On 403: show permission denied error

Delete Flow:
- Delete button → confirmation modal
- Modal prompts for:
  - Approval ID (context/approver reference)
  - Reason (user-provided deletion reason)
- On confirm: DELETE /application-services/{id}?approval_id={id}&reason={reason}
- On 204: redirect to application-services list
- On 409: show conflict error (has consumers)
- On 403: show permission error

Validation:
- Name: Required, 2-100 chars, unique
- Application: Required, valid application
- Type: Required, one of enum
- Owner: Required, valid user

[Cancel] [Save] [+ Save & Create Another]

**Command Dispatch (Edit Mode):**
```
When user modifies:
- "Type" or other core fields → POST /application-services/{id}/commands/update
- "Business Capability" → POST /application-services/{id}/commands/set-business-capability
- All other fields → PATCH /application-services/{id}
```

## 10. Application Interface Management Workflow

### List View: Application Interfaces
```
Filters:
- Status: Active, Inactive, Deprecated
- Type: REST, SOAP, GraphQL, Message Queue
- Application: Filter by app
- Owner: Team filter
```

### Detail View: Application Interface
```
Tabs:
- Overview: Interface properties
- Endpoints: API endpoints/methods
- Integrations: Connected integrations
- Performance: Metrics and monitoring
- Audit: History

Data:
- Name, Description
- Application, Type
- Status, Owner
- Protocol, Version
- Endpoint Count
```

### Create/Edit Form: Application Interface
```
Name *                    [Text input]
Description               [Textarea]
Application *            [Application select]
Type *                   [Enum: REST/SOAP/GraphQL/Message]
Protocol *               [Enum: HTTP/HTTPS/AMQP/etc]
Status *                 [Enum: Active/Inactive/Deprecated]
Owner *                  [User select]
Base URL                 [Text input, URL validation]
API Version              [Text input]
Rate Limit               [Number per time unit]
Authentication Type      [Select]
Tags                     [Multi-select]

Create Flow:
- POST /application-interfaces with form data
- On 201: redirect to detail page
- On 422: display validation errors per field

Edit Flow:
- Load entity via GET /application-interfaces/{id}
- User edits and clicks Save
- For fields: type, service, status → dispatch command endpoints (update, set-service, deprecate, retire)
- For other fields → PATCH /application-interfaces/{id}
- On 200: refetch entity, display success toast
- On 422: display validation errors
- On 403: show permission denied error

Delete Flow:
- Delete button → confirmation modal
- Modal prompts for:
  - Approval ID (context/approver reference)
  - Reason (user-provided deletion reason)
- On confirm: DELETE /application-interfaces/{id}?approval_id={id}&reason={reason}
- On 204: redirect to application-interfaces list
- On 409: show conflict error (has integrations)
- On 403: show permission error

Validation:
- Name: Required, 2-100 chars, unique
- Application: Required, valid application
- Type: Required, one of enum
- Protocol: Required, one of enum
- Owner: Required, valid user

[Cancel] [Save] [+ Save & Create Another]

**Command Dispatch (Edit Mode):**
```
When user modifies:
- "Type" or core fields → POST /application-interfaces/{id}/commands/update
- "Service" → POST /application-interfaces/{id}/commands/set-service
- "Status" to deprecated → POST /application-interfaces/{id}/commands/deprecate
- "Status" to retired → POST /application-interfaces/{id}/commands/retire
- All other fields → PATCH /application-interfaces/{id}
```

### List View: Relations
```
Filters:
- Type: Owns, Integrates, Processes, Stores, etc.
- Source Type: Entity type filter
- Target Type: Entity type filter
- Bidirectional: Yes/No
```

### Detail View: Relation
```
Header: Source Entity → Target Entity

Data:
- Relationship Type
- Direction (Unidirectional/Bidirectional)
- Properties (version, contract, etc.)
- Impact Analysis
- Related Relations

Actions:
- Edit Relationship
- Delete Relationship
- View Path Analysis
```

### Create/Edit Form: Relation
```
Source Entity *    [Searchable entity select]
Target Entity *    [Searchable entity select]
Type *            [Select from enum]
Direction *       [Unidirectional/Bidirectional]
Properties        [Dynamic based on type]
Description       [Textarea]
Strength         [Required/Optional]
Cardinality      [1:1, 1:N, N:N]

Create Flow:
- POST /relations with form data
- On 201: redirect to detail page
- On 422: display validation errors per field

Edit Flow:
- Load entity via GET /relations/{id}
- User edits and clicks Save
- For fields: confidence, effective dates, description → dispatch command endpoints
- For other fields → PATCH /relations/{id}
- On 200: refetch entity, display success toast
- On 422: display validation errors
- On 403: show permission denied error

Delete Flow:
- Delete button → confirmation modal
- Modal prompts for:
  - Approval ID (context/approver reference)
  - Reason (user-provided deletion reason)
- On confirm: DELETE /relations/{id}?approval_id={id}&reason={reason}
- On 204: redirect to relations list
- On 409: show conflict error (related to other entities)
- On 403: show permission error

Validation:
- Source Entity: Required, valid entity ID
- Target Entity: Required, valid entity ID, not same as source
- Type: Required, one of enum
- Direction: Required, one of enum

[Cancel] [Save] [+ Save & Create Another]

**Command Dispatch (Edit Mode):**
```
When user modifies:
- "Confidence" → POST /relations/{id}/commands/update-confidence
- "Effective Dates" → POST /relations/{id}/commands/set-effective-dates
- "Description" → POST /relations/{id}/commands/update-description
- All others → PATCH /relations/{id}
```

## 12. Cross-Cutting Workflows

### Search Across All Entities
```
[Global Search Box - always visible]
Search for: "term"

Results:
- Applications (3)
  • App 1
  • App 2
  • App 3
- Servers (2)
  • Server 1
  • Server 2
- Integrations (1)
  • Integration 1

[Load More Results]
```

### Bulk Operations Workflow
```
1. User selects multiple items via checkboxes
2. Bulk action toolbar appears at bottom
3. User selects action from dropdown:
   - Delete All
   - Export
   - Change Owner
   - Add Tags
   - Change Status
4. If destructive (delete): Confirmation modal
5. Action executes with progress indicator
6. Success/error notification
7. Toolbar disappears
```

### Export Workflow
```
1. User clicks Export
2. Export modal opens
3. Options:
   - Format: CSV, JSON, Excel
   - Include: Select columns/properties
   - Filters: Apply current view filters
4. User clicks Export
5. File downloads
6. Success notification
```

### Audit History Workflow
```
Timeline view showing:
- Date/Time (local timezone)
- User who made change
- Action (Created/Updated/Deleted/Tagged)
- Field changed (if applicable)
- Old value → New value
- Expandable for details

Filters:
- Date range
- User filter
- Action type filter
```

## 13. Validation Criteria

Entity-specific workflows must include:
- [ ] List views with search, filter, sort, pagination
- [ ] Detail views with all related data
- [ ] Create/Edit forms with validation
- [ ] Delete confirmation dialogs
- [ ] Related entity navigation
- [ ] Audit trail display
- [ ] Bulk operation support
- [ ] Export functionality
- [ ] Error handling and recovery
- [ ] Mobile responsive layout

## 14. Related Specifications

- [spec-design-ui-architecture.md](spec-design-ui-architecture.md) - Design system and component library
- [spec-tool-error-handling.md](spec-tool-error-handling.md) - Error display and recovery
- [spec-schema-domain-overview.md](spec-schema-domain-overview.md) - Entity definitions
