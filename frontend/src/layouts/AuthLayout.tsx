/**
 * AuthLayout Component
 * Minimal layout for authentication pages (login, logout)
 */

import React from 'react';
import { Outlet } from 'react-router-dom';
import './AuthLayout.css';

export const AuthLayout: React.FC = () => {
  return (
    <div className="auth-layout">
      <div className="auth-layout-container">
        <div className="auth-layout-branding">
          <h1>EATool</h1>
          <p>Enterprise Architecture Tool</p>
        </div>
        <main className="auth-layout-main">
          <Outlet />
        </main>
        <div className="auth-layout-footer">
          <p>&copy; 2024 Enterprise Architecture Tool. All rights reserved.</p>
        </div>
      </div>
    </div>
  );
};
