# Item-048: Fix F# Code Standards Violations in Backend

**Status:** 🟢 Ready  
**Priority:** P2 - MEDIUM  
**Effort:** 8-12 hours  
**Created:** 2026-01-07  
**Owner:** TBD

---

## Problem Statement

Audit of backend F# code against [copilot-instructions.md](../../copilot-instructions.md) revealed multiple violations of established F# best practices. These violations reduce code maintainability, readability, and consistency. The codebase does not fully comply with Microsoft's official F# formatting guidelines and our project standards.

---

## Violations Found

### 1. Missing `[<RequireQualifiedAccess>]` on Discriminated Unions

**Standard (Line 261-283)**: All DUs and enums MUST use `[<RequireQualifiedAccess>]` to prevent naming conflicts and improve readability.

**Violations in `src/Domain/Types.fs`:**
- Line 10: `type ActorType` — Missing attribute
- Line 16: `type Source` — Missing attribute
- Line 24: `type EntityType` — Missing attribute
- Line 36: `type Lifecycle` — Missing attribute
- Line 43: `type DataClassification` — Missing attribute
- Line 50: `type RelationType` — Missing attribute (67 lines of DU cases)
- Line 70: `type ArchiMateRelationship` — Missing attribute
- Line 84: `type JobStatus` — Missing attribute
- Line 91: `type InterfaceStatus` — Missing attribute

**Impact**: Code can have naming conflicts (e.g., multiple `Active` cases from different DUs). Readability suffers when consumers don't see `RelationType.DependsOn` qualification.

**Fix**: Add `[<RequireQualifiedAccess>]` attribute before each DU/enum definition.

**Note**: `RelationCommands.fs` line 111 correctly uses it for `RelationAggregate` - follow that pattern.

---

### 2. Type Abbreviation Instead of Single-Case DU

**Standard (Line 589-603)**: Type abbreviations (e.g., `type UtcTimestamp = string`) reduce type safety. Use single-case DUs instead to prevent accidentally passing raw types.

**Violation in `src/Domain/Types.fs`:**
- Line 7: `type UtcTimestamp = string`
  - Used throughout Models and endpoints
  - Provides no type safety — callers can pass any string
  - Should be: `type UtcTimestamp = UtcTimestamp of string`

**Impact**: Loss of type safety; easier to misuse; less self-documenting.

**Fix**: Convert to single-case DU and update all usages (Models.fs, Repositories, Endpoints, JSON serialization).

**Estimated Impact**: 50-100 usages across Domain and Infrastructure layers.

---

### 3. Mutable Module-Level State for Dependency Storage

**Standard (Line 297-319)**: Side effects at static initialization (mutable module-level values, Random instances, file I/O) are problematic. Push state outside the API using classes and dependency injection.

**Violation in `src/Infrastructure/Database.fs`:**
- Line 16: `let mutable private currentConfig: DatabaseConfig option = None`
  - Stores module-level mutable state
  - Requires callers to invoke `Database.initialize` before using any connection
  - Could cause `TypeInitializationException` if not configured
  - Tight coupling between configuration and connection logic

**Impact**: Less testable; harder to reason about state; potential runtime failures if initialization order is wrong; thread-safety concerns.

**Fix**: Refactor `Database` as a class or use a dependency injection container to manage configuration and connections.

**Alternative**: Use a thread-safe configuration holder that validates on each access.

---

### 4. Missing `private` Access Control in Some Helper Functions

**Standard (Line 618-625)**: Prefer non-`public` until needed publicly. Keep helper functions `private`. Minimize public surface area.

**Check Needed**: Review all `module` definitions in `src/Infrastructure/` and `src/Api/` to ensure helper functions are marked `private`.

**Suspected Issues:**
- Many helper functions in endpoint modules may be exposed unintentionally
- Repository functions may expose internal implementation details

**Fix**: Audit and mark helper functions as `private` where appropriate.

---

## Detailed Tasks

### Task 1: Add `[<RequireQualifiedAccess>]` to Types.fs

- [ ] Update `src/Domain/Types.fs`:
  - Add `[<RequireQualifiedAccess>]` before each type definition (lines 10, 16, 24, 36, 43, 50, 70, 84, 91)
  - Verify with build

- [ ] Update all usages across codebase:
  - `src/Domain/*.fs`: Update pattern matches and constructors (e.g., `| User` → `| ActorType.User`)
  - `src/Infrastructure/*.fs`: Update pattern matches
  - `src/Api/*.fs`: Update pattern matches and filters
  - `tests/integration/test_*.py`: May need filter adjustments if they reference these types

- [ ] Run full test suite to ensure no regressions:
  - `dotnet build src/EATool.fsproj`
  - `python -m pytest tests/ -v`

**Effort**: 3-4 hours

---

### Task 2: Convert `UtcTimestamp` to Single-Case DU

- [ ] Update `src/Domain/Types.fs`:
  - Change: `type UtcTimestamp = string`
  - To: `type UtcTimestamp = UtcTimestamp of string`

