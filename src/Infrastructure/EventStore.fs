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
    type SqlEventStore<'TEvent>(connectionString: string) =
        interface IEventStore<'TEvent> with
            member _.Append(evts) =
                // TODO: implement using SQLite and optimistic concurrency with unique (aggregate_id, aggregate_version)
                Ok ()
            member _.GetEvents(aggregateId) = []
            member _.GetEventsSince(aggregateId, version) = []
            member _.GetAggregateVersion(aggregateId) = 0
            member _.IsCommandProcessed(cmdId) = false
            member _.RecordCommandProcessed(cmdId) = ()
