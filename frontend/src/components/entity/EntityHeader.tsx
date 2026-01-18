/**
 * EntityHeader Component
 * Header section for entity detail pages with breadcrumbs, title, and actions
 */

import React from 'react';
import { Link } from 'react-router-dom';
import './EntityHeader.css';

export interface Breadcrumb {
  label: string;
  path?: string;
}

export interface Badge {
  label: string;
  variant?: 'default' | 'success' | 'warning' | 'error' | 'info';
}

export interface Action {
  label: string;
  onClick: () => void;
  variant?: 'primary' | 'secondary' | 'danger';
  disabled?: boolean;
  icon?: React.ReactNode;
}

interface EntityHeaderProps {
  breadcrumbs: Breadcrumb[];
  title: string;
  subtitle?: string;
  badges?: Badge[];
  actions?: Action[];
}

export const EntityHeader: React.FC<EntityHeaderProps> = ({
  breadcrumbs,
  title,
  subtitle,
  badges = [],
  actions = [],
}) => {
  const getBadgeClassName = (variant: Badge['variant'] = 'default') => {
    return `entity-badge entity-badge-${variant}`;
  };

  const getActionClassName = (variant: Action['variant'] = 'secondary') => {
    return `entity-action entity-action-${variant}`;
  };

  return (
    <div className="entity-header">
      {/* Breadcrumbs */}
      <nav className="entity-breadcrumbs" aria-label="Breadcrumb">
        <ol className="breadcrumb-list">
          {breadcrumbs.map((crumb, index) => (
            <li key={index} className="breadcrumb-item">
              {crumb.path ? (
                <Link to={crumb.path} className="breadcrumb-link">
                  {crumb.label}
                </Link>
              ) : (
                <span className="breadcrumb-current">{crumb.label}</span>
              )}
              {index < breadcrumbs.length - 1 && (
                <span className="breadcrumb-separator" aria-hidden="true">
                  /
                </span>
              )}
            </li>
          ))}
        </ol>
      </nav>

      {/* Title and Badges */}
      <div className="entity-header-main">
        <div className="entity-header-title-section">
          <h1 className="entity-title">{title}</h1>
          {badges.length > 0 && (
            <div className="entity-badges">
              {badges.map((badge, index) => (
                <span key={index} className={getBadgeClassName(badge.variant)}>
                  {badge.label}
                </span>
              ))}
            </div>
          )}
        </div>

        {/* Actions */}
        {actions.length > 0 && (
          <div className="entity-actions">
            {actions.map((action, index) => (
              <button
                key={index}
                onClick={action.onClick}
                className={getActionClassName(action.variant)}
                disabled={action.disabled}
                type="button"
              >
                {action.icon && <span className="action-icon">{action.icon}</span>}
                <span>{action.label}</span>
              </button>
            ))}
          </div>
        )}
      </div>

      {/* Subtitle */}
      {subtitle && <p className="entity-subtitle">{subtitle}</p>}
    </div>
  );
};
