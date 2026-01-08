namespace EATool.Domain

open System

module ApplicationInterfaceCommandHandler =
    let private ensureExists (state: ApplicationInterfaceAggregate) =
        if state.Id.IsNone then Error "ApplicationInterface does not exist"
        elif state.IsDeleted then Error "ApplicationInterface is deleted"
        else Ok ()

    let handleCreate (state: ApplicationInterfaceAggregate) (cmd: CreateApplicationInterfaceData) : Result<ApplicationInterfaceEvent list, string> =
        if state.Id.IsSome then
            Error "ApplicationInterface already exists"
        elif String.IsNullOrWhiteSpace(cmd.Id) then
            Error "ApplicationInterface ID is required"
        elif String.IsNullOrWhiteSpace(cmd.Name) then
            Error "ApplicationInterface name is required"
        elif String.IsNullOrWhiteSpace(cmd.Protocol) then
            Error "Protocol is required"
        elif String.IsNullOrWhiteSpace(cmd.ExposedByAppId) then
            Error "ExposedByAppId is required"
        else
            let evt : ApplicationInterfaceCreatedData = {
                Id = cmd.Id
                Name = cmd.Name
                Protocol = cmd.Protocol
                Endpoint = cmd.Endpoint
                SpecificationUrl = cmd.SpecificationUrl
                Version = cmd.Version
                AuthenticationMethod = cmd.AuthenticationMethod
                ExposedByAppId = cmd.ExposedByAppId
                ServesServiceIds = cmd.ServesServiceIds |> List.distinct
                RateLimits = cmd.RateLimits
                Status = cmd.Status
                Tags = cmd.Tags |> List.distinct
            }
            Ok [ ApplicationInterfaceCreated evt ]

    let handleUpdate (state: ApplicationInterfaceAggregate) (cmd: UpdateApplicationInterfaceData) : Result<ApplicationInterfaceEvent list, string> =
        match ensureExists state with
        | Error e -> Error e
        | Ok () ->
            if cmd.Name.IsNone && cmd.Protocol.IsNone && cmd.Endpoint.IsNone && cmd.Version.IsNone && cmd.AuthenticationMethod.IsNone && cmd.Tags.IsNone then
                Ok []
            else
                let evt : ApplicationInterfaceUpdatedData = {
                    Id = cmd.Id
                    Name = cmd.Name
                    Protocol = cmd.Protocol
                    Endpoint = cmd.Endpoint
                    Version = cmd.Version
                    AuthenticationMethod = cmd.AuthenticationMethod
                    Tags = cmd.Tags |> Option.map List.distinct
                }
                Ok [ ApplicationInterfaceUpdated evt ]

    let handleSetServedServices (state: ApplicationInterfaceAggregate) (cmd: SetServedServicesData) : Result<ApplicationInterfaceEvent list, string> =
        match ensureExists state with
        | Error e -> Error e
        | Ok () ->
            let distinctIds = cmd.ServiceIds |> List.distinct
            if distinctIds = state.ServesServiceIds then Ok []
            else Ok [ ServedServicesSet { Id = cmd.Id; ServiceIds = distinctIds } ]

    let handleSetStatus (state: ApplicationInterfaceAggregate) (cmd: SetInterfaceStatusData) : Result<ApplicationInterfaceEvent list, string> =
        match ensureExists state with
        | Error e -> Error e
        | Ok () ->
            if state.Status = Some cmd.Status then Ok []
            else Ok [ StatusChanged { Id = cmd.Id; Status = cmd.Status } ]

    let handleDelete (state: ApplicationInterfaceAggregate) (cmd: DeleteApplicationInterfaceData) : Result<ApplicationInterfaceEvent list, string> =
        match ensureExists state with
        | Error e -> Error e
        | Ok () ->
            if state.IsDeleted then Error "ApplicationInterface already deleted" else Ok [ ApplicationInterfaceDeleted { Id = cmd.Id } ]

    let handle (state: ApplicationInterfaceAggregate) (command: ApplicationInterfaceCommand) : Result<ApplicationInterfaceEvent list, string> =
        match command with
        | CreateApplicationInterface c -> handleCreate state c
        | UpdateApplicationInterface c -> handleUpdate state c
        | SetServedServices c -> handleSetServedServices state c
        | SetInterfaceStatus c -> handleSetStatus state c
        | DeleteApplicationInterface c -> handleDelete state c
