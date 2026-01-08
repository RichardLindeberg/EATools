/// Integration repository backed by SQLite
namespace EATool.Infrastructure

open System
open System.Text.Json
open Microsoft.Data.Sqlite
open EATool.Domain

module IntegrationRepository =
    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "int-" + guid.Substring(0, 8)

    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let private serializeTags (tags: string list) = JsonSerializer.Serialize(tags)
    let private deserializeTags (payload: string) = try JsonSerializer.Deserialize<string list>(payload) with _ -> []

    let private mapIntegration (reader: SqliteDataReader) : Integration =
        let idIdx = reader.GetOrdinal("id")
        let srcIdx = reader.GetOrdinal("source_app_id")
        let tgtIdx = reader.GetOrdinal("target_app_id")
        let protoIdx = reader.GetOrdinal("protocol")
        let contractIdx = reader.GetOrdinal("data_contract")
        let slaIdx = reader.GetOrdinal("sla")
        let freqIdx = reader.GetOrdinal("frequency")
        let tagsIdx = reader.GetOrdinal("tags")
        let createdIdx = reader.GetOrdinal("created_at")
        let updatedIdx = reader.GetOrdinal("updated_at")

        let optString (ord: int) = if reader.IsDBNull(ord) then None else Some(reader.GetString(ord))

        {
            Id = reader.GetString(idIdx)
            SourceAppId = reader.GetString(srcIdx)
            TargetAppId = reader.GetString(tgtIdx)
            Protocol = reader.GetString(protoIdx)
            DataContract = optString contractIdx
            Sla = optString slaIdx
            Frequency = optString freqIdx
            Tags = reader.GetString(tagsIdx) |> deserializeTags
            CreatedAt = reader.GetString(createdIdx)
            UpdatedAt = reader.GetString(updatedIdx)
        }

    let private addOptionalParam (cmd: SqliteCommand) (name: string) (value: obj option) =
        let p = cmd.CreateParameter()
        p.ParameterName <- name
        p.Value <- value |> Option.defaultValue (box DBNull.Value)
        cmd.Parameters.Add(p) |> ignore

    let private buildFilters (sourceAppId: string option) (targetAppId: string option) =
        let clauses = System.Collections.Generic.List<string>()
        let parameters = System.Collections.Generic.List<SqliteParameter>()

        match sourceAppId with
        | Some src when not (String.IsNullOrWhiteSpace src) ->
            clauses.Add("source_app_id = $src")
            parameters.Add(new SqliteParameter("$src", src))
        | _ -> ()

        match targetAppId with
        | Some tgt when not (String.IsNullOrWhiteSpace tgt) ->
            clauses.Add("target_app_id = $tgt")
            parameters.Add(new SqliteParameter("$tgt", tgt))
        | _ -> ()

        let whereClause = if clauses.Count = 0 then "" else " WHERE " + String.Join(" AND ", clauses)
        whereClause, parameters

    let getAll (page: int) (limit: int) (sourceAppId: string option) (targetAppId: string option) : PaginatedResponse<Integration> =
        let page = if page < 1 then 1 else page
        let limit = if limit < 1 || limit > 200 then 50 else limit
        let offset = (page - 1) * limit

        use conn = Database.getConnection ()
        let whereClause, parameters = buildFilters sourceAppId targetAppId

        use listCmd = conn.CreateCommand()
        listCmd.CommandText <- sprintf "SELECT id, source_app_id, target_app_id, protocol, data_contract, sla, frequency, tags, created_at, updated_at FROM integrations%s ORDER BY datetime(created_at) DESC LIMIT $limit OFFSET $offset" whereClause
        parameters |> Seq.iter (fun p -> listCmd.Parameters.Add(p) |> ignore)
        listCmd.Parameters.AddWithValue("$limit", limit) |> ignore
        listCmd.Parameters.AddWithValue("$offset", offset) |> ignore

        use reader = listCmd.ExecuteReader()
        let items = [ while reader.Read() do mapIntegration reader ]

        use countCmd = conn.CreateCommand()
        countCmd.CommandText <- sprintf "SELECT COUNT(1) FROM integrations%s" whereClause
        parameters |> Seq.iter (fun p -> countCmd.Parameters.Add(new SqliteParameter(p.ParameterName, p.Value)) |> ignore)
        let total = countCmd.ExecuteScalar() :?> int64 |> int

        { Items = items; Page = page; Limit = limit; Total = total }

    let getById (id: string) : Integration option =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT id, source_app_id, target_app_id, protocol, data_contract, sla, frequency, tags, created_at, updated_at FROM integrations WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        use reader = cmd.ExecuteReader()
        if reader.Read() then Some (mapIntegration reader) else None

    let create (req: CreateIntegrationRequest) : Integration =
        let id = generateId ()
        let now = getUtcTimestamp ()
        let tags = req.Tags |> Option.defaultValue []

        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <-
            """
            INSERT INTO integrations (id, source_app_id, target_app_id, protocol, data_contract, sla, frequency, tags, created_at, updated_at)
            VALUES ($id, $source_app_id, $target_app_id, $protocol, $data_contract, $sla, $frequency, $tags, $created_at, $updated_at)
            """
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        cmd.Parameters.AddWithValue("$source_app_id", req.SourceAppId) |> ignore
        cmd.Parameters.AddWithValue("$target_app_id", req.TargetAppId) |> ignore
        cmd.Parameters.AddWithValue("$protocol", req.Protocol) |> ignore
        addOptionalParam cmd "$data_contract" (req.DataContract |> Option.map box)
        addOptionalParam cmd "$sla" (req.Sla |> Option.map box)
        addOptionalParam cmd "$frequency" (req.Frequency |> Option.map box)
        cmd.Parameters.AddWithValue("$tags", serializeTags tags) |> ignore
        cmd.Parameters.AddWithValue("$created_at", now) |> ignore
        cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

        cmd.ExecuteNonQuery() |> ignore

        { Id = id
          SourceAppId = req.SourceAppId
          TargetAppId = req.TargetAppId
          Protocol = req.Protocol
          DataContract = req.DataContract
          Sla = req.Sla
          Frequency = req.Frequency
          Tags = tags
          CreatedAt = now
          UpdatedAt = now }

    let update (id: string) (req: CreateIntegrationRequest) : Integration option =
        match getById id with
        | Some existing ->
            let now = getUtcTimestamp ()
            let tags = req.Tags |> Option.defaultValue existing.Tags

            use conn = Database.getConnection ()
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                UPDATE integrations
                SET source_app_id = $source_app_id,
                    target_app_id = $target_app_id,
                    protocol = $protocol,
                    data_contract = $data_contract,
                    sla = $sla,
                    frequency = $frequency,
                    tags = $tags,
                    updated_at = $updated_at
                WHERE id = $id
                """
            cmd.Parameters.AddWithValue("$id", id) |> ignore
            cmd.Parameters.AddWithValue("$source_app_id", req.SourceAppId) |> ignore
            cmd.Parameters.AddWithValue("$target_app_id", req.TargetAppId) |> ignore
            cmd.Parameters.AddWithValue("$protocol", req.Protocol) |> ignore
            addOptionalParam cmd "$data_contract" (req.DataContract |> Option.map box)
            addOptionalParam cmd "$sla" (req.Sla |> Option.map box)
            addOptionalParam cmd "$frequency" (req.Frequency |> Option.map box)
            cmd.Parameters.AddWithValue("$tags", serializeTags tags) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

            let rows = cmd.ExecuteNonQuery()
            if rows > 0 then
                Some
                    { existing with
                        SourceAppId = req.SourceAppId
                        TargetAppId = req.TargetAppId
                        Protocol = req.Protocol
                        DataContract = req.DataContract
                        Sla = req.Sla
                        Frequency = req.Frequency
                        Tags = tags
                        UpdatedAt = now }
            else None
        | None -> None

    let delete (id: string) : bool =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "DELETE FROM integrations WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        cmd.ExecuteNonQuery() > 0

    let clear () =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "DELETE FROM integrations"
        cmd.ExecuteNonQuery() |> ignore
