/// Server API endpoints
namespace EATool.Api

open System
open Giraffe
open Thoth.Json.Net
open EATool.Domain
open EATool.Infrastructure

module ServersEndpoints =
    let routes: HttpHandler list =
        [
            // GET /servers - list
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

            // POST /servers - create
            POST >=> route "/servers" >=> fun next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateServerRequest bodyStr with
                | Ok req ->
                    if String.IsNullOrWhiteSpace(req.Hostname) then
                        ctx.SetStatusCode 400
                        let errJson = Json.encodeErrorResponse "validation_error" "Request validation failed"
                        return! (Giraffe.Core.json errJson) next ctx
                    else
                        let srv = ServerRepository.create req
                        ctx.SetStatusCode 201
                        ctx.SetHttpHeader ("Location", $"/servers/{srv.Id}")
                        let json = Json.encodeServer srv
                        return! (Giraffe.Core.json json) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errJson) next ctx
            }

            // GET /servers/{id}
            GET >=> routef "/servers/%s" (fun id next ctx -> task {
                match ServerRepository.getById id with
                | Some srv ->
                    let json = Json.encodeServer srv
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
                    if String.IsNullOrWhiteSpace(req.Hostname) then
                        ctx.SetStatusCode 400
                        let errJson = Json.encodeErrorResponse "validation_error" "Request validation failed"
                        return! (Giraffe.Core.json errJson) next ctx
                    else
                        match ServerRepository.update id req with
                        | Some srv ->
                            let json = Json.encodeServer srv
                            return! (Giraffe.Core.json json) next ctx
                        | None ->
                            ctx.SetStatusCode 404
                            let errJson = Json.encodeErrorResponse "not_found" "Server not found"
                            return! (Giraffe.Core.json errJson) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errJson) next ctx
            })

            // DELETE /servers/{id}
            DELETE >=> routef "/servers/%s" (fun id next ctx -> task {
                if ServerRepository.delete id then
                    ctx.SetStatusCode 204
                    return! next ctx
                else
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "Server not found"
                    return! (Giraffe.Core.json errJson) next ctx
            })
        ]
