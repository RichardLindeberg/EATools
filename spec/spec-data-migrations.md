---
title: Data Migration Strategy
version: 1.0
date_created: 2026-01-17
last_updated: 2026-01-17
owner: EA Platform Team
tags: [schema, migrations, database, deployment]
---

# Data Migration Strategy

## 1. Introduction & Purpose

This specification defines the strategy for managing database schema changes and data migrations in the EA Tool. The approach prioritizes production reliability, zero-downtime deployments, and data integrity.

### Key Principles

1. **Forward-Only Migrations**: No rollback support; migrations are permanent
2. **Transactional Safety**: Each migration runs within a transaction
3. **Zero-Downtime Deployments**: Migrations must not block production traffic
4. **Backwards Compatibility**: New schema changes must support old and new code simultaneously during rollouts
5. **Immutability**: Migration files never change after execution; they are historical records

## 2. Migration Technology Stack

### DbUp Framework

The EA Tool uses [DbUp](https://dbup.github.io/) for database migration management.

**Key Features:**
- Tracks executed migrations in a versioning table
- Only executes new (unexecuted) migrations on each run
- Supports embedded SQL scripts in assembly
- Provides logging and error tracking
- Transaction-per-script execution model

**Implementation:** [src/Infrastructure/Migrations.fs](../src/Infrastructure/Migrations.fs)

```fsharp
let upgrader =
    DeployChanges.To
        .SQLiteDatabase(config.ConnectionString)
        .WithTransactionPerScript()
        .WithScriptsEmbeddedInAssembly(assembly)
        .Build()

let result = upgrader.PerformUpgrade()
```

## 3. Migration File Structure

### Naming Convention

```
{SequenceNumber}_{DescriptiveTitle}.sql

Examples:
  001_create_applications.sql
  002_create_organizations.sql
  008_add_business_capability_description.sql
  010_create_application_services.sql
```

### Naming Rules

- **Sequence Number**: 3-digit zero-padded integer (001, 002, ..., 100, ...)
- **Increment by 1**: Sequential numbering to avoid conflicts
- **Descriptive Title**: Snake_case, 2-5 words, action-oriented
- **File Extension**: `.sql` (embedded in assembly)

### Anti-Patterns to Avoid

```
❌ Bad:     migration1.sql                    (no sequence number)
❌ Bad:     1_create_apps.sql                 (not zero-padded)
❌ Bad:     999_add_index.sql                 (gaps in sequence)
❌ Bad:     001_Create Apps.sql               (spaces, title case)
❌ Bad:     001_create_applications.sql_bak   (wrong extension)
```

### File Location

All migration files are located in:
```
src/Infrastructure/Migrations/
```

and embedded in the assembly during build.

## 4. Migration Execution Flow

### Startup Initialization

Migrations run automatically at application startup:

```fsharp
// Program.fs initialization
let migration_result = Migrations.run config
match migration_result with
| Ok () -> 
    printfn "Migrations completed successfully"
    // Continue with application startup
| Error msg -> 
    printfn "Migration failed: %s" msg
    failwith msg  // Exit on migration failure
```

### Execution Sequence

1. **Connection Established**: DbUp connects to configured database
2. **Version Table Check**: Queries `SchemaVersions` table for executed migrations
3. **Pending Scripts Identified**: Finds all .sql files not yet recorded
4. **Transaction Started**: Begins transaction for safety
5. **Script Execution**: Each script executes in sequence
6. **Record Versioning**: Successful script recorded in `SchemaVersions`
7. **Transaction Committed**: Confirms all changes
8. **Next Script**: Proceeds to next pending migration
9. **Completion**: All migrations completed or fails on first error

### Transaction Safety

- **Per-Script Transactions**: Each migration script is wrapped in its own transaction
- **All-or-Nothing**: Script succeeds completely or rolls back entirely
- **Atomic Recording**: Version is recorded only if migration succeeds
- **Failure Halts**: If any migration fails, process stops; manual intervention required

## 5. Forward-Only Migration Approach

### No Rollback Support

The EA Tool uses **forward-only migrations** with no rollback capability.

### Rationale

1. **Production Safety**: Rollback often breaks code that depends on schema
2. **Data Integrity**: Cannot safely restore data deleted in previous migrations
3. **Simplicity**: Simpler tooling and fewer edge cases
4. **Immutability**: Historical record of all schema changes

### Handling Mistakes

**If a migration has a mistake:**

1. **Development**: Delete migration file, revert database to previous snapshot, recreate corrected version
2. **Staging/Production**: Create a NEW forward migration that fixes the issue

**Example Fix Migration:**

```sql
-- 015_fix_missing_index_on_applications.sql
-- Fixes: Missing index on application names from migration 014

CREATE INDEX IF NOT EXISTS idx_applications_name 
ON applications(name);
```

## 6. Backwards Compatibility During Deployments

During rolling deployments, old and new code may access the database simultaneously. Migrations must support this.

### Safe Migration Patterns

#### Adding Columns

**✅ Safe:** Add column with DEFAULT value or nullable

```sql
-- 016_add_owner_to_applications.sql
ALTER TABLE applications 
ADD COLUMN owner TEXT DEFAULT 'unassigned';
```

**Why:** Old code doesn't know about new column, new code can handle default value

**❌ Unsafe:** Add NOT NULL column without default

```sql
-- Bad: Old code can't insert, will fail
ALTER TABLE applications 
ADD COLUMN owner TEXT NOT NULL;
```

#### Removing Columns

**✅ Safe:** Create new migration to remove after code is deployed

```sql
-- Migration 1: 017_add_new_owner_id_to_applications.sql
-- Old code uses owner (TEXT), new code uses owner_id (INT FK)
ALTER TABLE applications 
ADD COLUMN owner_id INTEGER REFERENCES organizations(id);

-- Wait for all code to deploy, then in migration 2:
-- 018_remove_deprecated_owner_column.sql
ALTER TABLE applications 
DROP COLUMN owner;
```

**Why:** Allows phased transition without breaking either code version

#### Renaming Columns

**✅ Safe:** Create intermediate column

```sql
-- 019_rename_app_name_to_display_name.sql
-- Add new column
ALTER TABLE applications 
ADD COLUMN display_name TEXT;

-- Copy data
UPDATE applications SET display_name = name;

-- Old code still uses 'name', new code uses 'display_name'
-- In later migration, drop old column after code deployed

-- 020_drop_deprecated_name_column.sql
ALTER TABLE applications DROP COLUMN name;
```

#### Adding NOT NULL Constraints

**✅ Safe:** Two-step process

```sql
-- 021_backfill_status_column.sql
-- Update all rows with NULL status to default
UPDATE applications SET status = 'active' WHERE status IS NULL;

-- 022_add_not_null_constraint_to_status.sql
-- Now add the constraint
ALTER TABLE applications MODIFY status TEXT NOT NULL;
```

## 7. Zero-Downtime Deployment Patterns

### Pattern 1: Expand-Contract

Adds new schema without modifying existing schema.

**Steps:**
1. Add new column/table in migration
2. Deploy updated code that writes to both old and new locations
3. Deploy code that reads from new location, falls back to old
4. Run migration to backfill old location from new location
5. Deploy code that only uses new location
6. (Later) Remove old column/table in separate migration

**Timeline:**
```
t1: Expand (add new column)
t2: Deploy code (write both, read new)
t3: Contract (remove old column when safe)
t4: Deploy code (read new only)
```

### Pattern 2: Feature Flags

Decouple schema changes from code changes using feature flags.

```fsharp
// Config-driven feature flag
let useNewSchema = config.Get("features:use_new_schema") |> bool.Parse

if useNewSchema then
    // Use new schema
    let owner_id = getOwnerId()
else
    // Use old schema
    let owner = getOwner()
```

**Timeline:**
```
t1: Deploy code with feature flag OFF (uses old schema)
t2: Deploy schema migration (adds new schema)
t3: Enable feature flag gradually (10% → 50% → 100%)
t4: Remove old schema migration (after rollback window)
```

### Pattern 3: Blue-Green Deployments

Deploy complete new versions without mixing old and new code.

**Advantages:**
- No partial state during deployment
- Can instantly rollback by switching load balancer
- Simpler migration compatibility logic

**Disadvantages:**
- Higher infrastructure cost (2x database instances)
- More complex deployment orchestration

## 8. Common Migration Patterns

### Creating Tables

```sql
-- 001_create_applications.sql
CREATE TABLE applications (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL UNIQUE,
    description TEXT,
    owner TEXT,
    lifecycle TEXT DEFAULT 'active',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_applications_lifecycle ON applications(lifecycle);
CREATE INDEX idx_applications_owner ON applications(owner);
```

### Adding Columns

```sql
-- 008_add_criticality_to_servers.sql
ALTER TABLE servers 
ADD COLUMN criticality TEXT DEFAULT 'medium';
```

### Creating Indexes

```sql
-- 012_add_unique_application_names.sql
CREATE UNIQUE INDEX idx_applications_name 
ON applications(name);
```

### Adding Foreign Keys

```sql
-- 010_add_owner_organization_fk.sql
ALTER TABLE applications 
ADD CONSTRAINT fk_app_org_owner 
FOREIGN KEY (owner_id) REFERENCES organizations(id);
```

### Data Transformation

```sql
-- 015_normalize_environment_values.sql
-- Migrate from text to enum-like values
UPDATE servers SET environment = 'prod' 
WHERE environment IN ('production', 'p', 'prd');

UPDATE servers SET environment = 'staging' 
WHERE environment IN ('stage', 's', 'stg');

UPDATE servers SET environment = 'dev' 
WHERE environment IN ('development', 'd', 'development', 'local');
```

### Backfilling Data

```sql
-- 016_backfill_parent_id_for_organizations.sql
-- Set parent_id based on historical data
UPDATE organizations o
SET parent_id = (
    SELECT id FROM organizations WHERE name = o.parent_name
)
WHERE parent_name IS NOT NULL;
```

### Deprecation Column

```sql
-- 017_deprecate_old_api_field.sql
-- Mark field for eventual removal
ALTER TABLE applications 
ADD COLUMN deprecated_old_field TEXT;

-- Add note for developers
UPDATE applications 
SET deprecated_old_field = 'Use new_field instead - will be removed in v2.0';
```

## 9. Testing Migrations

### Local Testing

**Before committing a migration:**

1. Run against clean database:
```bash
# Remove local database
rm ./eatool.db

# Run application (migrations execute at startup)
dotnet run

# Verify schema
sqlite3 ./eatool.db ".schema"
```

2. Verify data integrity:
```bash
sqlite3 ./eatool.db "SELECT COUNT(*) FROM applications;"
```

3. Test backwards compatibility:
```bash
# Checkout previous code version
git checkout HEAD~1

# Run with new schema
dotnet run

# Should work without errors
```

### Integration Tests

```fsharp
[<Fact>]
let ``Migration 025 adds search_index column`` () =
    use db = setupTestDatabase()
    // Run migrations
    Migrations.run db.Config |> Result.isOk |> Assert.True
    
    // Check column exists
    let query = "SELECT sql FROM sqlite_master WHERE name = 'applications'"
    let schema = db.QuerySingleString query
    Assert.Contains("search_index", schema)

[<Fact>]
let ``Migration 026 backfills search_index for existing rows`` () =
    use db = setupTestDatabase()
    // Insert test data before migration
    db.Execute("INSERT INTO applications VALUES ('app-1', 'Test App', NULL, NULL, 'active')")
    
    // Run migrations (includes 026)
    Migrations.run db.Config |> Result.isOk |> Assert.True
    
    // Verify backfill worked
    let count = db.QuerySingle<int>("SELECT COUNT(*) FROM applications WHERE search_index IS NOT NULL")
    Assert.Equal(1, count)
```

### Staging Environment Testing

1. Deploy to staging with new migration
2. Run smoke tests with staging data
3. Verify old application version still works with new schema
4. Deploy updated code version
5. Re-run tests with new code
6. Validate data integrity after migration

## 10. Troubleshooting Migrations

### Migration Hangs

**Symptom:** Application starts but migrations never complete

**Causes:**
- Long-running migration on large table
- Locking contention with other processes
- Missing PRAGMA for SQLite performance

**Solution:**
```sql
-- Add to migration for performance on large tables
PRAGMA synchronous = OFF;
PRAGMA journal_mode = MEMORY;

-- Perform migration
CREATE INDEX ... ;

-- Restore safety
PRAGMA synchronous = FULL;
PRAGMA journal_mode = WAL;
```

### Migration Fails with Constraint Error

**Symptom:** Migration fails with "FOREIGN KEY constraint failed"

**Causes:**
- Violating new constraint with existing data
- Missing backfill before adding constraint

**Solution:**
```sql
-- First: Backfill/clean data
UPDATE applications SET owner_id = 1 WHERE owner_id IS NULL;

-- Then: Add constraint
ALTER TABLE applications 
ADD CONSTRAINT fk_app_owner 
FOREIGN KEY (owner_id) REFERENCES organizations(id);
```

### Duplicate Migration Number

**Symptom:** Two developers create migrations with same number

**Solution:**
- One developer renames their migration to next available number
- Coordinate numbering through code review process
- Consider: Use timestamp + sequence (2026_01_17_001_xxx.sql)

### Schema Mismatch Between Environments

**Symptom:** Code works in dev, fails in prod

**Causes:**
- Migrations ran in different order (shouldn't happen with sequential numbering)
- Manual schema changes in production
- Forgotten migration file

**Solution:**
```bash
# Query version table on all environments
sqlite3 staging.db "SELECT * FROM SchemaVersions ORDER BY version;"
sqlite3 prod.db "SELECT * FROM SchemaVersions ORDER BY version;"

# Compare outputs - should be identical
# If not, apply missing migrations manually or reset problematic environment
```

## 11. Deprecation & Cleanup Strategy

### Column Deprecation Timeline

```
t1: Create new column/table
t2: Deploy code that writes to both locations
t3: Run migration to backfill historical data
t4: Deploy code that only uses new location
t5 (1+ month later): Delete old column/table in deprecation migration
```

### Deprecation Notices

```sql
-- 030_deprecate_old_owner_field.sql
-- Deprecation notice: This field is deprecated and will be removed in v3.0
-- Migration: Use owner_id column and join organizations table instead
-- Timeline: Current (v2.0) → v2.1 (warns in logs) → v3.0 (removed)

ALTER TABLE applications 
ADD COLUMN old_owner_text_deprecated TEXT;

-- Copy data for backward compatibility
UPDATE applications 
SET old_owner_text_deprecated = owner_text 
WHERE owner_text IS NOT NULL;
```

### Removal Migration

```sql
-- 035_remove_deprecated_owner_field.sql
-- BREAKING: Removes old_owner_text_deprecated field
-- Migration complete from v2.0 → v3.0
-- All code must use owner_id and organization join

ALTER TABLE applications 
DROP COLUMN old_owner_text_deprecated;
```

## 12. Best Practices

### ✅ DO

- ✅ Test migrations on clean database before committing
- ✅ Use descriptive migration names
- ✅ Keep individual migrations focused on one change
- ✅ Verify backwards compatibility during rolling deployments
- ✅ Document non-obvious migrations with comments
- ✅ Use explicit transaction control for complex operations
- ✅ Monitor migration execution time in production
- ✅ Maintain migration numbering discipline

### ❌ DON'T

- ❌ Modify existing migration files after execution
- ❌ Create large bulk data transformations in single script
- ❌ Add NOT NULL constraints without backfilling
- ❌ Use implicit transactions (rely on explicit `BEGIN/COMMIT`)
- ❌ Drop columns immediately; use deprecation period
- ❌ Skip testing migrations with old code version
- ❌ Mix schema changes with bulk data operations
- ❌ Assume rollback is available; always move forward

## 13. Related Documentation

- [Architecture: Data](spec-architecture-data.md) - Database design and indexing
- [Domain Model Overview](spec-schema-domain-overview.md) - Entity definitions
- [Implementation Status](spec-implementation-status.md) - Schema implementation progress
- DbUp Documentation: https://dbup.github.io/
