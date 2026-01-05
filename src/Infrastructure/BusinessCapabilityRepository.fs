/// Business capability repository (in-memory)
namespace EATool.Infrastructure

open System
open EATool.Domain

module BusinessCapabilityRepository =
    let private capabilities = System.Collections.Generic.Dictionary<string, BusinessCapability>()

    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "cap-" + guid.Substring(0, 8)

    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let getAll (page: int) (limit: int) (search: string option) (parentId: string option) : PaginatedResponse<BusinessCapability> =
        let page = if page < 1 then 1 else page
        let limit = if limit < 1 || limit > 200 then 50 else limit

        let filtered =
            capabilities.Values
            |> Seq.toList
            |> (fun xs ->
                match search with
                | Some term when term.Trim() <> "" -> xs |> List.filter (fun c -> c.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                | _ -> xs)
            |> (fun xs ->
                match parentId with
                | Some pid when pid.Trim() <> "" -> xs |> List.filter (fun c -> c.ParentId |> Option.exists (fun v -> v.Equals(pid, StringComparison.OrdinalIgnoreCase)))
                | _ -> xs)

        let total = List.length filtered
        let items =
            filtered
            |> List.skip ((page - 1) * limit)
            |> List.truncate limit

        { Items = items; Page = page; Limit = limit; Total = total }

    let getById (id: string) : BusinessCapability option =
        if capabilities.ContainsKey(id) then Some capabilities.[id] else None

    let create (req: CreateBusinessCapabilityRequest) : BusinessCapability =
        let id = generateId ()
        let now = getUtcTimestamp ()

        let cap =
            { Id = id
              Name = req.Name
              ParentId = req.ParentId
              CreatedAt = now
              UpdatedAt = now }

        capabilities.[id] <- cap
        cap

    let update (id: string) (req: CreateBusinessCapabilityRequest) : BusinessCapability option =
        match getById id with
        | Some existing ->
            let now = getUtcTimestamp ()
            let updated =
                { existing with
                    Name = req.Name
                    ParentId = req.ParentId
                    UpdatedAt = now }
            capabilities.[id] <- updated
            Some updated
        | None -> None

    let delete (id: string) : bool =
        capabilities.Remove(id)

    let clear () =
        capabilities.Clear()
