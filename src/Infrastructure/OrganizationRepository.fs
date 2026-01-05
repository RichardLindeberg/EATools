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

    let private mapOrganization (reader: SqliteDataReader) : Organization =
        let idIdx = reader.GetOrdinal("id")
        let nameIdx = reader.GetOrdinal("name")
        let domainsIdx = reader.GetOrdinal("domains")
        let contactsIdx = reader.GetOrdinal("contacts")
        let createdIdx = reader.GetOrdinal("created_at")
        let updatedIdx = reader.GetOrdinal("updated_at")

        {
            Id = reader.GetString(idIdx)
            Name = reader.GetString(nameIdx)
            Domains = reader.GetString(domainsIdx) |> deserializeList
            Contacts = reader.GetString(contactsIdx) |> deserializeList
            CreatedAt = reader.GetString(createdIdx)
            UpdatedAt = reader.GetString(updatedIdx)
        }

    let private buildFilters (search: string option) =
        let clauses = System.Collections.Generic.List<string>()
        let parameters = System.Collections.Generic.List<SqliteParameter>()

        match search with
        | Some term when not (String.IsNullOrWhiteSpace term) ->
            clauses.Add("name LIKE $search")
            parameters.Add(new SqliteParameter("$search", "%" + term + "%"))
        | _ -> ()

        let whereClause = if clauses.Count = 0 then "" else " WHERE " + String.Join(" AND ", clauses)
        whereClause, parameters

    let getAll (page: int) (limit: int) (search: string option) : PaginatedResponse<Organization> =
        let page = if page < 1 then 1 else page
        let limit = if limit < 1 || limit > 200 then 50 else limit
        let offset = (page - 1) * limit

        use conn = Database.getConnection ()
        let whereClause, parameters = buildFilters search

        use listCmd = conn.CreateCommand()
        listCmd.CommandText <- sprintf "SELECT id, name, domains, contacts, created_at, updated_at FROM organizations%s ORDER BY datetime(created_at) DESC LIMIT $limit OFFSET $offset" whereClause
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

    let getById (id: string) : Organization option =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT id, name, domains, contacts, created_at, updated_at FROM organizations WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore

        use reader = cmd.ExecuteReader()
        if reader.Read() then Some (mapOrganization reader) else None

    let create (req: CreateOrganizationRequest) : Organization =
        let id = generateId ()
        let now = getUtcTimestamp ()
        let domains = req.Domains
        let contacts = req.Contacts

        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <-
            """
            INSERT INTO organizations (id, name, domains, contacts, created_at, updated_at)
            VALUES ($id, $name, $domains, $contacts, $created_at, $updated_at)
            """
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        cmd.Parameters.AddWithValue("$name", req.Name) |> ignore
        cmd.Parameters.AddWithValue("$domains", serializeList domains) |> ignore
        cmd.Parameters.AddWithValue("$contacts", serializeList contacts) |> ignore
        cmd.Parameters.AddWithValue("$created_at", now) |> ignore
        cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

        cmd.ExecuteNonQuery() |> ignore

        { Id = id
          Name = req.Name
          Domains = domains
          Contacts = contacts
          CreatedAt = now
          UpdatedAt = now }

    let update (id: string) (req: CreateOrganizationRequest) : Organization option =
        match getById id with
        | Some existing ->
            let now = getUtcTimestamp ()
            let domains = req.Domains
            let contacts = req.Contacts

            use conn = Database.getConnection ()
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                UPDATE organizations
                SET name = $name,
                    domains = $domains,
                    contacts = $contacts,
                    updated_at = $updated_at
                WHERE id = $id
                """
            cmd.Parameters.AddWithValue("$id", id) |> ignore
            cmd.Parameters.AddWithValue("$name", req.Name) |> ignore
            cmd.Parameters.AddWithValue("$domains", serializeList domains) |> ignore
            cmd.Parameters.AddWithValue("$contacts", serializeList contacts) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

            let rows = cmd.ExecuteNonQuery()
            if rows > 0 then
                Some
                    { existing with
                        Name = req.Name
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
