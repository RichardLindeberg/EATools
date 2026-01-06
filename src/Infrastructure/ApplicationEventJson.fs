/// JSON encoding/decoding for ApplicationEvent
namespace EATool.Infrastructure

open Thoth.Json.Net
open EATool.Domain

module ApplicationEventJson =
    // Encoders for event data
    let encodeApplicationCreatedData (data: ApplicationCreatedData) =
        Encode.object [
            "id", Encode.string data.Id
            "name", Encode.string data.Name
            "owner", Encode.option Encode.string data.Owner
            "lifecycle", Encode.string data.Lifecycle
            "capability_id", Encode.option Encode.string data.CapabilityId
            "data_classification", Encode.option Encode.string data.DataClassification
            "criticality", Encode.option Encode.string data.Criticality
            "tags", Encode.list (List.map Encode.string data.Tags)
            "description", Encode.option Encode.string data.Description
        ]

    let encodeDataClassificationChangedData (data: DataClassificationChangedData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_classification", Encode.option Encode.string data.OldClassification
            "new_classification", Encode.string data.NewClassification
            "reason", Encode.string data.Reason
        ]

    let encodeLifecycleTransitionedData (data: LifecycleTransitionedData) =
        Encode.object [
            "id", Encode.string data.Id
            "from_lifecycle", Encode.string data.FromLifecycle
            "to_lifecycle", Encode.string data.ToLifecycle
            "sunset_date", Encode.option Encode.string data.SunsetDate
        ]

    let encodeOwnerSetData (data: OwnerSetData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_owner", Encode.option Encode.string data.OldOwner
            "new_owner", Encode.string data.NewOwner
            "reason", Encode.option Encode.string data.Reason
        ]

    let encodeCapabilityAssignedData (data: CapabilityAssignedData) =
        Encode.object [
            "id", Encode.string data.Id
            "capability_id", Encode.string data.CapabilityId
        ]

    let encodeCapabilityRemovedData (data: CapabilityRemovedData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_capability_id", Encode.string data.OldCapabilityId
        ]

    let encodeTagsAddedData (data: TagsAddedData) =
        Encode.object [
            "id", Encode.string data.Id
            "added_tags", Encode.list (List.map Encode.string data.AddedTags)
        ]

    let encodeTagsRemovedData (data: TagsRemovedData) =
        Encode.object [
            "id", Encode.string data.Id
            "removed_tags", Encode.list (List.map Encode.string data.RemovedTags)
        ]

    let encodeCriticalitySetData (data: CriticalitySetData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_criticality", Encode.option Encode.string data.OldCriticality
            "new_criticality", Encode.string data.NewCriticality
            "justification", Encode.string data.Justification
        ]

    let encodeDescriptionUpdatedData (data: DescriptionUpdatedData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_description", Encode.option Encode.string data.OldDescription
            "new_description", Encode.string data.NewDescription
        ]

    let encodeApplicationDeletedData (data: ApplicationDeletedData) =
        Encode.object [
            "id", Encode.string data.Id
            "reason", Encode.string data.Reason
            "approval_id", Encode.string data.ApprovalId
        ]

    // Encoder for ApplicationEvent discriminated union
    let encodeApplicationEvent (event: ApplicationEvent) : JsonValue =
        match event with
        | ApplicationCreated data ->
            Encode.object [
                "type", Encode.string "ApplicationCreated"
                "data", encodeApplicationCreatedData data
            ]
        | DataClassificationChanged data ->
            Encode.object [
                "type", Encode.string "DataClassificationChanged"
                "data", encodeDataClassificationChangedData data
            ]
        | LifecycleTransitioned data ->
            Encode.object [
                "type", Encode.string "LifecycleTransitioned"
                "data", encodeLifecycleTransitionedData data
            ]
        | OwnerSet data ->
            Encode.object [
                "type", Encode.string "OwnerSet"
                "data", encodeOwnerSetData data
            ]
        | CapabilityAssigned data ->
            Encode.object [
                "type", Encode.string "CapabilityAssigned"
                "data", encodeCapabilityAssignedData data
            ]
        | CapabilityRemoved data ->
            Encode.object [
                "type", Encode.string "CapabilityRemoved"
                "data", encodeCapabilityRemovedData data
            ]
        | TagsAdded data ->
            Encode.object [
                "type", Encode.string "TagsAdded"
                "data", encodeTagsAddedData data
            ]
        | TagsRemoved data ->
            Encode.object [
                "type", Encode.string "TagsRemoved"
                "data", encodeTagsRemovedData data
            ]
        | CriticalitySet data ->
            Encode.object [
                "type", Encode.string "CriticalitySet"
                "data", encodeCriticalitySetData data
            ]
        | DescriptionUpdated data ->
            Encode.object [
                "type", Encode.string "DescriptionUpdated"
                "data", encodeDescriptionUpdatedData data
            ]
        | ApplicationDeleted data ->
            Encode.object [
                "type", Encode.string "ApplicationDeleted"
                "data", encodeApplicationDeletedData data
            ]

    // Decoders for event data
    let decodeApplicationCreatedData: Decoder<ApplicationCreatedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                Name = get.Required.Field "name" Decode.string
                Owner = get.Optional.Field "owner" Decode.string
                Lifecycle = get.Required.Field "lifecycle" Decode.string
                CapabilityId = get.Optional.Field "capability_id" Decode.string
                DataClassification = get.Optional.Field "data_classification" Decode.string
                Criticality = get.Optional.Field "criticality" Decode.string
                Tags = get.Optional.Field "tags" (Decode.list Decode.string) |> Option.defaultValue []
                Description = get.Optional.Field "description" Decode.string
            })

    let decodeDataClassificationChangedData: Decoder<DataClassificationChangedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                OldClassification = get.Optional.Field "old_classification" Decode.string
                NewClassification = get.Required.Field "new_classification" Decode.string
                Reason = get.Required.Field "reason" Decode.string
            })

    let decodeLifecycleTransitionedData: Decoder<LifecycleTransitionedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                FromLifecycle = get.Required.Field "from_lifecycle" Decode.string
                ToLifecycle = get.Required.Field "to_lifecycle" Decode.string
                SunsetDate = get.Optional.Field "sunset_date" Decode.string
            })

    let decodeOwnerSetData: Decoder<OwnerSetData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                OldOwner = get.Optional.Field "old_owner" Decode.string
                NewOwner = get.Required.Field "new_owner" Decode.string
                Reason = get.Optional.Field "reason" Decode.string
            })

    let decodeCapabilityAssignedData: Decoder<CapabilityAssignedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                CapabilityId = get.Required.Field "capability_id" Decode.string
            })

    let decodeCapabilityRemovedData: Decoder<CapabilityRemovedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                OldCapabilityId = get.Required.Field "old_capability_id" Decode.string
            })

    let decodeTagsAddedData: Decoder<TagsAddedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                AddedTags = get.Required.Field "added_tags" (Decode.list Decode.string)
            })

    let decodeTagsRemovedData: Decoder<TagsRemovedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                RemovedTags = get.Required.Field "removed_tags" (Decode.list Decode.string)
            })

    let decodeCriticalitySetData: Decoder<CriticalitySetData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                OldCriticality = get.Optional.Field "old_criticality" Decode.string
                NewCriticality = get.Required.Field "new_criticality" Decode.string
                Justification = get.Required.Field "justification" Decode.string
            })

    let decodeDescriptionUpdatedData: Decoder<DescriptionUpdatedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                OldDescription = get.Optional.Field "old_description" Decode.string
                NewDescription = get.Required.Field "new_description" Decode.string
            })

    let decodeApplicationDeletedData: Decoder<ApplicationDeletedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                Reason = get.Required.Field "reason" Decode.string
                ApprovalId = get.Required.Field "approval_id" Decode.string
            })

    // Decoder for ApplicationEvent discriminated union
    let decodeApplicationEvent: Decoder<ApplicationEvent> =
        Decode.field "type" Decode.string
        |> Decode.andThen (fun eventType ->
            match eventType with
            | "ApplicationCreated" ->
                Decode.field "data" decodeApplicationCreatedData
                |> Decode.map ApplicationCreated
            | "DataClassificationChanged" ->
                Decode.field "data" decodeDataClassificationChangedData
                |> Decode.map DataClassificationChanged
            | "LifecycleTransitioned" ->
                Decode.field "data" decodeLifecycleTransitionedData
                |> Decode.map LifecycleTransitioned
            | "OwnerSet" ->
                Decode.field "data" decodeOwnerSetData
                |> Decode.map OwnerSet
            | "CapabilityAssigned" ->
                Decode.field "data" decodeCapabilityAssignedData
                |> Decode.map CapabilityAssigned
            | "CapabilityRemoved" ->
                Decode.field "data" decodeCapabilityRemovedData
                |> Decode.map CapabilityRemoved
            | "TagsAdded" ->
                Decode.field "data" decodeTagsAddedData
                |> Decode.map TagsAdded
            | "TagsRemoved" ->
                Decode.field "data" decodeTagsRemovedData
                |> Decode.map TagsRemoved
            | "CriticalitySet" ->
                Decode.field "data" decodeCriticalitySetData
                |> Decode.map CriticalitySet
            | "DescriptionUpdated" ->
                Decode.field "data" decodeDescriptionUpdatedData
                |> Decode.map DescriptionUpdated
            | "ApplicationDeleted" ->
                Decode.field "data" decodeApplicationDeletedData
                |> Decode.map ApplicationDeleted
            | other ->
                Decode.fail ($"Unknown event type: {other}"))
