/// Data entity commands, events, and aggregates
namespace EATool.Domain

open System

/// Data entity commands
type DataEntityCommand =
    | CreateDataEntity of CreateDataEntityData
    | SetClassification of SetClassificationData
    | SetPIIFlag of SetPIIFlagData
    | UpdateRetention of UpdateRetentionData
    | AddDataEntityTags of AddDataEntityTagsData
    | DeleteDataEntity of DeleteDataEntityData

/// Data entity events
and DataEntityEvent =
    | DataEntityCreated of DataEntityCreatedData
    | ClassificationSet of ClassificationSetData
    | PIIFlagSet of PIIFlagSetData
    | RetentionUpdated of RetentionUpdatedData
    | DataEntityTagsAdded of DataEntityTagsAddedData
    | DataEntityDeleted of DataEntityDeletedData

/// Command data types
and CreateDataEntityData = {
    Id: string
    Name: string
    Domain: string option
    Classification: string
    Retention: string option
    Owner: string option
    Steward: string option
    SourceSystem: string option
    Criticality: string option
    PiiFlag: bool
    Tags: string list
}

and SetClassificationData = {
    Id: string
    OldClassification: string
    NewClassification: string
}

and SetPIIFlagData = {
    Id: string
    OldPiiFlag: bool
    NewPiiFlag: bool
}

and UpdateRetentionData = {
    Id: string
    OldRetention: string option
    NewRetention: string option
}

and AddDataEntityTagsData = {
    Id: string
    AddedTags: string list
}

and DeleteDataEntityData = {
    Id: string
}

/// Event data types
and DataEntityCreatedData = {
    Id: string
    Name: string
    Domain: string option
    Classification: string
    Retention: string option
    Owner: string option
    Steward: string option
    SourceSystem: string option
    Criticality: string option
    PiiFlag: bool
    Tags: string list
}

and ClassificationSetData = {
    Id: string
    OldClassification: string
    NewClassification: string
}

and PIIFlagSetData = {
    Id: string
    OldPiiFlag: bool
    NewPiiFlag: bool
}

and RetentionUpdatedData = {
    Id: string
    OldRetention: string option
    NewRetention: string option
}

and DataEntityTagsAddedData = {
    Id: string
    AddedTags: string list
}

and DataEntityDeletedData = {
    Id: string
}

/// Data entity aggregate for event sourcing
type DataEntityAggregate = {
    Id: Option<string>
    Name: Option<string>
    Domain: string option
    Classification: Option<string>
    Retention: string option
    Owner: string option
    Steward: string option
    SourceSystem: string option
    Criticality: string option
    PiiFlag: bool
    Tags: string list
    IsDeleted: bool
}

module DataEntityAggregate =
    let Empty: DataEntityAggregate = {
        Id = None
        Name = None
        Domain = None
        Classification = None
        Retention = None
        Owner = None
        Steward = None
        SourceSystem = None
        Criticality = None
        PiiFlag = false
        Tags = []
        IsDeleted = false
    }

    let private applyDataEntityCreated (data: DataEntityCreatedData) (agg: DataEntityAggregate) =
        { agg with
            Id = Some data.Id
            Name = Some data.Name
            Domain = data.Domain
            Classification = Some data.Classification
            Retention = data.Retention
            Owner = data.Owner
            Steward = data.Steward
            SourceSystem = data.SourceSystem
            Criticality = data.Criticality
            PiiFlag = data.PiiFlag
            Tags = data.Tags
            IsDeleted = false }

    let private applyClassificationSet (data: ClassificationSetData) (agg: DataEntityAggregate) =
        { agg with Classification = Some data.NewClassification }

    let private applyPIIFlagSet (data: PIIFlagSetData) (agg: DataEntityAggregate) =
        { agg with PiiFlag = data.NewPiiFlag }

    let private applyRetentionUpdated (data: RetentionUpdatedData) (agg: DataEntityAggregate) =
        { agg with Retention = data.NewRetention }

    let private applyDataEntityTagsAdded (data: DataEntityTagsAddedData) (agg: DataEntityAggregate) =
        let newTags = (agg.Tags @ data.AddedTags) |> List.distinct
        { agg with Tags = newTags }

    let private applyDataEntityDeleted (data: DataEntityDeletedData) (agg: DataEntityAggregate) =
        { agg with IsDeleted = true }

    let ApplyEvent (agg: DataEntityAggregate) (event: DataEntityEvent) : DataEntityAggregate =
        match event with
        | DataEntityCreated data -> applyDataEntityCreated data agg
        | ClassificationSet data -> applyClassificationSet data agg
        | PIIFlagSet data -> applyPIIFlagSet data agg
        | RetentionUpdated data -> applyRetentionUpdated data agg
        | DataEntityTagsAdded data -> applyDataEntityTagsAdded data agg
        | DataEntityDeleted data -> applyDataEntityDeleted data agg

    let ApplyEvents (agg: DataEntityAggregate) (events: DataEntityEvent list) : DataEntityAggregate =
        events |> List.fold (fun acc e -> ApplyEvent acc e) agg
