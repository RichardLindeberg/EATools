/**
 * OrganizationListPage
 * List page for Organization entities
 */

import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { EntityListTemplate } from '../../components/entity/EntityListTemplate';
import type { ColumnConfig } from '../../components/entity/EntityTable';
import type { FilterDefinition } from '../../components/entity/FilterPanel';
import { useEntityList, useBulkSelection, useEntityActions } from '../../hooks/useEntityList';
import { organizationsApi } from '../../api/entitiesApi';
import type { Organization } from '../../types/entities';
import './OrganizationListPage.css';

const COLUMNS: ColumnConfig<Organization>[] = [
  { key: 'id', label: 'ID', width: '100px' },
  { key: 'name', label: 'Name', sortable: true },
  { key: 'type', label: 'Type', width: '150px', format: (value) => (value ? value.toUpperCase() : '-') },
  { key: 'owner', label: 'Owner', width: '150px' },
  { key: 'description', label: 'Description', width: '250px' },
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
    label: 'Organization Type',
    type: 'select',
    options: [
      { value: '', label: 'All Types' },
      { value: 'department', label: 'Department' },
      { value: 'team', label: 'Team' },
      { value: 'business-unit', label: 'Business Unit' },
      { value: 'division', label: 'Division' },
      { value: 'other', label: 'Other' },
    ],
  },
  {
    key: 'owner',
    label: 'Owner',
    type: 'search',
    placeholder: 'Search by owner name...',
  },
];

export const OrganizationListPage: React.FC = () => {
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
    clearFilters: clearQueryFilters,
  } = useEntityList(
    (filterParams) =>
      organizationsApi.list({
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

  const handleRowAction = (action: 'view' | 'edit' | 'delete', item: Organization) => {
    switch (action) {
      case 'view':
        navigate(`/entities/organizations/${item.id}`);
        break;
      case 'edit':
        navigate(`/entities/organizations/${item.id}/edit`);
        break;
      case 'delete':
        if (window.confirm(`Delete organization "${item.name}"?`)) {
          deleteEntity(async () => organizationsApi.delete(item.id));
        }
        break;
    }
  };

  const handleBulkDelete = async (ids: string[]) => {
    await bulkDelete(async () => organizationsApi.bulkDelete(ids), ids);
    clearSelection();
  };

  return (
    <div className="organization-list-page">
      <EntityListTemplate
        title="Organizations"
        description="Manage all organizational units and structures"
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
        onCreateNew={() => navigate('/entities/organizations/new')}
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
