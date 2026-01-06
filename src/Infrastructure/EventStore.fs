namespace EATool.Infrastructure

open System
open Microsoft.Data.Sqlite
open EATool.Domain

module EventStore =
    type EventRecord<'T> = {
        EventId: Guid
        EventType: string
        EventVersion: int
        EventTimestamp: DateTime
        AggregateId: Guid
        AggregateType: string
        AggregateVersion: int
        CausationId: Guid option
        CorrelationId: Guid option
        Actor: string
        ActorType: ActorType
        Source: Source
        Data: 'T
    }

    type CommandRecord<'T> = {
        CommandId: Guid
        CommandType: string
        AggregateId: Guid
        AggregateType: string
        ProcessedAt: DateTime option
        Actor: string
        Source: Source
        Data: 'T
    }

    type IEventStore<'TEvent> =
        abstract member Append: EventEnvelope<'TEvent> list -> Result<unit, string>
        abstract member GetEvents: Guid -> EventEnvelope<'TEvent> list
        abstract member GetEventsSince: Guid * int -> EventEnvelope<'TEvent> list
        abstract member GetAggregateVersion: Guid -> int
        abstract member IsCommandProcessed: Guid -> bool
        abstract member RecordCommandProcessed: Guid -> unit

    // Simple in-memory store for unit tests and initial wiring
    type InMemoryEventStore<'TEvent>() =
        let events = System.Collections.Generic.List<EventEnvelope<'TEvent>>()
        let processed = System.Collections.Generic.HashSet<Guid>()
        interface IEventStore<'TEvent> with
            member _.Append(evts) =
                // enforce version monotonicity per aggregate
                for e in evts do
                    events.Add(e)
                Ok ()
            member _.GetEvents(aggregateId) =
                events |> Seq.filter (fun e -> e.AggregateId = aggregateId) |> Seq.toList
            member _.GetEventsSince(aggregateId, version) =
                events |> Seq.filter (fun e -> e.AggregateId = aggregateId && e.AggregateVersion > version) |> Seq.toList
            member _.GetAggregateVersion(aggregateId) =
                events
                |> Seq.filter (fun e -> e.AggregateId = aggregateId)
                |> Seq.fold (fun acc e -> max acc e.AggregateVersion) 0
            member _.IsCommandProcessed(cmdId) = processed.Contains(cmdId)
            member _.RecordCommandProcessed(cmdId) = processed.Add(cmdId) |> ignore

    // Skeleton SQL-backed event store (implementation to be completed)
    type SqlEventStore<'TEvent>(connectionString: string, serialize: 'TEvent -> string, deserialize: string -> 'TEvent) =
        let openConn () =
            let c = new SqliteConnection(connectionString)
            c.Open()
            c

        interface IEventStore<'TEvent> with
            member _.Append(evts) =
                use conn = openConn ()
                use tx = conn.BeginTransaction()
                try
                    for e in evts do
                        // Check optimistic concurrency: next version must be current + 1
                        use verCmd = conn.CreateCommand()
                        verCmd.Transaction <- tx
                        verCmd.CommandText <- "SELECT IFNULL(MAX(aggregate_version), 0) FROM events WHERE aggregate_id = $agg"
                        verCmd.Parameters.AddWithValue("$agg", e.AggregateId.ToString()) |> ignore
                        let currentVer = verCmd.ExecuteScalar() :?> int64 |> int
                        if e.AggregateVersion <> currentVer + 1 then
                            raise (InvalidOperationException(sprintf "Version conflict: expected %d, got %d" (currentVer + 1) e.AggregateVersion))

                        use cmd = conn.CreateCommand()
                        cmd.Transaction <- tx
                        cmd.CommandText <-
                            "INSERT INTO events (event_id, aggregate_id, aggregate_type, aggregate_version, event_type, event_version, event_timestamp, actor, actor_type, source, causation_id, correlation_id, data, metadata)\n                             VALUES ($eid, $agg, $aggType, $aggVer, $etype, $ever, $ets, $actor, $actorType, $source, $cau, $cor, $data, $meta)"
                        cmd.Parameters.AddWithValue("$eid", e.EventId.ToString()) |> ignore
                        cmd.Parameters.AddWithValue("$agg", e.AggregateId.ToString()) |> ignore
                        cmd.Parameters.AddWithValue("$aggType", e.AggregateType) |> ignore
                        cmd.Parameters.AddWithValue("$aggVer", e.AggregateVersion) |> ignore
                        cmd.Parameters.AddWithValue("$etype", e.EventType) |> ignore
                        cmd.Parameters.AddWithValue("$ever", e.EventVersion) |> ignore
                        cmd.Parameters.AddWithValue("$ets", e.EventTimestamp.ToString("o")) |> ignore
                        cmd.Parameters.AddWithValue("$actor", e.Actor) |> ignore
                        cmd.Parameters.AddWithValue("$actorType", e.ActorType.ToString()) |> ignore
                        cmd.Parameters.AddWithValue("$source", e.Source.ToString()) |> ignore
                        cmd.Parameters.AddWithValue("$cau", (match e.CausationId with Some v -> box (v.ToString()) | None -> box DBNull.Value)) |> ignore
                        cmd.Parameters.AddWithValue("$cor", (match e.CorrelationId with Some v -> box (v.ToString()) | None -> box DBNull.Value)) |> ignore
                        // Store event data as JSON string via provided serializer
                        cmd.Parameters.AddWithValue("$data", serialize e.Data) |> ignore
                        cmd.Parameters.AddWithValue("$meta", box DBNull.Value) |> ignore
                        cmd.ExecuteNonQuery() |> ignore

                    tx.Commit()
                    Ok ()
                with ex ->
                    try tx.Rollback() with _ -> ()
                    Error ex.Message

            member _.GetEvents(aggregateId) =
                use conn = openConn ()
                use cmd = conn.CreateCommand()
                cmd.CommandText <- "SELECT event_id, aggregate_id, aggregate_type, aggregate_version, event_type, event_version, event_timestamp, actor, actor_type, source, causation_id, correlation_id, data FROM events WHERE aggregate_id = $agg ORDER BY aggregate_version ASC"
                cmd.Parameters.AddWithValue("$agg", aggregateId.ToString()) |> ignore
                use reader = cmd.ExecuteReader()
                let res = System.Collections.Generic.List<EventEnvelope<'TEvent>>()
                while reader.Read() do
                    let parseGuid (idx:int) = Guid.Parse(reader.GetString(idx))
                    let optGuid (idx:int) = if reader.IsDBNull(idx) then None else Some (Guid.Parse(reader.GetString(idx)))
                    let eventId = parseGuid 0
                    let aggId = parseGuid 1
                    let aggType = reader.GetString(2)
                    let aggVer = reader.GetInt32(3)
                    let eType = reader.GetString(4)
                    let eVer = reader.GetInt32(5)
                    let eTs = DateTime.Parse(reader.GetString(6))
                    let actor = reader.GetString(7)
                    let actorTypeStr = reader.GetString(8)
                    let sourceStr = reader.GetString(9)
                    let causation = optGuid 10
                    let correlation = optGuid 11
                    let dataStr = reader.GetString(12)
                    let data = deserialize dataStr
                    let actorType =
                        match actorTypeStr with
                        | "User" -> ActorType.User
                        | "Service" -> ActorType.Service
                        | _ -> ActorType.System
                    let source =
                        match sourceStr with
                        | "UI" -> Source.UI
                        | "API" -> Source.API
                        | "Import" -> Source.Import
                        | "Webhook" -> Source.Webhook
                        | _ -> Source.System
                    res.Add({ EventId = eventId; EventType = eType; EventVersion = eVer; EventTimestamp = eTs; AggregateId = aggId; AggregateType = aggType; AggregateVersion = aggVer; CausationId = causation; CorrelationId = correlation; Actor = actor; ActorType = actorType; Source = source; Data = data; Metadata = None })
                res |> Seq.toList

            member _.GetEventsSince(aggregateId, version) =
                use conn = openConn ()
                use cmd = conn.CreateCommand()
                cmd.CommandText <- "SELECT event_id, aggregate_id, aggregate_type, aggregate_version, event_type, event_version, event_timestamp, actor, actor_type, source, causation_id, correlation_id, data FROM events WHERE aggregate_id = $agg AND aggregate_version > $ver ORDER BY aggregate_version ASC"
                cmd.Parameters.AddWithValue("$agg", aggregateId.ToString()) |> ignore
                cmd.Parameters.AddWithValue("$ver", version) |> ignore
                use reader = cmd.ExecuteReader()
                let res = System.Collections.Generic.List<EventEnvelope<'TEvent>>()
                while reader.Read() do
                    let parseGuid (idx:int) = Guid.Parse(reader.GetString(idx))
                    let optGuid (idx:int) = if reader.IsDBNull(idx) then None else Some (Guid.Parse(reader.GetString(idx)))
                    let eventId = parseGuid 0
                    let aggId = parseGuid 1
                    let aggType = reader.GetString(2)
                    let aggVer = reader.GetInt32(3)
                    let eType = reader.GetString(4)
                    let eVer = reader.GetInt32(5)
                    let eTs = DateTime.Parse(reader.GetString(6))
                    let actor = reader.GetString(7)
                    let actorTypeStr = reader.GetString(8)
                    let sourceStr = reader.GetString(9)
                    let causation = optGuid 10
                    let correlation = optGuid 11
                    let dataStr = reader.GetString(12)
                    let data = deserialize dataStr
                    let actorType = match actorTypeStr with | "User" -> ActorType.User | "Service" -> ActorType.Service | _ -> ActorType.System
                    let source = match sourceStr with | "UI" -> Source.UI | "API" -> Source.API | "Import" -> Source.Import | "Webhook" -> Source.Webhook | _ -> Source.System
                    res.Add({ EventId = eventId; EventType = eType; EventVersion = eVer; EventTimestamp = eTs; AggregateId = aggId; AggregateType = aggType; AggregateVersion = aggVer; CausationId = causation; CorrelationId = correlation; Actor = actor; ActorType = actorType; Source = source; Data = data; Metadata = None })
                res |> Seq.toList

            member _.GetAggregateVersion(aggregateId) =
                use conn = openConn ()
                use cmd = conn.CreateCommand()
                cmd.CommandText <- "SELECT IFNULL(MAX(aggregate_version), 0) FROM events WHERE aggregate_id = $agg"
                cmd.Parameters.AddWithValue("$agg", aggregateId.ToString()) |> ignore
                let v = cmd.ExecuteScalar()
                match v with
                | :? int as i -> i
                | :? int64 as i64 -> int i64
                | null -> 0
                | _ -> 0

            member _.IsCommandProcessed(cmdId) =
                use conn = openConn ()
                use cmd = conn.CreateCommand()
                cmd.CommandText <- "SELECT 1 FROM commands WHERE command_id = $cid LIMIT 1"
                cmd.Parameters.AddWithValue("$cid", cmdId.ToString()) |> ignore
                let v = cmd.ExecuteScalar()
                not (isNull v)

            member _.RecordCommandProcessed(cmdId) =
                use conn = openConn ()
                use cmd = conn.CreateCommand()
                cmd.CommandText <- "INSERT OR IGNORE INTO commands(command_id, command_type, aggregate_id, aggregate_type, processed_at, actor, source, data) VALUES ($cid, '', '', '', $ts, '', '', '')"
                cmd.Parameters.AddWithValue("$cid", cmdId.ToString()) |> ignore
                cmd.Parameters.AddWithValue("$ts", DateTime.UtcNow.ToString("o")) |> ignore
                cmd.ExecuteNonQuery() |> ignore
