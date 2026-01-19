import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { ProtectedRoute } from '../../components/auth/ProtectedRoute';
import * as useAuthModule from '../../hooks/useAuth';

vi.mock('../../hooks/useAuth');
vi.mock('../../components/Loading/Spinner', () => ({
  Spinner: () => <div data-testid="spinner">Loading...</div>,
}));

const TestPage = () => <div>Test Protected Content</div>;
const LoginPageTest = () => <div>Sign in to your account</div>;

describe('ProtectedRoute redirect integration', () => {
  const mockUseAuth = vi.mocked(useAuthModule.useAuth);

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('redirects unauthenticated users to login', async () => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: false,
      isLoading: false,
      hasPermission: vi.fn(() => false),
      hasRole: vi.fn(() => false),
      login: vi.fn(),
    } as any);

    render(
      <MemoryRouter initialEntries={[{ pathname: '/protected' }]}>
        <Routes>
          <Route
            path="/protected"
            element={
              <ProtectedRoute requiredPermission="test:read">
                <TestPage />
              </ProtectedRoute>
            }
          />
          <Route path="/auth/login" element={<LoginPageTest />} />
        </Routes>
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/Sign in to your account/i)).toBeInTheDocument();
    });
    expect(screen.queryByText('Test Protected Content')).not.toBeInTheDocument();
  });

  it('renders protected page when authenticated & permitted', async () => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      hasPermission: vi.fn(() => true),
      hasRole: vi.fn(() => true),
      login: vi.fn(),
    } as any);

    render(
      <MemoryRouter initialEntries={[{ pathname: '/protected' }]}>
        <Routes>
          <Route
            path="/protected"
            element={
              <ProtectedRoute requiredPermission="test:read">
                <TestPage />
              </ProtectedRoute>
            }
          />
          <Route path="/auth/login" element={<LoginPageTest />} />
        </Routes>
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Test Protected Content')).toBeInTheDocument();
    });
    expect(screen.queryByText(/Sign in to your account/i)).not.toBeInTheDocument();
  });
});
