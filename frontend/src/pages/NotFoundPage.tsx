/**
 * NotFoundPage Component
 * Displays 404 error page for routes that don't exist
 */

import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '../components/Button/Button';
import './NotFoundPage.css';

export const NotFoundPage: React.FC = () => {
  const navigate = useNavigate();

  return (
    <div className="error-page not-found-page">
      <div className="error-container">
        <div className="error-content">
          <div className="error-code">404</div>
          <h1>Page Not Found</h1>
          <p>The page you are looking for doesn't exist or has been moved.</p>
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
