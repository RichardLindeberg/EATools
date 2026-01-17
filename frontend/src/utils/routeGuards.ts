/**
 * Route Guards Utilities
 * Permission-based route access control
 */

/**
 * Permission types for route access control
 */
export type Permission =
  | 'app:read'
  | 'app:create'
  | 'app:update'
  | 'app:delete'
  | 'server:read'
  | 'server:create'
  | 'server:update'
  | 'server:delete'
  | 'integration:read'
  | 'integration:create'
  | 'integration:update'
  | 'integration:delete'
  | 'data:read'
  | 'data:create'
  | 'data:update'
  | 'data:delete'
  | 'capability:read'
  | 'capability:create'
  | 'capability:update'
  | 'capability:delete'
  | 'org:read'
  | 'org:create'
  | 'org:update'
  | 'org:delete'
  | 'relation:read'
  | 'relation:create'
  | 'relation:update'
  | 'relation:delete'
  | 'service:read'
  | 'service:create'
  | 'service:update'
  | 'service:delete'
  | 'interface:read'
  | 'interface:create'
  | 'interface:update'
  | 'interface:delete'
  | 'admin:read'
  | 'admin:write';

/**
 * Role types for role-based access control
 */
export type Role = 'admin' | 'architect' | 'manager' | 'viewer' | 'user';

/**
 * Check if user has permission
 * @param userPermissions Array of user's permissions
 * @param requiredPermission Permission to check
 * @returns true if user has the permission
 */
export const hasPermission = (
  userPermissions: Permission[],
  requiredPermission: Permission
): boolean => {
  // Admin can do everything
  if (userPermissions.includes('admin:write')) {
    return true;
  }

  return userPermissions.includes(requiredPermission);
};

/**
 * Check if user has role
 * @param userRoles Array of user's roles
 * @param requiredRole Role to check
 * @returns true if user has the role
 */
export const hasRole = (
  userRoles: Role[],
  requiredRole: Role
): boolean => {
  // Admin has all roles
  if (userRoles.includes('admin')) {
    return true;
  }

  return userRoles.includes(requiredRole);
};

/**
 * Check if user has any of the required permissions
 * @param userPermissions Array of user's permissions
 * @param requiredPermissions Array of permissions (any match)
 * @returns true if user has any of the required permissions
 */
export const hasAnyPermission = (
  userPermissions: Permission[],
  requiredPermissions: Permission[]
): boolean => {
  if (userPermissions.includes('admin:write')) {
    return true;
  }

  return requiredPermissions.some((permission) =>
    userPermissions.includes(permission)
  );
};

/**
 * Check if user has all required permissions
 * @param userPermissions Array of user's permissions
 * @param requiredPermissions Array of permissions (all required)
 * @returns true if user has all required permissions
 */
export const hasAllPermissions = (
  userPermissions: Permission[],
  requiredPermissions: Permission[]
): boolean => {
  if (userPermissions.includes('admin:write')) {
    return true;
  }

  return requiredPermissions.every((permission) =>
    userPermissions.includes(permission)
  );
};

/**
 * Get permission for action on resource
 * @param resource Resource type (app, server, etc.)
 * @param action Action (read, create, update, delete)
 * @returns Permission string
 */
export const getPermissionForAction = (
  resource: string,
  action: 'read' | 'create' | 'update' | 'delete'
): Permission => {
  const resourceMap: Record<string, string> = {
    application: 'app',
    applications: 'app',
    server: 'server',
    servers: 'server',
    integration: 'integration',
    integrations: 'integration',
    'data-entity': 'data',
    'data-entities': 'data',
    'business-capability': 'capability',
    'business-capabilities': 'capability',
    organization: 'org',
    organizations: 'org',
    relation: 'relation',
    relations: 'relation',
    'application-service': 'service',
    'application-services': 'service',
    'application-interface': 'interface',
    'application-interfaces': 'interface',
  };

  const normalizedResource = resourceMap[resource.toLowerCase()] || resource;
  return `${normalizedResource}:${action}` as Permission;
};
