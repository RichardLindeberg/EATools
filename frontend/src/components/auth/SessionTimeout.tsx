/**
 * Session Timeout Component
 * Tracks user activity and shows warning before session expires
 * Automatically logs out user after timeout
 */

import React, { useEffect, useState, useCallback, useRef } from 'react';
import { useAuth } from '../../hooks/useAuth';
import { Modal } from '../../components/Modal/Modal';
import { Button } from '../../components/Button/Button';
import type { SessionTimeoutConfig } from '../../types/auth';

const DEFAULT_CONFIG: SessionTimeoutConfig = {
  idleTimeout: 30 * 60 * 1000, // 30 minutes
  absoluteTimeout: 8 * 60 * 60 * 1000, // 8 hours
  warningTime: 2 * 60 * 1000, // 2 minutes before timeout
};

interface SessionTimeoutProps {
  config?: Partial<SessionTimeoutConfig>;
}

export const SessionTimeout: React.FC<SessionTimeoutProps> = ({ config: userConfig }) => {
  const { isAuthenticated, logout, refreshAccessToken } = useAuth();
  const [showWarning, setShowWarning] = useState(false);
  const [timeRemaining, setTimeRemaining] = useState(0);

  const config: SessionTimeoutConfig = { ...DEFAULT_CONFIG, ...userConfig };

  const lastActivityRef = useRef<number>(Date.now());
  const sessionStartRef = useRef<number>(Date.now());
  const warningTimerRef = useRef<NodeJS.Timeout | null>(null);
  const logoutTimerRef = useRef<NodeJS.Timeout | null>(null);
  const countdownIntervalRef = useRef<NodeJS.Timeout | null>(null);

  /**
   * Reset activity timestamp and timers
   */
  const resetActivity = useCallback(() => {
    lastActivityRef.current = Date.now();
    setShowWarning(false);

    // Clear existing timers
    if (warningTimerRef.current) {
      clearTimeout(warningTimerRef.current);
    }
    if (logoutTimerRef.current) {
      clearTimeout(logoutTimerRef.current);
    }
    if (countdownIntervalRef.current) {
      clearInterval(countdownIntervalRef.current);
    }

    // Check if absolute timeout has been reached
    const sessionDuration = Date.now() - sessionStartRef.current;
    if (sessionDuration >= config.absoluteTimeout) {
      handleTimeout();
      return;
    }

    // Calculate time until warning
    const timeUntilWarning = config.idleTimeout - config.warningTime;

    // Set warning timer
    warningTimerRef.current = setTimeout(() => {
      setShowWarning(true);
      setTimeRemaining(config.warningTime);

      // Start countdown
      countdownIntervalRef.current = setInterval(() => {
        setTimeRemaining((prev) => {
          const newTime = prev - 1000;
          if (newTime <= 0) {
            if (countdownIntervalRef.current) {
              clearInterval(countdownIntervalRef.current);
            }
          }
          return Math.max(0, newTime);
        });
      }, 1000);

      // Set logout timer
      logoutTimerRef.current = setTimeout(() => {
        handleTimeout();
      }, config.warningTime);
    }, timeUntilWarning);
  }, [config.idleTimeout, config.warningTime, config.absoluteTimeout]);

  /**
   * Handle session timeout
   */
  const handleTimeout = useCallback(async () => {
    setShowWarning(false);
    await logout();
  }, [logout]);

  /**
   * Extend session by refreshing token and resetting activity
   */
  const handleExtendSession = useCallback(async () => {
    try {
      await refreshAccessToken();
      sessionStartRef.current = Date.now(); // Reset session start time
      resetActivity();
    } catch (error) {
      console.error('Failed to extend session:', error);
      await handleTimeout();
    }
  }, [refreshAccessToken, resetActivity, handleTimeout]);

  /**
   * Track user activity events
   */
  useEffect(() => {
    if (!isAuthenticated) {
      return;
    }

    const activityEvents = ['mousedown', 'keydown', 'scroll', 'touchstart', 'click'];

    const handleActivity = () => {
      const now = Date.now();
      const timeSinceLastActivity = now - lastActivityRef.current;

      // Only reset if enough time has passed (debounce)
      if (timeSinceLastActivity > 1000) {
        resetActivity();
      }
    };

    // Add event listeners
    activityEvents.forEach((event) => {
      document.addEventListener(event, handleActivity, { passive: true });
    });

    // Initialize timers
    resetActivity();

    // Cleanup
    return () => {
      activityEvents.forEach((event) => {
        document.removeEventListener(event, handleActivity);
      });

      if (warningTimerRef.current) {
        clearTimeout(warningTimerRef.current);
      }
      if (logoutTimerRef.current) {
        clearTimeout(logoutTimerRef.current);
      }
      if (countdownIntervalRef.current) {
        clearInterval(countdownIntervalRef.current);
      }
    };
  }, [isAuthenticated, resetActivity]);

  if (!isAuthenticated || !showWarning) {
    return null;
  }

  const minutes = Math.floor(timeRemaining / 60000);
  const seconds = Math.floor((timeRemaining % 60000) / 1000);

  return (
    <Modal
      isOpen={showWarning}
      onClose={handleExtendSession}
      title="Session Timeout Warning"
      size="sm"
    >
      <div style={{ textAlign: 'center' }}>
        <p style={{ marginBottom: '1.5rem' }}>
          Your session will expire in{' '}
          <strong>
            {minutes}:{seconds.toString().padStart(2, '0')}
          </strong>
        </p>
        <p style={{ marginBottom: '1.5rem', color: 'var(--color-gray-600)' }}>
          Would you like to continue working?
        </p>
        <div style={{ display: 'flex', gap: '1rem', justifyContent: 'center' }}>
          <Button variant="secondary" onClick={handleTimeout}>
            Logout
          </Button>
          <Button variant="primary" onClick={handleExtendSession}>
            Continue Working
          </Button>
        </div>
      </div>
    </Modal>
  );
};
