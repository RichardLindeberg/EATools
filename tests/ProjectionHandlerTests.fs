module ProjectionHandlerTests

open System
open Xunit
open EATool.Domain
open EATool.Infrastructure
open EATool.Infrastructure.Projections

[<Fact>]
let ``application projection handles created event`` () =
    let tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".db")
    let connString = $"Data Source={tmp};Cache=Shared;Mode=ReadWriteCreate"
    let cfg = { DatabaseConfig.ConnectionString = connString; Environment = "test" }
    match Migrations.run cfg with
    | Error e -> Assert.True(false, e)
    | Ok () ->
        let handler = ApplicationProjection.Handler(connString) :> ProjectionEngine.IProjectionHandler<ApplicationEvent>
        
        let evt : EventEnvelope<ApplicationEvent> =
            {
                EventId = Guid.NewGuid()
                EventType = "ApplicationCreated"
                EventVersion = 1
                EventTimestamp = DateTime.UtcNow
                AggregateId = Guid.NewGuid()
                AggregateType = "Application"
                AggregateVersion = 1
                CausationId = None
                CorrelationId = None
                Actor = "test-user"
                ActorType = ActorType.User
                Source = Source.API
                Data = ApplicationCreated {
                    Id = "app-test123"
                    Name = "Test App"
                    Owner = Some "test-owner"
                    Lifecycle = "active"
                    CapabilityId = None
                    DataClassification = Some "internal"
                    Criticality = Some "medium"
                    Tags = ["test"; "demo"]
                    Description = Some "Test application"
                }
                Metadata = None
            }
        
        match handler.Handle(evt) with
        | Error e -> Assert.True(false, e)
        | Ok () ->
            // Verify app was written to database
            use conn = new Microsoft.Data.Sqlite.SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "SELECT name, owner FROM applications WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", "app-test123") |> ignore
            use reader = cmd.ExecuteReader()
            Assert.True(reader.Read(), "Application should exist in database")
            Assert.Equal("Test App", reader.GetString(0))
            Assert.Equal("test-owner", reader.GetString(1))

[<Fact>]
let ``application projection handles lifecycle transition`` () =
    let tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".db")
    let connString = $"Data Source={tmp};Cache=Shared;Mode=ReadWriteCreate"
    let cfg = { DatabaseConfig.ConnectionString = connString; Environment = "test" }
    match Migrations.run cfg with
    | Error e -> Assert.True(false, e)
    | Ok () ->
        let handler = ApplicationProjection.Handler(connString) :> ProjectionEngine.IProjectionHandler<ApplicationEvent>
        
        // Create initial app
        let createEvt : EventEnvelope<ApplicationEvent> =
            {
                EventId = Guid.NewGuid()
                EventType = "ApplicationCreated"
                EventVersion = 1
                EventTimestamp = DateTime.UtcNow
                AggregateId = Guid.NewGuid()
                AggregateType = "Application"
                AggregateVersion = 1
                CausationId = None
                CorrelationId = None
                Actor = "test-user"
                ActorType = ActorType.User
                Source = Source.API
                Data = ApplicationCreated {
                    Id = "app-lifecycle-test"
                    Name = "Lifecycle Test App"
                    Owner = Some "owner"
                    Lifecycle = "planned"
                    CapabilityId = None
                    DataClassification = None
                    Criticality = None
                    Tags = []
                    Description = None
                }
                Metadata = None
            }
        
        handler.Handle(createEvt) |> ignore
        
        // Transition lifecycle
        let transitionEvt : EventEnvelope<ApplicationEvent> =
            {
                EventId = Guid.NewGuid()
                EventType = "LifecycleTransitioned"
                EventVersion = 1
                EventTimestamp = DateTime.UtcNow
                AggregateId = Guid.NewGuid()
                AggregateType = "Application"
                AggregateVersion = 2
                CausationId = None
                CorrelationId = None
                Actor = "test-user"
                ActorType = ActorType.User
                Source = Source.API
                Data = LifecycleTransitioned {
                    Id = "app-lifecycle-test"
                    FromLifecycle = "planned"
                    ToLifecycle = "active"
                    SunsetDate = None
                }
                Metadata = None
            }
        
        match handler.Handle(transitionEvt) with
        | Error e -> Assert.True(false, e)
        | Ok () ->
            use conn = new Microsoft.Data.Sqlite.SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "SELECT lifecycle FROM applications WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", "app-lifecycle-test") |> ignore
            use reader = cmd.ExecuteReader()
            Assert.True(reader.Read())
            Assert.Equal("active", reader.GetString(0))

