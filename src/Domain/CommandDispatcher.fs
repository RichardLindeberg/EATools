namespace EATool.Domain

open System

/// Dependencies required to dispatch a command
type DispatchDependencies<'TAggregate, 'TEvent> =
    {
        LoadAggregate : Guid -> Result<'TAggregate, string>
        IsProcessed : Guid -> bool
        MarkProcessed : Guid -> unit
        SaveEvents : EventEnvelope<'TEvent> list -> Result<unit, string>
    }

module CommandDispatcher =
    let dispatch
        (deps: DispatchDependencies<'TAggregate, 'TEvent>)
        (handler: ICommandHandler<'TCommand, 'TAggregate, 'TEvent>)
        (cmd: CommandEnvelope<'TCommand>)
        : CommandResult<EventEnvelope<'TEvent> list> =

        if deps.IsProcessed cmd.CommandId then
            ValidationError ["Duplicate command"]
        else
            match deps.LoadAggregate cmd.AggregateId with
            | Error err -> BusinessRuleViolation err
            | Ok agg ->
                match handler.ValidateCommand cmd.Data, handler.ValidateBusinessRules cmd.Data agg with
                | Error errs, _ -> ValidationError errs
                | _, Error err -> BusinessRuleViolation err
                | Ok (), Ok () ->
                    match handler.Handle cmd agg with
                    | Success evts as ok ->
                        match deps.SaveEvents evts with
                        | Ok () -> deps.MarkProcessed cmd.CommandId; ok
                        | Error e -> ConcurrencyConflict e
                    | other -> other
