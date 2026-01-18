-- Migration 015: Create RefreshTokens table for token lifecycle management
-- Stores refresh tokens (hashed) and their revocation status

CREATE TABLE IF NOT EXISTS RefreshTokens (
    id TEXT PRIMARY KEY,
    user_id TEXT NOT NULL,
    token_hash TEXT NOT NULL UNIQUE,
    expires_at TEXT NOT NULL,
    revoked_at TEXT,
    created_at TEXT NOT NULL,
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE
);

-- Index for user ID queries (find all tokens for a user)
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_user_id ON RefreshTokens(user_id);

-- Index for expiry cleanup queries
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_expires_at ON RefreshTokens(expires_at);

-- Index for revocation checks
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_revoked_at ON RefreshTokens(revoked_at);
