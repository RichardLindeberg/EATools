namespace EATool.Domain

open System

module ApplicationServiceCommandHandler =
    let private ensureExists (state: ApplicationServiceAggregate) =
        if state.Id.IsNone then Error "ApplicationService does not exist"
        elif state.IsDeleted then Error "ApplicationService is deleted"
        else Ok ()

    let handleCreate (state: ApplicationServiceAggregate) (cmd: CreateApplicationServiceData) : Result<ApplicationServiceEvent list, string> =
        if state.Id.IsSome then
            Error "ApplicationService already exists"
        elif String.IsNullOrWhiteSpace(cmd.Id) then
            Error "ApplicationService ID is required"
        elif String.IsNullOrWhiteSpace(cmd.Name) then
            Error "ApplicationService name is required"
        else
            let data : ApplicationServiceCreatedData = {
                Id = cmd.Id
                Name = cmd.Name
                Description = cmd.Description
                BusinessCapabilityId = cmd.BusinessCapabilityId
                Sla = cmd.Sla
                ExposedByAppIds = cmd.ExposedByAppIds |> List.distinct
                Consumers = cmd.Consumers |> List.distinct
                Tags = cmd.Tags |> List.distinct
            }
            Ok [ ApplicationServiceCreated data ]

    let handleUpdate (state: ApplicationServiceAggregate) (cmd: UpdateApplicationServiceData) : Result<ApplicationServiceEvent list, string> =
        match ensureExists state with
        | Error e -> Error e
        | Ok () ->
            if cmd.Name.IsNone && cmd.Description.IsNone && cmd.Sla.IsNone && cmd.Tags.IsNone then
                Ok []
            else
                let evt = {
                    Id = cmd.Id
                    Name = cmd.Name
                    Description = cmd.Description
                    Sla = cmd.Sla
                    Tags = cmd.Tags |> Option.map List.distinct
                }
                Ok [ ApplicationServiceUpdated evt ]

    let handleSetBusinessCapability (state: ApplicationServiceAggregate) (cmd: SetBusinessCapabilityData) : Result<ApplicationServiceEvent list, string> =
        match ensureExists state with
        | Error e -> Error e
        | Ok () ->
            if state.BusinessCapabilityId = cmd.BusinessCapabilityId then Ok []
            else Ok [ BusinessCapabilitySet { Id = cmd.Id; BusinessCapabilityId = cmd.BusinessCapabilityId } ]

    let handleAddConsumer (state: ApplicationServiceAggregate) (cmd: AddConsumerData) : Result<ApplicationServiceEvent list, string> =
        match ensureExists state with
        | Error e -> Error e
        | Ok () ->
            if String.IsNullOrWhiteSpace(cmd.ConsumerAppId) then
                Error "Consumer app ID is required"
            elif state.Consumers |> List.contains cmd.ConsumerAppId then
                Ok []
            else
                Ok [ ConsumerAdded { Id = cmd.Id; ConsumerAppId = cmd.ConsumerAppId } ]

    let handleRemoveConsumer (state: ApplicationServiceAggregate) (cmd: RemoveConsumerData) : Result<ApplicationServiceEvent list, string> =
        match ensureExists state with
        | Error e -> Error e
        | Ok () ->
            if state.Consumers |> List.contains cmd.ConsumerAppId |> not then
                Ok []
            else
                Ok [ ConsumerRemoved { Id = cmd.Id; ConsumerAppId = cmd.ConsumerAppId } ]

    let handleDelete (state: ApplicationServiceAggregate) (cmd: DeleteApplicationServiceData) : Result<ApplicationServiceEvent list, string> =
        match ensureExists state with
        | Error e -> Error e
        | Ok () ->
            if state.IsDeleted then Error "ApplicationService already deleted" else Ok [ ApplicationServiceDeleted { Id = cmd.Id } ]

    let handle (state: ApplicationServiceAggregate) (command: ApplicationServiceCommand) : Result<ApplicationServiceEvent list, string> =
        match command with
        | CreateApplicationService c -> handleCreate state c
        | UpdateApplicationService c -> handleUpdate state c
        | SetBusinessCapability c -> handleSetBusinessCapability state c
        | AddConsumer c -> handleAddConsumer state c
        | RemoveConsumer c -> handleRemoveConsumer state c
        | DeleteApplicationService c -> handleDelete state c
