/// Application repository and database operations (in-memory)
namespace EATool.Infrastructure

open System
open EATool.Domain

module ApplicationRepository =
    let private applications = System.Collections.Generic.Dictionary<string, Application>()

    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "app-" + guid.Substring(0, 8)

    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let getAll (page: int) (limit: int) (search: string option) (owner: string option) (lifecycle: Lifecycle option) : PaginatedResponse<Application> =
        let page = if page < 1 then 1 else page
        let limit = if limit < 1 || limit > 200 then 50 else limit

        let filtered =
            applications.Values
            |> Seq.toList
            |> (fun xs ->
                match search with
                | Some term when term.Trim() <> "" -> xs |> List.filter (fun a -> a.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                | _ -> xs)
            |> (fun xs ->
                match owner with
                | Some o when o.Trim() <> "" -> xs |> List.filter (fun a -> a.Owner |> Option.exists (fun v -> v.Equals(o, StringComparison.OrdinalIgnoreCase)))
                | _ -> xs)
            |> (fun xs ->
                match lifecycle with
                | Some lc -> xs |> List.filter (fun a -> a.Lifecycle = lc)
                | None -> xs)

        let total = List.length filtered
        let items =
            filtered
            |> List.skip ((page - 1) * limit)
            |> List.truncate limit

        { Items = items; Page = page; Limit = limit; Total = total }

    let getById (id: string) : Application option =
        if applications.ContainsKey(id) then Some applications.[id] else None

    let create (req: CreateApplicationRequest) : Application =
        let id = generateId ()
        let now = getUtcTimestamp ()
        let tags = req.Tags |> Option.defaultValue []

        let app =
            { Id = id
              Name = req.Name
              Owner = req.Owner
              Lifecycle = req.Lifecycle
              LifecycleRaw = req.LifecycleRaw
              CapabilityId = req.CapabilityId
              DataClassification = req.DataClassification
              Tags = tags
              CreatedAt = now
              UpdatedAt = now }

        applications.[id] <- app
        app

    let update (id: string) (req: CreateApplicationRequest) : Application option =
        match getById id with
        | Some existing ->
            let now = getUtcTimestamp ()
            let tags = req.Tags |> Option.defaultValue existing.Tags
            let updated =
                { existing with
                    Name = req.Name
                    Owner = req.Owner
                    Lifecycle = req.Lifecycle
                    LifecycleRaw = req.LifecycleRaw
                    CapabilityId = req.CapabilityId
                    DataClassification = req.DataClassification
                    Tags = tags
                    UpdatedAt = now }
            applications.[id] <- updated
            Some updated
        | None -> None

    let delete (id: string) : bool =
        applications.Remove(id)

    let clear () =
        applications.Clear()
