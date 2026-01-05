-- Create data entities table
CREATE TABLE IF NOT EXISTS data_entities (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    domain TEXT NULL,
    classification TEXT NOT NULL,
    retention TEXT NULL,
    owner TEXT NULL,
    steward TEXT NULL,
    source_system TEXT NULL,
    criticality TEXT NULL,
    pii_flag INTEGER NOT NULL,
    glossary_terms TEXT NOT NULL,
    lineage TEXT NOT NULL,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_data_entities_name ON data_entities(name);
CREATE INDEX IF NOT EXISTS idx_data_entities_domain ON data_entities(domain);
CREATE INDEX IF NOT EXISTS idx_data_entities_classification ON data_entities(classification);
