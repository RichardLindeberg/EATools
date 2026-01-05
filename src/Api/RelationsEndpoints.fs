/// Relation API endpoints
namespace EATool.Api

open System
open Giraffe
open Thoth.Json.Net
open EATool.Domain
open EATool.Infrastructure

module RelationsEndpoints =
    let private isAllowedCombination (sourceType: EntityType) (targetType: EntityType) (relationType: RelationType) : bool =
        match sourceType, targetType, relationType with
        | EntityType.Application, EntityType.Application, (RelationType.DependsOn | RelationType.CommunicatesWith | RelationType.Calls | RelationType.PublishesEventTo | RelationType.ConsumesEventFrom) -> true
        | EntityType.Application, EntityType.ApplicationService, (RelationType.Realizes | RelationType.Uses) -> true
        | EntityType.Application, EntityType.ApplicationInterface, RelationType.Exposes -> true
        | EntityType.Application, EntityType.Server, (RelationType.DeployedOn | RelationType.StoresDataOn) -> true
        | EntityType.Application, EntityType.DataEntity, (RelationType.Reads | RelationType.Writes) -> true
        | EntityType.Application, EntityType.BusinessCapability, RelationType.Supports -> true
        | EntityType.ApplicationService, EntityType.BusinessCapability, (RelationType.Realizes | RelationType.Supports) -> true
        | EntityType.ApplicationInterface, EntityType.ApplicationService, RelationType.Serves -> true
        | EntityType.Integration, EntityType.Application, (RelationType.CommunicatesWith | RelationType.PublishesEventTo | RelationType.ConsumesEventFrom) -> true
        | EntityType.Integration, EntityType.ApplicationInterface, (RelationType.Calls | RelationType.Uses) -> true
        | EntityType.Server, EntityType.Server, RelationType.ConnectedTo -> true
        | EntityType.Organization, EntityType.Application, RelationType.Owns -> true
        | EntityType.Organization, EntityType.Server, RelationType.Owns -> true
        | EntityType.BusinessCapability, EntityType.Application, (RelationType.Realizes | RelationType.Implements | RelationType.Serves) -> true
        | _ -> false

    let private validateRelation (req: CreateRelationRequest) : bool =
        not (String.IsNullOrWhiteSpace req.SourceId)
        && not (String.IsNullOrWhiteSpace req.TargetId)
        && isAllowedCombination req.SourceType req.TargetType req.RelationType

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
            // GET /relations - list
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

            // POST /relations - create
            POST >=> route "/relations" >=> fun next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateRelationRequest bodyStr with
                | Ok req ->
                    if not (validateRelation req) then
                        ctx.SetStatusCode 422
                        let errJson = Json.encodeErrorResponse "validation_error" "Invalid source/target/relation combination"
                        return! (Giraffe.Core.json errJson) next ctx
                    else
                        let rel = RelationRepository.create req
                        ctx.SetStatusCode 201
                        ctx.SetHttpHeader ("Location", $"/relations/{rel.Id}")
                        let json = Json.encodeRelation rel
                        return! (Giraffe.Core.json json) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errJson) next ctx
            }

            // GET /relations/{id}
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

            // PATCH /relations/{id}
            PATCH >=> routef "/relations/%s" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateRelationRequest bodyStr with
                | Ok req ->
                    if not (validateRelation req) then
                        ctx.SetStatusCode 422
                        let errJson = Json.encodeErrorResponse "validation_error" "Invalid source/target/relation combination"
                        return! (Giraffe.Core.json errJson) next ctx
                    else
                        match RelationRepository.update id req with
                        | Some rel ->
                            let json = Json.encodeRelation rel
                            return! (Giraffe.Core.json json) next ctx
                        | None ->
                            ctx.SetStatusCode 404
                            let errJson = Json.encodeErrorResponse "not_found" "Relation not found"
                            return! (Giraffe.Core.json errJson) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errJson) next ctx
            })

            // DELETE /relations/{id}
            DELETE >=> routef "/relations/%s" (fun id next ctx -> task {
                if RelationRepository.delete id then
                    ctx.SetStatusCode 204
                    return! next ctx
                else
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "Relation not found"
                    return! (Giraffe.Core.json errJson) next ctx
            })
        ]
