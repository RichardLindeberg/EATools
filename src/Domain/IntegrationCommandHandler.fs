/// Integration command handlers with business logic validation
namespace EATool.Domain

open System

module IntegrationCommandHandler =
    
    /// Validate protocol value (common protocols)
    let private validateProtocol (protocol: string) : Result<unit, string> =
        let valid = ["rest"; "graphql"; "grpc"; "soap"; "kafka"; "rabbitmq"; "sqs"; "http"; "https"; "tcp"; "udp"; "websocket"; "mqtt"]
        if List.contains (protocol.ToLowerInvariant()) valid then
            Ok ()
        else
            let validStr = String.Join(", ", valid)
            Error $"Invalid protocol {protocol}. Must be one of: {validStr}"
    
    /// Validate frequency format (e.g., "5m", "1h", "daily", "real-time")
    let private validateFrequency (frequency: string option) : Result<unit, string> =
        match frequency with
        | None -> Ok ()
        | Some freq when String.IsNullOrWhiteSpace freq -> Ok ()
        | Some freq ->
            let valid = ["real-time"; "streaming"; "5m"; "15m"; "30m"; "1h"; "4h"; "12h"; "daily"; "weekly"; "monthly"; "on-demand"]
            let freqLower = freq.ToLowerInvariant()
            if List.contains freqLower valid || System.Text.RegularExpressions.Regex.IsMatch(freq, @"^\d+[smh]$") then
                Ok ()
            else
                Error $"Invalid frequency {freq}. Use format like '5m', '1h', or keywords: real-time, daily, weekly"
    
    /// Validate SLA format (e.g., "99.9%", "< 100ms", "24/7")
    let private validateSLA (sla: string option) : Result<unit, string> =
        match sla with
        | None -> Ok ()
        | Some s when String.IsNullOrWhiteSpace s -> Ok ()
        | Some s ->
            // Basic validation - should contain percentage, time, or availability indicator
            if System.Text.RegularExpressions.Regex.IsMatch(s, @"\d+(\.\d+)?%|\d+\s*(ms|s|min|h)|24/7|best-effort", System.Text.RegularExpressions.RegexOptions.IgnoreCase) then
                Ok ()
            else
                Error $"Invalid SLA format {s}. Use format like '99.9%%', '< 100ms', or '24/7'"
    
    /// Handle CreateIntegration command
    let handleCreateIntegration (state: IntegrationAggregate) (cmd: CreateIntegrationData) : Result<IntegrationEvent list, string> =
        if state.Id.IsSome then
            Error "Integration already exists"
        elif String.IsNullOrWhiteSpace(cmd.Id) then
            Error "Integration ID is required"
        elif String.IsNullOrWhiteSpace(cmd.SourceAppId) then
            Error "Source application ID is required"
        elif String.IsNullOrWhiteSpace(cmd.TargetAppId) then
            Error "Target application ID is required"
        elif String.IsNullOrWhiteSpace(cmd.Protocol) then
            Error "Protocol is required"
        elif cmd.SourceAppId = cmd.TargetAppId then
            Error "Source and target applications must be different"
        else
            // Validate protocol
            match validateProtocol cmd.Protocol with
            | Error e -> Error e
            | Ok () ->
                // Validate frequency
                match validateFrequency cmd.Frequency with
                | Error e -> Error e
                | Ok () ->
                    // Validate SLA
                    match validateSLA cmd.Sla with
                    | Error e -> Error e
                    | Ok () ->
                        let eventData : IntegrationCreatedData = {
                            Id = cmd.Id
                            SourceAppId = cmd.SourceAppId
                            TargetAppId = cmd.TargetAppId
                            Protocol = cmd.Protocol
                            DataContract = cmd.DataContract
                            Sla = cmd.Sla
                            Frequency = cmd.Frequency
                            Tags = cmd.Tags
                        }
                        Ok [IntegrationCreated eventData]
    
    /// Handle UpdateProtocol command
    let handleUpdateProtocol (state: IntegrationAggregate) (cmd: UpdateProtocolData) : Result<IntegrationEvent list, string> =
        if state.Id.IsNone then
            Error "Integration does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted integration"
        elif String.IsNullOrWhiteSpace(cmd.NewProtocol) then
            Error "Protocol cannot be empty"
        else
            match validateProtocol cmd.NewProtocol with
            | Error e -> Error e
            | Ok () ->
                match state.Protocol with
                | None -> Error "Integration has no current protocol"
                | Some currentProtocol ->
                    if currentProtocol = cmd.NewProtocol then
                        Ok [] // No change
                    else
                        Ok [ProtocolUpdated {
                            Id = cmd.Id
                            OldProtocol = currentProtocol
                            NewProtocol = cmd.NewProtocol
                        }]
    
    /// Handle SetSLA command
    let handleSetSLA (state: IntegrationAggregate) (cmd: SetSLAData) : Result<IntegrationEvent list, string> =
        if state.Id.IsNone then
            Error "Integration does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted integration"
        else
            match validateSLA cmd.Sla with
            | Error e -> Error e
            | Ok () ->
                if state.Sla = cmd.Sla then
                    Ok [] // No change
                else
                    Ok [SLASet {
                        Id = cmd.Id
                        OldSla = state.Sla
                        NewSla = cmd.Sla
                    }]
    
    /// Handle SetFrequency command
    let handleSetFrequency (state: IntegrationAggregate) (cmd: SetFrequencyData) : Result<IntegrationEvent list, string> =
        if state.Id.IsNone then
            Error "Integration does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted integration"
        else
            match validateFrequency cmd.Frequency with
            | Error e -> Error e
            | Ok () ->
                if state.Frequency = cmd.Frequency then
                    Ok [] // No change
                else
                    Ok [FrequencySet {
                        Id = cmd.Id
                        OldFrequency = state.Frequency
                        NewFrequency = cmd.Frequency
                    }]
    
    /// Handle UpdateDataContract command
    let handleUpdateDataContract (state: IntegrationAggregate) (cmd: UpdateDataContractData) : Result<IntegrationEvent list, string> =
        if state.Id.IsNone then
            Error "Integration does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted integration"
        else
            if state.DataContract = cmd.DataContract then
                Ok [] // No change
            else
                Ok [DataContractUpdated {
                    Id = cmd.Id
                    OldDataContract = state.DataContract
                    NewDataContract = cmd.DataContract
                }]
    
    /// Handle SetSourceApp command
    let handleSetSourceApp (state: IntegrationAggregate) (cmd: SetSourceAppData) : Result<IntegrationEvent list, string> =
        if state.Id.IsNone then
            Error "Integration does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted integration"
        elif String.IsNullOrWhiteSpace(cmd.NewSourceAppId) then
            Error "Source application ID cannot be empty"
        elif Some cmd.NewSourceAppId = state.TargetAppId then
            Error "Source and target applications must be different"
        else
            match state.SourceAppId with
            | None -> Error "Integration has no current source application"
            | Some currentSourceAppId ->
                if currentSourceAppId = cmd.NewSourceAppId then
                    Ok [] // No change
                else
                    Ok [SourceAppSet {
                        Id = cmd.Id
                        OldSourceAppId = currentSourceAppId
                        NewSourceAppId = cmd.NewSourceAppId
                    }]
    
    /// Handle SetTargetApp command
    let handleSetTargetApp (state: IntegrationAggregate) (cmd: SetTargetAppData) : Result<IntegrationEvent list, string> =
        if state.Id.IsNone then
            Error "Integration does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted integration"
        elif String.IsNullOrWhiteSpace(cmd.NewTargetAppId) then
            Error "Target application ID cannot be empty"
        elif Some cmd.NewTargetAppId = state.SourceAppId then
            Error "Source and target applications must be different"
        else
            match state.TargetAppId with
            | None -> Error "Integration has no current target application"
            | Some currentTargetAppId ->
                if currentTargetAppId = cmd.NewTargetAppId then
                    Ok [] // No change
                else
                    Ok [TargetAppSet {
                        Id = cmd.Id
                        OldTargetAppId = currentTargetAppId
                        NewTargetAppId = cmd.NewTargetAppId
                    }]
    
    /// Handle AddTags command
    let handleAddTags (state: IntegrationAggregate) (cmd: AddIntegrationTagsData) : Result<IntegrationEvent list, string> =
        if state.Id.IsNone then
            Error "Integration does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted integration"
        elif cmd.Tags.IsEmpty then
            Error "No tags to add"
        else
            let newTags = cmd.Tags |> List.filter (fun t -> not (List.contains t state.Tags))
            if newTags.IsEmpty then
                Ok [] // All tags already exist
            else
                Ok [IntegrationTagsAdded { Id = cmd.Id; AddedTags = newTags }]
    
    /// Handle RemoveTags command
    let handleRemoveTags (state: IntegrationAggregate) (cmd: RemoveIntegrationTagsData) : Result<IntegrationEvent list, string> =
        if state.Id.IsNone then
            Error "Integration does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted integration"
        elif cmd.Tags.IsEmpty then
            Error "No tags to remove"
        else
            let tagsToRemove = cmd.Tags |> List.filter (fun t -> List.contains t state.Tags)
            if tagsToRemove.IsEmpty then
                Ok [] // None of the tags exist
            else
                Ok [IntegrationTagsRemoved { Id = cmd.Id; RemovedTags = tagsToRemove }]
    
    /// Handle DeleteIntegration command
    let handleDeleteIntegration (state: IntegrationAggregate) (cmd: DeleteIntegrationData) : Result<IntegrationEvent list, string> =
        if state.Id.IsNone then
            Error "Integration does not exist"
        elif state.IsDeleted then
            Ok [] // Already deleted
        else
            Ok [IntegrationDeleted { Id = cmd.Id }]
    
    /// Main command handler that dispatches to specific handlers
    let handle (state: IntegrationAggregate) (command: IntegrationCommand) : Result<IntegrationEvent list, string> =
        match command with
        | CreateIntegration cmd -> handleCreateIntegration state cmd
        | UpdateProtocol cmd -> handleUpdateProtocol state cmd
        | SetSLA cmd -> handleSetSLA state cmd
        | SetFrequency cmd -> handleSetFrequency state cmd
        | UpdateDataContract cmd -> handleUpdateDataContract state cmd
        | SetSourceApp cmd -> handleSetSourceApp state cmd
        | SetTargetApp cmd -> handleSetTargetApp state cmd
        | AddIntegrationTags cmd -> handleAddTags state cmd
        | RemoveIntegrationTags cmd -> handleRemoveTags state cmd
        | DeleteIntegration cmd -> handleDeleteIntegration state cmd
