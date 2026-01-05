/// Organization API endpoints
namespace EATool.Api

open System
open Giraffe
open EATool.Domain
open EATool.Infrastructure

/// Register organization endpoints
module OrganizationsEndpoints =
    
    let routes: HttpHandler list =
        [
            // GET /organizations - List with pagination and search
            GET >=> route "/organizations" >=> fun next ctx -> task {
                let page = ctx.TryGetQueryStringValue "page" |> Option.bind (fun s -> try Some (int s) with _ -> None) |> Option.defaultValue 1
                let limit = ctx.TryGetQueryStringValue "limit" |> Option.bind (fun s -> try Some (int s) with _ -> None) |> Option.defaultValue 50
                let search = ctx.TryGetQueryStringValue "search" |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))
                
                let pageParam = if page < 1 then 1 else page
                let limitParam = if limit < 1 || limit > 200 then 50 else limit
                
                let result = OrganizationRepository.getAll pageParam limitParam search
                return! Giraffe.Core.json result next ctx
            }
            
            // POST /organizations - Create
            POST >=> route "/organizations" >=> fun next ctx -> task {
                let! req = ctx.BindJsonAsync<CreateOrganizationRequest>()
                if String.IsNullOrWhiteSpace(req.Name) then
                    ctx.SetStatusCode 400
                    return! Giraffe.Core.json {|
                        Code = "validation_error"
                        Message = "Request validation failed"
                        Errors = [ ("name", "Name is required") ]
                    |} next ctx
                else
                    let org = OrganizationRepository.create req
                    ctx.SetStatusCode 201
                    ctx.SetHttpHeader ("Location", $"/organizations/{org.Id}")
                    return! Giraffe.Core.json org next ctx
            }
            
            // GET /organizations/{id} - Get by ID
            GET >=> routef "/organizations/%s" (fun id next ctx -> task {
                match OrganizationRepository.getById id with
                | Some org -> return! Giraffe.Core.json org next ctx
                | None ->
                    ctx.SetStatusCode 404
                    return! Giraffe.Core.json {|
                        Code = "not_found"
                        Message = "Organization not found"
                    |} next ctx
            })
            
            // PATCH /organizations/{id} - Update
            PATCH >=> routef "/organizations/%s" (fun id next ctx -> task {
                let! req = ctx.BindJsonAsync<CreateOrganizationRequest>()
                if String.IsNullOrWhiteSpace(req.Name) then
                    ctx.SetStatusCode 400
                    return! Giraffe.Core.json {|
                        Code = "validation_error"
                        Message = "Request validation failed"
                        Errors = [ ("name", "Name is required") ]
                    |} next ctx
                else
                    match OrganizationRepository.update id req with
                    | Some org -> return! Giraffe.Core.json org next ctx
                    | None ->
                        ctx.SetStatusCode 404
                        return! Giraffe.Core.json {|
                            Code = "not_found"
                            Message = "Organization not found"
                        |} next ctx
            })
            
            // DELETE /organizations/{id} - Delete
            DELETE >=> routef "/organizations/%s" (fun id next ctx -> task {
                if OrganizationRepository.delete id then
                    ctx.SetStatusCode 204
                    return! next ctx
                else
                    ctx.SetStatusCode 404
                    return! Giraffe.Core.json {|
                        Code = "not_found"
                        Message = "Organization not found"
                    |} next ctx
            })
        ]
