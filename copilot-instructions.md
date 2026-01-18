# Copilot Instructions for EATool

## Backlog Workflow & Agent Prompt

All agents must follow this when working with the backlog.

### Workflow Rules
- Status lifecycle: 🟢 Ready → 🟡 In Progress → ✅ Done.
- Never create new files in `backlog/old/`. Always move the existing item file there when marking it ✅ Done.
- Keep only one live file per item. Avoid duplicates (e.g., Ready and Done copies in `backlog/`).
- After any status change, update `backlog/INDEX.md` totals, sections, and navigation to reflect the new state.
- File naming pattern: `Item-{id}-Prio-{Pn}-{STATUS}.md` where STATUS ∈ {🟢 Ready, 🟡 In Progress, 🔴 Blocked, ✅ Done}.

### Required Actions per Step
1. Create new item:
    - Add the file in `backlog/` with status 🟢 Ready.
    - Add a link in `backlog/INDEX.md` under the correct priority section.
2. Start work:
    - Update the same file’s status to 🟡 In Progress (do not move).
    - Reflect the change in `backlog/INDEX.md` status counts and “Currently In Progress” list.
3. Complete work:
    - Update the file’s status to ✅ Done.
    - Move the file from `backlog/` to `backlog/old/` (do not create a new file).
    - Remove the item from Active sections; add it to the Completed totals in `backlog/INDEX.md`.

### Agent Prompt (use verbatim)
"When editing the backlog:
- Do not create new files under backlog/old; move completed items there.
- Follow status lifecycle: Ready → In Progress → Done.
- On completion: update status to Done, move the file to backlog/old, and update backlog/INDEX.md counts and sections.
- Ensure only one status file exists per item in backlog.
- Keep links and navigation in backlog/INDEX.md accurate after changes."

## Project Overview

