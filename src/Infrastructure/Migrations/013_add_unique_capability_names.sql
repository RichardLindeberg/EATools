-- Add unique constraint for business capability names (unique per parent)
CREATE UNIQUE INDEX IF NOT EXISTS idx_business_capabilities_parent_name_unique ON business_capabilities(parent_id, name);
