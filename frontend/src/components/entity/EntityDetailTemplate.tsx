/**
 * EntityDetailTemplate Component
 * Reusable template for entity detail pages with tabs
 */

import React, { useState } from 'react';
import { EntityHeader } from './EntityHeader';
import type { Breadcrumb, Badge, Action } from './EntityHeader';
import { PropertyGrid } from './PropertyGrid';
import type { Property } from './PropertyGrid';
import './EntityDetailTemplate.css';

export interface Tab {
  id: string;
  label: string;
  content: React.ReactNode;
  badge?: string | number;
}

interface EntityDetailTemplateProps {
  breadcrumbs: Breadcrumb[];
  title: string;
  subtitle?: string;
  badges?: Badge[];
  actions?: Action[];
  properties: Property[];
  tabs?: Tab[];
  loading?: boolean;
  error?: Error | null;
  notFound?: boolean;
  forbidden?: boolean;
}

export const EntityDetailTemplate: React.FC<EntityDetailTemplateProps> = ({
  breadcrumbs,
  title,
  subtitle,
  badges,
  actions,
  properties,
  tabs = [],
  loading = false,
  error = null,
  notFound = false,
  forbidden = false,
}) => {
  const [activeTabId, setActiveTabId] = useState<string>(tabs[0]?.id || 'overview');

  // Loading state
  if (loading) {
    return (
      <div className="entity-detail">
        <div className="entity-detail-loading">
          <div className="loading-spinner" />
          <p>Loading...</p>
        </div>
      </div>
    );
  }

  // Not found state
  if (notFound) {
    return (
      <div className="entity-detail">
        <div className="entity-detail-error">
          <h2>Entity Not Found</h2>
          <p>The entity you're looking for doesn't exist or has been deleted.</p>
        </div>
      </div>
    );
  }

  // Forbidden state
  if (forbidden) {
    return (
      <div className="entity-detail">
        <div className="entity-detail-error">
          <h2>Access Denied</h2>
          <p>You don't have permission to view this entity.</p>
        </div>
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className="entity-detail">
        <div className="entity-detail-error">
          <h2>Error Loading Entity</h2>
          <p>{error.message || 'An unexpected error occurred.'}</p>
        </div>
      </div>
    );
  }

  const activeTab = tabs.find((tab) => tab.id === activeTabId) || tabs[0];

  return (
    <div className="entity-detail">
      <EntityHeader
        breadcrumbs={breadcrumbs}
        title={title}
        subtitle={subtitle}
        badges={badges}
        actions={actions}
      />

      <div className="entity-detail-content">
        {/* Tabs Navigation (if tabs are provided) */}
        {tabs.length > 0 && (
          <div className="entity-tabs">
            <div className="tabs-nav" role="tablist">
              {tabs.map((tab) => (
                <button
                  key={tab.id}
                  role="tab"
                  aria-selected={activeTabId === tab.id}
                  aria-controls={`tab-panel-${tab.id}`}
                  id={`tab-${tab.id}`}
                  className={`tab-button ${activeTabId === tab.id ? 'active' : ''}`}
                  onClick={() => setActiveTabId(tab.id)}
                  type="button"
                >
                  {tab.label}
                  {tab.badge !== undefined && <span className="tab-badge">{tab.badge}</span>}
                </button>
              ))}
            </div>

            {/* Tab Content */}
            <div
              role="tabpanel"
              id={`tab-panel-${activeTabId}`}
              aria-labelledby={`tab-${activeTabId}`}
              className="tab-content"
            >
              {activeTab?.content}
            </div>
          </div>
        )}

        {/* Default Properties Grid (if no tabs) */}
        {tabs.length === 0 && (
          <div className="entity-detail-body">
            <PropertyGrid properties={properties} />
          </div>
        )}
      </div>
    </div>
  );
};
