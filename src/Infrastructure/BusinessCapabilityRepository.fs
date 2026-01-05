/// Business capability repository backed by SQLite
namespace EATool.Infrastructure

open System
open Microsoft.Data.Sqlite
open EATool.Domain

module BusinessCapabilityRepository =
    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "cap-" + guid.Substring(0, 8)

    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let private addOptionalParam (cmd: SqliteCommand) (name: string) (value: obj option) =
        let p = cmd.CreateParameter()
        p.ParameterName <- name
        p.Value <- value |> Option.defaultValue (box DBNull.Value)
        cmd.Parameters.Add(p) |> ignore

    let private getStringOption (reader: SqliteDataReader) (ordinal: int) =
        if reader.IsDBNull(ordinal) then None else Some(reader.GetString(ordinal))

    let private mapCapability (reader: SqliteDataReader) : BusinessCapability =
        let idIdx = reader.GetOrdinal("id")
        let nameIdx = reader.GetOrdinal("name")
        let parentIdx = reader.GetOrdinal("parent_id")
        let createdIdx = reader.GetOrdinal("created_at")
        let updatedIdx = reader.GetOrdinal("updated_at")

        {
            Id = reader.GetString(idIdx)
            Name = reader.GetString(nameIdx)
            ParentId = getStringOption reader parentIdx
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
        | Some pid when not (String.IsNullOrWhiteSpace pid) ->
            clauses.Add("parent_id = $parent")
            parameters.Add(new SqliteParameter("$parent", pid))
        | _ -> ()

        let whereClause = if clauses.Count = 0 then "" else " WHERE " + String.Join(" AND ", clauses)
        whereClause, parameters

    let getAll (page: int) (limit: int) (search: string option) (parentId: string option) : PaginatedResponse<BusinessCapability> =
        let page = if page < 1 then 1 else page
        let limit = if limit < 1 || limit > 200 then 50 else limit
        let offset = (page - 1) * limit

        use conn = Database.getConnection ()
        let whereClause, parameters = buildFilters search parentId

        use listCmd = conn.CreateCommand()
        listCmd.CommandText <- sprintf "SELECT id, name, parent_id, created_at, updated_at FROM business_capabilities%s ORDER BY datetime(created_at) DESC LIMIT $limit OFFSET $offset" whereClause
        parameters |> Seq.iter (fun p -> listCmd.Parameters.Add(p) |> ignore)
        listCmd.Parameters.AddWithValue("$limit", limit) |> ignore
        listCmd.Parameters.AddWithValue("$offset", offset) |> ignore

        use reader = listCmd.ExecuteReader()
        let items = [ while reader.Read() do mapCapability reader ]

        use countCmd = conn.CreateCommand()
        countCmd.CommandText <- sprintf "SELECT COUNT(1) FROM business_capabilities%s" whereClause
        parameters |> Seq.iter (fun p -> countCmd.Parameters.Add(new SqliteParameter(p.ParameterName, p.Value)) |> ignore)
        let total = countCmd.ExecuteScalar() :?> int64 |> int

        { Items = items; Page = page; Limit = limit; Total = total }

    let getById (id: string) : BusinessCapability option =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT id, name, parent_id, created_at, updated_at FROM business_capabilities WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore

        use reader = cmd.ExecuteReader()
        if reader.Read() then Some (mapCapability reader) else None

    let create (req: CreateBusinessCapabilityRequest) : BusinessCapability =
        let id = generateId ()
        let now = getUtcTimestamp ()

        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <-
            """
            INSERT INTO business_capabilities (id, name, parent_id, created_at, updated_at)
            VALUES ($id, $name, $parent_id, $created_at, $updated_at)
            """
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        cmd.Parameters.AddWithValue("$name", req.Name) |> ignore
        addOptionalParam cmd "$parent_id" (req.ParentId |> Option.map box)
        cmd.Parameters.AddWithValue("$created_at", now) |> ignore
        cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

        cmd.ExecuteNonQuery() |> ignore

        { Id = id
          Name = req.Name
          ParentId = req.ParentId
          CreatedAt = now
          UpdatedAt = now }

    let update (id: string) (req: CreateBusinessCapabilityRequest) : BusinessCapability option =
        match getById id with
        | Some existing ->
            let now = getUtcTimestamp ()

            use conn = Database.getConnection ()
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                UPDATE business_capabilities
                SET name = $name,
                    parent_id = $parent_id,
                    updated_at = $updated_at
                WHERE id = $id
                """
            cmd.Parameters.AddWithValue("$id", id) |> ignore
            cmd.Parameters.AddWithValue("$name", req.Name) |> ignore
            addOptionalParam cmd "$parent_id" (req.ParentId |> Option.map box)
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

            let rows = cmd.ExecuteNonQuery()
            if rows > 0 then
                Some { existing with Name = req.Name; ParentId = req.ParentId; UpdatedAt = now }
            else None
        | None -> None

    let delete (id: string) : bool =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "DELETE FROM business_capabilities WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        cmd.ExecuteNonQuery() > 0

    let clear () =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "DELETE FROM business_capabilities"
        cmd.ExecuteNonQuery() |> ignore
