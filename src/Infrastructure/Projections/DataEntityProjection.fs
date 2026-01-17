/// Data entity projection that maintains the data_entities read model
namespace EATool.Infrastructure.Projections

open System
open System.Text.Json
open Microsoft.Data.Sqlite
open EATool.Domain
open EATool.Infrastructure

module DataEntityProjection =
    
    let private serializeTags (tags: string list) =
        JsonSerializer.Serialize(tags)
    
    let private deserializeTags (json: string) =
        try JsonSerializer.Deserialize<string list>(json)
        with _ -> []

    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let private addOptionalParam (cmd: SqliteCommand) (name: string) (value: obj option) =
        let p = cmd.CreateParameter()
        p.ParameterName <- name
        p.Value <- value |> Option.defaultValue (box DBNull.Value)
        cmd.Parameters.Add(p) |> ignore
    
    let private getTags (connString: string) (id: string) : string list =
        use conn = new SqliteConnection(connString)
        conn.Open()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT tags FROM data_entities WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        use reader = cmd.ExecuteReader()
        if reader.Read() && not (reader.IsDBNull(0)) then
            deserializeTags (reader.GetString(0))
        else
            []

    let private handleCreated (data: DataEntityCreatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                INSERT INTO data_entities (id, name, domain, classification, retention, owner, steward, source_system, criticality, pii_flag, tags, created_at, updated_at)
                VALUES ($id, $name, $domain, $classification, $retention, $owner, $steward, $source_system, $criticality, $pii_flag, $tags, $created_at, $updated_at)
                ON CONFLICT(id) DO NOTHING
                """
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$name", data.Name) |> ignore
            addOptionalParam cmd "$domain" (data.Domain |> Option.map box)
            cmd.Parameters.AddWithValue("$classification", data.Classification) |> ignore
            addOptionalParam cmd "$retention" (data.Retention |> Option.map box)
            addOptionalParam cmd "$owner" (data.Owner |> Option.map box)
            addOptionalParam cmd "$steward" (data.Steward |> Option.map box)
            addOptionalParam cmd "$source_system" (data.SourceSystem |> Option.map box)
            addOptionalParam cmd "$criticality" (data.Criticality |> Option.map box)
            cmd.Parameters.AddWithValue("$pii_flag", if data.PiiFlag then 1 else 0) |> ignore
            cmd.Parameters.AddWithValue("$tags", serializeTags data.Tags) |> ignore
            cmd.Parameters.AddWithValue("$created_at", now) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle DataEntityCreated: {ex.Message}"
    
    let private handleClassificationSet (data: ClassificationSetData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE data_entities SET classification = $classification, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$classification", data.NewClassification) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle ClassificationSet: {ex.Message}"
    
    let private handlePIIFlagSet (data: PIIFlagSetData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE data_entities SET pii_flag = $pii_flag, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$pii_flag", if data.NewPiiFlag then 1 else 0) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle PIIFlagSet: {ex.Message}"
    
    let private handleRetentionUpdated (data: RetentionUpdatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE data_entities SET retention = $retention, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            addOptionalParam cmd "$retention" (data.NewRetention |> Option.map box)
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle RetentionUpdated: {ex.Message}"
    
    let private handleTagsAdded (data: DataEntityTagsAddedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            let currentTags = getTags connString data.Id
            let newTags = (currentTags @ data.AddedTags) |> List.distinct
            
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE data_entities SET tags = $tags, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$tags", serializeTags newTags) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle DataEntityTagsAdded: {ex.Message}"
    
    let private handleDeleted (data: DataEntityDeletedData) (connString: string) : Result<unit, string> =
        try
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "DELETE FROM data_entities WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle DataEntityDeleted: {ex.Message}"

    /// Projection handler implementation for DataEntity events
    type Handler(connString: string) =
        interface ProjectionEngine.IProjectionHandler<DataEntityEvent> with
            member _.Handle(envelope: EventEnvelope<DataEntityEvent>) =
                match envelope.Data with
                | DataEntityCreated data -> handleCreated data connString
                | ClassificationSet data -> handleClassificationSet data connString
                | PIIFlagSet data -> handlePIIFlagSet data connString
                | RetentionUpdated data -> handleRetentionUpdated data connString
                | DataEntityTagsAdded data -> handleTagsAdded data connString
                | DataEntityDeleted data -> handleDeleted data connString
            
            member _.ProjectionName = "DataEntityProjection"
            
            member _.CanHandle(eventType: string) =
                match eventType with
                | "DataEntityCreated" | "ClassificationSet" | "PIIFlagSet" | "RetentionUpdated" 
                | "DataEntityTagsAdded" | "DataEntityDeleted" -> true
                | _ -> false
