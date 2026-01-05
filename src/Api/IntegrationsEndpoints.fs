/// Integration API endpoints
namespace EATool.Api

open System
open Giraffe
open Thoth.Json.Net
open EATool.Domain
open EATool.Infrastructure

module IntegrationsEndpoints =
    let routes: HttpHandler list =
        [
            // GET /integrations - list
            GET >=> route "/integrations" >=> fun next ctx -> task {
                let page = ctx.TryGetQueryStringValue "page" |> Option.bind (fun s -> try Some (int s) with _ -> None) |> Option.defaultValue 1
                let limit = ctx.TryGetQueryStringValue "limit" |> Option.bind (fun s -> try Some (int s) with _ -> None) |> Option.defaultValue 50
                let source = ctx.TryGetQueryStringValue "source_app_id" |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))
                let target = ctx.TryGetQueryStringValue "target_app_id" |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))

                let pageParam = if page < 1 then 1 else page
                let limitParam = if limit < 1 || limit > 200 then 50 else limit

                let result = IntegrationRepository.getAll pageParam limitParam source target
                let json = Json.encodePaginatedResponse Json.encodeIntegration result
                return! (Giraffe.Core.json json) next ctx
            }

            // POST /integrations - create
            POST >=> route "/integrations" >=> fun next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateIntegrationRequest bodyStr with
                | Ok req ->
                    if String.IsNullOrWhiteSpace(req.SourceAppId) || String.IsNullOrWhiteSpace(req.TargetAppId) then
                        ctx.SetStatusCode 400
                        let errJson = Json.encodeErrorResponse "validation_error" "Request validation failed"
                        return! (Giraffe.Core.json errJson) next ctx
                    else
                        let integ = IntegrationRepository.create req
                        ctx.SetStatusCode 201
                        ctx.SetHttpHeader ("Location", $"/integrations/{integ.Id}")
                        let json = Json.encodeIntegration integ
                        return! (Giraffe.Core.json json) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errJson) next ctx
            }

            // GET /integrations/{id}
            GET >=> routef "/integrations/%s" (fun id next ctx -> task {
                match IntegrationRepository.getById id with
                | Some integ ->
                    let json = Json.encodeIntegration integ
                    return! (Giraffe.Core.json json) next ctx
                | None ->
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "Integration not found"
                    return! (Giraffe.Core.json errJson) next ctx
            })

            // PATCH /integrations/{id}
            PATCH >=> routef "/integrations/%s" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateIntegrationRequest bodyStr with
                | Ok req ->
                    if String.IsNullOrWhiteSpace(req.SourceAppId) || String.IsNullOrWhiteSpace(req.TargetAppId) then
                        ctx.SetStatusCode 400
                        let errJson = Json.encodeErrorResponse "validation_error" "Request validation failed"
                        return! (Giraffe.Core.json errJson) next ctx
                    else
                        match IntegrationRepository.update id req with
                        | Some integ ->
                            let json = Json.encodeIntegration integ
                            return! (Giraffe.Core.json json) next ctx
                        | None ->
                            ctx.SetStatusCode 404
                            let errJson = Json.encodeErrorResponse "not_found" "Integration not found"
                            return! (Giraffe.Core.json errJson) next ctx
                | Error err ->
                    ctx.SetStatusCode 400
                    let errJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
                    return! (Giraffe.Core.json errJson) next ctx
            })

            // DELETE /integrations/{id}
            DELETE >=> routef "/integrations/%s" (fun id next ctx -> task {
                if IntegrationRepository.delete id then
                    ctx.SetStatusCode 204
                    return! next ctx
                else
                    ctx.SetStatusCode 404
                    let errJson = Json.encodeErrorResponse "not_found" "Integration not found"
                    return! (Giraffe.Core.json errJson) next ctx
            })
        ]
