/**
 * EntityListTemplate Component
 * Reusable template for entity list pages
 * Combines: Header + FilterPanel + EntityTable + Pagination + BulkActionBar
 */

import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '../Button/Button';
import { EntityTable, ColumnConfig } from './EntityTable';
import { FilterPanel, FilterDefinition } from './FilterPanel';
import { BulkActionBar } from './BulkActionBar';
import { Pagination } from '../Navigation/Pagination';
import './EntityListTemplate.css';

export interface EntityListTemplateProps<T> {
  title: string;
  description?: string;
  items: T[];
  columns: ColumnConfig<T>[];
  loading: boolean;
  error?: string | null;
  total: number;
  currentPage: number;
  pageSize: number;
  currentSort?: string;
  currentOrder?: 'asc' | 'desc';
  filters?: FilterDefinition[];
  filterValues?: Record<string, any>;
  selectedIds: Set<string>;
  onCreateNew?: () => void;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
  onSort?: (key: string) => void;
  onRowClick?: (item: T) => void;
  onRowAction?: (action: 'view' | 'edit' | 'delete', item: T) => void;
  onFilterChange?: (key: string, value: any) => void;
  onClearFilters?: () => void;
  onSelectRow?: (id: string) => void;
  onSelectAll?: (ids: string[]) => void;
  onClearSelection?: () => void;
  onBulkDelete?: (ids: string[]) => Promise<void>;
  onBulkArchive?: (ids: string[]) => Promise<void>;
  onBulkExport?: (ids: string[]) => void;
  showBulkActions?: boolean;
  createNewRoute?: string;
  editRoute?: (id: string) => string;
  viewRoute?: (id: string) => string;
}

export const EntityListTemplate = React.forwardRef<
  HTMLDivElement,
  EntityListTemplateProps<any>
