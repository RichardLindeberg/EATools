/**
 * BulkActionBar Component Tests
 */

import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BulkActionBar } from './BulkActionBar';

describe('BulkActionBar', () => {
  it('does not render when selectedCount is 0', () => {
    const { container } = render(
      <BulkActionBar
        selectedCount={0}
        totalCount={10}
        onClearSelection={() => {}}
      />
    );

    expect(container.firstChild).toBeNull();
  });

  it('renders when selectedCount > 0', () => {
    render(
      <BulkActionBar
        selectedCount={3}
        totalCount={10}
        onClearSelection={() => {}}
        visible={true}
      />
    );

    expect(screen.getByText(/3 of 10 selected/)).toBeInTheDocument();
  });

  it('displays correct selection count', () => {
    render(
      <BulkActionBar
        selectedCount={5}
        totalCount={20}
        onClearSelection={() => {}}
        visible={true}
      />
    );

    expect(screen.getByText('5 of 20 selected')).toBeInTheDocument();
  });

  it('renders action buttons when callbacks are provided', () => {
    render(
      <BulkActionBar
        selectedCount={3}
        totalCount={10}
        onDelete={() => {}}
        onArchive={() => {}}
        onExport={() => {}}
        onClearSelection={() => {}}
        visible={true}
      />
    );

    expect(screen.getByRole('button', { name: /Export/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Archive/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Delete/i })).toBeInTheDocument();
  });

  it('does not render action button if callback is not provided', () => {
    render(
      <BulkActionBar
        selectedCount={3}
        totalCount={10}
        onDelete={undefined}
        onArchive={() => {}}
        onExport={() => {}}
        onClearSelection={() => {}}
        visible={true}
      />
    );

    expect(screen.queryByRole('button', { name: /Delete/i })).not.toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Archive/i })).toBeInTheDocument();
  });

  it('calls onDelete when delete button is clicked', async () => {
    const onDelete = vi.fn();
    render(
      <BulkActionBar
        selectedCount={3}
        totalCount={10}
        onDelete={onDelete}
        onClearSelection={() => {}}
        visible={true}
      />
    );

    const deleteButton = screen.getByRole('button', { name: /Delete/i });
    await userEvent.click(deleteButton);

    expect(onDelete).toHaveBeenCalled();
  });

  it('calls onArchive when archive button is clicked', async () => {
    const onArchive = vi.fn();
    render(
      <BulkActionBar
        selectedCount={3}
        totalCount={10}
        onArchive={onArchive}
        onClearSelection={() => {}}
        visible={true}
      />
    );

    const archiveButton = screen.getByRole('button', { name: /Archive/i });
    await userEvent.click(archiveButton);

    expect(onArchive).toHaveBeenCalled();
  });

  it('calls onExport when export button is clicked', async () => {
    const onExport = vi.fn();
    render(
      <BulkActionBar
        selectedCount={3}
        totalCount={10}
        onExport={onExport}
        onClearSelection={() => {}}
        visible={true}
      />
    );

    const exportButton = screen.getByRole('button', { name: /Export/i });
    await userEvent.click(exportButton);

    expect(onExport).toHaveBeenCalled();
  });

  it('calls onClearSelection when clear selection button is clicked', async () => {
    const onClearSelection = vi.fn();
    render(
      <BulkActionBar
        selectedCount={3}
        totalCount={10}
        onClearSelection={onClearSelection}
        visible={true}
      />
    );

    const clearButton = screen.getByRole('button', { name: /Clear Selection/i });
    await userEvent.click(clearButton);

    expect(onClearSelection).toHaveBeenCalled();
  });

  it('disables buttons when loading', () => {
    render(
      <BulkActionBar
        selectedCount={3}
        totalCount={10}
        onDelete={() => {}}
        onArchive={() => {}}
        onExport={() => {}}
        onClearSelection={() => {}}
        loading={true}
        visible={true}
      />
    );

    const buttons = screen.getAllByRole('button');
    buttons.forEach((button) => {
      expect(button).toBeDisabled();
    });
  });

  it('shows selection count in action buttons', () => {
    render(
      <BulkActionBar
        selectedCount={3}
        totalCount={10}
        onDelete={() => {}}
        onArchive={() => {}}
        onExport={() => {}}
        onClearSelection={() => {}}
        visible={true}
      />
    );

    expect(screen.getByRole('button', { name: /Export \(3\)/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Archive \(3\)/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Delete \(3\)/i })).toBeInTheDocument();
  });

  it('respects visible prop', () => {
    const { rerender, container } = render(
      <BulkActionBar
        selectedCount={3}
        totalCount={10}
        onClearSelection={() => {}}
        visible={false}
      />
    );

    expect(container.firstChild).toBeNull();

    rerender(
      <BulkActionBar
        selectedCount={3}
        totalCount={10}
        onClearSelection={() => {}}
        visible={true}
      />
    );

    expect(screen.getByText(/3 of 10 selected/)).toBeInTheDocument();
  });
});
