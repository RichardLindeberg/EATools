/// JSON serialization/deserialization using Thoth.Json.Net
namespace EATool.Infrastructure

open System
open Thoth.Json.Net
open EATool.Domain

module Json =
    // Decoders
    let decodeCreateOrganizationRequest: Decoder<CreateOrganizationRequest> =
        Decode.object (fun get ->
            {
                Name = get.Required.Field "name" Decode.string
                ParentId = get.Optional.Field "parent_id" Decode.string
                Domains = get.Optional.Field "domains" (Decode.list Decode.string) |> Option.defaultValue []
                Contacts = get.Optional.Field "contacts" (Decode.list Decode.string) |> Option.defaultValue []
            })

    let private decodeLifecycle: Decoder<Lifecycle> =
        Decode.string
        |> Decode.andThen (fun s ->
            match s.ToLowerInvariant() with
            | "planned" -> Decode.succeed Lifecycle.Planned
            | "active" -> Decode.succeed Lifecycle.Active
            | "deprecated" -> Decode.succeed Lifecycle.Deprecated
            | "sunset" -> Decode.succeed Lifecycle.Deprecated
            | "retired" -> Decode.succeed Lifecycle.Retired
            | other -> Decode.fail ($"Invalid lifecycle: {other}"))

    let decodeCreateApplicationRequest: Decoder<CreateApplicationRequest> =
        Decode.object (fun get ->
            {
                Name = get.Required.Field "name" Decode.string
                Owner = get.Optional.Field "owner" Decode.string
                Lifecycle = get.Required.Field "lifecycle" decodeLifecycle
                CapabilityId = get.Optional.Field "capability_id" Decode.string
                DataClassification = get.Optional.Field "data_classification" Decode.string
                Tags = get.Optional.Field "tags" (Decode.list Decode.string)
            })

    let decodeCreateServerRequest: Decoder<CreateServerRequest> =
        Decode.object (fun get ->
            {
                Hostname = get.Required.Field "hostname" Decode.string
                Environment = get.Optional.Field "environment" Decode.string
                Region = get.Optional.Field "region" Decode.string
                Platform = get.Optional.Field "platform" Decode.string
                Criticality = get.Optional.Field "criticality" Decode.string
                OwningTeam = get.Optional.Field "owning_team" Decode.string
                Tags = get.Optional.Field "tags" (Decode.list Decode.string)
            })

    let decodeCreateIntegrationRequest: Decoder<CreateIntegrationRequest> =
        Decode.object (fun get ->
            {
                SourceAppId = get.Required.Field "source_app_id" Decode.string
                TargetAppId = get.Required.Field "target_app_id" Decode.string
                Protocol = get.Optional.Field "protocol" Decode.string
                DataContract = get.Optional.Field "data_contract" Decode.string
                Sla = get.Optional.Field "sla" Decode.string
                Frequency = get.Optional.Field "frequency" Decode.string
                Tags = get.Optional.Field "tags" (Decode.list Decode.string)
            })

    let decodeCreateBusinessCapabilityRequest: Decoder<CreateBusinessCapabilityRequest> =
        Decode.object (fun get ->
            {
                Name = get.Required.Field "name" Decode.string
                ParentId = get.Optional.Field "parent_id" Decode.string
                Description = get.Optional.Field "description" Decode.string
            })

    let private decodeDataClassification: Decoder<DataClassification> =
        Decode.string
        |> Decode.andThen (fun s ->
            match s.ToLowerInvariant() with
            | "public" -> Decode.succeed DataClassification.Public
            | "internal" -> Decode.succeed DataClassification.Internal
            | "confidential" -> Decode.succeed DataClassification.Confidential
            | "restricted" -> Decode.succeed DataClassification.Restricted
            | other -> Decode.fail ($"Invalid data classification: {other}"))

    let decodeCreateDataEntityRequest: Decoder<CreateDataEntityRequest> =
        Decode.object (fun get ->
            {
                Name = get.Required.Field "name" Decode.string
                Domain = get.Optional.Field "domain" Decode.string
                Classification = get.Required.Field "classification" decodeDataClassification
                Retention = get.Optional.Field "retention" Decode.string
                Owner = get.Optional.Field "owner" Decode.string
                Steward = get.Optional.Field "steward" Decode.string
                SourceSystem = get.Optional.Field "source_system" Decode.string
                Criticality = get.Optional.Field "criticality" Decode.string
                PiiFlag = get.Optional.Field "pii_flag" Decode.bool
                GlossaryTerms = get.Optional.Field "glossary_terms" (Decode.list Decode.string)
                Lineage = get.Optional.Field "lineage" (Decode.list Decode.string)
            })

    let private decodeEntityType: Decoder<EntityType> =
        Decode.string
        |> Decode.andThen (fun s ->
            match s.ToLowerInvariant() with
            | "organization" -> Decode.succeed EntityType.Organization
            | "application" -> Decode.succeed EntityType.Application
            | "application_service" -> Decode.succeed EntityType.ApplicationService
            | "application_interface" -> Decode.succeed EntityType.ApplicationInterface
            | "server" -> Decode.succeed EntityType.Server
            | "integration" -> Decode.succeed EntityType.Integration
            | "business_capability" -> Decode.succeed EntityType.BusinessCapability
            | "data_entity" -> Decode.succeed EntityType.DataEntity
            | "view" -> Decode.succeed EntityType.View
            | other -> Decode.fail ($"Invalid entity type: {other}"))

    let private decodeRelationType: Decoder<RelationType> =
        Decode.string
        |> Decode.andThen (fun s ->
            match s.ToLowerInvariant() with
            | "depends_on" -> Decode.succeed RelationType.DependsOn
            | "communicates_with" -> Decode.succeed RelationType.CommunicatesWith
            | "calls" -> Decode.succeed RelationType.Calls
            | "publishes_event_to" -> Decode.succeed RelationType.PublishesEventTo
            | "consumes_event_from" -> Decode.succeed RelationType.ConsumesEventFrom
            | "deployed_on" -> Decode.succeed RelationType.DeployedOn
            | "stores_data_on" -> Decode.succeed RelationType.StoresDataOn
            | "reads" -> Decode.succeed RelationType.Reads
            | "writes" -> Decode.succeed RelationType.Writes
            | "owns" -> Decode.succeed RelationType.Owns
            | "supports" -> Decode.succeed RelationType.Supports
            | "implements" -> Decode.succeed RelationType.Implements
            | "realizes" -> Decode.succeed RelationType.Realizes
            | "serves" -> Decode.succeed RelationType.Serves
            | "connected_to" -> Decode.succeed RelationType.ConnectedTo
            | "exposes" -> Decode.succeed RelationType.Exposes
            | "uses" -> Decode.succeed RelationType.Uses
            | other -> Decode.fail ($"Invalid relation type: {other}"))

    let private decodeArchiMateRelationship: Decoder<ArchiMateRelationship> =
        Decode.string
        |> Decode.andThen (fun s ->
            match s.ToLowerInvariant() with
            | "assignment" -> Decode.succeed ArchiMateRelationship.Assignment
            | "realization" -> Decode.succeed ArchiMateRelationship.Realization
            | "serving" -> Decode.succeed ArchiMateRelationship.Serving
            | "access" -> Decode.succeed ArchiMateRelationship.Access
            | "flow" -> Decode.succeed ArchiMateRelationship.Flow
            | "triggering" -> Decode.succeed ArchiMateRelationship.Triggering
            | "association" -> Decode.succeed ArchiMateRelationship.Association
            | "composition" -> Decode.succeed ArchiMateRelationship.Composition
            | "aggregation" -> Decode.succeed ArchiMateRelationship.Aggregation
            | "specialization" -> Decode.succeed ArchiMateRelationship.Specialization
            | "influence" -> Decode.succeed ArchiMateRelationship.Influence
            | other -> Decode.fail ($"Invalid ArchiMate relationship: {other}"))

    let decodeCreateRelationRequest: Decoder<CreateRelationRequest> =
        Decode.object (fun get ->
            {
                SourceId = get.Required.Field "source_id" Decode.string
                TargetId = get.Required.Field "target_id" Decode.string
                SourceType = get.Required.Field "source_type" decodeEntityType
                TargetType = get.Required.Field "target_type" decodeEntityType
                RelationType = get.Required.Field "relation_type" decodeRelationType
                ArchiMateElement = get.Optional.Field "archimate_element" Decode.string
                ArchiMateRelationship = get.Optional.Field "archimate_relationship" decodeArchiMateRelationship
                Description = get.Optional.Field "description" Decode.string
                DataClassification = get.Optional.Field "data_classification" Decode.string
                Criticality = get.Optional.Field "criticality" Decode.string
                Confidence = get.Optional.Field "confidence" Decode.float
                EvidenceSource = get.Optional.Field "evidence_source" Decode.string
                LastVerifiedAt = get.Optional.Field "last_verified_at" Decode.string
                EffectiveFrom = get.Optional.Field "effective_from" Decode.string
                EffectiveTo = get.Optional.Field "effective_to" Decode.string
                Label = get.Optional.Field "label" Decode.string
                Color = get.Optional.Field "color" Decode.string
                Style = get.Optional.Field "style" Decode.string
                Bidirectional = get.Optional.Field "bidirectional" Decode.bool
            })
    
    // Encoders
    let encodeOrganization (org: Organization): JsonValue =
        Encode.object [
            "id", Encode.string org.Id
            "name", Encode.string org.Name
            "parent_id", (match org.ParentId with Some id -> Encode.string id | None -> Encode.nil)
            "domains", Encode.list (List.map Encode.string org.Domains)
            "contacts", Encode.list (List.map Encode.string org.Contacts)
            "created_at", Encode.string org.CreatedAt
            "updated_at", Encode.string org.UpdatedAt
        ]

    let encodeLifecycle (lc: Lifecycle) : JsonValue =
        let s =
            match lc with
            | Lifecycle.Planned -> "planned"
            | Lifecycle.Active -> "active"
            | Lifecycle.Deprecated -> "deprecated"
            | Lifecycle.Retired -> "retired"
        Encode.string s

    let encodeApplication (app: Application): JsonValue =
        Encode.object [
            "id", Encode.string app.Id
            "name", Encode.string app.Name
            "owner", (match app.Owner with | Some o -> Encode.string o | None -> Encode.nil)
            "lifecycle", encodeLifecycle app.Lifecycle
            "capability_id", (match app.CapabilityId with | Some v -> Encode.string v | None -> Encode.nil)
            "data_classification", (match app.DataClassification with | Some v -> Encode.string v | None -> Encode.nil)
            "tags", Encode.list (List.map Encode.string app.Tags)
            "created_at", Encode.string app.CreatedAt
            "updated_at", Encode.string app.UpdatedAt
        ]

    let encodeServer (srv: Server): JsonValue =
        Encode.object [
            "id", Encode.string srv.Id
            "hostname", Encode.string srv.Hostname
            "environment", (match srv.Environment with | Some v -> Encode.string v | None -> Encode.nil)
            "region", (match srv.Region with | Some v -> Encode.string v | None -> Encode.nil)
            "platform", (match srv.Platform with | Some v -> Encode.string v | None -> Encode.nil)
            "criticality", (match srv.Criticality with | Some v -> Encode.string v | None -> Encode.nil)
            "owning_team", (match srv.OwningTeam with | Some v -> Encode.string v | None -> Encode.nil)
            "tags", Encode.list (List.map Encode.string srv.Tags)
            "created_at", Encode.string srv.CreatedAt
            "updated_at", Encode.string srv.UpdatedAt
        ]

    let encodeIntegration (i: Integration): JsonValue =
        Encode.object [
            "id", Encode.string i.Id
            "source_app_id", Encode.string i.SourceAppId
            "target_app_id", Encode.string i.TargetAppId
            "protocol", (match i.Protocol with | Some v -> Encode.string v | None -> Encode.nil)
            "data_contract", (match i.DataContract with | Some v -> Encode.string v | None -> Encode.nil)
            "sla", (match i.Sla with | Some v -> Encode.string v | None -> Encode.nil)
            "frequency", (match i.Frequency with | Some v -> Encode.string v | None -> Encode.nil)
            "tags", Encode.list (List.map Encode.string i.Tags)
            "created_at", Encode.string i.CreatedAt
            "updated_at", Encode.string i.UpdatedAt
        ]

    let encodeBusinessCapability (cap: BusinessCapability): JsonValue =
        Encode.object [
            "id", Encode.string cap.Id
            "name", Encode.string cap.Name
            "parent_id", (match cap.ParentId with | Some v -> Encode.string v | None -> Encode.nil)
            "created_at", Encode.string cap.CreatedAt
            "updated_at", Encode.string cap.UpdatedAt
        ]

    let encodeDataEntity (entity: DataEntity): JsonValue =
        Encode.object [
            "id", Encode.string entity.Id
            "name", Encode.string entity.Name
            "domain", (match entity.Domain with | Some v -> Encode.string v | None -> Encode.nil)
            "classification",
                (match entity.Classification with
                 | DataClassification.Public -> Encode.string "public"
                 | DataClassification.Internal -> Encode.string "internal"
                 | DataClassification.Confidential -> Encode.string "confidential"
                 | DataClassification.Restricted -> Encode.string "restricted")
            "retention", (match entity.Retention with | Some v -> Encode.string v | None -> Encode.nil)
            "owner", (match entity.Owner with | Some v -> Encode.string v | None -> Encode.nil)
            "steward", (match entity.Steward with | Some v -> Encode.string v | None -> Encode.nil)
            "source_system", (match entity.SourceSystem with | Some v -> Encode.string v | None -> Encode.nil)
            "criticality", (match entity.Criticality with | Some v -> Encode.string v | None -> Encode.nil)
            "pii_flag", Encode.bool entity.PiiFlag
            "glossary_terms", Encode.list (List.map Encode.string entity.GlossaryTerms)
            "lineage", Encode.list (List.map Encode.string entity.Lineage)
            "created_at", Encode.string entity.CreatedAt
            "updated_at", Encode.string entity.UpdatedAt
        ]

    let private encodeEntityType (et: EntityType): JsonValue =
        let s =
            match et with
            | EntityType.Organization -> "organization"
            | EntityType.Application -> "application"
            | EntityType.ApplicationService -> "application_service"
            | EntityType.ApplicationInterface -> "application_interface"
            | EntityType.Server -> "server"
            | EntityType.Integration -> "integration"
            | EntityType.BusinessCapability -> "business_capability"
            | EntityType.DataEntity -> "data_entity"
            | EntityType.View -> "view"
        Encode.string s

    let private encodeRelationType (rt: RelationType): JsonValue =
        let s =
            match rt with
            | RelationType.DependsOn -> "depends_on"
            | RelationType.CommunicatesWith -> "communicates_with"
            | RelationType.Calls -> "calls"
            | RelationType.PublishesEventTo -> "publishes_event_to"
            | RelationType.ConsumesEventFrom -> "consumes_event_from"
            | RelationType.DeployedOn -> "deployed_on"
            | RelationType.StoresDataOn -> "stores_data_on"
            | RelationType.Reads -> "reads"
            | RelationType.Writes -> "writes"
            | RelationType.Owns -> "owns"
            | RelationType.Supports -> "supports"
            | RelationType.Implements -> "implements"
            | RelationType.Realizes -> "realizes"
            | RelationType.Serves -> "serves"
            | RelationType.ConnectedTo -> "connected_to"
            | RelationType.Exposes -> "exposes"
            | RelationType.Uses -> "uses"
        Encode.string s

    let private encodeArchiMateRelationship (rel: ArchiMateRelationship): JsonValue =
        let s =
            match rel with
            | ArchiMateRelationship.Assignment -> "assignment"
            | ArchiMateRelationship.Realization -> "realization"
            | ArchiMateRelationship.Serving -> "serving"
            | ArchiMateRelationship.Access -> "access"
            | ArchiMateRelationship.Flow -> "flow"
            | ArchiMateRelationship.Triggering -> "triggering"
            | ArchiMateRelationship.Association -> "association"
            | ArchiMateRelationship.Composition -> "composition"
            | ArchiMateRelationship.Aggregation -> "aggregation"
            | ArchiMateRelationship.Specialization -> "specialization"
            | ArchiMateRelationship.Influence -> "influence"
        Encode.string s

    let encodeRelation (rel: Relation): JsonValue =
        Encode.object [
            "id", Encode.string rel.Id
            "source_id", Encode.string rel.SourceId
            "target_id", Encode.string rel.TargetId
            "source_type", encodeEntityType rel.SourceType
            "target_type", encodeEntityType rel.TargetType
            "relation_type", encodeRelationType rel.RelationType
            "archimate_element", (match rel.ArchiMateElement with | Some v -> Encode.string v | None -> Encode.nil)
            "archimate_relationship", (match rel.ArchiMateRelationship with | Some v -> encodeArchiMateRelationship v | None -> Encode.nil)
            "description", (match rel.Description with | Some v -> Encode.string v | None -> Encode.nil)
            "data_classification", (match rel.DataClassification with | Some v -> Encode.string v | None -> Encode.nil)
            "criticality", (match rel.Criticality with | Some v -> Encode.string v | None -> Encode.nil)
            "confidence", (match rel.Confidence with | Some v -> Encode.float v | None -> Encode.nil)
            "evidence_source", (match rel.EvidenceSource with | Some v -> Encode.string v | None -> Encode.nil)
            "last_verified_at", (match rel.LastVerifiedAt with | Some v -> Encode.string v | None -> Encode.nil)
            "effective_from", (match rel.EffectiveFrom with | Some v -> Encode.string v | None -> Encode.nil)
            "effective_to", (match rel.EffectiveTo with | Some v -> Encode.string v | None -> Encode.nil)
            "label", (match rel.Label with | Some v -> Encode.string v | None -> Encode.nil)
            "color", (match rel.Color with | Some v -> Encode.string v | None -> Encode.nil)
            "style", (match rel.Style with | Some v -> Encode.string v | None -> Encode.nil)
            "bidirectional", Encode.bool rel.Bidirectional
            "created_at", Encode.string rel.CreatedAt
            "updated_at", Encode.string rel.UpdatedAt
        ]
    
    let encodePaginatedResponse<'T> (encoder: 'T -> JsonValue) (response: PaginatedResponse<'T>): JsonValue =
        Encode.object [
            "items", Encode.list (List.map encoder response.Items)
            "page", Encode.int response.Page
            "limit", Encode.int response.Limit
            "total", Encode.int response.Total
        ]
    
    let encodeErrorResponse (code: string) (message: string): JsonValue =
        Encode.object [
            "code", Encode.string code
            "message", Encode.string message
        ]
