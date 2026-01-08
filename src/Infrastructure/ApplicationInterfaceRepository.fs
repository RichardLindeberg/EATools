/// ApplicationInterface repository backed by SQLite projections
namespace EATool.Infrastructure

open System
open System.Text.Json
open Microsoft.Data.Sqlite
open EATool.Domain

module ApplicationInterfaceRepository =
    let private deserializeList (payload: string) = try JsonSerializer.Deserialize<string list>(payload) with _ -> []
    let private serializeList (items: string list) = JsonSerializer.Serialize(items)
    let private deserializeRateLimits (payload: string) = try JsonSerializer.Deserialize<Map<string, string>>(payload) with _ -> Map.empty

    let private getStringOption (reader: SqliteDataReader) (ordinal: int) = if reader.IsDBNull(ordinal) then None else Some(reader.GetString(ordinal))

    let private statusFromString (value: string) =
        match value.ToLowerInvariant() with
        | "active" -> InterfaceStatus.Active
        | "deprecated" -> InterfaceStatus.Deprecated
        | "retired" -> InterfaceStatus.Retired
        | _ -> InterfaceStatus.Active

    let private mapInterface (reader: SqliteDataReader) : ApplicationInterface =
        let idIdx = reader.GetOrdinal("id")
        let nameIdx = reader.GetOrdinal("name")
        let protocolIdx = reader.GetOrdinal("protocol")
        let endpointIdx = reader.GetOrdinal("endpoint")
        let specIdx = reader.GetOrdinal("specification_url")
        let versionIdx = reader.GetOrdinal("version")
        let authIdx = reader.GetOrdinal("authentication_method")
        let exposedIdx = reader.GetOrdinal("exposed_by_app_id")
        let servesIdx = reader.GetOrdinal("serves_service_ids")
        let rateIdx = reader.GetOrdinal("rate_limits")
        let statusIdx = reader.GetOrdinal("status")
        let tagsIdx = reader.GetOrdinal("tags")
        let createdIdx = reader.GetOrdinal("created_at")
        let updatedIdx = reader.GetOrdinal("updated_at")
        {
            Id = reader.GetString(idIdx)
            Name = reader.GetString(nameIdx)
            Protocol = reader.GetString(protocolIdx)
            Endpoint = getStringOption reader endpointIdx
            SpecificationUrl = getStringOption reader specIdx
            Version = getStringOption reader versionIdx
            AuthenticationMethod = getStringOption reader authIdx
            ExposedByAppId = reader.GetString(exposedIdx)
            ServesServiceIds = reader.GetString(servesIdx) |> deserializeList
            RateLimits = if reader.IsDBNull(rateIdx) then None else Some (deserializeRateLimits (reader.GetString(rateIdx)))
            Status = reader.GetString(statusIdx) |> statusFromString
            Tags = reader.GetString(tagsIdx) |> deserializeList
            CreatedAt = reader.GetString(createdIdx)
            UpdatedAt = reader.GetString(updatedIdx)
        }

    let private addOptionalParam (cmd: SqliteCommand) (name: string) (value: obj option) =
        let p = cmd.CreateParameter()
        p.ParameterName <- name
        p.Value <- value |> Option.defaultValue (box DBNull.Value)
        cmd.Parameters.Add(p) |> ignore

    let private buildFilters (applicationId: string option) (status: InterfaceStatus option) =
        let clauses = System.Collections.Generic.List<string>()
        let parameters = System.Collections.Generic.List<SqliteParameter>()
        match applicationId with
        | Some app when not (String.IsNullOrWhiteSpace app) ->
            clauses.Add("exposed_by_app_id = $app")
            parameters.Add(new SqliteParameter("$app", app))
        | _ -> ()
        match status with
        | Some st ->
            let s =
                match st with
                | InterfaceStatus.Active -> "active"
                | InterfaceStatus.Deprecated -> "deprecated"
                | InterfaceStatus.Retired -> "retired"
            clauses.Add("status = $status")
            parameters.Add(new SqliteParameter("$status", s))
        | None -> ()
        let whereClause = if clauses.Count = 0 then "" else " WHERE " + String.Join(" AND ", clauses)
        whereClause, parameters

    let getAll (page: int) (limit: int) (applicationId: string option) (status: InterfaceStatus option) : PaginatedResponse<ApplicationInterface> =
        let page = if page < 1 then 1 else page
        let limit = if limit < 1 || limit > 200 then 50 else limit
        let offset = (page - 1) * limit
        use conn = Database.getConnection ()
        let whereClause, parameters = buildFilters applicationId status
        use listCmd = conn.CreateCommand()
        listCmd.CommandText <- sprintf "SELECT id, name, protocol, endpoint, specification_url, version, authentication_method, exposed_by_app_id, serves_service_ids, rate_limits, status, tags, created_at, updated_at FROM application_interfaces%s ORDER BY datetime(created_at) DESC LIMIT $limit OFFSET $offset" whereClause
        parameters |> Seq.iter (fun p -> listCmd.Parameters.Add(p) |> ignore)
        listCmd.Parameters.AddWithValue("$limit", limit) |> ignore
        listCmd.Parameters.AddWithValue("$offset", offset) |> ignore
        use reader = listCmd.ExecuteReader()
        let items = [ while reader.Read() do mapInterface reader ]
        use countCmd = conn.CreateCommand()
        countCmd.CommandText <- sprintf "SELECT COUNT(1) FROM application_interfaces%s" whereClause
        parameters |> Seq.iter (fun p -> countCmd.Parameters.Add(new SqliteParameter(p.ParameterName, p.Value)) |> ignore)
        let total = countCmd.ExecuteScalar() :?> int64 |> int
        { Items = items; Page = page; Limit = limit; Total = total }

    let getById (id: string) : ApplicationInterface option =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT id, name, protocol, endpoint, specification_url, version, authentication_method, exposed_by_app_id, serves_service_ids, rate_limits, status, tags, created_at, updated_at FROM application_interfaces WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        use reader = cmd.ExecuteReader()
        if reader.Read() then Some (mapInterface reader) else None

    let getByApplicationId (appId: string) : ApplicationInterface list =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT id, name, protocol, endpoint, specification_url, version, authentication_method, exposed_by_app_id, serves_service_ids, rate_limits, status, tags, created_at, updated_at FROM application_interfaces WHERE exposed_by_app_id = $app"
        cmd.Parameters.AddWithValue("$app", appId) |> ignore
        use reader = cmd.ExecuteReader()
        [ while reader.Read() do mapInterface reader ]

    let clear () =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "DELETE FROM application_interfaces"
        cmd.ExecuteNonQuery() |> ignore
