/// Integration-specific commands and events
namespace EATool.Domain

open System

/// Integration Commands
type IntegrationCommand =
    | CreateIntegration of CreateIntegrationData
    | UpdateProtocol of UpdateProtocolData
    | SetSLA of SetSLAData
    | SetFrequency of SetFrequencyData
    | UpdateDataContract of UpdateDataContractData
    | SetSourceApp of SetSourceAppData
    | SetTargetApp of SetTargetAppData
    | AddIntegrationTags of AddIntegrationTagsData
    | RemoveIntegrationTags of RemoveIntegrationTagsData
    | DeleteIntegration of DeleteIntegrationData

and CreateIntegrationData =
    {
        Id: string
        SourceAppId: string
        TargetAppId: string
        Protocol: string
        DataContract: string option
        Sla: string option
        Frequency: string option
        Tags: string list
    }

and UpdateProtocolData =
    {
        Id: string
        NewProtocol: string
    }

and SetSLAData =
    {
        Id: string
        Sla: string option
    }

and SetFrequencyData =
    {
        Id: string
        Frequency: string option
    }

and UpdateDataContractData =
    {
        Id: string
        DataContract: string option
    }

and SetSourceAppData =
    {
        Id: string
        NewSourceAppId: string
    }

and SetTargetAppData =
    {
        Id: string
        NewTargetAppId: string
    }

and AddIntegrationTagsData =
    {
        Id: string
        Tags: string list
    }

and RemoveIntegrationTagsData =
    {
        Id: string
        Tags: string list
    }

and DeleteIntegrationData =
    {
        Id: string
    }

/// Integration Events
type IntegrationEvent =
    | IntegrationCreated of IntegrationCreatedData
    | ProtocolUpdated of ProtocolUpdatedData
    | SLASet of SLASetData
    | FrequencySet of FrequencySetData
    | DataContractUpdated of DataContractUpdatedData
    | SourceAppSet of SourceAppSetData
    | TargetAppSet of TargetAppSetData
    | IntegrationTagsAdded of IntegrationTagsAddedData
    | IntegrationTagsRemoved of IntegrationTagsRemovedData
    | IntegrationDeleted of IntegrationDeletedData

and IntegrationCreatedData =
    {
        Id: string
        SourceAppId: string
        TargetAppId: string
        Protocol: string
        DataContract: string option
        Sla: string option
        Frequency: string option
        Tags: string list
    }

and ProtocolUpdatedData =
    {
        Id: string
        OldProtocol: string
        NewProtocol: string
    }

and SLASetData =
    {
        Id: string
        OldSla: string option
        NewSla: string option
    }

and FrequencySetData =
    {
        Id: string
        OldFrequency: string option
        NewFrequency: string option
    }

and DataContractUpdatedData =
    {
        Id: string
        OldDataContract: string option
        NewDataContract: string option
    }

and SourceAppSetData =
    {
        Id: string
        OldSourceAppId: string
        NewSourceAppId: string
    }

and TargetAppSetData =
    {
        Id: string
        OldTargetAppId: string
        NewTargetAppId: string
    }

and IntegrationTagsAddedData =
    {
        Id: string
        AddedTags: string list
    }

and IntegrationTagsRemovedData =
    {
        Id: string
        RemovedTags: string list
    }

and IntegrationDeletedData =
    {
        Id: string
    }

/// Integration Aggregate for event sourcing
type IntegrationAggregate =
    {
        Id: string option
        SourceAppId: string option
        TargetAppId: string option
        Protocol: string option
        DataContract: string option
        Sla: string option
        Frequency: string option
        Tags: string list
        IsDeleted: bool
    }
    static member Empty =
        {
            Id = None
            SourceAppId = None
            TargetAppId = None
            Protocol = None
            DataContract = None
            Sla = None
            Frequency = None
            Tags = []
            IsDeleted = false
        }
    
    /// Apply events to update aggregate state
    member state.ApplyEvent(event: IntegrationEvent) : IntegrationAggregate =
        match event with
        | IntegrationCreated evt ->
            { state with
                Id = Some evt.Id
                SourceAppId = Some evt.SourceAppId
                TargetAppId = Some evt.TargetAppId
                Protocol = Some evt.Protocol
                DataContract = evt.DataContract
                Sla = evt.Sla
                Frequency = evt.Frequency
                Tags = evt.Tags
            }
        | ProtocolUpdated evt ->
            { state with Protocol = Some evt.NewProtocol }
        | SLASet evt ->
            { state with Sla = evt.NewSla }
        | FrequencySet evt ->
            { state with Frequency = evt.NewFrequency }
        | DataContractUpdated evt ->
            { state with DataContract = evt.NewDataContract }
        | SourceAppSet evt ->
            { state with SourceAppId = Some evt.NewSourceAppId }
        | TargetAppSet evt ->
            { state with TargetAppId = Some evt.NewTargetAppId }
        | IntegrationTagsAdded evt ->
            let newTags = evt.AddedTags |> List.filter (fun t -> not (List.contains t state.Tags))
            { state with Tags = state.Tags @ newTags }
        | IntegrationTagsRemoved evt ->
            { state with Tags = state.Tags |> List.filter (fun t -> not (List.contains t evt.RemovedTags)) }
        | IntegrationDeleted _ ->
            { state with IsDeleted = true }

    /// Apply multiple events in order
    member state.ApplyEvents(events: IntegrationEvent list) : IntegrationAggregate =
        events |> List.fold (fun s evt -> s.ApplyEvent(evt)) state
