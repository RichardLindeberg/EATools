/**
 * UnauthorizedPage Component
 * Displays 403 Forbidden page when user lacks permissions
 */

import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '../components/Button/Button';
import './UnauthorizedPage.css';

export const UnauthorizedPage: React.FC = () => {
  const navigate = useNavigate();

  return (
    <div className="error-page unauthorized-page">
      <div className="error-container">
        <div className="error-content">
          <div className="error-code">403</div>
          <h1>Access Denied</h1>
          <p>You don't have permission to access this resource.</p>
          <p className="error-info">
            If you believe this is a mistake, please contact your administrator.
          </p>
          <div className="error-actions">
            <Button
              onClick={() => navigate('/')}
              variant="primary"
            >
              Go to Home
            </Button>
            <Button
              onClick={() => navigate(-1)}
              variant="secondary"
            >
              Go Back
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
};
