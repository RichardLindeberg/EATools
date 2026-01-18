/**
 * ApplicationDetailPage
 * Detail page for Application entities
 */

import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { EntityDetailTemplate } from '../../components/entity/EntityDetailTemplate';
import type { Property } from '../../components/entity/PropertyGrid';
import { useEntityDetail, useEntityRelationships } from '../../hooks/useEntityDetail';
import type { Application } from '../../types/entities';
import { EntityType } from '../../types/entities';

export const ApplicationDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { entity, loading, error, isNotFound, isForbidden } = useEntityDetail<Application>({
    entityType: EntityType.APPLICATION,
    id: id!,
  });

  const { relationships, loading: loadingRelationships } = useEntityRelationships({
    entityType: EntityType.APPLICATION,
    id: id!,
    enabled: !!entity,
  });

  if (!id) {
    return <div>Invalid application ID</div>;
  }

  const properties: Property[] = entity
    ? [
        { key: 'id', label: 'ID', value: entity.id },
        { key: 'name', label: 'Name', value: entity.name },
        { key: 'description', label: 'Description', value: entity.description, span: 2 },
        {
          key: 'type',
          label: 'Type',
          value: entity.type,
          format: (value) => value?.toUpperCase() || '—',
        },
        {
          key: 'status',
          label: 'Status',
          value: entity.status,
          format: (value) => value?.toUpperCase() || '—',
        },
        { key: 'version', label: 'Version', value: entity.version },
        { key: 'owner', label: 'Owner', value: entity.owner },
        {
          key: 'createdAt',
          label: 'Created',
          value: entity.createdAt,
          format: (value) => (value ? new Date(value).toLocaleString() : '—'),
        },
        {
          key: 'updatedAt',
          label: 'Updated',
          value: entity.updatedAt,
          format: (value) => (value ? new Date(value).toLocaleString() : '—'),
        },
      ]
    : [];

  const tabs = [
    {
      id: 'overview',
      label: 'Overview',
      content: (
        <div>
          <h3 style={{ marginTop: 0 }}>Application Details</h3>
          {entity && (
            <div>
              {properties.map((prop) => (
                <div key={prop.key} style={{ marginBottom: '1rem' }}>
                  <strong>{prop.label}:</strong>{' '}
                  {prop.format ? prop.format(prop.value) : prop.value || '—'}
                </div>
              ))}
            </div>
          )}
        </div>
      ),
    },
    {
      id: 'relationships',
      label: 'Relationships',
      badge: relationships.length,
      content: (
        <div>
          <h3 style={{ marginTop: 0 }}>Related Entities</h3>
          {loadingRelationships ? (
            <p>Loading relationships...</p>
          ) : relationships.length === 0 ? (
            <p>No relationships found.</p>
          ) : (
            <ul>
              {relationships.map((rel) => (
                <li key={rel.id}>
                  {rel.type}: {rel.targetEntityType} #{rel.targetEntityId}
                </li>
              ))}
            </ul>
          )}
        </div>
      ),
    },
    {
      id: 'audit',
      label: 'Audit History',
      content: (
        <div>
          <h3 style={{ marginTop: 0 }}>Change History</h3>
          <p>Audit history will be implemented when event sourcing integration is complete.</p>
        </div>
      ),
    },
  ];

  const badges = entity
    ? [
        { label: entity.status || 'Unknown', variant: 'info' as const },
        { label: entity.type || 'Unknown', variant: 'default' as const },
      ]
    : [];

  const actions = [
    {
      label: 'Edit',
      onClick: () => navigate(`/entities/applications/${id}/edit`),
      variant: 'primary' as const,
    },
    {
      label: 'Delete',
      onClick: () => {
        if (confirm('Are you sure you want to delete this application?')) {
          // TODO: Implement delete functionality
          console.log('Delete application', id);
        }
      },
      variant: 'danger' as const,
    },
    {
      label: 'Back to List',
      onClick: () => navigate('/entities/applications'),
      variant: 'secondary' as const,
    },
  ];

  return (
    <EntityDetailTemplate
      breadcrumbs={[
        { label: 'Home', path: '/' },
        { label: 'Applications', path: '/entities/applications' },
        { label: entity?.name || id },
      ]}
      title={entity?.name || 'Application'}
      subtitle={entity?.description}
      badges={badges}
      actions={actions}
      properties={properties}
      tabs={tabs}
      loading={loading}
      error={error as Error}
      notFound={isNotFound}
      forbidden={isForbidden}
    />
  );
};
