/**
 * Authentication TypeScript types
 */

export interface User {
  id: string;
  email: string;
  name: string;
  roles: string[];
  permissions: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}

export interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}

export interface AuthContextType extends AuthState {
  login: (credentials: LoginRequest) => Promise<void>;
  logout: () => Promise<void>;
  refreshAccessToken: () => Promise<string | null>;
  hasPermission: (permission: string) => boolean;
  hasRole: (role: string) => boolean;
}

export interface SessionTimeoutConfig {
  idleTimeout: number; // milliseconds
  absoluteTimeout: number; // milliseconds
  warningTime: number; // milliseconds before timeout to show warning
}
