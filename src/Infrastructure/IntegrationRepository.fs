/// Integration repository and database operations (in-memory)
namespace EATool.Infrastructure

open System
open EATool.Domain

module IntegrationRepository =
    let private integrations = System.Collections.Generic.Dictionary<string, Integration>()

    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "int-" + guid.Substring(0, 8)

    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let getAll (page: int) (limit: int) (sourceAppId: string option) (targetAppId: string option) : PaginatedResponse<Integration> =
        let page = if page < 1 then 1 else page
        let limit = if limit < 1 || limit > 200 then 50 else limit

        let filtered =
            integrations.Values
            |> Seq.toList
            |> (fun xs ->
                match sourceAppId with
                | Some src when src.Trim() <> "" -> xs |> List.filter (fun i -> i.SourceAppId.Equals(src, StringComparison.OrdinalIgnoreCase))
                | _ -> xs)
            |> (fun xs ->
                match targetAppId with
                | Some tgt when tgt.Trim() <> "" -> xs |> List.filter (fun i -> i.TargetAppId.Equals(tgt, StringComparison.OrdinalIgnoreCase))
                | _ -> xs)

        let total = List.length filtered
        let items =
            filtered
            |> List.skip ((page - 1) * limit)
            |> List.truncate limit

        { Items = items; Page = page; Limit = limit; Total = total }

    let getById (id: string) : Integration option =
        if integrations.ContainsKey(id) then Some integrations.[id] else None

    let create (req: CreateIntegrationRequest) : Integration =
        let id = generateId ()
        let now = getUtcTimestamp ()
        let tags = req.Tags |> Option.defaultValue []

        let integ =
            { Id = id
              SourceAppId = req.SourceAppId
              TargetAppId = req.TargetAppId
              Protocol = req.Protocol
              DataContract = req.DataContract
              Sla = req.Sla
              Frequency = req.Frequency
              Tags = tags
              CreatedAt = now
              UpdatedAt = now }

        integrations.[id] <- integ
        integ

    let update (id: string) (req: CreateIntegrationRequest) : Integration option =
        match getById id with
        | Some existing ->
            let now = getUtcTimestamp ()
            let tags = req.Tags |> Option.defaultValue existing.Tags
            let updated =
                { existing with
                    SourceAppId = req.SourceAppId
                    TargetAppId = req.TargetAppId
                    Protocol = req.Protocol
                    DataContract = req.DataContract
                    Sla = req.Sla
                    Frequency = req.Frequency
                    Tags = tags
                    UpdatedAt = now }
            integrations.[id] <- updated
            Some updated
        | None -> None

    let delete (id: string) : bool =
        integrations.Remove(id)

    let clear () =
        integrations.Clear()
