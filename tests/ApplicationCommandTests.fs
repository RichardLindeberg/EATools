module ApplicationCommandTests

open System
open Xunit
open EATool.Domain

[<Fact>]
let ``CreateApplication succeeds with valid data`` () =
    let state = ApplicationAggregate.Initial
    let cmd : CreateApplicationData = {
        Id = "app-test123"
        Name = "Test Application"
        Owner = Some "test-owner"
        Lifecycle = "planned"
        CapabilityId = None
        DataClassification = Some "internal"
        Criticality = Some "medium"
        Tags = ["test"; "demo"]
        Description = Some "Test app"
    }
    
    match ApplicationCommandHandler.handleCreateApplication state cmd with
    | Error e -> Assert.True(false, e)
    | Ok events ->
        Assert.Equal(1, events.Length)
        match events.[0] with
        | ApplicationCreated data ->
            Assert.Equal("app-test123", data.Id)
            Assert.Equal("Test Application", data.Name)
            Assert.Equal(Some "test-owner", data.Owner)
        | _ -> Assert.True(false, "Expected ApplicationCreated event")

[<Fact>]
let ``CreateApplication fails when application already exists`` () =
    let state = { ApplicationAggregate.Initial with Id = Some "app-existing" }
    let cmd : CreateApplicationData = {
        Id = "app-existing"
        Name = "Test"
        Owner = None
        Lifecycle = "planned"
        CapabilityId = None
        DataClassification = None
        Criticality = None
        Tags = []
        Description = None
    }
    
    match ApplicationCommandHandler.handleCreateApplication state cmd with
    | Error msg -> Assert.Contains("already exists", msg)
    | Ok _ -> Assert.True(false, "Should have failed")

[<Fact>]
let ``CreateApplication fails with invalid classification`` () =
    let state = ApplicationAggregate.Initial
    let cmd : CreateApplicationData = {
        Id = "app-test"
        Name = "Test"
        Owner = None
        Lifecycle = "planned"
        CapabilityId = None
        DataClassification = Some "invalid-class"
        Criticality = None
        Tags = []
        Description = None
    }
    
    match ApplicationCommandHandler.handleCreateApplication state cmd with
    | Error msg -> Assert.Contains("Invalid classification", msg)
    | Ok _ -> Assert.True(false, "Should have failed")

[<Fact>]
let ``TransitionLifecycle validates state transition`` () =
    let state = { ApplicationAggregate.Initial with 
                    Id = Some "app-test"
                    Lifecycle = Some "planned" }
    let cmd : TransitionLifecycleData = {
        Id = "app-test"
        TargetLifecycle = "active"
        SunsetDate = None
    }
    
    match ApplicationCommandHandler.handleTransitionLifecycle state cmd with
    | Error e -> Assert.True(false, e)
    | Ok events ->
        Assert.Equal(1, events.Length)
        match events.[0] with
        | LifecycleTransitioned data ->
            Assert.Equal("planned", data.FromLifecycle)
            Assert.Equal("active", data.ToLifecycle)
        | _ -> Assert.True(false, "Expected LifecycleTransitioned event")

[<Fact>]
let ``TransitionLifecycle rejects invalid transition`` () =
    let state = { ApplicationAggregate.Initial with 
                    Id = Some "app-test"
                    Lifecycle = Some "retired" }
    let cmd : TransitionLifecycleData = {
        Id = "app-test"
        TargetLifecycle = "active"
        SunsetDate = None
    }
    
    match ApplicationCommandHandler.handleTransitionLifecycle state cmd with
    | Error msg -> Assert.Contains("Invalid lifecycle transition", msg)
    | Ok _ -> Assert.True(false, "Should have failed - cannot transition from retired")

[<Fact>]
let ``SetDataClassification requires reason`` () =
    let state = { ApplicationAggregate.Initial with 
                    Id = Some "app-test"
                    DataClassification = Some "internal" }
    let cmd : SetDataClassificationData = {
        Id = "app-test"
        Classification = "confidential"
        Reason = ""
    }
    
    match ApplicationCommandHandler.handleSetDataClassification state cmd with
    | Error msg -> Assert.Contains("Reason is required", msg)
    | Ok _ -> Assert.True(false, "Should have failed")

[<Fact>]
let ``SetDataClassification succeeds with valid reason`` () =
    let state = { ApplicationAggregate.Initial with 
                    Id = Some "app-test"
                    DataClassification = Some "internal" }
    let cmd : SetDataClassificationData = {
        Id = "app-test"
        Classification = "confidential"
        Reason = "Processing sensitive customer data"
    }
    
    match ApplicationCommandHandler.handleSetDataClassification state cmd with
    | Error e -> Assert.True(false, e)
    | Ok events ->
        Assert.Equal(1, events.Length)
        match events.[0] with
        | DataClassificationChanged data ->
            Assert.Equal(Some "internal", data.OldClassification)
            Assert.Equal("confidential", data.NewClassification)
            Assert.Equal("Processing sensitive customer data", data.Reason)
        | _ -> Assert.True(false, "Expected DataClassificationChanged event")

