/// Business capability API endpoints
namespace EATool.Api

open System
open Giraffe
open Thoth.Json.Net
open EATool.Domain
open EATool.Infrastructure

module BusinessCapabilitiesEndpoints =
    let routes: HttpHandler list =
        [
            // GET /business-capabilities - list
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

            // POST /business-capabilities - create
            POST >=> route "/business-capabilities" >=> fun next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateBusinessCapabilityRequest bodyStr with
                | Ok req ->
                    if String.IsNullOrWhiteSpace(req.Name) then
                        ctx.SetStatusCode 400
                        let errJson = Json.encodeErrorResponse "validation_error" "Request validation failed"
                        return! (Giraffe.Core.json errJson) next ctx
                    else
                        let cap = BusinessCapabilityRepository.create req
                        ctx.SetStatusCode 201
                        ctx.SetHttpHeader ("Location", $"/business-capabilities/{cap.Id}")
                        let json = Json.encodeBusinessCapability cap
                        return! (Giraffe.Core.json json) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errJson) next ctx
            }

            // GET /business-capabilities/{id}
            GET >=> routef "/business-capabilities/%s" (fun id next ctx -> task {
                match BusinessCapabilityRepository.getById id with
                | Some cap ->
                    let json = Json.encodeBusinessCapability cap
                    return! (Giraffe.Core.json json) next ctx
                | None ->
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "Business capability not found"
                    return! (Giraffe.Core.json errJson) next ctx
            })

            // PATCH /business-capabilities/{id}
            PATCH >=> routef "/business-capabilities/%s" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateBusinessCapabilityRequest bodyStr with
                | Ok req ->
                    if String.IsNullOrWhiteSpace(req.Name) then
                        ctx.SetStatusCode 400
                        let errJson = Json.encodeErrorResponse "validation_error" "Request validation failed"
                        return! (Giraffe.Core.json errJson) next ctx
                    else
                        match BusinessCapabilityRepository.update id req with
                        | Some cap ->
                            let json = Json.encodeBusinessCapability cap
                            return! (Giraffe.Core.json json) next ctx
                        | None ->
                            ctx.SetStatusCode 404
                            let errJson = Json.encodeErrorResponse "not_found" "Business capability not found"
                            return! (Giraffe.Core.json errJson) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errJson) next ctx
            })

            // DELETE /business-capabilities/{id}
            DELETE >=> routef "/business-capabilities/%s" (fun id next ctx -> task {
                if BusinessCapabilityRepository.delete id then
                    ctx.SetStatusCode 204
                    return! next ctx
                else
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "Business capability not found"
                    return! (Giraffe.Core.json errJson) next ctx
            })
        ]
