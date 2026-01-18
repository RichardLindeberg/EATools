/**
 * RelationDetailPage
 * Detail page for Relation entities
 */

import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { EntityDetailTemplate } from '../../components/entity/EntityDetailTemplate';
import type { Property } from '../../components/entity/PropertyGrid';
import { useEntityDetail } from '../../hooks/useEntityDetail';
import type { Relation } from '../../types/entities';
import { EntityType } from '../../types/entities';

export const RelationDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { entity, loading, error, isNotFound, isForbidden } = useEntityDetail<Relation>({
    entityType: EntityType.RELATION,
    id: id!,
  });

  if (!id) {
    return <div>Invalid relation ID</div>;
  }

  const properties: Property[] = entity
    ? [
        { key: 'id', label: 'ID', value: entity.id },
        { key: 'type', label: 'Type', value: entity.type?.toUpperCase() },
        { key: 'sourceEntityType', label: 'Source Entity Type', value: entity.sourceEntityType },
        { key: 'sourceEntityId', label: 'Source Entity ID', value: entity.sourceEntityId },
        { key: 'targetEntityType', label: 'Target Entity Type', value: entity.targetEntityType },
        { key: 'targetEntityId', label: 'Target Entity ID', value: entity.targetEntityId },
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
          <h3 style={{ marginTop: 0 }}>Relation Details</h3>
          {entity && properties.map((prop) => (
            <div key={prop.key} style={{ marginBottom: '1rem' }}>
              <strong>{prop.label}:</strong> {prop.format ? prop.format(prop.value) : prop.value || '—'}
            </div>
          ))}
        </div>
      ),
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
        { label: 'Relations', path: '/entities/relations' },
        { label: entity?.type || id },
      ]}
      title={entity?.type || 'Relation'}
      subtitle={entity ? `${entity.sourceEntityType} → ${entity.targetEntityType}` : undefined}
      badges={entity ? [{ label: entity.type || 'Unknown', variant: 'info' }] : []}
      actions={[
        { label: 'Edit', onClick: () => navigate(`/entities/relations/${id}/edit`), variant: 'primary' },
        { label: 'Delete', onClick: () => console.log('Delete'), variant: 'danger' },
        { label: 'Back to List', onClick: () => navigate('/entities/relations'), variant: 'secondary' },
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
