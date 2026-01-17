/// JSON encoding/decoding for IntegrationEvent
namespace EATool.Infrastructure

open Thoth.Json.Net
open EATool.Domain

module IntegrationEventJson =
    // Encoders for event data
    let encodeIntegrationCreatedData (data: IntegrationCreatedData) =
        Encode.object [
            "id", Encode.string data.Id
            "source_app_id", Encode.string data.SourceAppId
            "target_app_id", Encode.string data.TargetAppId
            "protocol", Encode.string data.Protocol
            "data_contract", Encode.option Encode.string data.DataContract
            "sla", Encode.option Encode.string data.Sla
            "frequency", Encode.option Encode.string data.Frequency
            "tags", Encode.list (List.map Encode.string data.Tags)
        ]

    let encodeProtocolUpdatedData (data: ProtocolUpdatedData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_protocol", Encode.string data.OldProtocol
            "new_protocol", Encode.string data.NewProtocol
        ]

    let encodeSLASetData (data: SLASetData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_sla", Encode.option Encode.string data.OldSla
            "new_sla", Encode.option Encode.string data.NewSla
        ]

    let encodeFrequencySetData (data: FrequencySetData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_frequency", Encode.option Encode.string data.OldFrequency
            "new_frequency", Encode.option Encode.string data.NewFrequency
        ]

    let encodeDataContractUpdatedData (data: DataContractUpdatedData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_data_contract", Encode.option Encode.string data.OldDataContract
            "new_data_contract", Encode.option Encode.string data.NewDataContract
        ]

    let encodeSourceAppSetData (data: SourceAppSetData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_source_app_id", Encode.string data.OldSourceAppId
            "new_source_app_id", Encode.string data.NewSourceAppId
        ]

    let encodeTargetAppSetData (data: TargetAppSetData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_target_app_id", Encode.string data.OldTargetAppId
            "new_target_app_id", Encode.string data.NewTargetAppId
        ]

    let encodeIntegrationTagsAddedData (data: IntegrationTagsAddedData) =
        Encode.object [
            "id", Encode.string data.Id
            "added_tags", Encode.list (List.map Encode.string data.AddedTags)
        ]

    let encodeIntegrationTagsRemovedData (data: IntegrationTagsRemovedData) =
        Encode.object [
            "id", Encode.string data.Id
            "removed_tags", Encode.list (List.map Encode.string data.RemovedTags)
        ]

    let encodeIntegrationDeletedData (data: IntegrationDeletedData) =
        Encode.object [
            "id", Encode.string data.Id
        ]

    // Encoder for IntegrationEvent discriminated union
    let encodeIntegrationEvent (event: IntegrationEvent) : JsonValue =
        match event with
        | IntegrationCreated data ->
            Encode.object [
                "type", Encode.string "IntegrationCreated"
                "data", encodeIntegrationCreatedData data
            ]
        | ProtocolUpdated data ->
            Encode.object [
                "type", Encode.string "ProtocolUpdated"
                "data", encodeProtocolUpdatedData data
            ]
        | SLASet data ->
            Encode.object [
                "type", Encode.string "SLASet"
                "data", encodeSLASetData data
            ]
        | FrequencySet data ->
            Encode.object [
                "type", Encode.string "FrequencySet"
                "data", encodeFrequencySetData data
            ]
        | DataContractUpdated data ->
            Encode.object [
                "type", Encode.string "DataContractUpdated"
                "data", encodeDataContractUpdatedData data
            ]
        | SourceAppSet data ->
            Encode.object [
                "type", Encode.string "SourceAppSet"
                "data", encodeSourceAppSetData data
            ]
        | TargetAppSet data ->
            Encode.object [
                "type", Encode.string "TargetAppSet"
                "data", encodeTargetAppSetData data
            ]
        | IntegrationTagsAdded data ->
            Encode.object [
                "type", Encode.string "IntegrationTagsAdded"
                "data", encodeIntegrationTagsAddedData data
            ]
        | IntegrationTagsRemoved data ->
            Encode.object [
                "type", Encode.string "IntegrationTagsRemoved"
                "data", encodeIntegrationTagsRemovedData data
            ]
        | IntegrationDeleted data ->
            Encode.object [
                "type", Encode.string "IntegrationDeleted"
                "data", encodeIntegrationDeletedData data
            ]

    // Decoders for event data
    let decodeIntegrationCreatedData : Decoder<IntegrationCreatedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            SourceAppId = get.Required.Field "source_app_id" Decode.string
            TargetAppId = get.Required.Field "target_app_id" Decode.string
            Protocol = get.Required.Field "protocol" Decode.string
            DataContract = get.Optional.Field "data_contract" Decode.string
            Sla = get.Optional.Field "sla" Decode.string
            Frequency = get.Optional.Field "frequency" Decode.string
            Tags = get.Required.Field "tags" (Decode.list Decode.string)
        })

    let decodeProtocolUpdatedData : Decoder<ProtocolUpdatedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            OldProtocol = get.Required.Field "old_protocol" Decode.string
            NewProtocol = get.Required.Field "new_protocol" Decode.string
        })

    let decodeSLASetData : Decoder<SLASetData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            OldSla = get.Optional.Field "old_sla" Decode.string
            NewSla = get.Optional.Field "new_sla" Decode.string
        })

    let decodeFrequencySetData : Decoder<FrequencySetData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            OldFrequency = get.Optional.Field "old_frequency" Decode.string
            NewFrequency = get.Optional.Field "new_frequency" Decode.string
        })

    let decodeDataContractUpdatedData : Decoder<DataContractUpdatedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            OldDataContract = get.Optional.Field "old_data_contract" Decode.string
            NewDataContract = get.Optional.Field "new_data_contract" Decode.string
        })

    let decodeSourceAppSetData : Decoder<SourceAppSetData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            OldSourceAppId = get.Required.Field "old_source_app_id" Decode.string
            NewSourceAppId = get.Required.Field "new_source_app_id" Decode.string
        })

    let decodeTargetAppSetData : Decoder<TargetAppSetData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            OldTargetAppId = get.Required.Field "old_target_app_id" Decode.string
            NewTargetAppId = get.Required.Field "new_target_app_id" Decode.string
        })

    let decodeIntegrationTagsAddedData : Decoder<IntegrationTagsAddedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            AddedTags = get.Required.Field "added_tags" (Decode.list Decode.string)
        })

    let decodeIntegrationTagsRemovedData : Decoder<IntegrationTagsRemovedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            RemovedTags = get.Required.Field "removed_tags" (Decode.list Decode.string)
        })

    let decodeIntegrationDeletedData : Decoder<IntegrationDeletedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
        })

    // Decoder for IntegrationEvent discriminated union
    let decodeIntegrationEvent : Decoder<IntegrationEvent> =
        Decode.field "type" Decode.string
        |> Decode.andThen (fun eventType ->
            match eventType with
            | "IntegrationCreated" ->
                Decode.field "data" decodeIntegrationCreatedData
                |> Decode.map IntegrationCreated
            | "ProtocolUpdated" ->
                Decode.field "data" decodeProtocolUpdatedData
                |> Decode.map ProtocolUpdated
            | "SLASet" ->
                Decode.field "data" decodeSLASetData
                |> Decode.map SLASet
            | "FrequencySet" ->
                Decode.field "data" decodeFrequencySetData
                |> Decode.map FrequencySet
            | "DataContractUpdated" ->
                Decode.field "data" decodeDataContractUpdatedData
                |> Decode.map DataContractUpdated
            | "SourceAppSet" ->
                Decode.field "data" decodeSourceAppSetData
                |> Decode.map SourceAppSet
            | "TargetAppSet" ->
                Decode.field "data" decodeTargetAppSetData
                |> Decode.map TargetAppSet
            | "IntegrationTagsAdded" ->
                Decode.field "data" decodeIntegrationTagsAddedData
                |> Decode.map IntegrationTagsAdded
            | "IntegrationTagsRemoved" ->
                Decode.field "data" decodeIntegrationTagsRemovedData
                |> Decode.map IntegrationTagsRemoved
            | "IntegrationDeleted" ->
                Decode.field "data" decodeIntegrationDeletedData
                |> Decode.map IntegrationDeleted
            | other ->
                Decode.fail $"Unknown event type: {other}"
        )
