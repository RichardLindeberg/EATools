/// Server repository and database operations (in-memory)
namespace EATool.Infrastructure

open System
open EATool.Domain

module ServerRepository =
    let private servers = System.Collections.Generic.Dictionary<string, Server>()

    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "srv-" + guid.Substring(0, 8)

    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let getAll (page: int) (limit: int) (environment: string option) (region: string option) : PaginatedResponse<Server> =
        let page = if page < 1 then 1 else page
        let limit = if limit < 1 || limit > 200 then 50 else limit

        let filtered =
            servers.Values
            |> Seq.toList
            |> (fun xs ->
                match environment with
                | Some env when env.Trim() <> "" -> xs |> List.filter (fun s -> s.Environment |> Option.exists (fun v -> v.Equals(env, StringComparison.OrdinalIgnoreCase)))
                | _ -> xs)
            |> (fun xs ->
                match region with
                | Some r when r.Trim() <> "" -> xs |> List.filter (fun s -> s.Region |> Option.exists (fun v -> v.Equals(r, StringComparison.OrdinalIgnoreCase)))
                | _ -> xs)

        let total = List.length filtered
        let items =
            filtered
            |> List.skip ((page - 1) * limit)
            |> List.truncate limit

        { Items = items; Page = page; Limit = limit; Total = total }

    let getById (id: string) : Server option =
        if servers.ContainsKey(id) then Some servers.[id] else None

    let create (req: CreateServerRequest) : Server =
        let id = generateId ()
        let now = getUtcTimestamp ()
        let tags = req.Tags |> Option.defaultValue []

        let srv =
            { Id = id
              Hostname = req.Hostname
              Environment = req.Environment
              Region = req.Region
              Platform = req.Platform
              Criticality = req.Criticality
              OwningTeam = req.OwningTeam
              Tags = tags
              CreatedAt = now
              UpdatedAt = now }

        servers.[id] <- srv
        srv

    let update (id: string) (req: CreateServerRequest) : Server option =
        match getById id with
        | Some existing ->
            let now = getUtcTimestamp ()
            let tags = req.Tags |> Option.defaultValue existing.Tags
            let updated =
                { existing with
                    Hostname = req.Hostname
                    Environment = req.Environment
                    Region = req.Region
                    Platform = req.Platform
                    Criticality = req.Criticality
                    OwningTeam = req.OwningTeam
                    Tags = tags
                    UpdatedAt = now }
            servers.[id] <- updated
            Some updated
        | None -> None

    let delete (id: string) : bool =
        servers.Remove(id)

    let clear () =
        servers.Clear()
