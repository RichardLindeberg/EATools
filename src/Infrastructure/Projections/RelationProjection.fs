/// Relation projection that maintains the relations read model
namespace EATool.Infrastructure.Projections

open System
open Microsoft.Data.Sqlite
open EATool.Domain
open EATool.Infrastructure

module RelationProjection =
    
    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let private addOptionalParam (cmd: SqliteCommand) (name: string) (value: obj option) =
        let p = cmd.CreateParameter()
        p.ParameterName <- name
        p.Value <- value |> Option.defaultValue (box DBNull.Value)
        cmd.Parameters.Add(p) |> ignore
    
    let private entityTypeToString (entityType: EntityType) : string =
        match entityType with
        | EntityType.Organization -> "organization"
        | EntityType.Application -> "application"
        | EntityType.ApplicationService -> "application_service"
        | EntityType.ApplicationInterface -> "application_interface"
        | EntityType.Server -> "server"
        | EntityType.Integration -> "integration"
        | EntityType.BusinessCapability -> "business_capability"
        | EntityType.DataEntity -> "data_entity"
        | EntityType.View -> "view"
    
    let private relationTypeToString (relationType: RelationType) : string =
        match relationType with
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
    
    let private handleCreated (data: RelationCreatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                INSERT INTO relations (
                    id, source_id, target_id, source_type, target_type, relation_type,
                    description, data_classification, confidence, effective_from, effective_to,
                    created_at, updated_at
                )
                VALUES (
                    $id, $source_id, $target_id, $source_type, $target_type, $relation_type,
                    $description, $data_classification, $confidence, $effective_from, $effective_to,
                    $created_at, $updated_at
                )
                ON CONFLICT(id) DO NOTHING
                """
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$source_id", data.SourceId) |> ignore
            cmd.Parameters.AddWithValue("$target_id", data.TargetId) |> ignore
            cmd.Parameters.AddWithValue("$source_type", entityTypeToString data.SourceType) |> ignore
            cmd.Parameters.AddWithValue("$target_type", entityTypeToString data.TargetType) |> ignore
            cmd.Parameters.AddWithValue("$relation_type", relationTypeToString data.RelationType) |> ignore
            addOptionalParam cmd "$description" (data.Description |> Option.map box)
            addOptionalParam cmd "$data_classification" (data.DataClassification |> Option.map box)
            addOptionalParam cmd "$confidence" (data.Confidence |> Option.map box)
            addOptionalParam cmd "$effective_from" (data.EffectiveFrom |> Option.map box)
            addOptionalParam cmd "$effective_to" (data.EffectiveTo |> Option.map box)
            cmd.Parameters.AddWithValue("$created_at", now) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle RelationCreated: {ex.Message}"
    
    let private handleConfidenceUpdated (data: ConfidenceUpdatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                UPDATE relations 
                SET confidence = $confidence, evidence_source = $evidence_source, 
                    last_verified_at = $last_verified_at, updated_at = $updated_at 
                WHERE id = $id
                """
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$confidence", data.NewConfidence) |> ignore
            addOptionalParam cmd "$evidence_source" (data.EvidenceSource |> Option.map box)
            addOptionalParam cmd "$last_verified_at" (data.LastVerifiedAt |> Option.map box)
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle ConfidenceUpdated: {ex.Message}"
    
    let private handleEffectiveDatesSet (data: EffectiveDatesSetData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                UPDATE relations 
                SET effective_from = $effective_from, effective_to = $effective_to, updated_at = $updated_at 
                WHERE id = $id
                """
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            addOptionalParam cmd "$effective_from" (data.NewEffectiveFrom |> Option.map box)
            addOptionalParam cmd "$effective_to" (data.NewEffectiveTo |> Option.map box)
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle EffectiveDatesSet: {ex.Message}"
    
    let private handleDescriptionUpdated (data: RelationDescriptionUpdatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE relations SET description = $description, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            addOptionalParam cmd "$description" (data.NewDescription |> Option.map box)
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle RelationDescriptionUpdated: {ex.Message}"
    
    let private handleDeleted (data: RelationDeletedData) (connString: string) : Result<unit, string> =
        try
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "DELETE FROM relations WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle RelationDeleted: {ex.Message}"
    
    /// Projection handler that processes Relation events
    type Handler(connString: string) =
        interface ProjectionEngine.IProjectionHandler<RelationEvent> with
            member _.ProjectionName = "RelationProjection"
            
            member _.CanHandle(eventType: string) =
                match eventType with
                | "RelationCreated"
                | "ConfidenceUpdated"
                | "EffectiveDatesSet"
                | "RelationDescriptionUpdated"
                | "RelationDeleted" -> true
                | _ -> false

            member _.Handle(envelope: EventEnvelope<RelationEvent>) =
                match envelope.Data with
                | RelationCreated data -> handleCreated data connString
                | ConfidenceUpdated data -> handleConfidenceUpdated data connString
                | EffectiveDatesSet data -> handleEffectiveDatesSet data connString
                | RelationDescriptionUpdated data -> handleDescriptionUpdated data connString
                | RelationDeleted data -> handleDeleted data connString

