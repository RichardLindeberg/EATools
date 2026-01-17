/**
 * Token Manager Tests
 */

import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import * as tokenManager from './tokenManager';

describe('tokenManager', () => {
  beforeEach(() => {
    localStorage.clear();
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  describe('setAccessToken', () => {
    it('should store access token in localStorage', () => {
      tokenManager.setAccessToken('test-token');
      expect(localStorage.getItem('eatool_access_token')).toBe('test-token');
    });

    it('should store token expiry time', () => {
      const now = Date.now();
      vi.setSystemTime(now);
      
      tokenManager.setAccessToken('test-token');
      
      const expiry = localStorage.getItem('eatool_token_expiry');
      expect(expiry).toBe((now + 15 * 60 * 1000).toString());
    });
  });

  describe('getAccessToken', () => {
    it('should retrieve access token from localStorage', () => {
      localStorage.setItem('eatool_access_token', 'test-token');
      expect(tokenManager.getAccessToken()).toBe('test-token');
    });

    it('should return null if no token exists', () => {
      expect(tokenManager.getAccessToken()).toBeNull();
    });
  });

  describe('setRefreshToken', () => {
    it('should store refresh token in localStorage', () => {
      tokenManager.setRefreshToken('refresh-token');
      expect(localStorage.getItem('eatool_refresh_token')).toBe('refresh-token');
    });
  });

  describe('getRefreshToken', () => {
    it('should retrieve refresh token from localStorage', () => {
      localStorage.setItem('eatool_refresh_token', 'refresh-token');
      expect(tokenManager.getRefreshToken()).toBe('refresh-token');
    });

    it('should return null if no refresh token exists', () => {
      expect(tokenManager.getRefreshToken()).toBeNull();
    });
  });

  describe('clearTokens', () => {
    it('should remove all tokens from localStorage', () => {
      localStorage.setItem('eatool_access_token', 'token');
      localStorage.setItem('eatool_refresh_token', 'refresh');
      localStorage.setItem('eatool_token_expiry', '123456');

      tokenManager.clearTokens();

      expect(localStorage.getItem('eatool_access_token')).toBeNull();
      expect(localStorage.getItem('eatool_refresh_token')).toBeNull();
      expect(localStorage.getItem('eatool_token_expiry')).toBeNull();
    });
  });

  describe('isTokenExpired', () => {
    it('should return true if no expiry time is set', () => {
      expect(tokenManager.isTokenExpired()).toBe(true);
    });

    it('should return true if token has expired', () => {
      const now = Date.now();
      const pastExpiry = now - 1000;
      
      localStorage.setItem('eatool_token_expiry', pastExpiry.toString());
      vi.setSystemTime(now);

      expect(tokenManager.isTokenExpired()).toBe(true);
    });

    it('should return true if token will expire within buffer time', () => {
      const now = Date.now();
      const nearExpiry = now + 30 * 1000; // 30 seconds from now (within 1 min buffer)
      
      localStorage.setItem('eatool_token_expiry', nearExpiry.toString());
      vi.setSystemTime(now);

      expect(tokenManager.isTokenExpired()).toBe(true);
    });

    it('should return false if token is still valid', () => {
      const now = Date.now();
      const futureExpiry = now + 10 * 60 * 1000; // 10 minutes from now
      
      localStorage.setItem('eatool_token_expiry', futureExpiry.toString());
      vi.setSystemTime(now);

      expect(tokenManager.isTokenExpired()).toBe(false);
    });
  });

  describe('getTokenTimeRemaining', () => {
    it('should return 0 if no expiry time is set', () => {
      expect(tokenManager.getTokenTimeRemaining()).toBe(0);
    });

    it('should return 0 if token has expired', () => {
      const now = Date.now();
      const pastExpiry = now - 1000;
      
      localStorage.setItem('eatool_token_expiry', pastExpiry.toString());
      vi.setSystemTime(now);

      expect(tokenManager.getTokenTimeRemaining()).toBe(0);
    });

    it('should return remaining time in milliseconds', () => {
      const now = Date.now();
      const futureExpiry = now + 5 * 60 * 1000; // 5 minutes from now
      
      localStorage.setItem('eatool_token_expiry', futureExpiry.toString());
      vi.setSystemTime(now);

      expect(tokenManager.getTokenTimeRemaining()).toBe(5 * 60 * 1000);
    });
  });

  describe('isValidTokenFormat', () => {
    it('should return false for null token', () => {
      expect(tokenManager.isValidTokenFormat(null)).toBe(false);
    });

    it('should return false for empty string', () => {
      expect(tokenManager.isValidTokenFormat('')).toBe(false);
    });

    it('should return false for invalid JWT format', () => {
      expect(tokenManager.isValidTokenFormat('invalid-token')).toBe(false);
      expect(tokenManager.isValidTokenFormat('part1.part2')).toBe(false);
    });

    it('should return true for valid JWT format', () => {
      expect(tokenManager.isValidTokenFormat('header.payload.signature')).toBe(true);
    });
  });

  describe('hasValidSession', () => {
    it('should return false if no token exists', () => {
      expect(tokenManager.hasValidSession()).toBe(false);
    });

    it('should return false if token format is invalid', () => {
      localStorage.setItem('eatool_access_token', 'invalid');
      expect(tokenManager.hasValidSession()).toBe(false);
    });

    it('should return false if token is expired', () => {
      const now = Date.now();
      localStorage.setItem('eatool_access_token', 'header.payload.signature');
      localStorage.setItem('eatool_token_expiry', (now - 1000).toString());
      vi.setSystemTime(now);

      expect(tokenManager.hasValidSession()).toBe(false);
    });

    it('should return true if token is valid and not expired', () => {
      const now = Date.now();
      localStorage.setItem('eatool_access_token', 'header.payload.signature');
      localStorage.setItem('eatool_token_expiry', (now + 10 * 60 * 1000).toString());
      vi.setSystemTime(now);

      expect(tokenManager.hasValidSession()).toBe(true);
    });
  });
});
