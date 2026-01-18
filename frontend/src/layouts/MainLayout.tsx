/**
 * MainLayout Component
 * Primary layout with Header, Sidebar, Breadcrumbs, and Content Area
 */

import React from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import { Header } from '../components/Navigation/Header';
import { Sidebar } from '../components/Navigation/Sidebar';
import type { SidebarSection, SidebarItem } from '../components/Navigation/Sidebar';
import { Breadcrumbs } from '../components/Navigation/Breadcrumbs';
import { useBreadcrumbs } from '../hooks/useBreadcrumbs';
import './MainLayout.css';

const SIDEBAR_SECTIONS: SidebarSection[] = [
  {
    title: 'Main',
    items: [
      { id: 'home', label: 'Dashboard', href: '/', active: true },
    ],
  },
  {
    title: 'Management',
    items: [
      { id: 'apps', label: 'Applications', href: '/applications' },
      { id: 'servers', label: 'Servers', href: '/servers' },
      { id: 'integrations', label: 'Integrations', href: '/integrations' },
      { id: 'data', label: 'Data Entities', href: '/data-entities' },
    ],
  },
  {
    title: 'Architecture',
    items: [
      { id: 'capabilities', label: 'Business Capabilities', href: '/business-capabilities' },
      { id: 'orgs', label: 'Organizations', href: '/organizations' },
      { id: 'relations', label: 'Relations', href: '/relations' },
      { id: 'services', label: 'Application Services', href: '/application-services' },
      { id: 'interfaces', label: 'Application Interfaces', href: '/application-interfaces' },
    ],
  },
  {
    title: 'Administration',
    items: [
      { id: 'admin-users', label: 'Users', href: '/admin/users' },
      { id: 'admin-roles', label: 'Roles', href: '/admin/roles' },
      { id: 'admin-perms', label: 'Permissions', href: '/admin/permissions' },
      { id: 'admin-settings', label: 'Settings', href: '/admin/settings' },
    ],
  },
];

export const MainLayout: React.FC = () => {
  const breadcrumbs = useBreadcrumbs();
  const location = useLocation();
  const [sidebarOpen, setSidebarOpen] = React.useState(true);

  const updateActiveSidebar = (sections: SidebarSection[]): SidebarSection[] => {
    return sections.map(section => ({
      ...section,
      items: section.items.map(item => ({
        ...item,
        active: item.href === location.pathname,
      })),
    }));
  };

  const activeSidebar = updateActiveSidebar(SIDEBAR_SECTIONS);

  const toggleSidebar = () => {
    setSidebarOpen(!sidebarOpen);
  };

  return (
    <div className="main-layout">
      <Header onToggleSidebar={toggleSidebar} />
      <div className="main-layout-container">
        {sidebarOpen && <Sidebar sections={activeSidebar} onClose={() => setSidebarOpen(false)} />}
        <div className="main-layout-content">
          {breadcrumbs.length > 0 && (
            <div className="main-layout-breadcrumbs">
              <Breadcrumbs items={breadcrumbs} />
            </div>
          )}
          <main className="main-layout-main">
            <Outlet />
          </main>
        </div>
      </div>
    </div>
  );
};
