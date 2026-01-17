/**
 * AdminLayout Component
 * Layout for admin pages with admin-specific sidebar and controls
 */

import React from 'react';
import { Outlet } from 'react-router-dom';
import { Header } from '../components/Navigation/Header';
import { Sidebar } from '../components/Navigation/Sidebar';
import './AdminLayout.css';

export const AdminLayout: React.FC = () => {
  const [sidebarOpen, setSidebarOpen] = React.useState(true);

  const toggleSidebar = () => {
    setSidebarOpen(!sidebarOpen);
  };

  return (
    <div className="admin-layout">
      <Header onToggleSidebar={toggleSidebar} />
      <div className="admin-layout-container">
        {sidebarOpen && <Sidebar onClose={() => setSidebarOpen(false)} />}
        <div className="admin-layout-content">
          <main className="admin-layout-main">
            <div className="admin-layout-header">
              <h1>Administration</h1>
            </div>
            <Outlet />
          </main>
        </div>
      </div>
    </div>
  );
};
