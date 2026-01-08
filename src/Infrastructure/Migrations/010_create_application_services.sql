-- Create application_services projection table
CREATE TABLE IF NOT EXISTS application_services (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    description TEXT NULL,
    business_capability_id TEXT NULL,
    sla TEXT NULL,
    exposed_by_app_ids TEXT NOT NULL,
    consumers TEXT NOT NULL,
    tags TEXT NOT NULL,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_application_services_capability ON application_services(business_capability_id);
CREATE INDEX IF NOT EXISTS idx_application_services_created_at ON application_services(created_at);
