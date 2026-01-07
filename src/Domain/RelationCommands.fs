/// Relation-specific commands and events
namespace EATool.Domain

open System

/// Relation Commands
type RelationCommand =
    | CreateRelation of CreateRelationData
    | UpdateConfidence of UpdateConfidenceData
    | SetEffectiveDates of SetEffectiveDatesData
    | UpdateDescription of UpdateRelationDescriptionData
    | DeleteRelation of DeleteRelationData

and CreateRelationData =
    {
        Id: string
        SourceId: string
        TargetId: string
        SourceType: EntityType
        TargetType: EntityType
        RelationType: RelationType
        Description: string option
        DataClassification: string option
        Confidence: float option
        EffectiveFrom: UtcTimestamp option
        EffectiveTo: UtcTimestamp option
    }

and UpdateConfidenceData =
    {
        Id: string
        Confidence: float
        EvidenceSource: string option
        LastVerifiedAt: UtcTimestamp option
    }

and SetEffectiveDatesData =
    {
        Id: string
        EffectiveFrom: UtcTimestamp option
        EffectiveTo: UtcTimestamp option
    }

and UpdateRelationDescriptionData =
    {
        Id: string
        Description: string option
    }

and DeleteRelationData =
    {
        Id: string
        Reason: string option
    }

/// Relation Events
type RelationEvent =
    | RelationCreated of RelationCreatedData
    | ConfidenceUpdated of ConfidenceUpdatedData
    | EffectiveDatesSet of EffectiveDatesSetData
    | RelationDescriptionUpdated of RelationDescriptionUpdatedData
    | RelationDeleted of RelationDeletedData

and RelationCreatedData =
    {
        Id: string
        SourceId: string
        TargetId: string
        SourceType: EntityType
        TargetType: EntityType
        RelationType: RelationType
        Description: string option
        DataClassification: string option
        Confidence: float option
        EffectiveFrom: UtcTimestamp option
        EffectiveTo: UtcTimestamp option
    }

and ConfidenceUpdatedData =
    {
        Id: string
        OldConfidence: float option
        NewConfidence: float
        EvidenceSource: string option
        LastVerifiedAt: UtcTimestamp option
    }

and EffectiveDatesSetData =
    {
        Id: string
        OldEffectiveFrom: UtcTimestamp option
        OldEffectiveTo: UtcTimestamp option
        NewEffectiveFrom: UtcTimestamp option
        NewEffectiveTo: UtcTimestamp option
    }

and RelationDescriptionUpdatedData =
    {
        Id: string
        OldDescription: string option
        NewDescription: string option
    }

and RelationDeletedData =
    {
        Id: string
        Reason: string option
    }

/// Relation Aggregate State
[<RequireQualifiedAccess>]
type RelationAggregate =
    | Initial
    | Active of RelationState
    | Deleted

and RelationState =
    {
        Id: string
        SourceId: string
        TargetId: string
        SourceType: EntityType
        TargetType: EntityType
        RelationType: RelationType
        Description: string option
        DataClassification: string option
        Confidence: float option
        EvidenceSource: string option
        LastVerifiedAt: UtcTimestamp option
        EffectiveFrom: UtcTimestamp option
        EffectiveTo: UtcTimestamp option
    }

/// Apply events to aggregate
module RelationAggregate =
    let apply (state: RelationAggregate) (event: RelationEvent) : RelationAggregate =
        match state, event with
        | RelationAggregate.Initial, RelationCreated data ->
            RelationAggregate.Active {
                Id = data.Id
                SourceId = data.SourceId
                TargetId = data.TargetId
                SourceType = data.SourceType
                TargetType = data.TargetType
                RelationType = data.RelationType
                Description = data.Description
                DataClassification = data.DataClassification
                Confidence = data.Confidence
                EvidenceSource = None
                LastVerifiedAt = None
                EffectiveFrom = data.EffectiveFrom
                EffectiveTo = data.EffectiveTo
            }
        | RelationAggregate.Active current, ConfidenceUpdated data ->
            RelationAggregate.Active {
                current with
                    Confidence = Some data.NewConfidence
                    EvidenceSource = data.EvidenceSource
                    LastVerifiedAt = data.LastVerifiedAt
            }
        | RelationAggregate.Active current, EffectiveDatesSet data ->
            RelationAggregate.Active {
                current with
                    EffectiveFrom = data.NewEffectiveFrom
                    EffectiveTo = data.NewEffectiveTo
            }
        | RelationAggregate.Active current, RelationDescriptionUpdated data ->
            RelationAggregate.Active { current with Description = data.NewDescription }
        | RelationAggregate.Active _, RelationDeleted _ ->
            RelationAggregate.Deleted
        | _ ->
            state

