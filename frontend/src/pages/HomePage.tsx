/**
 * HomePage Component
 * Displays application dashboard/home
 */

import React from 'react';
import { useAuth } from '../hooks/useAuth';
import { Card } from '../components/Card/Card';
import './HomePage.css';

export const HomePage: React.FC = () => {
  const { user, isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return (
      <div className="home-page">
        <div className="home-container">
          <Card>
            <div className="home-welcome">
              <h1>Welcome to EATool</h1>
              <p>Enterprise Architecture Tool</p>
              <p>Please log in to continue.</p>
            </div>
          </Card>
        </div>
      </div>
    );
  }

  return (
    <div className="home-page">
      <div className="home-container">
        <div className="home-header">
          <h1>Dashboard</h1>
          {user && <p className="home-welcome-text">Welcome, {user.name}!</p>}
        </div>

        <div className="home-grid">
          <Card>
            <div className="home-card-content">
              <h2>Applications</h2>
              <p>Manage enterprise applications</p>
            </div>
          </Card>

          <Card>
            <div className="home-card-content">
              <h2>Servers</h2>
              <p>Manage infrastructure servers</p>
            </div>
          </Card>

          <Card>
            <div className="home-card-content">
              <h2>Integrations</h2>
              <p>Manage system integrations</p>
            </div>
          </Card>

          <Card>
            <div className="home-card-content">
              <h2>Data Entities</h2>
              <p>Manage data entities and relationships</p>
            </div>
          </Card>

          <Card>
            <div className="home-card-content">
              <h2>Business Capabilities</h2>
              <p>Manage business capabilities</p>
            </div>
          </Card>

          <Card>
            <div className="home-card-content">
              <h2>Organizations</h2>
              <p>Manage organizational structures</p>
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
};
