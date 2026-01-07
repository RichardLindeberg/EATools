/// Organization API endpoints with command-based architecture
namespace EATool.Api

open System
open Giraffe
open Thoth.Json.Net
open EATool.Domain
open EATool.Infrastructure
open EATool.Infrastructure.EventStore
open EATool.Infrastructure.OrganizationEventJson
open EATool.Infrastructure.EventJson
open EATool.Infrastructure.ProjectionEngine

module OrganizationsEndpoints =
    
    /// Helper to generate organization ID
    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "org-" + guid.Substring(0, 8)
    
    /// Create event store for OrganizationEvents
    let private createOrganizationEventStore () =
        let connectionString = Database.getConnectionString()
        createSqlEventStore(connectionString, encodeOrganizationEvent, decodeOrganizationEvent)
    
    /// Create projection engine for OrganizationEvents
    let private createProjectionEngine (eventStore: IEventStore<OrganizationEvent>) =
        let connectionString = Database.getConnectionString()
        let handlers: IProjectionHandler<OrganizationEvent> list = [
            Projections.OrganizationProjection.Handler(connectionString) :> IProjectionHandler<OrganizationEvent>
        ]
        ProjectionEngine<OrganizationEvent>(connectionString, eventStore, handlers)
    
    /// Extract aggregate Guid from org-* identifier
    let private parseAggregateId (aggregateId: string) : Guid =
        let guidPart = if aggregateId.StartsWith("org-") then aggregateId.Substring(4) else aggregateId
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
    let private createEventEnvelope (aggregateId: string) (aggregateGuid: Guid) (version: int) (event: OrganizationEvent) (meta: string * ActorType * Guid * Guid) =
        let (actor, actorType, correlationId, causationId) = meta
        {
            EventId = Guid.NewGuid()
            AggregateId = aggregateGuid
            AggregateType = "Organization"
            AggregateVersion = version
            EventType =
                match event with
                | OrganizationCreated _ -> "OrganizationCreated"
                | ParentAssigned _ -> "ParentAssigned"
                | ParentRemoved _ -> "ParentRemoved"
                | ContactInfoUpdated _ -> "ContactInfoUpdated"
                | DomainAdded _ -> "DomainAdded"
                | DomainRemoved _ -> "DomainRemoved"
                | OrganizationDeleted _ -> "OrganizationDeleted"
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
    let private loadAggregateState (eventStore: IEventStore<OrganizationEvent>) (aggregateId: string) =
        let aggregateGuid = parseAggregateId aggregateId
        let existingEvents = eventStore.GetEvents aggregateGuid
        let baseVersion = eventStore.GetAggregateVersion aggregateGuid
        let stateFromEvents = existingEvents |> List.fold (fun acc e -> OrganizationAggregate.apply acc e.Data) OrganizationAggregate.Initial

        let state =
            if existingEvents |> List.isEmpty then
                // Fallback: load from projection
                match OrganizationRepository.getById aggregateId with
                | Some org ->
                    { OrganizationAggregate.Initial with
                        Id = Some org.Id
                        Name = Some org.Name
                        ParentId = org.ParentId
                        Domains = org.Domains
                        Contacts = org.Contacts
                        IsDeleted = false }
                | None -> OrganizationAggregate.Initial
            else stateFromEvents

        state, baseVersion, aggregateGuid

    /// Append events and project them; returns envelopes or error
    let private persistAndProject (eventStore: IEventStore<OrganizationEvent>) (projectionEngine: ProjectionEngine<OrganizationEvent>) (aggregateId: string) (aggregateGuid: Guid) (baseVersion: int) (meta: string * ActorType * Guid * Guid) (events: OrganizationEvent list) =
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
            // GET /organizations - List with pagination and search
            GET >=> route "/organizations" >=> fun next ctx -> task {
                let page = ctx.TryGetQueryStringValue "page" |> Option.bind (fun s -> try Some (int s) with _ -> None) |> Option.defaultValue 1
                let limit = ctx.TryGetQueryStringValue "limit" |> Option.bind (fun s -> try Some (int s) with _ -> None) |> Option.defaultValue 50
                let search = ctx.TryGetQueryStringValue "search" |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))
                let parentId = ctx.TryGetQueryStringValue "parent_id" |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))
                
                let pageParam = if page < 1 then 1 else page
                let limitParam = if limit < 1 || limit > 200 then 50 else limit
                
                let result = OrganizationRepository.getAll pageParam limitParam search parentId
                let json = Json.encodePaginatedResponse Json.encodeOrganization result
                return! (Giraffe.Core.json json) next ctx
            }
            
            // POST /organizations - Create
            POST >=> route "/organizations" >=> fun next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateOrganizationRequest bodyStr with
                | Ok req ->
                    if String.IsNullOrWhiteSpace(req.Name) then
                        ctx.SetStatusCode 400
                        let errorJson = Json.encodeErrorResponse "validation_error" "Organization name is required"
                        return! (Giraffe.Core.json errorJson) next ctx
                    else
                        let orgId = generateId()
                        let cmd : CreateOrganizationData = {
                            Id = orgId
                            Name = req.Name
                            ParentId = req.ParentId
                            Domains = req.Domains
                            Contacts = req.Contacts
                        }
                        
                        // Validate command and generate events
                        let state = OrganizationAggregate.Initial
                        match OrganizationCommandHandler.handleCreateOrganization state cmd OrganizationRepository.getById with
                        | Error err ->
                            ctx.SetStatusCode 400
                            let errJson = Json.encodeErrorResponse "validation_error" err
                            return! (Giraffe.Core.json errJson) next ctx
                        | Ok events ->
                            let eventStore = createOrganizationEventStore()
                            let projectionEngine = createProjectionEngine eventStore
                            let aggregateGuid = parseAggregateId orgId
                            let baseVersion = eventStore.GetAggregateVersion aggregateGuid
                            let meta = getActorMetadata ctx

                            match persistAndProject eventStore projectionEngine orgId aggregateGuid baseVersion meta events with
                            | Error err ->
                                ctx.SetStatusCode 500
                                let errJson = Json.encodeErrorResponse "event_store_error" err
                                return! (Giraffe.Core.json errJson) next ctx
                            | Ok _ ->
                                match OrganizationRepository.getById orgId with
                                | Some org ->
                                    ctx.SetStatusCode 201
                                    ctx.SetHttpHeader ("Location", $"/organizations/{org.Id}")
                                    let responseJson = Json.encodeOrganization org
                                    return! (Giraffe.Core.json responseJson) next ctx
                                | None ->
                                    ctx.SetStatusCode 201
                                    return! next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errorJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errorJson) next ctx
            }
            
            // GET /organizations/{id} - Get by ID
            GET >=> routef "/organizations/%s" (fun id next ctx -> task {
                match OrganizationRepository.getById id with
                | Some org -> 
                    let json = Json.encodeOrganization org
                    return! (Giraffe.Core.json json) next ctx
                | None ->
                    ctx.SetStatusCode 404
                    let errorJson = Json.encodeErrorResponse "not_found" "Organization not found"
                    return! (Giraffe.Core.json errorJson) next ctx
            })
            
            // PATCH /organizations/{id} - Update (dispatches to commands)
            PATCH >=> routef "/organizations/%s" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateOrganizationRequest bodyStr with
                | Ok req ->
                    let eventStore = createOrganizationEventStore()
                    let projectionEngine = createProjectionEngine eventStore
                    let state, baseVersion, aggregateGuid = loadAggregateState eventStore id

                    match state.Id with
                    | None ->
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "Organization not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    | Some _ ->
                        // Determine which command to execute based on changes
                        let parentChanged = req.ParentId <> state.ParentId
                        let nameChanged = Some req.Name <> state.Name
                        
                        if parentChanged then
                            // Handle parent change
                            let parentCmd = 
                                match req.ParentId with
                                | Some pid -> 
                                    let cmd : SetParentData = { Id = id; ParentId = pid }
                                    OrganizationCommandHandler.handleSetParent state cmd OrganizationRepository.getById
                                | None ->
                                    let cmd : RemoveParentData = { Id = id }
                                    OrganizationCommandHandler.handleRemoveParent state cmd
                            
                            match parentCmd with
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
                                    match OrganizationRepository.getById id with
                                    | Some updatedOrg ->
                                        let json = Json.encodeOrganization updatedOrg
                                        return! (Giraffe.Core.json json) next ctx
                                    | None ->
                                        ctx.SetStatusCode 404
                                        let errJson = Json.encodeErrorResponse "not_found" "Organization not found"
                                        return! (Giraffe.Core.json errJson) next ctx
                        elif nameChanged then
                            // Name change - update via repository for now (TODO: add UpdateName command)
                            match OrganizationRepository.update id req with
                            | Some updatedOrg ->
                                let json = Json.encodeOrganization updatedOrg
                                return! (Giraffe.Core.json json) next ctx
                            | None ->
                                ctx.SetStatusCode 404
                                let errJson = Json.encodeErrorResponse "not_found" "Organization not found"
                                return! (Giraffe.Core.json errJson) next ctx
                        else
                            // No changes - just return current state
                            match OrganizationRepository.getById id with
                            | Some org ->
                                let json = Json.encodeOrganization org
                                return! (Giraffe.Core.json json) next ctx
                            | None ->
                                ctx.SetStatusCode 404
                                let errJson = Json.encodeErrorResponse "not_found" "Organization not found"
                                return! (Giraffe.Core.json errJson) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errorJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errorJson) next ctx
            })
            
            // POST /organizations/{id}/commands/set-parent - Set parent with cycle detection
            POST >=> routef "/organizations/%s/commands/set-parent" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                
                let decoder = Decode.object (fun get -> {|
                    ParentId = get.Required.Field "parent_id" Decode.string
                |})
                
                match Decode.fromString decoder bodyStr with
                | Ok req ->
                    let cmd : SetParentData = {
                        Id = id
                        ParentId = req.ParentId
                    }

                    let eventStore = createOrganizationEventStore()
                    let projectionEngine = createProjectionEngine eventStore
                    let state, baseVersion, aggregateGuid = loadAggregateState eventStore id

                    match state.Id with
                    | None ->
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "Organization not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    | Some _ ->
                        match OrganizationCommandHandler.handleSetParent state cmd OrganizationRepository.getById with
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
                                match OrganizationRepository.getById id with
                                | Some updatedOrg ->
                                    let json = Json.encodeOrganization updatedOrg
                                    return! (Giraffe.Core.json json) next ctx
                                | None ->
                                    ctx.SetStatusCode 404
                                    let errJson = Json.encodeErrorResponse "not_found" "Organization not found"
                                    return! (Giraffe.Core.json errJson) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errorJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errorJson) next ctx
            })
            
            // POST /organizations/{id}/commands/remove-parent - Remove parent
            POST >=> routef "/organizations/%s/commands/remove-parent" (fun id next ctx -> task {
                let cmd : RemoveParentData = { Id = id }

                let eventStore = createOrganizationEventStore()
                let projectionEngine = createProjectionEngine eventStore
                let state, baseVersion, aggregateGuid = loadAggregateState eventStore id

                match state.Id with
                | None ->
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "Organization not found"
                    return! (Giraffe.Core.json errJson) next ctx
                | Some _ ->
                    match OrganizationCommandHandler.handleRemoveParent state cmd with
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
                            match OrganizationRepository.getById id with
                            | Some updatedOrg ->
                                let json = Json.encodeOrganization updatedOrg
                                return! (Giraffe.Core.json json) next ctx
                            | None ->
                                ctx.SetStatusCode 404
                                let errJson = Json.encodeErrorResponse "not_found" "Organization not found"
                                return! (Giraffe.Core.json errJson) next ctx
            })
            
            // DELETE /organizations/{id} - Delete
            DELETE >=> routef "/organizations/%s" (fun id next ctx -> task {
                let reason =
                    ctx.TryGetQueryStringValue "reason"
                    |> Option.defaultValue "User requested deletion"
                
                let cmd : DeleteOrganizationData = {
                    Id = id
                    Reason = reason
                }

                let eventStore = createOrganizationEventStore()
                let projectionEngine = createProjectionEngine eventStore
                let state, baseVersion, aggregateGuid = loadAggregateState eventStore id

                match state.Id with
                | None ->
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "Organization not found"
                    return! (Giraffe.Core.json errJson) next ctx
                | Some _ ->
                    match OrganizationCommandHandler.handleDeleteOrganization state cmd with
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
