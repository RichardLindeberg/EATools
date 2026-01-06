/// Application projection that maintains the applications read model
namespace EATool.Infrastructure.Projections

open System
open System.Text.Json
open Microsoft.Data.Sqlite
open EATool.Domain
open EATool.Infrastructure

/// Application domain events
type ApplicationEvent =
    | ApplicationCreated of ApplicationCreatedData
    | ApplicationUpdated of ApplicationUpdatedData
    | ApplicationDeleted of ApplicationDeletedData

and ApplicationCreatedData =
    {
        Id: string
        Name: string
        Owner: string option
        Lifecycle: string
        CapabilityId: string option
        DataClassification: string option
        Tags: string list
    }

and ApplicationUpdatedData =
    {
        Id: string
        Name: string option
        Owner: string option
        Lifecycle: string option
        CapabilityId: string option
        DataClassification: string option
        Tags: string list option
    }

and ApplicationDeletedData =
    {
        Id: string
    }

module ApplicationProjection =
    
    let private serializeTags (tags: string list) =
        JsonSerializer.Serialize(tags)

    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let private addOptionalParam (cmd: SqliteCommand) (name: string) (value: obj option) =
        let p = cmd.CreateParameter()
        p.ParameterName <- name
        p.Value <- value |> Option.defaultValue (box DBNull.Value)
        cmd.Parameters.Add(p) |> ignore

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

    let private handleUpdated (data: ApplicationUpdatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()

            // Build dynamic update based on provided fields
            let updates = ResizeArray<string>()
            let addUpdate field = updates.Add(field)

            data.Name |> Option.iter (fun _ -> addUpdate "name = $name")
            data.Owner |> Option.iter (fun _ -> addUpdate "owner = $owner")
            data.Lifecycle |> Option.iter (fun _ -> addUpdate "lifecycle = $lifecycle")
            data.CapabilityId |> Option.iter (fun _ -> addUpdate "capability_id = $capability_id")
            data.DataClassification |> Option.iter (fun _ -> addUpdate "data_classification = $data_classification")
            data.Tags |> Option.iter (fun _ -> addUpdate "tags = $tags")
            addUpdate "updated_at = $updated_at"

            if updates.Count > 0 then
                use cmd = conn.CreateCommand()
                cmd.CommandText <- sprintf "UPDATE applications SET %s WHERE id = $id" (String.Join(", ", updates))
                cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
                data.Name |> Option.iter (fun v -> cmd.Parameters.AddWithValue("$name", v) |> ignore)
                data.Owner |> Option.iter (fun v -> addOptionalParam cmd "$owner" (Some (box v)))
                data.Lifecycle |> Option.iter (fun v -> cmd.Parameters.AddWithValue("$lifecycle", v) |> ignore)
                data.CapabilityId |> Option.iter (fun v -> addOptionalParam cmd "$capability_id" (Some (box v)))
                data.DataClassification |> Option.iter (fun v -> addOptionalParam cmd "$data_classification" (Some (box v)))
                data.Tags |> Option.iter (fun v -> cmd.Parameters.AddWithValue("$tags", serializeTags v) |> ignore)
                cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

                cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle ApplicationUpdated: {ex.Message}"

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
                | "ApplicationCreated" | "ApplicationUpdated" | "ApplicationDeleted" -> true
                | _ -> false

            member _.Handle(envelope: EventEnvelope<ApplicationEvent>) =
                match envelope.Data with
                | ApplicationCreated data -> handleCreated data connString
                | ApplicationUpdated data -> handleUpdated data connString
                | ApplicationDeleted data -> handleDeleted data connString
