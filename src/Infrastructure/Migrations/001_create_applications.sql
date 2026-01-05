-- Create applications table
CREATE TABLE IF NOT EXISTS applications (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    owner TEXT NULL,
    lifecycle TEXT NOT NULL,
    lifecycle_raw TEXT NOT NULL,
    capability_id TEXT NULL,
    data_classification TEXT NULL,
    tags TEXT NOT NULL,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_applications_name ON applications(name);
CREATE INDEX IF NOT EXISTS idx_applications_owner ON applications(owner);
CREATE INDEX IF NOT EXISTS idx_applications_lifecycle ON applications(lifecycle);
