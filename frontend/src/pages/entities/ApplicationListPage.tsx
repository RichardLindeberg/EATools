/**
 * ApplicationListPage
 * List page for Application entities
 */

import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { EntityListTemplate } from '../../components/entity/EntityListTemplate';
import type { ColumnConfig } from '../../components/entity/EntityTable';
import type { FilterDefinition } from '../../components/entity/FilterPanel';
import { useEntityList, useBulkSelection, useEntityActions } from '../../hooks/useEntityList';
import { applicationsApi } from '../../api/entitiesApi';
import type { Application } from '../../types/entities';
import './ApplicationListPage.css';

const COLUMNS: ColumnConfig<Application>[] = [
  { key: 'id', label: 'ID', width: '100px' },
  { key: 'name', label: 'Name', sortable: true },
  { key: 'type', label: 'Type', width: '120px', format: (value) => (value ? value.toUpperCase() : '-') },
  { key: 'status', label: 'Status', width: '100px', format: (value) => (value ? value.toUpperCase() : '-') },
  { key: 'owner', label: 'Owner', width: '150px' },
  {
    key: 'createdAt',
    label: 'Created',
    width: '150px',
    format: (value) => (value ? new Date(value as string).toLocaleDateString() : '-'),
  },
  {
    key: 'updatedAt',
    label: 'Updated',
    width: '150px',
    format: (value) => (value ? new Date(value as string).toLocaleDateString() : '-'),
  },
];

const FILTERS: FilterDefinition[] = [
  {
    key: 'type',
    label: 'Application Type',
    type: 'select',
    options: [
      { value: '', label: 'All Types' },
      { value: 'web', label: 'Web' },
      { value: 'mobile', label: 'Mobile' },
      { value: 'desktop', label: 'Desktop' },
      { value: 'backend', label: 'Backend' },
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
      { value: 'deprecated', label: 'Deprecated' },
      { value: 'planned', label: 'Planned' },
    ],
  },
  {
    key: 'owner',
    label: 'Owner',
    type: 'search',
    placeholder: 'Search by owner name...',
  },
];

export const ApplicationListPage: React.FC = () => {
  const navigate = useNavigate();
  const [filters, setFilters] = useState<Record<string, any>>({});

  const {
    items,
    total,
    loading,
    error,
    params,
    setPage,
    setLimit,
    setSort,
    setSearch,
    clearFilters: clearQueryFilters,
  } = useEntityList(
    (filterParams) =>
      applicationsApi.list({
        skip: (filterParams.page - 1) * filterParams.limit,
        take: filterParams.limit,
        sort: filterParams.sort,
        order: filterParams.order as 'asc' | 'desc',
        search: filterParams.search,
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
    setPage(1); // Reset to first page when filtering
  };

  const handleClearFilters = () => {
    setFilters({});
    setSearch('');
    clearQueryFilters();
  };

  const handleSort = (key: string) => {
    if (params.sort === key) {
      setSort(key, params.order === 'asc' ? 'desc' : 'asc');
    } else {
      setSort(key, 'asc');
    }
  };

  const handleRowAction = (action: 'view' | 'edit' | 'delete', item: Application) => {
    switch (action) {
      case 'view':
        navigate(`/entities/applications/${item.id}`);
        break;
      case 'edit':
        navigate(`/entities/applications/${item.id}/edit`);
        break;
      case 'delete':
        // Applications require approval_id and reason for deletion
        // In production, this would open a modal to get the approval_id
        const approvalId = window.prompt('Enter approval ID for deletion:');
        if (approvalId) {
          const reason = window.prompt('Enter reason for deletion:') || 'User requested deletion';
          deleteEntity(async () => applicationsApi.delete(item.id, approvalId, reason));
        }
        break;
    }
  };

  const handleBulkDelete = async (ids: string[]) => {
    await bulkDelete(async () => applicationsApi.bulkDelete(ids), ids);
    clearSelection();
  };

  return (
    <div className="application-list-page">
      <EntityListTemplate
        title="Applications"
        description="Manage all registered applications in your enterprise architecture"
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
        onCreateNew={() => navigate('/entities/applications/new')}
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
    </div>
  );
};
