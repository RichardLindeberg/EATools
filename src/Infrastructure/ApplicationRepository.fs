/// Application repository backed by SQLite
namespace EATool.Infrastructure

open System
open System.Text.Json
open Microsoft.Data.Sqlite
open EATool.Domain

module ApplicationRepository =
    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "app-" + guid.Substring(0, 8)

    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let private lifecycleToString (lc: Lifecycle) =
        match lc with
        | Lifecycle.Planned -> "planned"
        | Lifecycle.Active -> "active"
        | Lifecycle.Deprecated -> "deprecated"
        | Lifecycle.Retired -> "retired"

    let private lifecycleFromString (value: string) =
        match value.ToLowerInvariant() with
        | "planned" -> Lifecycle.Planned
        | "active" -> Lifecycle.Active
        | "deprecated"
        | "sunset" -> Lifecycle.Deprecated
        | "retired" -> Lifecycle.Retired
        | _ -> Lifecycle.Active

    let private deserializeTags (payload: string) =
        try
            JsonSerializer.Deserialize<string list>(payload)
        with
        | _ -> []

    let private serializeTags (tags: string list) =
        JsonSerializer.Serialize(tags)

    let private getStringOption (reader: SqliteDataReader) (ordinal: int) =
        if reader.IsDBNull(ordinal) then None else Some(reader.GetString(ordinal))

    let private addOptionalParam (cmd: SqliteCommand) (name: string) (value: obj option) =
        let p = cmd.CreateParameter()
        p.ParameterName <- name
        p.Value <- value |> Option.defaultValue (box DBNull.Value)
        cmd.Parameters.Add(p) |> ignore

    let private mapApplication (reader: SqliteDataReader) : Application =
        let idOrdinal = reader.GetOrdinal("id")
        let nameOrdinal = reader.GetOrdinal("name")
        let ownerOrdinal = reader.GetOrdinal("owner")
        let lifecycleOrdinal = reader.GetOrdinal("lifecycle")
        let capabilityOrdinal = reader.GetOrdinal("capability_id")
        let dataClassificationOrdinal = reader.GetOrdinal("data_classification")
        let tagsOrdinal = reader.GetOrdinal("tags")
        let createdOrdinal = reader.GetOrdinal("created_at")
        let updatedOrdinal = reader.GetOrdinal("updated_at")

        {
            Id = reader.GetString(idOrdinal)
            Name = reader.GetString(nameOrdinal)
            Owner = getStringOption reader ownerOrdinal
            Lifecycle = reader.GetString(lifecycleOrdinal) |> lifecycleFromString
            CapabilityId = getStringOption reader capabilityOrdinal
            DataClassification = getStringOption reader dataClassificationOrdinal
            Tags = reader.GetString(tagsOrdinal) |> deserializeTags
            CreatedAt = reader.GetString(createdOrdinal)
            UpdatedAt = reader.GetString(updatedOrdinal)
        }

    let private buildFilters (search: string option) (owner: string option) (lifecycle: Lifecycle option) =
        let clauses = System.Collections.Generic.List<string>()
        let parameters = System.Collections.Generic.List<SqliteParameter>()

        match search with
        | Some term when not (String.IsNullOrWhiteSpace term) ->
            clauses.Add("name LIKE $search")
            parameters.Add(new SqliteParameter("$search", "%" + term + "%"))
        | _ -> ()

        match owner with
        | Some o when not (String.IsNullOrWhiteSpace o) ->
            clauses.Add("owner = $owner")
            parameters.Add(new SqliteParameter("$owner", o))
        | _ -> ()

        match lifecycle with
        | Some lc ->
            clauses.Add("lifecycle = $lifecycle")
            parameters.Add(new SqliteParameter("$lifecycle", lifecycleToString lc))
        | None -> ()

        let whereClause =
            if clauses.Count = 0 then "" else " WHERE " + String.Join(" AND ", clauses)
        whereClause, parameters

    let getAll (page: int) (limit: int) (search: string option) (owner: string option) (lifecycle: Lifecycle option) : PaginatedResponse<Application> =
        let page = if page < 1 then 1 else page
        let limit = if limit < 1 || limit > 200 then 50 else limit
        let offset = (page - 1) * limit

        use conn = Database.getConnection ()
        let whereClause, parameters = buildFilters search owner lifecycle

        use listCmd = conn.CreateCommand()
        listCmd.CommandText <- sprintf "SELECT id, name, owner, lifecycle, lifecycle_raw, capability_id, data_classification, tags, created_at, updated_at FROM applications%s ORDER BY datetime(created_at) DESC LIMIT $limit OFFSET $offset" whereClause
        parameters |> Seq.iter (fun p -> listCmd.Parameters.Add(p) |> ignore)
        listCmd.Parameters.AddWithValue("$limit", limit) |> ignore
        listCmd.Parameters.AddWithValue("$offset", offset) |> ignore

        use reader = listCmd.ExecuteReader()
        let items =
            [
                while reader.Read() do
                    mapApplication reader
            ]

        use countCmd = conn.CreateCommand()
        countCmd.CommandText <- sprintf "SELECT COUNT(1) FROM applications%s" whereClause
        parameters |> Seq.iter (fun p -> countCmd.Parameters.Add(new SqliteParameter(p.ParameterName, p.Value)) |> ignore)
        let total = countCmd.ExecuteScalar() :?> int64 |> int

        { Items = items; Page = page; Limit = limit; Total = total }

    let getById (id: string) : Application option =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT id, name, owner, lifecycle, lifecycle_raw, capability_id, data_classification, tags, created_at, updated_at FROM applications WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore

        use reader = cmd.ExecuteReader()
        if reader.Read() then Some (mapApplication reader) else None

    let create (req: CreateApplicationRequest) : Application =
        let id = generateId ()
        let now = getUtcTimestamp ()
        let tags = req.Tags |> Option.defaultValue []
        let lifecycleValue = lifecycleToString req.Lifecycle

        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <-
            """
            INSERT INTO applications (id, name, owner, lifecycle, lifecycle_raw, capability_id, data_classification, tags, created_at, updated_at)
            VALUES ($id, $name, $owner, $lifecycle, $lifecycle_raw, $capability_id, $data_classification, $tags, $created_at, $updated_at)
            """
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        cmd.Parameters.AddWithValue("$name", req.Name) |> ignore
        addOptionalParam cmd "$owner" (req.Owner |> Option.map box)
        cmd.Parameters.AddWithValue("$lifecycle", lifecycleValue) |> ignore
        cmd.Parameters.AddWithValue("$lifecycle_raw", lifecycleValue) |> ignore
        addOptionalParam cmd "$capability_id" (req.CapabilityId |> Option.map box)
        addOptionalParam cmd "$data_classification" (req.DataClassification |> Option.map box)
        cmd.Parameters.AddWithValue("$tags", serializeTags tags) |> ignore
        cmd.Parameters.AddWithValue("$created_at", now) |> ignore
        cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

        cmd.ExecuteNonQuery() |> ignore

        { Id = id
          Name = req.Name
          Owner = req.Owner
          Lifecycle = req.Lifecycle
          CapabilityId = req.CapabilityId
          DataClassification = req.DataClassification
          Tags = tags
          CreatedAt = now
          UpdatedAt = now }

    let update (id: string) (req: CreateApplicationRequest) : Application option =
        match getById id with
        | Some existing ->
            let now = getUtcTimestamp ()
            let tags = req.Tags |> Option.defaultValue existing.Tags
            let lifecycleValue = lifecycleToString req.Lifecycle

            use conn = Database.getConnection ()
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                UPDATE applications
                SET name = $name,
                    owner = $owner,
                    lifecycle = $lifecycle,
                    lifecycle_raw = $lifecycle_raw,
                    capability_id = $capability_id,
                    data_classification = $data_classification,
                    tags = $tags,
                    updated_at = $updated_at
                WHERE id = $id
                """
            cmd.Parameters.AddWithValue("$id", id) |> ignore
            cmd.Parameters.AddWithValue("$name", req.Name) |> ignore
            addOptionalParam cmd "$owner" (req.Owner |> Option.map box)
            cmd.Parameters.AddWithValue("$lifecycle", lifecycleValue) |> ignore
            cmd.Parameters.AddWithValue("$lifecycle_raw", lifecycleValue) |> ignore
            addOptionalParam cmd "$capability_id" (req.CapabilityId |> Option.map box)
            addOptionalParam cmd "$data_classification" (req.DataClassification |> Option.map box)
            cmd.Parameters.AddWithValue("$tags", serializeTags tags) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

            let rows = cmd.ExecuteNonQuery()
            if rows > 0 then
                Some
                    { existing with
                        Name = req.Name
                        Owner = req.Owner
                        Lifecycle = req.Lifecycle
                        CapabilityId = req.CapabilityId
                        DataClassification = req.DataClassification
                        Tags = tags
                        UpdatedAt = now }
            else None
        | None -> None

    let delete (id: string) : bool =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "DELETE FROM applications WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        cmd.ExecuteNonQuery() > 0

    let clear () =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "DELETE FROM applications"
        cmd.ExecuteNonQuery() |> ignore
