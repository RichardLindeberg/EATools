-- Create application_interfaces projection table
CREATE TABLE IF NOT EXISTS application_interfaces (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    protocol TEXT NOT NULL,
    endpoint TEXT NULL,
    specification_url TEXT NULL,
    version TEXT NULL,
    authentication_method TEXT NULL,
    exposed_by_app_id TEXT NOT NULL,
    serves_service_ids TEXT NOT NULL,
    rate_limits TEXT NULL,
    status TEXT NOT NULL,
    tags TEXT NOT NULL,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_application_interfaces_app ON application_interfaces(exposed_by_app_id);
CREATE INDEX IF NOT EXISTS idx_application_interfaces_status ON application_interfaces(status);
CREATE INDEX IF NOT EXISTS idx_application_interfaces_created_at ON application_interfaces(created_at);
