-- Create business capabilities table
CREATE TABLE IF NOT EXISTS business_capabilities (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    parent_id TEXT NULL,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_business_capabilities_name ON business_capabilities(name);
CREATE INDEX IF NOT EXISTS idx_business_capabilities_parent ON business_capabilities(parent_id);
