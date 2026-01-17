/// Data entity command handlers with validation
namespace EATool.Domain

open System
open System.Text.RegularExpressions

module DataEntityCommandHandler =

    /// Validate classification enum
    let private validateClassification (classification: string) : Result<unit, string> =
        match classification.ToLowerInvariant() with
        | "public" | "internal" | "confidential" | "restricted" -> Ok ()
        | _ -> Error "Invalid classification. Must be one of: public, internal, confidential, restricted"

    /// Validate retention format (e.g., "7 years", "90 days", "1y", "6m")
    let private validateRetention (retention: string option) : Result<unit, string> =
        match retention with
        | None -> Ok ()
        | Some s ->
            let trimmed = s.Trim()
            if String.length trimmed = 0 then
                Error "Retention cannot be empty string"
            else if Regex.IsMatch(trimmed, @"^\d+\s*(y|year|years|m|month|months|d|day|days|h|hour|hours|w|week|weeks)$", RegexOptions.IgnoreCase) then
                Ok ()
            else if Regex.IsMatch(trimmed, @"^\d+[ymdhw]$", RegexOptions.IgnoreCase) then
                Ok ()
            else
                Error $"Invalid retention format '{trimmed}'. Use formats like '7 years', '90 days', '1y', '6m', '12 months'"

    /// Handle CreateDataEntity command
    let handleCreateDataEntity (state: DataEntityAggregate) (cmd: CreateDataEntityData) : Result<DataEntityEvent list, string> =
        if state.Id.IsSome then
            Error "Data entity already exists"
        else
            if String.IsNullOrWhiteSpace(cmd.Name) then
                Error "Name is required"
            elif String.length cmd.Name > 255 then
                Error "Name cannot exceed 255 characters"
            else
                match validateClassification cmd.Classification with
                | Error e -> Error e
                | Ok () ->
                    match validateRetention cmd.Retention with
                    | Error e -> Error e
                    | Ok () ->
                        let evt = DataEntityCreated {
                            Id = cmd.Id
                            Name = cmd.Name
                            Domain = cmd.Domain
                            Classification = cmd.Classification
                            Retention = cmd.Retention
                            Owner = cmd.Owner
                            Steward = cmd.Steward
                            SourceSystem = cmd.SourceSystem
                            Criticality = cmd.Criticality
                            PiiFlag = cmd.PiiFlag
                            Tags = cmd.Tags
                        }
                        Ok [evt]

    /// Handle SetClassification command
    let handleSetClassification (state: DataEntityAggregate) (cmd: SetClassificationData) : Result<DataEntityEvent list, string> =
        if state.Id.IsNone then
            Error "Data entity does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted data entity"
        else
            match validateClassification cmd.NewClassification with
            | Error e -> Error e
            | Ok () ->
                if cmd.OldClassification = cmd.NewClassification then
                    Error "New classification must be different from current"
                else
                    let evt = ClassificationSet {
                        Id = cmd.Id
                        OldClassification = cmd.OldClassification
                        NewClassification = cmd.NewClassification
                    }
                    Ok [evt]

    /// Handle SetPIIFlag command
    let handleSetPIIFlag (state: DataEntityAggregate) (cmd: SetPIIFlagData) : Result<DataEntityEvent list, string> =
        if state.Id.IsNone then
            Error "Data entity does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted data entity"
        else
            if cmd.OldPiiFlag = cmd.NewPiiFlag then
                Error "New PII flag must be different from current"
            else
                let evt = PIIFlagSet {
                    Id = cmd.Id
                    OldPiiFlag = cmd.OldPiiFlag
                    NewPiiFlag = cmd.NewPiiFlag
                }
                Ok [evt]

    /// Handle UpdateRetention command
    let handleUpdateRetention (state: DataEntityAggregate) (cmd: UpdateRetentionData) : Result<DataEntityEvent list, string> =
        if state.Id.IsNone then
            Error "Data entity does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted data entity"
        else
            match validateRetention cmd.NewRetention with
            | Error e -> Error e
            | Ok () ->
                if state.Retention = cmd.NewRetention then
                    Error "New retention must be different from current"
                else
                    let evt = RetentionUpdated {
                        Id = cmd.Id
                        OldRetention = cmd.OldRetention
                        NewRetention = cmd.NewRetention
                    }
                    Ok [evt]

    /// Handle AddDataEntityTags command
    let handleAddDataEntityTags (state: DataEntityAggregate) (cmd: AddDataEntityTagsData) : Result<DataEntityEvent list, string> =
        if state.Id.IsNone then
            Error "Data entity does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted data entity"
        else
            if List.isEmpty cmd.AddedTags then
                Error "No tags to add"
            else
                // Check if any new tags are being added (not already in current tags)
                let newTagsOnly = cmd.AddedTags |> List.filter (fun t -> not (List.contains t state.Tags))
                if List.isEmpty newTagsOnly then
                    Error "All tags already present on data entity"
                else
                    let evt = DataEntityTagsAdded {
                        Id = cmd.Id
                        AddedTags = newTagsOnly
                    }
                    Ok [evt]

    /// Handle DeleteDataEntity command
    let handleDeleteDataEntity (state: DataEntityAggregate) (cmd: DeleteDataEntityData) : Result<DataEntityEvent list, string> =
        if state.Id.IsNone then
            Error "Data entity does not exist"
        elif state.IsDeleted then
            Error "Data entity already deleted"
        else
            let evt = DataEntityDeleted {
                Id = cmd.Id
            }
            Ok [evt]

    /// Main command handler dispatcher
    let handle (state: DataEntityAggregate) (command: DataEntityCommand) : Result<DataEntityEvent list, string> =
        match command with
        | CreateDataEntity cmd -> handleCreateDataEntity state cmd
        | SetClassification cmd -> handleSetClassification state cmd
        | SetPIIFlag cmd -> handleSetPIIFlag state cmd
        | UpdateRetention cmd -> handleUpdateRetention state cmd
        | AddDataEntityTags cmd -> handleAddDataEntityTags state cmd
        | DeleteDataEntity cmd -> handleDeleteDataEntity state cmd
