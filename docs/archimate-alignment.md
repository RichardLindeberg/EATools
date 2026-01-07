# ArchiMate 3.2 Alignment

This document explains how the EA Tool's domain model aligns with ArchiMate 3.2 concepts and how to leverage this alignment for standards-compliant architecture modeling.

## What is ArchiMate?

ArchiMate is an open and independent enterprise architecture modeling language that provides a uniform representation of enterprise architectures across business, application, and technology layers. Version 3.2 is the current standard maintained by The Open Group.

## Why ArchiMate Alignment?

1. **Industry Standard**: ArchiMate is widely adopted and understood by enterprise architects
2. **Consistent Visualization**: Standard shapes, colors, and notations
3. **Interoperability**: Exchange models with ArchiMate tools (Archi, BiZZdesign, etc.)
4. **Layered Views**: Business, Application, Technology separation
5. **Semantic Richness**: Well-defined relationship semantics

## Entity to ArchiMate Element Mapping

| EA Tool Entity | ArchiMate Element | Layer | Purpose |
|----------------|-------------------|-------|---------|
| Organization | BusinessActor / BusinessRole | Business | Who performs activities |
| BusinessCapability | BusinessFunction | Business | What the business does |
| ApplicationService | ApplicationService | Application | What capability is provided |
| Application | ApplicationComponent | Application | Software that realizes services |
| ApplicationInterface | ApplicationInterface | Application | Where/how services are accessed |
| Integration | ApplicationInteraction | Application | Communication between components |
| DataEntity | DataObject / BusinessObject | Application/Business | Significant data assets |
| Server | Node | Technology | Physical/virtual infrastructure |

## ArchiMate Relationship Types

The EA Tool's relation types map to ArchiMate relationships:

### Structural Relationships

#### Composition
**Meaning**: Part-of relationship where the part cannot exist without the whole.

**EA Tool Usage**: 
- `application` → `application_interface` (exposes)

**Notation**: Solid line with filled diamond

#### Aggregation
**Meaning**: Part-of relationship where parts can exist independently.

**EA Tool Usage**: 
- Group related applications or capabilities

**Notation**: Solid line with hollow diamond

#### Assignment
**Meaning**: Responsibility or execution of behavior.

**EA Tool Usage**:
- `application` → `server` (deployed_on)
- `organization` → `application` (owns)

**Notation**: Solid line with filled circle

#### Realization
**Meaning**: Implementation of an abstraction.

**EA Tool Usage**:
- `application` → `application_service` (realizes)
- `application_service` → `business_capability` (realizes)

**Notation**: Dashed line with hollow triangle

### Dependency Relationships

#### Serving
**Meaning**: Service provided to another element.

**EA Tool Usage**:
- `application_interface` → `application_service` (serves)
- `application` → `application` (depends_on)

**Notation**: Solid line with open arrow

#### Access
**Meaning**: Read or write access to data.

**EA Tool Usage**:
- `application` → `data_entity` (reads, writes)

**Notation**: Dashed line with open arrow (R/W label)

#### Influence
**Meaning**: One element affects another.

**EA Tool Usage**:
- Soft dependencies or guidance relationships

**Notation**: Dashed line with open arrow

### Dynamic Relationships

#### Flow
**Meaning**: Transfer of information, goods, or value.

**EA Tool Usage**:
- `application` → `application` (communicates_with)
- Data flows between components

**Notation**: Dashed line with open arrow

#### Triggering
**Meaning**: Temporal or causal relationship.

**EA Tool Usage**:
- `integration` → `application` (publishes_event_to / consumes_event_from)
- Event-driven architectures via message bus or pub/sub

**Notation**: Dashed line with open arrow

### Other Relationships

#### Association
**Meaning**: Unspecified relationship.

**EA Tool Usage**:
- `server` → `server` (connected_to)
- Generic connections

**Notation**: Solid line

#### Specialization
**Meaning**: Generalization/specialization (inheritance).

**EA Tool Usage**:
- Hierarchical capability models
- Application type hierarchies

**Notation**: Solid line with hollow triangle

## Layered Architecture Views

ArchiMate organizes elements into layers:

### Business Layer
**Focus**: Business services, processes, actors, roles

**EA Tool Entities**: Organization, BusinessCapability

