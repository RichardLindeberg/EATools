/**
 * Route Configuration
 * Defines all application routes with their paths, components, and access requirements
 */

import type { RouteObject } from 'react-router-dom';
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

// Pages - Entities (List Pages)
import { ApplicationListPage } from '../pages/entities/ApplicationListPage';
import { ServerListPage } from '../pages/entities/ServerListPage';
import { IntegrationListPage } from '../pages/entities/IntegrationListPage';
import { DataEntityListPage } from '../pages/entities/DataEntityListPage';
import { BusinessCapabilityListPage } from '../pages/entities/BusinessCapabilityListPage';
import { OrganizationListPage } from '../pages/entities/OrganizationListPage';
import { RelationListPage } from '../pages/entities/RelationListPage';
import { ApplicationServiceListPage } from '../pages/entities/ApplicationServiceListPage';
import { ApplicationInterfaceListPage } from '../pages/entities/ApplicationInterfaceListPage';

// Pages - Entities (Detail Pages)
import { ApplicationDetailPage } from '../pages/entities/ApplicationDetailPage';
import { ServerDetailPage } from '../pages/entities/ServerDetailPage';
import { IntegrationDetailPage } from '../pages/entities/IntegrationDetailPage';
import { DataEntityDetailPage } from '../pages/entities/DataEntityDetailPage';
import { BusinessCapabilityDetailPage } from '../pages/entities/BusinessCapabilityDetailPage';
import { OrganizationDetailPage } from '../pages/entities/OrganizationDetailPage';
import { RelationDetailPage } from '../pages/entities/RelationDetailPage';
import { ApplicationServiceDetailPage } from '../pages/entities/ApplicationServiceDetailPage';
import { ApplicationInterfaceDetailPage } from '../pages/entities/ApplicationInterfaceDetailPage';

// Placeholder pages for forms (will be created in future items)
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
        path: 'entities/applications',
        children: [
          {
            index: true,
            element: (
              <ProtectedRoute requiredPermission="app:read">
                <ApplicationListPage />
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
                <ApplicationDetailPage />
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
        path: 'entities/servers',
        children: [
          {
            index: true,
            element: (
              <ProtectedRoute requiredPermission="server:read">
                <ServerListPage />
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
                <ServerDetailPage />
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
        path: 'entities/integrations',
        children: [
          {
            index: true,
            element: (
              <ProtectedRoute requiredPermission="integration:read">
                <IntegrationListPage />
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
                <IntegrationDetailPage />
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
        path: 'entities/data-entities',
        children: [
          {
            index: true,
            element: (
              <ProtectedRoute requiredPermission="entity:read">
                <DataEntityListPage />
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
                <DataEntityDetailPage />
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
        path: 'entities/business-capabilities',
        children: [
          {
            index: true,
            element: (
              <ProtectedRoute requiredPermission="capability:read">
                <BusinessCapabilityListPage />
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
                <BusinessCapabilityDetailPage />
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
        path: 'entities/organizations',
        children: [
          {
            index: true,
            element: (
              <ProtectedRoute requiredPermission="org:read">
                <OrganizationListPage />
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
                <OrganizationDetailPage />
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
        path: 'entities/relations',
        children: [
          {
            index: true,
            element: (
              <ProtectedRoute requiredPermission="relation:read">
                <RelationListPage />
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
                <RelationDetailPage />
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
        path: 'entities/application-services',
        children: [
          {
            index: true,
            element: (
              <ProtectedRoute requiredPermission="service:read">
                <ApplicationServiceListPage />
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
                <ApplicationServiceDetailPage />
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
        path: 'entities/application-interfaces',
        children: [
          {
            index: true,
            element: (
              <ProtectedRoute requiredPermission="interface:read">
                <ApplicationInterfaceListPage />
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
                <ApplicationInterfaceDetailPage />
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
