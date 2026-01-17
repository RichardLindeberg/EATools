/// CQRS endpoints for ApplicationInterface aggregate
namespace EATool.Api

open System
open Giraffe
open Thoth.Json.Net
open EATool.Domain
open EATool.Infrastructure
open EATool.Infrastructure.EventStore
open EATool.Infrastructure.EventJson
open EATool.Infrastructure.ApplicationInterfaceEventJson
open EATool.Infrastructure.ProjectionEngine
open EATool.Infrastructure.Projections

module ApplicationInterfacesEndpoints =
    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "aif-" + guid.Substring(0, 8)

    let private createEventStore () =
        let connectionString = Database.getConnectionString()
        createSqlEventStore(connectionString, encodeApplicationInterfaceEvent, decodeApplicationInterfaceEvent)

    let private createProjectionEngine (eventStore: IEventStore<ApplicationInterfaceEvent>) =
        let connectionString = Database.getConnectionString()
        let handlers: IProjectionHandler<ApplicationInterfaceEvent> list = [
            Projections.ApplicationInterfaceProjection.Handler(connectionString) :> IProjectionHandler<ApplicationInterfaceEvent>
        ]
        ProjectionEngine<ApplicationInterfaceEvent>(connectionString, eventStore, handlers)

    let private parseAggregateId (aggregateId: string) : Guid =
        let guidPart = if aggregateId.StartsWith("aif-") then aggregateId.Substring(4) else aggregateId
        Guid.Parse(guidPart.PadRight(32, '0'))

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

    let private statusFromString (value: string) : InterfaceStatus option =
        match value.ToLowerInvariant() with
        | "active" -> Some InterfaceStatus.Active
        | "deprecated" -> Some InterfaceStatus.Deprecated
        | "retired" -> Some InterfaceStatus.Retired
        | _ -> None

    let private createEventEnvelope (aggregateId: string) (aggregateGuid: Guid) (aggregateVersion: int) (event: ApplicationInterfaceEvent) (actor: string, actorType: ActorType, correlationId: Guid, causationId: Guid) : EventEnvelope<ApplicationInterfaceEvent> =
        {
            EventId = Guid.NewGuid()
            AggregateId = aggregateGuid
            AggregateType = "ApplicationInterface"
            AggregateVersion = aggregateVersion
            EventType =
                match event with
                | ApplicationInterfaceCreated _ -> "ApplicationInterfaceCreated"
                | ApplicationInterfaceUpdated _ -> "ApplicationInterfaceUpdated"
                | ServedServicesSet _ -> "ServedServicesSet"
                | StatusChanged _ -> "StatusChanged"
                | ApplicationInterfaceDeleted _ -> "ApplicationInterfaceDeleted"
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

    let private loadAggregateState (eventStore: IEventStore<ApplicationInterfaceEvent>) (aggregateId: string) =
        let aggregateGuid = parseAggregateId aggregateId
        let existingEvents = eventStore.GetEvents aggregateGuid
        let baseVersion = eventStore.GetAggregateVersion aggregateGuid
        let stateFromEvents = existingEvents |> List.fold (fun acc e -> ApplicationInterfaceAggregate.apply acc e.Data) ApplicationInterfaceAggregate.Initial
        let state =
            if existingEvents |> List.isEmpty then
                match ApplicationInterfaceRepository.getById aggregateId with
                | Some iface ->
                    { ApplicationInterfaceAggregate.Initial with
                        Id = Some iface.Id
                        Name = Some iface.Name
                        Protocol = Some iface.Protocol
                        Endpoint = iface.Endpoint
                        SpecificationUrl = iface.SpecificationUrl
                        Version = iface.Version
                        AuthenticationMethod = iface.AuthenticationMethod
                        ExposedByAppId = Some iface.ExposedByAppId
                        ServesServiceIds = iface.ServesServiceIds
                        RateLimits = iface.RateLimits
                        Status = Some iface.Status
                        Tags = iface.Tags
                        IsDeleted = false }
                | None -> ApplicationInterfaceAggregate.Initial
            else stateFromEvents
        state, baseVersion, aggregateGuid

    let private persistAndProject (eventStore: IEventStore<ApplicationInterfaceEvent>) (projectionEngine: ProjectionEngine<ApplicationInterfaceEvent>) (aggregateId: string) (aggregateGuid: Guid) (baseVersion: int) (meta: string * ActorType * Guid * Guid) (events: ApplicationInterfaceEvent list) =
        let envelopes = events |> List.mapi (fun i evt -> createEventEnvelope aggregateId aggregateGuid (baseVersion + i + 1) evt meta)
        match eventStore.Append envelopes with
        | Error err -> Error err
        | Ok () ->
            envelopes
            |> List.fold (fun acc env -> match acc with | Error _ -> acc | Ok () -> projectionEngine.ProcessEvent env) (Ok ())

    let routes: HttpHandler list =
        [
            // GET /application-interfaces
            GET >=> route "/application-interfaces" >=> fun next ctx -> task {
                let page = ctx.TryGetQueryStringValue "page" |> Option.bind (fun s -> match Int32.TryParse s with | true, v -> Some v | _ -> None) |> Option.defaultValue 1
                let limit = ctx.TryGetQueryStringValue "limit" |> Option.bind (fun s -> match Int32.TryParse s with | true, v -> Some v | _ -> None) |> Option.defaultValue 50
                let search = ctx.TryGetQueryStringValue "search" |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))
                let appId = ctx.TryGetQueryStringValue "application_id" |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))
                let status = ctx.TryGetQueryStringValue "status" |> Option.bind statusFromString
                let pageParam = if page < 1 then 1 else page
                let limitParam = if limit < 1 || limit > 200 then 50 else limit
                let result = ApplicationInterfaceRepository.getAll pageParam limitParam appId status
                let json = Json.encodePaginatedResponse Json.encodeApplicationInterface result
                return! (Giraffe.Core.json json) next ctx
            }

            // GET /application-interfaces/{id}
            GET >=> routef "/application-interfaces/%s" (fun id next ctx -> task {
                match ApplicationInterfaceRepository.getById id with
                | Some iface ->
                    let json = Json.encodeApplicationInterface iface
                    return! (Giraffe.Core.json json) next ctx
                | None ->
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "ApplicationInterface not found"
                    return! (Giraffe.Core.json errJson) next ctx
            })

            // POST /application-interfaces (create)
            POST >=> route "/application-interfaces" >=> fun next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateApplicationInterfaceRequest bodyStr with
                | Error err ->
                    ctx.SetStatusCode 400
                    let errJson = Json.encodeErrorResponse "validation_error" ($"JSON parse error: {err}")
                    return! (Giraffe.Core.json errJson) next ctx
                | Ok req ->
                    let ifaceId = generateId ()
                    let cmd : CreateApplicationInterfaceData = {
                        Id = ifaceId
                        Name = req.Name
                        Protocol = req.Protocol
                        Endpoint = req.Endpoint
                        SpecificationUrl = req.SpecificationUrl
                        Version = req.Version
                        AuthenticationMethod = req.AuthenticationMethod
                        ExposedByAppId = req.ExposedByAppId
                        ServesServiceIds = req.ServesServiceIds |> Option.defaultValue []
                        RateLimits = None
                        Status = req.Status
                        Tags = req.Tags |> Option.defaultValue []
                    }
                    let state = ApplicationInterfaceAggregate.Initial
                    match ApplicationInterfaceCommandHandler.handleCreate state cmd with
                    | Error err ->
                        ctx.SetStatusCode 400
                        let errJson = Json.encodeErrorResponse "validation_error" err
                        return! (Giraffe.Core.json errJson) next ctx
                    | Ok events ->
                        let eventStore = createEventStore ()
                        let projectionEngine = createProjectionEngine eventStore
                        let aggregateGuid = parseAggregateId ifaceId
                        let baseVersion = eventStore.GetAggregateVersion aggregateGuid
                        let meta = getActorMetadata ctx
                        match persistAndProject eventStore projectionEngine ifaceId aggregateGuid baseVersion meta events with
                        | Error err ->
                            ctx.SetStatusCode 500
                            let errJson = Json.encodeErrorResponse "event_store_error" err
                            return! (Giraffe.Core.json errJson) next ctx
                        | Ok _ ->
                            match ApplicationInterfaceRepository.getById ifaceId with
                            | Some iface ->
                                ctx.SetStatusCode 201
                                ctx.SetHttpHeader("Location", $"/application-interfaces/{iface.Id}")
                                let json = Json.encodeApplicationInterface iface
                                return! (Giraffe.Core.json json) next ctx
                            | None ->
                                ctx.SetStatusCode 201
                                ctx.SetHttpHeader("Location", $"/application-interfaces/{ifaceId}")
                                let json = Encode.object [ "id", Encode.string ifaceId; "name", Encode.string cmd.Name ]
                                return! (Giraffe.Core.json json) next ctx
            }

            // POST /application-interfaces/{id}/commands/update
            POST >=> routef "/application-interfaces/%s/commands/update" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                let decoder: Decoder<UpdateApplicationInterfaceData> =
                    Decode.object (fun get ->
                        ({
                            Id = id
                            Name = get.Optional.Field "name" Decode.string
                            Protocol = get.Optional.Field "protocol" Decode.string
                            Endpoint = get.Optional.Field "endpoint" Decode.string
                            Version = get.Optional.Field "version" Decode.string
                            AuthenticationMethod = get.Optional.Field "authentication_method" Decode.string
                            Tags = get.Optional.Field "tags" (Decode.list Decode.string)
                        } : UpdateApplicationInterfaceData))
                match Decode.fromString decoder bodyStr with
                | Error err ->
                    ctx.SetStatusCode 400
                    let errJson = Json.encodeErrorResponse "validation_error" ($"JSON parse error: {err}")
                    return! (Giraffe.Core.json errJson) next ctx
                | Ok cmd ->
                    let eventStore = createEventStore ()
                    let projectionEngine = createProjectionEngine eventStore
                    let state, baseVersion, aggregateGuid = loadAggregateState eventStore id
                    match state.Id with
                    | None ->
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "ApplicationInterface not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    | Some _ ->
                        match ApplicationInterfaceCommandHandler.handleUpdate state cmd with
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
                                match ApplicationInterfaceRepository.getById id with
                                | Some iface ->
                                    let json = Json.encodeApplicationInterface iface
                                    return! (Giraffe.Core.json json) next ctx
                                | None ->
                                    ctx.SetStatusCode 404
                                    let errJson = Json.encodeErrorResponse "not_found" "ApplicationInterface not found"
                                    return! (Giraffe.Core.json errJson) next ctx
            })

            // POST /application-interfaces/{id}/commands/set-service
            POST >=> routef "/application-interfaces/%s/commands/set-service" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                let decoder: Decoder<SetServedServicesData> =
                    Decode.object (fun get -> ({ Id = id; ServiceIds = get.Optional.Field "service_ids" (Decode.list Decode.string) |> Option.defaultValue [] } : SetServedServicesData))
                match Decode.fromString decoder bodyStr with
                | Error err ->
                    ctx.SetStatusCode 400
                    let errJson = Json.encodeErrorResponse "validation_error" ($"JSON parse error: {err}")
                    return! (Giraffe.Core.json errJson) next ctx
                | Ok cmd ->
                    let eventStore = createEventStore ()
                    let projectionEngine = createProjectionEngine eventStore
                    let state, baseVersion, aggregateGuid = loadAggregateState eventStore id
                    match state.Id with
                    | None ->
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "ApplicationInterface not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    | Some _ ->
                        match ApplicationInterfaceCommandHandler.handleSetServedServices state cmd with
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
                                match ApplicationInterfaceRepository.getById id with
                                | Some iface ->
                                    let json = Json.encodeApplicationInterface iface
                                    return! (Giraffe.Core.json json) next ctx
                                | None ->
                                    ctx.SetStatusCode 404
                                    let errJson = Json.encodeErrorResponse "not_found" "ApplicationInterface not found"
                                    return! (Giraffe.Core.json errJson) next ctx
            })

            // POST /application-interfaces/{id}/commands/deprecate
            POST >=> routef "/application-interfaces/%s/commands/deprecate" (fun id next ctx -> task {
                let eventStore = createEventStore ()
                let projectionEngine = createProjectionEngine eventStore
                let state, baseVersion, aggregateGuid = loadAggregateState eventStore id
                match state.Id with
                | None ->
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "ApplicationInterface not found"
                    return! (Giraffe.Core.json errJson) next ctx
                | Some _ ->
                    let cmd : SetInterfaceStatusData = { Id = id; Status = InterfaceStatus.Deprecated }
                    match ApplicationInterfaceCommandHandler.handleSetStatus state cmd with
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
                            match ApplicationInterfaceRepository.getById id with
                            | Some iface ->
                                let json = Json.encodeApplicationInterface iface
                                return! (Giraffe.Core.json json) next ctx
                            | None ->
                                ctx.SetStatusCode 404
                                let errJson = Json.encodeErrorResponse "not_found" "ApplicationInterface not found"
                                return! (Giraffe.Core.json errJson) next ctx
            })

            // POST /application-interfaces/{id}/commands/retire
            POST >=> routef "/application-interfaces/%s/commands/retire" (fun id next ctx -> task {
                let eventStore = createEventStore ()
                let projectionEngine = createProjectionEngine eventStore
                let state, baseVersion, aggregateGuid = loadAggregateState eventStore id
                match state.Id with
                | None ->
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "ApplicationInterface not found"
                    return! (Giraffe.Core.json errJson) next ctx
                | Some _ ->
                    let cmd : SetInterfaceStatusData = { Id = id; Status = InterfaceStatus.Retired }
                    match ApplicationInterfaceCommandHandler.handleSetStatus state cmd with
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
                            match ApplicationInterfaceRepository.getById id with
                            | Some iface ->
                                let json = Json.encodeApplicationInterface iface
                                return! (Giraffe.Core.json json) next ctx
                            | None ->
                                ctx.SetStatusCode 404
                                let errJson = Json.encodeErrorResponse "not_found" "ApplicationInterface not found"
                                return! (Giraffe.Core.json errJson) next ctx
            })

            // POST /application-interfaces/{id}/commands/delete
            POST >=> routef "/application-interfaces/%s/commands/delete" (fun id next ctx -> task {
                let eventStore = createEventStore ()
                let projectionEngine = createProjectionEngine eventStore
                let state, baseVersion, aggregateGuid = loadAggregateState eventStore id
                match state.Id with
                | None ->
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "ApplicationInterface not found"
                    return! (Giraffe.Core.json errJson) next ctx
                | Some _ ->
                    let cmd : DeleteApplicationInterfaceData = { Id = id }
                    match ApplicationInterfaceCommandHandler.handleDelete state cmd with
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
                            ctx.SetStatusCode 200
                            let json = Encode.object [ "id", Encode.string id; "status", Encode.string "deleted" ]
                            return! (Giraffe.Core.json json) next ctx
            })
        ]
