/// JSON encoding/decoding for RelationEvent
namespace EATool.Infrastructure

open Thoth.Json.Net
open EATool.Domain

module RelationEventJson =
    
    let private encodeEntityType (entityType: EntityType) : JsonValue =
        let str = match entityType with
                  | EntityType.Organization -> "organization"
                  | EntityType.Application -> "application"
                  | EntityType.ApplicationService -> "application_service"
                  | EntityType.ApplicationInterface -> "application_interface"
                  | EntityType.Server -> "server"
                  | EntityType.Integration -> "integration"
                  | EntityType.BusinessCapability -> "business_capability"
                  | EntityType.DataEntity -> "data_entity"
                  | EntityType.View -> "view"
        Encode.string str
    
    let private decodeEntityType : Decoder<EntityType> =
        Decode.string
        |> Decode.andThen (fun str ->
            match str with
            | "organization" -> Decode.succeed EntityType.Organization
            | "application" -> Decode.succeed EntityType.Application
            | "application_service" -> Decode.succeed EntityType.ApplicationService
            | "application_interface" -> Decode.succeed EntityType.ApplicationInterface
            | "server" -> Decode.succeed EntityType.Server
            | "integration" -> Decode.succeed EntityType.Integration
            | "business_capability" -> Decode.succeed EntityType.BusinessCapability
            | "data_entity" -> Decode.succeed EntityType.DataEntity
            | "view" -> Decode.succeed EntityType.View
            | _ -> Decode.fail $"Unknown entity type: {str}"
        )
    
    let private encodeRelationType (relationType: RelationType) : JsonValue =
        let str = match relationType with
                  | RelationType.DependsOn -> "depends_on"
                  | RelationType.CommunicatesWith -> "communicates_with"
                  | RelationType.Calls -> "calls"
                  | RelationType.PublishesEventTo -> "publishes_event_to"
                  | RelationType.ConsumesEventFrom -> "consumes_event_from"
                  | RelationType.DeployedOn -> "deployed_on"
                  | RelationType.StoresDataOn -> "stores_data_on"
                  | RelationType.Reads -> "reads"
                  | RelationType.Writes -> "writes"
                  | RelationType.Owns -> "owns"
                  | RelationType.Supports -> "supports"
                  | RelationType.Implements -> "implements"
                  | RelationType.Realizes -> "realizes"
                  | RelationType.Serves -> "serves"
                  | RelationType.ConnectedTo -> "connected_to"
                  | RelationType.Exposes -> "exposes"
                  | RelationType.Uses -> "uses"
        Encode.string str
    
    let private decodeRelationType : Decoder<RelationType> =
        Decode.string
        |> Decode.andThen (fun str ->
            match str with
            | "depends_on" -> Decode.succeed RelationType.DependsOn
            | "communicates_with" -> Decode.succeed RelationType.CommunicatesWith
            | "calls" -> Decode.succeed RelationType.Calls
            | "publishes_event_to" -> Decode.succeed RelationType.PublishesEventTo
            | "consumes_event_from" -> Decode.succeed RelationType.ConsumesEventFrom
            | "deployed_on" -> Decode.succeed RelationType.DeployedOn
            | "stores_data_on" -> Decode.succeed RelationType.StoresDataOn
            | "reads" -> Decode.succeed RelationType.Reads
            | "writes" -> Decode.succeed RelationType.Writes
            | "owns" -> Decode.succeed RelationType.Owns
            | "supports" -> Decode.succeed RelationType.Supports
            | "implements" -> Decode.succeed RelationType.Implements
            | "realizes" -> Decode.succeed RelationType.Realizes
            | "serves" -> Decode.succeed RelationType.Serves
            | "connected_to" -> Decode.succeed RelationType.ConnectedTo
            | "exposes" -> Decode.succeed RelationType.Exposes
            | "uses" -> Decode.succeed RelationType.Uses
            | _ -> Decode.fail $"Unknown relation type: {str}"
        )
    
    // Encoders for event data
    let encodeRelationCreatedData (data: RelationCreatedData) =
        Encode.object [
            "id", Encode.string data.Id
            "source_id", Encode.string data.SourceId
            "target_id", Encode.string data.TargetId
            "source_type", encodeEntityType data.SourceType
            "target_type", encodeEntityType data.TargetType
            "relation_type", encodeRelationType data.RelationType
            "description", Encode.option Encode.string data.Description
            "data_classification", Encode.option Encode.string data.DataClassification
            "confidence", Encode.option Encode.float data.Confidence
            "effective_from", Encode.option Encode.string data.EffectiveFrom
            "effective_to", Encode.option Encode.string data.EffectiveTo
        ]

    let encodeConfidenceUpdatedData (data: ConfidenceUpdatedData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_confidence", Encode.option Encode.float data.OldConfidence
            "new_confidence", Encode.float data.NewConfidence
            "evidence_source", Encode.option Encode.string data.EvidenceSource
            "last_verified_at", Encode.option Encode.string data.LastVerifiedAt
        ]

    let encodeEffectiveDatesSetData (data: EffectiveDatesSetData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_effective_from", Encode.option Encode.string data.OldEffectiveFrom
            "old_effective_to", Encode.option Encode.string data.OldEffectiveTo
            "new_effective_from", Encode.option Encode.string data.NewEffectiveFrom
            "new_effective_to", Encode.option Encode.string data.NewEffectiveTo
        ]

    let encodeRelationDescriptionUpdatedData (data: RelationDescriptionUpdatedData) =
        Encode.object [
            "id", Encode.string data.Id
            "old_description", Encode.option Encode.string data.OldDescription
            "new_description", Encode.option Encode.string data.NewDescription
        ]

    let encodeRelationDeletedData (data: RelationDeletedData) =
        Encode.object [
            "id", Encode.string data.Id
            "reason", Encode.option Encode.string data.Reason
        ]

    // Encoder for RelationEvent discriminated union
    let encodeRelationEvent (event: RelationEvent) : JsonValue =
        match event with
        | RelationCreated data ->
            Encode.object [
                "type", Encode.string "RelationCreated"
                "data", encodeRelationCreatedData data
            ]
        | ConfidenceUpdated data ->
            Encode.object [
                "type", Encode.string "ConfidenceUpdated"
                "data", encodeConfidenceUpdatedData data
            ]
        | EffectiveDatesSet data ->
            Encode.object [
                "type", Encode.string "EffectiveDatesSet"
                "data", encodeEffectiveDatesSetData data
            ]
        | RelationDescriptionUpdated data ->
            Encode.object [
                "type", Encode.string "RelationDescriptionUpdated"
                "data", encodeRelationDescriptionUpdatedData data
            ]
        | RelationDeleted data ->
            Encode.object [
                "type", Encode.string "RelationDeleted"
                "data", encodeRelationDeletedData data
            ]

    // Decoders for event data
    let decodeRelationCreatedData : Decoder<RelationCreatedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            SourceId = get.Required.Field "source_id" Decode.string
            TargetId = get.Required.Field "target_id" Decode.string
            SourceType = get.Required.Field "source_type" decodeEntityType
            TargetType = get.Required.Field "target_type" decodeEntityType
            RelationType = get.Required.Field "relation_type" decodeRelationType
            Description = get.Optional.Field "description" Decode.string
            DataClassification = get.Optional.Field "data_classification" Decode.string
            Confidence = get.Optional.Field "confidence" Decode.float
            EffectiveFrom = get.Optional.Field "effective_from" Decode.string
            EffectiveTo = get.Optional.Field "effective_to" Decode.string
        })

    let decodeConfidenceUpdatedData : Decoder<ConfidenceUpdatedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            OldConfidence = get.Optional.Field "old_confidence" Decode.float
            NewConfidence = get.Required.Field "new_confidence" Decode.float
            EvidenceSource = get.Optional.Field "evidence_source" Decode.string
            LastVerifiedAt = get.Optional.Field "last_verified_at" Decode.string
        })

    let decodeEffectiveDatesSetData : Decoder<EffectiveDatesSetData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            OldEffectiveFrom = get.Optional.Field "old_effective_from" Decode.string
            OldEffectiveTo = get.Optional.Field "old_effective_to" Decode.string
            NewEffectiveFrom = get.Optional.Field "new_effective_from" Decode.string
            NewEffectiveTo = get.Optional.Field "new_effective_to" Decode.string
        })

    let decodeRelationDescriptionUpdatedData : Decoder<RelationDescriptionUpdatedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            OldDescription = get.Optional.Field "old_description" Decode.string
            NewDescription = get.Optional.Field "new_description" Decode.string
        })

    let decodeRelationDeletedData : Decoder<RelationDeletedData> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            Reason = get.Optional.Field "reason" Decode.string
        })

    // Decoder for RelationEvent discriminated union
    let decodeRelationEvent : Decoder<RelationEvent> =
        Decode.field "type" Decode.string
        |> Decode.andThen (fun eventType ->
            match eventType with
            | "RelationCreated" ->
                Decode.field "data" decodeRelationCreatedData
                |> Decode.map RelationCreated
            | "ConfidenceUpdated" ->
                Decode.field "data" decodeConfidenceUpdatedData
                |> Decode.map ConfidenceUpdated
            | "EffectiveDatesSet" ->
                Decode.field "data" decodeEffectiveDatesSetData
                |> Decode.map EffectiveDatesSet
            | "RelationDescriptionUpdated" ->
                Decode.field "data" decodeRelationDescriptionUpdatedData
                |> Decode.map RelationDescriptionUpdated
            | "RelationDeleted" ->
                Decode.field "data" decodeRelationDeletedData
                |> Decode.map RelationDeleted
            | _ ->
                Decode.fail $"Unknown event type: {eventType}"
        )
