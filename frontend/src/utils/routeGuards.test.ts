/**
 * Route Guards tests
 * Tests for permission and role-based access control
 */

import { describe, it, expect } from 'vitest';
import {
  hasPermission,
  hasRole,
  hasAnyPermission,
  hasAllPermissions,
  getPermissionForAction,
  type Permission,
  type Role,
} from './routeGuards';

describe('Route Guards - Permissions', () => {
  it('should check single permission', () => {
    const permissions: Permission[] = ['app:read', 'app:create'];
    
    expect(hasPermission(permissions, 'app:read')).toBe(true);
    expect(hasPermission(permissions, 'app:update')).toBe(false);
    expect(hasPermission(permissions, 'server:read')).toBe(false);
  });

  it('admin should have all permissions', () => {
    const permissions: Permission[] = ['admin:write'];
    
    expect(hasPermission(permissions, 'app:read')).toBe(true);
    expect(hasPermission(permissions, 'server:delete')).toBe(true);
    expect(hasPermission(permissions, 'capability:update')).toBe(true);
  });

  it('should check any permission', () => {
    const permissions: Permission[] = ['app:read', 'server:read'];
    
    expect(hasAnyPermission(permissions, ['app:create', 'app:read'])).toBe(true);
    expect(hasAnyPermission(permissions, ['app:update', 'server:create'])).toBe(false);
  });

  it('should check all permissions', () => {
    const permissions: Permission[] = ['app:read', 'app:create', 'server:read'];
    
    expect(hasAllPermissions(permissions, ['app:read', 'app:create'])).toBe(true);
    expect(hasAllPermissions(permissions, ['app:read', 'app:update'])).toBe(false);
  });

  it('admin should have any and all permissions', () => {
    const permissions: Permission[] = ['admin:write'];
    
    expect(hasAnyPermission(permissions, ['app:read', 'app:create'])).toBe(true);
    expect(hasAllPermissions(permissions, ['app:read', 'app:create', 'server:delete'])).toBe(
      true
    );
  });

  it('should generate correct permission for action', () => {
    expect(getPermissionForAction('application', 'read')).toBe('app:read');
    expect(getPermissionForAction('applications', 'create')).toBe('app:create');
    expect(getPermissionForAction('server', 'update')).toBe('server:update');
    expect(getPermissionForAction('servers', 'delete')).toBe('server:delete');
    expect(getPermissionForAction('data-entity', 'read')).toBe('data:read');
    expect(getPermissionForAction('data-entities', 'create')).toBe('data:create');
    expect(getPermissionForAction('business-capability', 'read')).toBe('capability:read');
    expect(getPermissionForAction('organization', 'update')).toBe('org:update');
  });
});

describe('Route Guards - Roles', () => {
  it('should check single role', () => {
    const roles: Role[] = ['architect', 'manager'];
    
    expect(hasRole(roles, 'architect')).toBe(true);
    expect(hasRole(roles, 'admin')).toBe(false);
    expect(hasRole(roles, 'viewer')).toBe(false);
  });

  it('admin should have all roles', () => {
    const roles: Role[] = ['admin'];
    
    expect(hasRole(roles, 'architect')).toBe(true);
    expect(hasRole(roles, 'manager')).toBe(true);
    expect(hasRole(roles, 'viewer')).toBe(true);
  });

  it('should validate role types', () => {
    const validRoles: Role[] = ['admin', 'architect', 'manager', 'viewer', 'user'];
    
    validRoles.forEach(role => {
      expect(() => {
        hasRole([role], 'admin');
      }).not.toThrow();
    });
  });
});
