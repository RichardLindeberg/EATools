/// Domain models representing core entities
namespace EATool.Domain

open System
open System.Text.Json.Serialization

/// Organization entity
type Organization =
    {
        [<JsonPropertyName("id")>]
        Id: string
        [<JsonPropertyName("name")>]
        Name: string
        [<JsonPropertyName("domains")>]
        Domains: string list
        [<JsonPropertyName("contacts")>]
        Contacts: string list
        [<JsonPropertyName("createdAt")>]
        CreatedAt: UtcTimestamp
        [<JsonPropertyName("updatedAt")>]
        UpdatedAt: UtcTimestamp
    }

/// Application entity
type Application =
    {
        Id: string
        Name: string
        Owner: string option
        Lifecycle: Lifecycle
        LifecycleRaw: string
        CapabilityId: string option
        DataClassification: string option
        Tags: string list
        CreatedAt: UtcTimestamp
        UpdatedAt: UtcTimestamp
    }

/// Application Service entity
type ApplicationService =
    {
        Id: string
        Name: string
        Description: string option
        BusinessCapabilityId: string option
        Sla: string option
        ExposedByAppIds: string list
        Consumers: string list
        Tags: string list
        CreatedAt: UtcTimestamp
        UpdatedAt: UtcTimestamp
    }

/// Application Interface entity
type ApplicationInterface =
    {
        Id: string
        Name: string
        Protocol: string
        Endpoint: string option
        SpecificationUrl: string option
        Version: string option
        AuthenticationMethod: string option
        ExposedByAppId: string
        ServesServiceIds: string list
        RateLimits: Map<string, string> option
        Status: InterfaceStatus
        Tags: string list
        CreatedAt: UtcTimestamp
        UpdatedAt: UtcTimestamp
    }

/// Server entity
type Server =
    {
        Id: string
        Hostname: string
        Environment: string option
        Region: string option
        Platform: string option
        Criticality: string option
        OwningTeam: string option
        Tags: string list
        CreatedAt: UtcTimestamp
        UpdatedAt: UtcTimestamp
    }

/// Integration entity
type Integration =
    {
        Id: string
        SourceAppId: string
        TargetAppId: string
        Protocol: string option
        DataContract: string option
        Sla: string option
        Frequency: string option
        Tags: string list
        CreatedAt: UtcTimestamp
        UpdatedAt: UtcTimestamp
    }

/// Business Capability entity
type BusinessCapability =
    {
        Id: string
        Name: string
        ParentId: string option
        CreatedAt: UtcTimestamp
        UpdatedAt: UtcTimestamp
    }

/// Data Entity
type DataEntity =
    {
        Id: string
        Name: string
        Domain: string option
        Classification: DataClassification
        Retention: string option
        Owner: string option
        Steward: string option
        SourceSystem: string option
        Criticality: string option
        PiiFlag: bool
        GlossaryTerms: string list
        Lineage: string list
        CreatedAt: UtcTimestamp
        UpdatedAt: UtcTimestamp
    }

/// Relation between entities
type Relation =
    {
        Id: string
        SourceId: string
        TargetId: string
        SourceType: EntityType
        TargetType: EntityType
        RelationType: RelationType
        ArchiMateElement: string option
        ArchiMateRelationship: ArchiMateRelationship option
        Description: string option
        DataClassification: string option
        Criticality: string option
        Confidence: float option
        EvidenceSource: string option
        LastVerifiedAt: UtcTimestamp option
        EffectiveFrom: UtcTimestamp option
        EffectiveTo: UtcTimestamp option
        Label: string option
        Color: string option
        Style: string option
        Bidirectional: bool
        CreatedAt: UtcTimestamp
        UpdatedAt: UtcTimestamp
    }

/// View definition
type View =
    {
        Id: string
        Name: string
        Description: string option
        Filter: Map<string, obj> option
        Layout: Map<string, obj> option
        CreatedAt: UtcTimestamp
        UpdatedAt: UtcTimestamp
    }

/// Import Job
type ImportJob =
    {
        Id: string
        Status: JobStatus
        InputType: string
        CreatedBy: string option
        CreatedAt: UtcTimestamp
        UpdatedAt: UtcTimestamp
        Error: string option
    }

/// Export Job
type ExportJob =
    {
        Id: string
        Status: JobStatus
        Filter: Map<string, obj> option
        CreatedBy: string option
        CreatedAt: UtcTimestamp
        UpdatedAt: UtcTimestamp
        DownloadUrl: string option
        Error: string option
    }

/// Webhook registration
type Webhook =
    {
        Id: string
        Url: string
        Secret: string
        Active: bool
        Events: string list
        CreatedAt: UtcTimestamp
        UpdatedAt: UtcTimestamp
        LastFailureAt: UtcTimestamp option
    }

/// Paginated response wrapper
type PaginatedResponse<'T> =
    {
        Items: 'T list
        Page: int
        Limit: int
        Total: int
    }

/// Create request types (for POST operations)
type CreateOrganizationRequest =
    {
        [<JsonPropertyName("name")>]
        Name: string
        [<JsonPropertyName("domains")>]
        Domains: string list
        [<JsonPropertyName("contacts")>]
        Contacts: string list
    }

type CreateApplicationRequest =
    {
        Name: string
        Owner: string option
        Lifecycle: Lifecycle
        LifecycleRaw: string
        CapabilityId: string option
        DataClassification: string option
        Tags: string list option
    }

type CreateApplicationServiceRequest =
    {
        Name: string
        Description: string option
        BusinessCapabilityId: string option
        Sla: string option
        ExposedByAppIds: string list option
        Tags: string list option
    }

type CreateApplicationInterfaceRequest =
    {
        Name: string
        Protocol: string
        Endpoint: string option
        SpecificationUrl: string option
        Version: string option
        AuthenticationMethod: string option
        ExposedByAppId: string
        ServesServiceIds: string list option
        Status: InterfaceStatus
        Tags: string list option
    }

type CreateDataEntityRequest =
    {
        Name: string
        Domain: string option
        Classification: DataClassification
        Retention: string option
        Owner: string option
        Steward: string option
        SourceSystem: string option
        Criticality: string option
        PiiFlag: bool option
        GlossaryTerms: string list option
        Lineage: string list option
    }

type CreateRelationRequest =
    {
        SourceId: string
        TargetId: string
        SourceType: EntityType
        TargetType: EntityType
        RelationType: RelationType
        ArchiMateElement: string option
        ArchiMateRelationship: ArchiMateRelationship option
        Description: string option
        DataClassification: string option
        Criticality: string option
        Confidence: float option
        EvidenceSource: string option
        LastVerifiedAt: UtcTimestamp option
        EffectiveFrom: UtcTimestamp option
        EffectiveTo: UtcTimestamp option
        Label: string option
        Color: string option
        Style: string option
        Bidirectional: bool option
    }
