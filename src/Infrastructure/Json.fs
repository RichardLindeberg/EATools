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
