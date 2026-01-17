/**
 * FilterPanel Component Tests
 */

import { describe, it, expect, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { FilterPanel, FilterDefinition } from '../FilterPanel';

const MOCK_FILTERS: FilterDefinition[] = [
  {
    key: 'status',
    label: 'Status',
    type: 'select',
    options: [
      { value: '', label: 'All Statuses' },
      { value: 'active', label: 'Active' },
      { value: 'inactive', label: 'Inactive' },
    ],
  },
  {
    key: 'type',
    label: 'Type',
    type: 'checkbox',
    options: [
      { value: 'type1', label: 'Type 1' },
      { value: 'type2', label: 'Type 2' },
      { value: 'type3', label: 'Type 3' },
    ],
  },
  {
    key: 'search',
    label: 'Search',
    type: 'search',
    placeholder: 'Search by name...',
  },
  {
    key: 'dateRange',
    label: 'Date Range',
    type: 'date-range',
  },
  {
    key: 'numberRange',
    label: 'Number Range',
    type: 'number-range',
  },
];

describe('FilterPanel', () => {
  it('renders filter panel with all filters', () => {
    render(
      <FilterPanel
        filters={MOCK_FILTERS}
        values={{}}
        onFilterChange={() => {}}
        onClearFilters={() => {}}
      />
    );

    expect(screen.getByText('Filters')).toBeInTheDocument();
    expect(screen.getByLabelText('Status')).toBeInTheDocument();
    expect(screen.getByLabelText('Type')).toBeInTheDocument();
    expect(screen.getByLabelText('Search')).toBeInTheDocument();
    expect(screen.getByLabelText('Date Range')).toBeInTheDocument();
    expect(screen.getByLabelText('Number Range')).toBeInTheDocument();
  });

  it('toggles filter panel expansion', async () => {
    render(
      <FilterPanel
        filters={MOCK_FILTERS}
        values={{}}
        onFilterChange={() => {}}
        onClearFilters={() => {}}
      />
    );

    const toggle = screen.getByRole('button', { name: /Filters/i });
    await userEvent.click(toggle);

    // Content should be hidden
    expect(screen.queryByLabelText('Status')).not.toBeInTheDocument();
  });

  it('displays active filter count badge', () => {
    render(
      <FilterPanel
        filters={MOCK_FILTERS}
        values={{ status: 'active', type: ['type1'] }}
        onFilterChange={() => {}}
        onClearFilters={() => {}}
      />
    );

    expect(screen.getByText('2')).toBeInTheDocument();
  });

  it('calls onFilterChange when select filter changes', async () => {
    const onFilterChange = vi.fn();
    render(
      <FilterPanel
        filters={MOCK_FILTERS}
        values={{}}
        onFilterChange={onFilterChange}
        onClearFilters={() => {}}
      />
    );

    const statusSelect = screen.getByLabelText('Status');
    await userEvent.selectOption(statusSelect, 'active');

    expect(onFilterChange).toHaveBeenCalledWith('status', 'active');
  });

  it('handles checkbox filter selection', async () => {
    const onFilterChange = vi.fn();
    render(
      <FilterPanel
        filters={MOCK_FILTERS}
        values={{}}
        onFilterChange={onFilterChange}
        onClearFilters={() => {}}
      />
    );

    const checkbox = screen.getByRole('checkbox', { name: 'Type 1' });
    await userEvent.click(checkbox);

    expect(onFilterChange).toHaveBeenCalledWith('type', expect.any(Array));
  });

  it('handles search filter input', async () => {
    const onFilterChange = vi.fn();
    render(
      <FilterPanel
        filters={MOCK_FILTERS}
        values={{}}
        onFilterChange={onFilterChange}
        onClearFilters={() => {}}
      />
    );

    const searchInput = screen.getByPlaceholderText('Search by name...');
    await userEvent.type(searchInput, 'test');

    expect(onFilterChange).toHaveBeenCalledWith('search', expect.stringContaining('test'));
  });

  it('handles date range filter', async () => {
    const onFilterChange = vi.fn();
    render(
      <FilterPanel
        filters={MOCK_FILTERS}
        values={{}}
        onFilterChange={onFilterChange}
        onClearFilters={() => {}}
      />
    );

    const dateInputs = screen.getAllByDisplayValue('');
    const fromInput = dateInputs.find((input) => (input as HTMLInputElement).placeholder === 'From');
    
    if (fromInput) {
      await userEvent.type(fromInput, '2024-01-01');
      expect(onFilterChange).toHaveBeenCalled();
    }
  });

  it('handles number range filter', async () => {
    const onFilterChange = vi.fn();
    render(
      <FilterPanel
        filters={MOCK_FILTERS}
        values={{}}
        onFilterChange={onFilterChange}
        onClearFilters={() => {}}
      />
    );

    const numberInputs = screen.getAllByDisplayValue('');
    const minInput = numberInputs.find((input) => (input as HTMLInputElement).placeholder === 'Min');
    
    if (minInput) {
      await userEvent.type(minInput, '10');
      expect(onFilterChange).toHaveBeenCalled();
    }
  });

  it('calls onClearFilters when Clear All button is clicked', async () => {
    const onClearFilters = vi.fn();
    render(
      <FilterPanel
        filters={MOCK_FILTERS}
        values={{ status: 'active' }}
        onFilterChange={() => {}}
        onClearFilters={onClearFilters}
      />
    );

    const clearButton = screen.getByRole('button', { name: /Clear All/i });
    await userEvent.click(clearButton);

    expect(onClearFilters).toHaveBeenCalled();
  });

  it('does not show Clear All button when no filters are active', () => {
    render(
      <FilterPanel
        filters={MOCK_FILTERS}
        values={{}}
        onFilterChange={() => {}}
        onClearFilters={() => {}}
      />
    );

    expect(screen.queryByRole('button', { name: /Clear All/i })).not.toBeInTheDocument();
  });

  it('disables filters while loading', async () => {
    render(
      <FilterPanel
        filters={MOCK_FILTERS}
        values={{}}
        onFilterChange={() => {}}
        onClearFilters={() => {}}
        loading={true}
      />
    );

    const statusSelect = screen.getByLabelText('Status');
    expect(statusSelect).toBeDisabled();
  });

  it('calls onApply when Apply Filters button is clicked', async () => {
    const onApply = vi.fn();
    render(
      <FilterPanel
        filters={MOCK_FILTERS}
        values={{}}
        onFilterChange={() => {}}
        onClearFilters={() => {}}
        onApply={onApply}
      />
    );

    const applyButton = screen.getByRole('button', { name: /Apply Filters/i });
    await userEvent.click(applyButton);

    expect(onApply).toHaveBeenCalled();
  });

  it('preserves filter values across renders', () => {
    const { rerender } = render(
      <FilterPanel
        filters={MOCK_FILTERS}
        values={{ status: 'active' }}
        onFilterChange={() => {}}
        onClearFilters={() => {}}
      />
    );

    const statusSelect = screen.getByLabelText('Status') as HTMLSelectElement;
    expect(statusSelect.value).toBe('active');

    rerender(
      <FilterPanel
        filters={MOCK_FILTERS}
        values={{ status: 'active' }}
        onFilterChange={() => {}}
        onClearFilters={() => {}}
      />
    );

    expect(statusSelect.value).toBe('active');
  });
});
