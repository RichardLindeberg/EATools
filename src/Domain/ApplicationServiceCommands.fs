namespace EATool.Domain

open System

/// Commands for ApplicationService aggregate
type ApplicationServiceCommand =
    | CreateApplicationService of CreateApplicationServiceData
    | UpdateApplicationService of UpdateApplicationServiceData
    | SetBusinessCapability of SetBusinessCapabilityData
    | AddConsumer of AddConsumerData
    | RemoveConsumer of RemoveConsumerData
    | DeleteApplicationService of DeleteApplicationServiceData

and CreateApplicationServiceData =
    {
        Id: string
        Name: string
        Description: string option
        BusinessCapabilityId: string option
        Sla: string option
        ExposedByAppIds: string list
        Consumers: string list
        Tags: string list
    }

and UpdateApplicationServiceData =
    {
        Id: string
        Name: string option
        Description: string option
        Sla: string option
        Tags: string list option
    }

and SetBusinessCapabilityData =
    {
        Id: string
        BusinessCapabilityId: string option
    }

and AddConsumerData =
    {
        Id: string
        ConsumerAppId: string
    }

and RemoveConsumerData =
    {
        Id: string
        ConsumerAppId: string
    }

and DeleteApplicationServiceData =
    {
        Id: string
    }

/// Events for ApplicationService aggregate
type ApplicationServiceEvent =
    | ApplicationServiceCreated of ApplicationServiceCreatedData
    | ApplicationServiceUpdated of ApplicationServiceUpdatedData
    | BusinessCapabilitySet of BusinessCapabilitySetData
    | ConsumerAdded of ConsumerAddedData
    | ConsumerRemoved of ConsumerRemovedData
    | ApplicationServiceDeleted of ApplicationServiceDeletedData

and ApplicationServiceCreatedData =
    {
        Id: string
        Name: string
        Description: string option
        BusinessCapabilityId: string option
        Sla: string option
        ExposedByAppIds: string list
        Consumers: string list
        Tags: string list
    }

and ApplicationServiceUpdatedData =
    {
        Id: string
        Name: string option
        Description: string option
        Sla: string option
        Tags: string list option
    }

and BusinessCapabilitySetData =
    {
        Id: string
        BusinessCapabilityId: string option
    }

and ConsumerAddedData =
    {
        Id: string
        ConsumerAppId: string
    }

and ConsumerRemovedData =
    {
        Id: string
        ConsumerAppId: string
    }

and ApplicationServiceDeletedData =
    {
        Id: string
    }

/// Aggregate state for ApplicationService
type ApplicationServiceAggregate =
    {
        Id: string option
        Name: string option
        Description: string option
        BusinessCapabilityId: string option
        Sla: string option
        ExposedByAppIds: string list
        Consumers: string list
        Tags: string list
        IsDeleted: bool
    }
    static member Initial =
        {
            Id = None
            Name = None
            Description = None
            BusinessCapabilityId = None
            Sla = None
            ExposedByAppIds = []
            Consumers = []
            Tags = []
            IsDeleted = false
        }

module ApplicationServiceAggregate =
    /// Apply an event to the aggregate state
    let apply (state: ApplicationServiceAggregate) (event: ApplicationServiceEvent) : ApplicationServiceAggregate =
        match event with
        | ApplicationServiceCreated data ->
            { state with
                Id = Some data.Id
                Name = Some data.Name
                Description = data.Description
                BusinessCapabilityId = data.BusinessCapabilityId
                Sla = data.Sla
                ExposedByAppIds = data.ExposedByAppIds
                Consumers = data.Consumers
                Tags = data.Tags
                IsDeleted = false }
        | ApplicationServiceUpdated data ->
            { state with
                Name = data.Name |> Option.orElse state.Name
                Description = data.Description |> Option.orElse state.Description
                Sla = data.Sla |> Option.orElse state.Sla
                Tags = data.Tags |> Option.defaultValue state.Tags }
        | BusinessCapabilitySet data -> { state with BusinessCapabilityId = data.BusinessCapabilityId }
        | ConsumerAdded data -> { state with Consumers = state.Consumers @ [ data.ConsumerAppId ] |> List.distinct }
        | ConsumerRemoved data -> { state with Consumers = state.Consumers |> List.filter ((<>) data.ConsumerAppId) }
        | ApplicationServiceDeleted _ -> { state with IsDeleted = true }
