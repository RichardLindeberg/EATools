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
            let lifecycleRaw = get.Required.Field "lifecycle" Decode.string
            {
                Name = get.Required.Field "name" Decode.string
                Owner = get.Optional.Field "owner" Decode.string
                Lifecycle = get.Required.Field "lifecycle" decodeLifecycle
                LifecycleRaw = lifecycleRaw
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
    
    // Encoders
    let encodeOrganization (org: Organization): JsonValue =
        Encode.object [
            "id", Encode.string org.Id
            "name", Encode.string org.Name
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
        let lifecycleValue =
            if String.IsNullOrWhiteSpace app.LifecycleRaw then encodeLifecycle app.Lifecycle else Encode.string app.LifecycleRaw

        Encode.object [
            "id", Encode.string app.Id
            "name", Encode.string app.Name
            "owner", (match app.Owner with | Some o -> Encode.string o | None -> Encode.nil)
            "lifecycle", lifecycleValue
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
