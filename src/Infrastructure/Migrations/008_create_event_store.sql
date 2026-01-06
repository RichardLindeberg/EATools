-- Migration 008: Create Event Store schema (SQLite)

-- Events table
CREATE TABLE IF NOT EXISTS events (
  event_id TEXT PRIMARY KEY,
  aggregate_id TEXT NOT NULL,
  aggregate_type TEXT NOT NULL,
  aggregate_version INTEGER NOT NULL,
  event_type TEXT NOT NULL,
  event_version INTEGER NOT NULL DEFAULT 1,
  event_timestamp TEXT NOT NULL,
  actor TEXT NOT NULL,
  actor_type TEXT NOT NULL,
  source TEXT NOT NULL,
  causation_id TEXT NULL,
  correlation_id TEXT NULL,
  data TEXT NOT NULL,
  metadata TEXT NULL
);

-- Unique constraint per aggregate version
CREATE UNIQUE INDEX IF NOT EXISTS ux_events_aggregate_version
  ON events(aggregate_id, aggregate_version);

-- Indexes for query patterns
CREATE INDEX IF NOT EXISTS ix_events_aggregate_id ON events(aggregate_id);
CREATE INDEX IF NOT EXISTS ix_events_event_type ON events(event_type);
CREATE INDEX IF NOT EXISTS ix_events_event_timestamp ON events(event_timestamp);
CREATE INDEX IF NOT EXISTS ix_events_correlation_id ON events(correlation_id);

-- Commands table for idempotency
CREATE TABLE IF NOT EXISTS commands (
  command_id TEXT PRIMARY KEY,
  command_type TEXT NOT NULL,
  aggregate_id TEXT NOT NULL,
  aggregate_type TEXT NOT NULL,
  processed_at TEXT NULL,
  actor TEXT NOT NULL,
  source TEXT NOT NULL,
  data TEXT NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_commands_command_id ON commands(command_id);

-- Snapshots table (optional, for performance)
CREATE TABLE IF NOT EXISTS snapshots (
  snapshot_id TEXT PRIMARY KEY,
  aggregate_id TEXT NOT NULL,
  aggregate_type TEXT NOT NULL,
  aggregate_version INTEGER NOT NULL,
  snapshot_version INTEGER NOT NULL DEFAULT 1,
  snapshot_timestamp TEXT NOT NULL,
  state TEXT NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_snapshots_aggregate_version
  ON snapshots(aggregate_id, aggregate_version);

CREATE INDEX IF NOT EXISTS ix_snapshots_aggregate_id ON snapshots(aggregate_id);
