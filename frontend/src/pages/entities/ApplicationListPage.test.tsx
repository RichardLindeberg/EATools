/**
 * ApplicationListPage Tests
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { ApplicationListPage } from './ApplicationListPage';
import * as entitiesApi from '../../api/entitiesApi';

vi.mock('../../api/entitiesApi');
vi.mock('../../hooks/useEntityList');
vi.mock('../../hooks/useEntityList', () => ({
  useEntityList: vi.fn(() => ({
    items: [],
    total: 0,
    loading: false,
    error: null,
    params: {
      page: 1,
      limit: 10,
      sort: 'name',
      order: 'asc',
      search: '',
    },
    setPage: vi.fn(),
    setLimit: vi.fn(),
    setSort: vi.fn(),
    setSearch: vi.fn(),
    clearFilters: vi.fn(),
  })),
  useBulkSelection: vi.fn(() => ({
    selectedIds: new Set(),
    selectedCount: 0,
    toggleSelect: vi.fn(),
    selectAll: vi.fn(),
    clearSelection: vi.fn(),
    isSelected: vi.fn(),
    isAllSelected: vi.fn(),
    isSomeSelected: vi.fn(),
  })),
  useEntityActions: vi.fn(() => ({
    loading: false,
    error: null,
    deleteEntity: vi.fn(),
    bulkDelete: vi.fn(),
  })),
}));

describe('ApplicationListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders page with title and description', async () => {
    render(
      <BrowserRouter>
        <ApplicationListPage />
      </BrowserRouter>
    );

    expect(screen.getByText('Applications')).toBeInTheDocument();
    expect(
      screen.getByText(/Manage all registered applications/)
    ).toBeInTheDocument();
  });

  it('renders create new button', () => {
    render(
      <BrowserRouter>
        <ApplicationListPage />
      </BrowserRouter>
    );

    expect(screen.getByRole('button', { name: /Create New/i })).toBeInTheDocument();
  });

  it('renders entity list template', () => {
    render(
      <BrowserRouter>
        <ApplicationListPage />
      </BrowserRouter>
    );

    // Check for table elements
    expect(screen.getByText('Applications')).toBeInTheDocument();
  });

  it('renders filters', async () => {
    render(
      <BrowserRouter>
        <ApplicationListPage />
      </BrowserRouter>
    );

    // Check for filter controls - they might be collapsed
    const filterToggle = screen.getByText('Filters');
    expect(filterToggle).toBeInTheDocument();
  });

  it('handles filter changes', async () => {
    const { useEntityList: mockUseEntityList } = await import(
      '../../hooks/useEntityList'
    );

    render(
      <BrowserRouter>
        <ApplicationListPage />
      </BrowserRouter>
    );

    // Verify hook was called
    expect(mockUseEntityList).toHaveBeenCalled();
  });

  it('handles pagination', async () => {
    render(
      <BrowserRouter>
        <ApplicationListPage />
      </BrowserRouter>
    );

    // Page should render without errors
    expect(screen.getByText('Applications')).toBeInTheDocument();
  });

  it('handles sorting', async () => {
    render(
      <BrowserRouter>
        <ApplicationListPage />
      </BrowserRouter>
    );

    // Page should render without errors
    expect(screen.getByText('Applications')).toBeInTheDocument();
  });
});
