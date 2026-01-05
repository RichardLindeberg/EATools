-- Create relations table
CREATE TABLE IF NOT EXISTS relations (
    id TEXT PRIMARY KEY,
    source_id TEXT NOT NULL,
    target_id TEXT NOT NULL,
    source_type TEXT NOT NULL,
    target_type TEXT NOT NULL,
    relation_type TEXT NOT NULL,
    archimate_element TEXT NULL,
    archimate_relationship TEXT NULL,
    description TEXT NULL,
    data_classification TEXT NULL,
    criticality TEXT NULL,
    confidence REAL NULL,
    evidence_source TEXT NULL,
    last_verified_at TEXT NULL,
    effective_from TEXT NULL,
    effective_to TEXT NULL,
    label TEXT NULL,
    color TEXT NULL,
    style TEXT NULL,
    bidirectional INTEGER NOT NULL,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_relations_source ON relations(source_id);
CREATE INDEX IF NOT EXISTS idx_relations_target ON relations(target_id);
CREATE INDEX IF NOT EXISTS idx_relations_type ON relations(relation_type);
