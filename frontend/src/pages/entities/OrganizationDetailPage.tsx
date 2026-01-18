/**
 * OrganizationDetailPage
 * Detail page for Organization entities
 */

import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { EntityDetailTemplate } from '../../components/entity/EntityDetailTemplate';
import type { Property } from '../../components/entity/PropertyGrid';
import { useEntityDetail, useEntityRelationships } from '../../hooks/useEntityDetail';
import type { Organization } from '../../types/entities';
import { EntityType } from '../../types/entities';

export const OrganizationDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { entity, loading, error, isNotFound, isForbidden } = useEntityDetail<Organization>({
    entityType: EntityType.ORGANIZATION,
    id: id!,
  });

  const { relationships } = useEntityRelationships({
    entityType: EntityType.ORGANIZATION,
    id: id!,
    enabled: !!entity,
  });

  if (!id) {
    return <div>Invalid organization ID</div>;
  }

  const properties: Property[] = entity
    ? [
        { key: 'id', label: 'ID', value: entity.id },
        { key: 'name', label: 'Name', value: entity.name },
        { key: 'type', label: 'Type', value: entity.type?.toUpperCase() },
        { key: 'parentId', label: 'Parent', value: entity.parentId },
        { key: 'owner', label: 'Owner', value: entity.owner },
        {
          key: 'createdAt',
          label: 'Created',
          value: entity.createdAt,
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
          <h3 style={{ marginTop: 0 }}>Organization Details</h3>
          {entity && properties.map((prop) => (
            <div key={prop.key} style={{ marginBottom: '1rem' }}>
              <strong>{prop.label}:</strong> {prop.format ? prop.format(prop.value) : prop.value || '—'}
            </div>
          ))}
        </div>
      ),
    },
    {
      id: 'relationships',
      label: 'Relationships',
      badge: relationships.length,
      content: <div><h3 style={{ marginTop: 0 }}>Related Entities</h3><p>{relationships.length === 0 ? 'No relationships found.' : `${relationships.length} relationships`}</p></div>,
    },
    {
      id: 'audit',
      label: 'Audit History',
      content: <div><h3 style={{ marginTop: 0 }}>Change History</h3><p>Audit history coming soon.</p></div>,
    },
  ];

  return (
    <EntityDetailTemplate
      breadcrumbs={[
        { label: 'Home', path: '/' },
        { label: 'Organizations', path: '/entities/organizations' },
        { label: entity?.name || id },
      ]}
      title={entity?.name || 'Organization'}
      badges={entity ? [{ label: entity.type || 'Unknown', variant: 'info' }] : []}
      actions={[
        { label: 'Edit', onClick: () => navigate(`/entities/organizations/${id}/edit`), variant: 'primary' },
        { label: 'Delete', onClick: () => console.log('Delete'), variant: 'danger' },
        { label: 'Back to List', onClick: () => navigate('/entities/organizations'), variant: 'secondary' },
      ]}
      properties={properties}
      tabs={tabs}
      loading={loading}
      error={error as Error}
      notFound={isNotFound}
      forbidden={isForbidden}
    />
  );
};
