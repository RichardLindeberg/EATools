/// Data entity repository (in-memory)
namespace EATool.Infrastructure

open System
open EATool.Domain

module DataEntityRepository =
    let private dataEntities = System.Collections.Generic.Dictionary<string, DataEntity>()

    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "dat-" + guid.Substring(0, 8)

    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let getAll (page: int) (limit: int) (search: string option) (domain: string option) (classification: DataClassification option) : PaginatedResponse<DataEntity> =
        let page = if page < 1 then 1 else page
        let limit = if limit < 1 || limit > 200 then 50 else limit

        let filtered =
            dataEntities.Values
            |> Seq.toList
            |> (fun xs ->
                match search with
                | Some term when term.Trim() <> "" -> xs |> List.filter (fun e -> e.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                | _ -> xs)
            |> (fun xs ->
                match domain with
                | Some d when d.Trim() <> "" -> xs |> List.filter (fun e -> e.Domain |> Option.exists (fun v -> v.Equals(d, StringComparison.OrdinalIgnoreCase)))
                | _ -> xs)
            |> (fun xs ->
                match classification with
                | Some cls -> xs |> List.filter (fun e -> e.Classification = cls)
                | None -> xs)

        let total = List.length filtered
        let items =
            filtered
            |> List.skip ((page - 1) * limit)
            |> List.truncate limit

        { Items = items; Page = page; Limit = limit; Total = total }

    let getById (id: string) : DataEntity option =
        if dataEntities.ContainsKey(id) then Some dataEntities.[id] else None

    let create (req: CreateDataEntityRequest) : DataEntity =
        let id = generateId ()
        let now = getUtcTimestamp ()
        let pii = req.PiiFlag |> Option.defaultValue false
        let glossary = req.GlossaryTerms |> Option.defaultValue []
        let lineage = req.Lineage |> Option.defaultValue []

        let entity =
            { Id = id
              Name = req.Name
              Domain = req.Domain
              Classification = req.Classification
              Retention = req.Retention
              Owner = req.Owner
              Steward = req.Steward
              SourceSystem = req.SourceSystem
              Criticality = req.Criticality
              PiiFlag = pii
              GlossaryTerms = glossary
              Lineage = lineage
              CreatedAt = now
              UpdatedAt = now }

        dataEntities.[id] <- entity
        entity

    let update (id: string) (req: CreateDataEntityRequest) : DataEntity option =
        match getById id with
        | Some existing ->
            let now = getUtcTimestamp ()
            let pii = req.PiiFlag |> Option.defaultValue existing.PiiFlag
            let glossary = req.GlossaryTerms |> Option.defaultValue existing.GlossaryTerms
            let lineage = req.Lineage |> Option.defaultValue existing.Lineage

            let updated =
                { existing with
                    Name = req.Name
                    Domain = req.Domain
                    Classification = req.Classification
                    Retention = req.Retention
                    Owner = req.Owner
                    Steward = req.Steward
                    SourceSystem = req.SourceSystem
                    Criticality = req.Criticality
                    PiiFlag = pii
                    GlossaryTerms = glossary
                    Lineage = lineage
                    UpdatedAt = now }
            dataEntities.[id] <- updated
            Some updated
        | None -> None

    let delete (id: string) : bool =
        dataEntities.Remove(id)

    let clear () =
        dataEntities.Clear()
