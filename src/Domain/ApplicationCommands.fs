/// Application-specific commands and events
namespace EATool.Domain

open System

/// Application Commands
type ApplicationCommand =
    | CreateApplication of CreateApplicationData
    | SetDataClassification of SetDataClassificationData
    | TransitionLifecycle of TransitionLifecycleData
    | SetOwner of SetOwnerData
    | AssignToCapability of AssignToCapabilityData
    | RemoveFromCapability of RemoveFromCapabilityData
    | AddTags of AddTagsData
    | RemoveTags of RemoveTagsData
    | SetCriticality of SetCriticalityData
    | UpdateDescription of UpdateDescriptionData
    | DeleteApplication of DeleteApplicationData

and CreateApplicationData =
    {
        Id: string
        Name: string
        Owner: string
        Lifecycle: string
        CapabilityId: string option
        DataClassification: string
        Criticality: string option
        Tags: string list
        Description: string option
    }

and SetDataClassificationData =
    {
        Id: string
        Classification: string
        Reason: string
    }

and TransitionLifecycleData =
    {
        Id: string
        TargetLifecycle: string
        SunsetDate: string option
    }

and SetOwnerData =
    {
        Id: string
        Owner: string
        Reason: string option
    }

and AssignToCapabilityData =
    {
        Id: string
        CapabilityId: string
    }

and RemoveFromCapabilityData =
    {
        Id: string
    }

and AddTagsData =
    {
        Id: string
        Tags: string list
    }

and RemoveTagsData =
    {
        Id: string
        Tags: string list
    }

and SetCriticalityData =
    {
        Id: string
        Criticality: string
        Justification: string
    }

and UpdateDescriptionData =
    {
        Id: string
        Description: string
    }

and DeleteApplicationData =
    {
        Id: string
        Reason: string
        ApprovalId: string
    }

/// Application Events  
type ApplicationEvent =
    | ApplicationCreated of ApplicationCreatedData
    | DataClassificationChanged of DataClassificationChangedData
    | LifecycleTransitioned of LifecycleTransitionedData
    | OwnerSet of OwnerSetData
    | CapabilityAssigned of CapabilityAssignedData
    | CapabilityRemoved of CapabilityRemovedData
    | TagsAdded of TagsAddedData
    | TagsRemoved of TagsRemovedData
    | CriticalitySet of CriticalitySetData
    | DescriptionUpdated of DescriptionUpdatedData
    | ApplicationDeleted of ApplicationDeletedData

and ApplicationCreatedData =
    {
        Id: string
        Name: string
        Owner: string option
        Lifecycle: string
        CapabilityId: string option
        DataClassification: string option
        Criticality: string option
        Tags: string list
        Description: string option
    }

and DataClassificationChangedData =
    {
        Id: string
        OldClassification: string option
        NewClassification: string
        Reason: string
    }

and LifecycleTransitionedData =
    {
        Id: string
        FromLifecycle: string
        ToLifecycle: string
        SunsetDate: string option
    }

and OwnerSetData =
    {
        Id: string
        OldOwner: string option
        NewOwner: string
        Reason: string option
    }

and CapabilityAssignedData =
    {
        Id: string
        CapabilityId: string
    }

and CapabilityRemovedData =
    {
        Id: string
        OldCapabilityId: string
    }

and TagsAddedData =
    {
        Id: string
        AddedTags: string list
    }

and TagsRemovedData =
    {
        Id: string
        RemovedTags: string list
    }

and CriticalitySetData =
    {
        Id: string
        OldCriticality: string option
        NewCriticality: string
        Justification: string
    }

and DescriptionUpdatedData =
    {
        Id: string
        OldDescription: string option
        NewDescription: string
    }

and ApplicationDeletedData =
    {
        Id: string
        Reason: string
        ApprovalId: string
    }

/// Application aggregate state for event sourcing
type ApplicationAggregate =
    {
        Id: string option
        Name: string option
        Owner: string option
        Lifecycle: string option
        CapabilityId: string option
        DataClassification: string option
        Criticality: string option
        Tags: string list
        Description: string option
        IsDeleted: bool
    }
    static member Initial =
        {
            Id = None
            Name = None
            Owner = None
            Lifecycle = None
            CapabilityId = None
            DataClassification = None
            Criticality = None
            Tags = []
            Description = None
            IsDeleted = false
        }

module ApplicationAggregate =
    /// Apply an event to the aggregate
    let apply (state: ApplicationAggregate) (event: ApplicationEvent) : ApplicationAggregate =
        match event with
        | ApplicationCreated data ->
            { state with
                Id = Some data.Id
                Name = Some data.Name
                Owner = data.Owner
                Lifecycle = Some data.Lifecycle
                CapabilityId = data.CapabilityId
                DataClassification = data.DataClassification
                Criticality = data.Criticality
                Tags = data.Tags
                Description = data.Description }
        
        | DataClassificationChanged data ->
            { state with DataClassification = Some data.NewClassification }
        
        | LifecycleTransitioned data ->
            { state with Lifecycle = Some data.ToLifecycle }
        
        | OwnerSet data ->
            { state with Owner = Some data.NewOwner }
        
        | CapabilityAssigned data ->
            { state with CapabilityId = Some data.CapabilityId }
        
        | CapabilityRemoved _ ->
            { state with CapabilityId = None }
        
        | TagsAdded data ->
            { state with Tags = state.Tags @ data.AddedTags |> List.distinct }
        
        | TagsRemoved data ->
            { state with Tags = state.Tags |> List.filter (fun t -> not (List.contains t data.RemovedTags)) }
        
        | CriticalitySet data ->
            { state with Criticality = Some data.NewCriticality }
        
        | DescriptionUpdated data ->
            { state with Description = Some data.NewDescription }
        
        | ApplicationDeleted _ ->
            { state with IsDeleted = true }