- [ ] Create helper functions in `src/Domain/Types.fs`:
  ```fsharp
  module UtcTimestamp =
      let value (UtcTimestamp ts) = ts
      let ofString (s: string) = UtcTimestamp s
      let now () = UtcTimestamp (System.DateTime.UtcNow.ToString("O"))
  ```

- [ ] Update all usages:
  - `src/Domain/Models.fs`: Record field initializers (wrap with `UtcTimestamp`)
  - `src/Infrastructure/*.fs`: Repositories and JSON handling (use unwrapping with `.value` or pattern match)
  - `src/Api/*.fs`: Endpoints and responses (wrap/unwrap as needed)
  - `src/Infrastructure/Json.fs`: Encoders/decoders (handle single-case DU serialization)

- [ ] Run full test suite:
  - `dotnet build src/EATool.fsproj`
  - `python -m pytest tests/ -v`

**Effort**: 3-4 hours

---

### Task 3: Refactor Database Module for Dependency Management

- [ ] Option A - Class-based approach:
  - Create `DatabaseConnection` class with constructor taking `DatabaseConfig`
  - Methods: `GetConnection()`, `Initialize()`, `Dispose()`
  - No mutable module state

- [ ] Option B - Service class approach:
  - Create `DatabaseService` class to manage connection pooling
  - Inject into Giraffe/ASP.NET Core dependency container
  - Use constructor injection in endpoints

- [ ] Update `src/Program.fs` to initialize database early with error handling

- [ ] Update all endpoints and repositories:
  - Pass `DatabaseService` or `DatabaseConfig` explicitly
  - Remove reliance on `Database.currentConfig` module state

- [ ] Run full test suite:
  - `dotnet build src/EATool.fsproj`
  - `python -m pytest tests/ -v`

**Effort**: 2-3 hours

---

### Task 4: Audit and Apply Access Control

- [ ] Review each module in `src/Infrastructure/`:
  - Mark helper functions as `private` (e.g., internal query builders, converter functions)
  - Keep only essential public functions exposed

- [ ] Review each module in `src/Api/`:
  - Mark handler helpers as `private`
  - Keep route handlers and main functions public

- [ ] Review `src/Domain/` helper functions:
  - Validation helpers should be `private` within handler modules
  - Only domain types and main handlers should be public

- [ ] Run `dotnet build` to verify no accessibility issues

**Effort**: 1-2 hours

---

### Task 5: Code Review & Testing

- [ ] Code review all changes for:
  - Correctness of pattern matching updates
  - No broken type inference
  - Proper handling of DU unwrapping
  - Database initialization works at startup

- [ ] Run full integration test suite:
  - `python -m pytest tests/ -v`
  - All 89+ tests must pass

- [ ] Manual smoke testing:
  - Start server: `dotnet run --project src/EATool.fsproj`
  - Call endpoints: basic CRUD operations
  - Verify no runtime errors related to type changes

**Effort**: 1-2 hours

---

## Acceptance Criteria

- [ ] All DUs in `Types.fs` have `[<RequireQualifiedAccess>]` attribute
- [ ] `UtcTimestamp` converted to single-case DU with helper functions
- [ ] All usages of `UtcTimestamp` and DUs updated throughout codebase
- [ ] Database configuration uses dependency injection (no module-level mutable state)
- [ ] Helper functions marked as `private` where appropriate
- [ ] Code compiles without warnings: `dotnet build src/EATool.fsproj`
- [ ] All integration tests pass: `python -m pytest tests/ -v`
- [ ] Code follows Microsoft F# style guide and copilot-instructions.md standards
- [ ] Reviewed against [copilot-instructions.md](../../copilot-instructions.md) checklist

---

## Dependencies

**Depends On:** None (improvements to existing code)

**Blocks:**
- None (foundation improvement, enables cleaner future code)

**Related:**
- [copilot-instructions.md](../../copilot-instructions.md#f-code-standards) — F# Code Standards section
- Item-047 (ApplicationService/Interface endpoints) — Will benefit from these standards applied to new code

---

## Definition of Done

- [ ] All code committed to feature branch `feature/f-standards-compliance`
- [ ] All builds succeed without warnings
- [ ] All tests passing (89+ integration tests)
- [ ] Code review completed
- [ ] Branch merged to main
- [ ] Verify standards compliance in new code (Item-047, Item-035, etc.)

---

## Notes

- **Breaking Change Risk**: Converting `UtcTimestamp` to DU may affect JSON serialization. Thoth.Json handles DUs, but test thoroughly.
- **Large Refactor**: This touches many files. Consider doing in one focused PR or split into smaller focused PRs per type.
- **Pattern Examples**: 
  - `RelationAggregate` in `RelationCommands.fs` shows correct DU qualification pattern
  - `RelationProjection.fs` shows how to handle pattern matches with qualified access
- **Testing Strategy**: Run after major changes to catch any missed updates
- **Future**: Once complete, enforce via code review: all new DUs MUST have `[<RequireQualifiedAccess>]`

---

## History

| Date | Event |
|------|-------|
| 2026-01-07 | Created item after F# standards audit |

