/// Business capability API endpoints with command-based architecture
namespace EATool.Api

open System
open Giraffe
open Thoth.Json.Net
open EATool.Domain
open EATool.Infrastructure
open EATool.Infrastructure.EventStore
open EATool.Infrastructure.BusinessCapabilityEventJson
open EATool.Infrastructure.EventJson
open EATool.Infrastructure.ProjectionEngine

module BusinessCapabilitiesEndpoints =
    
    /// Helper to generate business capability ID
    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "cap-" + guid.Substring(0, 8)
    
    /// Create event store for BusinessCapabilityEvents
    let private createBusinessCapabilityEventStore () =
        let connectionString = Database.getConnectionString()
        createSqlEventStore(connectionString, encodeBusinessCapabilityEvent, decodeBusinessCapabilityEvent)
    
    /// Create projection engine for BusinessCapabilityEvents
    let private createProjectionEngine (eventStore: IEventStore<BusinessCapabilityEvent>) =
        let connectionString = Database.getConnectionString()
        let handlers: IProjectionHandler<BusinessCapabilityEvent> list = [
            Projections.BusinessCapabilityProjection.Handler(connectionString) :> IProjectionHandler<BusinessCapabilityEvent>
        ]
        ProjectionEngine<BusinessCapabilityEvent>(connectionString, eventStore, handlers)
    
    /// Extract aggregate Guid from cap-* identifier
    let private parseAggregateId (aggregateId: string) : Guid =
        let guidPart = if aggregateId.StartsWith("cap-") then aggregateId.Substring(4) else aggregateId
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
    let private createEventEnvelope (aggregateId: string) (aggregateGuid: Guid) (version: int) (event: BusinessCapabilityEvent) (meta: string * ActorType * Guid * Guid) =
        let (actor, actorType, correlationId, causationId) = meta
        let eventType =
            match event with
            | CapabilityCreated _ -> "CapabilityCreated"
            | CapabilityParentAssigned _ -> "CapabilityParentAssigned"
            | CapabilityParentRemoved _ -> "CapabilityParentRemoved"
            | CapabilityDescriptionUpdated _ -> "CapabilityDescriptionUpdated"
            | CapabilityDeleted _ -> "CapabilityDeleted"
        {
            EventId = Guid.NewGuid()
            AggregateId = aggregateGuid
            AggregateType = "BusinessCapability"
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
    let private loadAggregateState (eventStore: IEventStore<BusinessCapabilityEvent>) (aggregateId: string) =
        let aggregateGuid = parseAggregateId aggregateId
        let existingEvents = eventStore.GetEvents aggregateGuid
        let baseVersion = eventStore.GetAggregateVersion aggregateGuid
        let stateFromEvents = existingEvents |> List.fold (fun acc e -> BusinessCapabilityAggregate.apply acc e.Data) BusinessCapabilityAggregate.Initial

        let state =
            if existingEvents |> List.isEmpty then
                // Fallback: load from projection
                match BusinessCapabilityRepository.getById aggregateId with
                | Some cap ->
                    let capState: BusinessCapabilityState = {
                        Id = cap.Id
                        Name = cap.Name
                        ParentId = cap.ParentId
                        Description = cap.Description
                    }
                    BusinessCapabilityAggregate.Active capState
                | None -> BusinessCapabilityAggregate.Initial
            else stateFromEvents

        state, baseVersion, aggregateGuid

    /// Append events and project them; returns envelopes or error
    let private persistAndProject (eventStore: IEventStore<BusinessCapabilityEvent>) (projectionEngine: ProjectionEngine<BusinessCapabilityEvent>) (aggregateId: string) (aggregateGuid: Guid) (baseVersion: int) (meta: string * ActorType * Guid * Guid) (events: BusinessCapabilityEvent list) =
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
            // GET /business-capabilities - List with pagination and search
            GET >=> route "/business-capabilities" >=> fun next ctx -> task {
                let page = ctx.TryGetQueryStringValue "page" |> Option.bind (fun s -> try Some (int s) with _ -> None) |> Option.defaultValue 1
                let limit = ctx.TryGetQueryStringValue "limit" |> Option.bind (fun s -> try Some (int s) with _ -> None) |> Option.defaultValue 50
                let search = ctx.TryGetQueryStringValue "search" |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))
                let parentId = ctx.TryGetQueryStringValue "parent_id" |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))
                
                let pageParam = if page < 1 then 1 else page
                let limitParam = if limit < 1 || limit > 200 then 50 else limit
                
                let result = BusinessCapabilityRepository.getAll pageParam limitParam search parentId
                let json = Json.encodePaginatedResponse Json.encodeBusinessCapability result
                return! (Giraffe.Core.json json) next ctx
            }
            
            // POST /business-capabilities - Create
            POST >=> route "/business-capabilities" >=> fun next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateBusinessCapabilityRequest bodyStr with
                | Ok req ->
                    if String.IsNullOrWhiteSpace(req.Name) then
                        ctx.SetStatusCode 400
                        let errorJson = Json.encodeErrorResponse "validation_error" "Business capability name is required"
                        return! (Giraffe.Core.json errorJson) next ctx
                    else
                        // Proceed with command generation; uniqueness will be enforced by projection/repository

                        let capId = generateId()
                        let cmd : CreateCapabilityData = {
                            Id = capId
                            Name = req.Name
                            ParentId = req.ParentId
                            Description = req.Description
                        }
                        
                        // Validate command and generate events
                        let state = BusinessCapabilityAggregate.Initial
                        match BusinessCapabilityCommandHandler.handleCreateCapability state cmd BusinessCapabilityRepository.getById with
                        | Error err ->
                            ctx.SetStatusCode 400
                            let errJson = Json.encodeErrorResponse "validation_error" err
                            return! (Giraffe.Core.json errJson) next ctx
                        | Ok events ->
                            let eventStore = createBusinessCapabilityEventStore()
                            let projectionEngine = createProjectionEngine eventStore
                            let aggregateGuid = parseAggregateId capId
                            let baseVersion = eventStore.GetAggregateVersion aggregateGuid
                            let meta = getActorMetadata ctx

                            match persistAndProject eventStore projectionEngine capId aggregateGuid baseVersion meta events with
                            | Error err ->
                                // Map known constraint errors to conflict
                                let isConflict =
                                    (err.Contains("already exists") || err.Contains("UNIQUE") || err.Contains("unique"))
                                if isConflict then ctx.SetStatusCode 409 else ctx.SetStatusCode 500
                                let errType = if isConflict then "conflict" else "event_store_error"
                                let errJson = Json.encodeErrorResponse errType err
                                return! (Giraffe.Core.json errJson) next ctx
                            | Ok _ ->
                                match BusinessCapabilityRepository.getById capId with
                                | Some cap ->
                                    ctx.SetStatusCode 201
                                    ctx.SetHttpHeader ("Location", $"/business-capabilities/{cap.Id}")
                                    let responseJson = Json.encodeBusinessCapability cap
                                    return! (Giraffe.Core.json responseJson) next ctx
                                | None ->
                                    ctx.SetStatusCode 201
                                    return! next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errorJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errorJson) next ctx
            }
            
            // GET /business-capabilities/{id} - Get by ID
            GET >=> routef "/business-capabilities/%s" (fun id next ctx -> task {
                match BusinessCapabilityRepository.getById id with
                | Some cap -> 
                    let json = Json.encodeBusinessCapability cap
                    return! (Giraffe.Core.json json) next ctx
                | None ->
                    ctx.SetStatusCode 404
                    let errorJson = Json.encodeErrorResponse "not_found" "Business capability not found"
                    return! (Giraffe.Core.json errorJson) next ctx
            })
            
            // POST /business-capabilities/{id}/commands/set-parent - Set parent with cycle detection
            POST >=> routef "/business-capabilities/%s/commands/set-parent" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                
                let decoder = Decode.object (fun get -> {|
                    ParentId = get.Required.Field "parent_id" Decode.string
                |})
                
                match Decode.fromString decoder bodyStr with
                | Ok req ->
                    let cmd : SetCapabilityParentData = {
                        Id = id
                        ParentId = req.ParentId
                    }

                    let eventStore = createBusinessCapabilityEventStore()
                    let projectionEngine = createProjectionEngine eventStore
                    let state, baseVersion, aggregateGuid = loadAggregateState eventStore id

                    match state with
                    | BusinessCapabilityAggregate.Initial | BusinessCapabilityAggregate.Deleted ->
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "Business capability not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    | BusinessCapabilityAggregate.Active _ ->
                        match BusinessCapabilityCommandHandler.handleSetParent state cmd BusinessCapabilityRepository.getById with
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
                                match BusinessCapabilityRepository.getById id with
                                | Some updatedCap ->
                                    let json = Json.encodeBusinessCapability updatedCap
                                    return! (Giraffe.Core.json json) next ctx
                                | None ->
                                    ctx.SetStatusCode 404
                                    let errJson = Json.encodeErrorResponse "not_found" "Business capability not found"
                                    return! (Giraffe.Core.json errJson) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errorJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errorJson) next ctx
            })
            
            // POST /business-capabilities/{id}/commands/remove-parent - Remove parent
            POST >=> routef "/business-capabilities/%s/commands/remove-parent" (fun id next ctx -> task {
                let cmd : RemoveCapabilityParentData = { Id = id }

                let eventStore = createBusinessCapabilityEventStore()
                let projectionEngine = createProjectionEngine eventStore
                let state, baseVersion, aggregateGuid = loadAggregateState eventStore id

                match state with
                | BusinessCapabilityAggregate.Initial | BusinessCapabilityAggregate.Deleted ->
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "Business capability not found"
                    return! (Giraffe.Core.json errJson) next ctx
                | BusinessCapabilityAggregate.Active _ ->
                    match BusinessCapabilityCommandHandler.handleRemoveParent state cmd with
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
                            match BusinessCapabilityRepository.getById id with
                            | Some updatedCap ->
                                let json = Json.encodeBusinessCapability updatedCap
                                return! (Giraffe.Core.json json) next ctx
                            | None ->
                                ctx.SetStatusCode 404
                                let errJson = Json.encodeErrorResponse "not_found" "Business capability not found"
                                return! (Giraffe.Core.json errJson) next ctx
            })
            
            // POST /business-capabilities/{id}/commands/update-description - Update description
            POST >=> routef "/business-capabilities/%s/commands/update-description" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                
                let decoder = Decode.object (fun get -> {|
                    Description = get.Optional.Field "description" Decode.string
                |})
                
                match Decode.fromString decoder bodyStr with
                | Ok req ->
                    let cmd : UpdateCapabilityDescriptionData = {
                        Id = id
                        Description = req.Description
                    }

                    let eventStore = createBusinessCapabilityEventStore()
                    let projectionEngine = createProjectionEngine eventStore
                    let state, baseVersion, aggregateGuid = loadAggregateState eventStore id

                    match state with
                    | BusinessCapabilityAggregate.Initial | BusinessCapabilityAggregate.Deleted ->
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "Business capability not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    | BusinessCapabilityAggregate.Active _ ->
                        match BusinessCapabilityCommandHandler.handleUpdateDescription state cmd with
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
                                match BusinessCapabilityRepository.getById id with
                                | Some updatedCap ->
                                    let json = Json.encodeBusinessCapability updatedCap
                                    return! (Giraffe.Core.json json) next ctx
                                | None ->
                                    ctx.SetStatusCode 404
                                    let errJson = Json.encodeErrorResponse "not_found" "Business capability not found"
                                    return! (Giraffe.Core.json errJson) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errorJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errorJson) next ctx
            })
            
            // POST /business-capabilities/{id}/commands/delete - Delete capability
            POST >=> routef "/business-capabilities/%s/commands/delete" (fun id next ctx -> task {
                let cmd : DeleteCapabilityData = { Id = id }

                let eventStore = createBusinessCapabilityEventStore()
                let projectionEngine = createProjectionEngine eventStore
                let state, baseVersion, aggregateGuid = loadAggregateState eventStore id

                match state with
                | BusinessCapabilityAggregate.Initial | BusinessCapabilityAggregate.Deleted ->
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "Business capability not found"
                    return! (Giraffe.Core.json errJson) next ctx
                | BusinessCapabilityAggregate.Active _ ->
                    match BusinessCapabilityCommandHandler.handleDeleteCapability state cmd with
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
        ]
