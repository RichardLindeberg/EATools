namespace EATool.Domain

open System

/// Command metadata carrying optional context
[<CLIMutable>]
type CommandMetadata =
    {
        IpAddress: string option
        BusinessJustification: string option
        ApprovalId: string option
    }

/// Generic command envelope capturing who/what/when and the command payload
[<CLIMutable>]
type CommandEnvelope<'TData> =
    {
        CommandId: Guid
        CommandType: string
        AggregateId: Guid
        AggregateType: string
        CorrelationId: Guid option
        Actor: string
        ActorType: ActorType
        Source: Source
        CommandTimestamp: DateTime
        Data: 'TData
        Metadata: CommandMetadata option
    }

/// Result of processing a command
type CommandResult<'T> =
    | Success of 'T
    | ValidationError of string list
    | BusinessRuleViolation of string
    | ConcurrencyConflict of string

module CommandResult =
    let map f = function
        | Success x -> Success (f x)
        | ValidationError e -> ValidationError e
        | BusinessRuleViolation e -> BusinessRuleViolation e
        | ConcurrencyConflict e -> ConcurrencyConflict e
        
