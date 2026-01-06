module ProjectionTests

open System
open Xunit
open EATool.Domain
open EATool.Infrastructure
open EATool.Infrastructure.EventStore
open EATool.Infrastructure.ProjectionEngine

type TestEvent = | Created of string | Updated of string

type TestProjectionHandler(connString: string) =
    let mutable events = []
    
    interface IProjectionHandler<TestEvent> with
        member _.ProjectionName = "TestProjection"
        member _.CanHandle(eventType) = eventType = "Created" || eventType = "Updated"
        member _.Handle(evt) =
            events <- evt :: events
            Ok ()
    
    member _.GetProcessedEvents() = List.rev events

[<Fact>]
let ``projection engine routes events to handlers`` () =
    let tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".db")
    let connString = $"Data Source={tmp};Cache=Shared;Mode=ReadWriteCreate"
    let cfg = { DatabaseConfig.ConnectionString = connString; Environment = "test" }
    match Migrations.run cfg with
    | Error e -> Assert.True(false, e)
    | Ok () ->
        let store = new InMemoryEventStore<TestEvent>() :> IEventStore<TestEvent>
        let handler = TestProjectionHandler(connString)
        let engine = ProjectionEngine<TestEvent>(connString, store, [handler])
        
        let aggId = Guid.NewGuid()
        let mkEvt ver evtType data : EventEnvelope<TestEvent> =
            {
                EventId = Guid.NewGuid()
                EventType = evtType
                EventVersion = 1
                EventTimestamp = DateTime.UtcNow
                AggregateId = aggId
                AggregateType = "TestAggregate"
                AggregateVersion = ver
                CausationId = None
                CorrelationId = None
                Actor = "test"
                ActorType = ActorType.System
                Source = Source.API
                Data = data
                Metadata = None
            }
        
        let evt1 = mkEvt 1 "Created" (Created "test")
        let evt2 = mkEvt 2 "Updated" (Updated "modified")
        
        match store.Append([evt1; evt2]) with
        | Error e -> Assert.True(false, e)
        | Ok () ->
            match engine.ProjectAggregate(aggId) with
            | Error e -> Assert.True(false, e)
            | Ok () ->
                let processed = handler.GetProcessedEvents()
                Assert.Equal(2, processed.Length)
                Assert.Equal("Created", processed.[0].EventType)
                Assert.Equal("Updated", processed.[1].EventType)

[<Fact>]
let ``projection tracker updates state`` () =
    let tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".db")
    let connString = $"Data Source={tmp};Cache=Shared;Mode=ReadWriteCreate"
    let cfg = { DatabaseConfig.ConnectionString = connString; Environment = "test" }
    match Migrations.run cfg with
    | Error e -> Assert.True(false, e)
    | Ok () ->
        let projName = "TestProjection"
        let eventId = Guid.NewGuid()
        
        match ProjectionTracker.updateLastProcessed connString projName eventId 42L with
        | Error e -> Assert.True(false, e)
        | Ok () ->
            let state = ProjectionTracker.getProjectionState connString projName
            Assert.True(state.IsSome)
            Assert.Equal(projName, state.Value.ProjectionName)
            Assert.Equal(Some eventId, state.Value.LastProcessedEventId)
            Assert.Equal(42L, state.Value.LastProcessedVersion)
