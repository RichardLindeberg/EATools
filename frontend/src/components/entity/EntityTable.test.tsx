/**
 * EntityTable Component Tests
 */

import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { EntityTable, ColumnConfig } from '../EntityTable';

interface TestItem {
  id: string;
  name: string;
  status: string;
  createdAt: string;
}

const COLUMNS: ColumnConfig<TestItem>[] = [
  { key: 'id', label: 'ID' },
  { key: 'name', label: 'Name', sortable: true },
  { key: 'status', label: 'Status', width: '100px' },
  {
    key: 'createdAt',
    label: 'Created',
    format: (value) => new Date(value as string).toLocaleDateString(),
  },
];

const MOCK_ITEMS: TestItem[] = [
  { id: '1', name: 'Item 1', status: 'active', createdAt: '2024-01-01' },
  { id: '2', name: 'Item 2', status: 'inactive', createdAt: '2024-01-02' },
  { id: '3', name: 'Item 3', status: 'active', createdAt: '2024-01-03' },
];

describe('EntityTable', () => {
  it('renders table with columns', () => {
    render(
      <EntityTable columns={COLUMNS} items={MOCK_ITEMS} loading={false} />
    );

    expect(screen.getByText('ID')).toBeInTheDocument();
    expect(screen.getByText('Name')).toBeInTheDocument();
    expect(screen.getByText('Status')).toBeInTheDocument();
    expect(screen.getByText('Created')).toBeInTheDocument();
  });

  it('renders all items in table', () => {
    render(
      <EntityTable columns={COLUMNS} items={MOCK_ITEMS} loading={false} />
    );

    MOCK_ITEMS.forEach((item) => {
      expect(screen.getByText(item.name)).toBeInTheDocument();
      expect(screen.getByText(item.status)).toBeInTheDocument();
    });
  });

  it('displays loading state', () => {
    render(
      <EntityTable columns={COLUMNS} items={[]} loading={true} />
    );

    expect(screen.getByText('Loading...')).toBeInTheDocument();
  });

  it('displays empty state when no items', () => {
    render(
      <EntityTable columns={COLUMNS} items={[]} loading={false} />
    );

    expect(screen.getByText('No items to display')).toBeInTheDocument();
  });

  it('calls onSort when column header is clicked', async () => {
    const onSort = vi.fn();
    render(
      <EntityTable
        columns={COLUMNS}
        items={MOCK_ITEMS}
        loading={false}
        onSort={onSort}
      />
    );

    const nameHeader = screen.getByRole('button', { name: /Name/i });
    await userEvent.click(nameHeader);

    expect(onSort).toHaveBeenCalledWith('name');
  });

  it('shows sort indicator on current sort column', () => {
    render(
      <EntityTable
        columns={COLUMNS}
        items={MOCK_ITEMS}
        loading={false}
        currentSort="name"
        currentOrder="asc"
      />
    );

    const nameHeader = screen.getByRole('button', { name: /Name/i });
    expect(nameHeader).toHaveClass('entity-table__sort-btn--active');
  });

  it('handles row selection with checkboxes', async () => {
    const onSelectRow = vi.fn();
    render(
      <EntityTable
        columns={COLUMNS}
        items={MOCK_ITEMS}
        loading={false}
        onSelectRow={onSelectRow}
        selectedIds={new Set()}
      />
    );

    const checkboxes = screen.getAllByRole('checkbox');
    await userEvent.click(checkboxes[1]); // First item

    expect(onSelectRow).toHaveBeenCalledWith('1');
  });

  it('shows selected row background', () => {
    const { container } = render(
      <EntityTable
        columns={COLUMNS}
        items={MOCK_ITEMS}
        loading={false}
        selectedIds={new Set(['1'])}
      />
    );

    const rows = container.querySelectorAll('.entity-table__row');
    expect(rows[0]).toHaveClass('entity-table__row--selected');
  });

  it('calls onSelectAll when select all checkbox is clicked', async () => {
    const onSelectAll = vi.fn();
    render(
      <EntityTable
        columns={COLUMNS}
        items={MOCK_ITEMS}
        loading={false}
        onSelectAll={onSelectAll}
        selectedIds={new Set()}
      />
    );

    const checkboxes = screen.getAllByRole('checkbox');
    await userEvent.click(checkboxes[0]); // Select all

    expect(onSelectAll).toHaveBeenCalled();
  });

  it('calls onRowClick when row is clicked', async () => {
    const onRowClick = vi.fn();
    render(
      <EntityTable
        columns={COLUMNS}
        items={MOCK_ITEMS}
        loading={false}
        onRowClick={onRowClick}
      />
    );

    const firstItem = screen.getByText('Item 1');
    await userEvent.click(firstItem);

    expect(onRowClick).toHaveBeenCalledWith(MOCK_ITEMS[0]);
  });

  it('calls onRowAction for row action buttons', async () => {
    const onRowAction = vi.fn();
    render(
      <EntityTable
        columns={COLUMNS}
        items={MOCK_ITEMS}
        loading={false}
        onRowAction={onRowAction}
        rowActions={[
          { label: 'View', action: 'view', variant: 'primary' },
          { label: 'Edit', action: 'edit', variant: 'secondary' },
          { label: 'Delete', action: 'delete', variant: 'danger' },
        ]}
      />
    );

    const viewButtons = screen.getAllByRole('button', { name: /View/i });
    await userEvent.click(viewButtons[0]);

    expect(onRowAction).toHaveBeenCalledWith('view', MOCK_ITEMS[0]);
  });

  it('applies column width styles', () => {
    const { container } = render(
      <EntityTable columns={COLUMNS} items={MOCK_ITEMS} loading={false} />
    );

    const statusCell = container.querySelector('[style*="100px"]');
    expect(statusCell).toBeTruthy();
  });

  it('formats cell values using column format function', () => {
    render(
      <EntityTable columns={COLUMNS} items={MOCK_ITEMS} loading={false} />
    );

    // Date should be formatted as "1/1/2024" or similar
    expect(screen.getByText(/1\/1\/2024|01\/01\/2024/)).toBeInTheDocument();
  });

  it('disables row interactions while loading', () => {
    const onRowClick = vi.fn();
    const { container } = render(
      <EntityTable
        columns={COLUMNS}
        items={MOCK_ITEMS}
        loading={true}
        onRowClick={onRowClick}
      />
    );

    const rows = container.querySelectorAll('.entity-table__row');
    rows.forEach((row) => {
      expect(row).toHaveClass('entity-table__row--disabled');
    });
  });
});
