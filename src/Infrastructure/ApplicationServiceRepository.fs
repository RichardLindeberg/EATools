/// ApplicationService repository backed by SQLite projections
namespace EATool.Infrastructure

open System
open System.Text.Json
open Microsoft.Data.Sqlite
open EATool.Domain

module ApplicationServiceRepository =
    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let private deserializeList (payload: string) =
        try JsonSerializer.Deserialize<string list>(payload) with _ -> []

    let private serializeList (items: string list) = JsonSerializer.Serialize(items)

    let private getStringOption (reader: SqliteDataReader) (ordinal: int) = if reader.IsDBNull(ordinal) then None else Some(reader.GetString(ordinal))

    let private mapService (reader: SqliteDataReader) : ApplicationService =
        let idIdx = reader.GetOrdinal("id")
        let nameIdx = reader.GetOrdinal("name")
        let descIdx = reader.GetOrdinal("description")
        let bcIdx = reader.GetOrdinal("business_capability_id")
        let slaIdx = reader.GetOrdinal("sla")
        let exposedIdx = reader.GetOrdinal("exposed_by_app_ids")
        let consumersIdx = reader.GetOrdinal("consumers")
        let tagsIdx = reader.GetOrdinal("tags")
        let createdIdx = reader.GetOrdinal("created_at")
        let updatedIdx = reader.GetOrdinal("updated_at")
        {
            Id = reader.GetString(idIdx)
            Name = reader.GetString(nameIdx)
            Description = getStringOption reader descIdx
            BusinessCapabilityId = getStringOption reader bcIdx
            Sla = getStringOption reader slaIdx
            ExposedByAppIds = reader.GetString(exposedIdx) |> deserializeList
            Consumers = reader.GetString(consumersIdx) |> deserializeList
            Tags = reader.GetString(tagsIdx) |> deserializeList
            CreatedAt = reader.GetString(createdIdx)
            UpdatedAt = reader.GetString(updatedIdx)
        }

    let private addOptionalParam (cmd: SqliteCommand) (name: string) (value: obj option) =
        let p = cmd.CreateParameter()
        p.ParameterName <- name
        p.Value <- value |> Option.defaultValue (box DBNull.Value)
        cmd.Parameters.Add(p) |> ignore

    let private buildFilters (businessCapabilityId: string option) =
        let clauses = System.Collections.Generic.List<string>()
        let parameters = System.Collections.Generic.List<SqliteParameter>()
        match businessCapabilityId with
        | Some bc when not (String.IsNullOrWhiteSpace bc) ->
            clauses.Add("business_capability_id = $bc")
            parameters.Add(new SqliteParameter("$bc", bc))
        | _ -> ()
        let whereClause = if clauses.Count = 0 then "" else " WHERE " + String.Join(" AND ", clauses)
        whereClause, parameters

    let getAll (page: int) (limit: int) (businessCapabilityId: string option) : PaginatedResponse<ApplicationService> =
        let page = if page < 1 then 1 else page
        let limit = if limit < 1 || limit > 200 then 50 else limit
        let offset = (page - 1) * limit
        use conn = Database.getConnection ()
        let whereClause, parameters = buildFilters businessCapabilityId
        use listCmd = conn.CreateCommand()
        listCmd.CommandText <- sprintf "SELECT id, name, description, business_capability_id, sla, exposed_by_app_ids, consumers, tags, created_at, updated_at FROM application_services%s ORDER BY datetime(created_at) DESC LIMIT $limit OFFSET $offset" whereClause
        parameters |> Seq.iter (fun p -> listCmd.Parameters.Add(p) |> ignore)
        listCmd.Parameters.AddWithValue("$limit", limit) |> ignore
        listCmd.Parameters.AddWithValue("$offset", offset) |> ignore
        use reader = listCmd.ExecuteReader()
        let items = [ while reader.Read() do mapService reader ]
        use countCmd = conn.CreateCommand()
        countCmd.CommandText <- sprintf "SELECT COUNT(1) FROM application_services%s" whereClause
        parameters |> Seq.iter (fun p -> countCmd.Parameters.Add(new SqliteParameter(p.ParameterName, p.Value)) |> ignore)
        let total = countCmd.ExecuteScalar() :?> int64 |> int
        { Items = items; Page = page; Limit = limit; Total = total }

    let getById (id: string) : ApplicationService option =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT id, name, description, business_capability_id, sla, exposed_by_app_ids, consumers, tags, created_at, updated_at FROM application_services WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        use reader = cmd.ExecuteReader()
        if reader.Read() then Some (mapService reader) else None

    let getByBusinessCapabilityId (capId: string) : ApplicationService list =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT id, name, description, business_capability_id, sla, exposed_by_app_ids, consumers, tags, created_at, updated_at FROM application_services WHERE business_capability_id = $bc"
        cmd.Parameters.AddWithValue("$bc", capId) |> ignore
        use reader = cmd.ExecuteReader()
        [ while reader.Read() do mapService reader ]

    let clear () =
        use conn = Database.getConnection ()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "DELETE FROM application_services"
        cmd.ExecuteNonQuery() |> ignore
