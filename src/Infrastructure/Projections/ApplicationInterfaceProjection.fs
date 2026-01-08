/// ApplicationInterface projection maintaining read model
namespace EATool.Infrastructure.Projections

open System
open System.Text.Json
open Microsoft.Data.Sqlite
open EATool.Domain
open EATool.Infrastructure

module ApplicationInterfaceProjection =
    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let private serializeList (items: string list) = JsonSerializer.Serialize(items)
    let private deserializeList (payload: string) = try JsonSerializer.Deserialize<string list>(payload) with _ -> []

    let private serializeRateLimits (rates: Map<string, string>) = JsonSerializer.Serialize(rates)

    let private addOptionalParam (cmd: SqliteCommand) (name: string) (value: obj option) =
        let p = cmd.CreateParameter()
        p.ParameterName <- name
        p.Value <- value |> Option.defaultValue (box DBNull.Value)
        cmd.Parameters.Add(p) |> ignore

    let private statusToString (status: InterfaceStatus) =
        match status with
        | InterfaceStatus.Active -> "active"
        | InterfaceStatus.Deprecated -> "deprecated"
        | InterfaceStatus.Retired -> "retired"

    let private handleCreated (data: ApplicationInterfaceCreatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                INSERT INTO application_interfaces (
                    id, name, protocol, endpoint, specification_url, version, authentication_method, exposed_by_app_id,
                    serves_service_ids, rate_limits, status, tags, created_at, updated_at
                ) VALUES (
                    $id, $name, $protocol, $endpoint, $specification_url, $version, $authentication_method, $exposed_by_app_id,
                    $serves_service_ids, $rate_limits, $status, $tags, $created_at, $updated_at
                )
                ON CONFLICT(id) DO NOTHING
                """
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$name", data.Name) |> ignore
            cmd.Parameters.AddWithValue("$protocol", data.Protocol) |> ignore
            addOptionalParam cmd "$endpoint" (data.Endpoint |> Option.map box)
            addOptionalParam cmd "$specification_url" (data.SpecificationUrl |> Option.map box)
            addOptionalParam cmd "$version" (data.Version |> Option.map box)
            addOptionalParam cmd "$authentication_method" (data.AuthenticationMethod |> Option.map box)
            cmd.Parameters.AddWithValue("$exposed_by_app_id", data.ExposedByAppId) |> ignore
            cmd.Parameters.AddWithValue("$serves_service_ids", serializeList data.ServesServiceIds) |> ignore
            let rateLimitsJson = data.RateLimits |> Option.map serializeRateLimits |> Option.map box
            addOptionalParam cmd "$rate_limits" rateLimitsJson
            cmd.Parameters.AddWithValue("$status", statusToString data.Status) |> ignore
            cmd.Parameters.AddWithValue("$tags", serializeList data.Tags) |> ignore
            cmd.Parameters.AddWithValue("$created_at", now) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex -> Error ($"Failed to handle ApplicationInterfaceCreated: {ex.Message}")

    let private handleUpdated (data: ApplicationInterfaceUpdatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                UPDATE application_interfaces
                SET name = COALESCE($name, name),
                    protocol = COALESCE($protocol, protocol),
                    endpoint = COALESCE($endpoint, endpoint),
                    version = COALESCE($version, version),
                    authentication_method = COALESCE($authentication_method, authentication_method),
                    tags = COALESCE($tags, tags),
                    updated_at = $updated_at
                WHERE id = $id
                """
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            addOptionalParam cmd "$name" (data.Name |> Option.map box)
            addOptionalParam cmd "$protocol" (data.Protocol |> Option.map box)
            addOptionalParam cmd "$endpoint" (data.Endpoint |> Option.map box)
            addOptionalParam cmd "$version" (data.Version |> Option.map box)
            addOptionalParam cmd "$authentication_method" (data.AuthenticationMethod |> Option.map box)
            let tagsJson = data.Tags |> Option.map serializeList |> Option.map box
            addOptionalParam cmd "$tags" tagsJson
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex -> Error ($"Failed to handle ApplicationInterfaceUpdated: {ex.Message}")

    let private handleServedServicesSet (data: ServedServicesSetData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE application_interfaces SET serves_service_ids = $service_ids, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$service_ids", serializeList data.ServiceIds) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex -> Error ($"Failed to handle ServedServicesSet: {ex.Message}")

    let private handleStatusChanged (data: StatusChangedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE application_interfaces SET status = $status, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$status", statusToString data.Status) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex -> Error ($"Failed to handle StatusChanged: {ex.Message}")

    let private handleDeleted (data: ApplicationInterfaceDeletedData) (connString: string) : Result<unit, string> =
        try
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "DELETE FROM application_interfaces WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex -> Error ($"Failed to handle ApplicationInterfaceDeleted: {ex.Message}")

    /// Projection handler implementation
    type Handler(connString: string) =
        interface ProjectionEngine.IProjectionHandler<ApplicationInterfaceEvent> with
            member _.ProjectionName = "ApplicationInterfaceProjection"
            member _.CanHandle(eventType: string) =
                match eventType with
                | "ApplicationInterfaceCreated"
                | "ApplicationInterfaceUpdated"
                | "ServedServicesSet"
                | "StatusChanged"
                | "ApplicationInterfaceDeleted" -> true
                | _ -> false
            member _.Handle(envelope: EventEnvelope<ApplicationInterfaceEvent>) =
                match envelope.Data with
                | ApplicationInterfaceCreated d -> handleCreated d connString
                | ApplicationInterfaceUpdated d -> handleUpdated d connString
                | ServedServicesSet d -> handleServedServicesSet d connString
                | StatusChanged d -> handleStatusChanged d connString
                | ApplicationInterfaceDeleted d -> handleDeleted d connString
