/// Application API endpoints with command-based architecture
namespace EATool.Api

open System
open Giraffe
open Thoth.Json.Net
open EATool.Domain
open EATool.Infrastructure

module ApplicationsEndpoints =
    
    /// Helper to generate application ID
    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "app-" + guid.Substring(0, 8)
    
    let routes: HttpHandler list =
        [
            // GET /applications - list (read from projection)
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

            // POST /applications - create application via CreateApplication command
            POST >=> route "/applications" >=> fun next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                match Decode.fromString Json.decodeCreateApplicationRequest bodyStr with
                | Ok req ->
                    let cmd : CreateApplicationData = {
                        Id = generateId ()
                        Name = req.Name
                        Owner = req.Owner
                        Lifecycle = req.Lifecycle |> function
                            | Lifecycle.Planned -> "planned"
                            | Lifecycle.Active -> "active"
                            | Lifecycle.Deprecated -> "deprecated"
                            | Lifecycle.Retired -> "retired"
                        CapabilityId = req.CapabilityId
                        DataClassification = req.DataClassification
                        Criticality = None // Not in current schema
                        Tags = req.Tags |> Option.defaultValue []
                        Description = None // Not in current schema
                    }
                    
                    let state = ApplicationAggregate.Initial
                    match ApplicationCommandHandler.handleCreateApplication state cmd with
                    | Error err ->
                        ctx.SetStatusCode 400
                        let errJson = Json.encodeErrorResponse "validation_error" err
                        return! (Giraffe.Core.json errJson) next ctx
                    | Ok events ->
                        // TODO: Persist events to event store and dispatch to projections
                        // For now, use repository to maintain compatibility
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

            // POST /applications/{id}/commands/set-classification
            POST >=> routef "/applications/%s/commands/set-classification" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                
                let decoder = Decode.object (fun get -> {|
                    Classification = get.Required.Field "classification" Decode.string
                    Reason = get.Required.Field "reason" Decode.string
                |})
                
                match Decode.fromString decoder bodyStr with
                | Ok req ->
                    let cmd : SetDataClassificationData = {
                        Id = id
                        Classification = req.Classification
                        Reason = req.Reason
                    }
                    
                    // TODO: Load aggregate state from event store
                    match ApplicationRepository.getById id with
                    | None ->
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "Application not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    | Some app ->
                        let state = { ApplicationAggregate.Initial with
                                        Id = Some app.Id
                                        DataClassification = app.DataClassification }
                        
                        match ApplicationCommandHandler.handleSetDataClassification state cmd with
                        | Error err ->
                            ctx.SetStatusCode 400
                            let errJson = Json.encodeErrorResponse "business_rule_violation" err
                            return! (Giraffe.Core.json errJson) next ctx
                        | Ok events ->
                            // TODO: Persist events and project
                            // For now, update directly
                            let updateReq = { 
                                Name = app.Name
                                Owner = app.Owner
                                Lifecycle = app.Lifecycle
                                CapabilityId = app.CapabilityId
                                DataClassification = Some req.Classification
                                Tags = Some app.Tags
                            }
                            match ApplicationRepository.update id updateReq with
                            | Some updatedApp ->
                                let json = Json.encodeApplication updatedApp
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

            // POST /applications/{id}/commands/transition-lifecycle
            POST >=> routef "/applications/%s/commands/transition-lifecycle" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                
                let decoder = Decode.object (fun get -> {|
                    TargetLifecycle = get.Required.Field "target_lifecycle" Decode.string
                    SunsetDate = get.Optional.Field "sunset_date" Decode.string
                |})
                
                match Decode.fromString decoder bodyStr with
                | Ok req ->
                    let cmd : TransitionLifecycleData = {
                        Id = id
                        TargetLifecycle = req.TargetLifecycle
                        SunsetDate = req.SunsetDate
                    }
                    
                    match ApplicationRepository.getById id with
                    | None ->
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "Application not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    | Some app ->
                        let currentLifecycle =
                            match app.Lifecycle with
                            | Lifecycle.Planned -> "planned"
                            | Lifecycle.Active -> "active"
                            | Lifecycle.Deprecated -> "deprecated"
                            | Lifecycle.Retired -> "retired"
                        
                        let state = { ApplicationAggregate.Initial with
                                        Id = Some app.Id
                                        Lifecycle = Some currentLifecycle }
                        
                        match ApplicationCommandHandler.handleTransitionLifecycle state cmd with
                        | Error err ->
                            ctx.SetStatusCode 400
                            let errJson = Json.encodeErrorResponse "business_rule_violation" err
                            return! (Giraffe.Core.json errJson) next ctx
                        | Ok events ->
                            // TODO: Persist events and project
                            let targetLifecycle =
                                match req.TargetLifecycle.ToLowerInvariant() with
                                | "planned" -> Lifecycle.Planned
                                | "active" -> Lifecycle.Active
                                | "deprecated" -> Lifecycle.Deprecated
                                | "retired" -> Lifecycle.Retired
                                | _ -> app.Lifecycle
                            
                            let updateReq = { 
                                Name = app.Name
                                Owner = app.Owner
                                Lifecycle = targetLifecycle
                                CapabilityId = app.CapabilityId
                                DataClassification = app.DataClassification
                                Tags = Some app.Tags
                            }
                            match ApplicationRepository.update id updateReq with
                            | Some updatedApp ->
                                let json = Json.encodeApplication updatedApp
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

            // POST /applications/{id}/commands/set-owner
            POST >=> routef "/applications/%s/commands/set-owner" (fun id next ctx -> task {
                let! bodyStr = ctx.ReadBodyFromRequestAsync()
                
                let decoder = Decode.object (fun get -> {|
                    Owner = get.Required.Field "owner" Decode.string
                    Reason = get.Optional.Field "reason" Decode.string
                |})
                
                match Decode.fromString decoder bodyStr with
                | Ok req ->
                    let cmd : SetOwnerData = {
                        Id = id
                        Owner = req.Owner
                        Reason = req.Reason
                    }
                    
                    match ApplicationRepository.getById id with
                    | None ->
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "Application not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    | Some app ->
                        let state = { ApplicationAggregate.Initial with
                                        Id = Some app.Id
                                        Owner = app.Owner }
                        
                        match ApplicationCommandHandler.handleSetOwner state cmd with
                        | Error err ->
                            ctx.SetStatusCode 400
                            let errJson = Json.encodeErrorResponse "business_rule_violation" err
                            return! (Giraffe.Core.json errJson) next ctx
                        | Ok events ->
                            let updateReq = { 
                                Name = app.Name
                                Owner = Some req.Owner
                                Lifecycle = app.Lifecycle
                                CapabilityId = app.CapabilityId
                                DataClassification = app.DataClassification
                                Tags = Some app.Tags
                            }
                            match ApplicationRepository.update id updateReq with
                            | Some updatedApp ->
                                let json = Json.encodeApplication updatedApp
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

            // DELETE /applications/{id} - requires approval
            DELETE >=> routef "/applications/%s" (fun id next ctx -> task {
                let approvalId = ctx.TryGetQueryStringValue "approval_id"
                let reason = ctx.TryGetQueryStringValue "reason"
                
                match approvalId, reason with
                | None, _ | _, None ->
                    ctx.SetStatusCode 400
                    let errJson = Json.encodeErrorResponse "validation_error" "approval_id and reason query parameters are required for deletion"
                    return! (Giraffe.Core.json errJson) next ctx
                | Some approval, Some deleteReason ->
                    let cmd : DeleteApplicationData = {
                        Id = id
                        Reason = deleteReason
                        ApprovalId = approval
                    }
                    
                    match ApplicationRepository.getById id with
                    | None ->
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "Application not found"
                        return! (Giraffe.Core.json errJson) next ctx
                    | Some app ->
                        let state = { ApplicationAggregate.Initial with Id = Some app.Id }
                        
                        match ApplicationCommandHandler.handleDeleteApplication state cmd with
                        | Error err ->
                            ctx.SetStatusCode 400
                            let errJson = Json.encodeErrorResponse "business_rule_violation" err
                            return! (Giraffe.Core.json errJson) next ctx
                        | Ok events ->
                            // TODO: Persist delete event
                            if ApplicationRepository.delete id then
                                ctx.SetStatusCode 204
                                return! next ctx
                            else
                                ctx.SetStatusCode 404
                                let errJson = Json.encodeErrorResponse "not_found" "Application not found"
                                return! (Giraffe.Core.json errJson) next ctx
            })

            // Legacy PATCH endpoint - deprecated, but kept for backwards compatibility
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
        ]
