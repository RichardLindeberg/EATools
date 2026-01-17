/**
 * Authentication Context
 * Provides authentication state and functions throughout the application
 */

import React, { createContext, useCallback, useEffect, useState, ReactNode } from 'react';
import type { AuthContextType, AuthState, LoginRequest, User } from '../types/auth';
import * as authApi from '../api/authApi';
import * as tokenManager from '../utils/tokenManager';

const initialAuthState: AuthState = {
  user: null,
  isAuthenticated: false,
  isLoading: true,
  error: null,
};

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [authState, setAuthState] = useState<AuthState>(initialAuthState);

  /**
   * Initialize auth state from stored tokens on mount
   */
  useEffect(() => {
    const initializeAuth = async () => {
      const hasSession = tokenManager.hasValidSession();
      
      if (hasSession) {
        try {
          // Verify token and get current user
          const user = await authApi.getCurrentUser();
          setAuthState({
            user,
            isAuthenticated: true,
            isLoading: false,
            error: null,
          });
        } catch (error) {
          // Token invalid or expired, clear storage
          tokenManager.clearTokens();
          setAuthState({
            user: null,
            isAuthenticated: false,
            isLoading: false,
            error: null,
          });
        }
      } else {
        setAuthState({
          user: null,
          isAuthenticated: false,
          isLoading: false,
          error: null,
        });
      }
    };

    initializeAuth();
  }, []);

  /**
   * Login with credentials
   */
  const login = useCallback(async (credentials: LoginRequest): Promise<void> => {
    setAuthState((prev) => ({ ...prev, isLoading: true, error: null }));

    try {
      const response = await authApi.login(credentials);
      
      // Store tokens
      tokenManager.setAccessToken(response.accessToken);
      tokenManager.setRefreshToken(response.refreshToken);

      // Update auth state
      setAuthState({
        user: response.user,
        isAuthenticated: true,
        isLoading: false,
        error: null,
      });
    } catch (error: any) {
      const errorMessage = error.response?.data?.message || 'Login failed. Please try again.';
      
      setAuthState({
        user: null,
        isAuthenticated: false,
        isLoading: false,
        error: errorMessage,
      });
      
      throw new Error(errorMessage);
    }
  }, []);

  /**
   * Logout and clear auth state
   */
  const logout = useCallback(async (): Promise<void> => {
    setAuthState((prev) => ({ ...prev, isLoading: true }));

    try {
      // Call logout API
      await authApi.logout();
    } catch (error) {
      console.error('Logout API call failed:', error);
    } finally {
      // Always clear local state and tokens
      tokenManager.clearTokens();
      setAuthState({
        user: null,
        isAuthenticated: false,
        isLoading: false,
        error: null,
      });
    }
  }, []);

  /**
   * Refresh access token using refresh token
   */
  const refreshAccessToken = useCallback(async (): Promise<string | null> => {
    const refreshToken = tokenManager.getRefreshToken();
    
    if (!refreshToken) {
      return null;
    }

    try {
      const response = await authApi.refreshToken(refreshToken);
      tokenManager.setAccessToken(response.accessToken);
      return response.accessToken;
    } catch (error) {
      console.error('Token refresh failed:', error);
      // Clear tokens and logout
      tokenManager.clearTokens();
      setAuthState({
        user: null,
        isAuthenticated: false,
        isLoading: false,
        error: 'Session expired. Please login again.',
      });
      return null;
    }
  }, []);

  /**
   * Check if user has specific permission
   */
  const hasPermission = useCallback(
    (permission: string): boolean => {
      return authState.user?.permissions?.includes(permission) || false;
    },
    [authState.user]
  );

  /**
   * Check if user has specific role
   */
  const hasRole = useCallback(
    (role: string): boolean => {
      return authState.user?.roles?.includes(role) || false;
    },
    [authState.user]
  );

  const contextValue: AuthContextType = {
    ...authState,
    login,
    logout,
    refreshAccessToken,
    hasPermission,
    hasRole,
  };

  return <AuthContext.Provider value={contextValue}>{children}</AuthContext.Provider>;
};
