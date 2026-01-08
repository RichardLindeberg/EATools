/// ApplicationService projection maintaining read model
namespace EATool.Infrastructure.Projections

open System
open System.Text.Json
open Microsoft.Data.Sqlite
open EATool.Domain
open EATool.Infrastructure

module ApplicationServiceProjection =
    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let private serializeList (items: string list) = JsonSerializer.Serialize(items)
    let private deserializeList (payload: string) = try JsonSerializer.Deserialize<string list>(payload) with _ -> []

    let private addOptionalParam (cmd: SqliteCommand) (name: string) (value: obj option) =
        let p = cmd.CreateParameter()
        p.ParameterName <- name
        p.Value <- value |> Option.defaultValue (box DBNull.Value)
        cmd.Parameters.Add(p) |> ignore

    let private getExistingList (connString: string) (id: string) (column: string) : string list =
        use conn = new SqliteConnection(connString)
        conn.Open()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- $"SELECT {column} FROM application_services WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        use reader = cmd.ExecuteReader()
        if reader.Read() && not (reader.IsDBNull(0)) then
            deserializeList (reader.GetString(0))
        else []

    let private handleCreated (data: ApplicationServiceCreatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                INSERT INTO application_services (
                    id, name, description, business_capability_id, sla, exposed_by_app_ids, consumers, tags, created_at, updated_at
                ) VALUES (
                    $id, $name, $description, $business_capability_id, $sla, $exposed_by_app_ids, $consumers, $tags, $created_at, $updated_at
                )
                ON CONFLICT(id) DO NOTHING
                """
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$name", data.Name) |> ignore
            addOptionalParam cmd "$description" (data.Description |> Option.map box)
            addOptionalParam cmd "$business_capability_id" (data.BusinessCapabilityId |> Option.map box)
            addOptionalParam cmd "$sla" (data.Sla |> Option.map box)
            cmd.Parameters.AddWithValue("$exposed_by_app_ids", serializeList data.ExposedByAppIds) |> ignore
            cmd.Parameters.AddWithValue("$consumers", serializeList data.Consumers) |> ignore
            cmd.Parameters.AddWithValue("$tags", serializeList data.Tags) |> ignore
            cmd.Parameters.AddWithValue("$created_at", now) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex -> Error ($"Failed to handle ApplicationServiceCreated: {ex.Message}")

    let private handleUpdated (data: ApplicationServiceUpdatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                UPDATE application_services
                SET name = COALESCE($name, name),
                    description = COALESCE($description, description),
                    sla = COALESCE($sla, sla),
                    tags = COALESCE($tags, tags),
                    updated_at = $updated_at
                WHERE id = $id
                """
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            addOptionalParam cmd "$name" (data.Name |> Option.map box)
            addOptionalParam cmd "$description" (data.Description |> Option.map box)
            addOptionalParam cmd "$sla" (data.Sla |> Option.map box)
            let tagsJson = data.Tags |> Option.map serializeList |> Option.map box
            addOptionalParam cmd "$tags" tagsJson
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex -> Error ($"Failed to handle ApplicationServiceUpdated: {ex.Message}")

    let private handleBusinessCapabilitySet (data: BusinessCapabilitySetData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE application_services SET business_capability_id = $bc, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            addOptionalParam cmd "$bc" (data.BusinessCapabilityId |> Option.map box)
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex -> Error ($"Failed to handle BusinessCapabilitySet: {ex.Message}")

    let private handleConsumerAdded (data: ConsumerAddedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            let existing = getExistingList connString data.Id "consumers"
            let updated = (existing @ [ data.ConsumerAppId ]) |> List.distinct
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE application_services SET consumers = $consumers, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$consumers", serializeList updated) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex -> Error ($"Failed to handle ConsumerAdded: {ex.Message}")

    let private handleConsumerRemoved (data: ConsumerRemovedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            let existing = getExistingList connString data.Id "consumers"
            let updated = existing |> List.filter (fun c -> c <> data.ConsumerAppId)
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE application_services SET consumers = $consumers, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$consumers", serializeList updated) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex -> Error ($"Failed to handle ConsumerRemoved: {ex.Message}")

    let private handleDeleted (data: ApplicationServiceDeletedData) (connString: string) : Result<unit, string> =
        try
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "DELETE FROM application_services WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex -> Error ($"Failed to handle ApplicationServiceDeleted: {ex.Message}")

    /// Projection handler implementation
    type Handler(connString: string) =
        interface ProjectionEngine.IProjectionHandler<ApplicationServiceEvent> with
            member _.ProjectionName = "ApplicationServiceProjection"
            member _.CanHandle(eventType: string) =
                match eventType with
                | "ApplicationServiceCreated"
                | "ApplicationServiceUpdated"
                | "BusinessCapabilitySet"
                | "ConsumerAdded"
                | "ConsumerRemoved"
                | "ApplicationServiceDeleted" -> true
                | _ -> false
            member _.Handle(envelope: EventEnvelope<ApplicationServiceEvent>) =
                match envelope.Data with
                | ApplicationServiceCreated d -> handleCreated d connString
                | ApplicationServiceUpdated d -> handleUpdated d connString
                | BusinessCapabilitySet d -> handleBusinessCapabilitySet d connString
                | ConsumerAdded d -> handleConsumerAdded d connString
                | ConsumerRemoved d -> handleConsumerRemoved d connString
                | ApplicationServiceDeleted d -> handleDeleted d connString
