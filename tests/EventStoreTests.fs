module EventStoreTests

open System
open Xunit
open EATool.Domain
open EATool.Infrastructure.EventStore

[<Fact>]
let ``in-memory event store append and query`` () =
    let store = new InMemoryEventStore<string>() :> IEventStore<string>
    let aggId = Guid.NewGuid()
    let env eType version data : EventEnvelope<string> =
        {
            EventId = Guid.NewGuid()
            EventType = eType
            EventVersion = 1
            EventTimestamp = DateTime.UtcNow
            AggregateId = aggId
            AggregateType = "TestAggregate"
            AggregateVersion = version
            CausationId = None
            CorrelationId = None
            Actor = "user-1"
            ActorType = ActorType.User
            Source = Source.API
            Data = data
            Metadata = None
        }
    let events = [ env "Created" 1 "a"; env "Updated" 2 "b" ]
    match store.Append(events) with
    | Ok () ->
        let all = store.GetEvents(aggId)
        Assert.Equal(2, all.Length)
        let since1 = store.GetEventsSince(aggId, 1)
        Assert.Equal(1, since1.Length)
        Assert.Equal(2, store.GetAggregateVersion(aggId))
    | Error e -> Assert.True(false, e)

[<Fact>]
let ``in-memory idempotency checks`` () =
    let store = new InMemoryEventStore<string>() :> IEventStore<string>
    let cmdId = Guid.NewGuid()
    Assert.False(store.IsCommandProcessed(cmdId))
    store.RecordCommandProcessed(cmdId)
    Assert.True(store.IsCommandProcessed(cmdId))
