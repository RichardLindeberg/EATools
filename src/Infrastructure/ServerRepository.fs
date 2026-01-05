/// Server repository backed by SQLite
namespace EATool.Infrastructure

open System
open System.Text.Json
open Microsoft.Data.Sqlite
open EATool.Domain

module ServerRepository =
    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "srv-" + guid.Substring(0, 8)

    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let private serializeTags (tags: string list) = JsonSerializer.Serialize(tags)
    let private deserializeTags (payload: string) = try JsonSerializer.Deserialize<string list>(payload) with _ -> []

    let private getStringOption (reader: SqliteDataReader) (ordinal: int) = if reader.IsDBNull(ordinal) then None else Some(reader.GetString(ordinal))

    let private mapServer (reader: SqliteDataReader) : Server =
        let idIdx = reader.GetOrdinal("id")
        let hostIdx = reader.GetOrdinal("hostname")
        let envIdx = reader.GetOrdinal("environment")
        let regionIdx = reader.GetOrdinal("region")
        let platformIdx = reader.GetOrdinal("platform")
        let critIdx = reader.GetOrdinal("criticality")
        let teamIdx = reader.GetOrdinal("owning_team")
        let tagsIdx = reader.GetOrdinal("tags")
        let createdIdx = reader.GetOrdinal("created_at")
        let updatedIdx = reader.GetOrdinal("updated_at")

        {
            Id = reader.GetString(idIdx)
            Hostname = reader.GetString(hostIdx)
            Environment = getStringOption reader envIdx
            Region = getStringOption reader regionIdx
            Platform = getStringOption reader platformIdx
            Criticality = getStringOption reader critIdx
            OwningTeam = getStringOption reader teamIdx
            Tags = reader.GetString(tagsIdx) |> deserializeTags
            CreatedAt = reader.GetString(createdIdx)
            UpdatedAt = reader.GetString(updatedIdx)
        }

    let private addOptionalParam (cmd: SqliteCommand) (name: string) (value: obj option) =
        let p = cmd.CreateParameter()
        p.ParameterName <- name
        p.Value <- value |> Option.defaultValue (box DBNull.Value)
        cmd.Parameters.Add(p) |> ignore

    let private buildFilters (environment: string option) (region: string option) =
        let clauses = System.Collections.Generic.List<string>()
        let parameters = System.Collections.Generic.List<SqliteParameter>()

        match environment with
        | Some env when not (String.IsNullOrWhiteSpace env) ->
            clauses.Add("environment = $env")
            parameters.Add(new SqliteParameter("$env", env))
        | _ -> ()

        match region with
        | Some r when not (String.IsNullOrWhiteSpace r) ->
            clauses.Add("region = $region")
            parameters.Add(new SqliteParameter("$region", r))
        | _ -> ()

        let whereClause = if clauses.Count = 0 then "" else " WHERE " + String.Join(" AND ", clauses)
        whereClause, parameters

    let getAll (page: int) (limit: int) (environment: string option) (region: string option) : PaginatedResponse<Server> =
        let page = if page < 1 then 1 else page
        let limit = if limit < 1 || limit > 200 then 50 else limit
        let offset = (page - 1) * limit

        use conn = Database.getConnection ()
        let whereClause, parameters = buildFilters environment region

        use listCmd = conn.CreateCommand()
        listCmd.CommandText <- sprintf "SELECT id, hostname, environment, region, platform, criticality, owning_team, tags, created_at, updated_at FROM servers%s ORDER BY datetime(created_at) DESC LIMIT $limit OFFSET $offset" whereClause
        parameters |> Seq.iter (fun p -> listCmd.Parameters.Add(p) |> ignore)
        listCmd.Parameters.AddWithValue("$limit", limit) |> ignore
        listCmd.Parameters.AddWithValue("$offset", offset) |> ignore

        use reader = listCmd.ExecuteReader()
        let items = [ while reader.Read() do mapServer reader ]

        use countCmd = conn.CreateCommand()
        countCmd.CommandText <- sprintf "SELECT COUNT(1) FROM servers%s" whereClause
        parameters |> Seq.iter (fun p -> countCmd.Parameters.Add(new SqliteParameter(p.ParameterName, p.Value)) |> ignore)
        let total = countCmd.ExecuteScalar() :?> int64 |> int

        { Items = items; Page = page; Limit = limit; Total = total }

    let getById (id: string) : Server option =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT id, hostname, environment, region, platform, criticality, owning_team, tags, created_at, updated_at FROM servers WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        use reader = cmd.ExecuteReader()
        if reader.Read() then Some (mapServer reader) else None

    let create (req: CreateServerRequest) : Server =
        let id = generateId ()
        let now = getUtcTimestamp ()
        let tags = req.Tags |> Option.defaultValue []

        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <-
            """
            INSERT INTO servers (id, hostname, environment, region, platform, criticality, owning_team, tags, created_at, updated_at)
            VALUES ($id, $hostname, $environment, $region, $platform, $criticality, $owning_team, $tags, $created_at, $updated_at)
            """
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        cmd.Parameters.AddWithValue("$hostname", req.Hostname) |> ignore
        addOptionalParam cmd "$environment" (req.Environment |> Option.map box)
        addOptionalParam cmd "$region" (req.Region |> Option.map box)
        addOptionalParam cmd "$platform" (req.Platform |> Option.map box)
        addOptionalParam cmd "$criticality" (req.Criticality |> Option.map box)
        addOptionalParam cmd "$owning_team" (req.OwningTeam |> Option.map box)
        cmd.Parameters.AddWithValue("$tags", serializeTags tags) |> ignore
        cmd.Parameters.AddWithValue("$created_at", now) |> ignore
        cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

        cmd.ExecuteNonQuery() |> ignore

        { Id = id
          Hostname = req.Hostname
          Environment = req.Environment
          Region = req.Region
          Platform = req.Platform
          Criticality = req.Criticality
          OwningTeam = req.OwningTeam
          Tags = tags
          CreatedAt = now
          UpdatedAt = now }

    let update (id: string) (req: CreateServerRequest) : Server option =
        match getById id with
        | Some existing ->
            let now = getUtcTimestamp ()
            let tags = req.Tags |> Option.defaultValue existing.Tags

            use conn = Database.getConnection ()
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                UPDATE servers
                SET hostname = $hostname,
                    environment = $environment,
                    region = $region,
                    platform = $platform,
                    criticality = $criticality,
                    owning_team = $owning_team,
                    tags = $tags,
                    updated_at = $updated_at
                WHERE id = $id
                """
            cmd.Parameters.AddWithValue("$id", id) |> ignore
            cmd.Parameters.AddWithValue("$hostname", req.Hostname) |> ignore
            addOptionalParam cmd "$environment" (req.Environment |> Option.map box)
            addOptionalParam cmd "$region" (req.Region |> Option.map box)
            addOptionalParam cmd "$platform" (req.Platform |> Option.map box)
            addOptionalParam cmd "$criticality" (req.Criticality |> Option.map box)
            addOptionalParam cmd "$owning_team" (req.OwningTeam |> Option.map box)
            cmd.Parameters.AddWithValue("$tags", serializeTags tags) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

            let rows = cmd.ExecuteNonQuery()
            if rows > 0 then
                Some
                    { existing with
                        Hostname = req.Hostname
                        Environment = req.Environment
                        Region = req.Region
                        Platform = req.Platform
                        Criticality = req.Criticality
                        OwningTeam = req.OwningTeam
                        Tags = tags
                        UpdatedAt = now }
            else None
        | None -> None

    let delete (id: string) : bool =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "DELETE FROM servers WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        cmd.ExecuteNonQuery() > 0

    let clear () =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "DELETE FROM servers"
        cmd.ExecuteNonQuery() |> ignore
