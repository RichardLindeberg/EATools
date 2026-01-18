/**
 * DataEntityListPage
 * List page for Data Entity entities
 */

import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { EntityListTemplate } from '../../components/entity/EntityListTemplate';
import type { ColumnConfig } from '../../components/entity/EntityTable';
import type { FilterDefinition } from '../../components/entity/FilterPanel';
import { useEntityList, useBulkSelection, useEntityActions } from '../../hooks/useEntityList';
import { dataEntitiesApi } from '../../api/entitiesApi';
import type { DataEntity } from '../../types/entities';
import './DataEntityListPage.css';

const COLUMNS: ColumnConfig<DataEntity>[] = [
  { key: 'id', label: 'ID', width: '100px' },
  { key: 'name', label: 'Name', sortable: true },
  { key: 'type', label: 'Type', sortable: true },
  { key: 'format', label: 'Format', width: '120px' },
  { key: 'classification', label: 'Classification', width: '140px', format: (value) => (value ? value.toUpperCase() : '-') },
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
    key: 'classification',
    label: 'Data Classification',
    type: 'select',
    options: [
      { value: '', label: 'All Classifications' },
      { value: 'public', label: 'Public' },
      { value: 'internal', label: 'Internal' },
      { value: 'confidential', label: 'Confidential' },
      { value: 'restricted', label: 'Restricted' },
    ],
  },
  {
    key: 'format',
    label: 'Data Format',
    type: 'select',
    options: [
      { value: '', label: 'All Formats' },
      { value: 'json', label: 'JSON' },
      { value: 'xml', label: 'XML' },
      { value: 'csv', label: 'CSV' },
      { value: 'parquet', label: 'Parquet' },
      { value: 'sql', label: 'SQL' },
    ],
  },
  {
    key: 'owner',
    label: 'Owner',
    type: 'search',
    placeholder: 'Search by owner name...',
  },
];

export const DataEntityListPage: React.FC = () => {
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
      dataEntitiesApi.list({
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

  const handleRowAction = (action: 'view' | 'edit' | 'delete', item: DataEntity) => {
    switch (action) {
      case 'view':
        navigate(`/entities/data-entities/${item.id}`);
        break;
      case 'edit':
        navigate(`/entities/data-entities/${item.id}/edit`);
        break;
      case 'delete':
        if (window.confirm(`Delete data entity "${item.name}"?`)) {
          deleteEntity(async () => dataEntitiesApi.delete(item.id));
        }
        break;
    }
  };

  const handleBulkDelete = async (ids: string[]) => {
    await bulkDelete(async () => dataEntitiesApi.bulkDelete(ids), ids);
    clearSelection();
  };

  return (
    <div className="data-entity-list-page">
      <EntityListTemplate
        title="Data Entities"
        description="Manage all data entities and datasets in your organization"
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
        onCreateNew={() => navigate('/entities/data-entities/new')}
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
