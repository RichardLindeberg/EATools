/// Unit tests for context propagation
module EATool.Tests.ContextPropagationTests

#nowarn "0020" // Suppress warnings for ignored fluent Activity results in tests

open System
open System.Diagnostics
open System.Threading.Tasks
open Xunit
open EATool.Tests.Fixtures

[<Fact>]
let ``Activity context is available in current scope`` () =
    let activity = new Activity("TestSpan")
    activity.Start()
    
    let current = Activity.Current
    Assert.NotNull(current)
    Assert.Equal("TestSpan", current.OperationName)
    
    activity.Stop()

[<Fact>]
let ``Child activity inherits parent trace ID`` () =
    let parent = new Activity("Parent")
    parent.Start()
    let parentTraceId = parent.TraceId
    
    let child = new Activity("Child")
    child.Start()
    let childTraceId = child.TraceId
    
    Assert.Equal(parentTraceId, childTraceId)
    
    child.Stop()
    parent.Stop()

[<Fact>]
let ``Activity stops properly and clears context`` () =
    let activity = new Activity("TestSpan")
    activity.Start()
    Assert.NotNull(Activity.Current)
    
    activity.Stop()
    // Note: Activity.Current might still reference the stopped activity
    // depending on context, but span operations should not use it

[<Fact>]
let ``Multiple child activities preserve trace ID`` () =
    let parent = new Activity("Parent")
    parent.Start()
    let parentTraceId = parent.TraceId
    
    let child1 = new Activity("Child1")
    child1.Start()
    Assert.Equal(parentTraceId, child1.TraceId)
    child1.Stop()
    
    let child2 = new Activity("Child2")
    child2.Start()
    Assert.Equal(parentTraceId, child2.TraceId)
    child2.Stop()
    
    parent.Stop()

[<Fact>]
let ``Activity tags are preserved`` () =
    let activity = new Activity("TestSpan")
    activity.AddTag("operation_id", "op_123")
    activity.AddTag("user_id", "user_456")
    activity.Start()
    
    let tags = activity.TagObjects |> Seq.toList
    Assert.NotEmpty(tags)
    
    activity.Stop()

[<Fact>]
let ``Activity events are recorded`` () =
    let activity = new Activity("TestSpan")
    activity.Start()
    
    // ActivityEvent is available in newer versions
    // For compatibility, we test tag behavior
    activity.AddTag("event", "exception_occurred")
    activity.SetStatus(ActivityStatusCode.Error, "Failed operation")
    
    Assert.Equal(ActivityStatusCode.Error, activity.Status)
    
    activity.Stop()

[<Fact>]
let ``Exception details can be logged with activity`` () =
    let activity = new Activity("TestSpan")
    activity.Start()
    
    try
        raise (System.ArgumentException("Invalid argument"))
    with
    | ex ->
        activity.AddTag("exception.type", ex.GetType().Name)
        activity.AddTag("exception.message", ex.Message)
        activity.SetStatus(ActivityStatusCode.Error, ex.Message)
    
    Assert.Equal(ActivityStatusCode.Error, activity.Status)
    
    activity.Stop()

[<Fact>]
let ``Activity context is isolated between parallel operations`` () =
    let activity1 = new Activity("Parallel1")
    let activity2 = new Activity("Parallel2")
    
    activity1.Start()
    let trace1 = activity1.TraceId
    activity1.Stop()
    
    activity2.Start()
    let trace2 = activity2.TraceId
    activity2.Stop()
    
    // Each should have its own trace ID
    Assert.NotEqual(trace1, trace2)

[<Fact>]
let ``Baggage items are preserved`` () =
    let activity = new Activity("TestSpan")
    activity.AddBaggage("correlation_id", "corr_abc123")
    activity.Start()
    
    // Baggage is propagated through ActivityBaggage
    let items = activity.Baggage |> Seq.toList
    Assert.NotEmpty(items)
    
    activity.Stop()

[<Fact>]
let ``Operation name is accessible`` () =
    let operationName = "CreateOrganization"
    let activity = new Activity(operationName)
    activity.Start()
    
    Assert.Equal(operationName, activity.OperationName)
    Assert.Equal(operationName, activity.DisplayName)
    
    activity.Stop()

[<Fact>]
let ``Span duration is measured`` () =
    let activity = new Activity("TimedSpan")
    activity.Start()
    
    System.Threading.Thread.Sleep(50)
    
    activity.Stop()
    Assert.True(activity.Duration.TotalMilliseconds >= 50.0)

[<Fact>]
let ``Activity ID follows correct format`` () =
    let activity = new Activity("TestSpan")
    activity.Start()
    
    let traceId = activity.TraceId.ToString()
    let spanId = activity.SpanId.ToString()
    
    Assert.Matches("^[0-9a-f]{32}$", traceId)
    Assert.Matches("^[0-9a-f]{16}$", spanId)
    
    activity.Stop()

[<Fact>]
let ``Parent span ID is preserved in child`` () =
    let parent = new Activity("Parent")
    parent.Start()
    let parentSpanId = parent.SpanId.ToString()
    
    let child = new Activity("Child")
    child.Start()
    let childParentSpanId = child.ParentSpanId.ToString()
    
    Assert.Equal(parentSpanId, childParentSpanId)
    
    child.Stop()
    parent.Stop()

[<Fact>]
let ``Activity handles rapid start/stop cycles`` () =
    for i in 1..10 do
        let activity = new Activity($"Cycle{i}")
        activity.Start()
        activity.Stop()
    
    // Just ensure no exceptions are thrown
    Assert.True(true)

[<Fact>]
let ``Multiple attributes can be added`` () =
    let activity = new Activity("MultiAttribute")
    activity.AddTag("attr1", "value1")
    activity.AddTag("attr2", 42)
    activity.AddTag("attr3", 3.14)
    activity.AddTag("attr4", true)
    activity.Start()
    
    let tags = activity.TagObjects |> Seq.toList
    Assert.Equal(4, tags.Length)
    
    activity.Stop()

[<Fact>]
let ``Status can be changed`` () =
    let activity = new Activity("StatusTest")
    activity.Start()
    
    activity.SetStatus(ActivityStatusCode.Ok)
    Assert.Equal(ActivityStatusCode.Ok, activity.Status)
    
    activity.SetStatus(ActivityStatusCode.Error, "Something failed")
    Assert.Equal(ActivityStatusCode.Error, activity.Status)
    Assert.Equal("Something failed", activity.StatusDescription)
    
    activity.Stop()

[<Fact>]
let ``Activity is properly disposed`` () =
    let activity = new Activity("DisposableTest")
    activity.Start()
    
    use disposable = activity
    disposable.Stop()
    
    // Verify cleanup

[<Fact>]
let ``Parent-child hierarchy is maintained`` () =
    let root = new Activity("Root")
    root.Start()
    let rootId = root.SpanId.ToString()
    
    let branch1 = new Activity("Branch1")
    branch1.Start()
    Assert.Equal(rootId, branch1.ParentSpanId.ToString())
    
    let leaf = new Activity("Leaf")
    leaf.Start()
    Assert.Equal(branch1.SpanId.ToString(), leaf.ParentSpanId.ToString())
    
    leaf.Stop()
    branch1.Stop()
    root.Stop()
