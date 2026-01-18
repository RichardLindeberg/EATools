/**
 * BusinessCapabilityDetailPage
 * Detail page for BusinessCapability entities
 */

import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { EntityDetailTemplate } from '../../components/entity/EntityDetailTemplate';
import type { Property } from '../../components/entity/PropertyGrid';
import { useEntityDetail, useEntityRelationships } from '../../hooks/useEntityDetail';
import type { BusinessCapability } from '../../types/entities';
import { EntityType } from '../../types/entities';
import { DeleteConfirmModal } from '../../components/forms/DeleteConfirmModal';
import { businessCapabilitiesApi } from '../../api/entitiesApi';

export const BusinessCapabilityDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [deleting, setDeleting] = useState(false);

  const { entity, loading, error, isNotFound, isForbidden } = useEntityDetail<BusinessCapability>({
    entityType: EntityType.BUSINESS_CAPABILITY,
    id: id!,
  });

  const { relationships } = useEntityRelationships({
    entityType: EntityType.BUSINESS_CAPABILITY,
    id: id!,
    enabled: !!entity,
  });

  if (!id) {
    return <div>Invalid business capability ID</div>;
  }

  const properties: Property[] = entity
    ? [
        { key: 'id', label: 'ID', value: entity.id },
        { key: 'name', label: 'Name', value: entity.name },
        { key: 'description', label: 'Description', value: entity.description, span: 2 },
        { key: 'level', label: 'Level', value: entity.level },
        { key: 'parentId', label: 'Parent', value: entity.parentId },
        { key: 'owner', label: 'Owner', value: entity.owner },
        { key: 'status', label: 'Status', value: entity.status?.toUpperCase() },
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
          <h3 style={{ marginTop: 0 }}>Business Capability Details</h3>
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

  const handleDeleteConfirm = async (approvalId: string, reason: string) => {
    if (!id) return;
    
    try {
      setDeleting(true);
      await businessCapabilitiesApi.delete(id, approvalId, reason);
      navigate('/entities/business-capabilities');
    } catch (err) {
      console.error('Delete failed:', err);
      setDeleting(false);
      throw err;
    }
  };

  const handleDeleteCancel = () => {
    setDeleteModalOpen(false);
  };

  return (
    <>
      <EntityDetailTemplate
        breadcrumbs={[
          { label: 'Home', path: '/' },
          { label: 'Business Capabilities', path: '/entities/business-capabilities' },
          { label: entity?.name || id },
        ]}
        title={entity?.name || 'Business Capability'}
        badges={entity ? [{ label: entity.status || 'Unknown', variant: 'info' }] : []}
        actions={[
          { label: 'Edit', onClick: () => navigate(`/entities/business-capabilities/${id}/edit`), variant: 'primary' },
          { label: 'Delete', onClick: () => setDeleteModalOpen(true), variant: 'danger' },
          { label: 'Back to List', onClick: () => navigate('/entities/business-capabilities'), variant: 'secondary' },
        ]}
        properties={properties}
        tabs={tabs}
        loading={loading}
        error={error as Error}
        notFound={isNotFound}
        forbidden={isForbidden}
      />
      
      <DeleteConfirmModal
        isOpen={deleteModalOpen}
        entityLabel={entity ? `business capability "${entity.name}"` : 'business capability'}
        loading={deleting}
        onConfirm={handleDeleteConfirm}
        onCancel={handleDeleteCancel}
      />
    </>
  );
};
