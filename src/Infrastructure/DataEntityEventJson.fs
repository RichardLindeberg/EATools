/// JSON encoding/decoding for DataEntityEvent
namespace EATool.Infrastructure

open Thoth.Json.Net
open EATool.Domain

module DataEntityEventJson =
    // Encoders for event data
    let encodeDataEntityCreatedData (data: DataEntityCreatedData) =
        Encode.object [
            "id", Encode.string data.Id
            "name", Encode.string data.Name
            "domain", Encode.option Encode.string data.Domain
            "classification", Encode.string data.Classification
            "retention", Encode.option Encode.string data.Retention
            "owner", Encode.option Encode.string data.Owner
            "steward", Encode.option Encode.string data.Steward
            "source_system", Encode.option Encode.string data.SourceSystem
            "criticality", Encode.option Encode.string data.Criticality
            "pii_flag", Encode.bool data.PiiFlag
            "tags", Encode.list (List.map Encode.string data.Tags)
        ]

    let encodeClassificationSetData (data: ClassificationSetData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_classification", Encode.string data.OldClassification
            "new_classification", Encode.string data.NewClassification
        ]

    let encodePIIFlagSetData (data: PIIFlagSetData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_pii_flag", Encode.bool data.OldPiiFlag
            "new_pii_flag", Encode.bool data.NewPiiFlag
        ]

    let encodeRetentionUpdatedData (data: RetentionUpdatedData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_retention", Encode.option Encode.string data.OldRetention
            "new_retention", Encode.option Encode.string data.NewRetention
        ]

    let encodeDataEntityTagsAddedData (data: DataEntityTagsAddedData) =
        Encode.object [
            "id", Encode.string data.Id
            "added_tags", Encode.list (List.map Encode.string data.AddedTags)
        ]

    let encodeDataEntityDeletedData (data: DataEntityDeletedData) =
        Encode.object [
            "id", Encode.string data.Id
        ]

    // Encoder for DataEntityEvent discriminated union
    let encodeDataEntityEvent (event: DataEntityEvent) : JsonValue =
        match event with
        | DataEntityCreated data ->
            Encode.object [
                "type", Encode.string "DataEntityCreated"
                "data", encodeDataEntityCreatedData data
            ]
        | ClassificationSet data ->
            Encode.object [
                "type", Encode.string "ClassificationSet"
                "data", encodeClassificationSetData data
            ]
        | PIIFlagSet data ->
            Encode.object [
                "type", Encode.string "PIIFlagSet"
                "data", encodePIIFlagSetData data
            ]
        | RetentionUpdated data ->
            Encode.object [
                "type", Encode.string "RetentionUpdated"
                "data", encodeRetentionUpdatedData data
            ]
        | DataEntityTagsAdded data ->
            Encode.object [
                "type", Encode.string "DataEntityTagsAdded"
                "data", encodeDataEntityTagsAddedData data
            ]
        | DataEntityDeleted data ->
            Encode.object [
                "type", Encode.string "DataEntityDeleted"
                "data", encodeDataEntityDeletedData data
            ]

    // Decoders for event data
    let decodeDataEntityCreatedData : Decoder<DataEntityCreatedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            Name = get.Required.Field "name" Decode.string
            Domain = get.Optional.Field "domain" Decode.string
            Classification = get.Required.Field "classification" Decode.string
            Retention = get.Optional.Field "retention" Decode.string
            Owner = get.Optional.Field "owner" Decode.string
            Steward = get.Optional.Field "steward" Decode.string
            SourceSystem = get.Optional.Field "source_system" Decode.string
            Criticality = get.Optional.Field "criticality" Decode.string
            PiiFlag = get.Required.Field "pii_flag" Decode.bool
            Tags = get.Required.Field "tags" (Decode.list Decode.string)
        })

    let decodeClassificationSetData : Decoder<ClassificationSetData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            OldClassification = get.Required.Field "old_classification" Decode.string
            NewClassification = get.Required.Field "new_classification" Decode.string
        })

    let decodePIIFlagSetData : Decoder<PIIFlagSetData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            OldPiiFlag = get.Required.Field "old_pii_flag" Decode.bool
            NewPiiFlag = get.Required.Field "new_pii_flag" Decode.bool
        })

    let decodeRetentionUpdatedData : Decoder<RetentionUpdatedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            OldRetention = get.Optional.Field "old_retention" Decode.string
            NewRetention = get.Optional.Field "new_retention" Decode.string
        })

    let decodeDataEntityTagsAddedData : Decoder<DataEntityTagsAddedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            AddedTags = get.Required.Field "added_tags" (Decode.list Decode.string)
        })

    let decodeDataEntityDeletedData : Decoder<DataEntityDeletedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
        })

    // Decoder for DataEntityEvent discriminated union
    let decodeDataEntityEvent : Decoder<DataEntityEvent> =
        Decode.field "type" Decode.string
        |> Decode.andThen (fun eventType ->
            match eventType with
            | "DataEntityCreated" ->
                Decode.field "data" decodeDataEntityCreatedData
                |> Decode.map DataEntityCreated
            | "ClassificationSet" ->
                Decode.field "data" decodeClassificationSetData
                |> Decode.map ClassificationSet
            | "PIIFlagSet" ->
                Decode.field "data" decodePIIFlagSetData
                |> Decode.map PIIFlagSet
            | "RetentionUpdated" ->
                Decode.field "data" decodeRetentionUpdatedData
                |> Decode.map RetentionUpdated
            | "DataEntityTagsAdded" ->
                Decode.field "data" decodeDataEntityTagsAddedData
                |> Decode.map DataEntityTagsAdded
            | "DataEntityDeleted" ->
                Decode.field "data" decodeDataEntityDeletedData
                |> Decode.map DataEntityDeleted
            | other ->
                Decode.fail $"Unknown event type: {other}"
        )
