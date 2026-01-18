/**
 * RelationListPage
 * List page for Relation entities
 */

import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { EntityListTemplate } from '../../components/entity/EntityListTemplate';
import type { ColumnConfig } from '../../components/entity/EntityTable';
import type { FilterDefinition } from '../../components/entity/FilterPanel';
import { useEntityList, useBulkSelection, useEntityActions } from '../../hooks/useEntityList';
import { relationsApi } from '../../api/entitiesApi';
import type { Relation } from '../../types/entities';
import './RelationListPage.css';

const COLUMNS: ColumnConfig<Relation>[] = [
  { key: 'id', label: 'ID', width: '100px' },
  { key: 'type', label: 'Relation Type', sortable: true, format: (value) => (value ? value.toUpperCase() : '-') },
  { key: 'sourceEntityType', label: 'From Entity Type', width: '140px' },
  { key: 'targetEntityType', label: 'To Entity Type', width: '140px' },
  { key: 'strength', label: 'Strength', width: '100px' },
  { key: 'description', label: 'Description', width: '200px' },
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
    label: 'Relation Type',
    type: 'select',
    options: [
      { value: '', label: 'All Types' },
      { value: 'implements', label: 'Implements' },
      { value: 'supports', label: 'Supports' },
      { value: 'uses', label: 'Uses' },
      { value: 'communicates-with', label: 'Communicates With' },
      { value: 'depends-on', label: 'Depends On' },
      { value: 'other', label: 'Other' },
    ],
  },
  {
    key: 'strength',
    label: 'Relation Strength',
    type: 'select',
    options: [
      { value: '', label: 'All Strengths' },
      { value: 'strong', label: 'Strong' },
      { value: 'medium', label: 'Medium' },
      { value: 'weak', label: 'Weak' },
    ],
  },
];

export const RelationListPage: React.FC = () => {
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
      relationsApi.list({
        skip: (filterParams.page - 1) * filterParams.limit,
        take: filterParams.limit,
        sort: filterParams.sort,
        order: filterParams.order as 'asc' | 'desc',
        ...filters,
      }),
    {
      defaultLimit: 10,
      defaultSort: 'type',
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

  const handleRowAction = (action: 'view' | 'edit' | 'delete', item: Relation) => {
    switch (action) {
      case 'view':
        navigate(`/entities/relations/${item.id}`);
        break;
      case 'edit':
        navigate(`/entities/relations/${item.id}/edit`);
        break;
      case 'delete':
        if (window.confirm(`Delete relation "${item.type}"?`)) {
          deleteEntity(async () => relationsApi.delete(item.id));
        }
        break;
    }
  };

  const handleBulkDelete = async (ids: string[]) => {
    await bulkDelete(async () => relationsApi.bulkDelete(ids), ids);
    clearSelection();
  };

  return (
    <div className="relation-list-page">
      <EntityListTemplate
        title="Relations"
        description="Manage all relationships between entities in your architecture"
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
        onCreateNew={() => navigate('/entities/relations/new')}
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
