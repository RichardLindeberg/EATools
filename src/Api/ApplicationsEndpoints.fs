/// Application API endpoints
namespace EATool.Api

open System
open Giraffe
open Thoth.Json.Net
open EATool.Domain
open EATool.Infrastructure

module ApplicationsEndpoints =
    let routes: HttpHandler list =
        [
            // GET /applications - list
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

            // POST /applications - create
            POST >=> route "/applications" >=> fun next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateApplicationRequest bodyStr with
                | Ok req ->
                    if String.IsNullOrWhiteSpace(req.Name) then
                        ctx.SetStatusCode 400
                        let errJson = Json.encodeErrorResponse "validation_error" "Request validation failed"
                        return! (Giraffe.Core.json errJson) next ctx
                    else
                        let app = ApplicationRepository.create req
                        ctx.SetStatusCode 201
                        ctx.SetHttpHeader ("Location", $"/applications/{app.Id}")
                        let json = Json.encodeApplication app
                        return! (Giraffe.Core.json json) next ctx
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

            // PATCH /applications/{id}
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

            // DELETE /applications/{id}
            DELETE >=> routef "/applications/%s" (fun id next ctx -> task {
                if ApplicationRepository.delete id then
                    ctx.SetStatusCode 204
                    return! next ctx
                else
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "Application not found"
                    return! (Giraffe.Core.json errJson) next ctx
            })
        ]
