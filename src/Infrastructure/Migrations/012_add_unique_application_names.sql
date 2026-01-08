-- Add unique constraint for application names (globally unique)
CREATE UNIQUE INDEX IF NOT EXISTS idx_applications_name_unique ON applications(name);
