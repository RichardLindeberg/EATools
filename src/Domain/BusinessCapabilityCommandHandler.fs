/// BusinessCapability command handlers with business logic validation
namespace EATool.Domain

open System

module BusinessCapabilityCommandHandler =
    
    /// Check for cycles in parent hierarchy (requires external fetch function)
    let private checkForCycles (capabilityId: string) (parentId: string) (getCapability: string -> BusinessCapability option) : Result<unit, string> =
        if capabilityId = parentId then
            Error "Business capability cannot be its own parent"
        else
            let rec walkHierarchy currentId visited =
                if Set.contains currentId visited then
                    Error $"Cycle detected in business capability hierarchy"
                elif currentId = capabilityId then
                    Error $"Setting this parent would create a cycle"
                else
                    match getCapability currentId with
                    | None -> Ok ()
                    | Some cap ->
                        match cap.ParentId with
                        | None -> Ok ()
                        | Some pid -> walkHierarchy pid (Set.add currentId visited)
            
            walkHierarchy parentId Set.empty
    
    /// Handle CreateCapability command (requires getCapability function for parent validation)
    let handleCreateCapability (state: BusinessCapabilityAggregate) (cmd: CreateCapabilityData) (getCapability: string -> BusinessCapability option) : Result<BusinessCapabilityEvent list, string> =
        match state with
        | Active _ -> Error "Business capability already exists"
        | Deleted -> Error "Cannot recreate deleted business capability"
        | Initial ->
            if String.IsNullOrWhiteSpace(cmd.Name) then
                Error "Business capability name is required"
            elif String.IsNullOrWhiteSpace(cmd.Id) then
                Error "Business capability ID is required"
            else
                // Validate parent exists if specified
                match cmd.ParentId with
                | Some parentId ->
                    match getCapability parentId with
                    | None -> Error $"Parent business capability {parentId} does not exist"
                    | Some _ ->
                        Ok [CapabilityCreated {
                            Id = cmd.Id
                            Name = cmd.Name
                            ParentId = cmd.ParentId
                            Description = cmd.Description
                        }]
                | None ->
                    Ok [CapabilityCreated {
                        Id = cmd.Id
                        Name = cmd.Name
                        ParentId = cmd.ParentId
                        Description = cmd.Description
                    }]
    
    /// Handle SetParent command (requires getCapability function for cycle detection)
    let handleSetParent (state: BusinessCapabilityAggregate) (cmd: SetCapabilityParentData) (getCapability: string -> BusinessCapability option) : Result<BusinessCapabilityEvent list, string> =
        match state with
        | Initial -> Error "Business capability does not exist"
        | Deleted -> Error "Cannot modify deleted business capability"
        | Active current ->
            match getCapability cmd.ParentId with
            | None -> Error $"Parent business capability {cmd.ParentId} does not exist"
            | Some _ ->
                match checkForCycles cmd.Id cmd.ParentId getCapability with
                | Error e -> Error e
                | Ok () ->
                    if current.ParentId = Some cmd.ParentId then
                        Ok [] // No change
                    else
                        Ok [CapabilityParentAssigned {
                            Id = cmd.Id
                            OldParentId = current.ParentId
                            NewParentId = cmd.ParentId
                        }]
    
    /// Handle RemoveParent command
    let handleRemoveParent (state: BusinessCapabilityAggregate) (cmd: RemoveCapabilityParentData) : Result<BusinessCapabilityEvent list, string> =
        match state with
        | Initial -> Error "Business capability does not exist"
        | Deleted -> Error "Cannot modify deleted business capability"
        | Active current ->
            match current.ParentId with
            | None -> Ok [] // Already no parent
            | Some oldParentId ->
                Ok [CapabilityParentRemoved {
                    Id = cmd.Id
                    OldParentId = oldParentId
                }]
    
    /// Handle UpdateDescription command
    let handleUpdateDescription (state: BusinessCapabilityAggregate) (cmd: UpdateCapabilityDescriptionData) : Result<BusinessCapabilityEvent list, string> =
        match state with
        | Initial -> Error "Business capability does not exist"
        | Deleted -> Error "Cannot modify deleted business capability"
        | Active current ->
            if current.Description = cmd.Description then
                Ok [] // No change
            else
                Ok [CapabilityDescriptionUpdated {
                    Id = cmd.Id
                    OldDescription = current.Description
                    NewDescription = cmd.Description
                }]
    
    /// Handle DeleteCapability command
    let handleDeleteCapability (state: BusinessCapabilityAggregate) (cmd: DeleteCapabilityData) : Result<BusinessCapabilityEvent list, string> =
        match state with
        | Initial -> Error "Business capability does not exist"
        | Deleted -> Error "Business capability already deleted"
        | Active _ ->
            Ok [CapabilityDeleted {
                Id = cmd.Id
            }]
