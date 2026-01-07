/// Organization command handlers with business logic validation
namespace EATool.Domain

open System
open System.Text.RegularExpressions

module OrganizationCommandHandler =
    
    /// Validate domain name format (basic DNS validation)
    let private validateDomain (domain: string) : Result<unit, string> =
        if String.IsNullOrWhiteSpace(domain) then
            Error "Domain cannot be empty"
        else
            let pattern = @"^([a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$"
            let regex = Regex(pattern)
            if regex.IsMatch(domain) then
                Ok ()
            else
                Error $"Invalid domain format: {domain}"
    
    /// Validate email format
    let private validateEmail (email: string) : Result<unit, string> =
        if String.IsNullOrWhiteSpace(email) then
            Error "Email cannot be empty"
        else
            let pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"
            let regex = Regex(pattern)
            if regex.IsMatch(email) then
                Ok ()
            else
                Error $"Invalid email format: {email}"
    
    /// Check for cycles in parent hierarchy (requires external fetch function)
    let private checkForCycles (orgId: string) (parentId: string) (getOrganization: string -> Organization option) : Result<unit, string> =
        if orgId = parentId then
            Error "Organization cannot be its own parent"
        else
            let rec walkHierarchy currentId visited =
                if Set.contains currentId visited then
                    Error $"Cycle detected in organization hierarchy"
                elif currentId = orgId then
                    Error $"Setting this parent would create a cycle"
                else
                    match getOrganization currentId with
                    | None -> Ok ()
                    | Some org ->
                        match org.ParentId with
                        | None -> Ok ()
                        | Some pid -> walkHierarchy pid (Set.add currentId visited)
            
            walkHierarchy parentId Set.empty
    
    /// Handle CreateOrganization command (requires getOrganization function for parent validation)
    let handleCreateOrganization (state: OrganizationAggregate) (cmd: CreateOrganizationData) (getOrganization: string -> Organization option) : Result<OrganizationEvent list, string> =
        if state.Id.IsSome then
            Error "Organization already exists"
        elif String.IsNullOrWhiteSpace(cmd.Name) then
            Error "Organization name is required"
        elif String.IsNullOrWhiteSpace(cmd.Id) then
            Error "Organization ID is required"
        else
            // Validate parent exists if specified
            match cmd.ParentId with
            | Some parentId ->
                match getOrganization parentId with
                | None -> Error $"Parent organization {parentId} does not exist"
                | Some _ ->
                    // Validate domains
                    let domainResults = cmd.Domains |> List.map validateDomain
                    match domainResults |> List.tryFind (function Error _ -> true | Ok _ -> false) with
                    | Some (Error e) -> Error e
                    | _ ->
                        // Validate contacts
                        let contactResults = cmd.Contacts |> List.map validateEmail
                        match contactResults |> List.tryFind (function Error _ -> true | Ok _ -> false) with
                        | Some (Error e) -> Error e
                        | _ ->
                            Ok [OrganizationCreated {
                                Id = cmd.Id
                                Name = cmd.Name
                                ParentId = cmd.ParentId
                                Domains = cmd.Domains
                                Contacts = cmd.Contacts
                            }]
            | None ->
                // Validate domains
                let domainResults = cmd.Domains |> List.map validateDomain
                match domainResults |> List.tryFind (function Error _ -> true | Ok _ -> false) with
                | Some (Error e) -> Error e
                | _ ->
                    // Validate contacts
                    let contactResults = cmd.Contacts |> List.map validateEmail
                    match contactResults |> List.tryFind (function Error _ -> true | Ok _ -> false) with
                    | Some (Error e) -> Error e
                    | _ ->
                        Ok [OrganizationCreated {
                            Id = cmd.Id
                            Name = cmd.Name
                            ParentId = cmd.ParentId
                            Domains = cmd.Domains
                            Contacts = cmd.Contacts
                        }]
    
    /// Handle SetParent command (requires getOrganization function for cycle detection)
    let handleSetParent (state: OrganizationAggregate) (cmd: SetParentData) (getOrganization: string -> Organization option) : Result<OrganizationEvent list, string> =
        if state.Id.IsNone then
            Error "Organization does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted organization"
        else
            match getOrganization cmd.ParentId with
            | None -> Error $"Parent organization {cmd.ParentId} does not exist"
            | Some _ ->
                match checkForCycles cmd.Id cmd.ParentId getOrganization with
                | Error e -> Error e
                | Ok () ->
                    if state.ParentId = Some cmd.ParentId then
                        Ok [] // No change
                    else
                        Ok [ParentAssigned {
                            Id = cmd.Id
                            OldParentId = state.ParentId
                            NewParentId = cmd.ParentId
                        }]
    
    /// Handle RemoveParent command
    let handleRemoveParent (state: OrganizationAggregate) (cmd: RemoveParentData) : Result<OrganizationEvent list, string> =
        if state.Id.IsNone then
            Error "Organization does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted organization"
        else
            match state.ParentId with
            | None -> Ok [] // Already has no parent
            | Some oldParent ->
                Ok [ParentRemoved {
                    Id = cmd.Id
                    OldParentId = oldParent
                }]
    
    /// Handle UpdateContactInfo command
    let handleUpdateContactInfo (state: OrganizationAggregate) (cmd: UpdateContactInfoData) : Result<OrganizationEvent list, string> =
        if state.Id.IsNone then
            Error "Organization does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted organization"
        else
            // Validate all contacts
            let contactResults = cmd.Contacts |> List.map validateEmail
            match contactResults |> List.tryFind (function Error _ -> true | Ok _ -> false) with
            | Some (Error e) -> Error e
            | _ ->
                if state.Contacts = cmd.Contacts then
                    Ok [] // No change
                else
                    Ok [ContactInfoUpdated {
                        Id = cmd.Id
                        OldContacts = state.Contacts
                        NewContacts = cmd.Contacts
                    }]
    
    /// Handle AddDomain command
    let handleAddDomain (state: OrganizationAggregate) (cmd: AddDomainData) : Result<OrganizationEvent list, string> =
        if state.Id.IsNone then
            Error "Organization does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted organization"
        else
            match validateDomain cmd.Domain with
            | Error e -> Error e
            | Ok () ->
                if List.contains cmd.Domain state.Domains then
                    Ok [] // Domain already exists
                else
                    Ok [DomainAdded {
                        Id = cmd.Id
                        Domain = cmd.Domain
                    }]
    
    /// Handle RemoveDomain command
    let handleRemoveDomain (state: OrganizationAggregate) (cmd: RemoveDomainData) : Result<OrganizationEvent list, string> =
        if state.Id.IsNone then
            Error "Organization does not exist"
        elif state.IsDeleted then
            Error "Cannot modify deleted organization"
        else
            if not (List.contains cmd.Domain state.Domains) then
                Ok [] // Domain doesn't exist
            else
                Ok [DomainRemoved {
                    Id = cmd.Id
                    Domain = cmd.Domain
                }]
    
    /// Handle DeleteOrganization command
    let handleDeleteOrganization (state: OrganizationAggregate) (cmd: DeleteOrganizationData) : Result<OrganizationEvent list, string> =
        if state.Id.IsNone then
            Error "Organization does not exist"
        elif state.IsDeleted then
            Error "Organization already deleted"
        elif String.IsNullOrWhiteSpace(cmd.Reason) then
            Error "Deletion reason is required"
        else
            Ok [OrganizationDeleted {
                Id = cmd.Id
                Reason = cmd.Reason
            }]
