/// Server API endpoints with command-based architecture
namespace EATool.Api

open System
open System.Diagnostics
open Giraffe
open Thoth.Json.Net
open EATool.Domain
open EATool.Infrastructure
open EATool.Infrastructure.EventStore
open EATool.Infrastructure.ServerEventJson
open EATool.Infrastructure.EventJson
open EATool.Infrastructure.ProjectionEngine

module ServersEndpoints =
    
    /// Helper to generate server ID
    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "srv-" + guid.Substring(0, 8)
    
    /// Create event store for ServerEvents
    let private createServerEventStore () =
        let connectionString = Database.getConnectionString()
        createSqlEventStore(connectionString, encodeServerEvent, decodeServerEvent)
    
    /// Create projection engine for ServerEvents
    let private createProjectionEngine (eventStore: IEventStore<ServerEvent>) =
        let connectionString = Database.getConnectionString()
        let handlers: IProjectionHandler<ServerEvent> list = [
            Projections.ServerProjection.Handler(connectionString) :> IProjectionHandler<ServerEvent>
        ]
        ProjectionEngine<ServerEvent>(connectionString, eventStore, handlers)
    
    /// Extract aggregate Guid from srv-* identifier
    let private parseAggregateId (aggregateId: string) : Guid =
        let guidPart = if aggregateId.StartsWith("srv-") then aggregateId.Substring(4) else aggregateId
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

    /// Helper to create EventEnvelope from a ServerEvent
    let private createEventEnvelope (aggregateId: string) (aggregateGuid: Guid) (aggregateVersion: int) (event: ServerEvent) (actor: string, actorType: ActorType, correlationId: Guid, causationId: Guid) : EventEnvelope<ServerEvent> =
        {
            EventId = Guid.NewGuid()
            AggregateId = aggregateGuid
            AggregateType = "Server"
            AggregateVersion = aggregateVersion
            EventType =
                match event with
                | ServerCreated _ -> "ServerCreated"
                | HostnameUpdated _ -> "HostnameUpdated"
                | EnvironmentSet _ -> "EnvironmentSet"
                | CriticalitySet _ -> "CriticalitySet"
                | RegionUpdated _ -> "RegionUpdated"
                | PlatformUpdated _ -> "PlatformUpdated"
                | OwningTeamSet _ -> "OwningTeamSet"
                | ServerTagsAdded _ -> "ServerTagsAdded"
                | ServerTagsRemoved _ -> "ServerTagsRemoved"
                | ServerDeleted _ -> "ServerDeleted"
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
    let private loadAggregateState (eventStore: IEventStore<ServerEvent>) (aggregateId: string) =
        let aggregateGuid = parseAggregateId aggregateId
        let existingEvents = eventStore.GetEvents aggregateGuid
        let baseVersion = eventStore.GetAggregateVersion aggregateGuid
        let stateFromEvents = existingEvents |> List.fold (fun (acc: ServerAggregate) e -> acc.ApplyEvent(e.Data)) ServerAggregate.Empty

        let state =
            if existingEvents |> List.isEmpty then
                match ServerRepository.getById aggregateId with
                | Some server ->
                    { ServerAggregate.Empty with
                        Id = Some server.Id
                        Hostname = Some server.Hostname
                        Environment = Some server.Environment
                        Region = server.Region
                        Platform = server.Platform
                        Criticality = Some server.Criticality
                        OwningTeam = server.OwningTeam
                        Tags = server.Tags
                        IsDeleted = false }
                | None -> ServerAggregate.Empty
            else stateFromEvents

        state, baseVersion, aggregateGuid

    /// Append events and project them; returns envelopes or error
    let private persistAndProject (eventStore: IEventStore<ServerEvent>) (projectionEngine: ProjectionEngine<ServerEvent>) (aggregateId: string) (aggregateGuid: Guid) (baseVersion: int) (meta: string * ActorType * Guid * Guid) (events: ServerEvent list) =
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
            // GET /servers - list (read from projection)
            GET >=> route "/servers" >=> fun next ctx -> task {
                let page = ctx.TryGetQueryStringValue "page" |> Option.bind (fun s -> try Some (int s) with _ -> None) |> Option.defaultValue 1
                let limit = ctx.TryGetQueryStringValue "limit" |> Option.bind (fun s -> try Some (int s) with _ -> None) |> Option.defaultValue 50
                let environment = ctx.TryGetQueryStringValue "environment" |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))
                let region = ctx.TryGetQueryStringValue "region" |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))

                let pageParam = if page < 1 then 1 else page
                let limitParam = if limit < 1 || limit > 200 then 50 else limit

                let result = ServerRepository.getAll pageParam limitParam environment region
                let json = Json.encodePaginatedResponse Json.encodeServer result
                return! (Giraffe.Core.json json) next ctx
            }

            // POST /servers - create server via CreateServer command
            POST >=> route "/servers" >=> fun next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateServerRequest bodyStr with
                | Ok req ->
                    try
                        // Generate unique server ID
                        let serverId = generateId()
                        
                        // Record command span event
                        let activity = Activity.Current
                        if activity <> null then
                            activity.SetTag("command.type", "CreateServer") |> ignore
                            activity.SetTag("entity.id", serverId) |> ignore
                            activity.SetTag("entity.type", "Server") |> ignore
                        
                        // Create command
                        let cmd : CreateServerData = {
                            Id = serverId
                            Hostname = req.Hostname
                            Environment = req.Environment
                            Region = req.Region
                            Platform = req.Platform
                            Criticality = req.Criticality
                            OwningTeam = req.OwningTeam
                            Tags = req.Tags |> Option.defaultValue []
                        }
                        
                        // Validate command and generate events
                        let state = ServerAggregate.Empty
                        match ServerCommandHandler.handleCreateServer state cmd with
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
                            
                            let eventStore = createServerEventStore()
                            let projectionEngine = createProjectionEngine eventStore
                            let aggregateGuid = parseAggregateId serverId
                            let baseVersion = eventStore.GetAggregateVersion aggregateGuid
                            let meta = getActorMetadata ctx

                            match persistAndProject eventStore projectionEngine serverId aggregateGuid baseVersion meta events with
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
                                match ServerRepository.getById serverId with
                                | Some server ->
                                    ctx.SetStatusCode 201
                                    ctx.SetHttpHeader ("Location", $"/servers/{server.Id}")
                                    let json = Json.encodeServer server
                                    return! (Giraffe.Core.json json) next ctx
                                | None ->
                                    let finalState = events |> List.fold (fun (s: ServerAggregate) evt -> s.ApplyEvent(evt)) state
                                    ctx.SetStatusCode 201
                                    ctx.SetHttpHeader ("Location", $"/servers/{serverId}")
                                    let responseJson = Encode.object [
                                        "id", Encode.string serverId
                                        "hostname", Encode.string (finalState.Hostname |> Option.defaultValue "")
                                        "environment", Encode.string (finalState.Environment |> Option.defaultValue "")
                                        "criticality", Encode.string (finalState.Criticality |> Option.defaultValue "")
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

            // GET /servers/{id}
            GET >=> routef "/servers/%s" (fun id next ctx -> task {
                match ServerRepository.getById id with
                | Some server ->
                    let json = Json.encodeServer server
                    return! (Giraffe.Core.json json) next ctx
                | None ->
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "Server not found"
                    return! (Giraffe.Core.json errJson) next ctx
            })

            // PATCH /servers/{id}
            PATCH >=> routef "/servers/%s" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateServerRequest bodyStr with
                | Ok req ->
                    try
                        let activity = Activity.Current
                        if activity <> null then
                            activity.SetTag("command.type", "UpdateServer") |> ignore
                            activity.SetTag("entity.id", id) |> ignore
                            activity.SetTag("entity.type", "Server") |> ignore
                        
                        let eventStore = createServerEventStore()
                        let projectionEngine = createProjectionEngine eventStore
                        let state, baseVersion, aggregateGuid = loadAggregateState eventStore id
                        
                        if state.Id.IsNone then
                            ctx.SetStatusCode 404
                            let errJson = Json.encodeErrorResponse "not_found" "Server not found"
                            return! (Giraffe.Core.json errJson) next ctx
                        else
                            // Build up all change commands
                            let results = 
                                [
                                    if state.Hostname <> Some req.Hostname then
                                        let cmd = { UpdateHostnameData.Id = id; NewHostname = req.Hostname }
                                        yield ServerCommandHandler.handleUpdateHostname state cmd
                                    
                                    if state.Environment <> Some req.Environment then
                                        let cmd = { SetEnvironmentData.Id = id; Environment = req.Environment }
                                        yield ServerCommandHandler.handleSetEnvironment state cmd
                                    
                                    if state.Criticality <> Some req.Criticality then
                                        let cmd = { SetServerCriticalityData.Id = id; Criticality = req.Criticality }
                                        yield ServerCommandHandler.handleSetCriticality state cmd
                                    
                                    if state.Region <> req.Region then
                                        let cmd = { UpdateRegionData.Id = id; Region = req.Region }
                                        yield ServerCommandHandler.handleUpdateRegion state cmd
                                    
                                    if state.Platform <> req.Platform then
                                        let cmd = { UpdatePlatformData.Id = id; Platform = req.Platform }
                                        yield ServerCommandHandler.handleUpdatePlatform state cmd
                                    
                                    if state.OwningTeam <> req.OwningTeam then
                                        let cmd = { SetOwningTeamData.Id = id; OwningTeam = req.OwningTeam }
                                        yield ServerCommandHandler.handleSetOwningTeam state cmd
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
                                    match ServerRepository.getById id with
                                    | Some server ->
                                        let json = Json.encodeServer server
                                        return! (Giraffe.Core.json json) next ctx
                                    | None ->
                                        ctx.SetStatusCode 404
                                        let errJson = Json.encodeErrorResponse "not_found" "Server not found"
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
                                        match ServerRepository.getById id with
                                        | Some server ->
                                            let json = Json.encodeServer server
                                            return! (Giraffe.Core.json json) next ctx
                                        | None ->
                                            ctx.SetStatusCode 404
                                            let errJson = Json.encodeErrorResponse "not_found" "Server not found"
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

            // DELETE /servers/{id}
            DELETE >=> routef "/servers/%s" (fun id next ctx -> task {
                try
                    let activity = Activity.Current
                    if activity <> null then
                        activity.SetTag("command.type", "DeleteServer") |> ignore
                        activity.SetTag("entity.id", id) |> ignore
                        activity.SetTag("entity.type", "Server") |> ignore
                    
                    let eventStore = createServerEventStore()
                    let projectionEngine = createProjectionEngine eventStore
                    let state, baseVersion, aggregateGuid = loadAggregateState eventStore id
                    
                    if state.Id.IsNone then
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "Server not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    else
                        let cmd = { DeleteServerData.Id = id }
                        match ServerCommandHandler.handleDeleteServer state cmd with
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
