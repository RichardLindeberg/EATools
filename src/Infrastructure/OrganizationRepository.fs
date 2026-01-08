/// Organization repository backed by SQLite
namespace EATool.Infrastructure

open System
open System.Text.Json
open Microsoft.Data.Sqlite
open EATool.Domain

module OrganizationRepository =

    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "org-" + guid.Substring(0, 8)

    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let private serializeList (items: string list) = JsonSerializer.Serialize(items)
    let private deserializeList (payload: string) =
        try JsonSerializer.Deserialize<string list>(payload) with _ -> []

    /// Validate all domains in the list
    let private validateDomains (domains: string list) : Result<unit, string> =
        domains
        |> List.tryFind (fun d -> DnsValidator.validate d |> Option.isSome)
        |> function
            | Some invalidDomain ->
                match DnsValidator.validate invalidDomain with
                | Some error -> Error error
                | None -> Ok ()
            | None -> Ok ()

    /// Validate all contacts in the list
    let private validateContacts (contacts: string list) : Result<unit, string> =
        contacts
        |> List.tryFind (fun c -> EmailValidator.validate c |> Option.isSome)
        |> function
            | Some invalidEmail ->
                match EmailValidator.validate invalidEmail with
                | Some error -> Error error
                | None -> Ok ()
            | None -> Ok ()

    let private mapOrganization (reader: SqliteDataReader) : Organization =
        let idIdx = reader.GetOrdinal("id")
        let nameIdx = reader.GetOrdinal("name")
        let parentIdIdx = reader.GetOrdinal("parent_id")
        let domainsIdx = reader.GetOrdinal("domains")
        let contactsIdx = reader.GetOrdinal("contacts")
        let createdIdx = reader.GetOrdinal("created_at")
        let updatedIdx = reader.GetOrdinal("updated_at")

        {
            Id = reader.GetString(idIdx)
            Name = reader.GetString(nameIdx)
            ParentId = if reader.IsDBNull(parentIdIdx) then None else Some (reader.GetString(parentIdIdx))
            Domains = reader.GetString(domainsIdx) |> deserializeList
            Contacts = reader.GetString(contactsIdx) |> deserializeList
            CreatedAt = reader.GetString(createdIdx)
            UpdatedAt = reader.GetString(updatedIdx)
        }

    let private buildFilters (search: string option) (parentId: string option) =
        let clauses = System.Collections.Generic.List<string>()
        let parameters = System.Collections.Generic.List<SqliteParameter>()

        match search with
        | Some term when not (String.IsNullOrWhiteSpace term) ->
            clauses.Add("name LIKE $search")
            parameters.Add(new SqliteParameter("$search", "%" + term + "%"))
        | _ -> ()

        match parentId with
        | Some id ->
            clauses.Add("parent_id = $parent_id")
            parameters.Add(new SqliteParameter("$parent_id", id))
        | None -> ()

        let whereClause = if clauses.Count = 0 then "" else " WHERE " + String.Join(" AND ", clauses)
        whereClause, parameters

    let rec getById (id: string) : Organization option =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT id, name, parent_id, domains, contacts, created_at, updated_at FROM organizations WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore

        use reader = cmd.ExecuteReader()
        if reader.Read() then Some (mapOrganization reader) else None

    /// Check if setting parent_id would create a cycle
    /// Returns true if the proposed parent is a descendant of the child
    and private wouldCreateCycle (childId: string) (newParentId: string option) : bool =
        match newParentId with
        | None -> false // Can always become a root
        | Some parentId when parentId = childId -> true // Self-reference
        | Some parentId ->
            // Walk up the parent chain from parentId
            let rec walkAncestors (currentId: string) (visited: Set<string>) : bool =
                if visited.Contains currentId then
                    false // Cycle already exists in DB, shouldn't happen
                else
                    let newVisited = visited.Add currentId
                    match getById currentId with
                    | Some org ->
                        match org.ParentId with
                        | Some pid when pid = childId -> true // Found child in ancestor chain = cycle
                        | Some pid -> walkAncestors pid newVisited // Continue walking up
                        | None -> false // Reached root, no cycle
                    | None -> false // Parent doesn't exist

            walkAncestors parentId Set.empty


    let getAll (page: int) (limit: int) (search: string option) (parentId: string option) : PaginatedResponse<Organization> =
        let page = if page < 1 then 1 else page
        let limit = if limit < 1 || limit > 200 then 50 else limit
        let offset = (page - 1) * limit

        use conn = Database.getConnection ()
        let whereClause, parameters = buildFilters search parentId

        use listCmd = conn.CreateCommand()
        listCmd.CommandText <- sprintf "SELECT id, name, parent_id, domains, contacts, created_at, updated_at FROM organizations%s ORDER BY datetime(created_at) DESC LIMIT $limit OFFSET $offset" whereClause
        parameters |> Seq.iter (fun p -> listCmd.Parameters.Add(p) |> ignore)
        listCmd.Parameters.AddWithValue("$limit", limit) |> ignore
        listCmd.Parameters.AddWithValue("$offset", offset) |> ignore


        use reader = listCmd.ExecuteReader()
        let items = [ while reader.Read() do mapOrganization reader ]

        use countCmd = conn.CreateCommand()
        countCmd.CommandText <- sprintf "SELECT COUNT(1) FROM organizations%s" whereClause
        parameters |> Seq.iter (fun p -> countCmd.Parameters.Add(new SqliteParameter(p.ParameterName, p.Value)) |> ignore)
        let total = countCmd.ExecuteScalar() :?> int64 |> int

        { Items = items; Page = page; Limit = limit; Total = total }

    let createWithValidation (req: CreateOrganizationRequest) : Result<Organization, string> =
        match validateDomains req.Domains, validateContacts req.Contacts with
        | Error domainErr, _ -> Error domainErr
        | _, Error contactErr -> Error contactErr
        | Ok (), Ok () ->
            let id = generateId ()
            let now = getUtcTimestamp ()
            let domains = req.Domains
            let contacts = req.Contacts

            use conn = Database.getConnection ()
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                INSERT INTO organizations (id, name, parent_id, domains, contacts, created_at, updated_at)
                VALUES ($id, $name, $parent_id, $domains, $contacts, $created_at, $updated_at)
                """
            cmd.Parameters.AddWithValue("$id", id) |> ignore
            cmd.Parameters.AddWithValue("$name", req.Name) |> ignore
            let parentParam = cmd.Parameters.Add(new SqliteParameter("$parent_id", match req.ParentId with Some pid -> pid :> obj | None -> DBNull.Value))
            cmd.Parameters.AddWithValue("$domains", serializeList domains) |> ignore
            cmd.Parameters.AddWithValue("$contacts", serializeList contacts) |> ignore
            cmd.Parameters.AddWithValue("$created_at", now) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

            cmd.ExecuteNonQuery() |> ignore

            Ok { Id = id
                 Name = req.Name
                 ParentId = req.ParentId
                 Domains = domains
                 Contacts = contacts
                 CreatedAt = now
                 UpdatedAt = now }

    let create (req: CreateOrganizationRequest) : Organization =
        let id = generateId ()
        let now = getUtcTimestamp ()
        let domains = req.Domains
        let contacts = req.Contacts

        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <-
            """
            INSERT INTO organizations (id, name, parent_id, domains, contacts, created_at, updated_at)
            VALUES ($id, $name, $parent_id, $domains, $contacts, $created_at, $updated_at)
            """
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        cmd.Parameters.AddWithValue("$name", req.Name) |> ignore
        let parentParam = cmd.Parameters.Add(new SqliteParameter("$parent_id", match req.ParentId with Some pid -> pid :> obj | None -> DBNull.Value))
        cmd.Parameters.AddWithValue("$domains", serializeList domains) |> ignore
        cmd.Parameters.AddWithValue("$contacts", serializeList contacts) |> ignore
        cmd.Parameters.AddWithValue("$created_at", now) |> ignore
        cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

        cmd.ExecuteNonQuery() |> ignore

        { Id = id
          Name = req.Name
          ParentId = req.ParentId
          Domains = domains
          Contacts = contacts
          CreatedAt = now
          UpdatedAt = now }

    let updateWithValidation (id: string) (req: CreateOrganizationRequest) : Result<Organization option, string> =
        match validateDomains req.Domains, validateContacts req.Contacts with
        | Error domainErr, _ -> Error domainErr
        | _, Error contactErr -> Error contactErr
        | Ok (), Ok () ->
            match getById id with
            | Some existing ->
                // Check if parent_id would create a cycle
                if wouldCreateCycle id req.ParentId then
                    Ok None // Reject circular reference
                else
                    // Check if new parent exists (if specified)
                    match req.ParentId with
                    | Some parentId ->
                        if getById parentId |> Option.isNone then
                            Ok None // Parent doesn't exist
                        else
                            let now = getUtcTimestamp ()
                            let domains = req.Domains
                            let contacts = req.Contacts

                            use conn = Database.getConnection ()
                            use cmd = conn.CreateCommand()
                            cmd.CommandText <-
                                """
                                UPDATE organizations
                                SET name = $name,
                                    parent_id = $parent_id,
                                    domains = $domains,
                                    contacts = $contacts,
                                    updated_at = $updated_at
                                WHERE id = $id
                                """
                            cmd.Parameters.AddWithValue("$id", id) |> ignore
                            cmd.Parameters.AddWithValue("$name", req.Name) |> ignore
                            cmd.Parameters.AddWithValue("$parent_id", parentId) |> ignore
                            cmd.Parameters.AddWithValue("$domains", serializeList domains) |> ignore
                            cmd.Parameters.AddWithValue("$contacts", serializeList contacts) |> ignore
                            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
                            let rows = cmd.ExecuteNonQuery()
                            if rows > 0 then
                                let updated = { existing with Name = req.Name; ParentId = Some parentId; Domains = domains; Contacts = contacts; UpdatedAt = now }
                                Ok (Some updated)
                            else
                                Ok None
                    | None ->
                        let now = getUtcTimestamp ()
                        let domains = req.Domains
                        let contacts = req.Contacts

                        use conn = Database.getConnection ()
                        use cmd = conn.CreateCommand()
                        cmd.CommandText <-
                            """
                            UPDATE organizations
                            SET name = $name,
                                parent_id = $parent_id,
                                domains = $domains,
                                contacts = $contacts,
                                updated_at = $updated_at
                            WHERE id = $id
                            """
                        cmd.Parameters.AddWithValue("$id", id) |> ignore
                        cmd.Parameters.AddWithValue("$name", req.Name) |> ignore
                        cmd.Parameters.AddWithValue("$parent_id", DBNull.Value) |> ignore
                        cmd.Parameters.AddWithValue("$domains", serializeList domains) |> ignore
                        cmd.Parameters.AddWithValue("$contacts", serializeList contacts) |> ignore
                        cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

                        let rows = cmd.ExecuteNonQuery()
                        if rows > 0 then
                            let updated = { existing with Name = req.Name; ParentId = None; Domains = domains; Contacts = contacts; UpdatedAt = now }
                            Ok (Some updated)
                        else
                            Ok None
            | None -> 
                Ok None

    let update (id: string) (req: CreateOrganizationRequest) : Organization option =
        match getById id with
        | Some existing ->
            // Check if parent_id would create a cycle
            if wouldCreateCycle id req.ParentId then
                None // Reject circular reference
            else
                // Check if new parent exists (if specified)
                match req.ParentId with
                | Some parentId ->
                    if getById parentId |> Option.isNone then
                        None // Parent doesn't exist
                    else
                        let now = getUtcTimestamp ()
                        let domains = req.Domains
                        let contacts = req.Contacts

                        use conn = Database.getConnection ()
                        use cmd = conn.CreateCommand()
                        cmd.CommandText <-
                            """
                            UPDATE organizations
                            SET name = $name,
                                parent_id = $parent_id,
                                domains = $domains,
                                contacts = $contacts,
                                updated_at = $updated_at
                            WHERE id = $id
                            """
                        cmd.Parameters.AddWithValue("$id", id) |> ignore
                        cmd.Parameters.AddWithValue("$name", req.Name) |> ignore
                        cmd.Parameters.AddWithValue("$parent_id", parentId) |> ignore
                        cmd.Parameters.AddWithValue("$domains", serializeList domains) |> ignore
                        cmd.Parameters.AddWithValue("$contacts", serializeList contacts) |> ignore
                        cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

                        let rows = cmd.ExecuteNonQuery()
                        if rows > 0 then
                            Some
                                { existing with
                                    Name = req.Name
                                    ParentId = Some parentId
                                    Domains = domains
                                    Contacts = contacts
                                    UpdatedAt = now }
                        else None
                | None ->
                    let now = getUtcTimestamp ()
                    let domains = req.Domains
                    let contacts = req.Contacts

                    use conn = Database.getConnection ()
                    use cmd = conn.CreateCommand()
                    cmd.CommandText <-
                        """
                        UPDATE organizations
                        SET name = $name,
                            parent_id = $parent_id,
                            domains = $domains,
                            contacts = $contacts,
                            updated_at = $updated_at
                        WHERE id = $id
                        """
                    cmd.Parameters.AddWithValue("$id", id) |> ignore
                    cmd.Parameters.AddWithValue("$name", req.Name) |> ignore
                    cmd.Parameters.AddWithValue("$parent_id", DBNull.Value) |> ignore
                    cmd.Parameters.AddWithValue("$domains", serializeList domains) |> ignore
                    cmd.Parameters.AddWithValue("$contacts", serializeList contacts) |> ignore
                    cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

                    let rows = cmd.ExecuteNonQuery()
                    if rows > 0 then
                        Some
                            { existing with
                                Name = req.Name
                                ParentId = None
                                Domains = domains
                                Contacts = contacts
                                UpdatedAt = now }
                    else None
        | None -> None

    let delete (id: string) : bool =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "DELETE FROM organizations WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        cmd.ExecuteNonQuery() > 0

    let clear () =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "DELETE FROM organizations"
        cmd.ExecuteNonQuery() |> ignore
