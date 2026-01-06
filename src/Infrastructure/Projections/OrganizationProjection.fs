/// Organization projection that maintains the organizations read model
namespace EATool.Infrastructure.Projections

open System
open System.Text.Json
open Microsoft.Data.Sqlite
open EATool.Domain
open EATool.Infrastructure

/// Organization domain events
type OrganizationEvent =
    | OrganizationCreated of OrganizationCreatedData
    | OrganizationUpdated of OrganizationUpdatedData
    | OrganizationDeleted of OrganizationDeletedData

and OrganizationCreatedData =
    {
        Id: string
        Name: string
        ParentId: string option
        Domains: string list
        Contacts: string list
    }

and OrganizationUpdatedData =
    {
        Id: string
        Name: string option
        ParentId: string option
        Domains: string list option
        Contacts: string list option
    }

and OrganizationDeletedData =
    {
        Id: string
    }

module OrganizationProjection =
    
    let private serializeList (items: string list) =
        JsonSerializer.Serialize(items)

    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let private addOptionalParam (cmd: SqliteCommand) (name: string) (value: obj option) =
        let p = cmd.CreateParameter()
        p.ParameterName <- name
        p.Value <- value |> Option.defaultValue (box DBNull.Value)
        cmd.Parameters.Add(p) |> ignore

    let private handleCreated (data: OrganizationCreatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                INSERT INTO organizations (id, name, parent_id, domains, contacts, created_at, updated_at)
                VALUES ($id, $name, $parent_id, $domains, $contacts, $created_at, $updated_at)
                ON CONFLICT(id) DO NOTHING
                """
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$name", data.Name) |> ignore
            addOptionalParam cmd "$parent_id" (data.ParentId |> Option.map box)
            cmd.Parameters.AddWithValue("$domains", serializeList data.Domains) |> ignore
            cmd.Parameters.AddWithValue("$contacts", serializeList data.Contacts) |> ignore
            cmd.Parameters.AddWithValue("$created_at", now) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle OrganizationCreated: {ex.Message}"

    let private handleUpdated (data: OrganizationUpdatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()

            // Build dynamic update based on provided fields
            let updates = ResizeArray<string>()
            let addUpdate field = updates.Add(field)

            data.Name |> Option.iter (fun _ -> addUpdate "name = $name")
            data.ParentId |> Option.iter (fun _ -> addUpdate "parent_id = $parent_id")
            data.Domains |> Option.iter (fun _ -> addUpdate "domains = $domains")
            data.Contacts |> Option.iter (fun _ -> addUpdate "contacts = $contacts")
            addUpdate "updated_at = $updated_at"

            if updates.Count > 0 then
                use cmd = conn.CreateCommand()
                cmd.CommandText <- sprintf "UPDATE organizations SET %s WHERE id = $id" (String.Join(", ", updates))
                cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
                data.Name |> Option.iter (fun v -> cmd.Parameters.AddWithValue("$name", v) |> ignore)
                data.ParentId |> Option.iter (fun v -> addOptionalParam cmd "$parent_id" (Some (box v)))
                data.Domains |> Option.iter (fun v -> cmd.Parameters.AddWithValue("$domains", serializeList v) |> ignore)
                data.Contacts |> Option.iter (fun v -> cmd.Parameters.AddWithValue("$contacts", serializeList v) |> ignore)
                cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

                cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle OrganizationUpdated: {ex.Message}"

    let private handleDeleted (data: OrganizationDeletedData) (connString: string) : Result<unit, string> =
        try
            use conn = new SqliteConnection(connString)
            conn.Open()
            
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "DELETE FROM organizations WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle OrganizationDeleted: {ex.Message}"

    /// Projection handler that processes Organization events
    type Handler(connString: string) =
        interface ProjectionEngine.IProjectionHandler<OrganizationEvent> with
            member _.ProjectionName = "OrganizationProjection"
            
            member _.CanHandle(eventType: string) =
                match eventType with
                | "OrganizationCreated" | "OrganizationUpdated" | "OrganizationDeleted" -> true
                | _ -> false

            member _.Handle(envelope: EventEnvelope<OrganizationEvent>) =
                match envelope.Data with
                | OrganizationCreated data -> handleCreated data connString
                | OrganizationUpdated data -> handleUpdated data connString
                | OrganizationDeleted data -> handleDeleted data connString
