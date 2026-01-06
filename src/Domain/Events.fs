namespace EATool.Domain

open System

/// Optional metadata attached to events
[<CLIMutable>]
type EventMetadata =
    {
        Tags: string list option
        BusinessJustification: string option
    }

/// Generic event envelope capturing the immutable fact produced by a command
[<CLIMutable>]
type EventEnvelope<'TData> =
    {
        EventId: Guid
        EventType: string
        EventVersion: int
        EventTimestamp: DateTime
        AggregateId: Guid
        AggregateType: string
        AggregateVersion: int
        CausationId: Guid option   // command_id that produced this event
        CorrelationId: Guid option
        Actor: string
        ActorType: ActorType
        Source: Source
        Data: 'TData
        Metadata: EventMetadata option
    }
