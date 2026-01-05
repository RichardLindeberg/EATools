/// Relation repository backed by SQLite
namespace EATool.Infrastructure

open System
open Microsoft.Data.Sqlite
open EATool.Domain

module RelationRepository =
    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "rel-" + guid.Substring(0, 8)

    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let private addOptionalParam (cmd: SqliteCommand) (name: string) (value: obj option) =
        let p = cmd.CreateParameter()
        p.ParameterName <- name
        p.Value <- value |> Option.defaultValue (box DBNull.Value)
        cmd.Parameters.Add(p) |> ignore

    let private getStringOption (reader: SqliteDataReader) (ordinal: int) = if reader.IsDBNull(ordinal) then None else Some(reader.GetString(ordinal))
    let private getFloatOption (reader: SqliteDataReader) (ordinal: int) = if reader.IsDBNull(ordinal) then None else Some(reader.GetDouble(ordinal) |> float)

    let private entityTypeToString (et: EntityType) =
        match et with
        | EntityType.Organization -> "organization"
        | EntityType.Application -> "application"
        | EntityType.ApplicationService -> "application_service"
        | EntityType.ApplicationInterface -> "application_interface"
        | EntityType.Server -> "server"
        | EntityType.Integration -> "integration"
        | EntityType.BusinessCapability -> "business_capability"
        | EntityType.DataEntity -> "data_entity"
        | EntityType.View -> "view"

    let private entityTypeFromString (s: string) =
        match s.ToLowerInvariant() with
        | "organization" -> EntityType.Organization
        | "application" -> EntityType.Application
        | "application_service" -> EntityType.ApplicationService
        | "application_interface" -> EntityType.ApplicationInterface
        | "server" -> EntityType.Server
        | "integration" -> EntityType.Integration
        | "business_capability" -> EntityType.BusinessCapability
        | "data_entity" -> EntityType.DataEntity
        | "view" -> EntityType.View
        | _ -> EntityType.Application

    let private relationTypeToString (rt: RelationType) =
        match rt with
        | RelationType.DependsOn -> "depends_on"
        | RelationType.CommunicatesWith -> "communicates_with"
        | RelationType.Calls -> "calls"
        | RelationType.PublishesEventTo -> "publishes_event_to"
        | RelationType.ConsumesEventFrom -> "consumes_event_from"
        | RelationType.DeployedOn -> "deployed_on"
        | RelationType.StoresDataOn -> "stores_data_on"
        | RelationType.Reads -> "reads"
        | RelationType.Writes -> "writes"
        | RelationType.Owns -> "owns"
        | RelationType.Supports -> "supports"
        | RelationType.Implements -> "implements"
        | RelationType.Realizes -> "realizes"
        | RelationType.Serves -> "serves"
        | RelationType.ConnectedTo -> "connected_to"
        | RelationType.Exposes -> "exposes"
        | RelationType.Uses -> "uses"

    let private relationTypeFromString (s: string) =
        match s.ToLowerInvariant() with
        | "depends_on" -> RelationType.DependsOn
        | "communicates_with" -> RelationType.CommunicatesWith
        | "calls" -> RelationType.Calls
        | "publishes_event_to" -> RelationType.PublishesEventTo
        | "consumes_event_from" -> RelationType.ConsumesEventFrom
        | "deployed_on" -> RelationType.DeployedOn
        | "stores_data_on" -> RelationType.StoresDataOn
        | "reads" -> RelationType.Reads
        | "writes" -> RelationType.Writes
        | "owns" -> RelationType.Owns
        | "supports" -> RelationType.Supports
        | "implements" -> RelationType.Implements
        | "realizes" -> RelationType.Realizes
        | "serves" -> RelationType.Serves
        | "connected_to" -> RelationType.ConnectedTo
        | "exposes" -> RelationType.Exposes
        | "uses" -> RelationType.Uses
        | _ -> RelationType.DependsOn

    let private archimateRelToString (rel: ArchiMateRelationship) =
        match rel with
        | ArchiMateRelationship.Assignment -> "assignment"
        | ArchiMateRelationship.Realization -> "realization"
        | ArchiMateRelationship.Serving -> "serving"
        | ArchiMateRelationship.Access -> "access"
        | ArchiMateRelationship.Flow -> "flow"
        | ArchiMateRelationship.Triggering -> "triggering"
        | ArchiMateRelationship.Association -> "association"
        | ArchiMateRelationship.Composition -> "composition"
        | ArchiMateRelationship.Aggregation -> "aggregation"
        | ArchiMateRelationship.Specialization -> "specialization"
        | ArchiMateRelationship.Influence -> "influence"

    let private archimateRelFromString (s: string) =
        match s.ToLowerInvariant() with
        | "assignment" -> ArchiMateRelationship.Assignment
        | "realization" -> ArchiMateRelationship.Realization
        | "serving" -> ArchiMateRelationship.Serving
        | "access" -> ArchiMateRelationship.Access
        | "flow" -> ArchiMateRelationship.Flow
        | "triggering" -> ArchiMateRelationship.Triggering
        | "association" -> ArchiMateRelationship.Association
        | "composition" -> ArchiMateRelationship.Composition
        | "aggregation" -> ArchiMateRelationship.Aggregation
        | "specialization" -> ArchiMateRelationship.Specialization
        | "influence" -> ArchiMateRelationship.Influence
        | _ -> ArchiMateRelationship.Assignment

    let private mapRelation (reader: SqliteDataReader) : Relation =
        let idIdx = reader.GetOrdinal("id")
        let srcIdx = reader.GetOrdinal("source_id")
        let tgtIdx = reader.GetOrdinal("target_id")
        let srcTypeIdx = reader.GetOrdinal("source_type")
        let tgtTypeIdx = reader.GetOrdinal("target_type")
        let relTypeIdx = reader.GetOrdinal("relation_type")
        let archElemIdx = reader.GetOrdinal("archimate_element")
        let archRelIdx = reader.GetOrdinal("archimate_relationship")
        let descIdx = reader.GetOrdinal("description")
        let dataClassIdx = reader.GetOrdinal("data_classification")
        let critIdx = reader.GetOrdinal("criticality")
        let confIdx = reader.GetOrdinal("confidence")
        let evidenceIdx = reader.GetOrdinal("evidence_source")
        let lastIdx = reader.GetOrdinal("last_verified_at")
        let effFromIdx = reader.GetOrdinal("effective_from")
        let effToIdx = reader.GetOrdinal("effective_to")
        let labelIdx = reader.GetOrdinal("label")
        let colorIdx = reader.GetOrdinal("color")
        let styleIdx = reader.GetOrdinal("style")
        let bidirIdx = reader.GetOrdinal("bidirectional")
        let createdIdx = reader.GetOrdinal("created_at")
        let updatedIdx = reader.GetOrdinal("updated_at")

        {
            Id = reader.GetString(idIdx)
            SourceId = reader.GetString(srcIdx)
            TargetId = reader.GetString(tgtIdx)
            SourceType = reader.GetString(srcTypeIdx) |> entityTypeFromString
            TargetType = reader.GetString(tgtTypeIdx) |> entityTypeFromString
            RelationType = reader.GetString(relTypeIdx) |> relationTypeFromString
            ArchiMateElement = getStringOption reader archElemIdx
            ArchiMateRelationship = getStringOption reader archRelIdx |> Option.map archimateRelFromString
            Description = getStringOption reader descIdx
            DataClassification = getStringOption reader dataClassIdx
            Criticality = getStringOption reader critIdx
            Confidence = getFloatOption reader confIdx
            EvidenceSource = getStringOption reader evidenceIdx
            LastVerifiedAt = getStringOption reader lastIdx
            EffectiveFrom = getStringOption reader effFromIdx
            EffectiveTo = getStringOption reader effToIdx
            Label = getStringOption reader labelIdx
            Color = getStringOption reader colorIdx
            Style = getStringOption reader styleIdx
            Bidirectional = (reader.GetInt32(bidirIdx) <> 0)
            CreatedAt = reader.GetString(createdIdx)
            UpdatedAt = reader.GetString(updatedIdx)
        }

    let private buildFilters (sourceId: string option) (targetId: string option) (relationType: RelationType option) =
        let clauses = System.Collections.Generic.List<string>()
        let parameters = System.Collections.Generic.List<SqliteParameter>()

        match sourceId with
        | Some sid when not (String.IsNullOrWhiteSpace sid) ->
            clauses.Add("source_id = $source")
            parameters.Add(new SqliteParameter("$source", sid))
        | _ -> ()

        match targetId with
        | Some tid when not (String.IsNullOrWhiteSpace tid) ->
            clauses.Add("target_id = $target")
            parameters.Add(new SqliteParameter("$target", tid))
        | _ -> ()

        match relationType with
        | Some rt ->
            clauses.Add("relation_type = $relation_type")
            parameters.Add(new SqliteParameter("$relation_type", relationTypeToString rt))
        | None -> ()

        let whereClause = if clauses.Count = 0 then "" else " WHERE " + String.Join(" AND ", clauses)
        whereClause, parameters

    let getAll (page: int) (limit: int) (sourceId: string option) (targetId: string option) (relationType: RelationType option) : PaginatedResponse<Relation> =
        let page = if page < 1 then 1 else page
        let limit = if limit < 1 || limit > 200 then 50 else limit
        let offset = (page - 1) * limit

        use conn = Database.getConnection ()
        let whereClause, parameters = buildFilters sourceId targetId relationType

        use listCmd = conn.CreateCommand()
        listCmd.CommandText <- sprintf "SELECT id, source_id, target_id, source_type, target_type, relation_type, archimate_element, archimate_relationship, description, data_classification, criticality, confidence, evidence_source, last_verified_at, effective_from, effective_to, label, color, style, bidirectional, created_at, updated_at FROM relations%s ORDER BY datetime(created_at) DESC LIMIT $limit OFFSET $offset" whereClause
        parameters |> Seq.iter (fun p -> listCmd.Parameters.Add(p) |> ignore)
        listCmd.Parameters.AddWithValue("$limit", limit) |> ignore
        listCmd.Parameters.AddWithValue("$offset", offset) |> ignore

        use reader = listCmd.ExecuteReader()
        let items = [ while reader.Read() do mapRelation reader ]

        use countCmd = conn.CreateCommand()
        countCmd.CommandText <- sprintf "SELECT COUNT(1) FROM relations%s" whereClause
        parameters |> Seq.iter (fun p -> countCmd.Parameters.Add(new SqliteParameter(p.ParameterName, p.Value)) |> ignore)
        let total = countCmd.ExecuteScalar() :?> int64 |> int

        { Items = items; Page = page; Limit = limit; Total = total }

    let getById (id: string) : Relation option =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT id, source_id, target_id, source_type, target_type, relation_type, archimate_element, archimate_relationship, description, data_classification, criticality, confidence, evidence_source, last_verified_at, effective_from, effective_to, label, color, style, bidirectional, created_at, updated_at FROM relations WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        use reader = cmd.ExecuteReader()
        if reader.Read() then Some (mapRelation reader) else None

    let create (req: CreateRelationRequest) : Relation =
        let id = generateId ()
        let now = getUtcTimestamp ()
        let bidir = req.Bidirectional |> Option.defaultValue false

        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <-
            """
            INSERT INTO relations (id, source_id, target_id, source_type, target_type, relation_type, archimate_element, archimate_relationship, description, data_classification, criticality, confidence, evidence_source, last_verified_at, effective_from, effective_to, label, color, style, bidirectional, created_at, updated_at)
            VALUES ($id, $source_id, $target_id, $source_type, $target_type, $relation_type, $archimate_element, $archimate_relationship, $description, $data_classification, $criticality, $confidence, $evidence_source, $last_verified_at, $effective_from, $effective_to, $label, $color, $style, $bidirectional, $created_at, $updated_at)
            """
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        cmd.Parameters.AddWithValue("$source_id", req.SourceId) |> ignore
        cmd.Parameters.AddWithValue("$target_id", req.TargetId) |> ignore
        cmd.Parameters.AddWithValue("$source_type", entityTypeToString req.SourceType) |> ignore
        cmd.Parameters.AddWithValue("$target_type", entityTypeToString req.TargetType) |> ignore
        cmd.Parameters.AddWithValue("$relation_type", relationTypeToString req.RelationType) |> ignore
        addOptionalParam cmd "$archimate_element" (req.ArchiMateElement |> Option.map box)
        addOptionalParam cmd "$archimate_relationship" (req.ArchiMateRelationship |> Option.map (archimateRelToString >> box))
        addOptionalParam cmd "$description" (req.Description |> Option.map box)
        addOptionalParam cmd "$data_classification" (req.DataClassification |> Option.map box)
        addOptionalParam cmd "$criticality" (req.Criticality |> Option.map box)
        addOptionalParam cmd "$confidence" (req.Confidence |> Option.map box)
        addOptionalParam cmd "$evidence_source" (req.EvidenceSource |> Option.map box)
        addOptionalParam cmd "$last_verified_at" (req.LastVerifiedAt |> Option.map box)
        addOptionalParam cmd "$effective_from" (req.EffectiveFrom |> Option.map box)
        addOptionalParam cmd "$effective_to" (req.EffectiveTo |> Option.map box)
        addOptionalParam cmd "$label" (req.Label |> Option.map box)
        addOptionalParam cmd "$color" (req.Color |> Option.map box)
        addOptionalParam cmd "$style" (req.Style |> Option.map box)
        cmd.Parameters.AddWithValue("$bidirectional", if bidir then 1 else 0) |> ignore
        cmd.Parameters.AddWithValue("$created_at", now) |> ignore
        cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

        cmd.ExecuteNonQuery() |> ignore

        { Id = id
          SourceId = req.SourceId
          TargetId = req.TargetId
          SourceType = req.SourceType
          TargetType = req.TargetType
          RelationType = req.RelationType
          ArchiMateElement = req.ArchiMateElement
          ArchiMateRelationship = req.ArchiMateRelationship
          Description = req.Description
          DataClassification = req.DataClassification
          Criticality = req.Criticality
          Confidence = req.Confidence
          EvidenceSource = req.EvidenceSource
          LastVerifiedAt = req.LastVerifiedAt
          EffectiveFrom = req.EffectiveFrom
          EffectiveTo = req.EffectiveTo
          Label = req.Label
          Color = req.Color
          Style = req.Style
          Bidirectional = bidir
          CreatedAt = now
          UpdatedAt = now }

    let update (id: string) (req: CreateRelationRequest) : Relation option =
        match getById id with
        | Some existing ->
            let now = getUtcTimestamp ()
            let bidir = req.Bidirectional |> Option.defaultValue existing.Bidirectional

            use conn = Database.getConnection ()
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                UPDATE relations
                SET source_id = $source_id,
                    target_id = $target_id,
                    source_type = $source_type,
                    target_type = $target_type,
                    relation_type = $relation_type,
                    archimate_element = $archimate_element,
                    archimate_relationship = $archimate_relationship,
                    description = $description,
                    data_classification = $data_classification,
                    criticality = $criticality,
                    confidence = $confidence,
                    evidence_source = $evidence_source,
                    last_verified_at = $last_verified_at,
                    effective_from = $effective_from,
                    effective_to = $effective_to,
                    label = $label,
                    color = $color,
                    style = $style,
                    bidirectional = $bidirectional,
                    updated_at = $updated_at
                WHERE id = $id
                """
            cmd.Parameters.AddWithValue("$id", id) |> ignore
            cmd.Parameters.AddWithValue("$source_id", req.SourceId) |> ignore
            cmd.Parameters.AddWithValue("$target_id", req.TargetId) |> ignore
            cmd.Parameters.AddWithValue("$source_type", entityTypeToString req.SourceType) |> ignore
            cmd.Parameters.AddWithValue("$target_type", entityTypeToString req.TargetType) |> ignore
            cmd.Parameters.AddWithValue("$relation_type", relationTypeToString req.RelationType) |> ignore
            addOptionalParam cmd "$archimate_element" (req.ArchiMateElement |> Option.map box)
            addOptionalParam cmd "$archimate_relationship" (req.ArchiMateRelationship |> Option.map (archimateRelToString >> box))
            addOptionalParam cmd "$description" (req.Description |> Option.map box)
            addOptionalParam cmd "$data_classification" (req.DataClassification |> Option.map box)
            addOptionalParam cmd "$criticality" (req.Criticality |> Option.map box)
            addOptionalParam cmd "$confidence" (req.Confidence |> Option.map box)
            addOptionalParam cmd "$evidence_source" (req.EvidenceSource |> Option.map box)
            addOptionalParam cmd "$last_verified_at" (req.LastVerifiedAt |> Option.map box)
            addOptionalParam cmd "$effective_from" (req.EffectiveFrom |> Option.map box)
            addOptionalParam cmd "$effective_to" (req.EffectiveTo |> Option.map box)
            addOptionalParam cmd "$label" (req.Label |> Option.map box)
            addOptionalParam cmd "$color" (req.Color |> Option.map box)
            addOptionalParam cmd "$style" (req.Style |> Option.map box)
            cmd.Parameters.AddWithValue("$bidirectional", if bidir then 1 else 0) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

            let rows = cmd.ExecuteNonQuery()
            if rows > 0 then
                Some
                    { existing with
                        SourceId = req.SourceId
                        TargetId = req.TargetId
                        SourceType = req.SourceType
                        TargetType = req.TargetType
                        RelationType = req.RelationType
                        ArchiMateElement = req.ArchiMateElement
                        ArchiMateRelationship = req.ArchiMateRelationship
                        Description = req.Description
                        DataClassification = req.DataClassification
                        Criticality = req.Criticality
                        Confidence = req.Confidence
                        EvidenceSource = req.EvidenceSource
                        LastVerifiedAt = req.LastVerifiedAt
                        EffectiveFrom = req.EffectiveFrom
                        EffectiveTo = req.EffectiveTo
                        Label = req.Label
                        Color = req.Color
                        Style = req.Style
                        Bidirectional = bidir
                        UpdatedAt = now }
            else None
        | None -> None

    let delete (id: string) : bool =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "DELETE FROM relations WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        cmd.ExecuteNonQuery() > 0

    let clear () =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "DELETE FROM relations"
        cmd.ExecuteNonQuery() |> ignore
