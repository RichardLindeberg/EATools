-- Migration 009: Create projection state tracking table

CREATE TABLE IF NOT EXISTS projection_state (
  projection_name TEXT PRIMARY KEY,
  last_processed_event_id TEXT NULL,
  last_processed_at TEXT NULL,
  last_processed_version INTEGER NOT NULL DEFAULT 0,
  status TEXT NOT NULL DEFAULT 'active'
);

CREATE INDEX IF NOT EXISTS ix_projection_state_status ON projection_state(status);
