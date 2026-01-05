-- Create integrations table
CREATE TABLE IF NOT EXISTS integrations (
    id TEXT PRIMARY KEY,
    source_app_id TEXT NOT NULL,
    target_app_id TEXT NOT NULL,
    protocol TEXT NULL,
    data_contract TEXT NULL,
    sla TEXT NULL,
    frequency TEXT NULL,
    tags TEXT NOT NULL,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_integrations_source ON integrations(source_app_id);
CREATE INDEX IF NOT EXISTS idx_integrations_target ON integrations(target_app_id);
