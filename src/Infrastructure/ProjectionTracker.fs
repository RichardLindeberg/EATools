namespace EATool.Infrastructure

open System
open Microsoft.Data.Sqlite

module ProjectionTracker =
    type ProjectionStatus = Active | Rebuilding | Failed

    type ProjectionState = {
        ProjectionName: string
        LastProcessedEventId: Guid option
        LastProcessedAt: DateTime option
        LastProcessedVersion: int64
        Status: ProjectionStatus
    }

    let private parseStatus (s: string) =
        match s.ToLowerInvariant() with
        | "active" -> Active
        | "rebuilding" -> Rebuilding
        | "failed" -> Failed
        | _ -> Active

    let private statusToString = function
        | Active -> "active"
        | Rebuilding -> "rebuilding"
        | Failed -> "failed"

    let getProjectionState (connString: string) (projectionName: string) : ProjectionState option =
        use conn = new SqliteConnection(connString)
        conn.Open()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT projection_name, last_processed_event_id, last_processed_at, last_processed_version, status FROM projection_state WHERE projection_name = $name"
        cmd.Parameters.AddWithValue("$name", projectionName) |> ignore
        use reader = cmd.ExecuteReader()
        if reader.Read() then
            let optGuid idx = if reader.IsDBNull(idx) then None else Some (Guid.Parse(reader.GetString(idx)))
            let optDate idx = if reader.IsDBNull(idx) then None else Some (DateTime.Parse(reader.GetString(idx)))
            Some {
                ProjectionName = reader.GetString(0)
                LastProcessedEventId = optGuid 1
                LastProcessedAt = optDate 2
                LastProcessedVersion = reader.GetInt64(3)
                Status = parseStatus (reader.GetString(4))
            }
        else None

    let updateLastProcessed (connString: string) (projectionName: string) (eventId: Guid) (version: int64) : Result<unit, string> =
        try
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- 
                "INSERT INTO projection_state(projection_name, last_processed_event_id, last_processed_at, last_processed_version, status)
                 VALUES ($name, $eid, $ts, $ver, 'active')
                 ON CONFLICT(projection_name) DO UPDATE SET
                   last_processed_event_id = $eid,
                   last_processed_at = $ts,
                   last_processed_version = $ver"
            cmd.Parameters.AddWithValue("$name", projectionName) |> ignore
            cmd.Parameters.AddWithValue("$eid", eventId.ToString()) |> ignore
            cmd.Parameters.AddWithValue("$ts", DateTime.UtcNow.ToString("o")) |> ignore
            cmd.Parameters.AddWithValue("$ver", version) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex -> Error ex.Message

    let markStatus (connString: string) (projectionName: string) (status: ProjectionStatus) : Result<unit, string> =
        try
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- 
                "INSERT INTO projection_state(projection_name, status, last_processed_version)
                 VALUES ($name, $status, 0)
                 ON CONFLICT(projection_name) DO UPDATE SET status = $status"
            cmd.Parameters.AddWithValue("$name", projectionName) |> ignore
            cmd.Parameters.AddWithValue("$status", statusToString status) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex -> Error ex.Message
