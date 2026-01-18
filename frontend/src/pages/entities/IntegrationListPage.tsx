/**
 * IntegrationListPage
 * List page for Integration entities
 */

import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { EntityListTemplate } from '../../components/entity/EntityListTemplate';
import type { ColumnConfig } from '../../components/entity/EntityTable';
import type { FilterDefinition } from '../../components/entity/FilterPanel';
import { useEntityList, useBulkSelection, useEntityActions } from '../../hooks/useEntityList';
import { integrationsApi } from '../../api/entitiesApi';
import type { Integration } from '../../types/entities';
import { DeleteConfirmModal } from '../../components/forms/DeleteConfirmModal';
import './IntegrationListPage.css';

const COLUMNS: ColumnConfig<Integration>[] = [
  { key: 'id', label: 'ID', width: '100px' },
  { key: 'name', label: 'Name', sortable: true },
  { key: 'type', label: 'Type', width: '120px', format: (value) => (value ? value.toUpperCase() : '-') },
  { key: 'protocol', label: 'Protocol', width: '120px' },
  { key: 'status', label: 'Status', width: '100px', format: (value) => (value ? value.toUpperCase() : '-') },
  { key: 'frequency', label: 'Frequency', width: '120px' },
  { key: 'owner', label: 'Owner', width: '150px' },
  {
    key: 'createdAt',
    label: 'Created',
    width: '150px',
    format: (value) => (value ? new Date(value as string).toLocaleDateString() : '-'),
  },
];

const FILTERS: FilterDefinition[] = [
  {
    key: 'type',
    label: 'Integration Type',
    type: 'select',
    options: [
      { value: '', label: 'All Types' },
      { value: 'api', label: 'API' },
      { value: 'message-queue', label: 'Message Queue' },
      { value: 'database', label: 'Database' },
      { value: 'file-transfer', label: 'File Transfer' },
      { value: 'other', label: 'Other' },
    ],
  },
  {
    key: 'status',
    label: 'Status',
    type: 'select',
    options: [
      { value: '', label: 'All Statuses' },
      { value: 'active', label: 'Active' },
      { value: 'inactive', label: 'Inactive' },
      { value: 'error', label: 'Error' },
      { value: 'maintenance', label: 'Maintenance' },
    ],
  },
  {
    key: 'owner',
    label: 'Owner',
    type: 'search',
    placeholder: 'Search by owner name...',
  },
];

export const IntegrationListPage: React.FC = () => {
  const navigate = useNavigate();
  const [filters, setFilters] = useState<Record<string, any>>({});
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [entityToDelete, setEntityToDelete] = useState<Integration | null>(null);

  const {
    items,
    total,
    loading,
    error,
    params,
    setPage,
    setLimit,
    setSort,
    clearFilters: clearQueryFilters,
    refetch,
  } = useEntityList(
    (filterParams) =>
      integrationsApi.list({
        skip: (filterParams.page - 1) * filterParams.limit,
        take: filterParams.limit,
        sort: filterParams.sort,
        order: filterParams.order as 'asc' | 'desc',
        ...filters,
      }),
    {
      defaultLimit: 10,
      defaultSort: 'name',
    }
  );

  const {
    selectedIds,
    toggleSelect,
    selectAll,
    clearSelection,
  } = useBulkSelection();

  const { deleteEntity, bulkDelete, loading: actionLoading } = useEntityActions();

  const handleFilterChange = (key: string, value: any) => {
    setFilters((prev) => ({
      ...prev,
      [key]: value,
    }));
    setPage(1);
  };

  const handleClearFilters = () => {
    setFilters({});
    clearQueryFilters();
  };

  const handleSort = (key: string) => {
    if (params.sort === key) {
      setSort(key, params.order === 'asc' ? 'desc' : 'asc');
    } else {
      setSort(key, 'asc');
    }
  };

  const handleRowAction = (action: 'view' | 'edit' | 'delete', item: Integration) => {
    switch (action) {
      case 'view':
        navigate(`/entities/integrations/${item.id}`);
        break;
      case 'edit':
        navigate(`/entities/integrations/${item.id}/edit`);
        break;
      case 'delete':
        setEntityToDelete(item);
        setDeleteModalOpen(true);
        break;
    }
  };

  const handleDeleteConfirm = async (approvalId: string, reason: string) => {
    if (!entityToDelete) return;
    
    try {
      await deleteEntity(async () => integrationsApi.delete(entityToDelete.id, approvalId, reason));
      setDeleteModalOpen(false);
      setEntityToDelete(null);
      refetch();
    } catch (err) {
      console.error('Delete failed:', err);
      throw err;
    }
  };

  const handleDeleteCancel = () => {
    setDeleteModalOpen(false);
    setEntityToDelete(null);
  };

  const handleBulkDelete = async (ids: string[]) => {
    await bulkDelete(async () => integrationsApi.bulkDelete(ids), ids);
    clearSelection();
  };

  return (
    <div className="integration-list-page">
      <EntityListTemplate
        title="Integrations"
        description="Manage all integrations between systems and applications"
        items={items}
        columns={COLUMNS}
        loading={loading || actionLoading}
        error={error}
        total={total}
        currentPage={params.page}
        pageSize={params.limit}
        currentSort={params.sort}
        currentOrder={params.order as 'asc' | 'desc'}
        filters={FILTERS}
        filterValues={filters}
        selectedIds={selectedIds}
        onCreateNew={() => navigate('/entities/integrations/new')}
        onPageChange={setPage}
        onPageSizeChange={setLimit}
        onSort={handleSort}
        onRowAction={handleRowAction}
        onFilterChange={handleFilterChange}
        onClearFilters={handleClearFilters}
        onSelectRow={(id) => toggleSelect(id)}
        onSelectAll={() => selectAll(items.map((item) => item.id))}
        onClearSelection={clearSelection}
        onBulkDelete={handleBulkDelete}
        showBulkActions
      />
      
      <DeleteConfirmModal
        isOpen={deleteModalOpen}
        entityLabel={entityToDelete ? `integration "${entityToDelete.name}"` : 'integration'}
        loading={actionLoading}
        onConfirm={handleDeleteConfirm}
        onCancel={handleDeleteCancel}
      />
    </div>
  );
};
