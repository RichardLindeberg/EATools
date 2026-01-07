/// BusinessCapability projection that maintains the business_capabilities read model
namespace EATool.Infrastructure.Projections

open System
open Microsoft.Data.Sqlite
open EATool.Domain
open EATool.Infrastructure

module BusinessCapabilityProjection =
    
    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let private addOptionalParam (cmd: SqliteCommand) (name: string) (value: obj option) =
        let p = cmd.CreateParameter()
        p.ParameterName <- name
        p.Value <- value |> Option.defaultValue (box DBNull.Value)
        cmd.Parameters.Add(p) |> ignore
    
    let private handleCreated (data: CapabilityCreatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                INSERT INTO business_capabilities (id, name, parent_id, description, created_at, updated_at)
                VALUES ($id, $name, $parent_id, $description, $created_at, $updated_at)
                ON CONFLICT(id) DO NOTHING
                """
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$name", data.Name) |> ignore
            addOptionalParam cmd "$parent_id" (data.ParentId |> Option.map box)
            addOptionalParam cmd "$description" (data.Description |> Option.map box)
            cmd.Parameters.AddWithValue("$created_at", now) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle CapabilityCreated: {ex.Message}"
    
    let private handleParentAssigned (data: CapabilityParentAssignedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE business_capabilities SET parent_id = $parent_id, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$parent_id", data.NewParentId) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle ParentAssigned: {ex.Message}"
    
    let private handleParentRemoved (data: CapabilityParentRemovedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE business_capabilities SET parent_id = NULL, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle ParentRemoved: {ex.Message}"
    
    let private handleDescriptionUpdated (data: CapabilityDescriptionUpdatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE business_capabilities SET description = $description, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            addOptionalParam cmd "$description" (data.NewDescription |> Option.map box)
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle DescriptionUpdated: {ex.Message}"
    
    let private handleDeleted (data: CapabilityDeletedData) (connString: string) : Result<unit, string> =
        try
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "DELETE FROM business_capabilities WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle CapabilityDeleted: {ex.Message}"
    
    /// Projection handler that processes BusinessCapability events
    type Handler(connString: string) =
        interface ProjectionEngine.IProjectionHandler<BusinessCapabilityEvent> with
            member _.ProjectionName = "BusinessCapabilityProjection"
            
            member _.CanHandle(eventType: string) =
                match eventType with
                | "CapabilityCreated"
                | "CapabilityParentAssigned"
                | "CapabilityParentRemoved"
                | "CapabilityDescriptionUpdated"
                | "CapabilityDeleted" -> true
                | _ -> false

            member _.Handle(envelope: EventEnvelope<BusinessCapabilityEvent>) =
                match envelope.Data with
                | CapabilityCreated data -> handleCreated data connString
                | CapabilityParentAssigned data -> handleParentAssigned data connString
                | CapabilityParentRemoved data -> handleParentRemoved data connString
                | CapabilityDescriptionUpdated data -> handleDescriptionUpdated data connString
                | CapabilityDeleted data -> handleDeleted data connString
