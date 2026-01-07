/// JSON encoding/decoding for BusinessCapabilityEvent
namespace EATool.Infrastructure

open Thoth.Json.Net
open EATool.Domain

module BusinessCapabilityEventJson =
    // Encoders for event data
    let encodeCapabilityCreatedData (data: CapabilityCreatedData) =
        Encode.object [
            "id", Encode.string data.Id
            "name", Encode.string data.Name
            "parent_id", Encode.option Encode.string data.ParentId
            "description", Encode.option Encode.string data.Description
        ]

    let encodeCapabilityParentAssignedData (data: CapabilityParentAssignedData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_parent_id", Encode.option Encode.string data.OldParentId
            "new_parent_id", Encode.string data.NewParentId
        ]

    let encodeCapabilityParentRemovedData (data: CapabilityParentRemovedData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_parent_id", Encode.string data.OldParentId
        ]

    let encodeCapabilityDescriptionUpdatedData (data: CapabilityDescriptionUpdatedData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_description", Encode.option Encode.string data.OldDescription
            "new_description", Encode.option Encode.string data.NewDescription
        ]

    let encodeCapabilityDeletedData (data: CapabilityDeletedData) =
        Encode.object [
            "id", Encode.string data.Id
        ]

    // Encoder for BusinessCapabilityEvent discriminated union
    let encodeBusinessCapabilityEvent (event: BusinessCapabilityEvent) : JsonValue =
        match event with
        | CapabilityCreated data ->
            Encode.object [
                "type", Encode.string "CapabilityCreated"
                "data", encodeCapabilityCreatedData data
            ]
        | CapabilityParentAssigned data ->
            Encode.object [
                "type", Encode.string "CapabilityParentAssigned"
                "data", encodeCapabilityParentAssignedData data
            ]
        | CapabilityParentRemoved data ->
            Encode.object [
                "type", Encode.string "CapabilityParentRemoved"
                "data", encodeCapabilityParentRemovedData data
            ]
        | CapabilityDescriptionUpdated data ->
            Encode.object [
                "type", Encode.string "CapabilityDescriptionUpdated"
                "data", encodeCapabilityDescriptionUpdatedData data
            ]
        | CapabilityDeleted data ->
            Encode.object [
                "type", Encode.string "CapabilityDeleted"
                "data", encodeCapabilityDeletedData data
            ]

    // Decoders for event data
    let decodeCapabilityCreatedData : Decoder<CapabilityCreatedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            Name = get.Required.Field "name" Decode.string
            ParentId = get.Optional.Field "parent_id" Decode.string
            Description = get.Optional.Field "description" Decode.string
        })

    let decodeCapabilityParentAssignedData : Decoder<CapabilityParentAssignedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            OldParentId = get.Optional.Field "old_parent_id" Decode.string
            NewParentId = get.Required.Field "new_parent_id" Decode.string
        })

    let decodeCapabilityParentRemovedData : Decoder<CapabilityParentRemovedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            OldParentId = get.Required.Field "old_parent_id" Decode.string
        })

    let decodeCapabilityDescriptionUpdatedData : Decoder<CapabilityDescriptionUpdatedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            OldDescription = get.Optional.Field "old_description" Decode.string
            NewDescription = get.Optional.Field "new_description" Decode.string
        })

    let decodeCapabilityDeletedData : Decoder<CapabilityDeletedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
        })

    // Decoder for BusinessCapabilityEvent discriminated union
    let decodeBusinessCapabilityEvent : Decoder<BusinessCapabilityEvent> =
        Decode.field "type" Decode.string
        |> Decode.andThen (fun eventType ->
            match eventType with
            | "CapabilityCreated" ->
                Decode.field "data" decodeCapabilityCreatedData
                |> Decode.map CapabilityCreated
            | "CapabilityParentAssigned" ->
                Decode.field "data" decodeCapabilityParentAssignedData
                |> Decode.map CapabilityParentAssigned
            | "CapabilityParentRemoved" ->
                Decode.field "data" decodeCapabilityParentRemovedData
                |> Decode.map CapabilityParentRemoved
            | "CapabilityDescriptionUpdated" ->
                Decode.field "data" decodeCapabilityDescriptionUpdatedData
                |> Decode.map CapabilityDescriptionUpdated
            | "CapabilityDeleted" ->
                Decode.field "data" decodeCapabilityDeletedData
                |> Decode.map CapabilityDeleted
            | _ ->
                Decode.fail $"Unknown event type: {eventType}"
        )
