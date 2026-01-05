/// Data entity API endpoints
namespace EATool.Api

open System
open Giraffe
open Thoth.Json.Net
open EATool.Domain
open EATool.Infrastructure

module DataEntitiesEndpoints =
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
            // GET /data-entities - list
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

            // POST /data-entities - create
            POST >=> route "/data-entities" >=> fun next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateDataEntityRequest bodyStr with
                | Ok req ->
                    if String.IsNullOrWhiteSpace(req.Name) then
                        ctx.SetStatusCode 400
                        let errJson = Json.encodeErrorResponse "validation_error" "Request validation failed"
                        return! (Giraffe.Core.json errJson) next ctx
                    else
                        let entity = DataEntityRepository.create req
                        ctx.SetStatusCode 201
                        ctx.SetHttpHeader ("Location", $"/data-entities/{entity.Id}")
                        let json = Json.encodeDataEntity entity
                        return! (Giraffe.Core.json json) next ctx
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
                    if String.IsNullOrWhiteSpace(req.Name) then
                        ctx.SetStatusCode 400
                        let errJson = Json.encodeErrorResponse "validation_error" "Request validation failed"
                        return! (Giraffe.Core.json errJson) next ctx
                    else
                        match DataEntityRepository.update id req with
                        | Some entity ->
                            let json = Json.encodeDataEntity entity
                            return! (Giraffe.Core.json json) next ctx
                        | None ->
                            ctx.SetStatusCode 404
                            let errJson = Json.encodeErrorResponse "not_found" "Data entity not found"
                            return! (Giraffe.Core.json errJson) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errJson) next ctx
            })

            // DELETE /data-entities/{id}
            DELETE >=> routef "/data-entities/%s" (fun id next ctx -> task {
                if DataEntityRepository.delete id then
                    ctx.SetStatusCode 204
                    return! next ctx
                else
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "Data entity not found"
                    return! (Giraffe.Core.json errJson) next ctx
            })
        ]
