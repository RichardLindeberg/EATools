/// BusinessCapability-specific commands and events
namespace EATool.Domain

open System

/// BusinessCapability Commands
type BusinessCapabilityCommand =
    | CreateCapability of CreateCapabilityData
    | SetParent of SetCapabilityParentData
    | RemoveParent of RemoveCapabilityParentData
    | UpdateDescription of UpdateCapabilityDescriptionData
    | DeleteCapability of DeleteCapabilityData

and CreateCapabilityData =
    {
        Id: string
        Name: string
        ParentId: string option
        Description: string option
    }

and SetCapabilityParentData =
    {
        Id: string
        ParentId: string
    }

and RemoveCapabilityParentData =
    {
        Id: string
    }

and UpdateCapabilityDescriptionData =
    {
        Id: string
        Description: string option
    }

and DeleteCapabilityData =
    {
        Id: string
    }

/// BusinessCapability Events
type BusinessCapabilityEvent =
    | CapabilityCreated of CapabilityCreatedData
    | CapabilityParentAssigned of CapabilityParentAssignedData
    | CapabilityParentRemoved of CapabilityParentRemovedData
    | CapabilityDescriptionUpdated of CapabilityDescriptionUpdatedData
    | CapabilityDeleted of CapabilityDeletedData

and CapabilityCreatedData =
    {
        Id: string
        Name: string
        ParentId: string option
        Description: string option
    }

and CapabilityParentAssignedData =
    {
        Id: string
        OldParentId: string option
        NewParentId: string
    }

and CapabilityParentRemovedData =
    {
        Id: string
        OldParentId: string
    }

and CapabilityDescriptionUpdatedData =
    {
        Id: string
        OldDescription: string option
        NewDescription: string option
    }

and CapabilityDeletedData =
    {
        Id: string
    }

/// BusinessCapability Aggregate State
type BusinessCapabilityAggregate =
    | Initial
    | Active of BusinessCapabilityState
    | Deleted

and BusinessCapabilityState =
    {
        Id: string
        Name: string
        ParentId: string option
        Description: string option
    }

/// Apply events to aggregate
module BusinessCapabilityAggregate =
    let apply (state: BusinessCapabilityAggregate) (event: BusinessCapabilityEvent) : BusinessCapabilityAggregate =
        match state, event with
        | Initial, CapabilityCreated data ->
            Active {
                Id = data.Id
                Name = data.Name
                ParentId = data.ParentId
                Description = data.Description
            }
        | Active current, CapabilityParentAssigned data ->
            Active { current with ParentId = Some data.NewParentId }
        | Active current, CapabilityParentRemoved _ ->
            Active { current with ParentId = None }
        | Active current, CapabilityDescriptionUpdated data ->
            Active { current with Description = data.NewDescription }
        | Active _, CapabilityDeleted _ ->
            Deleted
        | _ ->
            state
