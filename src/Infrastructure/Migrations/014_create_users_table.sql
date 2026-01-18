-- Migration 014: Create Users table for authentication
-- Stores user credentials and account information

CREATE TABLE IF NOT EXISTS Users (
    id TEXT PRIMARY KEY,
    email TEXT NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    password_salt TEXT NOT NULL,
    roles TEXT NOT NULL DEFAULT '[]',
    status TEXT NOT NULL DEFAULT 'active',
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,
    last_login_at TEXT
);

-- Index for email lookups (required for login)
CREATE UNIQUE INDEX IF NOT EXISTS idx_users_email ON Users(email);

-- Index for status queries
CREATE INDEX IF NOT EXISTS idx_users_status ON Users(status);
