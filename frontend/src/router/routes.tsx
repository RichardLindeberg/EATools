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

// Pages - Entities (Form Pages)
import { ApplicationFormPage } from '../pages/entities/ApplicationFormPage';
import { ServerFormPage } from '../pages/entities/ServerFormPage';
import { IntegrationFormPage } from '../pages/entities/IntegrationFormPage';
import { DataEntityFormPage } from '../pages/entities/DataEntityFormPage';
import { BusinessCapabilityFormPage } from '../pages/entities/BusinessCapabilityFormPage';
import { OrganizationFormPage } from '../pages/entities/OrganizationFormPage';
import { ApplicationServiceFormPage } from '../pages/entities/ApplicationServiceFormPage';
import { ApplicationInterfaceFormPage } from '../pages/entities/ApplicationInterfaceFormPage';
import { RelationFormPage } from '../pages/entities/RelationFormPage';

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
                <ApplicationFormPage />
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
                <ApplicationFormPage isEdit={true} />
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
                 <ServerFormPage />
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
                 <ServerFormPage isEdit={true} />
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
                 <IntegrationFormPage />
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
                 <IntegrationFormPage isEdit={true} />
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
                 <DataEntityFormPage />
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
                 <DataEntityFormPage isEdit={true} />
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
                 <BusinessCapabilityFormPage />
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
                 <BusinessCapabilityFormPage isEdit={true} />
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
                 <OrganizationFormPage />
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
                 <OrganizationFormPage isEdit={true} />
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
                 <RelationFormPage />
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
                 <RelationFormPage isEdit={true} />
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
                 <ApplicationServiceFormPage />
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
                 <ApplicationServiceFormPage isEdit={true} />
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
                 <ApplicationInterfaceFormPage />
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
                 <ApplicationInterfaceFormPage isEdit={true} />
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
