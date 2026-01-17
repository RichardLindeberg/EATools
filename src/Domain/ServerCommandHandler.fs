/// Server command handlers with business logic validation
namespace EATool.Domain

open System

module ServerCommandHandler =
    
    /// Validate environment value
    let private validateEnvironment (environment: string) : Result<unit, string> =
        let valid = ["dev"; "staging"; "prod"]
        if List.contains (environment.ToLowerInvariant()) valid then
            Ok ()
        else
            let validStr = String.Join(", ", valid)
            Error $"Invalid environment {environment}. Must be one of: {validStr}"
    
    /// Validate criticality value
    let private validateCriticality (criticality: string) : Result<unit, string> =
        let valid = ["low"; "medium"; "high"; "critical"]
        if List.contains (criticality.ToLowerInvariant()) valid then
            Ok ()
        else
            let validStr = String.Join(", ", valid)
            Error $"Invalid criticality {criticality}. Must be one of: {validStr}"
    
    /// Validate hostname format using HostnameValidator
    let private validateHostname (hostname: string) : Result<unit, string> =
        match HostnameValidator.validate hostname with
        | Some error -> Error error
        | None -> Ok ()
    
    /// Handle CreateServer command
    let handleCreateServer (state: ServerAggregate) (cmd: CreateServerData) : Result<ServerEvent list, string> =
        if state.Id.IsSome then
            Error "Server already exists"
        elif String.IsNullOrWhiteSpace(cmd.Hostname) then
            Error "Server hostname is required"
        elif String.IsNullOrWhiteSpace(cmd.Id) then
            Error "Server ID is required"
        elif String.IsNullOrWhiteSpace(cmd.Environment) then
            Error "Server environment is required"
        elif String.IsNullOrWhiteSpace(cmd.Criticality) then
            Error "Server criticality is required"
        else
            // Validate hostname format
            match validateHostname cmd.Hostname with
            | Error e -> Error e
            | Ok () ->
                // Validate environment
                match validateEnvironment cmd.Environment with
                | Error e -> Error e
                | Ok () ->
                    // Validate criticality
                    match validateCriticality cmd.Criticality with
                    | Error e -> Error e
                    | Ok () ->
                        let eventData : ServerCreatedData = {
                            Id = cmd.Id
                            Hostname = cmd.Hostname
                            Environment = cmd.Environment
                            Region = cmd.Region
                            Platform = cmd.Platform
                            Criticality = cmd.Criticality
                            OwningTeam = cmd.OwningTeam
                            Tags = cmd.Tags
                        }
                        Ok [ServerCreated eventData]
    
    /// Handle UpdateHostname command
    let handleUpdateHostname (state: ServerAggregate) (cmd: UpdateHostnameData) : Result<ServerEvent list, string> =
        if state.Id.IsNone then
            Error "Server does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted server"
        elif String.IsNullOrWhiteSpace(cmd.NewHostname) then
            Error "New hostname cannot be empty"
        else
            match validateHostname cmd.NewHostname with
            | Error e -> Error e
            | Ok () ->
                match state.Hostname with
                | None -> Error "Server has no current hostname"
                | Some currentHostname ->
                    if currentHostname = cmd.NewHostname then
                        Ok [] // No change
                    else
                        Ok [HostnameUpdated {
                            Id = cmd.Id
                            OldHostname = currentHostname
                            NewHostname = cmd.NewHostname
                        }]
    
    /// Handle SetEnvironment command
    let handleSetEnvironment (state: ServerAggregate) (cmd: SetEnvironmentData) : Result<ServerEvent list, string> =
        if state.Id.IsNone then
            Error "Server does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted server"
        elif String.IsNullOrWhiteSpace(cmd.Environment) then
            Error "Environment cannot be empty"
        else
            match validateEnvironment cmd.Environment with
            | Error e -> Error e
            | Ok () ->
                match state.Environment with
                | None -> Error "Server has no current environment"
                | Some currentEnv ->
                    if currentEnv = cmd.Environment then
                        Ok [] // No change
                    else
                        Ok [EnvironmentSet {
                            Id = cmd.Id
                            OldEnvironment = currentEnv
                            NewEnvironment = cmd.Environment
                        }]
    
    /// Handle SetCriticality command
    let handleSetCriticality (state: ServerAggregate) (cmd: SetServerCriticalityData) : Result<ServerEvent list, string> =
        if state.Id.IsNone then
            Error "Server does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted server"
        elif String.IsNullOrWhiteSpace(cmd.Criticality) then
            Error "Criticality cannot be empty"
        else
            match validateCriticality cmd.Criticality with
            | Error e -> Error e
            | Ok () ->
                match state.Criticality with
                | None -> Error "Server has no current criticality"
                | Some currentCrit ->
                    if currentCrit = cmd.Criticality then
                        Ok [] // No change
                    else
                        Ok [CriticalitySet {
                            Id = cmd.Id
                            OldCriticality = currentCrit
                            NewCriticality = cmd.Criticality
                        }]
    
    /// Handle UpdateRegion command
    let handleUpdateRegion (state: ServerAggregate) (cmd: UpdateRegionData) : Result<ServerEvent list, string> =
        if state.Id.IsNone then
            Error "Server does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted server"
        else
            if state.Region = cmd.Region then
                Ok [] // No change
            else
                Ok [RegionUpdated {
                    Id = cmd.Id
                    OldRegion = state.Region
                    NewRegion = cmd.Region
                }]
    
    /// Handle UpdatePlatform command
    let handleUpdatePlatform (state: ServerAggregate) (cmd: UpdatePlatformData) : Result<ServerEvent list, string> =
        if state.Id.IsNone then
            Error "Server does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted server"
        else
            if state.Platform = cmd.Platform then
                Ok [] // No change
            else
                Ok [PlatformUpdated {
                    Id = cmd.Id
                    OldPlatform = state.Platform
                    NewPlatform = cmd.Platform
                }]
    
    /// Handle SetOwningTeam command
    let handleSetOwningTeam (state: ServerAggregate) (cmd: SetOwningTeamData) : Result<ServerEvent list, string> =
        if state.Id.IsNone then
            Error "Server does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted server"
        else
            if state.OwningTeam = cmd.OwningTeam then
                Ok [] // No change
            else
                Ok [OwningTeamSet {
                    Id = cmd.Id
                    OldTeam = state.OwningTeam
                    NewTeam = cmd.OwningTeam
                }]
    
    /// Handle AddTags command
    let handleAddTags (state: ServerAggregate) (cmd: AddServerTagsData) : Result<ServerEvent list, string> =
        if state.Id.IsNone then
            Error "Server does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted server"
        elif cmd.Tags.IsEmpty then
            Error "No tags to add"
        else
            let newTags = cmd.Tags |> List.filter (fun t -> not (List.contains t state.Tags))
            if newTags.IsEmpty then
                Ok [] // All tags already exist
            else
                Ok [ServerTagsAdded { Id = cmd.Id; AddedTags = newTags }]
    
    /// Handle RemoveTags command
    let handleRemoveTags (state: ServerAggregate) (cmd: RemoveServerTagsData) : Result<ServerEvent list, string> =
        if state.Id.IsNone then
            Error "Server does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted server"
        elif cmd.Tags.IsEmpty then
            Error "No tags to remove"
        else
            let tagsToRemove = cmd.Tags |> List.filter (fun t -> List.contains t state.Tags)
            if tagsToRemove.IsEmpty then
                Ok [] // None of the tags exist
            else
                Ok [ServerTagsRemoved { Id = cmd.Id; RemovedTags = tagsToRemove }]
    
    /// Handle DeleteServer command
    let handleDeleteServer (state: ServerAggregate) (cmd: DeleteServerData) : Result<ServerEvent list, string> =
        if state.Id.IsNone then
            Error "Server does not exist"
        elif state.IsDeleted then
            Ok [] // Already deleted
        else
            Ok [ServerDeleted { Id = cmd.Id }]
    
    /// Main command handler that dispatches to specific handlers
    let handle (state: ServerAggregate) (command: ServerCommand) : Result<ServerEvent list, string> =
        match command with
        | CreateServer cmd -> handleCreateServer state cmd
        | UpdateHostname cmd -> handleUpdateHostname state cmd
        | SetEnvironment cmd -> handleSetEnvironment state cmd
        | SetCriticality cmd -> handleSetCriticality state cmd
        | UpdateRegion cmd -> handleUpdateRegion state cmd
        | UpdatePlatform cmd -> handleUpdatePlatform state cmd
        | SetOwningTeam cmd -> handleSetOwningTeam state cmd
        | AddTags cmd -> handleAddTags state cmd
        | RemoveTags cmd -> handleRemoveTags state cmd
        | DeleteServer cmd -> handleDeleteServer state cmd
