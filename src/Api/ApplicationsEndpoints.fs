/// Application API endpoints with command-based architecture
namespace EATool.Api

open System
open System.Diagnostics
open Giraffe
open Thoth.Json.Net
open EATool.Domain
open EATool.Infrastructure
open EATool.Infrastructure.EventStore
open EATool.Infrastructure.ApplicationEventJson
open EATool.Infrastructure.EventJson
open EATool.Infrastructure.ProjectionEngine
open EATool.Infrastructure.Tracing

module ApplicationsEndpoints =
    
    /// Helper to generate application ID
    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "app-" + guid.Substring(0, 8)
    
    /// Create event store for ApplicationEvents
    let private createApplicationEventStore () =
        let connectionString = Database.getConnectionString()
        createSqlEventStore(connectionString, encodeApplicationEvent, decodeApplicationEvent)
    
    /// Create projection engine for ApplicationEvents
    let private createProjectionEngine (eventStore: IEventStore<ApplicationEvent>) =
        let connectionString = Database.getConnectionString()
        let handlers: IProjectionHandler<ApplicationEvent> list = [
            Projections.ApplicationProjection.Handler(connectionString) :> IProjectionHandler<ApplicationEvent>
        ]
        ProjectionEngine<ApplicationEvent>(connectionString, eventStore, handlers)
    
    /// Extract aggregate Guid from app-* identifier
    let private parseAggregateId (aggregateId: string) : Guid =
        let guidPart = if aggregateId.StartsWith("app-") then aggregateId.Substring(4) else aggregateId
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

    /// Helper to create EventEnvelope from an ApplicationEvent
    let private createEventEnvelope (aggregateId: string) (aggregateGuid: Guid) (aggregateVersion: int) (event: ApplicationEvent) (actor: string, actorType: ActorType, correlationId: Guid, causationId: Guid) : EventEnvelope<ApplicationEvent> =
        {
            EventId = Guid.NewGuid()
            AggregateId = aggregateGuid
            AggregateType = "Application"
            AggregateVersion = aggregateVersion
            EventType =
                match event with
                | ApplicationCreated _ -> "ApplicationCreated"
                | DataClassificationChanged _ -> "DataClassificationChanged"
                | LifecycleTransitioned _ -> "LifecycleTransitioned"
                | OwnerSet _ -> "OwnerSet"
                | CapabilityAssigned _ -> "CapabilityAssigned"
                | CapabilityRemoved _ -> "CapabilityRemoved"
                | TagsAdded _ -> "TagsAdded"
                | TagsRemoved _ -> "TagsRemoved"
                | CriticalitySet _ -> "CriticalitySet"
                | DescriptionUpdated _ -> "DescriptionUpdated"
                | ApplicationDeleted _ -> "ApplicationDeleted"
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
    let private loadAggregateState (eventStore: IEventStore<ApplicationEvent>) (aggregateId: string) =
        let aggregateGuid = parseAggregateId aggregateId
        let existingEvents = eventStore.GetEvents aggregateGuid
        let baseVersion = eventStore.GetAggregateVersion aggregateGuid
        let stateFromEvents = existingEvents |> List.fold (fun acc e -> ApplicationAggregate.apply acc e.Data) ApplicationAggregate.Initial

        let state =
            if existingEvents |> List.isEmpty then
                match ApplicationRepository.getById aggregateId with
                | Some app ->
                    let lifecycleStr =
                        match app.Lifecycle with
                        | Lifecycle.Planned -> "planned"
                        | Lifecycle.Active -> "active"
                        | Lifecycle.Deprecated -> "deprecated"
                        | Lifecycle.Retired -> "retired"

                    { ApplicationAggregate.Initial with
                        Id = Some app.Id
                        Name = Some app.Name
                        Owner = app.Owner
                        Lifecycle = Some lifecycleStr
                        CapabilityId = app.CapabilityId
                        DataClassification = app.DataClassification
                        Criticality = None
                        Tags = app.Tags
                        Description = None
                        IsDeleted = false }
                | None -> ApplicationAggregate.Initial
            else stateFromEvents

        state, baseVersion, aggregateGuid

    /// Append events and project them; returns envelopes or error
    let private persistAndProject (eventStore: IEventStore<ApplicationEvent>) (projectionEngine: ProjectionEngine<ApplicationEvent>) (aggregateId: string) (aggregateGuid: Guid) (baseVersion: int) (meta: string * ActorType * Guid * Guid) (events: ApplicationEvent list) =
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

    /// Encode an event envelope for debugging APIs
    let private encodeEventEnvelope (env: EventEnvelope<ApplicationEvent>) : JsonValue =
        Encode.object [
            "event_id", Encode.string (env.EventId.ToString())
            "aggregate_id", Encode.string (env.AggregateId.ToString())
            "aggregate_type", Encode.string env.AggregateType
            "aggregate_version", Encode.int env.AggregateVersion
            "event_type", Encode.string env.EventType
            "event_version", Encode.int env.EventVersion
            "event_timestamp", Encode.string (env.EventTimestamp.ToString("o"))
            "actor", Encode.string env.Actor
            "actor_type", Encode.string (env.ActorType.ToString())
            "source", Encode.string (env.Source.ToString())
            "causation_id", Encode.option (Encode.string) (env.CausationId |> Option.map string)
            "correlation_id", Encode.option (Encode.string) (env.CorrelationId |> Option.map string)
            "data", encodeApplicationEvent env.Data
        ]
    
    let routes: HttpHandler list =
        [
            // GET /applications - list (read from projection)
            GET >=> route "/applications" >=> fun next ctx -> task {
                let page = ctx.TryGetQueryStringValue "page" |> Option.bind (fun s -> try Some (int s) with _ -> None) |> Option.defaultValue 1
                let limit = ctx.TryGetQueryStringValue "limit" |> Option.bind (fun s -> try Some (int s) with _ -> None) |> Option.defaultValue 50
                let search = ctx.TryGetQueryStringValue "search" |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))
                let owner = ctx.TryGetQueryStringValue "owner" |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))
                let lifecycle =
                    ctx.TryGetQueryStringValue "lifecycle"
                    |> Option.bind (fun s ->
                        match s.ToLowerInvariant() with
                        | "planned" -> Some Lifecycle.Planned
                        | "active" -> Some Lifecycle.Active
                        | "deprecated" -> Some Lifecycle.Deprecated
                        | "retired" -> Some Lifecycle.Retired
                        | "sunset" -> Some Lifecycle.Deprecated
                        | _ -> None)

                let pageParam = if page < 1 then 1 else page
                let limitParam = if limit < 1 || limit > 200 then 50 else limit

                let result = ApplicationRepository.getAll pageParam limitParam search owner lifecycle
                let json = Json.encodePaginatedResponse Json.encodeApplication result
                return! (Giraffe.Core.json json) next ctx
            }

            // POST /applications - create application via CreateApplication command
            POST >=> route "/applications" >=> fun next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateApplicationRequest bodyStr with
                | Ok req ->
                    try
                        // Generate unique application ID
                        let appId = generateId()
                        
                        // Record command span event
                        let activity = Activity.Current
                        if activity <> null then
                            activity.SetTag("command.type", "CreateApplication") |> ignore
                            activity.SetTag("entity.id", appId) |> ignore
                            activity.SetTag("entity.type", "Application") |> ignore
                        
                        // Create command
                        let cmd : CreateApplicationData = {
                            Id = appId
                            Name = req.Name
                            Owner = req.Owner
                            Lifecycle = req.Lifecycle |> function
                                | Lifecycle.Planned -> "planned"
                                | Lifecycle.Active -> "active"
                                | Lifecycle.Deprecated -> "deprecated"
                                | Lifecycle.Retired -> "retired"
                            CapabilityId = req.CapabilityId
                            DataClassification = req.DataClassification
                            Criticality = None
                            Tags = req.Tags |> Option.defaultValue []
                            Description = None
                        }
                        
                        // Validate command and generate events
                        let state = ApplicationAggregate.Initial
                        match ApplicationCommandHandler.handleCreateApplication state cmd with
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
                            
                            let eventStore = createApplicationEventStore()
                            let projectionEngine = createProjectionEngine eventStore
                            let aggregateGuid = parseAggregateId appId
                            let baseVersion = eventStore.GetAggregateVersion aggregateGuid
                            let meta = getActorMetadata ctx

                            match persistAndProject eventStore projectionEngine appId aggregateGuid baseVersion meta events with
                            | Error err ->
                                if activity <> null then
                                    activity.SetTag("event.persist.error", err) |> ignore
                                ctx.SetStatusCode 500
                                let errJson = Json.encodeErrorResponse "event_store_error" err
                                return! (Giraffe.Core.json errJson) next ctx
                            | Ok _ ->
                                if activity <> null then
                                    activity.SetTag("command.result", "success") |> ignore
                                match ApplicationRepository.getById appId with
                                | Some app ->
                                    ctx.SetStatusCode 201
                                    ctx.SetHttpHeader ("Location", $"/applications/{app.Id}")
                                    let json = Json.encodeApplication app
                                    return! (Giraffe.Core.json json) next ctx
                                | None ->
                                    let finalState = events |> List.fold ApplicationAggregate.apply state
                                    ctx.SetStatusCode 201
                                    ctx.SetHttpHeader ("Location", $"/applications/{appId}")
                                    let responseJson = Encode.object [
                                        "id", Encode.string appId
                                        "name", Encode.string (finalState.Name |> Option.defaultValue "")
                                        "owner", Encode.option Encode.string finalState.Owner
                                        "lifecycle", Encode.string (finalState.Lifecycle |> Option.defaultValue "planned")
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

            // GET /applications/{id}
            GET >=> routef "/applications/%s" (fun id next ctx -> task {
                match ApplicationRepository.getById id with
                | Some app ->
                    let json = Json.encodeApplication app
                    return! (Giraffe.Core.json json) next ctx
                | None ->
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "Application not found"
                    return! (Giraffe.Core.json errJson) next ctx
            })

            // GET /applications/{id}/events - debugging endpoint to inspect event stream
            GET >=> routef "/applications/%s/events" (fun id next ctx -> task {
                let limit =
                    ctx.TryGetQueryStringValue "limit"
                    |> Option.bind (fun s -> match Int32.TryParse s with | true, v -> Some v | _ -> None)
                    |> Option.defaultValue 50
                    |> max 1
                    |> min 200

                let eventStore = createApplicationEventStore()
                let aggregateGuid = parseAggregateId id
                let events = eventStore.GetEvents aggregateGuid
                if events.IsEmpty then
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "No events found for aggregate"
                    return! (Giraffe.Core.json errJson) next ctx
                else
                    let trimmed =
                        events
                        |> List.rev
                        |> List.truncate limit
                        |> List.rev
                        |> List.map encodeEventEnvelope
                        |> Encode.list
                    return! (Giraffe.Core.json trimmed) next ctx
            })

            // POST /applications/{id}/commands/set-classification
            POST >=> routef "/applications/%s/commands/set-classification" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                
                let decoder = Decode.object (fun get -> {|
                    Classification = get.Required.Field "classification" Decode.string
                    Reason = get.Required.Field "reason" Decode.string
                |})
                
                match Decode.fromString decoder bodyStr with
                | Ok req ->
                    let cmd : SetDataClassificationData = {
                        Id = id
                        Classification = req.Classification
                        Reason = req.Reason
                    }

                    let eventStore = createApplicationEventStore()
                    let projectionEngine = createProjectionEngine eventStore
                    let state, baseVersion, aggregateGuid = loadAggregateState eventStore id

                    match state.Id with
                    | None ->
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "Application not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    | Some _ ->
                        match ApplicationCommandHandler.handleSetDataClassification state cmd with
                        | Error err ->
                            ctx.SetStatusCode 400
                            let errJson = Json.encodeErrorResponse "business_rule_violation" err
                            return! (Giraffe.Core.json errJson) next ctx
                        | Ok events ->
                            let meta = getActorMetadata ctx
                            match persistAndProject eventStore projectionEngine id aggregateGuid baseVersion meta events with
                            | Error err ->
                                ctx.SetStatusCode 500
                                let errJson = Json.encodeErrorResponse "event_store_error" err
                                return! (Giraffe.Core.json errJson) next ctx
                            | Ok _ ->
                                match ApplicationRepository.getById id with
                                | Some updatedApp ->
                                    let json = Json.encodeApplication updatedApp
                                    return! (Giraffe.Core.json json) next ctx
                                | None ->
                                    ctx.SetStatusCode 404
                                    let errJson = Json.encodeErrorResponse "not_found" "Application not found"
                                    return! (Giraffe.Core.json errJson) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errJson) next ctx
            })

            // POST /applications/{id}/commands/transition-lifecycle
            POST >=> routef "/applications/%s/commands/transition-lifecycle" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                
                let decoder = Decode.object (fun get -> {|
                    TargetLifecycle = get.Required.Field "target_lifecycle" Decode.string
                    SunsetDate = get.Optional.Field "sunset_date" Decode.string
                |})
                
                match Decode.fromString decoder bodyStr with
                | Ok req ->
                    let cmd : TransitionLifecycleData = {
                        Id = id
                        TargetLifecycle = req.TargetLifecycle
                        SunsetDate = req.SunsetDate
                    }

                    let eventStore = createApplicationEventStore()
                    let projectionEngine = createProjectionEngine eventStore
                    let state, baseVersion, aggregateGuid = loadAggregateState eventStore id

                    match state.Id with
                    | None ->
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "Application not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    | Some _ ->
                        match ApplicationCommandHandler.handleTransitionLifecycle state cmd with
                        | Error err ->
                            ctx.SetStatusCode 400
                            let errJson = Json.encodeErrorResponse "business_rule_violation" err
                            return! (Giraffe.Core.json errJson) next ctx
                        | Ok events ->
                            let meta = getActorMetadata ctx
                            match persistAndProject eventStore projectionEngine id aggregateGuid baseVersion meta events with
                            | Error err ->
                                ctx.SetStatusCode 500
                                let errJson = Json.encodeErrorResponse "event_store_error" err
                                return! (Giraffe.Core.json errJson) next ctx
                            | Ok _ ->
                                match ApplicationRepository.getById id with
                                | Some updatedApp ->
                                    let json = Json.encodeApplication updatedApp
                                    return! (Giraffe.Core.json json) next ctx
                                | None ->
                                    ctx.SetStatusCode 404
                                    let errJson = Json.encodeErrorResponse "not_found" "Application not found"
                                    return! (Giraffe.Core.json errJson) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errJson) next ctx
            })

            // POST /applications/{id}/commands/set-owner
            POST >=> routef "/applications/%s/commands/set-owner" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                
                let decoder = Decode.object (fun get -> {|
                    Owner = get.Required.Field "owner" Decode.string
                    Reason = get.Optional.Field "reason" Decode.string
                |})
                
                match Decode.fromString decoder bodyStr with
                | Ok req ->
                    let cmd : SetOwnerData = {
                        Id = id
                        Owner = req.Owner
                        Reason = req.Reason
                    }

                    let eventStore = createApplicationEventStore()
                    let projectionEngine = createProjectionEngine eventStore
                    let state, baseVersion, aggregateGuid = loadAggregateState eventStore id

                    match state.Id with
                    | None ->
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "Application not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    | Some _ ->
                        match ApplicationCommandHandler.handleSetOwner state cmd with
                        | Error err ->
                            ctx.SetStatusCode 400
                            let errJson = Json.encodeErrorResponse "business_rule_violation" err
                            return! (Giraffe.Core.json errJson) next ctx
                        | Ok events ->
                            let meta = getActorMetadata ctx
                            match persistAndProject eventStore projectionEngine id aggregateGuid baseVersion meta events with
                            | Error err ->
                                ctx.SetStatusCode 500
                                let errJson = Json.encodeErrorResponse "event_store_error" err
                                return! (Giraffe.Core.json errJson) next ctx
                            | Ok _ ->
                                match ApplicationRepository.getById id with
                                | Some updatedApp ->
                                    let json = Json.encodeApplication updatedApp
                                    return! (Giraffe.Core.json json) next ctx
                                | None ->
                                    ctx.SetStatusCode 404
                                    let errJson = Json.encodeErrorResponse "not_found" "Application not found"
                                    return! (Giraffe.Core.json errJson) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errJson) next ctx
            })

            // DELETE /applications/{id} - requires approval
            DELETE >=> routef "/applications/%s" (fun id next ctx -> task {
                let approvalId = ctx.TryGetQueryStringValue "approval_id"
                let reason = ctx.TryGetQueryStringValue "reason"
                
                match approvalId, reason with
                | None, _ | _, None ->
                    ctx.SetStatusCode 400
                    let errJson = Json.encodeErrorResponse "validation_error" "approval_id and reason query parameters are required for deletion"
                    return! (Giraffe.Core.json errJson) next ctx
                | Some approval, Some deleteReason ->
                    let cmd : DeleteApplicationData = {
                        Id = id
                        Reason = deleteReason
                        ApprovalId = approval
                    }

                    let eventStore = createApplicationEventStore()
                    let projectionEngine = createProjectionEngine eventStore
                    let state, baseVersion, aggregateGuid = loadAggregateState eventStore id

                    match state.Id with
                    | None ->
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "Application not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    | Some _ ->
                        match ApplicationCommandHandler.handleDeleteApplication state cmd with
                        | Error err ->
                            ctx.SetStatusCode 400
                            let errJson = Json.encodeErrorResponse "business_rule_violation" err
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
            })

            // Legacy PATCH endpoint - deprecated, but kept for backwards compatibility
            PATCH >=> routef "/applications/%s" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateApplicationRequest bodyStr with
                | Ok req ->
                    if String.IsNullOrWhiteSpace(req.Name) then
                        ctx.SetStatusCode 400
                        let errJson = Json.encodeErrorResponse "validation_error" "Request validation failed"
                        return! (Giraffe.Core.json errJson) next ctx
                    else
                        match ApplicationRepository.update id req with
                        | Some app ->
                            let json = Json.encodeApplication app
                            return! (Giraffe.Core.json json) next ctx
                        | None ->
                            ctx.SetStatusCode 404
                            let errJson = Json.encodeErrorResponse "not_found" "Application not found"
                            return! (Giraffe.Core.json errJson) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errJson) next ctx
            })
        ]