**Color Scheme**: Yellow/Tan (#FFF9C4)

### Application Layer
**Focus**: Application services, components, interfaces, data

**EA Tool Entities**: Application, ApplicationService, ApplicationInterface, Integration, DataEntity

**Color Scheme**: Blue (#BBDEFB)

### Technology Layer
**Focus**: Infrastructure, devices, networks, system software

**EA Tool Entities**: Server

**Color Scheme**: Green (#C8E6C9)

## Modeling Patterns

### Pattern 1: Service Realization

**ArchiMate Pattern**: Application components realize application services, which realize business capabilities.

```
BusinessCapability (Business Layer)
        ▲
        │ Realization
ApplicationService (Application Layer)
        ▲
        │ Realization
ApplicationComponent (Application Layer)
```

**EA Tool Implementation**:
```json
[
  {
    "source_id": "svc-payment",
    "target_id": "cap-payments",
    "source_type": "application_service",
    "target_type": "business_capability",
    "relation_type": "realizes",
    "archimate_relationship": "Realization"
  },
  {
    "source_id": "app-billing",
    "target_id": "svc-payment",
    "source_type": "application",
    "target_type": "application_service",
    "relation_type": "realizes",
    "archimate_relationship": "Realization"
  }
]
```

### Pattern 2: Interface Serving

**ArchiMate Pattern**: Application interfaces provide access to services.

```
ApplicationInterface
        │ Serving
        ▼
ApplicationService
```

**EA Tool Implementation**:
```json
[
  {
    "source_id": "app-billing",
    "target_id": "intf-payment-api",
    "source_type": "application",
    "target_type": "application_interface",
    "relation_type": "exposes",
    "archimate_relationship": "Composition"
  },
  {
    "source_id": "intf-payment-api",
    "target_id": "svc-payment",
    "source_type": "application_interface",
    "target_type": "application_service",
    "relation_type": "serves",
    "archimate_relationship": "Serving"
  }
]
```

### Pattern 3: Deployment

**ArchiMate Pattern**: Application components are assigned to nodes (servers).

```
ApplicationComponent
        │ Assignment
        ▼
Node (Server)
```

**EA Tool Implementation**:
```json
{
  "source_id": "app-web",
  "target_id": "srv-k8s-01",
  "source_type": "application",
  "target_type": "server",
  "relation_type": "deployed_on",
  "archimate_relationship": "Assignment"
}
```

### Pattern 4: Data Access

**ArchiMate Pattern**: Application components access data objects.

```
ApplicationComponent
        │ Access (Read/Write)
        ▼
DataObject
```

**EA Tool Implementation**:
```json
{
  "source_id": "app-crm",
  "target_id": "data-customer",
  "source_type": "application",
  "target_type": "data_entity",
  "relation_type": "writes",
  "archimate_relationship": "Access"
}
```

## Visualization Guidelines

### Element Shapes
- **Business elements**: Rounded rectangles (tan/yellow)
- **Application elements**: Rectangles (blue)
- **Technology elements**: Rectangles (green)
- **Data elements**: Rectangles with folded corner (blue)

### Relationship Styles
- **Structural** (Composition, Assignment, Realization): Solid or dashed with special endpoints
- **Dependency** (Serving, Access): Arrows
- **Dynamic** (Flow, Triggering): Dashed arrows

### Example Rendering Hints
```json
{
  "relation_type": "realizes",
  "archimate_relationship": "Realization",
  "style": "dashed",
  "color": "#2196F3",
  "label": "realizes"
}
```

## Export to ArchiMate Exchange Format

The EA Tool can export to Open Exchange Format (.xml) for import into ArchiMate tools:

```xml
<model xmlns="http://www.opengroup.org/xsd/archimate/3.0/">
  <elements>
    <element identifier="app-123" xsi:type="ApplicationComponent">
      <name>Billing Service</name>
    </element>
    <element identifier="svc-payment" xsi:type="ApplicationService">
      <name>Payment Processing Service</name>
    </element>
  </elements>
  <relationships>
    <relationship identifier="rel-1" source="app-123" target="svc-payment" 
                  xsi:type="RealizationRelationship"/>
  </relationships>
</model>
```

## Best Practices

### 1. Use Proper Layering
Respect layer boundaries:
- Business concepts at Business layer
- Application logic at Application layer
- Infrastructure at Technology layer

### 2. Follow Relationship Semantics
Use the correct ArchiMate relationship type for the semantic meaning, not just visual preference.

### 3. Model Services Explicitly
Don't skip ApplicationService - it's the key abstraction layer between business and implementation.

### 4. Interface Everything
Model interfaces explicitly for external APIs and integration points.

### 5. Keep Views Focused
Use layer-specific views:
- **Business Capability Map**: BusinessCapabilities only
- **Application Portfolio**: Applications and their services
- **Infrastructure View**: Servers and deployment

## Reference Resources

- [ArchiMate 3.2 Specification](https://pubs.opengroup.org/architecture/archimate3-doc/) - Official specification
- [Archi Tool](https://www.archimatetool.com/) - Free open-source ArchiMate modeler
- [ArchiMate Cookbook](https://www.hosiaisluoma.fi/ArchiMate-Cookbook.pdf) - Modeling patterns

## Next Steps

- [Entity Guide](./entity-guide.md) - Detailed entity descriptions
- [Relationship Modeling](./relationship-modeling.md) - How to create relations
- [API Usage Guide](./api-usage-guide.md) - API integration
