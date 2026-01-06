module EventJsonStoreTests

open System
open Xunit
open Thoth.Json.Net
open EATool.Infrastructure
open EATool.Infrastructure.EventJson
open EATool.Infrastructure.EventStore
open EATool.Domain

type TestEvt = { Id: int; Name: string }

let encTestEvt (e: TestEvt) : JsonValue =
    Encode.object [
        "id", Encode.int e.Id
        "name", Encode.string e.Name
    ]

let decTestEvt : Decoder<TestEvt> =
    Decode.object (fun get ->
        {
            Id = get.Required.Field "id" Decode.int
            Name = get.Required.Field "name" Decode.string
        }
    )

[<Fact>]
let ``sql event store json round-trip`` () =
    let tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".db")
    let connString = $"Data Source={tmp};Cache=Shared;Mode=ReadWriteCreate"
    let cfg = { DatabaseConfig.ConnectionString = connString; Environment = "test" }
    match Migrations.run cfg with
    | Error e -> Assert.True(false, e)
    | Ok () ->
        let store = EventJson.createSqlEventStore<TestEvt>(connString, encTestEvt, decTestEvt)
        let aggId = Guid.NewGuid()
        let mk ver payload : EventEnvelope<TestEvt> =
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
                ActorType = ActorType.User
                Source = Source.API
                Data = payload
                Metadata = None
            }
        match store.Append([ mk 1 { Id = 1; Name = "A" }; mk 2 { Id = 2; Name = "B" } ]) with
        | Error e -> Assert.True(false, e)
        | Ok () ->
            let events = store.GetEvents(aggId)
            Assert.Equal(2, events.Length)
            Assert.Equal(1, events.[0].Data.Id)
            Assert.Equal("A", events.[0].Data.Name)
            Assert.Equal(2, events.[1].Data.Id)
            Assert.Equal("B", events.[1].Data.Name)
