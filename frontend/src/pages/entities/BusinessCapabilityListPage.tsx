/**
 * BusinessCapabilityListPage
 * List page for Business Capability entities
 */

import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { EntityListTemplate } from '../../components/entity/EntityListTemplate';
import { ColumnConfig } from '../../components/entity/EntityTable';
import { FilterDefinition } from '../../components/entity/FilterPanel';
import { useEntityList, useBulkSelection, useEntityActions } from '../../hooks/useEntityList';
import { businessCapabilitiesApi } from '../../api/entitiesApi';
import { BusinessCapability } from '../../types/entities';
import './BusinessCapabilityListPage.css';

const COLUMNS: ColumnConfig<BusinessCapability>[] = [
  { key: 'id', label: 'ID', width: '100px' },
  { key: 'name', label: 'Name', sortable: true },
  { key: 'description', label: 'Description', width: '250px' },
  { key: 'level', label: 'Level', width: '120px', format: (value) => (value ? value.toUpperCase() : '-') },
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
    key: 'level',
    label: 'Capability Level',
    type: 'select',
    options: [
      { value: '', label: 'All Levels' },
      { value: 'strategic', label: 'Strategic' },
      { value: 'core', label: 'Core' },
      { value: 'supporting', label: 'Supporting' },
      { value: 'management', label: 'Management' },
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
      { value: 'planned', label: 'Planned' },
      { value: 'deprecated', label: 'Deprecated' },
    ],
  },
  {
    key: 'owner',
    label: 'Owner',
    type: 'search',
    placeholder: 'Search by owner name...',
  },
];

export const BusinessCapabilityListPage: React.FC = () => {
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
      businessCapabilitiesApi.list({
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

  const handleRowAction = (action: 'view' | 'edit' | 'delete', item: BusinessCapability) => {
    switch (action) {
      case 'view':
        navigate(`/entities/business-capabilities/${item.id}`);
        break;
      case 'edit':
        navigate(`/entities/business-capabilities/${item.id}/edit`);
        break;
      case 'delete':
        if (window.confirm(`Delete business capability "${item.name}"?`)) {
          deleteEntity(async () => businessCapabilitiesApi.delete(item.id));
        }
        break;
    }
  };

  const handleBulkDelete = async (ids: string[]) => {
    await bulkDelete(async () => businessCapabilitiesApi.bulkDelete(ids), ids);
    clearSelection();
  };

  return (
    <div className="business-capability-list-page">
      <EntityListTemplate
        title="Business Capabilities"
        description="Manage all business capabilities aligned with your enterprise architecture"
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
        onCreateNew={() => navigate('/entities/business-capabilities/new')}
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
