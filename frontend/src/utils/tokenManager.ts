/**
 * Token Manager - Handles secure storage and retrieval of JWT tokens
 * 
 * Security considerations:
 * - localStorage is used for convenience (accessible across tabs)
 * - In production, consider httpOnly cookies for refresh tokens
 * - XSS protection should be handled at application level
 * - CSRF tokens should be implemented for state-changing operations
 */

const ACCESS_TOKEN_KEY = 'eatool_access_token';
const REFRESH_TOKEN_KEY = 'eatool_refresh_token';
const TOKEN_EXPIRY_KEY = 'eatool_token_expiry';

/**
 * Store access token in localStorage
 */
export const setAccessToken = (token: string): void => {
  try {
    localStorage.setItem(ACCESS_TOKEN_KEY, token);
    
    // Store expiry time (15 minutes from now)
    const expiryTime = Date.now() + 15 * 60 * 1000;
    localStorage.setItem(TOKEN_EXPIRY_KEY, expiryTime.toString());
  } catch (error) {
    console.error('Failed to store access token:', error);
  }
};

/**
 * Get access token from localStorage
 */
export const getAccessToken = (): string | null => {
  try {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  } catch (error) {
    console.error('Failed to retrieve access token:', error);
    return null;
  }
};

/**
 * Store refresh token in localStorage
 */
export const setRefreshToken = (token: string): void => {
  try {
    localStorage.setItem(REFRESH_TOKEN_KEY, token);
  } catch (error) {
    console.error('Failed to store refresh token:', error);
  }
};

/**
 * Get refresh token from localStorage
 */
export const getRefreshToken = (): string | null => {
  try {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  } catch (error) {
    console.error('Failed to retrieve refresh token:', error);
    return null;
  }
};

/**
 * Remove all tokens from storage
 */
export const clearTokens = (): void => {
  try {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(TOKEN_EXPIRY_KEY);
  } catch (error) {
    console.error('Failed to clear tokens:', error);
  }
};

/**
 * Check if access token is expired or will expire soon (within 1 minute)
 */
export const isTokenExpired = (): boolean => {
  try {
    const expiryTime = localStorage.getItem(TOKEN_EXPIRY_KEY);
    if (!expiryTime) {
      return true;
    }
    
    const expiry = parseInt(expiryTime, 10);
    const now = Date.now();
    const bufferTime = 60 * 1000; // 1 minute buffer
    
    return now >= expiry - bufferTime;
  } catch (error) {
    console.error('Failed to check token expiry:', error);
    return true;
  }
};

/**
 * Get time remaining until token expires (in milliseconds)
 */
export const getTokenTimeRemaining = (): number => {
  try {
    const expiryTime = localStorage.getItem(TOKEN_EXPIRY_KEY);
    if (!expiryTime) {
      return 0;
    }
    
    const expiry = parseInt(expiryTime, 10);
    const remaining = expiry - Date.now();
    
    return Math.max(0, remaining);
  } catch (error) {
    console.error('Failed to get token time remaining:', error);
    return 0;
  }
};

/**
 * Validate token format (basic JWT structure check)
 */
export const isValidTokenFormat = (token: string | null): boolean => {
  if (!token) {
    return false;
  }
  
  // JWT format: header.payload.signature
  const parts = token.split('.');
  return parts.length === 3;
};

/**
 * Check if user has a valid session (has token and it's not expired)
 */
export const hasValidSession = (): boolean => {
  const token = getAccessToken();
  return isValidTokenFormat(token) && !isTokenExpired();
};
