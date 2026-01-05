/// Relation repository (in-memory)
namespace EATool.Infrastructure

open System
open EATool.Domain

module RelationRepository =
    let private relations = System.Collections.Generic.Dictionary<string, Relation>()

    let private generateId () =
        let guid = Guid.NewGuid().ToString("N")
        "rel-" + guid.Substring(0, 8)

    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let getAll (page: int) (limit: int) (sourceId: string option) (targetId: string option) (relationType: RelationType option) : PaginatedResponse<Relation> =
        let page = if page < 1 then 1 else page
        let limit = if limit < 1 || limit > 200 then 50 else limit

        let filtered =
            relations.Values
            |> Seq.toList
            |> (fun xs ->
                match sourceId with
                | Some sid when sid.Trim() <> "" -> xs |> List.filter (fun r -> r.SourceId.Equals(sid, StringComparison.OrdinalIgnoreCase))
                | _ -> xs)
            |> (fun xs ->
                match targetId with
                | Some tid when tid.Trim() <> "" -> xs |> List.filter (fun r -> r.TargetId.Equals(tid, StringComparison.OrdinalIgnoreCase))
                | _ -> xs)
            |> (fun xs ->
                match relationType with
                | Some rt -> xs |> List.filter (fun r -> r.RelationType = rt)
                | None -> xs)

        let total = List.length filtered
        let items =
            filtered
            |> List.skip ((page - 1) * limit)
            |> List.truncate limit

        { Items = items; Page = page; Limit = limit; Total = total }

    let getById (id: string) : Relation option =
        if relations.ContainsKey(id) then Some relations.[id] else None

    let create (req: CreateRelationRequest) : Relation =
        let id = generateId ()
        let now = getUtcTimestamp ()
        let bidir = req.Bidirectional |> Option.defaultValue false

        let rel =
            { Id = id
              SourceId = req.SourceId
              TargetId = req.TargetId
              SourceType = req.SourceType
              TargetType = req.TargetType
              RelationType = req.RelationType
              ArchiMateElement = req.ArchiMateElement
              ArchiMateRelationship = req.ArchiMateRelationship
              Description = req.Description
              DataClassification = req.DataClassification
              Criticality = req.Criticality
              Confidence = req.Confidence
              EvidenceSource = req.EvidenceSource
              LastVerifiedAt = req.LastVerifiedAt
              EffectiveFrom = req.EffectiveFrom
              EffectiveTo = req.EffectiveTo
              Label = req.Label
              Color = req.Color
              Style = req.Style
              Bidirectional = bidir
              CreatedAt = now
              UpdatedAt = now }

        relations.[id] <- rel
        rel

    let update (id: string) (req: CreateRelationRequest) : Relation option =
        match getById id with
        | Some existing ->
            let now = getUtcTimestamp ()
            let bidir = req.Bidirectional |> Option.defaultValue existing.Bidirectional

            let updated =
                { existing with
                    SourceId = req.SourceId
                    TargetId = req.TargetId
                    SourceType = req.SourceType
                    TargetType = req.TargetType
                    RelationType = req.RelationType
                    ArchiMateElement = req.ArchiMateElement
                    ArchiMateRelationship = req.ArchiMateRelationship
                    Description = req.Description
                    DataClassification = req.DataClassification
                    Criticality = req.Criticality
                    Confidence = req.Confidence
                    EvidenceSource = req.EvidenceSource
                    LastVerifiedAt = req.LastVerifiedAt
                    EffectiveFrom = req.EffectiveFrom
                    EffectiveTo = req.EffectiveTo
                    Label = req.Label
                    Color = req.Color
                    Style = req.Style
                    Bidirectional = bidir
                    UpdatedAt = now }
            relations.[id] <- updated
            Some updated
        | None -> None

    let delete (id: string) : bool =
        relations.Remove(id)

    let clear () =
        relations.Clear()
