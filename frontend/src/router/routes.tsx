/**
 * Route Configuration
 * Defines all application routes with their paths, components, and access requirements
 */

import { RouteObject } from 'react-router-dom';
import { ProtectedRoute } from '../components/auth/ProtectedRoute';

// Layouts
import { MainLayout } from '../layouts/MainLayout';
import { AuthLayout } from '../layouts/AuthLayout';
import { AdminLayout } from '../layouts/AdminLayout';

// Pages - Auth
import { LoginPage } from '../pages/auth/LoginPage';
import { LogoutPage } from '../pages/auth/LogoutPage';

// Pages - Core
import { HomePage } from '../pages/HomePage';
import { NotFoundPage } from '../pages/NotFoundPage';
import { UnauthorizedPage } from '../pages/UnauthorizedPage';

// Placeholder pages for entities (will be replaced with actual components in future items)
const PlaceholderPage = ({ title }: { title: string }) => (
  <div style={{ padding: '2rem' }}>
    <h1>{title}</h1>
    <p>Coming soon...</p>
  </div>
);

export const routes: RouteObject[] = [
  {
    element: <MainLayout />,
    errorElement: <NotFoundPage />,
    children: [
      {
        index: true,
        element: <HomePage />,
      },
      {
        path: 'dashboard',
        element: <HomePage />,
      },

      // ===== Authentication Routes =====
      {
        element: <AuthLayout />,
        children: [
          {
            path: 'auth/login',
            element: <LoginPage />,
          },
          {
            path: 'auth/logout',
            element: <LogoutPage />,
          },
        ],
      },

      // ===== Admin Routes =====
      {
        path: 'admin',
        element: (
          <ProtectedRoute requiredPermission="admin:read">
            <AdminLayout />
          </ProtectedRoute>
        ),
        children: [
          {
            path: 'users',
            element: <PlaceholderPage title="User Management" />,
          },
          {
            path: 'roles',
            element: <PlaceholderPage title="Role Management" />,
          },
          {
            path: 'permissions',
            element: <PlaceholderPage title="Permission Management" />,
          },
          {
            path: 'settings',
            element: <PlaceholderPage title="Application Settings" />,
          },
        ],
      },

      // ===== Applications Routes =====
      {
        path: 'applications',
        children: [
          {
            index: true,
            element: (
              <ProtectedRoute requiredPermission="app:read">
                <PlaceholderPage title="Applications" />
              </ProtectedRoute>
            ),
          },
          {
            path: 'new',
            element: (
              <ProtectedRoute requiredPermission="app:create">
                <PlaceholderPage title="Create Application" />
              </ProtectedRoute>
            ),
          },
          {
            path: ':id',
            element: (
              <ProtectedRoute requiredPermission="app:read">
                <PlaceholderPage title="Application Detail" />
              </ProtectedRoute>
            ),
          },
          {
            path: ':id/edit',
            element: (
              <ProtectedRoute requiredPermission="app:update">
                <PlaceholderPage title="Edit Application" />
              </ProtectedRoute>
            ),
          },
        ],
      },

      // ===== Servers Routes =====
      {
        path: 'servers',
        children: [
          {
            index: true,
            element: (
              <ProtectedRoute requiredPermission="server:read">
                <PlaceholderPage title="Servers" />
              </ProtectedRoute>
            ),
          },
          {
            path: 'new',
            element: (
              <ProtectedRoute requiredPermission="server:create">
                <PlaceholderPage title="Create Server" />
              </ProtectedRoute>
            ),
          },
          {
            path: ':id',
            element: (
              <ProtectedRoute requiredPermission="server:read">
                <PlaceholderPage title="Server Detail" />
              </ProtectedRoute>
            ),
          },
          {
            path: ':id/edit',
            element: (
              <ProtectedRoute requiredPermission="server:update">
                <PlaceholderPage title="Edit Server" />
              </ProtectedRoute>
            ),
          },
        ],
      },

      // ===== Integrations Routes =====
      {
        path: 'integrations',
        children: [
          {
            index: true,
            element: (
              <ProtectedRoute requiredPermission="integration:read">
                <PlaceholderPage title="Integrations" />
              </ProtectedRoute>
            ),
          },
          {
            path: 'new',
            element: (
              <ProtectedRoute requiredPermission="integration:create">
                <PlaceholderPage title="Create Integration" />
              </ProtectedRoute>
            ),
          },
          {
            path: ':id',
            element: (
              <ProtectedRoute requiredPermission="integration:read">
                <PlaceholderPage title="Integration Detail" />
              </ProtectedRoute>
            ),
          },
          {
            path: ':id/edit',
            element: (
              <ProtectedRoute requiredPermission="integration:update">
                <PlaceholderPage title="Edit Integration" />
              </ProtectedRoute>
            ),
          },
        ],
      },

      // ===== Data Entities Routes =====
      {
        path: 'data-entities',
        children: [
          {
            index: true,
            element: (
              <ProtectedRoute requiredPermission="entity:read">
                <PlaceholderPage title="Data Entities" />
              </ProtectedRoute>
            ),
          },
          {
            path: 'new',
            element: (
              <ProtectedRoute requiredPermission="entity:create">
                <PlaceholderPage title="Create Data Entity" />
              </ProtectedRoute>
            ),
          },
          {
            path: ':id',
            element: (
              <ProtectedRoute requiredPermission="entity:read">
                <PlaceholderPage title="Data Entity Detail" />
              </ProtectedRoute>
            ),
          },
          {
            path: ':id/edit',
            element: (
              <ProtectedRoute requiredPermission="entity:update">
                <PlaceholderPage title="Edit Data Entity" />
              </ProtectedRoute>
            ),
          },
        ],
      },

      // ===== Business Capabilities Routes =====
      {
        path: 'business-capabilities',
        children: [
          {
            index: true,
            element: (
              <ProtectedRoute requiredPermission="capability:read">
                <PlaceholderPage title="Business Capabilities" />
              </ProtectedRoute>
            ),
          },
          {
            path: 'new',
            element: (
              <ProtectedRoute requiredPermission="capability:create">
                <PlaceholderPage title="Create Business Capability" />
              </ProtectedRoute>
            ),
          },
          {
            path: ':id',
            element: (
              <ProtectedRoute requiredPermission="capability:read">
                <PlaceholderPage title="Business Capability Detail" />
              </ProtectedRoute>
            ),
          },
          {
            path: ':id/edit',
            element: (
              <ProtectedRoute requiredPermission="capability:update">
                <PlaceholderPage title="Edit Business Capability" />
              </ProtectedRoute>
            ),
          },
        ],
      },

      // ===== Organizations Routes =====
      {
        path: 'organizations',
        children: [
          {
            index: true,
            element: (
              <ProtectedRoute requiredPermission="org:read">
                <PlaceholderPage title="Organizations" />
              </ProtectedRoute>
            ),
          },
          {
            path: 'new',
            element: (
              <ProtectedRoute requiredPermission="org:create">
                <PlaceholderPage title="Create Organization" />
              </ProtectedRoute>
            ),
          },
          {
            path: ':id',
            element: (
              <ProtectedRoute requiredPermission="org:read">
                <PlaceholderPage title="Organization Detail" />
              </ProtectedRoute>
            ),
          },
          {
            path: ':id/edit',
            element: (
              <ProtectedRoute requiredPermission="org:update">
                <PlaceholderPage title="Edit Organization" />
              </ProtectedRoute>
            ),
          },
        ],
      },

      // ===== Relations Routes =====
      {
        path: 'relations',
        children: [
          {
            index: true,
            element: (
              <ProtectedRoute requiredPermission="relation:read">
                <PlaceholderPage title="Relations" />
              </ProtectedRoute>
            ),
          },
          {
            path: 'new',
            element: (
              <ProtectedRoute requiredPermission="relation:create">
                <PlaceholderPage title="Create Relation" />
              </ProtectedRoute>
            ),
          },
          {
            path: ':id',
            element: (
              <ProtectedRoute requiredPermission="relation:read">
                <PlaceholderPage title="Relation Detail" />
              </ProtectedRoute>
            ),
          },
          {
            path: ':id/edit',
            element: (
              <ProtectedRoute requiredPermission="relation:update">
                <PlaceholderPage title="Edit Relation" />
              </ProtectedRoute>
            ),
          },
        ],
      },

      // ===== Application Services Routes =====
      {
        path: 'application-services',
        children: [
          {
            index: true,
            element: (
              <ProtectedRoute requiredPermission="service:read">
                <PlaceholderPage title="Application Services" />
              </ProtectedRoute>
            ),
          },
          {
            path: 'new',
            element: (
              <ProtectedRoute requiredPermission="service:create">
                <PlaceholderPage title="Create Application Service" />
              </ProtectedRoute>
            ),
          },
          {
            path: ':id',
            element: (
              <ProtectedRoute requiredPermission="service:read">
                <PlaceholderPage title="Application Service Detail" />
              </ProtectedRoute>
            ),
          },
          {
            path: ':id/edit',
            element: (
              <ProtectedRoute requiredPermission="service:update">
                <PlaceholderPage title="Edit Application Service" />
              </ProtectedRoute>
            ),
          },
        ],
      },

      // ===== Application Interfaces Routes =====
      {
        path: 'application-interfaces',
        children: [
          {
            index: true,
            element: (
              <ProtectedRoute requiredPermission="interface:read">
                <PlaceholderPage title="Application Interfaces" />
              </ProtectedRoute>
            ),
          },
          {
            path: 'new',
            element: (
              <ProtectedRoute requiredPermission="interface:create">
                <PlaceholderPage title="Create Application Interface" />
              </ProtectedRoute>
            ),
          },
          {
            path: ':id',
            element: (
              <ProtectedRoute requiredPermission="interface:read">
                <PlaceholderPage title="Application Interface Detail" />
              </ProtectedRoute>
            ),
          },
          {
            path: ':id/edit',
            element: (
              <ProtectedRoute requiredPermission="interface:update">
                <PlaceholderPage title="Edit Application Interface" />
              </ProtectedRoute>
            ),
          },
        ],
      },

      // ===== Error Routes =====
      {
        path: 'unauthorized',
        element: <UnauthorizedPage />,
      },
      {
        path: '*',
        element: <NotFoundPage />,
      },
    ],
  },
];
