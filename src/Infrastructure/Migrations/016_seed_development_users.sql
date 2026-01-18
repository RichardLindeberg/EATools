-- Migration 016: Seed development users
-- Only applied in development environment
-- IMPORTANT: Replace bcrypt hashes with real hashes before production

-- Note: These password hashes are bcrypt hashes (10 rounds) for the password "Password123!"
-- In production, use proper user management endpoints to create users

INSERT OR IGNORE INTO Users (id, email, password_hash, password_salt, roles, status, created_at, updated_at)
VALUES 
    (
        'user-admin-dev-001', 
        'admin@example.com', 
        '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36P4/K1u',
        '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36P4/K1u',
        '[''Admin'']', 
        'active', 
        datetime('now'), 
        datetime('now')
    ),
    (
        'user-viewer-dev-001', 
        'user@example.com', 
        '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36P4/K1u',
        '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36P4/K1u',
        '[''Viewer'']', 
        'active', 
        datetime('now'), 
        datetime('now')
    );
