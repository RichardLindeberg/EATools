/// Organization repository and database operations
namespace EATool.Infrastructure

open System
open EATool.Domain

/// In-memory storage for development/testing
/// TODO: Replace with actual database implementation
module OrganizationRepository =
    
    let private organizations = System.Collections.Generic.Dictionary<string, Organization>()
    
    /// Generate a unique ID for organizations
    let private generateId () = 
        let guid = Guid.NewGuid().ToString("N")
        "org-" + guid.Substring(0, 8)
    
    /// Get current UTC timestamp in ISO 8601 format
    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")
    
    /// Get all organizations with pagination and optional search
    let getAll (page: int) (limit: int) (search: string option) : PaginatedResponse<Organization> =
        let page = if page < 1 then 1 else page
        let limit = if limit < 1 || limit > 200 then 50 else limit
        
        let filtered =
            organizations.Values
            |> Seq.toList
            |> match search with
               | Some searchTerm ->
                   List.filter (fun org ->
                       org.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
               | None -> id
        
        let total = List.length filtered
        let items =
            filtered
            |> List.skip ((page - 1) * limit)
            |> List.truncate limit
        
        {
            Items = items
            Page = page
            Limit = limit
            Total = total
        }
    
    /// Get organization by ID
    let getById (id: string) : Organization option =
        if organizations.ContainsKey(id) then
            Some organizations.[id]
        else
            None
    
    /// Create a new organization
    let create (req: CreateOrganizationRequest) : Organization =
        let id = generateId ()
        let now = getUtcTimestamp ()
        
        let org =
            {
                Id = id
                Name = req.Name
                Domains = req.Domains |> Option.defaultValue []
                Contacts = req.Contacts |> Option.defaultValue []
                CreatedAt = now
                UpdatedAt = now
            }
        
        organizations.[id] <- org
        org
    
    /// Update an organization
    let update (id: string) (req: CreateOrganizationRequest) : Organization option =
        match getById id with
        | Some existing ->
            let now = getUtcTimestamp ()
            let updated =
                {
                    existing with
                        Name = req.Name
                        Domains = req.Domains |> Option.defaultValue existing.Domains
                        Contacts = req.Contacts |> Option.defaultValue existing.Contacts
                        UpdatedAt = now
                }
            organizations.[id] <- updated
            Some updated
        | None -> None
    
    /// Delete an organization
    let delete (id: string) : bool =
        organizations.Remove(id)
    
    /// Clear all organizations (for testing)
    let clear () =
        organizations.Clear()
