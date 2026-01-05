/// Data entity repository backed by SQLite
namespace EATool.Infrastructure

open System
open System.Text.Json
open Microsoft.Data.Sqlite
open EATool.Domain

module DataEntityRepository =
    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "dat-" + guid.Substring(0, 8)

    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let private serializeList (items: string list) = JsonSerializer.Serialize(items)
    let private deserializeList (payload: string) = try JsonSerializer.Deserialize<string list>(payload) with _ -> []

    let private classificationToString (cls: DataClassification) =
        match cls with
        | DataClassification.Public -> "public"
        | DataClassification.Internal -> "internal"
        | DataClassification.Confidential -> "confidential"
        | DataClassification.Restricted -> "restricted"

    let private classificationFromString (value: string) =
        match value.ToLowerInvariant() with
        | "public" -> DataClassification.Public
        | "internal" -> DataClassification.Internal
        | "confidential" -> DataClassification.Confidential
        | "restricted" -> DataClassification.Restricted
        | _ -> DataClassification.Internal

    let private getStringOption (reader: SqliteDataReader) (ordinal: int) = if reader.IsDBNull(ordinal) then None else Some(reader.GetString(ordinal))

    let private mapEntity (reader: SqliteDataReader) : DataEntity =
        let idIdx = reader.GetOrdinal("id")
        let nameIdx = reader.GetOrdinal("name")
        let domainIdx = reader.GetOrdinal("domain")
        let classificationIdx = reader.GetOrdinal("classification")
        let retentionIdx = reader.GetOrdinal("retention")
        let ownerIdx = reader.GetOrdinal("owner")
        let stewardIdx = reader.GetOrdinal("steward")
        let sourceIdx = reader.GetOrdinal("source_system")
        let critIdx = reader.GetOrdinal("criticality")
        let piiIdx = reader.GetOrdinal("pii_flag")
        let glossaryIdx = reader.GetOrdinal("glossary_terms")
        let lineageIdx = reader.GetOrdinal("lineage")
        let createdIdx = reader.GetOrdinal("created_at")
        let updatedIdx = reader.GetOrdinal("updated_at")

        {
            Id = reader.GetString(idIdx)
            Name = reader.GetString(nameIdx)
            Domain = getStringOption reader domainIdx
            Classification = reader.GetString(classificationIdx) |> classificationFromString
            Retention = getStringOption reader retentionIdx
            Owner = getStringOption reader ownerIdx
            Steward = getStringOption reader stewardIdx
            SourceSystem = getStringOption reader sourceIdx
            Criticality = getStringOption reader critIdx
            PiiFlag = reader.GetInt32(piiIdx) <> 0
            GlossaryTerms = reader.GetString(glossaryIdx) |> deserializeList
            Lineage = reader.GetString(lineageIdx) |> deserializeList
            CreatedAt = reader.GetString(createdIdx)
            UpdatedAt = reader.GetString(updatedIdx)
        }

    let private addOptionalParam (cmd: SqliteCommand) (name: string) (value: obj option) =
        let p = cmd.CreateParameter()
        p.ParameterName <- name
        p.Value <- value |> Option.defaultValue (box DBNull.Value)
        cmd.Parameters.Add(p) |> ignore

    let private buildFilters (search: string option) (domain: string option) (classification: DataClassification option) =
        let clauses = System.Collections.Generic.List<string>()
        let parameters = System.Collections.Generic.List<SqliteParameter>()

        match search with
        | Some term when not (String.IsNullOrWhiteSpace term) ->
            clauses.Add("name LIKE $search")
            parameters.Add(new SqliteParameter("$search", "%" + term + "%"))
        | _ -> ()

        match domain with
        | Some d when not (String.IsNullOrWhiteSpace d) ->
            clauses.Add("domain = $domain")
            parameters.Add(new SqliteParameter("$domain", d))
        | _ -> ()

        match classification with
        | Some cls ->
            clauses.Add("classification = $classification")
            parameters.Add(new SqliteParameter("$classification", classificationToString cls))
        | None -> ()

        let whereClause = if clauses.Count = 0 then "" else " WHERE " + String.Join(" AND ", clauses)
        whereClause, parameters

    let getAll (page: int) (limit: int) (search: string option) (domain: string option) (classification: DataClassification option) : PaginatedResponse<DataEntity> =
        let page = if page < 1 then 1 else page
        let limit = if limit < 1 || limit > 200 then 50 else limit
        let offset = (page - 1) * limit

        use conn = Database.getConnection ()
        let whereClause, parameters = buildFilters search domain classification

        use listCmd = conn.CreateCommand()
        listCmd.CommandText <- sprintf "SELECT id, name, domain, classification, retention, owner, steward, source_system, criticality, pii_flag, glossary_terms, lineage, created_at, updated_at FROM data_entities%s ORDER BY datetime(created_at) DESC LIMIT $limit OFFSET $offset" whereClause
        parameters |> Seq.iter (fun p -> listCmd.Parameters.Add(p) |> ignore)
        listCmd.Parameters.AddWithValue("$limit", limit) |> ignore
        listCmd.Parameters.AddWithValue("$offset", offset) |> ignore

        use reader = listCmd.ExecuteReader()
        let items = [ while reader.Read() do mapEntity reader ]

        use countCmd = conn.CreateCommand()
        countCmd.CommandText <- sprintf "SELECT COUNT(1) FROM data_entities%s" whereClause
        parameters |> Seq.iter (fun p -> countCmd.Parameters.Add(new SqliteParameter(p.ParameterName, p.Value)) |> ignore)
        let total = countCmd.ExecuteScalar() :?> int64 |> int

        { Items = items; Page = page; Limit = limit; Total = total }

    let getById (id: string) : DataEntity option =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT id, name, domain, classification, retention, owner, steward, source_system, criticality, pii_flag, glossary_terms, lineage, created_at, updated_at FROM data_entities WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        use reader = cmd.ExecuteReader()
        if reader.Read() then Some (mapEntity reader) else None

    let create (req: CreateDataEntityRequest) : DataEntity =
        let id = generateId ()
        let now = getUtcTimestamp ()
        let pii = req.PiiFlag |> Option.defaultValue false
        let glossary = req.GlossaryTerms |> Option.defaultValue []
        let lineage = req.Lineage |> Option.defaultValue []

        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <-
            """
            INSERT INTO data_entities (id, name, domain, classification, retention, owner, steward, source_system, criticality, pii_flag, glossary_terms, lineage, created_at, updated_at)
            VALUES ($id, $name, $domain, $classification, $retention, $owner, $steward, $source_system, $criticality, $pii_flag, $glossary_terms, $lineage, $created_at, $updated_at)
            """
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        cmd.Parameters.AddWithValue("$name", req.Name) |> ignore
        addOptionalParam cmd "$domain" (req.Domain |> Option.map box)
        cmd.Parameters.AddWithValue("$classification", classificationToString req.Classification) |> ignore
        addOptionalParam cmd "$retention" (req.Retention |> Option.map box)
        addOptionalParam cmd "$owner" (req.Owner |> Option.map box)
        addOptionalParam cmd "$steward" (req.Steward |> Option.map box)
        addOptionalParam cmd "$source_system" (req.SourceSystem |> Option.map box)
        addOptionalParam cmd "$criticality" (req.Criticality |> Option.map box)
        cmd.Parameters.AddWithValue("$pii_flag", if pii then 1 else 0) |> ignore
        cmd.Parameters.AddWithValue("$glossary_terms", serializeList glossary) |> ignore
        cmd.Parameters.AddWithValue("$lineage", serializeList lineage) |> ignore
        cmd.Parameters.AddWithValue("$created_at", now) |> ignore
        cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

        cmd.ExecuteNonQuery() |> ignore

        { Id = id
          Name = req.Name
          Domain = req.Domain
          Classification = req.Classification
          Retention = req.Retention
          Owner = req.Owner
          Steward = req.Steward
          SourceSystem = req.SourceSystem
          Criticality = req.Criticality
          PiiFlag = pii
          GlossaryTerms = glossary
          Lineage = lineage
          CreatedAt = now
          UpdatedAt = now }

    let update (id: string) (req: CreateDataEntityRequest) : DataEntity option =
        match getById id with
        | Some existing ->
            let now = getUtcTimestamp ()
            let pii = req.PiiFlag |> Option.defaultValue existing.PiiFlag
            let glossary = req.GlossaryTerms |> Option.defaultValue existing.GlossaryTerms
            let lineage = req.Lineage |> Option.defaultValue existing.Lineage

            use conn = Database.getConnection ()
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                UPDATE data_entities
                SET name = $name,
                    domain = $domain,
                    classification = $classification,
                    retention = $retention,
                    owner = $owner,
                    steward = $steward,
                    source_system = $source_system,
                    criticality = $criticality,
                    pii_flag = $pii_flag,
                    glossary_terms = $glossary_terms,
                    lineage = $lineage,
                    updated_at = $updated_at
                WHERE id = $id
                """
            cmd.Parameters.AddWithValue("$id", id) |> ignore
            cmd.Parameters.AddWithValue("$name", req.Name) |> ignore
            addOptionalParam cmd "$domain" (req.Domain |> Option.map box)
            cmd.Parameters.AddWithValue("$classification", classificationToString req.Classification) |> ignore
            addOptionalParam cmd "$retention" (req.Retention |> Option.map box)
            addOptionalParam cmd "$owner" (req.Owner |> Option.map box)
            addOptionalParam cmd "$steward" (req.Steward |> Option.map box)
            addOptionalParam cmd "$source_system" (req.SourceSystem |> Option.map box)
            addOptionalParam cmd "$criticality" (req.Criticality |> Option.map box)
            cmd.Parameters.AddWithValue("$pii_flag", if pii then 1 else 0) |> ignore
            cmd.Parameters.AddWithValue("$glossary_terms", serializeList glossary) |> ignore
            cmd.Parameters.AddWithValue("$lineage", serializeList lineage) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

            let rows = cmd.ExecuteNonQuery()
            if rows > 0 then
                Some
                    { existing with
                        Name = req.Name
                        Domain = req.Domain
                        Classification = req.Classification
                        Retention = req.Retention
                        Owner = req.Owner
                        Steward = req.Steward
                        SourceSystem = req.SourceSystem
                        Criticality = req.Criticality
                        PiiFlag = pii
                        GlossaryTerms = glossary
                        Lineage = lineage
                        UpdatedAt = now }
            else None
        | None -> None

    let delete (id: string) : bool =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "DELETE FROM data_entities WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        cmd.ExecuteNonQuery() > 0

    let clear () =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "DELETE FROM data_entities"
        cmd.ExecuteNonQuery() |> ignore
