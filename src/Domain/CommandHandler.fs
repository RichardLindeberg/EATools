namespace EATool.Domain

open System

/// Command handler abstraction responsible for validation and event creation
type ICommandHandler<'TCommand, 'TAggregate, 'TEvent> =
    abstract member ValidateCommand : 'TCommand -> Result<unit, string list>
    abstract member ValidateBusinessRules : 'TCommand -> 'TAggregate -> Result<unit, string>
    abstract member Handle : CommandEnvelope<'TCommand> -> 'TAggregate -> CommandResult<EventEnvelope<'TEvent> list>

/// Base helper for orchestrating validation and handling
[<AbstractClass>]
type CommandHandlerBase<'TCommand, 'TAggregate, 'TEvent>() =
    abstract member ValidateCommand : 'TCommand -> Result<unit, string list>
    abstract member ValidateBusinessRules : 'TCommand -> 'TAggregate -> Result<unit, string>
    abstract member Handle : CommandEnvelope<'TCommand> -> 'TAggregate -> CommandResult<EventEnvelope<'TEvent> list>

    member this.Execute
        (cmd: CommandEnvelope<'TCommand>)
        (aggregate: 'TAggregate)
        (isProcessed: Guid -> bool)
        (markProcessed: Guid -> unit)
        : CommandResult<EventEnvelope<'TEvent> list> =

        if isProcessed cmd.CommandId then
            ValidationError ["Command already processed"]
        else
            match this.ValidateCommand cmd.Data, this.ValidateBusinessRules cmd.Data aggregate with
            | Error errs, _ -> ValidationError errs
            | _, Error err -> BusinessRuleViolation err
            | Ok (), Ok () ->
                match this.Handle cmd aggregate with
                | Success evts as ok ->
                    markProcessed cmd.CommandId
                    ok
                | other -> other
