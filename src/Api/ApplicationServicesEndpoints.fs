/// CQRS endpoints for ApplicationService aggregate
namespace EATool.Api

open System
open Giraffe
open Thoth.Json.Net
open EATool.Domain
open EATool.Infrastructure
open EATool.Infrastructure.EventStore
open EATool.Infrastructure.EventJson
open EATool.Infrastructure.ApplicationServiceEventJson
open EATool.Infrastructure.ProjectionEngine
open EATool.Infrastructure.Projections

module ApplicationServicesEndpoints =
    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "aps-" + guid.Substring(0, 8)

    let private createEventStore () =
        let connectionString = Database.getConnectionString()
        createSqlEventStore(connectionString, encodeApplicationServiceEvent, decodeApplicationServiceEvent)

    let private createProjectionEngine (eventStore: IEventStore<ApplicationServiceEvent>) =
        let connectionString = Database.getConnectionString()
        let handlers: IProjectionHandler<ApplicationServiceEvent> list = [
            Projections.ApplicationServiceProjection.Handler(connectionString) :> IProjectionHandler<ApplicationServiceEvent>
        ]
        ProjectionEngine<ApplicationServiceEvent>(connectionString, eventStore, handlers)

    let private parseAggregateId (aggregateId: string) : Guid =
        let guidPart = if aggregateId.StartsWith("aps-") then aggregateId.Substring(4) else aggregateId
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

    let private createEventEnvelope (aggregateId: string) (aggregateGuid: Guid) (aggregateVersion: int) (event: ApplicationServiceEvent) (actor: string, actorType: ActorType, correlationId: Guid, causationId: Guid) : EventEnvelope<ApplicationServiceEvent> =
        {
            EventId = Guid.NewGuid()
            AggregateId = aggregateGuid
            AggregateType = "ApplicationService"
            AggregateVersion = aggregateVersion
            EventType =
                match event with
                | ApplicationServiceCreated _ -> "ApplicationServiceCreated"
                | ApplicationServiceUpdated _ -> "ApplicationServiceUpdated"
                | BusinessCapabilitySet _ -> "BusinessCapabilitySet"
                | ConsumerAdded _ -> "ConsumerAdded"
                | ConsumerRemoved _ -> "ConsumerRemoved"
                | ApplicationServiceDeleted _ -> "ApplicationServiceDeleted"
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

    let private loadAggregateState (eventStore: IEventStore<ApplicationServiceEvent>) (aggregateId: string) =
        let aggregateGuid = parseAggregateId aggregateId
        let existingEvents = eventStore.GetEvents aggregateGuid
        let baseVersion = eventStore.GetAggregateVersion aggregateGuid
        let stateFromEvents = existingEvents |> List.fold (fun acc e -> ApplicationServiceAggregate.apply acc e.Data) ApplicationServiceAggregate.Initial

        let state =
            if existingEvents |> List.isEmpty then
                match ApplicationServiceRepository.getById aggregateId with
                | Some svc ->
                    { ApplicationServiceAggregate.Initial with
                        Id = Some svc.Id
                        Name = Some svc.Name
                        Description = svc.Description
                        BusinessCapabilityId = svc.BusinessCapabilityId
                        Sla = svc.Sla
                        ExposedByAppIds = svc.ExposedByAppIds
                        Consumers = svc.Consumers
                        Tags = svc.Tags
                        IsDeleted = false }
                | None -> ApplicationServiceAggregate.Initial
            else stateFromEvents
        state, baseVersion, aggregateGuid

    let private persistAndProject (eventStore: IEventStore<ApplicationServiceEvent>) (projectionEngine: ProjectionEngine<ApplicationServiceEvent>) (aggregateId: string) (aggregateGuid: Guid) (baseVersion: int) (meta: string * ActorType * Guid * Guid) (events: ApplicationServiceEvent list) =
        let envelopes =
            events
            |> List.mapi (fun i event -> createEventEnvelope aggregateId aggregateGuid (baseVersion + i + 1) event meta)

        match eventStore.Append envelopes with
        | Error err -> Error err
        | Ok () ->
            envelopes
            |> List.fold (fun acc env ->
                match acc with
                | Error _ -> acc
                | Ok () -> projectionEngine.ProcessEvent env) (Ok ())

    let routes: HttpHandler list =
        [
            // GET /application-services
            GET >=> route "/application-services" >=> fun next ctx -> task {
                let page = ctx.TryGetQueryStringValue "page" |> Option.bind (fun s -> match Int32.TryParse s with | true, v -> Some v | _ -> None) |> Option.defaultValue 1
                let limit = ctx.TryGetQueryStringValue "limit" |> Option.bind (fun s -> match Int32.TryParse s with | true, v -> Some v | _ -> None) |> Option.defaultValue 50
                let bcId = ctx.TryGetQueryStringValue "business_capability_id" |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))
                let pageParam = if page < 1 then 1 else page
                let limitParam = if limit < 1 || limit > 200 then 50 else limit
                let result = ApplicationServiceRepository.getAll pageParam limitParam bcId
                let json = Json.encodePaginatedResponse Json.encodeApplicationService result
                return! (Giraffe.Core.json json) next ctx
            }

            // GET /application-services/{id}
            GET >=> routef "/application-services/%s" (fun id next ctx -> task {
                match ApplicationServiceRepository.getById id with
                | Some svc ->
                    let json = Json.encodeApplicationService svc
                    return! (Giraffe.Core.json json) next ctx
                | None ->
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "ApplicationService not found"
                    return! (Giraffe.Core.json errJson) next ctx
            })

            // POST /application-services (create)
            POST >=> route "/application-services" >=> fun next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateApplicationServiceRequest bodyStr with
                | Error err ->
                    ctx.SetStatusCode 400
                    let errJson = Json.encodeErrorResponse "validation_error" ($"JSON parse error: {err}")
                    return! (Giraffe.Core.json errJson) next ctx
                | Ok req ->
                    let svcId = generateId ()
                    let cmd : CreateApplicationServiceData = {
                        Id = svcId
                        Name = req.Name
                        Description = req.Description
                        BusinessCapabilityId = req.BusinessCapabilityId
                        Sla = req.Sla
                        ExposedByAppIds = req.ExposedByAppIds |> Option.defaultValue []
                        Consumers = []
                        Tags = req.Tags |> Option.defaultValue []
                    }
                    let state = ApplicationServiceAggregate.Initial
                    match ApplicationServiceCommandHandler.handleCreate state cmd with
                    | Error err ->
                        ctx.SetStatusCode 400
                        let errJson = Json.encodeErrorResponse "validation_error" err
                        return! (Giraffe.Core.json errJson) next ctx
                    | Ok events ->
                        let eventStore = createEventStore ()
                        let projectionEngine = createProjectionEngine eventStore
                        let aggregateGuid = parseAggregateId svcId
                        let baseVersion = eventStore.GetAggregateVersion aggregateGuid
                        let meta = getActorMetadata ctx
                        match persistAndProject eventStore projectionEngine svcId aggregateGuid baseVersion meta events with
                        | Error err ->
                            ctx.SetStatusCode 500
                            let errJson = Json.encodeErrorResponse "event_store_error" err
                            return! (Giraffe.Core.json errJson) next ctx
                        | Ok _ ->
                            match ApplicationServiceRepository.getById svcId with
                            | Some created ->
                                ctx.SetStatusCode 201
                                ctx.SetHttpHeader("Location", $"/application-services/{created.Id}")
                                let json = Json.encodeApplicationService created
                                return! (Giraffe.Core.json json) next ctx
                            | None ->
                                ctx.SetStatusCode 201
                                ctx.SetHttpHeader("Location", $"/application-services/{svcId}")
                                let json = Encode.object [ "id", Encode.string svcId; "name", Encode.string cmd.Name ]
                                return! (Giraffe.Core.json json) next ctx
            }

            // POST /application-services/{id}/commands/update
            POST >=> routef "/application-services/%s/commands/update" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                let decoder: Decoder<UpdateApplicationServiceData> =
                    Decode.object (fun get ->
                        ({
                            Id = id
                            Name = get.Optional.Field "name" Decode.string
                            Description = get.Optional.Field "description" Decode.string
                            Sla = get.Optional.Field "sla" Decode.string
                            Tags = get.Optional.Field "tags" (Decode.list Decode.string)
                        } : UpdateApplicationServiceData))
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
                        let errJson = Json.encodeErrorResponse "not_found" "ApplicationService not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    | Some _ ->
                        match ApplicationServiceCommandHandler.handleUpdate state cmd with
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
                                match ApplicationServiceRepository.getById id with
                                | Some svc ->
                                    let json = Json.encodeApplicationService svc
                                    return! (Giraffe.Core.json json) next ctx
                                | None ->
                                    ctx.SetStatusCode 404
                                    let errJson = Json.encodeErrorResponse "not_found" "ApplicationService not found"
                                    return! (Giraffe.Core.json errJson) next ctx
            })

            // POST /application-services/{id}/commands/set-business-capability
            POST >=> routef "/application-services/%s/commands/set-business-capability" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                let decoder: Decoder<SetBusinessCapabilityData> =
                    Decode.object (fun get -> ({ Id = id; BusinessCapabilityId = get.Optional.Field "business_capability_id" Decode.string } : SetBusinessCapabilityData))
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
                        let errJson = Json.encodeErrorResponse "not_found" "ApplicationService not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    | Some _ ->
                        match ApplicationServiceCommandHandler.handleSetBusinessCapability state cmd with
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
                                match ApplicationServiceRepository.getById id with
                                | Some svc ->
                                    let json = Json.encodeApplicationService svc
                                    return! (Giraffe.Core.json json) next ctx
                                | None ->
                                    ctx.SetStatusCode 404
                                    let errJson = Json.encodeErrorResponse "not_found" "ApplicationService not found"
                                    return! (Giraffe.Core.json errJson) next ctx
            })

            // POST /application-services/{id}/commands/add-consumer
            POST >=> routef "/application-services/%s/commands/add-consumer" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                let decoder: Decoder<AddConsumerData> =
                    Decode.object (fun get -> ({ Id = id; ConsumerAppId = get.Required.Field "app_id" Decode.string } : AddConsumerData))
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
                        let errJson = Json.encodeErrorResponse "not_found" "ApplicationService not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    | Some _ ->
                        match ApplicationServiceCommandHandler.handleAddConsumer state cmd with
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
                                match ApplicationServiceRepository.getById id with
                                | Some svc ->
                                    let json = Json.encodeApplicationService svc
                                    return! (Giraffe.Core.json json) next ctx
                                | None ->
                                    ctx.SetStatusCode 404
                                    let errJson = Json.encodeErrorResponse "not_found" "ApplicationService not found"
                                    return! (Giraffe.Core.json errJson) next ctx
            })

            // POST /application-services/{id}/commands/delete
            POST >=> routef "/application-services/%s/commands/delete" (fun id next ctx -> task {
                let eventStore = createEventStore ()
                let projectionEngine = createProjectionEngine eventStore
                let state, baseVersion, aggregateGuid = loadAggregateState eventStore id
                match state.Id with
                | None ->
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "ApplicationService not found"
                    return! (Giraffe.Core.json errJson) next ctx
                | Some _ ->
                    let cmd : DeleteApplicationServiceData = { Id = id }
                    match ApplicationServiceCommandHandler.handleDelete state cmd with
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
