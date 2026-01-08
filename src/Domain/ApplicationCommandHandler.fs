/// Application command handlers with business logic validation
namespace EATool.Domain

open System

module ApplicationCommandHandler =
    
    /// Validate lifecycle transition paths
    let private validateLifecycleTransition (current: string) (target: string) : Result<unit, string> =
        let validTransitions =
            [
                ("planned", ["active"; "retired"])
                ("active", ["deprecated"; "retired"])
                ("deprecated", ["retired"; "active"]) // Can reactivate
                ("retired", []) // No transitions from retired
            ]
        
        let allowed = 
            validTransitions 
            |> List.tryFind (fun (from, _) -> from = current.ToLowerInvariant())
            |> Option.map snd
            |> Option.defaultValue []
        
        if List.contains (target.ToLowerInvariant()) allowed then
            Ok ()
        else
            let allowedStr = String.Join(", ", allowed)
            Error $"Invalid lifecycle transition from {current} to {target}. Allowed: {allowedStr}"
    
    /// Validate data classification value
    let private validateClassification (classification: string) : Result<unit, string> =
        let valid = ["public"; "internal"; "confidential"; "restricted"]
        if List.contains (classification.ToLowerInvariant()) valid then
            Ok ()
        else
            let validStr = String.Join(", ", valid)
            Error $"Invalid classification {classification}. Must be one of: {validStr}"
    
    /// Validate criticality value
    let private validateCriticality (criticality: string) : Result<unit, string> =
        let valid = ["low"; "medium"; "high"; "critical"]
        if List.contains (criticality.ToLowerInvariant()) valid then
            Ok ()
        else
            let validStr = String.Join(", ", valid)
            Error $"Invalid criticality {criticality}. Must be one of: {validStr}"
    
    /// Handle CreateApplication command
    let handleCreateApplication (state: ApplicationAggregate) (cmd: CreateApplicationData) : Result<ApplicationEvent list, string> =
        if state.Id.IsSome then
            Error "Application already exists"
        elif String.IsNullOrWhiteSpace(cmd.Name) then
            Error "Application name is required"
        elif String.IsNullOrWhiteSpace(cmd.Id) then
            Error "Application ID is required"
        elif String.IsNullOrWhiteSpace(cmd.Owner) then
            Error "Application owner is required"
        elif String.IsNullOrWhiteSpace(cmd.DataClassification) then
            Error "Application data_classification is required"
        else
            // Validate classification value
            match validateClassification cmd.DataClassification with
            | Error e -> Error e
            | Ok () ->
                // Validate criticality if provided
                match cmd.Criticality with
                | Some cr when not (String.IsNullOrWhiteSpace cr) ->
                    match validateCriticality cr with
                    | Error e -> Error e
                    | Ok () ->
                        let eventData : ApplicationCreatedData = {
                            Id = cmd.Id
                            Name = cmd.Name
                            Owner = Some cmd.Owner
                            Lifecycle = cmd.Lifecycle
                            CapabilityId = cmd.CapabilityId
                            DataClassification = Some cmd.DataClassification
                            Criticality = cmd.Criticality
                            Tags = cmd.Tags
                            Description = cmd.Description
                        }
                        Ok [ApplicationCreated eventData]
                | _ ->
                    let eventData : ApplicationCreatedData = {
                        Id = cmd.Id
                        Name = cmd.Name
                        Owner = Some cmd.Owner
                        Lifecycle = cmd.Lifecycle
                        CapabilityId = cmd.CapabilityId
                        DataClassification = Some cmd.DataClassification
                        Criticality = cmd.Criticality
                        Tags = cmd.Tags
                        Description = cmd.Description
                    }
                    Ok [ApplicationCreated eventData]
    
    /// Handle SetDataClassification command
    let handleSetDataClassification (state: ApplicationAggregate) (cmd: SetDataClassificationData) : Result<ApplicationEvent list, string> =
        if state.Id.IsNone then
            Error "Application does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted application"
        elif String.IsNullOrWhiteSpace(cmd.Reason) then
            Error "Reason is required for classification change"
        else
            match validateClassification cmd.Classification with
            | Error e -> Error e
            | Ok () ->
                if state.DataClassification = Some cmd.Classification then
                    Ok [] // No change
                else
                    Ok [DataClassificationChanged { 
                        Id = cmd.Id
                        OldClassification = state.DataClassification
                        NewClassification = cmd.Classification
                        Reason = cmd.Reason 
                    }]
    
    /// Handle TransitionLifecycle command
    let handleTransitionLifecycle (state: ApplicationAggregate) (cmd: TransitionLifecycleData) : Result<ApplicationEvent list, string> =
        if state.Id.IsNone then
            Error "Application does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted application"
        else
            match state.Lifecycle with
            | None -> Error "Application has no current lifecycle state"
            | Some current ->
                if current = cmd.TargetLifecycle then
                    Ok [] // No change
                else
                    match validateLifecycleTransition current cmd.TargetLifecycle with
                    | Error e -> Error e
                    | Ok () ->
                        Ok [LifecycleTransitioned {
                            Id = cmd.Id
                            FromLifecycle = current
                            ToLifecycle = cmd.TargetLifecycle
                            SunsetDate = cmd.SunsetDate
                        }]
    
    /// Handle SetOwner command
    let handleSetOwner (state: ApplicationAggregate) (cmd: SetOwnerData) : Result<ApplicationEvent list, string> =
        if state.Id.IsNone then
            Error "Application does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted application"
        elif String.IsNullOrWhiteSpace(cmd.Owner) then
            Error "Owner cannot be empty"
        else
            if state.Owner = Some cmd.Owner then
                Ok [] // No change
            else
                Ok [OwnerSet {
                    Id = cmd.Id
                    OldOwner = state.Owner
                    NewOwner = cmd.Owner
                    Reason = cmd.Reason
                }]
    
    /// Handle AssignToCapability command
    let handleAssignToCapability (state: ApplicationAggregate) (cmd: AssignToCapabilityData) : Result<ApplicationEvent list, string> =
        if state.Id.IsNone then
            Error "Application does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted application"
        elif String.IsNullOrWhiteSpace(cmd.CapabilityId) then
            Error "Capability ID is required"
        else
            if state.CapabilityId = Some cmd.CapabilityId then
                Ok [] // Already assigned
            else
                Ok [CapabilityAssigned { Id = cmd.Id; CapabilityId = cmd.CapabilityId }]
    
    /// Handle RemoveFromCapability command
    let handleRemoveFromCapability (state: ApplicationAggregate) (cmd: RemoveFromCapabilityData) : Result<ApplicationEvent list, string> =
        if state.Id.IsNone then
            Error "Application does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted application"
        else
            match state.CapabilityId with
            | None -> Ok [] // Not assigned to any capability
            | Some capId -> Ok [CapabilityRemoved { Id = cmd.Id; OldCapabilityId = capId }]
    
    /// Handle AddTags command
    let handleAddTags (state: ApplicationAggregate) (cmd: AddTagsData) : Result<ApplicationEvent list, string> =
        if state.Id.IsNone then
            Error "Application does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted application"
        elif cmd.Tags.IsEmpty then
            Error "No tags to add"
        else
            let newTags = cmd.Tags |> List.filter (fun t -> not (List.contains t state.Tags))
            if newTags.IsEmpty then
                Ok [] // All tags already exist
            else
                Ok [TagsAdded { Id = cmd.Id; AddedTags = newTags }]
    
    /// Handle RemoveTags command
    let handleRemoveTags (state: ApplicationAggregate) (cmd: RemoveTagsData) : Result<ApplicationEvent list, string> =
        if state.Id.IsNone then
            Error "Application does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted application"
        elif cmd.Tags.IsEmpty then
            Error "No tags to remove"
        else
            let tagsToRemove = cmd.Tags |> List.filter (fun t -> List.contains t state.Tags)
            if tagsToRemove.IsEmpty then
                Ok [] // None of the tags exist
            else
                Ok [TagsRemoved { Id = cmd.Id; RemovedTags = tagsToRemove }]
    
    /// Handle SetCriticality command
    let handleSetCriticality (state: ApplicationAggregate) (cmd: SetCriticalityData) : Result<ApplicationEvent list, string> =
        if state.Id.IsNone then
            Error "Application does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted application"
        elif String.IsNullOrWhiteSpace(cmd.Justification) then
            Error "Justification is required for criticality change"
        else
            match validateCriticality cmd.Criticality with
            | Error e -> Error e
            | Ok () ->
                if state.Criticality = Some cmd.Criticality then
                    Ok [] // No change
                else
                    Ok [CriticalitySet {
                        Id = cmd.Id
                        OldCriticality = state.Criticality
                        NewCriticality = cmd.Criticality
                        Justification = cmd.Justification
                    }]
    
    /// Handle UpdateDescription command
    let handleUpdateDescription (state: ApplicationAggregate) (cmd: UpdateDescriptionData) : Result<ApplicationEvent list, string> =
        if state.Id.IsNone then
            Error "Application does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted application"
        else
            if state.Description = Some cmd.Description then
                Ok [] // No change
            else
                Ok [DescriptionUpdated {
                    Id = cmd.Id
                    OldDescription = state.Description
                    NewDescription = cmd.Description
                }]
    
    /// Handle DeleteApplication command
    let handleDeleteApplication (state: ApplicationAggregate) (cmd: DeleteApplicationData) : Result<ApplicationEvent list, string> =
        if state.Id.IsNone then
            Error "Application does not exist"
        elif state.IsDeleted then
            Error "Application already deleted"
        elif String.IsNullOrWhiteSpace(cmd.Reason) then
            Error "Deletion reason is required"
        elif String.IsNullOrWhiteSpace(cmd.ApprovalId) then
            Error "Approval ID is required for deletion"
        else
            Ok [ApplicationDeleted {
                Id = cmd.Id
                Reason = cmd.Reason
                ApprovalId = cmd.ApprovalId
            }]
    
    /// Main command handler dispatcher
    let handle (state: ApplicationAggregate) (command: ApplicationCommand) : Result<ApplicationEvent list, string> =
        match command with
        | CreateApplication cmd -> handleCreateApplication state cmd
        | SetDataClassification cmd -> handleSetDataClassification state cmd
        | TransitionLifecycle cmd -> handleTransitionLifecycle state cmd
        | SetOwner cmd -> handleSetOwner state cmd
        | AssignToCapability cmd -> handleAssignToCapability state cmd
        | RemoveFromCapability cmd -> handleRemoveFromCapability state cmd
        | AddTags cmd -> handleAddTags state cmd
        | RemoveTags cmd -> handleRemoveTags state cmd
        | SetCriticality cmd -> handleSetCriticality state cmd
        | UpdateDescription cmd -> handleUpdateDescription state cmd
        | DeleteApplication cmd -> handleDeleteApplication state cmd
