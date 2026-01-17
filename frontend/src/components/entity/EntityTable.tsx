/**
 * EntityTable Component
 * Reusable table for displaying entity lists with sorting and actions
 */

import React from 'react';
import { Table } from '../Table/Table';
import { Button } from '../Button/Button';
import './EntityTable.css';

export interface ColumnConfig<T> {
  key: string;
  label: string;
  sortable?: boolean;
  format?: (value: unknown, item: T) => React.ReactNode;
  width?: string;
}

export interface EntityTableProps<T> {
  columns: ColumnConfig<T>[];
  items: T[];
  loading?: boolean;
  onSort?: (column: string, order: 'asc' | 'desc') => void;
  onRowClick?: (item: T) => void;
  rowActions?: (item: T) => React.ReactNode;
  selectedIds?: string[];
  onSelectRow?: (id: string) => void;
  onSelectAll?: (selected: boolean, allIds: string[]) => void;
  currentSort?: string;
  currentOrder?: 'asc' | 'desc';
}

export const EntityTable = React.forwardRef<HTMLDivElement, EntityTableProps<any>>(
  (
    {
      columns,
      items,
      loading = false,
      onSort,
      onRowClick,
      rowActions,
      selectedIds = [],
      onSelectRow,
      onSelectAll,
      currentSort,
      currentOrder,
    },
    ref
  ) => {
    const allIds = items.map((item: any) => item.id);
    const isAllSelected = allIds.length > 0 && allIds.every(id => selectedIds.includes(id));
    const isSomeSelected = allIds.some(id => selectedIds.includes(id));

    const handleSelectAll = (e: React.ChangeEvent<HTMLInputElement>) => {
      if (onSelectAll) {
        onSelectAll(e.target.checked, allIds);
      }
    };

    const renderHeader = () => {
      return (
        <thead>
          <tr>
            {onSelectRow && (
              <th className="entity-table__checkbox-col">
                <input
                  type="checkbox"
                  checked={isAllSelected}
                  indeterminate={isSomeSelected && !isAllSelected}
                  onChange={handleSelectAll}
                  aria-label="Select all rows"
                />
              </th>
            )}
            {columns.map(column => (
              <th key={column.key} style={{ width: column.width }} className="entity-table__header">
                <div className="entity-table__header-content">
                  <span>{column.label}</span>
                  {column.sortable && (
                    <button
                      className={`entity-table__sort-btn ${
                        currentSort === column.key ? `entity-table__sort-btn--${currentOrder}` : ''
                      }`}
                      onClick={() => {
                        const newOrder =
                          currentSort === column.key && currentOrder === 'asc' ? 'desc' : 'asc';
                        if (onSort) {
                          onSort(column.key, newOrder);
                        }
                      }}
                      aria-label={`Sort by ${column.label}`}
                    >
                      ↕
                    </button>
                  )}
                </div>
              </th>
            ))}
            {rowActions && <th className="entity-table__actions-col">Actions</th>}
          </tr>
        </thead>
      );
    };

    const renderBody = () => {
      if (loading) {
        return (
          <tbody>
            <tr>
              <td colSpan={columns.length + (onSelectRow ? 1 : 0) + (rowActions ? 1 : 0)}>
                <div className="entity-table__loading">Loading...</div>
              </td>
            </tr>
          </tbody>
        );
      }

      if (items.length === 0) {
        return (
          <tbody>
            <tr>
              <td colSpan={columns.length + (onSelectRow ? 1 : 0) + (rowActions ? 1 : 0)}>
                <div className="entity-table__empty">No items to display</div>
              </td>
            </tr>
          </tbody>
        );
      }

      return (
        <tbody>
          {items.map((item: any) => (
            <tr
              key={item.id}
              className={`entity-table__row ${selectedIds.includes(item.id) ? 'entity-table__row--selected' : ''}`}
              onClick={() => onRowClick && onRowClick(item)}
            >
              {onSelectRow && (
                <td className="entity-table__checkbox-col">
                  <input
                    type="checkbox"
                    checked={selectedIds.includes(item.id)}
                    onChange={e => {
                      e.stopPropagation();
                      if (onSelectRow) {
                        onSelectRow(item.id);
                      }
                    }}
                    onClick={e => e.stopPropagation()}
                    aria-label={`Select ${item.name || item.id}`}
                  />
                </td>
              )}
              {columns.map(column => (
                <td key={column.key} className="entity-table__cell">
                  {column.format ? column.format(item[column.key], item) : String(item[column.key] || '-')}
                </td>
              ))}
              {rowActions && (
                <td className="entity-table__actions-col" onClick={e => e.stopPropagation()}>
                  {rowActions(item)}
                </td>
              )}
            </tr>
          ))}
        </tbody>
      );
    };

    return (
      <div ref={ref} className="entity-table">
        <div className="entity-table__wrapper">
          <table className="entity-table__table">
            {renderHeader()}
            {renderBody()}
          </table>
        </div>
      </div>
    );
  }
);

EntityTable.displayName = 'EntityTable';
