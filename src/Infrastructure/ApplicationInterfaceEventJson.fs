namespace EATool.Infrastructure

open Thoth.Json.Net
open EATool.Domain

module ApplicationInterfaceEventJson =
    let private encodeStatus (status: InterfaceStatus) : JsonValue =
        let s =
            match status with
            | InterfaceStatus.Active -> "active"
            | InterfaceStatus.Deprecated -> "deprecated"
            | InterfaceStatus.Retired -> "retired"
        Encode.string s

    let private encodeRateLimits (rates: Map<string, string>) : JsonValue =
        rates
        |> Map.toList
        |> List.map (fun (k, v) -> k, Encode.string v)
        |> Encode.object

    let private decodeStatus : Decoder<InterfaceStatus> =
        Decode.string
        |> Decode.andThen (fun s ->
            match s.ToLowerInvariant() with
            | "active" -> Decode.succeed InterfaceStatus.Active
            | "deprecated" -> Decode.succeed InterfaceStatus.Deprecated
            | "retired" -> Decode.succeed InterfaceStatus.Retired
            | other -> Decode.fail ($"Invalid interface status: {other}"))

    // Encoders
    let encodeApplicationInterfaceCreatedData (data: ApplicationInterfaceCreatedData) : JsonValue =
        Encode.object [
            "id", Encode.string data.Id
            "name", Encode.string data.Name
            "protocol", Encode.string data.Protocol
            "endpoint", (match data.Endpoint with Some v -> Encode.string v | None -> Encode.nil)
            "specification_url", (match data.SpecificationUrl with Some v -> Encode.string v | None -> Encode.nil)
            "version", (match data.Version with Some v -> Encode.string v | None -> Encode.nil)
            "authentication_method", (match data.AuthenticationMethod with Some v -> Encode.string v | None -> Encode.nil)
            "exposed_by_app_id", Encode.string data.ExposedByAppId
            "serves_service_ids", Encode.list (List.map Encode.string data.ServesServiceIds)
            "rate_limits", (match data.RateLimits with Some m -> encodeRateLimits m | None -> Encode.nil)
            "status", encodeStatus data.Status
            "tags", Encode.list (List.map Encode.string data.Tags)
        ]

    let encodeApplicationInterfaceUpdatedData (data: ApplicationInterfaceUpdatedData) : JsonValue =
        Encode.object [
            "id", Encode.string data.Id
            "name", (match data.Name with Some v -> Encode.string v | None -> Encode.nil)
            "protocol", (match data.Protocol with Some v -> Encode.string v | None -> Encode.nil)
            "endpoint", (match data.Endpoint with Some v -> Encode.string v | None -> Encode.nil)
            "version", (match data.Version with Some v -> Encode.string v | None -> Encode.nil)
            "authentication_method", (match data.AuthenticationMethod with Some v -> Encode.string v | None -> Encode.nil)
            "tags", (match data.Tags with Some xs -> Encode.list (List.map Encode.string xs) | None -> Encode.nil)
        ]

    let encodeServedServicesSetData (data: ServedServicesSetData) : JsonValue =
        Encode.object [
            "id", Encode.string data.Id
            "service_ids", Encode.list (List.map Encode.string data.ServiceIds)
        ]

    let encodeStatusChangedData (data: StatusChangedData) : JsonValue =
        Encode.object [
            "id", Encode.string data.Id
            "status", encodeStatus data.Status
        ]

    let encodeApplicationInterfaceDeletedData (data: ApplicationInterfaceDeletedData) : JsonValue =
        Encode.object [
            "id", Encode.string data.Id
        ]

    let encodeApplicationInterfaceEvent (event: ApplicationInterfaceEvent) : JsonValue =
        match event with
        | ApplicationInterfaceCreated d -> Encode.object [ "type", Encode.string "ApplicationInterfaceCreated"; "data", encodeApplicationInterfaceCreatedData d ]
        | ApplicationInterfaceUpdated d -> Encode.object [ "type", Encode.string "ApplicationInterfaceUpdated"; "data", encodeApplicationInterfaceUpdatedData d ]
        | ServedServicesSet d -> Encode.object [ "type", Encode.string "ServedServicesSet"; "data", encodeServedServicesSetData d ]
        | StatusChanged d -> Encode.object [ "type", Encode.string "StatusChanged"; "data", encodeStatusChangedData d ]
        | ApplicationInterfaceDeleted d -> Encode.object [ "type", Encode.string "ApplicationInterfaceDeleted"; "data", encodeApplicationInterfaceDeletedData d ]

    // Decoders
    let decodeApplicationInterfaceCreatedData : Decoder<ApplicationInterfaceCreatedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                Name = get.Required.Field "name" Decode.string
                Protocol = get.Required.Field "protocol" Decode.string
                Endpoint = get.Optional.Field "endpoint" Decode.string
                SpecificationUrl = get.Optional.Field "specification_url" Decode.string
                Version = get.Optional.Field "version" Decode.string
                AuthenticationMethod = get.Optional.Field "authentication_method" Decode.string
                ExposedByAppId = get.Required.Field "exposed_by_app_id" Decode.string
                ServesServiceIds = get.Optional.Field "serves_service_ids" (Decode.list Decode.string) |> Option.defaultValue []
                RateLimits = get.Optional.Field "rate_limits" (Decode.keyValuePairs Decode.string |> Decode.map Map.ofList)
                Status = get.Required.Field "status" decodeStatus
                Tags = get.Optional.Field "tags" (Decode.list Decode.string) |> Option.defaultValue []
            })

    let decodeApplicationInterfaceUpdatedData : Decoder<ApplicationInterfaceUpdatedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                Name = get.Optional.Field "name" Decode.string
                Protocol = get.Optional.Field "protocol" Decode.string
                Endpoint = get.Optional.Field "endpoint" Decode.string
                Version = get.Optional.Field "version" Decode.string
                AuthenticationMethod = get.Optional.Field "authentication_method" Decode.string
                Tags = get.Optional.Field "tags" (Decode.list Decode.string)
            })

    let decodeServedServicesSetData : Decoder<ServedServicesSetData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                ServiceIds = get.Optional.Field "service_ids" (Decode.list Decode.string) |> Option.defaultValue []
            })

    let decodeStatusChangedData : Decoder<StatusChangedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                Status = get.Required.Field "status" decodeStatus
            })

    let decodeApplicationInterfaceDeletedData : Decoder<ApplicationInterfaceDeletedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
            })

    let decodeApplicationInterfaceEvent : Decoder<ApplicationInterfaceEvent> =
        Decode.field "type" Decode.string
        |> Decode.andThen (fun eventType ->
            match eventType with
            | "ApplicationInterfaceCreated" -> Decode.field "data" decodeApplicationInterfaceCreatedData |> Decode.map ApplicationInterfaceCreated
            | "ApplicationInterfaceUpdated" -> Decode.field "data" decodeApplicationInterfaceUpdatedData |> Decode.map ApplicationInterfaceUpdated
            | "ServedServicesSet" -> Decode.field "data" decodeServedServicesSetData |> Decode.map ServedServicesSet
            | "StatusChanged" -> Decode.field "data" decodeStatusChangedData |> Decode.map StatusChanged
            | "ApplicationInterfaceDeleted" -> Decode.field "data" decodeApplicationInterfaceDeletedData |> Decode.map ApplicationInterfaceDeleted
            | other -> Decode.fail ($"Unknown event type: {other}"))
