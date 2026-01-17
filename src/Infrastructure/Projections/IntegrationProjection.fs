/// Integration projection that maintains the integrations read model
namespace EATool.Infrastructure.Projections

open System
open System.Text.Json
open Microsoft.Data.Sqlite
open EATool.Domain
open EATool.Infrastructure

module IntegrationProjection =
    
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
        cmd.CommandText <- "SELECT tags FROM integrations WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        use reader = cmd.ExecuteReader()
        if reader.Read() && not (reader.IsDBNull(0)) then
            deserializeTags (reader.GetString(0))
        else
            []

    let private handleCreated (data: IntegrationCreatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                INSERT INTO integrations (id, source_app_id, target_app_id, protocol, data_contract, sla, frequency, tags, created_at, updated_at)
                VALUES ($id, $source_app_id, $target_app_id, $protocol, $data_contract, $sla, $frequency, $tags, $created_at, $updated_at)
                ON CONFLICT(id) DO NOTHING
                """
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$source_app_id", data.SourceAppId) |> ignore
            cmd.Parameters.AddWithValue("$target_app_id", data.TargetAppId) |> ignore
            cmd.Parameters.AddWithValue("$protocol", data.Protocol) |> ignore
            addOptionalParam cmd "$data_contract" (data.DataContract |> Option.map box)
            addOptionalParam cmd "$sla" (data.Sla |> Option.map box)
            addOptionalParam cmd "$frequency" (data.Frequency |> Option.map box)
            cmd.Parameters.AddWithValue("$tags", serializeTags data.Tags) |> ignore
            cmd.Parameters.AddWithValue("$created_at", now) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle IntegrationCreated: {ex.Message}"
    
    let private handleProtocolUpdated (data: ProtocolUpdatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE integrations SET protocol = $protocol, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$protocol", data.NewProtocol) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle ProtocolUpdated: {ex.Message}"
    
    let private handleSLASet (data: SLASetData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE integrations SET sla = $sla, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            addOptionalParam cmd "$sla" (data.NewSla |> Option.map box)
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle SLASet: {ex.Message}"
    
    let private handleFrequencySet (data: FrequencySetData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE integrations SET frequency = $frequency, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            addOptionalParam cmd "$frequency" (data.NewFrequency |> Option.map box)
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle FrequencySet: {ex.Message}"
    
    let private handleDataContractUpdated (data: DataContractUpdatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE integrations SET data_contract = $data_contract, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            addOptionalParam cmd "$data_contract" (data.NewDataContract |> Option.map box)
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle DataContractUpdated: {ex.Message}"
    
    let private handleSourceAppSet (data: SourceAppSetData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE integrations SET source_app_id = $source_app_id, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$source_app_id", data.NewSourceAppId) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle SourceAppSet: {ex.Message}"
    
    let private handleTargetAppSet (data: TargetAppSetData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE integrations SET target_app_id = $target_app_id, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$target_app_id", data.NewTargetAppId) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle TargetAppSet: {ex.Message}"
    
    let private handleTagsAdded (data: IntegrationTagsAddedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            let currentTags = getTags connString data.Id
            let newTags = (currentTags @ data.AddedTags) |> List.distinct
            
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE integrations SET tags = $tags, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$tags", serializeTags newTags) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle IntegrationTagsAdded: {ex.Message}"
    
    let private handleTagsRemoved (data: IntegrationTagsRemovedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            let currentTags = getTags connString data.Id
            let newTags = currentTags |> List.filter (fun t -> not (List.contains t data.RemovedTags))
            
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE integrations SET tags = $tags, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$tags", serializeTags newTags) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle IntegrationTagsRemoved: {ex.Message}"
    
    let private handleDeleted (data: IntegrationDeletedData) (connString: string) : Result<unit, string> =
        try
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "DELETE FROM integrations WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle IntegrationDeleted: {ex.Message}"

    /// Projection handler implementation for Integration events
    type Handler(connString: string) =
        interface ProjectionEngine.IProjectionHandler<IntegrationEvent> with
            member _.Handle(envelope: EventEnvelope<IntegrationEvent>) =
                match envelope.Data with
                | IntegrationCreated data -> handleCreated data connString
                | ProtocolUpdated data -> handleProtocolUpdated data connString
                | SLASet data -> handleSLASet data connString
                | FrequencySet data -> handleFrequencySet data connString
                | DataContractUpdated data -> handleDataContractUpdated data connString
                | SourceAppSet data -> handleSourceAppSet data connString
                | TargetAppSet data -> handleTargetAppSet data connString
                | IntegrationTagsAdded data -> handleTagsAdded data connString
                | IntegrationTagsRemoved data -> handleTagsRemoved data connString
                | IntegrationDeleted data -> handleDeleted data connString
            
            member _.ProjectionName = "IntegrationProjection"
            
            member _.CanHandle(eventType: string) =
                match eventType with
                | "IntegrationCreated" | "ProtocolUpdated" | "SLASet" | "FrequencySet" 
                | "DataContractUpdated" | "SourceAppSet" | "TargetAppSet" 
                | "IntegrationTagsAdded" | "IntegrationTagsRemoved" | "IntegrationDeleted" -> true
                | _ -> false
