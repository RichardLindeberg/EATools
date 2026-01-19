import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { ProtectedRoute } from '../../components/auth/ProtectedRoute';
import { ApplicationListPage } from '../entities/ApplicationListPage';
import { LoginPage } from './LoginPage';

vi.mock('../../hooks/useAuth');

describe('ProtectedRoute redirect integration', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('redirects unauthenticated users to login with returnUrl', () => {
    const { useAuth } = vi.importActual('../../hooks/useAuth') as any;
    vi.mocked(useAuth).mockReturnValue({
      isAuthenticated: false,
      isLoading: false,
      hasPermission: vi.fn(() => false),
      hasRole: vi.fn(() => false),
      login: vi.fn(),
    });

    render(
      <MemoryRouter initialEntries={[{ pathname: '/entities/applications' }]}>
        <Routes>
          <Route
            path="/entities/applications"
            element={
              <ProtectedRoute requiredPermission="app:read">
                <ApplicationListPage />
              </ProtectedRoute>
            }
          />
          <Route path="/auth/login" element={<LoginPage />} />
        </Routes>
      </MemoryRouter>
    );

    expect(
      screen.getByText(/Sign in to your account/i)
    ).toBeInTheDocument();
    expect(screen.queryByText('Applications')).not.toBeInTheDocument();
  });

  it('renders protected page when authenticated & permitted', () => {
    const { useAuth } = vi.importActual('../../hooks/useAuth') as any;
    vi.mocked(useAuth).mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      hasPermission: vi.fn(() => true),
      hasRole: vi.fn(() => true),
      login: vi.fn(),
    });

    render(
      <MemoryRouter initialEntries={[{ pathname: '/entities/applications' }]}>
        <Routes>
          <Route
            path="/entities/applications"
            element={
              <ProtectedRoute requiredPermission="app:read">
                <ApplicationListPage />
              </ProtectedRoute>
            }
          />
          <Route path="/auth/login" element={<LoginPage />} />
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByText('Applications')).toBeInTheDocument();
    expect(
      screen.queryByText(/Sign in to your account/i)
    ).not.toBeInTheDocument();
  });
});
