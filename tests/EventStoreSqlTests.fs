module EventStoreSqlTests

open System
open Xunit
open EATool.Infrastructure
open EATool.Infrastructure.EventStore

[<Fact>]
let ``sql event store append enforces version`` () =
    // use temp file for sqlite
    let tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".db")
    let connString = $"Data Source={tmp};Cache=Shared;Mode=ReadWriteCreate"
    let cfg = { DatabaseConfig.ConnectionString = connString; Environment = "test" }
    match Migrations.run cfg with
    | Error e -> Assert.True(false, e)
    | Ok () ->
        // For string payloads, JSON serializer is identity
        let store = new SqlEventStore<string>(connString, id, id) :> EventStore.IEventStore<string>
        let aggId = Guid.NewGuid()
        let mkEvt ver data : EATool.Domain.EventEnvelope<string> =
            {
                EventId = Guid.NewGuid()
                EventType = "TestEvent"
                EventVersion = 1
                EventTimestamp = DateTime.UtcNow
                AggregateId = aggId
                AggregateType = "TestAggregate"
                AggregateVersion = ver
                CausationId = None
                CorrelationId = None
                Actor = "user-1"
                ActorType = EATool.Domain.ActorType.User
                Source = EATool.Domain.Source.API
                Data = data
                Metadata = None
            }
        // append version 1 then 2 should succeed
        match store.Append([ mkEvt 1 "a"; mkEvt 2 "b" ]) with
        | Ok () -> ()
        | Error e -> Assert.True(false, e)
        // appending version 2 again should fail
        match store.Append([ mkEvt 2 "dup" ]) with
        | Ok () -> Assert.True(false, "Expected version conflict")
        | Error _ -> ()
