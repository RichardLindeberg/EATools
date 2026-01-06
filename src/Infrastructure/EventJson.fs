namespace EATool.Infrastructure

open Thoth.Json.Net
open EATool.Infrastructure.EventStore

module EventJson =
    /// Serialize an event payload to JSON string using Thoth encoder
    let serialize (encoder: 'T -> JsonValue) (x: 'T) : string =
        Encode.toString 0 (encoder x)

    /// Deserialize an event payload from JSON string using Thoth decoder
    let deserialize (decoder: Decoder<'T>) (s: string) : 'T =
        match Decode.fromString decoder s with
        | Ok v -> v
        | Error err -> failwithf "Event deserialization failed: %s" err

    /// Helper to construct a SqlEventStore using Thoth encoder/decoder
    let createSqlEventStore<'T>(connectionString: string, encoder: 'T -> JsonValue, decoder: Decoder<'T>) : EventStore.IEventStore<'T> =
        SqlEventStore<'T>(connectionString, serialize encoder, deserialize decoder) :> EventStore.IEventStore<'T>
