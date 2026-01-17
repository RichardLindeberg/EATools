/**
 * Logout Page Component
 * Provides logout confirmation and handles logout process
 */

import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { Card } from '../../components/Card/Card';
import { Button } from '../../components/Button/Button';
import './LogoutPage.css';

export const LogoutPage: React.FC = () => {
  const navigate = useNavigate();
  const { logout, user } = useAuth();
  const [isLoggingOut, setIsLoggingOut] = useState(false);

  const handleLogout = async () => {
    setIsLoggingOut(true);

    try {
      await logout();
      navigate('/auth/login', { replace: true });
    } catch (error) {
      console.error('Logout error:', error);
      // Even if logout API fails, still redirect to login
      navigate('/auth/login', { replace: true });
    }
  };

  const handleCancel = () => {
    navigate(-1); // Go back to previous page
  };

  return (
    <div className="logout-page">
      <div className="logout-container">
        <Card>
          <div className="logout-content">
            <div className="logout-icon">
              <svg
                width="48"
                height="48"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
              >
                <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
                <polyline points="16 17 21 12 16 7" />
                <line x1="21" y1="12" x2="9" y2="12" />
              </svg>
            </div>

            <h1 className="logout-title">Logout Confirmation</h1>

            {user && (
              <p className="logout-user">
                Logged in as: <strong>{user.email}</strong>
              </p>
            )}

            <p className="logout-message">
              Are you sure you want to logout? You will need to sign in again to access your account.
            </p>

            <div className="logout-actions">
              <Button
                variant="secondary"
                onClick={handleCancel}
                disabled={isLoggingOut}
                fullWidth
              >
                Cancel
              </Button>
              <Button
                variant="danger"
                onClick={handleLogout}
                disabled={isLoggingOut}
                fullWidth
              >
                {isLoggingOut ? 'Logging out...' : 'Logout'}
              </Button>
            </div>
          </div>
        </Card>
      </div>
    </div>
  );
};
