/// Relation API endpoints with command-based architecture
namespace EATool.Api

open System
open Giraffe
open Thoth.Json.Net
open EATool.Domain
open EATool.Infrastructure
open EATool.Infrastructure.EventStore
open EATool.Infrastructure.RelationEventJson
open EATool.Infrastructure.EventJson
open EATool.Infrastructure.ProjectionEngine

module RelationsEndpoints =
    
    /// Helper to generate relation ID
    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "rel-" + guid.Substring(0, 8)
    
    /// Create event store for RelationEvents
    let private createRelationEventStore () =
        let connectionString = Database.getConnectionString()
        createSqlEventStore(connectionString, encodeRelationEvent, decodeRelationEvent)
    
    /// Create projection engine for RelationEvents
    let private createProjectionEngine (eventStore: IEventStore<RelationEvent>) =
        let connectionString = Database.getConnectionString()
        let handlers: IProjectionHandler<RelationEvent> list = [
            Projections.RelationProjection.Handler(connectionString) :> IProjectionHandler<RelationEvent>
        ]
        ProjectionEngine<RelationEvent>(connectionString, eventStore, handlers)
    
    /// Extract aggregate Guid from rel-* identifier
    let private parseAggregateId (aggregateId: string) : Guid =
        let guidPart = if aggregateId.StartsWith("rel-") then aggregateId.Substring(4) else aggregateId
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
            |> Option.map Guid.Parse
            |> Option.defaultValue (Guid.NewGuid())

        let causationId = Guid.NewGuid()
        
        (actor, actorType, correlationId, causationId)

    /// Create EventEnvelope from event metadata
    let private createEventEnvelope (aggregateId: string) (aggregateGuid: Guid) (version: int) (event: RelationEvent) (meta: string * ActorType * Guid * Guid) =
        let (actor, actorType, correlationId, causationId) = meta
        let eventType =
            match event with
            | RelationCreated _ -> "RelationCreated"
            | ConfidenceUpdated _ -> "ConfidenceUpdated"
            | EffectiveDatesSet _ -> "EffectiveDatesSet"
            | RelationDescriptionUpdated _ -> "RelationDescriptionUpdated"
            | RelationDeleted _ -> "RelationDeleted"
        {
            EventId = Guid.NewGuid()
            AggregateId = aggregateGuid
            AggregateType = "Relation"
            AggregateVersion = version
            EventType = eventType
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

    /// Load aggregate state and current version from event store (fallback to projection state)
    let private loadAggregateState (eventStore: IEventStore<RelationEvent>) (aggregateId: string) =
        let aggregateGuid = parseAggregateId aggregateId
        let existingEvents = eventStore.GetEvents aggregateGuid
        let baseVersion = eventStore.GetAggregateVersion aggregateGuid
        let stateFromEvents = existingEvents |> List.fold (fun acc e -> RelationAggregate.apply acc e.Data) RelationAggregate.Initial

        let state =
            if existingEvents |> List.isEmpty then
                // Fallback: load from projection
                match RelationRepository.getById aggregateId with
                | Some rel ->
                    let relState: RelationState = {
                        Id = rel.Id
                        SourceId = rel.SourceId
                        TargetId = rel.TargetId
                        SourceType = rel.SourceType
                        TargetType = rel.TargetType
                        RelationType = rel.RelationType
                        Description = rel.Description
                        DataClassification = rel.DataClassification
                        Confidence = rel.Confidence
                        EvidenceSource = rel.EvidenceSource
                        LastVerifiedAt = rel.LastVerifiedAt
                        EffectiveFrom = rel.EffectiveFrom
                        EffectiveTo = rel.EffectiveTo
                    }
                    RelationAggregate.Active relState
                | None -> RelationAggregate.Initial
            else stateFromEvents

        state, baseVersion, aggregateGuid

    /// Append events and project them; returns envelopes or error
    let private persistAndProject (eventStore: IEventStore<RelationEvent>) (projectionEngine: ProjectionEngine<RelationEvent>) (aggregateId: string) (aggregateGuid: Guid) (baseVersion: int) (meta: string * ActorType * Guid * Guid) (events: RelationEvent list) =
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
    
    let private tryParseRelationType (value: string option) : RelationType option =
        value
        |> Option.bind (fun s ->
            match s.Trim().ToLowerInvariant() with
            | "depends_on" -> Some RelationType.DependsOn
            | "communicates_with" -> Some RelationType.CommunicatesWith
            | "calls" -> Some RelationType.Calls
            | "publishes_event_to" -> Some RelationType.PublishesEventTo
            | "consumes_event_from" -> Some RelationType.ConsumesEventFrom
            | "deployed_on" -> Some RelationType.DeployedOn
            | "stores_data_on" -> Some RelationType.StoresDataOn
            | "reads" -> Some RelationType.Reads
            | "writes" -> Some RelationType.Writes
            | "owns" -> Some RelationType.Owns
            | "supports" -> Some RelationType.Supports
            | "implements" -> Some RelationType.Implements
            | "realizes" -> Some RelationType.Realizes
            | "serves" -> Some RelationType.Serves
            | "connected_to" -> Some RelationType.ConnectedTo
            | "exposes" -> Some RelationType.Exposes
            | "uses" -> Some RelationType.Uses
            | _ -> None)
    
    let routes: HttpHandler list =
        [
            // GET /relations - List with pagination and filters
            GET >=> route "/relations" >=> fun next ctx -> task {
                let page = ctx.TryGetQueryStringValue "page" |> Option.bind (fun s -> try Some (int s) with _ -> None) |> Option.defaultValue 1
                let limit = ctx.TryGetQueryStringValue "limit" |> Option.bind (fun s -> try Some (int s) with _ -> None) |> Option.defaultValue 50
                let sourceId = ctx.TryGetQueryStringValue "source_id" |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))
                let targetId = ctx.TryGetQueryStringValue "target_id" |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))
                let relationType = ctx.TryGetQueryStringValue "relation_type" |> tryParseRelationType

                let pageParam = if page < 1 then 1 else page
                let limitParam = if limit < 1 || limit > 200 then 50 else limit

                let result = RelationRepository.getAll pageParam limitParam sourceId targetId relationType
                let json = Json.encodePaginatedResponse Json.encodeRelation result
                return! (Giraffe.Core.json json) next ctx
            }

            // POST /relations - Create with relation matrix validation
            POST >=> route "/relations" >=> fun next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateRelationRequest bodyStr with
                | Ok req ->
                    let relId = generateId()
                    let cmd : CreateRelationData = {
                        Id = relId
                        SourceId = req.SourceId
                        TargetId = req.TargetId
                        SourceType = req.SourceType
                        TargetType = req.TargetType
                        RelationType = req.RelationType
                        Description = req.Description
                        DataClassification = req.DataClassification
                        Confidence = req.Confidence
                        EffectiveFrom = req.EffectiveFrom
                        EffectiveTo = req.EffectiveTo
                    }
                    
                    // Validate command and generate events
                    let state = RelationAggregate.Initial
                    match RelationCommandHandler.handleCreateRelation state cmd with
                    | Error err ->
                        ctx.SetStatusCode 422
                        let errJson = Json.encodeErrorResponse "validation_error" err
                        return! (Giraffe.Core.json errJson) next ctx
                    | Ok events ->
                        let eventStore = createRelationEventStore()
                        let projectionEngine = createProjectionEngine eventStore
                        let aggregateGuid = parseAggregateId relId
                        let baseVersion = eventStore.GetAggregateVersion aggregateGuid
                        let meta = getActorMetadata ctx

                        match persistAndProject eventStore projectionEngine relId aggregateGuid baseVersion meta events with
                        | Error err ->
                            ctx.SetStatusCode 500
                            let errJson = Json.encodeErrorResponse "event_store_error" err
                            return! (Giraffe.Core.json errJson) next ctx
                        | Ok _ ->
                            match RelationRepository.getById relId with
                            | Some rel ->
                                ctx.SetStatusCode 201
                                ctx.SetHttpHeader ("Location", $"/relations/{rel.Id}")
                                let responseJson = Json.encodeRelation rel
                                return! (Giraffe.Core.json responseJson) next ctx
                            | None ->
                                ctx.SetStatusCode 201
                                return! next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errorJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errorJson) next ctx
            }
            
            // GET /relations/{id} - Get by ID
            GET >=> routef "/relations/%s" (fun id next ctx -> task {
                match RelationRepository.getById id with
                | Some rel ->
                    let json = Json.encodeRelation rel
                    return! (Giraffe.Core.json json) next ctx
                | None ->
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "Relation not found"
                    return! (Giraffe.Core.json errJson) next ctx
            })
            
            // POST /relations/{id}/commands/update-confidence
            POST >=> routef "/relations/%s/commands/update-confidence" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                
                let decoder = Decode.object (fun get -> {|
                    Confidence = get.Required.Field "confidence" Decode.float
                    EvidenceSource = get.Optional.Field "evidence_source" Decode.string
                    LastVerifiedAt = get.Optional.Field "last_verified_at" Decode.string
                |})
                
                match Decode.fromString decoder bodyStr with
                | Ok req ->
                    let cmd : UpdateConfidenceData = {
                        Id = id
                        Confidence = req.Confidence
                        EvidenceSource = req.EvidenceSource
                        LastVerifiedAt = req.LastVerifiedAt
                    }

                    let eventStore = createRelationEventStore()
                    let projectionEngine = createProjectionEngine eventStore
                    let state, baseVersion, aggregateGuid = loadAggregateState eventStore id

                    match state with
                    | RelationAggregate.Initial | RelationAggregate.Deleted ->
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "Relation not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    | RelationAggregate.Active _ ->
                        match RelationCommandHandler.handleUpdateConfidence state cmd with
                        | Error err ->
                            ctx.SetStatusCode 400
                            let errJson = Json.encodeErrorResponse "validation_error" err
                            return! (Giraffe.Core.json errJson) next ctx
                        | Ok events ->
                            let meta = getActorMetadata ctx
                            match persistAndProject eventStore projectionEngine id aggregateGuid baseVersion meta events with
                            | Error err ->
                                ctx.SetStatusCode 500
                                let errJson = Json.encodeErrorResponse "event_store_error" err
                                return! (Giraffe.Core.json errJson) next ctx
                            | Ok _ ->
                                match RelationRepository.getById id with
                                | Some rel ->
                                    let responseJson = Json.encodeRelation rel
                                    return! (Giraffe.Core.json responseJson) next ctx
                                | None ->
                                    ctx.SetStatusCode 404
                                    let errJson = Json.encodeErrorResponse "not_found" "Relation not found"
                                    return! (Giraffe.Core.json errJson) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errorJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errorJson) next ctx
            })
            
            // POST /relations/{id}/commands/set-effective-dates
            POST >=> routef "/relations/%s/commands/set-effective-dates" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                
                let decoder = Decode.object (fun get -> {|
                    EffectiveFrom = get.Optional.Field "effective_from" Decode.string
                    EffectiveTo = get.Optional.Field "effective_to" Decode.string
                |})
                
                match Decode.fromString decoder bodyStr with
                | Ok req ->
                    let cmd : SetEffectiveDatesData = {
                        Id = id
                        EffectiveFrom = req.EffectiveFrom
                        EffectiveTo = req.EffectiveTo
                    }

                    let eventStore = createRelationEventStore()
                    let projectionEngine = createProjectionEngine eventStore
                    let state, baseVersion, aggregateGuid = loadAggregateState eventStore id

                    match state with
                    | RelationAggregate.Initial | RelationAggregate.Deleted ->
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "Relation not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    | RelationAggregate.Active _ ->
                        match RelationCommandHandler.handleSetEffectiveDates state cmd with
                        | Error err ->
                            ctx.SetStatusCode 400
                            let errJson = Json.encodeErrorResponse "validation_error" err
                            return! (Giraffe.Core.json errJson) next ctx
                        | Ok events ->
                            let meta = getActorMetadata ctx
                            match persistAndProject eventStore projectionEngine id aggregateGuid baseVersion meta events with
                            | Error err ->
                                ctx.SetStatusCode 500
                                let errJson = Json.encodeErrorResponse "event_store_error" err
                                return! (Giraffe.Core.json errJson) next ctx
                            | Ok _ ->
                                match RelationRepository.getById id with
                                | Some rel ->
                                    let responseJson = Json.encodeRelation rel
                                    return! (Giraffe.Core.json responseJson) next ctx
                                | None ->
                                    ctx.SetStatusCode 404
                                    let errJson = Json.encodeErrorResponse "not_found" "Relation not found"
                                    return! (Giraffe.Core.json errJson) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errorJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errorJson) next ctx
            })
            
            // POST /relations/{id}/commands/update-description
            POST >=> routef "/relations/%s/commands/update-description" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                
                let decoder = Decode.object (fun get -> {|
                    Description = get.Optional.Field "description" Decode.string
                |})
                
                match Decode.fromString decoder bodyStr with
                | Ok req ->
                    let cmd : UpdateRelationDescriptionData = {
                        Id = id
                        Description = req.Description
                    }

                    let eventStore = createRelationEventStore()
                    let projectionEngine = createProjectionEngine eventStore
                    let state, baseVersion, aggregateGuid = loadAggregateState eventStore id

                    match state with
                    | RelationAggregate.Initial | RelationAggregate.Deleted ->
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "Relation not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    | RelationAggregate.Active _ ->
                        match RelationCommandHandler.handleUpdateDescription state cmd with
                        | Error err ->
                            ctx.SetStatusCode 400
                            let errJson = Json.encodeErrorResponse "validation_error" err
                            return! (Giraffe.Core.json errJson) next ctx
                        | Ok events ->
                            let meta = getActorMetadata ctx
                            match persistAndProject eventStore projectionEngine id aggregateGuid baseVersion meta events with
                            | Error err ->
                                ctx.SetStatusCode 500
                                let errJson = Json.encodeErrorResponse "event_store_error" err
                                return! (Giraffe.Core.json errJson) next ctx
                            | Ok _ ->
                                match RelationRepository.getById id with
                                | Some rel ->
                                    let responseJson = Json.encodeRelation rel
                                    return! (Giraffe.Core.json responseJson) next ctx
                                | None ->
                                    ctx.SetStatusCode 404
                                    let errJson = Json.encodeErrorResponse "not_found" "Relation not found"
                                    return! (Giraffe.Core.json errJson) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errorJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errorJson) next ctx
            })
            
            // POST /relations/{id}/commands/delete
            POST >=> routef "/relations/%s/commands/delete" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                
                let decoder = Decode.object (fun get -> {|
                    Reason = get.Optional.Field "reason" Decode.string
                |})
                
                match Decode.fromString decoder bodyStr with
                | Ok req ->
                    let cmd : DeleteRelationData = {
                        Id = id
                        Reason = req.Reason
                    }

                    let eventStore = createRelationEventStore()
                    let projectionEngine = createProjectionEngine eventStore
                    let state, baseVersion, aggregateGuid = loadAggregateState eventStore id

                    match state with
                    | RelationAggregate.Initial | RelationAggregate.Deleted ->
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "Relation not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    | RelationAggregate.Active _ ->
                        match RelationCommandHandler.handleDeleteRelation state cmd with
                        | Error err ->
                            ctx.SetStatusCode 400
                            let errJson = Json.encodeErrorResponse "validation_error" err
                            return! (Giraffe.Core.json errJson) next ctx
                        | Ok events ->
                            let meta = getActorMetadata ctx
                            match persistAndProject eventStore projectionEngine id aggregateGuid baseVersion meta events with
                            | Error err ->
                                ctx.SetStatusCode 500
                                let errJson = Json.encodeErrorResponse "event_store_error" err
                                return! (Giraffe.Core.json errJson) next ctx
                            | Ok _ ->
                                ctx.SetStatusCode 204
                                return! next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errorJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errorJson) next ctx
            })
        ]

