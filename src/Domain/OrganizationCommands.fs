/// Organization-specific commands and events
namespace EATool.Domain

open System

/// Organization Commands
type OrganizationCommand =
    | CreateOrganization of CreateOrganizationData
    | SetParent of SetParentData
    | RemoveParent of RemoveParentData
    | UpdateContactInfo of UpdateContactInfoData
    | AddDomain of AddDomainData
    | RemoveDomain of RemoveDomainData
    | DeleteOrganization of DeleteOrganizationData

and CreateOrganizationData =
    {
        Id: string
        Name: string
        ParentId: string option
        Domains: string list
        Contacts: string list
    }

and SetParentData =
    {
        Id: string
        ParentId: string
    }

and RemoveParentData =
    {
        Id: string
    }

and UpdateContactInfoData =
    {
        Id: string
        Contacts: string list
    }

and AddDomainData =
    {
        Id: string
        Domain: string
    }

and RemoveDomainData =
    {
        Id: string
        Domain: string
    }

and DeleteOrganizationData =
    {
        Id: string
        Reason: string
    }

/// Organization Events
type OrganizationEvent =
    | OrganizationCreated of OrganizationCreatedData
    | ParentAssigned of ParentAssignedData
    | ParentRemoved of ParentRemovedData
    | ContactInfoUpdated of ContactInfoUpdatedData
    | DomainAdded of DomainAddedData
    | DomainRemoved of DomainRemovedData
    | OrganizationDeleted of OrganizationDeletedData

and OrganizationCreatedData =
    {
        Id: string
        Name: string
        ParentId: string option
        Domains: string list
        Contacts: string list
    }

and ParentAssignedData =
    {
        Id: string
        OldParentId: string option
        NewParentId: string
    }

and ParentRemovedData =
    {
        Id: string
        OldParentId: string
    }

and ContactInfoUpdatedData =
    {
        Id: string
        OldContacts: string list
        NewContacts: string list
    }

and DomainAddedData =
    {
        Id: string
        Domain: string
    }

and DomainRemovedData =
    {
        Id: string
        Domain: string
    }

and OrganizationDeletedData =
    {
        Id: string
        Reason: string
    }

/// Organization aggregate state for event sourcing
type OrganizationAggregate =
    {
        Id: string option
        Name: string option
        ParentId: string option
        Domains: string list
        Contacts: string list
        IsDeleted: bool
    }
    static member Initial =
        {
            Id = None
            Name = None
            ParentId = None
            Domains = []
            Contacts = []
            IsDeleted = false
        }

module OrganizationAggregate =
    /// Apply an event to the aggregate
    let apply (state: OrganizationAggregate) (event: OrganizationEvent) : OrganizationAggregate =
        match event with
        | OrganizationCreated data ->
            { state with
                Id = Some data.Id
                Name = Some data.Name
                ParentId = data.ParentId
                Domains = data.Domains
                Contacts = data.Contacts }
        
        | ParentAssigned data ->
            { state with ParentId = Some data.NewParentId }
        
        | ParentRemoved _ ->
            { state with ParentId = None }
        
        | ContactInfoUpdated data ->
            { state with Contacts = data.NewContacts }
        
        | DomainAdded data ->
            { state with Domains = state.Domains @ [data.Domain] |> List.distinct }
        
        | DomainRemoved data ->
            { state with Domains = state.Domains |> List.filter ((<>) data.Domain) }
        
        | OrganizationDeleted _ ->
            { state with IsDeleted = true }