EATool is an Enterprise Architecture management tool built with F# .NET 10, using:
- **Architecture**: Event Sourcing + CQRS
- **Framework**: Giraffe (F# HTTP framework)
- **Database**: SQLite with event store and projections
- **Testing**: pytest integration tests (Python)
- **API**: RESTful with OpenAPI 3.0.3 documentation

## Core Architectural Patterns

### Event Sourcing & CQRS

All domain entities follow event-sourced command-based architecture:

1. **Commands** define user intent (e.g., `CreateCapability`, `SetParent`)
2. **Events** represent facts that happened (e.g., `CapabilityCreated`, `CapabilityParentAssigned`)
3. **Aggregates** maintain state through discriminated unions:
   ```fsharp
   type EntityAggregate =
       | Initial
       | Active of EntityState
       | Deleted
   ```
4. **Command Handlers** validate business rules and generate events
5. **Projections** update read models from events
6. **Event Store** persists events with metadata (actor, correlation, causation)

### File Organization

Domain-driven structure with F# compilation order dependency:

```
src/
  Domain/
    Types.fs              # Shared types and enums
    Models.fs             # Domain models (read models)
    EntityCommands.fs     # Commands, events, aggregates
    EntityCommandHandler.fs  # Business logic and validation
  
  Infrastructure/
    Database.fs           # SQLite connection
    Migrations.fs         # Database migrations
    EventStore.fs         # Event persistence
    ProjectionEngine.fs   # Event projection framework
    EntityEventJson.fs    # Event serialization
    EntityRepository.fs   # CRUD for read models
    Json.fs              # JSON encoding/decoding
    Projections/
      EntityProjection.fs  # Projection handlers
  
  Api/
    EntityEndpoints.fs    # HTTP endpoints
    Endpoints.fs         # Route composition

  Program.fs            # Application entry point
```

**Critical**: Files must be ordered in `EATool.fsproj` respecting dependencies (Domain → Infrastructure → Api).

## API Endpoint Patterns

### Command-Based Endpoints

Use POST for all commands, even destructive operations:

```
POST /entities                           # Create (returns 201 + Location header)
GET  /entities                           # List with pagination
GET  /entities/{id}                      # Get single entity
POST /entities/{id}/commands/action      # Execute command (returns 200 + updated entity)
POST /entities/{id}/commands/delete      # Delete (returns 204)
```

**Examples**:
- `POST /organizations/{id}/commands/set-parent`
- `POST /business-capabilities/{id}/commands/update-description`
- `POST /applications/{id}/commands/set-classification`

### Endpoint Implementation Pattern

```fsharp
POST >=> routef "/entities/%s/commands/action" (fun id next ctx -> task {
    let! bodyStr = ctx.ReadBodyFromRequestAsync()
    
    // 1. Parse request
    match Decode.fromString decoder bodyStr with
    | Error err ->
        ctx.SetStatusCode 400
        let errJson = Json.encodeErrorResponse "validation_error" $"JSON parse error: {err}"
        return! (Giraffe.Core.json errJson) next ctx
    | Ok req ->
        let cmd = { Id = id; /* ... */ }
        
        // 2. Load aggregate state
        let eventStore = createEventStore()
        let projectionEngine = createProjectionEngine eventStore
        let state, baseVersion, aggregateGuid = loadAggregateState eventStore id
        
        // 3. Validate state exists
        match state with
        | EntityAggregate.Initial | EntityAggregate.Deleted ->
            ctx.SetStatusCode 404
            let errJson = Json.encodeErrorResponse "not_found" "Entity not found"
            return! (Giraffe.Core.json errJson) next ctx
        | EntityAggregate.Active _ ->
            // 4. Execute command handler
            match EntityCommandHandler.handleAction state cmd with
            | Error err ->
                ctx.SetStatusCode 400
                let errJson = Json.encodeErrorResponse "business_rule_violation" err
                return! (Giraffe.Core.json errJson) next ctx
            | Ok events ->
                // 5. Persist events and project
                let meta = getActorMetadata ctx
                match persistAndProject eventStore projectionEngine id aggregateGuid baseVersion meta events with
                | Error err ->
                    ctx.SetStatusCode 500
                    let errJson = Json.encodeErrorResponse "event_store_error" err
                    return! (Giraffe.Core.json errJson) next ctx
                | Ok _ ->
                    // 6. Return updated entity from projection
                    match EntityRepository.getById id with
                    | Some entity ->
                        let json = Json.encodeEntity entity
                        return! (Giraffe.Core.json json) next ctx
                    | None ->
                        ctx.SetStatusCode 404
                        let errJson = Json.encodeErrorResponse "not_found" "Entity not found"
                        return! (Giraffe.Core.json errJson) next ctx
})
```

### HTTP Status Codes

- `200 OK` - Successful command execution with entity response
- `201 Created` - Entity created (+ `Location` header)
- `204 No Content` - Successful delete
- `400 Bad Request` - Validation error or business rule violation
- `404 Not Found` - Entity not found
- `422 Unprocessable Entity` - Semantic validation error
- `500 Internal Server Error` - Event store or system error

### Error Response Format

```json
{
  "error": "error_type",
  "message": "Human-readable description"
}
```

Error types: `validation_error`, `business_rule_violation`, `not_found`, `event_store_error`

## Business Rules & Validation

### Cycle Detection

For hierarchical entities (Organizations, BusinessCapabilities), always validate parent relationships:

```fsharp
let rec checkForCycles (entityId: string) (parentId: string) (getEntity: string -> Entity option) : bool =
    if entityId = parentId then true  // Direct self-reference
    else
        match getEntity parentId with
        | None -> false  // Parent doesn't exist (handled separately)
        | Some parent ->
            match parent.ParentId with
            | None -> false  // Reached root
            | Some grandparentId -> checkForCycles entityId grandparentId getEntity
```

### Validation Order

1. **Parse** - JSON structure valid?
2. **State Check** - Does entity exist? Is it deleted?
3. **Field Validation** - Required fields present? Format valid?
4. **Reference Check** - Do referenced entities exist?
5. **Business Rules** - Cycles? Uniqueness? Domain constraints?

## Naming Conventions

### Event Naming

Prefix events with entity name to avoid conflicts across domains:

```fsharp
// ✅ Good
type BusinessCapabilityEvent =
    | CapabilityCreated of ...
    | CapabilityParentAssigned of ...
    | CapabilityDeleted of ...

type OrganizationEvent =
    | OrganizationCreated of ...
    | ParentAssigned of ...        // Different domain, clearer context
    | OrganizationDeleted of ...

// ❌ Bad - Ambiguous across domains
type BusinessCapabilityEvent =
    | Created of ...
    | ParentAssigned of ...
```

### ID Prefixes

Use entity-specific prefixes for generated IDs:
- `org-` for organizations
- `cap-` for business capabilities
- `app-` for applications
- `srv-` for servers
- `int-` for integrations
- `de-` for data entities

### Field Naming

- **F# Types**: `PascalCase` (e.g., `ParentId`, `CreatedAt`)
- **JSON API**: `snake_case` (e.g., `parent_id`, `created_at`)
- **Database**: `snake_case` (e.g., `parent_id`, `created_at`)

## F# Code Standards

### Code Organization

**Prefer namespaces over modules at the top level**:

```fsharp
// ✅ Good - Consumable from C# without issues
namespace EATool.Domain

type BusinessCapability = {
    Id: string
    Name: string
}

// ❌ Avoid - Appears as static class, requires `using static` in C#
module MyCode

type MyClass() = ...
```

**Sort `open` statements topologically**:
- Group by layers of your system (System → External → Internal)
- Within each layer, sort alphabetically
- Separate layers with blank lines

```fsharp
open System
open System.Collections.Generic
open System.IO

open Giraffe
open Thoth.Json.Net

open EATool.Domain
open EATool.Infrastructure
```

### Discriminated Unions & Enums

**Always use `[<RequireQualifiedAccess>]`** for discriminated unions and enums to:
- Prevent naming conflicts across modules
- Improve code readability with explicit qualification
- Avoid ambiguity when pattern matching

```fsharp
// ✅ Good - Requires qualification
[<RequireQualifiedAccess>]
type Lifecycle =
    | Planning
    | Development
    | Production
    | Deprecated
    | Retired

// Usage: Lifecycle.Production, Lifecycle.Retired
match app.Lifecycle with
| Lifecycle.Production -> "active"
| Lifecycle.Retired -> "archived"
| _ -> "other"

// ✅ Good - Event unions should also be qualified
[<RequireQualifiedAccess>]
type BusinessCapabilityEvent =
    | CapabilityCreated of CapabilityCreatedData
    | CapabilityParentAssigned of CapabilityParentAssignedData
    | CapabilityDeleted of CapabilityDeletedData

// ❌ Bad - Unqualified types can conflict
type Lifecycle =
    | Planning  // Could conflict with other Planning types
    | Production
```

### Classes vs Modules for Side Effects

**Use classes to contain values with side effects**:

```fsharp
// ❌ Bad - Side effects at static initialization
module MyApi =
    let dep1 = File.ReadAllText "/path/to/config.txt"  // Side effect!
    let dep2 = Environment.GetEnvironmentVariable "DEP_2"
    let private r = Random()  // Not thread-safe
    let dep3() = r.Next()

// ✅ Good - Dependencies injected, testable, thread-safe
type MyApi(dep1, dep2, dep3) =
    member _.Function1 arg1 = doStuffWith dep1 dep2 dep3 arg1
    member _.Function2 arg2 = doStuffWith dep1 dep2 dep3 arg2
```

Benefits:
- Push state outside the API
- Configuration external to module
- Errors don't manifest as `TypeInitializationException`
- Easier to test and reason about

### Mutation and Immutability

**Wrap mutable code in immutable interfaces**:

```fsharp
// ✅ Good - Mutation encapsulated, referentially transparent
let inline contains value (array:'T[]) =
    checkNonNull "array" array
    let mutable state = false
    let mutable i = 0
    while not state && i < array.Length do
        state <- value = array[i]
        i <- i + 1
    state
```

**Prefer `let mutable` to `ref`**:

```fsharp
// ❌ Avoid - Verbose dereferencing
let kernels =
    let acc = ref Set.empty
    processWorkList startKernels (fun kernel ->
        if not ((!acc).Contains(kernel)) then
            acc := (!acc).Add(kernel)
        ...)
    !acc |> Seq.toList

// ✅ Good - Cleaner syntax
let kernels =
    let mutable acc = Set.empty
    processWorkList startKernels (fun kernel ->
        if not (acc.Contains(kernel)) then
            acc <- acc.Add(kernel)
        ...)
    acc |> Seq.toList
```

**Keep mutable scope minimal**:

```fsharp
let data =
    [
        let mutable completed = false
        while not completed do
            logic ()
            if someCondition then
                completed <- true
    ]
// 'completed' not accessible outside this scope
```

### Type Inference and Generics

**Label argument names with explicit types in public APIs**:

```fsharp
// ✅ Good - Explicit types for public API
let sendData (data: byte[]) (bufferSize: int) (timeout: TimeSpan) : Result<unit, string> =
    ...

// ❌ Avoid in public APIs - Relies on type inference
let sendData data bufferSize timeout =
    ...
```

**Use meaningful names for generic type parameters**:

```fsharp
// ✅ Good - Clear intent
type DocumentStore<'Document> =
    abstract member Save : 'Document -> unit
    abstract member Load : string -> 'Document option

// ❌ Avoid - Generic 'T tells nothing about usage
type Store<'T> =
    abstract member Save : 'T -> unit
```

**Use PascalCase for generic type parameters**:

```fsharp
// ✅ Good
type Repository<'TEntity, 'TKey> = ...

// ❌ Avoid
type Repository<'t_entity, 't_key> = ...
```

### Performance Considerations

**Use structs for small types with high allocation rates**:

Guidelines:
- Size ≤ 16 bytes → consider struct
- Many instances in memory → likely benefit from struct
- Always measure with benchmarking tools like BenchmarkDotNet

```fsharp
// For small value types with high allocation
[<Struct>]
type Point = { X: float; Y: float; Z: float }

[<Struct>]
type SName = SName of string  // Single-case DU wrapper

// Struct tuples for grouping small values
let rec runWithStructTuple t offset times =
    let offsetValues x y z offset =
        struct(x + offset, y + offset, z + offset)  // Struct tuple
    if times <= 0 then t
    else
        let struct(x, y, z) = t
        let r = offsetValues x y z offset
        runWithStructTuple r offset (times - 1)
```

**Note**: Structs aren't always faster - copying large structs can be slower than reference types. Always benchmark!

### Null Handling

**Avoid nulls in F# code**:

```fsharp
// ❌ Avoid - Pollutes type system
[<AllowNullLiteral>]
type MyType() = ...

// ❌ Avoid - Creates null/zero values
let defaultValue = Unchecked.defaultof<MyType>

// ✅ Good - Use Option for optional values
type MyType = {
    Name: string
    Description: string option  // Explicitly optional
}
```

**Check for null at API boundaries, raise early**:

```fsharp
let inline checkNonNull argName arg =
    if isNull arg then
        nullArg argName

module Array =
    let contains value (array:'T[]) =
        checkNonNull "array" array
        // ... rest of implementation
```

**Use F# 9 null syntax at boundaries only**:

```fsharp
// ✅ Good - At API boundary
type CustomType(m1, m2) =
    override this.Equals(obj: obj | null) =
        match obj with
        | :? CustomType as other -> this.M1 = other.M1 && this.M2 = other.M2
        | _ -> false

// ✅ Good - Convert to Option immediately
let getLineFromStream (stream: System.IO.StreamReader) =
    stream.ReadLine() |> Option.ofObj  // Not: string | null

// Special null checking functions that shadow with safe values
let inline processNullableList list =
   let list = nullArgCheck (nameof list) list  // Throws if null
   // 'list' guaranteed non-null from here
   list |> List.distinct
```

### Object-Oriented Features

**Prefer composition over inheritance**:

```fsharp
// ✅ Good - Composition
type EmailService(smtpClient: ISmtpClient, logger: ILogger) =
    member _.SendEmail(to: string, body: string) =
        logger.Log("Sending email")
        smtpClient.Send(to, body)

// ❌ Avoid - Deep inheritance hierarchies
type BaseService() =
    abstract member DoWork : unit -> unit

type EmailService() =
    inherit BaseService()
    override _.DoWork() = ...
```

**Use object expressions for interfaces when you don't need a class**:

```fsharp
// ✅ Good - Quick interface implementation
let createProvider () =
    { new ICodeActionProvider with
        member _.provideCodeActions(doc, range, context, ct) =
            // Implementation here
            [||] |> ResizeArray
    }
```

**Use these OO features judiciously**:
- ✅ Dot notation, instance members, implicit constructors
- ✅ Static members, indexers, slicing notation
- ✅ Named and optional arguments
- ✅ Interfaces and implementations
- ⚠️ Use carefully: Method overloading, auto properties, events
- ❌ Generally avoid: Implementation inheritance, nulls, deep hierarchies

### Exception Handling

**Represent domain errors with types**:

```fsharp
// ✅ Good - Errors part of domain model
type MoneyWithdrawalResult =
    | Success of amount:decimal
    | InsufficientFunds of balance:decimal
    | CardExpired of DateTime
    | UndisclosedFailure

let handleWithdrawal amount =
    let w = withdrawMoney amount
    match w with
    | Success am -> printfn $"Successfully withdrew %f{am}"
    | InsufficientFunds balance -> printfn $"Failed: balance is %f{balance}"
    | CardExpired expiredDate -> printfn $"Failed: card expired on {expiredDate}"
    | UndisclosedFailure -> printfn "Failed: unknown"
```

**Use specific exception helpers in order of preference**:

```fsharp
// 1. Most specific
nullArg "argumentName"  // ArgumentNullException
invalidArg "argumentName" "message"  // ArgumentException
invalidOp "message"  // InvalidOperationException

// 2. Specific exception types
raise (FileNotFoundException("File not found"))

// 3. Avoid generic exceptions
failwith "message"  // Raises generic Exception
```

**Don't replace exceptions with stringly-typed Results**:

```fsharp
// ❌ Bad - Loses diagnostic info, forces string parsing
let tryReadAllText (path : string) =
    try System.IO.File.ReadAllText path |> Ok
    with e -> Error e.Message  // Caller must parse strings!

// ✅ Good - Specific errors with semantic meaning
let tryReadAllTextIfPresent (path : string) =
    try System.IO.File.ReadAllText path |> Some
    with :? FileNotFoundException -> None  // Specific case handled
```

**When to use Result vs exceptions**:
- Use `Result<'Success, 'Error>` for expected error cases in your domain
- Use exceptions for unexpected errors and system failures
- Don't nest Results deeply or create "stringly typed" errors
- Exceptions include diagnostic info, work across .NET languages, and are well-understood by the runtime

### Type Abbreviations

**Avoid using type abbreviations to represent domain concepts**:

```fsharp
// ❌ Bad - Not an abstraction, just an alias
type BufferSize = int

let send data (bufferSize: BufferSize) = ...
// Can still pass any int - no type safety!

// ✅ Good - Single-case DU provides type safety
type BufferSize = BufferSize of int

let send data (BufferSize size) = ...
// Caller must construct BufferSize, can't pass raw int
```

**Type abbreviations are good for function signatures**:

```fsharp
// ✅ Good - Simplifies complex signatures
type Computation = DeviceDescriptor -> Variable -> Function

let runComputation (comp: Computation) = ...
```

### Access Control

For widely consumed libraries:
- Prefer non-`public` until needed publicly
- Keep helper functions `private`
- Consider `[<AutoOpen>]` for private helper modules
- Minimize public surface area to reduce coupling

## F# Code Formatting

Follow Microsoft's official formatting guidelines. Use **Fantomas** code formatter with agreed-upon settings checked into the repository.

### Indentation
- **4 spaces per indentation level** (never tabs)
- F# compiler errors on tabs outside strings/comments
- Be consistent throughout codebase

### Avoid Name-Length-Sensitive Formatting
```fsharp
// ✅ Good - not sensitive to name length
let myLongValueName =
    someExpression
    |> anotherExpression

// ❌ Bad - breaks if name changes
let myLongValueName = someExpression
                      |> anotherExpression
```

### Comments
- Prefer `//` over `(* *)` block comments
- Capitalize first letter, use well-formed sentences

```fsharp
// ✅ Good comment style.
let f x = x + 1 // Increment by one.

// ❌ bad comment
let f x = x + 1 // plus one
```

### Function Application
- Omit parentheses unless required
- Add space before tupled/parenthesized arguments (lowercase functions)
- No space for capitalized methods (fluent programming)

```fsharp
// ✅ Good
someFunction1 x.IngredientName x.Quantity
someFunction1 (convertVolume x)
String.Format(x.Name, x.Quantity)

// ❌ Bad
someFunction1 (x.IngredientName)
someFunction2()
String.Format (x.Name, x.Quantity)
```

### Pipelines
- Pipeline operators `|>` go underneath expressions on new lines

```fsharp
// ✅ Good
let methods =
    System.AppDomain.CurrentDomain.GetAssemblies()
    |> List.ofArray
    |> List.map (fun assm -> assm.GetTypes())
    |> Array.concat

// ❌ Bad - alignment sensitive
let methods = System.AppDomain.CurrentDomain.GetAssemblies()
            |> List.ofArray
```

### Lambda Expressions
- Place lambda body on new line when followed by other arguments
- If lambda is last argument, args until arrow on same line

```fsharp
// ✅ Good
List.iter
    (fun elem ->
         printfn $"Value: %d{a + elem}")
    list1

// ✅ Good - in pipeline
list1
|> List.map (fun x -> x + 1)
|> List.iter (fun elem ->
    printfn $"Value: %d{elem}")
```

### Operators
- Use white space around binary operators: `x + y`
- Unary `-` immediately precedes value: `-x` not `- x`
- Range `..` only has spaces if expression non-atomic

```fsharp
// ✅ Good
let a = [ 2..7 ]
let b = [ 0.7 .. 9.2 ]
```

### If Expressions
- One line if short and simple
- Multi-line for imperative if (missing else)

```fsharp
// ✅ Good
if cond then e1 else e2

// ✅ Good - imperative
if a then
    ()

// ✅ Good - multi-line
if cond1 then
    let e1 = something()
    e1
elif cond2 then
    e2
```

### Lists and Arrays
- Use spaces: `[ 1; 2; 3 ]` and `[| 1; 2; 3 |]`
- Prefer `->` over `do ... yield`

```fsharp
// ✅ Good
let squares = [ for x in 1..10 -> x * x ]
```

### Record Expressions
- Three styles: Cramped (default), Aligned, Stroustrup
- Short records one line: `{ X = 1.0; Y = 0.0 }`

```fsharp
// ✅ Good - Cramped
let rainbow = 
    { Boss = "Jeffrey"
      Lackeys = [ "Zippy" ] }

// ✅ Good - Aligned
let rainbow =
    {
        Boss = "Jeffrey"
        Lackeys = [ "Zippy" ]
    }

// ✅ Good - Stroustrup
let rainbow = {
    Boss = "Jeffrey"
    Lackeys = [ "Zippy" ]
}
```

### Pattern Matching
- Use `|` for each clause with no indentation
- No space before opening parenthesis in patterns

```fsharp
// ✅ Good
match l with
| Some(y) -> y
| None -> 0
```

### Blank Lines
- Single blank line between top-level functions/classes
- Blank line before XML doc comments

### Function/Member Declarations
- Right-hand side on same line or new line indented one level
- Long parameters on new lines, indented consistently

```fsharp
// ✅ Good
let longFunction
    (aVeryLongParam: LongType)
    (aSecondLongParam: LongType)
    =
    // body
```

### Record Type Declarations
- Don't use `with`/`end` for additional members (except Stroustrup style)

```fsharp
// ✅ Good - default
type PostalAddress =
    { Address: string
      City: string }
    member x.FullAddress = $"{x.Address}, {x.City}"

// ✅ Good - Stroustrup with members
type PostalAddress = {
    Address: string
    City: string
} with
    member x.FullAddress = $"{x.Address}, {x.City}"
```

### Discriminated Union Declarations
- Indent `|` by four spaces
- Can omit leading `|` for single short union
- Empty line before `///` comments

```fsharp
// ✅ Good
type Volume =
    | Liter of float
    | FluidOunce of float

// ✅ Good - single case
type Address = Address of string
```

### Type Annotations
- Prefer prefix syntax: `List<int>` not `int list`
- Exceptions: `int list`, `int option`, `int voption`, `int array`, `int ref`, `int seq`
- White space around `->` in function types
- White space after `:` (not before) in value annotations

```fsharp
// ✅ Good
type MyFun = int -> int -> string
let complexFunction (a: int) (b: int) c = a + b + c
```

## OpenAPI Documentation

### Keep openapi.yaml Synchronized

**Always update openapi.yaml when**:
- Adding/removing/changing endpoints
- Adding/modifying request/response schemas
- Changing field names or types
- Adding command endpoints

### Schema Definitions

Match F# domain models exactly:

```yaml
BusinessCapability:
  type: object
  properties:
    id: { type: string }
    name: { type: string }
    parent_id: { type: string }
    description: { type: string, nullable: true }
    created_at: { type: string, format: date-time }
    updated_at: { type: string, format: date-time }
  required: [id, name, created_at, updated_at]
```

### Command Endpoint Documentation

Document command endpoints with:
- Clear description of what the command does
- Which event(s) it generates
- Business rules enforced
- Cycle detection behavior (if applicable)

```yaml
/business-capabilities/{id}/commands/set-parent:
  post:
    summary: Set business capability parent with cycle detection
    description: |
      Assigns a parent business capability to this capability.
      Validates that no cycles are created in the hierarchy (A→B→C→A).
      Uses event sourcing with CapabilityParentAssigned event.
    parameters:
      - $ref: '#/components/parameters/idPath'
    requestBody:
      required: true
      content:
        application/json:
          schema:
            type: object
            required: [parent_id]
            properties:
              parent_id:
                type: string
                description: ID of the parent business capability
```

## Testing Standards

### Integration Tests

Write pytest integration tests for:
- ✅ All CRUD operations (list, create, get, update, delete)
- ✅ All command endpoints
- ✅ Validation errors (missing fields, invalid data)
- ✅ Business rule violations (cycles, invalid references)
- ✅ Edge cases (nonexistent entities, empty lists)

### Test Organization

```python
class TestEntityName:
    def test_list_entities(self, client):
        """GET /entities should return paginated list."""
        
    def test_create_entity(self, client):
        """POST /entities should create entity with 201."""
        
    def test_get_entity(self, client):
        """GET /entities/{id} should return entity details."""
        
    def test_command_name(self, client):
        """POST /entities/{id}/commands/action should execute command."""
        
    def test_validation_error(self, client):
        """Should return 400 for invalid input."""
```

### Test Expectations

- **All tests must pass** before committing changes
- Target: **90+ tests** for comprehensive coverage
- Test both happy paths and error cases
- Verify HTTP status codes explicitly
- Clean up test data (delete created entities)

## Git Workflow

### Branch Strategy

- `main` - stable production code
- `feature/entity-name-commands` - feature branches for command migrations
- Merge feature branches after all tests pass

### Commit Messages

Use descriptive multi-line commit messages:

```
Item-XXX: Short summary (50 chars)

- Detailed bullet points of what changed
- Include file names for major changes
- Mention test results (e.g., "All 91 tests passing")
- Reference business rules implemented
- Note any breaking changes
```

**Example**:
```
Item-033: Complete business capability command migration

- Replaced old CRUD endpoints with command-based architecture
- Endpoints now follow event sourcing pattern (POST /business-capabilities/{id}/commands/*)
- Implemented endpoints:
  * POST /business-capabilities - create capability with optional parent and description
  * POST /business-capabilities/{id}/commands/set-parent - set parent with cycle detection
  * POST /business-capabilities/{id}/commands/remove-parent - remove parent
  * POST /business-capabilities/{id}/commands/update-description - update description
  * POST /business-capabilities/{id}/commands/delete - delete capability
- Updated openapi.yaml to document new command endpoints
- Added description field to BusinessCapability schema
- Fixed JSON encoding to include description field in responses
- Updated integration tests for new endpoints
- All 91 integration tests passing
```

### Backlog Management

Track work items in `backlog/` directory:
- `Item-XXX-Prio-P1-🟢 Ready.md` - Ready to start
- `Item-XXX-Prio-P1-⏳ In Progress.md` - Currently working
- `Item-XXX-Prio-P1-✅ Done.md` - Completed

Rename files to reflect status changes and commit renames.

## Database Migrations

### Migration Files

Create sequential numbered migrations in `src/Infrastructure/Migrations/`:

```sql
-- 008_add_business_capability_description.sql
ALTER TABLE business_capabilities ADD COLUMN description TEXT NULL;
```

### Migration Naming

Format: `NNN_action_description.sql`
- `001` - `099`: Core schema
- `100` - `199`: Event sourcing tables
- `200+`: Feature additions

### Migration Rules

- **Never modify existing migrations** - create new ones
- Include rollback strategy in comments if complex
- Test migrations on clean database
- Update repository and model when adding fields

## Common Patterns

### Loading Aggregate State

```fsharp
let private loadAggregateState (eventStore: IEventStore<EntityEvent>) (aggregateId: string) =
    let aggregateGuid = parseAggregateId aggregateId
    let existingEvents = eventStore.GetEvents aggregateGuid
    let baseVersion = eventStore.GetAggregateVersion aggregateGuid
    let stateFromEvents = existingEvents |> List.fold (fun acc e -> EntityAggregate.apply acc e.Data) EntityAggregate.Initial
    
    let state =
        if existingEvents |> List.isEmpty then
            // Fallback: load from projection for backwards compatibility
            match EntityRepository.getById aggregateId with
            | Some entity ->
                let entityState: EntityState = {
                    Id = entity.Id
                    Name = entity.Name
                    // ... map all fields
                }
                EntityAggregate.Active entityState
            | None -> EntityAggregate.Initial
        else stateFromEvents
    
    state, baseVersion, aggregateGuid
```

### Persist and Project Events

```fsharp
let private persistAndProject 
    (eventStore: IEventStore<EntityEvent>) 
    (projectionEngine: ProjectionEngine<EntityEvent>) 
    (aggregateId: string) 
    (aggregateGuid: Guid) 
    (baseVersion: int) 
    (meta: string * ActorType * Guid * Guid) 
    (events: EntityEvent list) =
    
    let envelopes =
        events
        |> List.mapi (fun i event -> 
            createEventEnvelope aggregateId aggregateGuid (baseVersion + i + 1) event meta)
    
    match eventStore.Append envelopes with
    | Error err -> Error err
    | Ok () ->
        let projectionResult =
            envelopes
            |> List.fold (fun acc env ->
                match acc with
                | Error _ -> acc
                | Ok () -> projectionEngine.ProcessEvent(env)) (Ok ())
        
        match projectionResult with
        | Ok () -> Ok envelopes
        | Error e -> Error e
```

## When Adding New Domain Entity

1. **Create domain models** (`EntityCommands.fs`):
   - Commands (discriminated union)
   - Events (discriminated union)
   - Aggregate (Initial | Active | Deleted)
   - Event data records

2. **Implement command handler** (`EntityCommandHandler.fs`):
   - Validation functions
   - Command handlers returning `Result<Event list, string>`
   - Business rule enforcement

3. **Add event serialization** (`EntityEventJson.fs`):
   - Encode/decode functions for each event
   - Type field for discrimination

4. **Create projection** (`Projections/EntityProjection.fs`):
   - Implement `IProjectionHandler<EntityEvent>`
   - Handle each event type
   - Update read model tables

5. **Add repository** (`EntityRepository.fs`):
   - CRUD functions for read model
   - Map database rows to domain models

6. **Create migration** (`Migrations/NNN_create_entities.sql`):
   - Table schema with id, timestamps, foreign keys
   - Indexes for common queries

7. **Implement endpoints** (`Api/EntityEndpoints.fs`):
   - List (GET /entities)
   - Create (POST /entities)
   - Get (GET /entities/{id})
   - Commands (POST /entities/{id}/commands/*)

8. **Update openapi.yaml**:
   - Path definitions
   - Schema definitions
   - Request/response examples

9. **Write integration tests** (`tests/integration/test_entities.py`):
   - All CRUD operations
   - All command endpoints
   - Error cases

10. **Update EATool.fsproj**:
    - Add new files in correct compilation order

## Code Review Checklist

Before committing:

- [ ] All tests pass (run `python -m pytest tests/`)
- [ ] Build succeeds (run `dotnet build src/EATool.fsproj`)
- [ ] OpenAPI documentation updated
- [ ] JSON encoders/decoders include all fields
- [ ] Event naming avoids conflicts across domains
- [ ] Cycle detection for hierarchical entities
- [ ] Error handling for all failure paths
- [ ] HTTP status codes appropriate
- [ ] Backlog item status updated
- [ ] Commit message descriptive and detailed

## Development Commands

```bash
# Build
dotnet build src/EATool.fsproj

# Run server (development)
dotnet run --project src/EATool.fsproj

# Run tests
python -m pytest tests/ -v

# Run specific test file
python -m pytest tests/integration/test_entities.py -v

# Check git status
git status

# Commit changes
git add -A
git commit -m "Item-XXX: Summary

- Detailed changes
- Test results"

# View git log
git log --oneline --graph --all -20
```

## Anti-Patterns to Avoid

### ❌ DON'T
- Use PATCH/PUT for updates - use POST commands instead
- Use DELETE for deletions - use POST /commands/delete instead
- Modify existing migrations - create new ones
- Put business logic in endpoints - keep it in command handlers
- Skip cycle detection for hierarchical entities
- Forget to update openapi.yaml
- Commit without running tests
- Use generic event names that conflict across domains
- Hardcode entity IDs - generate them
- Skip error handling in endpoints

### ✅ DO
- Use command-based architecture for all mutations
- Validate business rules in command handlers
- Return appropriate HTTP status codes
- Keep openapi.yaml in sync with implementation
- Write integration tests for all endpoints
- Use event sourcing for audit trail
- Include description fields in JSON responses
- Follow F# compilation order in .fsproj
- Check for cycles in parent hierarchies
- Generate unique IDs with entity prefixes

## Additional Resources

- **F# Documentation**: https://fsharp.org/
- **Giraffe Framework**: https://github.com/giraffe-fsharp/Giraffe
- **Event Sourcing**: https://martinfowler.com/eaaDev/EventSourcing.html
- **CQRS Pattern**: https://martinfowler.com/bliki/CQRS.html
- **OpenAPI Specification**: https://swagger.io/specification/

---

*Last Updated: January 7, 2026*
*Project: EATool - Enterprise Architecture Tool*
