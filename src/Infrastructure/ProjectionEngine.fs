namespace EATool.Infrastructure

open System
open EATool.Domain
open EATool.Infrastructure.EventStore

module ProjectionEngine =
    /// Handler for a specific projection
    type IProjectionHandler<'TEvent> =
        abstract member ProjectionName: string
        abstract member Handle: EventEnvelope<'TEvent> -> Result<unit, string>
        abstract member CanHandle: string -> bool

    /// Core projection engine that routes events to handlers
    type ProjectionEngine<'TEvent>(connectionString: string, eventStore: IEventStore<'TEvent>, handlers: IProjectionHandler<'TEvent> list) =
        
        /// Process a single event through all applicable handlers
        member this.ProcessEvent(evt: EventEnvelope<'TEvent>) : Result<unit, string> =
            let applicableHandlers = handlers |> List.filter (fun h -> h.CanHandle evt.EventType)
            let results = applicableHandlers |> List.map (fun h -> 
                match h.Handle evt with
                | Ok () -> 
                    ProjectionTracker.updateLastProcessed connectionString h.ProjectionName evt.EventId (int64 evt.AggregateVersion)
                | Error e -> Error e
            )
            // Return first error or Ok
            results |> List.tryFind (function Error _ -> true | _ -> false) |> Option.defaultValue (Ok ())

        /// Project events for a specific aggregate from last checkpoint
        member this.ProjectAggregate(aggregateId: Guid, ?sinceVersion: int) : Result<unit, string> =
            let events = 
                match sinceVersion with
                | Some v -> eventStore.GetEventsSince(aggregateId, v)
                | None -> eventStore.GetEvents(aggregateId)
            
            events |> List.fold (fun acc evt ->
                match acc with
                | Error _ -> acc
                | Ok () -> this.ProcessEvent evt
            ) (Ok ())

        /// Rebuild a projection from scratch
        member this.RebuildProjection(projectionName: string) : Result<unit, string> =
            // Mark as rebuilding
            match ProjectionTracker.markStatus connectionString projectionName ProjectionTracker.Rebuilding with
            | Error e -> Error e
            | Ok () ->
                // TODO: Clear projection tables (handler-specific)
                // TODO: Replay all events (requires event store query by type)
                // For now, just mark as active
                ProjectionTracker.markStatus connectionString projectionName ProjectionTracker.Active
