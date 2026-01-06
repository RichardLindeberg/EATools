module CommandFrameworkTests

open System
open Xunit
open EATool.Domain
open EATool.Domain.Validation

// Dummy handler to exercise dispatcher logic
type DummyHandler() =
    interface ICommandHandler<string, int, string> with
        member _.ValidateCommand cmd = if cmd = "bad-cmd" then Error ["invalid command"] else Ok ()
        member _.ValidateBusinessRules _ aggregate = if aggregate < 0 then Error "invalid aggregate" else Ok ()
        member _.Handle cmd _ =
            let evt : EventEnvelope<string> =
                {
                    EventId = Guid.NewGuid()
                    EventType = "TestEvent"
                    EventVersion = 1
                    EventTimestamp = DateTime.UtcNow
                    AggregateId = cmd.AggregateId
                    AggregateType = cmd.AggregateType
                    AggregateVersion = 1
                    CausationId = Some cmd.CommandId
                    CorrelationId = cmd.CorrelationId
                    Actor = cmd.Actor
                    ActorType = cmd.ActorType
                    Source = cmd.Source
                    Data = cmd.Data
                    Metadata = None
                }
            Success [evt]

[<Fact>]
let ``validation combinator aggregates errors`` () =
    let result = (validateRequired "" "name") <*> (validateLength 1 3 "toolong" "name")
    match result with
    | Error errs ->
        Assert.Equal(2, errs.Length)
        Assert.Contains("name is required", errs)
        Assert.Contains("name must be between 1 and 3 characters", errs)
    | Ok _ -> Assert.True(false, "Expected validation errors")

[<Fact>]
let ``dispatcher rejects duplicate command`` () =
    let deps : DispatchDependencies<int, string> =
        {
            LoadAggregate = fun _ -> Ok 0
            IsProcessed = fun _ -> true
            MarkProcessed = fun _ -> ()
            SaveEvents = fun _ -> Ok ()
        }
    let handler = DummyHandler() :> ICommandHandler<string, int, string>
    let cmd : CommandEnvelope<string> =
        {
            CommandId = Guid.NewGuid()
            CommandType = "Test"
            AggregateId = Guid.NewGuid()
            AggregateType = "TestAggregate"
            CorrelationId = None
            Actor = "user-1"
            ActorType = ActorType.User
            Source = Source.API
            CommandTimestamp = DateTime.UtcNow
            Data = "ok"
            Metadata = None
        }
    let result = CommandDispatcher.dispatch deps handler cmd
    match result with
    | ValidationError errs -> Assert.Contains("Duplicate command", errs)
    | _ -> Assert.True(false, "Expected duplicate command validation error")

[<Fact>]
let ``dispatcher saves events and marks processed`` () =
    let mutable marked = false
    let mutable saved = false
    let deps : DispatchDependencies<int, string> =
        {
            LoadAggregate = fun _ -> Ok 0
            IsProcessed = fun _ -> false
            MarkProcessed = fun _ -> marked <- true
            SaveEvents = fun _ -> saved <- true; Ok ()
        }
    let handler = DummyHandler() :> ICommandHandler<string, int, string>
    let cmd : CommandEnvelope<string> =
        {
            CommandId = Guid.NewGuid()
            CommandType = "Test"
            AggregateId = Guid.NewGuid()
            AggregateType = "TestAggregate"
            CorrelationId = None
            Actor = "user-1"
            ActorType = ActorType.User
            Source = Source.API
            CommandTimestamp = DateTime.UtcNow
            Data = "ok"
            Metadata = None
        }
    let result = CommandDispatcher.dispatch deps handler cmd
    match result with
    | Success evts ->
        Assert.True(saved, "events should be saved")
        Assert.True(marked, "command should be marked processed")
        Assert.Equal(1, evts.Length)
    | _ -> Assert.True(false, "Expected success result")
