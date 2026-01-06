-- Add parent_id support to organizations for hierarchical structures
-- Allows organizations to have parent organizations (departments → divisions → enterprise)

-- Add parent_id column (nullable to support root organizations)
ALTER TABLE organizations ADD COLUMN parent_id TEXT NULL;

-- Create index for hierarchy queries (finding children of a parent)
CREATE INDEX IF NOT EXISTS idx_organizations_parent ON organizations(parent_id);

-- Create unique compound index for scoped uniqueness (same name not allowed under same parent)
CREATE UNIQUE INDEX IF NOT EXISTS idx_organizations_parent_name ON organizations(parent_id, name);