[<Fact>]
let ``AddTags adds only new tags`` () =
    let state = { ApplicationAggregate.Initial with 
                    Id = Some "app-test"
                    Tags = ["existing"; "tag"] }
    let cmd : AddTagsData = {
        Id = "app-test"
        Tags = ["existing"; "new-tag"; "another"]
    }
    
    match ApplicationCommandHandler.handleAddTags state cmd with
    | Error e -> Assert.True(false, e)
    | Ok events ->
        Assert.Equal(1, events.Length)
        match events.[0] with
        | TagsAdded data ->
            Assert.Equal(2, data.AddedTags.Length)
            Assert.Contains("new-tag", data.AddedTags)
            Assert.Contains("another", data.AddedTags)
            Assert.DoesNotContain("existing", data.AddedTags)
        | _ -> Assert.True(false, "Expected TagsAdded event")

[<Fact>]
let ``RemoveTags removes only existing tags`` () =
    let state = { ApplicationAggregate.Initial with 
                    Id = Some "app-test"
                    Tags = ["tag1"; "tag2"; "tag3"] }
    let cmd : RemoveTagsData = {
        Id = "app-test"
        Tags = ["tag1"; "non-existent"; "tag3"]
    }
    
    match ApplicationCommandHandler.handleRemoveTags state cmd with
    | Error e -> Assert.True(false, e)
    | Ok events ->
        Assert.Equal(1, events.Length)
        match events.[0] with
        | TagsRemoved data ->
            Assert.Equal(2, data.RemovedTags.Length)
            Assert.Contains("tag1", data.RemovedTags)
            Assert.Contains("tag3", data.RemovedTags)
            Assert.DoesNotContain("non-existent", data.RemovedTags)
        | _ -> Assert.True(false, "Expected TagsRemoved event")

[<Fact>]
let ``DeleteApplication requires approval ID`` () =
    let state = { ApplicationAggregate.Initial with Id = Some "app-test" }
    let cmd : DeleteApplicationData = {
        Id = "app-test"
        Reason = "No longer needed"
        ApprovalId = ""
    }
    
    match ApplicationCommandHandler.handleDeleteApplication state cmd with
    | Error msg -> Assert.Contains("Approval ID is required", msg)
    | Ok _ -> Assert.True(false, "Should have failed")

[<Fact>]
let ``DeleteApplication succeeds with approval`` () =
    let state = { ApplicationAggregate.Initial with Id = Some "app-test" }
    let cmd : DeleteApplicationData = {
        Id = "app-test"
        Reason = "End of life"
        ApprovalId = "approval-12345"
    }
    
    match ApplicationCommandHandler.handleDeleteApplication state cmd with
    | Error e -> Assert.True(false, e)
    | Ok events ->
        Assert.Equal(1, events.Length)
        match events.[0] with
        | ApplicationDeleted data ->
            Assert.Equal("app-test", data.Id)
            Assert.Equal("End of life", data.Reason)
            Assert.Equal("approval-12345", data.ApprovalId)
        | _ -> Assert.True(false, "Expected ApplicationDeleted event")

[<Fact>]
let ``ApplicationAggregate applies events correctly`` () =
    let state = ApplicationAggregate.Initial
    
    // Apply creation event
    let created = ApplicationCreated {
        Id = "app-test"
        Name = "Test App"
        Owner = Some "owner1"
        Lifecycle = "planned"
        CapabilityId = None
        DataClassification = Some "internal"
        Criticality = Some "high"
        Tags = ["tag1"]
        Description = Some "desc"
    }
    let state = ApplicationAggregate.apply state created
    Assert.Equal(Some "app-test", state.Id)
    Assert.Equal(Some "Test App", state.Name)
    Assert.Equal(1, state.Tags.Length)
    
    // Apply lifecycle transition
    let transitioned = LifecycleTransitioned {
        Id = "app-test"
        FromLifecycle = "planned"
        ToLifecycle = "active"
        SunsetDate = None
    }
    let state = ApplicationAggregate.apply state transitioned
    Assert.Equal(Some "active", state.Lifecycle)
    
    // Apply tags addition
    let tagsAdded = TagsAdded { Id = "app-test"; AddedTags = ["tag2"; "tag3"] }
    let state = ApplicationAggregate.apply state tagsAdded
    Assert.Equal(3, state.Tags.Length)
    Assert.Contains("tag1", state.Tags)
    Assert.Contains("tag2", state.Tags)
    
    // Apply tags removal
    let tagsRemoved = TagsRemoved { Id = "app-test"; RemovedTags = ["tag1"] }
    let state = ApplicationAggregate.apply state tagsRemoved
    Assert.Equal(2, state.Tags.Length)
    Assert.DoesNotContain("tag1", state.Tags)
    
    // Apply deletion
    let deleted = ApplicationDeleted { Id = "app-test"; Reason = "test"; ApprovalId = "a123" }
    let state = ApplicationAggregate.apply state deleted
    Assert.True(state.IsDeleted)
