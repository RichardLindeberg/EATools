namespace EATool.Infrastructure

open Thoth.Json.Net
open EATool.Domain

module ApplicationServiceEventJson =
    // Encoders
    let encodeApplicationServiceCreatedData (data: ApplicationServiceCreatedData) : JsonValue =
        Encode.object [
            "id", Encode.string data.Id
            "name", Encode.string data.Name
            "description", (match data.Description with Some v -> Encode.string v | None -> Encode.nil)
            "business_capability_id", (match data.BusinessCapabilityId with Some v -> Encode.string v | None -> Encode.nil)
            "sla", (match data.Sla with Some v -> Encode.string v | None -> Encode.nil)
            "exposed_by_app_ids", Encode.list (List.map Encode.string data.ExposedByAppIds)
            "consumers", Encode.list (List.map Encode.string data.Consumers)
            "tags", Encode.list (List.map Encode.string data.Tags)
        ]

    let encodeApplicationServiceUpdatedData (data: ApplicationServiceUpdatedData) : JsonValue =
        Encode.object [
            "id", Encode.string data.Id
            "name", (match data.Name with Some v -> Encode.string v | None -> Encode.nil)
            "description", (match data.Description with Some v -> Encode.string v | None -> Encode.nil)
            "sla", (match data.Sla with Some v -> Encode.string v | None -> Encode.nil)
            "tags", (match data.Tags with Some xs -> Encode.list (List.map Encode.string xs) | None -> Encode.nil)
        ]

    let encodeBusinessCapabilitySetData (data: BusinessCapabilitySetData) : JsonValue =
        Encode.object [
            "id", Encode.string data.Id
            "business_capability_id", (match data.BusinessCapabilityId with Some v -> Encode.string v | None -> Encode.nil)
        ]

    let encodeConsumerAddedData (data: ConsumerAddedData) : JsonValue =
        Encode.object [
            "id", Encode.string data.Id
            "consumer_app_id", Encode.string data.ConsumerAppId
        ]

    let encodeConsumerRemovedData (data: ConsumerRemovedData) : JsonValue =
        Encode.object [
            "id", Encode.string data.Id
            "consumer_app_id", Encode.string data.ConsumerAppId
        ]

    let encodeApplicationServiceDeletedData (data: ApplicationServiceDeletedData) : JsonValue =
        Encode.object [
            "id", Encode.string data.Id
        ]

    let encodeApplicationServiceEvent (event: ApplicationServiceEvent) : JsonValue =
        match event with
        | ApplicationServiceCreated d -> Encode.object [ "type", Encode.string "ApplicationServiceCreated"; "data", encodeApplicationServiceCreatedData d ]
        | ApplicationServiceUpdated d -> Encode.object [ "type", Encode.string "ApplicationServiceUpdated"; "data", encodeApplicationServiceUpdatedData d ]
        | BusinessCapabilitySet d -> Encode.object [ "type", Encode.string "BusinessCapabilitySet"; "data", encodeBusinessCapabilitySetData d ]
        | ConsumerAdded d -> Encode.object [ "type", Encode.string "ConsumerAdded"; "data", encodeConsumerAddedData d ]
        | ConsumerRemoved d -> Encode.object [ "type", Encode.string "ConsumerRemoved"; "data", encodeConsumerRemovedData d ]
        | ApplicationServiceDeleted d -> Encode.object [ "type", Encode.string "ApplicationServiceDeleted"; "data", encodeApplicationServiceDeletedData d ]

    // Decoders
    let decodeApplicationServiceCreatedData : Decoder<ApplicationServiceCreatedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                Name = get.Required.Field "name" Decode.string
                Description = get.Optional.Field "description" Decode.string
                BusinessCapabilityId = get.Optional.Field "business_capability_id" Decode.string
                Sla = get.Optional.Field "sla" Decode.string
                ExposedByAppIds = get.Optional.Field "exposed_by_app_ids" (Decode.list Decode.string) |> Option.defaultValue []
                Consumers = get.Optional.Field "consumers" (Decode.list Decode.string) |> Option.defaultValue []
                Tags = get.Optional.Field "tags" (Decode.list Decode.string) |> Option.defaultValue []
            })

    let decodeApplicationServiceUpdatedData : Decoder<ApplicationServiceUpdatedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                Name = get.Optional.Field "name" Decode.string
                Description = get.Optional.Field "description" Decode.string
                Sla = get.Optional.Field "sla" Decode.string
                Tags = get.Optional.Field "tags" (Decode.list Decode.string)
            })

    let decodeBusinessCapabilitySetData : Decoder<BusinessCapabilitySetData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                BusinessCapabilityId = get.Optional.Field "business_capability_id" Decode.string
            })

    let decodeConsumerAddedData : Decoder<ConsumerAddedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                ConsumerAppId = get.Required.Field "consumer_app_id" Decode.string
            })

    let decodeConsumerRemovedData : Decoder<ConsumerRemovedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                ConsumerAppId = get.Required.Field "consumer_app_id" Decode.string
            })

    let decodeApplicationServiceDeletedData : Decoder<ApplicationServiceDeletedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
            })

    let decodeApplicationServiceEvent : Decoder<ApplicationServiceEvent> =
        Decode.field "type" Decode.string
        |> Decode.andThen (fun eventType ->
            match eventType with
            | "ApplicationServiceCreated" -> Decode.field "data" decodeApplicationServiceCreatedData |> Decode.map ApplicationServiceCreated
            | "ApplicationServiceUpdated" -> Decode.field "data" decodeApplicationServiceUpdatedData |> Decode.map ApplicationServiceUpdated
            | "BusinessCapabilitySet" -> Decode.field "data" decodeBusinessCapabilitySetData |> Decode.map BusinessCapabilitySet
            | "ConsumerAdded" -> Decode.field "data" decodeConsumerAddedData |> Decode.map ConsumerAdded
            | "ConsumerRemoved" -> Decode.field "data" decodeConsumerRemovedData |> Decode.map ConsumerRemoved
            | "ApplicationServiceDeleted" -> Decode.field "data" decodeApplicationServiceDeletedData |> Decode.map ApplicationServiceDeleted
            | other -> Decode.fail ($"Unknown event type: {other}"))
