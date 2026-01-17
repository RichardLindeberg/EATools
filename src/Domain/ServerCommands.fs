/// Server-specific commands and events
namespace EATool.Domain

open System

/// Server Commands
type ServerCommand =
    | CreateServer of CreateServerData
    | UpdateHostname of UpdateHostnameData
    | SetEnvironment of SetEnvironmentData
    | SetCriticality of SetServerCriticalityData
    | UpdateRegion of UpdateRegionData
    | UpdatePlatform of UpdatePlatformData
    | SetOwningTeam of SetOwningTeamData
    | AddTags of AddServerTagsData
    | RemoveTags of RemoveServerTagsData
    | DeleteServer of DeleteServerData

and CreateServerData =
    {
        Id: string
        Hostname: string
        Environment: string
        Region: string option
        Platform: string option
        Criticality: string
        OwningTeam: string option
        Tags: string list
    }

and UpdateHostnameData =
    {
        Id: string
        NewHostname: string
    }

and SetEnvironmentData =
    {
        Id: string
        Environment: string
    }

and SetServerCriticalityData =
    {
        Id: string
        Criticality: string
    }

and UpdateRegionData =
    {
        Id: string
        Region: string option
    }

and UpdatePlatformData =
    {
        Id: string
        Platform: string option
    }

and SetOwningTeamData =
    {
        Id: string
        OwningTeam: string option
    }

and AddServerTagsData =
    {
        Id: string
        Tags: string list
    }

and RemoveServerTagsData =
    {
        Id: string
        Tags: string list
    }

and DeleteServerData =
    {
        Id: string
    }

/// Server Events
type ServerEvent =
    | ServerCreated of ServerCreatedData
    | HostnameUpdated of HostnameUpdatedData
    | EnvironmentSet of EnvironmentSetData
    | CriticalitySet of ServerCriticalitySetData
    | RegionUpdated of RegionUpdatedData
    | PlatformUpdated of PlatformUpdatedData
    | OwningTeamSet of OwningTeamSetData
    | ServerTagsAdded of ServerTagsAddedData
    | ServerTagsRemoved of ServerTagsRemovedData
    | ServerDeleted of ServerDeletedData

and ServerCreatedData =
    {
        Id: string
        Hostname: string
        Environment: string
        Region: string option
        Platform: string option
        Criticality: string
        OwningTeam: string option
        Tags: string list
    }

and HostnameUpdatedData =
    {
        Id: string
        OldHostname: string
        NewHostname: string
    }

and EnvironmentSetData =
    {
        Id: string
        OldEnvironment: string
        NewEnvironment: string
    }

and ServerCriticalitySetData =
    {
        Id: string
        OldCriticality: string
        NewCriticality: string
    }

and RegionUpdatedData =
    {
        Id: string
        OldRegion: string option
        NewRegion: string option
    }

and PlatformUpdatedData =
    {
        Id: string
        OldPlatform: string option
        NewPlatform: string option
    }

and OwningTeamSetData =
    {
        Id: string
        OldTeam: string option
        NewTeam: string option
    }

and ServerTagsAddedData =
    {
        Id: string
        AddedTags: string list
    }

and ServerTagsRemovedData =
    {
        Id: string
        RemovedTags: string list
    }

and ServerDeletedData =
    {
        Id: string
    }

/// Server Aggregate for event sourcing
type ServerAggregate =
    {
        Id: string option
        Hostname: string option
        Environment: string option
        Region: string option
        Platform: string option
        Criticality: string option
        OwningTeam: string option
        Tags: string list
        IsDeleted: bool
    }
    static member Empty =
        {
            Id = None
            Hostname = None
            Environment = None
            Region = None
            Platform = None
            Criticality = None
            OwningTeam = None
            Tags = []
            IsDeleted = false
        }
    
    /// Apply events to update aggregate state
    member state.ApplyEvent(event: ServerEvent) : ServerAggregate =
        match event with
        | ServerCreated evt ->
            { state with
                Id = Some evt.Id
                Hostname = Some evt.Hostname
                Environment = Some evt.Environment
                Region = evt.Region
                Platform = evt.Platform
                Criticality = Some evt.Criticality
                OwningTeam = evt.OwningTeam
                Tags = evt.Tags
            }
        | HostnameUpdated evt ->
            { state with Hostname = Some evt.NewHostname }
        | EnvironmentSet evt ->
            { state with Environment = Some evt.NewEnvironment }
        | CriticalitySet evt ->
            { state with Criticality = Some evt.NewCriticality }
        | RegionUpdated evt ->
            { state with Region = evt.NewRegion }
        | PlatformUpdated evt ->
            { state with Platform = evt.NewPlatform }
        | OwningTeamSet evt ->
            { state with OwningTeam = evt.NewTeam }
        | ServerTagsAdded evt ->
            let newTags = evt.AddedTags |> List.filter (fun t -> not (List.contains t state.Tags))
            { state with Tags = state.Tags @ newTags }
        | ServerTagsRemoved evt ->
            { state with Tags = state.Tags |> List.filter (fun t -> not (List.contains t evt.RemovedTags)) }
        | ServerDeleted _ ->
            { state with IsDeleted = true }

    /// Apply multiple events in order
    member state.ApplyEvents(events: ServerEvent list) : ServerAggregate =
        events |> List.fold (fun s evt -> s.ApplyEvent(evt)) state
