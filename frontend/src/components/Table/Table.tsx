import React, { ReactNode } from 'react'
import './Table.css'

export interface TableColumn<T = any> {
  key: string
  header: string | ReactNode
  accessor?: (row: T) => ReactNode
  sortable?: boolean
  width?: string
  align?: 'left' | 'center' | 'right'
}

export interface TableProps<T = any> {
  /**
   * Table columns
   */
  columns: TableColumn<T>[]

  /**
   * Table data rows
   */
  data: T[]

  /**
   * Sort column key
   */
  sortColumn?: string

  /**
   * Sort direction
   */
  sortDirection?: 'asc' | 'desc'

  /**
   * Sort change handler
   */
  onSort?: (column: string) => void

  /**
   * Row click handler
   */
  onRowClick?: (row: T, index: number) => void

  /**
   * Empty state message
   */
  emptyMessage?: string

  /**
   * Loading state
   */
  loading?: boolean

  /**
   * Show striped rows
   */
  striped?: boolean

  /**
   * Show hover effect
   */
  hoverable?: boolean

  /**
   * Additional CSS class
   */
  className?: string
}

/**
 * Table Component
 *
 * Data table with sorting, custom rendering, and row interactions.
 * Accessible with proper ARIA attributes.
 *
 * @example
 * <Table 
 *   columns={[
 *     { key: 'name', header: 'Name', sortable: true },
 *     { key: 'status', header: 'Status', accessor: (row) => <Badge>{row.status}</Badge> }
 *   ]}
 *   data={servers}
 *   onRowClick={(row) => navigate(`/servers/${row.id}`)}
 * />
 */
export function Table<T extends Record<string, any>>({
  columns,
  data,
  sortColumn,
  sortDirection = 'asc',
  onSort,
  onRowClick,
  emptyMessage = 'No data available',
  loading = false,
  striped = true,
  hoverable = true,
  className,
}: TableProps<T>) {
  const tableClasses = [
    'table',
    striped && 'table--striped',
    hoverable && 'table--hoverable',
    loading && 'table--loading',
    className,
  ]
    .filter(Boolean)
    .join(' ')

  const handleSort = (column: TableColumn<T>) => {
    if (column.sortable && onSort) {
      onSort(column.key)
    }
  }

  const renderCellContent = (column: TableColumn<T>, row: T) => {
    if (column.accessor) {
      return column.accessor(row)
    }
    return row[column.key]
  }

  const getSortIcon = (column: TableColumn<T>) => {
    if (!column.sortable) return null

    if (sortColumn === column.key) {
      return sortDirection === 'asc' ? ' ↑' : ' ↓'
    }
    return ' ↕'
  }

  if (loading) {
    return (
      <div className={tableClasses}>
        <div className="table__loading">Loading...</div>
      </div>
    )
  }

  if (data.length === 0) {
    return (
      <div className={tableClasses}>
        <div className="table__empty">{emptyMessage}</div>
      </div>
    )
  }

  return (
    <div className="table-container">
      <table className={tableClasses}>
        <thead className="table__head">
          <tr className="table__row">
            {columns.map((column) => (
              <th
                key={column.key}
                className={`table__header ${
                  column.sortable ? 'table__header--sortable' : ''
                } table__header--${column.align || 'left'}`}
                style={{ width: column.width }}
                onClick={() => handleSort(column)}
                role={column.sortable ? 'button' : undefined}
                tabIndex={column.sortable ? 0 : undefined}
                aria-sort={
                  sortColumn === column.key
                    ? sortDirection === 'asc'
                      ? 'ascending'
                      : 'descending'
                    : undefined
                }
              >
                {column.header}
                {getSortIcon(column)}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="table__body">
          {data.map((row, rowIndex) => (
            <tr
              key={rowIndex}
              className={`table__row ${
                onRowClick ? 'table__row--clickable' : ''
              }`}
              onClick={() => onRowClick?.(row, rowIndex)}
            >
              {columns.map((column) => (
                <td
                  key={column.key}
                  className={`table__cell table__cell--${column.align || 'left'}`}
                >
                  {renderCellContent(column, row)}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

export default Table
