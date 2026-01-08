namespace EATool.Domain

open System

/// Commands for ApplicationInterface aggregate
type ApplicationInterfaceCommand =
    | CreateApplicationInterface of CreateApplicationInterfaceData
    | UpdateApplicationInterface of UpdateApplicationInterfaceData
    | SetServedServices of SetServedServicesData
    | SetInterfaceStatus of SetInterfaceStatusData
    | DeleteApplicationInterface of DeleteApplicationInterfaceData

and CreateApplicationInterfaceData =
    {
        Id: string
        Name: string
        Protocol: string
        Endpoint: string option
        SpecificationUrl: string option
        Version: string option
        AuthenticationMethod: string option
        ExposedByAppId: string
        ServesServiceIds: string list
        RateLimits: Map<string, string> option
        Status: InterfaceStatus
        Tags: string list
    }

and UpdateApplicationInterfaceData =
    {
        Id: string
        Name: string option
        Protocol: string option
        Endpoint: string option
        Version: string option
        AuthenticationMethod: string option
        Tags: string list option
    }

and SetServedServicesData =
    {
        Id: string
        ServiceIds: string list
    }

and SetInterfaceStatusData =
    {
        Id: string
        Status: InterfaceStatus
    }

and DeleteApplicationInterfaceData =
    {
        Id: string
    }

/// Events for ApplicationInterface aggregate
type ApplicationInterfaceEvent =
    | ApplicationInterfaceCreated of ApplicationInterfaceCreatedData
    | ApplicationInterfaceUpdated of ApplicationInterfaceUpdatedData
    | ServedServicesSet of ServedServicesSetData
    | StatusChanged of StatusChangedData
    | ApplicationInterfaceDeleted of ApplicationInterfaceDeletedData

and ApplicationInterfaceCreatedData =
    {
        Id: string
        Name: string
        Protocol: string
        Endpoint: string option
        SpecificationUrl: string option
        Version: string option
        AuthenticationMethod: string option
        ExposedByAppId: string
        ServesServiceIds: string list
        RateLimits: Map<string, string> option
        Status: InterfaceStatus
        Tags: string list
    }

and ApplicationInterfaceUpdatedData =
    {
        Id: string
        Name: string option
        Protocol: string option
        Endpoint: string option
        Version: string option
        AuthenticationMethod: string option
        Tags: string list option
    }

and ServedServicesSetData =
    {
        Id: string
        ServiceIds: string list
    }

and StatusChangedData =
    {
        Id: string
        Status: InterfaceStatus
    }

and ApplicationInterfaceDeletedData =
    {
        Id: string
    }

/// Aggregate state for ApplicationInterface
type ApplicationInterfaceAggregate =
    {
        Id: string option
        Name: string option
        Protocol: string option
        Endpoint: string option
        SpecificationUrl: string option
        Version: string option
        AuthenticationMethod: string option
        ExposedByAppId: string option
        ServesServiceIds: string list
        RateLimits: Map<string, string> option
        Status: InterfaceStatus option
        Tags: string list
        IsDeleted: bool
    }
    static member Initial =
        {
            Id = None
            Name = None
            Protocol = None
            Endpoint = None
            SpecificationUrl = None
            Version = None
            AuthenticationMethod = None
            ExposedByAppId = None
            ServesServiceIds = []
            RateLimits = None
            Status = None
            Tags = []
            IsDeleted = false
        }

module ApplicationInterfaceAggregate =
    /// Apply an event to the aggregate state
    let apply (state: ApplicationInterfaceAggregate) (event: ApplicationInterfaceEvent) : ApplicationInterfaceAggregate =
        match event with
        | ApplicationInterfaceCreated data ->
            { state with
                Id = Some data.Id
                Name = Some data.Name
                Protocol = Some data.Protocol
                Endpoint = data.Endpoint
                SpecificationUrl = data.SpecificationUrl
                Version = data.Version
                AuthenticationMethod = data.AuthenticationMethod
                ExposedByAppId = Some data.ExposedByAppId
                ServesServiceIds = data.ServesServiceIds
                RateLimits = data.RateLimits
                Status = Some data.Status
                Tags = data.Tags
                IsDeleted = false }
        | ApplicationInterfaceUpdated data ->
            { state with
                Name = data.Name |> Option.orElse state.Name
                Protocol = data.Protocol |> Option.orElse state.Protocol
                Endpoint = data.Endpoint |> Option.orElse state.Endpoint
                Version = data.Version |> Option.orElse state.Version
                AuthenticationMethod = data.AuthenticationMethod |> Option.orElse state.AuthenticationMethod
                Tags = data.Tags |> Option.defaultValue state.Tags }
        | ServedServicesSet data -> { state with ServesServiceIds = data.ServiceIds }
        | StatusChanged data -> { state with Status = Some data.Status }
        | ApplicationInterfaceDeleted _ -> { state with IsDeleted = true }
