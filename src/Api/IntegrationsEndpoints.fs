/// Integration API endpoints with command-based architecture
namespace EATool.Api

open System
open System.Diagnostics
open Giraffe
open Thoth.Json.Net
open EATool.Domain
open EATool.Infrastructure
open EATool.Infrastructure.EventStore
open EATool.Infrastructure.IntegrationEventJson
open EATool.Infrastructure.EventJson
open EATool.Infrastructure.ProjectionEngine

module IntegrationsEndpoints =
    
    /// Helper to generate integration ID
    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "int-" + guid.Substring(0, 8)
    
    /// Create event store for IntegrationEvents
    let private createIntegrationEventStore () =
        let connectionString = Database.getConnectionString()
        createSqlEventStore(connectionString, encodeIntegrationEvent, decodeIntegrationEvent)
    
    /// Create projection engine for IntegrationEvents
    let private createProjectionEngine (eventStore: IEventStore<IntegrationEvent>) =
        let connectionString = Database.getConnectionString()
        let handlers: IProjectionHandler<IntegrationEvent> list = [
            Projections.IntegrationProjection.Handler(connectionString) :> IProjectionHandler<IntegrationEvent>
        ]
        ProjectionEngine<IntegrationEvent>(connectionString, eventStore, handlers)
    
    /// Extract aggregate Guid from int-* identifier
    let private parseAggregateId (aggregateId: string) : Guid =
        let guidPart = if aggregateId.StartsWith("int-") then aggregateId.Substring(4) else aggregateId
        Guid.Parse(guidPart.PadRight(32, '0'))

    /// Resolve actor/correlation metadata from request context
    let private getActorMetadata (ctx: Microsoft.AspNetCore.Http.HttpContext) =
        let actor =
            ctx.TryGetRequestHeader "X-Actor"
            |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))
            |> Option.defaultValue "system"

        let actorType =
            ctx.TryGetRequestHeader "X-Actor-Type"
            |> Option.map (fun s -> s.ToLowerInvariant())
            |> Option.map (function | "service" -> ActorType.Service | "user" -> ActorType.User | _ -> ActorType.System)
            |> Option.defaultValue ActorType.System

        let correlationId =
            ctx.TryGetRequestHeader "X-Correlation-Id"
            |> Option.bind (fun s -> match Guid.TryParse s with | true, g -> Some g | _ -> None)
            |> Option.defaultValue (Guid.NewGuid())

        let causationId = Guid.NewGuid()
        (actor, actorType, correlationId, causationId)

    /// Helper to create EventEnvelope from an IntegrationEvent
    let private createEventEnvelope (aggregateId: string) (aggregateGuid: Guid) (aggregateVersion: int) (event: IntegrationEvent) (actor: string, actorType: ActorType, correlationId: Guid, causationId: Guid) : EventEnvelope<IntegrationEvent> =
        {
            EventId = Guid.NewGuid()
            AggregateId = aggregateGuid
            AggregateType = "Integration"
            AggregateVersion = aggregateVersion
            EventType =
                match event with
                | IntegrationCreated _ -> "IntegrationCreated"
                | ProtocolUpdated _ -> "ProtocolUpdated"
                | SLASet _ -> "SLASet"
                | FrequencySet _ -> "FrequencySet"
                | DataContractUpdated _ -> "DataContractUpdated"
                | SourceAppSet _ -> "SourceAppSet"
                | TargetAppSet _ -> "TargetAppSet"
                | IntegrationTagsAdded _ -> "IntegrationTagsAdded"
                | IntegrationTagsRemoved _ -> "IntegrationTagsRemoved"
                | IntegrationDeleted _ -> "IntegrationDeleted"
            EventVersion = 1
            EventTimestamp = DateTime.UtcNow
            Data = event
            Actor = actor
            ActorType = actorType
            Source = Source.API
            CausationId = Some causationId
            CorrelationId = Some correlationId
            Metadata = None
        }

    /// Load aggregate state and current version from event store
    let private loadAggregateState (eventStore: IEventStore<IntegrationEvent>) (aggregateId: string) =
        let aggregateGuid = parseAggregateId aggregateId
        let existingEvents = eventStore.GetEvents aggregateGuid
        let baseVersion = eventStore.GetAggregateVersion aggregateGuid
        let stateFromEvents = existingEvents |> List.fold (fun (acc: IntegrationAggregate) e -> acc.ApplyEvent(e.Data)) IntegrationAggregate.Empty

        let state =
            if existingEvents |> List.isEmpty then
                match IntegrationRepository.getById aggregateId with
                | Some integration ->
                    { IntegrationAggregate.Empty with
                        Id = Some integration.Id
                        SourceAppId = Some integration.SourceAppId
                        TargetAppId = Some integration.TargetAppId
                        Protocol = Some integration.Protocol
                        DataContract = integration.DataContract
                        Sla = integration.Sla
                        Frequency = integration.Frequency
                        Tags = integration.Tags
                        IsDeleted = false }
                | None -> IntegrationAggregate.Empty
            else stateFromEvents

        state, baseVersion, aggregateGuid

    /// Append events and project them; returns envelopes or error
    let private persistAndProject (eventStore: IEventStore<IntegrationEvent>) (projectionEngine: ProjectionEngine<IntegrationEvent>) (aggregateId: string) (aggregateGuid: Guid) (baseVersion: int) (meta: string * ActorType * Guid * Guid) (events: IntegrationEvent list) =
        let envelopes =
            events
            |> List.mapi (fun i event -> createEventEnvelope aggregateId aggregateGuid (baseVersion + i + 1) event meta)

        match eventStore.Append envelopes with
        | Error err -> Error err
        | Ok () ->
            let projectionResult =
                envelopes
                |> List.fold (fun acc env ->
                    match acc with
                    | Error _ -> acc
                    | Ok () -> projectionEngine.ProcessEvent(env)) (Ok ())

            match projectionResult with
            | Ok () -> Ok envelopes
            | Error e -> Error e

    let routes: HttpHandler list =
        [
            // GET /integrations - list (read from projection)
            GET >=> route "/integrations" >=> fun next ctx -> task {
                let page = ctx.TryGetQueryStringValue "page" |> Option.bind (fun s -> try Some (int s) with _ -> None) |> Option.defaultValue 1
                let limit = ctx.TryGetQueryStringValue "limit" |> Option.bind (fun s -> try Some (int s) with _ -> None) |> Option.defaultValue 50
                let source = ctx.TryGetQueryStringValue "source_app_id" |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))
                let target = ctx.TryGetQueryStringValue "target_app_id" |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))

                let pageParam = if page < 1 then 1 else page
                let limitParam = if limit < 1 || limit > 200 then 50 else limit

                let result = IntegrationRepository.getAll pageParam limitParam source target
                let json = Json.encodePaginatedResponse Json.encodeIntegration result
                return! (Giraffe.Core.json json) next ctx
            }

            // POST /integrations - create integration via CreateIntegration command
            POST >=> route "/integrations" >=> fun next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateIntegrationRequest bodyStr with
                | Ok req ->
                    try
                        // Generate unique integration ID
                        let integrationId = generateId()
                        
                        // Record command span event
                        let activity = Activity.Current
                        if activity <> null then
                            activity.SetTag("command.type", "CreateIntegration") |> ignore
                            activity.SetTag("entity.id", integrationId) |> ignore
                            activity.SetTag("entity.type", "Integration") |> ignore
                        
                        // Create command
                        let cmd : CreateIntegrationData = {
                            Id = integrationId
                            SourceAppId = req.SourceAppId
                            TargetAppId = req.TargetAppId
                            Protocol = req.Protocol
                            DataContract = req.DataContract
                            Sla = req.Sla
                            Frequency = req.Frequency
                            Tags = req.Tags |> Option.defaultValue []
                        }
                        
                        // Validate command and generate events
                        let state = IntegrationAggregate.Empty
                        match IntegrationCommandHandler.handleCreateIntegration state cmd with
                        | Error err ->
                            if activity <> null then
                                activity.SetTag("command.result", "validation_failed") |> ignore
                            ctx.SetStatusCode 400
                            let errJson = Json.encodeErrorResponse "validation_error" err
                            return! (Giraffe.Core.json errJson) next ctx
                        | Ok events ->
                            // Record event persistence span
                            if activity <> null then
                                activity.SetTag("event.count", events.Length) |> ignore
                            
                            let eventStore = createIntegrationEventStore()
                            let projectionEngine = createProjectionEngine eventStore
                            let aggregateGuid = parseAggregateId integrationId
                            let baseVersion = eventStore.GetAggregateVersion aggregateGuid
                            let meta = getActorMetadata ctx

                            match persistAndProject eventStore projectionEngine integrationId aggregateGuid baseVersion meta events with
                            | Error err ->
                                if activity <> null then
                                    activity.SetTag("event.persist.error", err) |> ignore
                                // Map known repository constraint errors to Conflict
                                let isConflict =
                                    (err.Contains("already exists") || err.Contains("UNIQUE") || err.Contains("unique"))
                                if isConflict then ctx.SetStatusCode 409 else ctx.SetStatusCode 500
                                let errType = if isConflict then "conflict" else "event_store_error"
                                let errJson = Json.encodeErrorResponse errType err
                                return! (Giraffe.Core.json errJson) next ctx
                            | Ok _ ->
                                if activity <> null then
                                    activity.SetTag("command.result", "success") |> ignore
                                match IntegrationRepository.getById integrationId with
                                | Some integration ->
                                    ctx.SetStatusCode 201
                                    ctx.SetHttpHeader ("Location", $"/integrations/{integration.Id}")
                                    let json = Json.encodeIntegration integration
                                    return! (Giraffe.Core.json json) next ctx
                                | None ->
                                    let finalState = events |> List.fold (fun (s: IntegrationAggregate) evt -> s.ApplyEvent(evt)) state
                                    ctx.SetStatusCode 201
                                    ctx.SetHttpHeader ("Location", $"/integrations/{integrationId}")
                                    let responseJson = Encode.object [
                                        "id", Encode.string integrationId
                                        "source_app_id", Encode.string (finalState.SourceAppId |> Option.defaultValue "")
                                        "target_app_id", Encode.string (finalState.TargetAppId |> Option.defaultValue "")
                                        "protocol", Encode.string (finalState.Protocol |> Option.defaultValue "")
                                    ]
                                    return! (Giraffe.Core.json responseJson) next ctx
                    with ex ->
                        ctx.SetStatusCode 500
                        let errJson = Json.encodeErrorResponse "internal_error" ex.Message
                        return! (Giraffe.Core.json errJson) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errJson) next ctx
            }

            // GET /integrations/{id}
            GET >=> routef "/integrations/%s" (fun id next ctx -> task {
                match IntegrationRepository.getById id with
                | Some integration ->
                    let json = Json.encodeIntegration integration
                    return! (Giraffe.Core.json json) next ctx
                | None ->
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "Integration not found"
                    return! (Giraffe.Core.json errJson) next ctx
            })

            // PATCH /integrations/{id}
            PATCH >=> routef "/integrations/%s" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateIntegrationRequest bodyStr with
                | Ok req ->
                    try
                        let activity = Activity.Current
                        if activity <> null then
                            activity.SetTag("command.type", "UpdateIntegration") |> ignore
                            activity.SetTag("entity.id", id) |> ignore
                            activity.SetTag("entity.type", "Integration") |> ignore
                        
                        let eventStore = createIntegrationEventStore()
                        let projectionEngine = createProjectionEngine eventStore
                        let state, baseVersion, aggregateGuid = loadAggregateState eventStore id
                        
                        if state.Id.IsNone then
                            ctx.SetStatusCode 404
                            let errJson = Json.encodeErrorResponse "not_found" "Integration not found"
                            return! (Giraffe.Core.json errJson) next ctx
                        else
                            // Build up all change commands
                            let results = 
                                [
                                    if state.Protocol <> Some req.Protocol then
                                        let cmd = { UpdateProtocolData.Id = id; NewProtocol = req.Protocol }
                                        yield IntegrationCommandHandler.handleUpdateProtocol state cmd
                                    
                                    if state.Sla <> req.Sla then
                                        let cmd = { SetSLAData.Id = id; Sla = req.Sla }
                                        yield IntegrationCommandHandler.handleSetSLA state cmd
                                    
                                    if state.Frequency <> req.Frequency then
                                        let cmd = { SetFrequencyData.Id = id; Frequency = req.Frequency }
                                        yield IntegrationCommandHandler.handleSetFrequency state cmd
                                    
                                    if state.DataContract <> req.DataContract then
                                        let cmd = { UpdateDataContractData.Id = id; DataContract = req.DataContract }
                                        yield IntegrationCommandHandler.handleUpdateDataContract state cmd
                                    
                                    if state.SourceAppId <> Some req.SourceAppId then
                                        let cmd = { SetSourceAppData.Id = id; NewSourceAppId = req.SourceAppId }
                                        yield IntegrationCommandHandler.handleSetSourceApp state cmd
                                    
                                    if state.TargetAppId <> Some req.TargetAppId then
                                        let cmd = { SetTargetAppData.Id = id; NewTargetAppId = req.TargetAppId }
                                        yield IntegrationCommandHandler.handleSetTargetApp state cmd
                                ]
                            
                            // Check if any validation errors
                            let errors = results |> List.choose (function | Error e -> Some e | _ -> None)
                            if not errors.IsEmpty then
                                ctx.SetStatusCode 400
                                let errJson = Json.encodeErrorResponse "validation_error" (String.concat "; " errors)
                                return! (Giraffe.Core.json errJson) next ctx
                            else
                                // Collect all events
                                let allEvents = results |> List.collect (function | Ok events -> events | _ -> [])
                                
                                if allEvents.IsEmpty then
                                    // No changes
                                    match IntegrationRepository.getById id with
                                    | Some integration ->
                                        let json = Json.encodeIntegration integration
                                        return! (Giraffe.Core.json json) next ctx
                                    | None ->
                                        ctx.SetStatusCode 404
                                        let errJson = Json.encodeErrorResponse "not_found" "Integration not found"
                                        return! (Giraffe.Core.json errJson) next ctx
                                else
                                    let meta = getActorMetadata ctx
                                    match persistAndProject eventStore projectionEngine id aggregateGuid baseVersion meta allEvents with
                                    | Error err ->
                                        if activity <> null then
                                            activity.SetTag("event.persist.error", err) |> ignore
                                        ctx.SetStatusCode 500
                                        let errJson = Json.encodeErrorResponse "event_store_error" err
                                        return! (Giraffe.Core.json errJson) next ctx
                                    | Ok _ ->
                                        if activity <> null then
                                            activity.SetTag("command.result", "success") |> ignore
                                        match IntegrationRepository.getById id with
                                        | Some integration ->
                                            let json = Json.encodeIntegration integration
                                            return! (Giraffe.Core.json json) next ctx
                                        | None ->
                                            ctx.SetStatusCode 404
                                            let errJson = Json.encodeErrorResponse "not_found" "Integration not found"
                                            return! (Giraffe.Core.json errJson) next ctx
                    with ex ->
                        ctx.SetStatusCode 500
                        let errJson = Json.encodeErrorResponse "internal_error" ex.Message
                        return! (Giraffe.Core.json errJson) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errJson) next ctx
            })

            // DELETE /integrations/{id}
            DELETE >=> routef "/integrations/%s" (fun id next ctx -> task {
                try
                    let activity = Activity.Current
                    if activity <> null then
                        activity.SetTag("command.type", "DeleteIntegration") |> ignore
                        activity.SetTag("entity.id", id) |> ignore
                        activity.SetTag("entity.type", "Integration") |> ignore
                    
                    let eventStore = createIntegrationEventStore()
                    let projectionEngine = createProjectionEngine eventStore
                    let state, baseVersion, aggregateGuid = loadAggregateState eventStore id
                    
                    if state.Id.IsNone then
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "Integration not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    else
                        let cmd = { DeleteIntegrationData.Id = id }
                        match IntegrationCommandHandler.handleDeleteIntegration state cmd with
                        | Error err ->
                            if activity <> null then
                                activity.SetTag("command.result", "validation_failed") |> ignore
                            ctx.SetStatusCode 400
                            let errJson = Json.encodeErrorResponse "validation_error" err
                            return! (Giraffe.Core.json errJson) next ctx
                        | Ok events ->
                            let meta = getActorMetadata ctx
                            match persistAndProject eventStore projectionEngine id aggregateGuid baseVersion meta events with
                            | Error err ->
                                if activity <> null then
                                    activity.SetTag("event.persist.error", err) |> ignore
                                ctx.SetStatusCode 500
                                let errJson = Json.encodeErrorResponse "event_store_error" err
                                return! (Giraffe.Core.json errJson) next ctx
                            | Ok _ ->
                                if activity <> null then
                                    activity.SetTag("command.result", "success") |> ignore
                                ctx.SetStatusCode 204
                                return! next ctx
                with ex ->
                    ctx.SetStatusCode 500
                    let errJson = Json.encodeErrorResponse "internal_error" ex.Message
                    return! (Giraffe.Core.json errJson) next ctx
            })
        ]
