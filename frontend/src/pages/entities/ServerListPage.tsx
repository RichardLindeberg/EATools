/**
 * ServerListPage
 * List page for Server entities
 */

import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { EntityListTemplate } from '../../components/entity/EntityListTemplate';
import type { ColumnConfig } from '../../components/entity/EntityTable';
import type { FilterDefinition } from '../../components/entity/FilterPanel';
import { useEntityList, useBulkSelection, useEntityActions } from '../../hooks/useEntityList';
import { serversApi } from '../../api/entitiesApi';
import type { Server } from '../../types/entities';
import { DeleteConfirmModal } from '../../components/forms/DeleteConfirmModal';
import './ServerListPage.css';

const COLUMNS: ColumnConfig<Server>[] = [
  { key: 'id', label: 'ID', width: '100px' },
  { key: 'name', label: 'Name', sortable: true },
  { key: 'hostname', label: 'Hostname', sortable: true },
  { key: 'ipAddress', label: 'IP Address', width: '140px' },
  { key: 'environment', label: 'Environment', width: '120px', format: (value) => (value ? value.toUpperCase() : '-') },
  { key: 'status', label: 'Status', width: '100px', format: (value) => (value ? value.toUpperCase() : '-') },
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
    key: 'environment',
    label: 'Environment',
    type: 'select',
    options: [
      { value: '', label: 'All Environments' },
      { value: 'dev', label: 'Development' },
      { value: 'staging', label: 'Staging' },
      { value: 'prod', label: 'Production' },
      { value: 'test', label: 'Test' },
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
      { value: 'maintenance', label: 'Maintenance' },
      { value: 'decommissioned', label: 'Decommissioned' },
    ],
  },
  {
    key: 'owner',
    label: 'Owner',
    type: 'search',
    placeholder: 'Search by owner name...',
  },
];

export const ServerListPage: React.FC = () => {
  const navigate = useNavigate();
  const [filters, setFilters] = useState<Record<string, any>>({});
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [entityToDelete, setEntityToDelete] = useState<Server | null>(null);

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
      serversApi.list({
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

  const handleRowAction = (action: 'view' | 'edit' | 'delete', item: Server) => {
    switch (action) {
      case 'view':
        navigate(`/entities/servers/${item.id}`);
        break;
      case 'edit':
        navigate(`/entities/servers/${item.id}/edit`);
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
      await deleteEntity(async () => serversApi.delete(entityToDelete.id, approvalId, reason));
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
    await bulkDelete(async () => serversApi.bulkDelete(ids), ids);
    clearSelection();
  };

  return (
    <div className="server-list-page">
      <EntityListTemplate
        title="Servers"
        description="Manage all servers in your infrastructure"
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
        onCreateNew={() => navigate('/entities/servers/new')}
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
        entityLabel={entityToDelete ? `server "${entityToDelete.name}"` : 'server'}
        loading={actionLoading}
        onConfirm={handleDeleteConfirm}
        onCancel={handleDeleteCancel}
      />
    </div>
  );
};
