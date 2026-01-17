/// Data entity API endpoints with command-based architecture
namespace EATool.Api

open System
open System.Diagnostics
open Giraffe
open Thoth.Json.Net
open EATool.Domain
open EATool.Infrastructure
open EATool.Infrastructure.EventStore
open EATool.Infrastructure.DataEntityEventJson
open EATool.Infrastructure.EventJson
open EATool.Infrastructure.ProjectionEngine

module DataEntitiesEndpoints =
    
    /// Helper to generate data entity ID
    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "dat-" + guid.Substring(0, 8)
    
    /// Create event store for DataEntityEvents
    let private createDataEntityEventStore () =
        let connectionString = Database.getConnectionString()
        createSqlEventStore(connectionString, encodeDataEntityEvent, decodeDataEntityEvent)
    
    /// Create projection engine for DataEntityEvents
    let private createProjectionEngine (eventStore: IEventStore<DataEntityEvent>) =
        let connectionString = Database.getConnectionString()
        let handlers: IProjectionHandler<DataEntityEvent> list = [
            Projections.DataEntityProjection.Handler(connectionString) :> IProjectionHandler<DataEntityEvent>
        ]
        ProjectionEngine<DataEntityEvent>(connectionString, eventStore, handlers)
    
    /// Extract aggregate Guid from dat-* identifier
    let private parseAggregateId (aggregateId: string) : Guid =
        let guidPart = if aggregateId.StartsWith("dat-") then aggregateId.Substring(4) else aggregateId
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

    /// Helper to create EventEnvelope from a DataEntityEvent
    let private createEventEnvelope (aggregateId: string) (aggregateGuid: Guid) (aggregateVersion: int) (event: DataEntityEvent) (actor: string, actorType: ActorType, correlationId: Guid, causationId: Guid) : EventEnvelope<DataEntityEvent> =
        {
            EventId = Guid.NewGuid()
            AggregateId = aggregateGuid
            AggregateType = "DataEntity"
            AggregateVersion = aggregateVersion
            EventType =
                match event with
                | DataEntityCreated _ -> "DataEntityCreated"
                | ClassificationSet _ -> "ClassificationSet"
                | PIIFlagSet _ -> "PIIFlagSet"
                | RetentionUpdated _ -> "RetentionUpdated"
                | DataEntityTagsAdded _ -> "DataEntityTagsAdded"
                | DataEntityDeleted _ -> "DataEntityDeleted"
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
    let private loadAggregateState (eventStore: IEventStore<DataEntityEvent>) (aggregateId: string) =
        let aggregateGuid = parseAggregateId aggregateId
        let existingEvents = eventStore.GetEvents aggregateGuid
        let baseVersion = eventStore.GetAggregateVersion aggregateGuid
        let stateFromEvents = existingEvents |> List.fold (fun (acc: DataEntityAggregate) e -> DataEntityAggregate.ApplyEvent acc e.Data) DataEntityAggregate.Empty

        let state =
            if existingEvents |> List.isEmpty then
                match DataEntityRepository.getById aggregateId with
                | Some entity ->
                    let classStr = match entity.Classification with | DataClassification.Public -> "public" | DataClassification.Internal -> "internal" | DataClassification.Confidential -> "confidential" | DataClassification.Restricted -> "restricted"
                    { DataEntityAggregate.Empty with
                        Id = Some entity.Id
                        Name = Some entity.Name
                        Domain = entity.Domain
                        Classification = Some classStr
                        Retention = entity.Retention
                        Owner = entity.Owner
                        Steward = entity.Steward
                        SourceSystem = entity.SourceSystem
                        Criticality = entity.Criticality
                        PiiFlag = entity.PiiFlag
                        Tags = entity.GlossaryTerms
                        IsDeleted = false }
                | None -> DataEntityAggregate.Empty
            else stateFromEvents

        state, baseVersion, aggregateGuid

    /// Append events and project them; returns envelopes or error
    let private persistAndProject (eventStore: IEventStore<DataEntityEvent>) (projectionEngine: ProjectionEngine<DataEntityEvent>) (aggregateId: string) (aggregateGuid: Guid) (baseVersion: int) (meta: string * ActorType * Guid * Guid) (events: DataEntityEvent list) =
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

    let private tryParseClassification (value: string option) : DataClassification option =
        value
        |> Option.bind (fun s ->
            match s.Trim().ToLowerInvariant() with
            | "public" -> Some DataClassification.Public
            | "internal" -> Some DataClassification.Internal
            | "confidential" -> Some DataClassification.Confidential
            | "restricted" -> Some DataClassification.Restricted
            | _ -> None)

    let routes: HttpHandler list =
        [
            // GET /data-entities - list (read from projection)
            GET >=> route "/data-entities" >=> fun next ctx -> task {
                let page = ctx.TryGetQueryStringValue "page" |> Option.bind (fun s -> try Some (int s) with _ -> None) |> Option.defaultValue 1
                let limit = ctx.TryGetQueryStringValue "limit" |> Option.bind (fun s -> try Some (int s) with _ -> None) |> Option.defaultValue 50
                let search = ctx.TryGetQueryStringValue "search" |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))
                let domain = ctx.TryGetQueryStringValue "domain" |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))
                let classification = ctx.TryGetQueryStringValue "classification" |> tryParseClassification

                let pageParam = if page < 1 then 1 else page
                let limitParam = if limit < 1 || limit > 200 then 50 else limit

                let result = DataEntityRepository.getAll pageParam limitParam search domain classification
                let json = Json.encodePaginatedResponse Json.encodeDataEntity result
                return! (Giraffe.Core.json json) next ctx
            }

            // POST /data-entities - create data entity via CreateDataEntity command
            POST >=> route "/data-entities" >=> fun next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateDataEntityRequest bodyStr with
                | Ok req ->
                    try
                        // Generate unique data entity ID
                        let dataEntityId = generateId()
                        
                        // Record command span event
                        let activity = Activity.Current
                        if activity <> null then
                            activity.SetTag("command.type", "CreateDataEntity") |> ignore
                            activity.SetTag("entity.id", dataEntityId) |> ignore
                            activity.SetTag("entity.type", "DataEntity") |> ignore
                        
                        // Create command
                        let cmd : CreateDataEntityData = {
                            Id = dataEntityId
                            Name = req.Name
                            Domain = req.Domain
                            Classification = match req.Classification with | DataClassification.Public -> "public" | DataClassification.Internal -> "internal" | DataClassification.Confidential -> "confidential" | DataClassification.Restricted -> "restricted"
                            Retention = req.Retention
                            Owner = req.Owner
                            Steward = req.Steward
                            SourceSystem = req.SourceSystem
                            Criticality = req.Criticality
                            PiiFlag = req.PiiFlag |> Option.defaultValue false
                            Tags = req.GlossaryTerms |> Option.defaultValue []
                        }
                        
                        // Validate command and generate events
                        let state = DataEntityAggregate.Empty
                        match DataEntityCommandHandler.handleCreateDataEntity state cmd with
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
                            
                            let eventStore = createDataEntityEventStore()
                            let projectionEngine = createProjectionEngine eventStore
                            let aggregateGuid = parseAggregateId dataEntityId
                            let baseVersion = eventStore.GetAggregateVersion aggregateGuid
                            let meta = getActorMetadata ctx

                            match persistAndProject eventStore projectionEngine dataEntityId aggregateGuid baseVersion meta events with
                            | Error err ->
                                if activity <> null then
                                    activity.SetTag("event.persist.error", err) |> ignore
                                let isConflict =
                                    (err.Contains("already exists") || err.Contains("UNIQUE") || err.Contains("unique"))
                                if isConflict then ctx.SetStatusCode 409 else ctx.SetStatusCode 500
                                let errType = if isConflict then "conflict" else "event_store_error"
                                let errJson = Json.encodeErrorResponse errType err
                                return! (Giraffe.Core.json errJson) next ctx
                            | Ok _ ->
                                if activity <> null then
                                    activity.SetTag("command.result", "success") |> ignore
                                match DataEntityRepository.getById dataEntityId with
                                | Some entity ->
                                    ctx.SetStatusCode 201
                                    ctx.SetHttpHeader ("Location", $"/data-entities/{entity.Id}")
                                    let json = Json.encodeDataEntity entity
                                    return! (Giraffe.Core.json json) next ctx
                                | None ->
                                    let finalState = events |> List.fold (fun (s: DataEntityAggregate) evt -> DataEntityAggregate.ApplyEvent s evt) state
                                    ctx.SetStatusCode 201
                                    ctx.SetHttpHeader ("Location", $"/data-entities/{dataEntityId}")
                                    let responseJson = Encode.object [
                                        "id", Encode.string dataEntityId
                                        "name", Encode.string (finalState.Name |> Option.defaultValue "")
                                        "classification", Encode.string (finalState.Classification |> Option.defaultValue "")
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

            // GET /data-entities/{id}
            GET >=> routef "/data-entities/%s" (fun id next ctx -> task {
                match DataEntityRepository.getById id with
                | Some entity ->
                    let json = Json.encodeDataEntity entity
                    return! (Giraffe.Core.json json) next ctx
                | None ->
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "Data entity not found"
                    return! (Giraffe.Core.json errJson) next ctx
            })

            // PATCH /data-entities/{id}
            PATCH >=> routef "/data-entities/%s" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateDataEntityRequest bodyStr with
                | Ok req ->
                    try
                        let activity = Activity.Current
                        if activity <> null then
                            activity.SetTag("command.type", "UpdateDataEntity") |> ignore
                            activity.SetTag("entity.id", id) |> ignore
                            activity.SetTag("entity.type", "DataEntity") |> ignore
                        
                        let eventStore = createDataEntityEventStore()
                        let projectionEngine = createProjectionEngine eventStore
                        let state, baseVersion, aggregateGuid = loadAggregateState eventStore id
                        
                        if state.Id.IsNone then
                            ctx.SetStatusCode 404
                            let errJson = Json.encodeErrorResponse "not_found" "Data entity not found"
                            return! (Giraffe.Core.json errJson) next ctx
                        else
                            // Build up all change commands
                            let classificationStr = match req.Classification with | DataClassification.Public -> "public" | DataClassification.Internal -> "internal" | DataClassification.Confidential -> "confidential" | DataClassification.Restricted -> "restricted"
                            let results = 
                                [
                                    if state.Classification <> Some classificationStr then
                                        let oldClass = state.Classification |> Option.defaultValue ""
                                        let cmd = { SetClassificationData.Id = id; OldClassification = oldClass; NewClassification = classificationStr }
                                        yield DataEntityCommandHandler.handleSetClassification state cmd
                                    
                                    if state.PiiFlag <> (req.PiiFlag |> Option.defaultValue false) then
                                        let cmd = { SetPIIFlagData.Id = id; OldPiiFlag = state.PiiFlag; NewPiiFlag = req.PiiFlag |> Option.defaultValue false }
                                        yield DataEntityCommandHandler.handleSetPIIFlag state cmd
                                    
                                    if state.Retention <> req.Retention then
                                        let cmd = { UpdateRetentionData.Id = id; OldRetention = state.Retention; NewRetention = req.Retention }
                                        yield DataEntityCommandHandler.handleUpdateRetention state cmd
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
                                    match DataEntityRepository.getById id with
                                    | Some entity ->
                                        let json = Json.encodeDataEntity entity
                                        return! (Giraffe.Core.json json) next ctx
                                    | None ->
                                        ctx.SetStatusCode 404
                                        let errJson = Json.encodeErrorResponse "not_found" "Data entity not found"
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
                                        match DataEntityRepository.getById id with
                                        | Some entity ->
                                            let json = Json.encodeDataEntity entity
                                            return! (Giraffe.Core.json json) next ctx
                                        | None ->
                                            ctx.SetStatusCode 404
                                            let errJson = Json.encodeErrorResponse "not_found" "Data entity not found"
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

            // DELETE /data-entities/{id}
            DELETE >=> routef "/data-entities/%s" (fun id next ctx -> task {
                try
                    let activity = Activity.Current
                    if activity <> null then
                        activity.SetTag("command.type", "DeleteDataEntity") |> ignore
                        activity.SetTag("entity.id", id) |> ignore
                        activity.SetTag("entity.type", "DataEntity") |> ignore
                    
                    let eventStore = createDataEntityEventStore()
                    let projectionEngine = createProjectionEngine eventStore
                    let state, baseVersion, aggregateGuid = loadAggregateState eventStore id
                    
                    if state.Id.IsNone then
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "Data entity not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    else
                        let cmd = { DeleteDataEntityData.Id = id }
                        match DataEntityCommandHandler.handleDeleteDataEntity state cmd with
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
