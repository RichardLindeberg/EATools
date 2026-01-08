/// Business capability repository backed by SQLite
namespace EATool.Infrastructure

open System
open Microsoft.Data.Sqlite
open EATool.Domain
open EATool.Infrastructure.Validation

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
        let descIdx = reader.GetOrdinal("description")
        let createdIdx = reader.GetOrdinal("created_at")
        let updatedIdx = reader.GetOrdinal("updated_at")

        {
            Id = reader.GetString(idIdx)
            Name = reader.GetString(nameIdx)
            ParentId = getStringOption reader parentIdx
            Description = getStringOption reader descIdx
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
        listCmd.CommandText <- sprintf "SELECT id, name, parent_id, description, created_at, updated_at FROM business_capabilities%s ORDER BY datetime(created_at) DESC LIMIT $limit OFFSET $offset" whereClause
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
        cmd.CommandText <- "SELECT id, name, parent_id, description, created_at, updated_at FROM business_capabilities WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore

        use reader = cmd.ExecuteReader()
        if reader.Read() then Some (mapCapability reader) else None

    /// Check if a capability name already exists under the same parent (unique within parent scope)
    let capNameExistsUnderParent (name: string) (parentId: string option) (excludeId: string option) : bool =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        
        let baseQuery = "SELECT COUNT(1) FROM business_capabilities WHERE name = $name"
        let parentClause = 
            match parentId with
            | None -> " AND parent_id IS NULL"
            | Some pid -> " AND parent_id = $parent_id"
        
        let excludeClause =
            match excludeId with
            | Some id -> " AND id != $id"
            | None -> ""
        
        cmd.CommandText <- baseQuery + parentClause + excludeClause
        cmd.Parameters.AddWithValue("$name", name) |> ignore
        
        match parentId with
        | Some pid -> cmd.Parameters.AddWithValue("$parent_id", pid) |> ignore
        | None -> ()
        
        match excludeId with
        | Some id -> cmd.Parameters.AddWithValue("$id", id) |> ignore
        | None -> ()
        
        let count = cmd.ExecuteScalar() :?> int64
        count > 0L

    /// Check if setting a new parent would create a cycle
    let wouldCreateCycle (childId: string) (newParentId: string option) : bool =
        CycleDetection.wouldCreateCycleBusCapability childId newParentId getById

    let create (req: CreateBusinessCapabilityRequest) : BusinessCapability =
        // Check for duplicate name under same parent
        if capNameExistsUnderParent req.Name req.ParentId None then
            let parentDesc = match req.ParentId with None -> "root" | Some pid -> $"parent {pid}"
            failwith $"BusinessCapability with name '{req.Name}' already exists under {parentDesc}"
        
        // Check for cycles if setting a parent
        if wouldCreateCycle (Guid.NewGuid().ToString()) req.ParentId then
            failwith "Cannot set parent: would create circular parent reference"
        
        let id = generateId ()
        let now = getUtcTimestamp ()

        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <-
            """
            INSERT INTO business_capabilities (id, name, parent_id, description, created_at, updated_at)
            VALUES ($id, $name, $parent_id, $description, $created_at, $updated_at)
            """
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        cmd.Parameters.AddWithValue("$name", req.Name) |> ignore
        addOptionalParam cmd "$parent_id" (req.ParentId |> Option.map box)
        addOptionalParam cmd "$description" (req.Description |> Option.map box)
        cmd.Parameters.AddWithValue("$created_at", now) |> ignore
        cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

        cmd.ExecuteNonQuery() |> ignore

        { Id = id
          Name = req.Name
          ParentId = req.ParentId
          Description = req.Description
          CreatedAt = now
          UpdatedAt = now }

    let update (id: string) (req: CreateBusinessCapabilityRequest) : BusinessCapability option =
        match getById id with
        | Some existing ->
            // Check for duplicate name under same parent (excluding current record)
            if capNameExistsUnderParent req.Name req.ParentId (Some id) then
                let parentDesc = match req.ParentId with None -> "root" | Some pid -> $"parent {pid}"
                failwith $"BusinessCapability with name '{req.Name}' already exists under {parentDesc}"
            
            // Check for cycles if parent changed
            if existing.ParentId <> req.ParentId && wouldCreateCycle id req.ParentId then
                failwith "Cannot set parent: would create circular parent reference"
            
            let now = getUtcTimestamp ()

            use conn = Database.getConnection ()
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                UPDATE business_capabilities
                SET name = $name,
                    parent_id = $parent_id,
                    description = $description,
                    updated_at = $updated_at
                WHERE id = $id
                """
            cmd.Parameters.AddWithValue("$id", id) |> ignore
            cmd.Parameters.AddWithValue("$name", req.Name) |> ignore
            addOptionalParam cmd "$parent_id" (req.ParentId |> Option.map box)
            addOptionalParam cmd "$description" (req.Description |> Option.map box)
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

            let rows = cmd.ExecuteNonQuery()
            if rows > 0 then
                Some { existing with Name = req.Name; ParentId = req.ParentId; Description = req.Description; UpdatedAt = now }
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
