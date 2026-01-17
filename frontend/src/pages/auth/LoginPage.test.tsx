/**
 * LoginPage Component Tests
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
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
    useLocation: () => ({ state: null }),
  };
});

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

const renderLoginPage = (authContext: AuthContextType) => {
  return render(
    <AuthContext.Provider value={authContext}>
      <BrowserRouter>
        <LoginPage />
      </BrowserRouter>
    </AuthContext.Provider>
  );
};

describe('LoginPage', () => {
  beforeEach(() => {
    mockNavigate.mockClear();
  });

  it('should render login form', () => {
    const authContext = createMockAuthContext();
    renderLoginPage(authContext);

    expect(screen.getByText('EATool')).toBeInTheDocument();
    expect(screen.getByText('Sign in to your account')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('you@example.com')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('Enter your password')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
  });

  it('should show validation errors for empty fields', async () => {
    const authContext = createMockAuthContext();
    renderLoginPage(authContext);

    const submitButton = screen.getByRole('button', { name: /sign in/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('Email is required')).toBeInTheDocument();
      expect(screen.getByText('Password is required')).toBeInTheDocument();
    });
  });

  it('should show validation error for invalid email', async () => {
    const authContext = createMockAuthContext();
    renderLoginPage(authContext);

    const emailInput = screen.getByPlaceholderText('you@example.com');
    fireEvent.change(emailInput, { target: { value: 'invalid-email' } });
    
    const submitButton = screen.getByRole('button', { name: /sign in/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('Invalid email address')).toBeInTheDocument();
    });
  });

  it('should show validation error for short password', async () => {
    const authContext = createMockAuthContext();
    renderLoginPage(authContext);

    const emailInput = screen.getByPlaceholderText('you@example.com');
    const passwordInput = screen.getByPlaceholderText('Enter your password');
    
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });
    fireEvent.change(passwordInput, { target: { value: 'short' } });
    
    const submitButton = screen.getByRole('button', { name: /sign in/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('Password must be at least 8 characters')).toBeInTheDocument();
    });
  });

  it('should call login function with form data', async () => {
    const login = vi.fn().mockResolvedValue(undefined);
    const authContext = createMockAuthContext({ login });
    renderLoginPage(authContext);

    const emailInput = screen.getByPlaceholderText('you@example.com');
    const passwordInput = screen.getByPlaceholderText('Enter your password');
    
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });
    fireEvent.change(passwordInput, { target: { value: 'password123' } });
    
    const submitButton = screen.getByRole('button', { name: /sign in/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(login).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: 'password123',
        rememberMe: false,
      });
    });
  });

  it('should include rememberMe in login request when checked', async () => {
    const login = vi.fn().mockResolvedValue(undefined);
    const authContext = createMockAuthContext({ login });
    renderLoginPage(authContext);

    const emailInput = screen.getByPlaceholderText('you@example.com');
    const passwordInput = screen.getByPlaceholderText('Enter your password');
    const rememberMeCheckbox = screen.getByRole('checkbox');
    
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });
    fireEvent.change(passwordInput, { target: { value: 'password123' } });
    fireEvent.click(rememberMeCheckbox);
    
    const submitButton = screen.getByRole('button', { name: /sign in/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(login).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: 'password123',
        rememberMe: true,
      });
    });
  });

  it('should show error message on login failure', async () => {
    const login = vi.fn().mockRejectedValue(new Error('Invalid credentials'));
    const authContext = createMockAuthContext({ login });
    renderLoginPage(authContext);

    const emailInput = screen.getByPlaceholderText('you@example.com');
    const passwordInput = screen.getByPlaceholderText('Enter your password');
    
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });
    fireEvent.change(passwordInput, { target: { value: 'wrongpassword' } });
    
    const submitButton = screen.getByRole('button', { name: /sign in/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('Invalid credentials')).toBeInTheDocument();
    });
  });

  it('should show loading state when auth is loading', () => {
    const authContext = createMockAuthContext({ isLoading: true });
    renderLoginPage(authContext);

    expect(screen.getByText('Loading...')).toBeInTheDocument();
    expect(screen.queryByPlaceholderText('you@example.com')).not.toBeInTheDocument();
  });

  it('should display auth error from context', () => {
    const authContext = createMockAuthContext({ error: 'Account locked' });
    renderLoginPage(authContext);

    expect(screen.getByText('Account locked')).toBeInTheDocument();
  });
});
