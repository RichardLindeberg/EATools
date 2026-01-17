---
title: Entity Management UI Workflows Specification
version: 1.0
date_created: 2026-01-17
owner: EA Platform Team
tags: [ui, workflows, entities, patterns]
---

# Entity Management UI Workflows Specification

## 1. Purpose & Scope

This specification defines the UI workflows for managing each entity type in the EA Tool system. It provides detailed interaction flows and screen layouts for:

- Applications
- Servers
- Integrations
- DataEntities
- BusinessCapabilities
- Relations
- Organizations
- ApplicationServices
- ApplicationInterfaces

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
All create/edit forms include:
- **Required field marker**: * for required fields
- **Field validation**: Real-time validation with error messages
- **Type-specific fields**: Per entity type
- **Relationship selector**: For entity references
- **Save/Cancel buttons**: Standard form actions
- **Autosave indicator**: Show save status

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
Technology Stack     [Multi-select tags]
Department           [Select]
Business Owner       [User select]
Critical Flag        [Checkbox]

Validation:
- Name: Required, 2-100 chars, unique
- Owner: Required, valid user
- Environment: Required, one of enum
- Status: Required, one of enum

[Cancel] [Save] [+ Save & Create Another]
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

Validation:
- Name: Required, 2-100 chars
- Host: Required, valid hostname
- IP: Optional, valid IPv4/IPv6
- Environment: Required
- OS Type: Required
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

Data Contract:
+ Add Mapping
  Source Field > Target Field
  Transformation Rule
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

Schema Editor:
+ Add Column
  Name | Type | Nullable | Description | Sensitive

[Cancel] [Save]
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

## 8. Relationship Management Workflow

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

[Cancel] [Save]
```

## 9. Cross-Cutting Workflows

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

## 10. Validation Criteria

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

## 11. Related Specifications

- [spec-design-ui-architecture.md](spec-design-ui-architecture.md) - Design system and component library
- [spec-tool-error-handling.md](spec-tool-error-handling.md) - Error display and recovery
- [spec-schema-domain-overview.md](spec-schema-domain-overview.md) - Entity definitions
