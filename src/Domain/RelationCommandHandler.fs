/// Relation command handlers with business logic validation
namespace EATool.Domain

open System

module RelationCommandHandler =
    
    /// Relation validation matrix - valid (source_type, target_type, relation_type) combinations
    /// Based on spec/spec-schema-validation.md
    let private validRelationMatrix = 
        [
            (EntityType.Application, EntityType.Server, RelationType.DeployedOn)
            (EntityType.Application, EntityType.Server, RelationType.StoresDataOn)
            (EntityType.Application, EntityType.BusinessCapability, RelationType.Supports)
            (EntityType.Application, EntityType.DataEntity, RelationType.Reads)
            (EntityType.Application, EntityType.DataEntity, RelationType.Writes)
            (EntityType.Application, EntityType.ApplicationService, RelationType.Realizes)
            (EntityType.Application, EntityType.ApplicationService, RelationType.Uses)
            (EntityType.Application, EntityType.ApplicationInterface, RelationType.Exposes)
            (EntityType.Application, EntityType.Application, RelationType.DependsOn)
            (EntityType.Application, EntityType.Application, RelationType.CommunicatesWith)
            (EntityType.Application, EntityType.Application, RelationType.Calls)
            (EntityType.ApplicationInterface, EntityType.ApplicationService, RelationType.Serves)
            (EntityType.ApplicationService, EntityType.BusinessCapability, RelationType.Realizes)
            (EntityType.ApplicationService, EntityType.BusinessCapability, RelationType.Supports)
            (EntityType.Integration, EntityType.Application, RelationType.CommunicatesWith)
            (EntityType.Integration, EntityType.Application, RelationType.PublishesEventTo)
            (EntityType.Integration, EntityType.Application, RelationType.ConsumesEventFrom)
            (EntityType.Organization, EntityType.Application, RelationType.Owns)
            (EntityType.Organization, EntityType.Server, RelationType.Owns)
            (EntityType.Server, EntityType.Server, RelationType.ConnectedTo)
        ]
        |> Set.ofList
    
    /// Validate relation matrix
    let private validateRelationMatrix (sourceType: EntityType) (targetType: EntityType) (relationType: RelationType) : Result<unit, string> =
        if validRelationMatrix.Contains(sourceType, targetType, relationType) then
            Ok ()
        else
            Error $"Invalid relation: {sourceType} -> {targetType} with relation type {relationType}. See relation validation matrix in spec/spec-schema-validation.md"
    
    /// Validate effective dates
    let private validateEffectiveDates (effectiveFrom: UtcTimestamp option) (effectiveTo: UtcTimestamp option) : Result<unit, string> =
        match effectiveFrom, effectiveTo with
        | Some fromDate, Some toDate ->
            if fromDate <= toDate then
                Ok ()
            else
                Error "effective_from must be less than or equal to effective_to"
        | _ -> Ok ()
    
    /// Validate confidence range
    let private validateConfidence (confidence: float option) : Result<unit, string> =
        match confidence with
        | Some c when c < 0.0 || c > 1.0 ->
            Error "Confidence must be between 0.0 and 1.0"
        | _ -> Ok ()
    
    /// Handle CreateRelation command
    let handleCreateRelation (state: RelationAggregate) (cmd: CreateRelationData) : Result<RelationEvent list, string> =
        match state with
        | RelationAggregate.Active _ -> Error "Relation already exists"
        | RelationAggregate.Deleted -> Error "Cannot recreate deleted relation"
        | RelationAggregate.Initial ->
            if String.IsNullOrWhiteSpace(cmd.Id) then
                Error "Relation ID is required"
            elif String.IsNullOrWhiteSpace(cmd.SourceId) then
                Error "Source ID is required"
            elif String.IsNullOrWhiteSpace(cmd.TargetId) then
                Error "Target ID is required"
            else
                // Validate relation matrix
                match validateRelationMatrix cmd.SourceType cmd.TargetType cmd.RelationType with
                | Error e -> Error e
                | Ok () ->
                    // Validate effective dates
                    match validateEffectiveDates cmd.EffectiveFrom cmd.EffectiveTo with
                    | Error e -> Error e
                    | Ok () ->
                        // Validate confidence
                        match validateConfidence cmd.Confidence with
                        | Error e -> Error e
                        | Ok () ->
                            Ok [RelationCreated {
                                Id = cmd.Id
                                SourceId = cmd.SourceId
                                TargetId = cmd.TargetId
                                SourceType = cmd.SourceType
                                TargetType = cmd.TargetType
                                RelationType = cmd.RelationType
                                Description = cmd.Description
                                DataClassification = cmd.DataClassification
                                Confidence = cmd.Confidence
                                EffectiveFrom = cmd.EffectiveFrom
                                EffectiveTo = cmd.EffectiveTo
                            }]
    
    /// Handle UpdateConfidence command
    let handleUpdateConfidence (state: RelationAggregate) (cmd: UpdateConfidenceData) : Result<RelationEvent list, string> =
        match state with
        | RelationAggregate.Initial -> Error "Relation does not exist"
        | RelationAggregate.Deleted -> Error "Cannot modify deleted relation"
        | RelationAggregate.Active current ->
            // Validate confidence range
            if cmd.Confidence < 0.0 || cmd.Confidence > 1.0 then
                Error "Confidence must be between 0.0 and 1.0"
            elif current.Confidence = Some cmd.Confidence then
                Ok [] // No change
            else
                Ok [ConfidenceUpdated {
                    Id = cmd.Id
                    OldConfidence = current.Confidence
                    NewConfidence = cmd.Confidence
                    EvidenceSource = cmd.EvidenceSource
                    LastVerifiedAt = cmd.LastVerifiedAt
                }]
    
    /// Handle SetEffectiveDates command
    let handleSetEffectiveDates (state: RelationAggregate) (cmd: SetEffectiveDatesData) : Result<RelationEvent list, string> =
        match state with
        | RelationAggregate.Initial -> Error "Relation does not exist"
        | RelationAggregate.Deleted -> Error "Cannot modify deleted relation"
        | RelationAggregate.Active current ->
            // Validate effective dates
            match validateEffectiveDates cmd.EffectiveFrom cmd.EffectiveTo with
            | Error e -> Error e
            | Ok () ->
                if current.EffectiveFrom = cmd.EffectiveFrom && current.EffectiveTo = cmd.EffectiveTo then
                    Ok [] // No change
                else
                    Ok [EffectiveDatesSet {
                        Id = cmd.Id
                        OldEffectiveFrom = current.EffectiveFrom
                        OldEffectiveTo = current.EffectiveTo
                        NewEffectiveFrom = cmd.EffectiveFrom
                        NewEffectiveTo = cmd.EffectiveTo
                    }]
    
    /// Handle UpdateDescription command
    let handleUpdateDescription (state: RelationAggregate) (cmd: UpdateRelationDescriptionData) : Result<RelationEvent list, string> =
        match state with
        | RelationAggregate.Initial -> Error "Relation does not exist"
        | RelationAggregate.Deleted -> Error "Cannot modify deleted relation"
        | RelationAggregate.Active current ->
            if current.Description = cmd.Description then
                Ok [] // No change
            else
                Ok [RelationDescriptionUpdated {
                    Id = cmd.Id
                    OldDescription = current.Description
                    NewDescription = cmd.Description
                }]
    
    /// Handle DeleteRelation command
    let handleDeleteRelation (state: RelationAggregate) (cmd: DeleteRelationData) : Result<RelationEvent list, string> =
        match state with
        | RelationAggregate.Initial -> Error "Relation does not exist"
        | RelationAggregate.Deleted -> Error "Relation already deleted"
        | RelationAggregate.Active _ ->
            Ok [RelationDeleted {
                Id = cmd.Id
                Reason = cmd.Reason
            }]
    
    /// Main command handler router
    let handleCommand (state: RelationAggregate) (command: RelationCommand) : Result<RelationEvent list, string> =
        match command with
        | CreateRelation data -> handleCreateRelation state data
        | UpdateConfidence data -> handleUpdateConfidence state data
        | SetEffectiveDates data -> handleSetEffectiveDates state data
        | UpdateDescription data -> handleUpdateDescription state data
        | DeleteRelation data -> handleDeleteRelation state data