[<Fact>]
let ``organization projection handles created event`` () =
    let tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".db")
    let connString = $"Data Source={tmp};Cache=Shared;Mode=ReadWriteCreate"
    let cfg = { DatabaseConfig.ConnectionString = connString; Environment = "test" }
    match Migrations.run cfg with
    | Error e -> Assert.True(false, e)
    | Ok () ->
        let handler = OrganizationProjection.Handler(connString) :> ProjectionEngine.IProjectionHandler<OrganizationEvent>
        
        let evt : EventEnvelope<OrganizationEvent> =
            {
                EventId = Guid.NewGuid()
                EventType = "OrganizationCreated"
                EventVersion = 1
                EventTimestamp = DateTime.UtcNow
                AggregateId = Guid.NewGuid()
                AggregateType = "Organization"
                AggregateVersion = 1
                CausationId = None
                CorrelationId = None
                Actor = "test-user"
                ActorType = ActorType.User
                Source = Source.API
                Data = OrganizationCreated {
                    Id = "org-test123"
                    Name = "Test Org"
                    ParentId = None
                    Domains = ["example.com"]
                    Contacts = ["admin@example.com"]
                }
                Metadata = None
            }
        
        match handler.Handle(evt) with
        | Error e -> Assert.True(false, e)
        | Ok () ->
            use conn = new Microsoft.Data.Sqlite.SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "SELECT name FROM organizations WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", "org-test123") |> ignore
            use reader = cmd.ExecuteReader()
            Assert.True(reader.Read(), "Organization should exist in database")
            Assert.Equal("Test Org", reader.GetString(0))

[<Fact>]
let ``application projection is idempotent`` () =
    let tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".db")
    let connString = $"Data Source={tmp};Cache=Shared;Mode=ReadWriteCreate"
    let cfg = { DatabaseConfig.ConnectionString = connString; Environment = "test" }
    match Migrations.run cfg with
    | Error e -> Assert.True(false, e)
    | Ok () ->
        let handler = ApplicationProjection.Handler(connString) :> ProjectionEngine.IProjectionHandler<ApplicationEvent>
        
        let evt : EventEnvelope<ApplicationEvent> =
            {
                EventId = Guid.NewGuid()
                EventType = "ApplicationCreated"
                EventVersion = 1
                EventTimestamp = DateTime.UtcNow
                AggregateId = Guid.NewGuid()
                AggregateType = "Application"
                AggregateVersion = 1
                CausationId = None
                CorrelationId = None
                Actor = "test-user"
                ActorType = ActorType.User
                Source = Source.API
                Data = ApplicationCreated {
                    Id = "app-idempotent"
                    Name = "Idempotent App"
                    Owner = None
                    Lifecycle = "active"
                    CapabilityId = None
                    DataClassification = None
                    Criticality = None
                    Tags = []
                    Description = None
                }
                Metadata = None
            }
        
        // Process same event twice
        handler.Handle(evt) |> ignore
        handler.Handle(evt) |> ignore
        
        // Should only have one record
        use conn = new Microsoft.Data.Sqlite.SqliteConnection(connString)
        conn.Open()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT COUNT(*) FROM applications WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", "app-idempotent") |> ignore
        let count = cmd.ExecuteScalar() :?> int64
        Assert.Equal(1L, count)
