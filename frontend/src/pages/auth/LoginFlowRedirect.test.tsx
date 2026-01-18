/**
 * Login Flow Redirect Integration Test
 */
import { describe, it, expect, vi } from 'vitest';
import { render } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { LoginPage } from './LoginPage';
import { AuthContext } from '../../contexts/AuthContext';
import type { AuthContextType } from '../../types/auth';

const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
    useLocation: () => ({ state: { from: '/dashboard' } }),
  };
});

describe('LoginFlowRedirect', () => {
  it('redirects to returnUrl when already authenticated', () => {
    const authContext: AuthContextType = {
      user: { id: 'u1', email: 'user@example.com', roles: [], permissions: [] },
      isAuthenticated: true,
      isLoading: false,
      error: null,
      login: vi.fn(),
      logout: vi.fn(),
      refreshAccessToken: vi.fn(),
      hasPermission: vi.fn(),
      hasRole: vi.fn(),
    };

    render(
      <AuthContext.Provider value={authContext}>
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      </AuthContext.Provider>
    );

    expect(mockNavigate).toHaveBeenCalledWith('/dashboard', { replace: true });
  });
});
