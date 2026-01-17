/**
 * Authentication API Client
 * Handles login, logout, and token refresh operations
 */

import { apiClient } from './client';
import type { LoginRequest, TokenResponse, User } from '../types/auth';

/**
 * Login with email and password
 * POST /api/auth/login
 */
export const login = async (credentials: LoginRequest): Promise<TokenResponse> => {
  const response = await apiClient.post<TokenResponse>('/auth/login', {
    email: credentials.email,
    password: credentials.password,
    rememberMe: credentials.rememberMe,
  });
  
  return response.data;
};

/**
 * Logout and invalidate refresh token
 * POST /api/auth/logout
 */
export const logout = async (): Promise<void> => {
  try {
    await apiClient.post('/auth/logout');
  } catch (error) {
    // Continue with logout even if API call fails
    console.error('Logout API call failed:', error);
  }
};

/**
 * Refresh access token using refresh token
 * POST /api/auth/refresh
 */
export const refreshToken = async (refreshToken: string): Promise<{ accessToken: string }> => {
  const response = await apiClient.post<{ accessToken: string }>('/auth/refresh', {
    refreshToken,
  });
  
  return response.data;
};

/**
 * Get current user profile
 * GET /api/auth/me
 */
export const getCurrentUser = async (): Promise<User> => {
  const response = await apiClient.get<User>('/auth/me');
  return response.data;
};

/**
 * Request password reset
 * POST /api/auth/password-reset
 */
export const requestPasswordReset = async (email: string): Promise<void> => {
  await apiClient.post('/auth/password-reset', { email });
};

/**
 * Confirm password reset with token
 * POST /api/auth/password-reset/confirm
 */
export const confirmPasswordReset = async (
  token: string,
  newPassword: string
): Promise<void> => {
  await apiClient.post('/auth/password-reset/confirm', {
    token,
    newPassword,
  });
};

/**
 * Validate token (check if token is still valid)
 * GET /api/auth/validate
 */
export const validateToken = async (): Promise<boolean> => {
  try {
    const response = await apiClient.get<{ valid: boolean }>('/auth/validate');
    return response.data.valid;
  } catch (error) {
    return false;
  }
};
