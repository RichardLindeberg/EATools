/**
 * ProtectedRoute Component Tests
 */

import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { ProtectedRoute } from './ProtectedRoute';
import { AuthContext } from '../../contexts/AuthContext';
import type { AuthContextType } from '../../types/auth';

const mockUser = {
  id: '1',
  email: 'test@example.com',
  name: 'Test User',
  roles: ['user'],
  permissions: ['read:data', 'write:data'],
};

const createMockAuthContext = (overrides: Partial<AuthContextType> = {}): AuthContextType => ({
  user: null,
  isAuthenticated: false,
  isLoading: false,
  error: null,
  login: vi.fn(),
  logout: vi.fn(),
  refreshAccessToken: vi.fn(),
  hasPermission: vi.fn(),
  hasRole: vi.fn(),
  ...overrides,
});

const renderWithAuth = (
  component: React.ReactElement,
  authContext: AuthContextType
) => {
  return render(
    <AuthContext.Provider value={authContext}>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={component} />
          <Route path="/auth/login" element={<div>Login Page</div>} />
        </Routes>
      </BrowserRouter>
    </AuthContext.Provider>
  );
};

describe('ProtectedRoute', () => {
  it('should show loading spinner when auth is loading', () => {
    const authContext = createMockAuthContext({ isLoading: true });
    renderWithAuth(
      <ProtectedRoute>
        <div>Protected Content</div>
      </ProtectedRoute>,
      authContext
    );

    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
  });

  it('should redirect to login when not authenticated', () => {
    const authContext = createMockAuthContext({ isAuthenticated: false });
    renderWithAuth(
      <ProtectedRoute>
        <div>Protected Content</div>
      </ProtectedRoute>,
      authContext
    );

    expect(screen.getByText('Login Page')).toBeInTheDocument();
    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
  });

  it('should render children when authenticated', () => {
    const authContext = createMockAuthContext({
      isAuthenticated: true,
      isLoading: false,
      user: mockUser,
    });
    
    render(
      <AuthContext.Provider value={authContext}>
        <BrowserRouter>
          <ProtectedRoute>
            <div>Protected Content</div>
          </ProtectedRoute>
        </BrowserRouter>
      </AuthContext.Provider>
    );

    expect(screen.getByText('Protected Content')).toBeInTheDocument();
  });

  it('should show access denied if required permission is missing', () => {
    const hasPermission = vi.fn((perm: string) => false);
    const authContext = createMockAuthContext({
      isAuthenticated: true,
      user: mockUser,
      hasPermission,
      isLoading: false,
    });

    render(
      <AuthContext.Provider value={authContext}>
        <BrowserRouter>
          <ProtectedRoute requiredPermission="admin:access">
            <div>Admin Content</div>
          </ProtectedRoute>
        </BrowserRouter>
      </AuthContext.Provider>
    );

    expect(screen.getByText('Access Denied')).toBeInTheDocument();
    expect(screen.queryByText('Admin Content')).not.toBeInTheDocument();
    expect(hasPermission).toHaveBeenCalledWith('admin:access');
  });

  it('should render children if required permission is present', () => {
    const hasPermission = vi.fn((perm: string) => true);
    const authContext = createMockAuthContext({
      isAuthenticated: true,
      user: mockUser,
      hasPermission,
      isLoading: false,
    });

    render(
      <AuthContext.Provider value={authContext}>
        <BrowserRouter>
          <ProtectedRoute requiredPermission="read:data">
            <div>Data Content</div>
          </ProtectedRoute>
        </BrowserRouter>
      </AuthContext.Provider>
    );

    expect(screen.getByText('Data Content')).toBeInTheDocument();
    expect(hasPermission).toHaveBeenCalledWith('read:data');
  });

  it('should show access denied if required role is missing', () => {
    const hasRole = vi.fn((role: string) => false);
    const authContext = createMockAuthContext({
      isAuthenticated: true,
      user: mockUser,
      hasRole,
      isLoading: false,
    });

    render(
      <AuthContext.Provider value={authContext}>
        <BrowserRouter>
          <ProtectedRoute requiredRole="admin">
            <div>Admin Dashboard</div>
          </ProtectedRoute>
        </BrowserRouter>
      </AuthContext.Provider>
    );

    expect(screen.getByText('Access Denied')).toBeInTheDocument();
    expect(screen.queryByText('Admin Dashboard')).not.toBeInTheDocument();
    expect(hasRole).toHaveBeenCalledWith('admin');
  });

  it('should render children if required role is present', () => {
    const hasRole = vi.fn((role: string) => true);
    const authContext = createMockAuthContext({
      isAuthenticated: true,
      user: mockUser,
      hasRole,
      isLoading: false,
    });

    render(
      <AuthContext.Provider value={authContext}>
        <BrowserRouter>
          <ProtectedRoute requiredRole="user">
            <div>User Dashboard</div>
          </ProtectedRoute>
        </BrowserRouter>
      </AuthContext.Provider>
    );

    expect(screen.getByText('User Dashboard')).toBeInTheDocument();
    expect(hasRole).toHaveBeenCalledWith('user');
  });

  it('should show custom fallback when permission check fails', () => {
    const hasPermission = vi.fn((perm: string) => false);
    const authContext = createMockAuthContext({
      isAuthenticated: true,
      user: mockUser,
      hasPermission,
      isLoading: false,
    });

    render(
      <AuthContext.Provider value={authContext}>
        <BrowserRouter>
          <ProtectedRoute
            requiredPermission="admin:access"
            fallback={<div>Custom Fallback</div>}
          >
            <div>Admin Content</div>
          </ProtectedRoute>
        </BrowserRouter>
      </AuthContext.Provider>
    );

    expect(screen.getByText('Custom Fallback')).toBeInTheDocument();
    expect(screen.queryByText('Admin Content')).not.toBeInTheDocument();
  });
});
