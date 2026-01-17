/// Application projection that maintains the applications read model
namespace EATool.Infrastructure.Projections

open System
open System.Text.Json
open Microsoft.Data.Sqlite
open EATool.Domain
open EATool.Infrastructure

module ApplicationProjection =
    
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
        cmd.CommandText <- "SELECT tags FROM applications WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        use reader = cmd.ExecuteReader()
        if reader.Read() && not (reader.IsDBNull(0)) then
            deserializeTags (reader.GetString(0))
        else
            []

    let private handleCreated (data: ApplicationCreatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                INSERT INTO applications (id, name, owner, lifecycle, lifecycle_raw, capability_id, data_classification, tags, created_at, updated_at)
                VALUES ($id, $name, $owner, $lifecycle, $lifecycle, $capability_id, $data_classification, $tags, $created_at, $updated_at)
                ON CONFLICT(id) DO NOTHING
                """
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$name", data.Name) |> ignore
            addOptionalParam cmd "$owner" (data.Owner |> Option.map box)
            cmd.Parameters.AddWithValue("$lifecycle", data.Lifecycle) |> ignore
            addOptionalParam cmd "$capability_id" (data.CapabilityId |> Option.map box)
            addOptionalParam cmd "$data_classification" (data.DataClassification |> Option.map box)
            cmd.Parameters.AddWithValue("$tags", serializeTags data.Tags) |> ignore
            cmd.Parameters.AddWithValue("$created_at", now) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle ApplicationCreated: {ex.Message}"
    
    let private handleDataClassificationChanged (data: DataClassificationChangedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE applications SET data_classification = $classification, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$classification", data.NewClassification) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle DataClassificationChanged: {ex.Message}"
    
    let private handleLifecycleTransitioned (data: LifecycleTransitionedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE applications SET lifecycle = $lifecycle, lifecycle_raw = $lifecycle, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$lifecycle", data.ToLifecycle) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle LifecycleTransitioned: {ex.Message}"
    
    let private handleOwnerSet (data: OwnerSetData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE applications SET owner = $owner, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$owner", data.NewOwner) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle OwnerSet: {ex.Message}"
    
    let private handleCapabilityAssigned (data: CapabilityAssignedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE applications SET capability_id = $capability_id, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$capability_id", data.CapabilityId) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle CapabilityAssigned: {ex.Message}"
    
    let private handleCapabilityRemoved (data: CapabilityRemovedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE applications SET capability_id = NULL, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle CapabilityRemoved: {ex.Message}"
    
    let private handleTagsAdded (data: TagsAddedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            let currentTags = getTags connString data.Id
            let newTags = (currentTags @ data.AddedTags) |> List.distinct
            
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE applications SET tags = $tags, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$tags", serializeTags newTags) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle TagsAdded: {ex.Message}"
    
    let private handleTagsRemoved (data: TagsRemovedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            let currentTags = getTags connString data.Id
            let newTags = currentTags |> List.filter (fun t -> not (List.contains t data.RemovedTags))
            
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE applications SET tags = $tags, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$tags", serializeTags newTags) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle TagsRemoved: {ex.Message}"
    
    let private handleCriticalitySet (data: CriticalitySetData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            // Note: criticality column may not exist yet - this is for future schema
            cmd.CommandText <- "UPDATE applications SET updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle CriticalitySet: {ex.Message}"
    
    let private handleDescriptionUpdated (data: DescriptionUpdatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            // Note: description column may not exist yet - this is for future schema
            cmd.CommandText <- "UPDATE applications SET updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle DescriptionUpdated: {ex.Message}"

    let private handleDeleted (data: ApplicationDeletedData) (connString: string) : Result<unit, string> =
        try
            use conn = new SqliteConnection(connString)
            conn.Open()
            
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "DELETE FROM applications WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle ApplicationDeleted: {ex.Message}"

    /// Projection handler that processes Application events
    type Handler(connString: string) =
        interface ProjectionEngine.IProjectionHandler<ApplicationEvent> with
            member _.ProjectionName = "ApplicationProjection"
            
            member _.CanHandle(eventType: string) =
                match eventType with
                | "ApplicationCreated" 
                | "DataClassificationChanged"
                | "LifecycleTransitioned"
                | "OwnerSet"
                | "CapabilityAssigned"
                | "CapabilityRemoved"
                | "TagsAdded"
                | "TagsRemoved"
                | "CriticalitySet"
                | "DescriptionUpdated"
                | "ApplicationDeleted" -> true
                | _ -> false

            member _.Handle(envelope: EventEnvelope<ApplicationEvent>) =
                match envelope.Data with
                | ApplicationCreated data -> handleCreated data connString
                | DataClassificationChanged data -> handleDataClassificationChanged data connString
                | LifecycleTransitioned data -> handleLifecycleTransitioned data connString
                | OwnerSet data -> handleOwnerSet data connString
                | CapabilityAssigned data -> handleCapabilityAssigned data connString
                | CapabilityRemoved data -> handleCapabilityRemoved data connString
                | TagsAdded data -> handleTagsAdded data connString
                | TagsRemoved data -> handleTagsRemoved data connString
                | EATool.Domain.ApplicationEvent.CriticalitySet data -> handleCriticalitySet data connString
                | DescriptionUpdated data -> handleDescriptionUpdated data connString
                | ApplicationDeleted data -> handleDeleted data connString
