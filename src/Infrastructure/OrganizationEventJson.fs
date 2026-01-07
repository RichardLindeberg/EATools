/// JSON encoding/decoding for OrganizationEvent
namespace EATool.Infrastructure

open Thoth.Json.Net
open EATool.Domain

module OrganizationEventJson =
    // Encoders for event data
    let encodeOrganizationCreatedData (data: OrganizationCreatedData) =
        Encode.object [
            "id", Encode.string data.Id
            "name", Encode.string data.Name
            "parent_id", Encode.option Encode.string data.ParentId
            "domains", Encode.list (List.map Encode.string data.Domains)
            "contacts", Encode.list (List.map Encode.string data.Contacts)
        ]

    let encodeParentAssignedData (data: ParentAssignedData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_parent_id", Encode.option Encode.string data.OldParentId
            "new_parent_id", Encode.string data.NewParentId
        ]

    let encodeParentRemovedData (data: ParentRemovedData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_parent_id", Encode.string data.OldParentId
        ]

    let encodeContactInfoUpdatedData (data: ContactInfoUpdatedData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_contacts", Encode.list (List.map Encode.string data.OldContacts)
            "new_contacts", Encode.list (List.map Encode.string data.NewContacts)
        ]

    let encodeDomainAddedData (data: DomainAddedData) =
        Encode.object [
            "id", Encode.string data.Id
            "domain", Encode.string data.Domain
        ]

    let encodeDomainRemovedData (data: DomainRemovedData) =
        Encode.object [
            "id", Encode.string data.Id
            "domain", Encode.string data.Domain
        ]

    let encodeOrganizationDeletedData (data: OrganizationDeletedData) =
        Encode.object [
            "id", Encode.string data.Id
            "reason", Encode.string data.Reason
        ]

    // Encoder for OrganizationEvent discriminated union
    let encodeOrganizationEvent (event: OrganizationEvent) : JsonValue =
        match event with
        | OrganizationCreated data ->
            Encode.object [
                "type", Encode.string "OrganizationCreated"
                "data", encodeOrganizationCreatedData data
            ]
        | ParentAssigned data ->
            Encode.object [
                "type", Encode.string "ParentAssigned"
                "data", encodeParentAssignedData data
            ]
        | ParentRemoved data ->
            Encode.object [
                "type", Encode.string "ParentRemoved"
                "data", encodeParentRemovedData data
            ]
        | ContactInfoUpdated data ->
            Encode.object [
                "type", Encode.string "ContactInfoUpdated"
                "data", encodeContactInfoUpdatedData data
            ]
        | DomainAdded data ->
            Encode.object [
                "type", Encode.string "DomainAdded"
                "data", encodeDomainAddedData data
            ]
        | DomainRemoved data ->
            Encode.object [
                "type", Encode.string "DomainRemoved"
                "data", encodeDomainRemovedData data
            ]
        | OrganizationDeleted data ->
            Encode.object [
                "type", Encode.string "OrganizationDeleted"
                "data", encodeOrganizationDeletedData data
            ]

    // Decoders for event data
    let decodeOrganizationCreatedData: Decoder<OrganizationCreatedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                Name = get.Required.Field "name" Decode.string
                ParentId = get.Optional.Field "parent_id" Decode.string
                Domains = get.Optional.Field "domains" (Decode.list Decode.string) |> Option.defaultValue []
                Contacts = get.Optional.Field "contacts" (Decode.list Decode.string) |> Option.defaultValue []
            })

    let decodeParentAssignedData: Decoder<ParentAssignedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                OldParentId = get.Optional.Field "old_parent_id" Decode.string
                NewParentId = get.Required.Field "new_parent_id" Decode.string
            })

    let decodeParentRemovedData: Decoder<ParentRemovedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                OldParentId = get.Required.Field "old_parent_id" Decode.string
            })

    let decodeContactInfoUpdatedData: Decoder<ContactInfoUpdatedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                OldContacts = get.Optional.Field "old_contacts" (Decode.list Decode.string) |> Option.defaultValue []
                NewContacts = get.Optional.Field "new_contacts" (Decode.list Decode.string) |> Option.defaultValue []
            })

    let decodeDomainAddedData: Decoder<DomainAddedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                Domain = get.Required.Field "domain" Decode.string
            })

    let decodeDomainRemovedData: Decoder<DomainRemovedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                Domain = get.Required.Field "domain" Decode.string
            })

    let decodeOrganizationDeletedData: Decoder<OrganizationDeletedData> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                Reason = get.Required.Field "reason" Decode.string
            })

    // Decoder for OrganizationEvent discriminated union
    let decodeOrganizationEvent: Decoder<OrganizationEvent> =
        Decode.field "type" Decode.string
        |> Decode.andThen (fun eventType ->
            match eventType with
            | "OrganizationCreated" ->
                Decode.field "data" decodeOrganizationCreatedData
                |> Decode.map OrganizationCreated
            | "ParentAssigned" ->
                Decode.field "data" decodeParentAssignedData
                |> Decode.map ParentAssigned
            | "ParentRemoved" ->
                Decode.field "data" decodeParentRemovedData
                |> Decode.map ParentRemoved
            | "ContactInfoUpdated" ->
                Decode.field "data" decodeContactInfoUpdatedData
                |> Decode.map ContactInfoUpdated
            | "DomainAdded" ->
                Decode.field "data" decodeDomainAddedData
                |> Decode.map DomainAdded
            | "DomainRemoved" ->
                Decode.field "data" decodeDomainRemovedData
                |> Decode.map DomainRemoved
            | "OrganizationDeleted" ->
                Decode.field "data" decodeOrganizationDeletedData
                |> Decode.map OrganizationDeleted
            | unknown ->
                Decode.fail $"Unknown event type: {unknown}"
        )