>(
  (
    {
      title,
      description,
      items,
      columns,
      loading,
      error,
      total,
      currentPage,
      pageSize,
      currentSort,
      currentOrder,
      filters,
      filterValues = {},
      selectedIds,
      onCreateNew,
      onPageChange,
      onPageSizeChange,
      onSort,
      onRowClick,
      onRowAction,
      onFilterChange,
      onClearFilters,
      onSelectRow,
      onSelectAll,
      onClearSelection,
      onBulkDelete,
      onBulkArchive,
      onBulkExport,
      showBulkActions = true,
      createNewRoute,
      editRoute,
      viewRoute,
    },
    ref
  ) => {
    const navigate = useNavigate();
    const [bulkDeleteLoading, setBulkDeleteLoading] = useState(false);
    const [bulkDeleteError, setBulkDeleteError] = useState<string | null>(null);

    const totalPages = Math.ceil(total / pageSize);
    const selectedArray = Array.from(selectedIds);

    const handleRowAction = (action: 'view' | 'edit' | 'delete', item: any) => {
      if (onRowAction) {
        onRowAction(action, item);
        return;
      }

      switch (action) {
        case 'view':
          if (viewRoute) {
            navigate(viewRoute(item.id));
          }
          break;
        case 'edit':
          if (editRoute) {
            navigate(editRoute(item.id));
          }
          break;
        case 'delete':
          if (window.confirm(`Are you sure you want to delete this item?`)) {
            // Delete logic handled by parent
          }
          break;
      }
    };

    const handleBulkDelete = async () => {
      if (!onBulkDelete || selectedArray.length === 0) return;

      if (!window.confirm(`Delete ${selectedArray.length} selected items?`)) {
        return;
      }

      setBulkDeleteLoading(true);
      setBulkDeleteError(null);

      try {
        await onBulkDelete(selectedArray);
        onClearSelection?.();
      } catch (err) {
        setBulkDeleteError(
          err instanceof Error ? err.message : 'Failed to delete items'
        );
      } finally {
        setBulkDeleteLoading(false);
      }
    };

    const handleBulkArchive = async () => {
      if (!onBulkArchive || selectedArray.length === 0) return;

      setBulkDeleteLoading(true);
      setBulkDeleteError(null);

      try {
        await onBulkArchive(selectedArray);
        onClearSelection?.();
      } catch (err) {
        setBulkDeleteError(
          err instanceof Error ? err.message : 'Failed to archive items'
        );
      } finally {
        setBulkDeleteLoading(false);
      }
    };

    const handleBulkExport = () => {
      if (!onBulkExport || selectedArray.length === 0) return;
      onBulkExport(selectedArray);
    };

    return (
      <div ref={ref} className="entity-list-template">
        {/* Header */}
        <div className="entity-list-template__header">
          <div className="entity-list-template__title-section">
            <h1 className="entity-list-template__title">{title}</h1>
            {description && (
              <p className="entity-list-template__description">{description}</p>
            )}
          </div>

          {(onCreateNew || createNewRoute) && (
            <Button
              variant="primary"
              onClick={
                onCreateNew
                  ? onCreateNew
                  : () => createNewRoute && navigate(createNewRoute)
              }
              disabled={loading}
            >
              Create New
            </Button>
          )}
        </div>

        {/* Error Message */}
        {(error || bulkDeleteError) && (
          <div className="entity-list-template__error">
            <span className="entity-list-template__error-icon">⚠</span>
            <span className="entity-list-template__error-text">
              {error || bulkDeleteError}
            </span>
          </div>
        )}

        {/* Bulk Action Bar */}
        {showBulkActions && (
          <BulkActionBar
            selectedCount={selectedArray.length}
            totalCount={total}
            onDelete={onBulkDelete ? handleBulkDelete : undefined}
            onArchive={onBulkArchive ? handleBulkArchive : undefined}
            onExport={onBulkExport ? handleBulkExport : undefined}
            onClearSelection={() => onClearSelection?.()}
            loading={bulkDeleteLoading}
            visible={selectedArray.length > 0}
          />
        )}

        {/* Filters */}
        {filters && filters.length > 0 && (
          <FilterPanel
            filters={filters}
            values={filterValues}
            onFilterChange={(key, value) => onFilterChange?.(key, value)}
            onClearFilters={() => onClearFilters?.()}
            loading={loading}
          />
        )}

        {/* Table Container */}
        <div className="entity-list-template__table-container">
          {loading && items.length === 0 ? (
            <div className="entity-list-template__loading">
              <div className="entity-list-template__spinner"></div>
              <p>Loading {title.toLowerCase()}...</p>
            </div>
          ) : items.length === 0 ? (
            <div className="entity-list-template__empty">
              <p>No {title.toLowerCase()} found.</p>
              {(onCreateNew || createNewRoute) && (
                <Button
                  variant="secondary"
                  onClick={
                    onCreateNew
                      ? onCreateNew
                      : () => createNewRoute && navigate(createNewRoute)
                  }
                >
                  Create the first one
                </Button>
              )}
            </div>
          ) : (
            <EntityTable
              columns={columns}
              items={items}
              loading={loading}
              currentSort={currentSort}
              currentOrder={currentOrder}
              selectedIds={selectedIds}
              onSort={(key) => {
                onSort?.(key);
                onPageChange(1); // Reset to page 1 when sorting
              }}
              onRowClick={onRowClick}
              onRowAction={handleRowAction}
              onSelectRow={onSelectRow}
              onSelectAll={onSelectAll}
              rowActions={[
                { label: 'View', action: 'view', variant: 'primary' },
                { label: 'Edit', action: 'edit', variant: 'secondary' },
                { label: 'Delete', action: 'delete', variant: 'danger' },
              ]}
            />
          )}
        </div>

        {/* Pagination */}
        {items.length > 0 && (
          <div className="entity-list-template__pagination">
            <Pagination
              currentPage={currentPage}
              totalPages={totalPages}
              pageSize={pageSize}
              onPageChange={onPageChange}
              onPageSizeChange={onPageSizeChange}
              pageSizeOptions={[10, 25, 50, 100]}
              disabled={loading}
              showPageSizeSelector
              showPageInfo
            />
          </div>
        )}
      </div>
    );
  }
);

EntityListTemplate.displayName = 'EntityListTemplate';
