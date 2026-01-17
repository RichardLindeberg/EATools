/// JSON encoding/decoding for ServerEvent
namespace EATool.Infrastructure

open Thoth.Json.Net
open EATool.Domain

module ServerEventJson =
    // Encoders for event data
    let encodeServerCreatedData (data: ServerCreatedData) =
        Encode.object [
            "id", Encode.string data.Id
            "hostname", Encode.string data.Hostname
            "environment", Encode.string data.Environment
            "region", Encode.option Encode.string data.Region
            "platform", Encode.option Encode.string data.Platform
            "criticality", Encode.string data.Criticality
            "owning_team", Encode.option Encode.string data.OwningTeam
            "tags", Encode.list (List.map Encode.string data.Tags)
        ]

    let encodeHostnameUpdatedData (data: HostnameUpdatedData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_hostname", Encode.string data.OldHostname
            "new_hostname", Encode.string data.NewHostname
        ]

    let encodeEnvironmentSetData (data: EnvironmentSetData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_environment", Encode.string data.OldEnvironment
            "new_environment", Encode.string data.NewEnvironment
        ]

    let encodeCriticalitySetData (data: ServerCriticalitySetData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_criticality", Encode.string data.OldCriticality
            "new_criticality", Encode.string data.NewCriticality
        ]

    let encodeRegionUpdatedData (data: RegionUpdatedData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_region", Encode.option Encode.string data.OldRegion
            "new_region", Encode.option Encode.string data.NewRegion
        ]

    let encodePlatformUpdatedData (data: PlatformUpdatedData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_platform", Encode.option Encode.string data.OldPlatform
            "new_platform", Encode.option Encode.string data.NewPlatform
        ]

    let encodeOwningTeamSetData (data: OwningTeamSetData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_team", Encode.option Encode.string data.OldTeam
            "new_team", Encode.option Encode.string data.NewTeam
        ]

    let encodeServerTagsAddedData (data: ServerTagsAddedData) =
        Encode.object [
            "id", Encode.string data.Id
            "added_tags", Encode.list (List.map Encode.string data.AddedTags)
        ]

    let encodeServerTagsRemovedData (data: ServerTagsRemovedData) =
        Encode.object [
            "id", Encode.string data.Id
            "removed_tags", Encode.list (List.map Encode.string data.RemovedTags)
        ]

    let encodeServerDeletedData (data: ServerDeletedData) =
        Encode.object [
            "id", Encode.string data.Id
        ]

    // Encoder for ServerEvent discriminated union
    let encodeServerEvent (event: ServerEvent) : JsonValue =
        match event with
        | ServerCreated data ->
            Encode.object [
                "type", Encode.string "ServerCreated"
                "data", encodeServerCreatedData data
            ]
        | HostnameUpdated data ->
            Encode.object [
                "type", Encode.string "HostnameUpdated"
                "data", encodeHostnameUpdatedData data
            ]
        | EnvironmentSet data ->
            Encode.object [
                "type", Encode.string "EnvironmentSet"
                "data", encodeEnvironmentSetData data
            ]
        | CriticalitySet data ->
            Encode.object [
                "type", Encode.string "CriticalitySet"
                "data", encodeCriticalitySetData data
            ]
        | RegionUpdated data ->
            Encode.object [
                "type", Encode.string "RegionUpdated"
                "data", encodeRegionUpdatedData data
            ]
        | PlatformUpdated data ->
            Encode.object [
                "type", Encode.string "PlatformUpdated"
                "data", encodePlatformUpdatedData data
            ]
        | OwningTeamSet data ->
            Encode.object [
                "type", Encode.string "OwningTeamSet"
                "data", encodeOwningTeamSetData data
            ]
        | ServerTagsAdded data ->
            Encode.object [
                "type", Encode.string "ServerTagsAdded"
                "data", encodeServerTagsAddedData data
            ]
        | ServerTagsRemoved data ->
            Encode.object [
                "type", Encode.string "ServerTagsRemoved"
                "data", encodeServerTagsRemovedData data
            ]
        | ServerDeleted data ->
            Encode.object [
                "type", Encode.string "ServerDeleted"
                "data", encodeServerDeletedData data
            ]

    // Decoders for event data
    let decodeServerCreatedData : Decoder<ServerCreatedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            Hostname = get.Required.Field "hostname" Decode.string
            Environment = get.Required.Field "environment" Decode.string
            Region = get.Optional.Field "region" Decode.string
            Platform = get.Optional.Field "platform" Decode.string
            Criticality = get.Required.Field "criticality" Decode.string
            OwningTeam = get.Optional.Field "owning_team" Decode.string
            Tags = get.Required.Field "tags" (Decode.list Decode.string)
        })

    let decodeHostnameUpdatedData : Decoder<HostnameUpdatedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            OldHostname = get.Required.Field "old_hostname" Decode.string
            NewHostname = get.Required.Field "new_hostname" Decode.string
        })

    let decodeEnvironmentSetData : Decoder<EnvironmentSetData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            OldEnvironment = get.Required.Field "old_environment" Decode.string
            NewEnvironment = get.Required.Field "new_environment" Decode.string
        })

    let decodeCriticalitySetData : Decoder<ServerCriticalitySetData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            OldCriticality = get.Required.Field "old_criticality" Decode.string
            NewCriticality = get.Required.Field "new_criticality" Decode.string
        })

    let decodeRegionUpdatedData : Decoder<RegionUpdatedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            OldRegion = get.Optional.Field "old_region" Decode.string
            NewRegion = get.Optional.Field "new_region" Decode.string
        })

    let decodePlatformUpdatedData : Decoder<PlatformUpdatedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            OldPlatform = get.Optional.Field "old_platform" Decode.string
            NewPlatform = get.Optional.Field "new_platform" Decode.string
        })

    let decodeOwningTeamSetData : Decoder<OwningTeamSetData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            OldTeam = get.Optional.Field "old_team" Decode.string
            NewTeam = get.Optional.Field "new_team" Decode.string
        })

    let decodeServerTagsAddedData : Decoder<ServerTagsAddedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            AddedTags = get.Required.Field "added_tags" (Decode.list Decode.string)
        })

    let decodeServerTagsRemovedData : Decoder<ServerTagsRemovedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            RemovedTags = get.Required.Field "removed_tags" (Decode.list Decode.string)
        })

    let decodeServerDeletedData : Decoder<ServerDeletedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
        })

    // Decoder for ServerEvent discriminated union
    let decodeServerEvent : Decoder<ServerEvent> =
        Decode.field "type" Decode.string
        |> Decode.andThen (fun eventType ->
            match eventType with
            | "ServerCreated" ->
                Decode.field "data" decodeServerCreatedData
                |> Decode.map ServerCreated
            | "HostnameUpdated" ->
                Decode.field "data" decodeHostnameUpdatedData
                |> Decode.map HostnameUpdated
            | "EnvironmentSet" ->
                Decode.field "data" decodeEnvironmentSetData
                |> Decode.map EnvironmentSet
            | "CriticalitySet" ->
                Decode.field "data" decodeCriticalitySetData
                |> Decode.map CriticalitySet
            | "RegionUpdated" ->
                Decode.field "data" decodeRegionUpdatedData
                |> Decode.map RegionUpdated
            | "PlatformUpdated" ->
                Decode.field "data" decodePlatformUpdatedData
                |> Decode.map PlatformUpdated
            | "OwningTeamSet" ->
                Decode.field "data" decodeOwningTeamSetData
                |> Decode.map OwningTeamSet
            | "ServerTagsAdded" ->
                Decode.field "data" decodeServerTagsAddedData
                |> Decode.map ServerTagsAdded
            | "ServerTagsRemoved" ->
                Decode.field "data" decodeServerTagsRemovedData
                |> Decode.map ServerTagsRemoved
            | "ServerDeleted" ->
                Decode.field "data" decodeServerDeletedData
                |> Decode.map ServerDeleted
            | other ->
                Decode.fail $"Unknown event type: {other}"
        )
