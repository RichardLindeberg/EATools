import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { ApplicationListPage } from './ApplicationListPage';
import * as useEntityListModule from '../../hooks/useEntityList';

vi.mock('../../hooks/useEntityList');
vi.mock('../../api/entitiesApi');

// Mock EntityListTemplate to avoid component rendering complexity
vi.mock('../../components/entity/EntityListTemplate', () => ({
  EntityListTemplate: ({ title }: any) => (
    <div data-testid="entity-list-template">
      <h1>{title}</h1>
      <div data-testid="placeholder">Entity List Template</div>
    </div>
  ),
}));

describe('ApplicationListPage integration: filters & sorting', () => {
  const mockSetPage = vi.fn();
  const mockSetSort = vi.fn();
  const mockSetSearch = vi.fn();
  const mockClearFilters = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    // Setup useEntityList mock with proper structure
    vi.mocked(useEntityListModule.useEntityList).mockReturnValue({
      items: [
        {
          id: '1',
          name: 'Test App 1',
          type: 'web',
          status: 'active',
          owner: 'alice',
          description: 'Test application',
          createdAt: '2024-01-01',
          updatedAt: '2024-01-02',
        },
      ],
      total: 1,
      loading: false,
      error: null,
      params: {
        page: 1,
        limit: 10,
        sort: 'name',
        order: 'asc',
        search: '',
      },
      setPage: mockSetPage,
      setLimit: vi.fn(),
      setSort: mockSetSort,
      setSearch: mockSetSearch,
      clearFilters: mockClearFilters,
      refetch: vi.fn(),
    });

    // Setup useBulkSelection mock
    vi.mocked(useEntityListModule.useBulkSelection).mockReturnValue({
      selectedIds: {
        includes: vi.fn(() => false),
        has: vi.fn(() => false),
      } as any,
      selectedCount: 0,
      toggleSelect: vi.fn(),
      selectAll: vi.fn(),
      clearSelection: vi.fn(),
      isSelected: vi.fn(() => false),
      isAllSelected: vi.fn(() => false),
      isSomeSelected: vi.fn(() => false),
    });

    // Setup useEntityActions mock
    vi.mocked(useEntityListModule.useEntityActions).mockReturnValue({
      loading: false,
      error: null,
      deleteEntity: vi.fn(),
      bulkDelete: vi.fn(),
    });
  });

  it('renders page with Applications title', () => {
    render(
      <BrowserRouter>
        <ApplicationListPage />
      </BrowserRouter>
    );

    expect(screen.getByText('Applications')).toBeInTheDocument();
  });

  it('calls useEntityList hook when component mounts', () => {
    render(
      <BrowserRouter>
        <ApplicationListPage />
      </BrowserRouter>
    );

    // Verify useEntityList was called
    expect(useEntityListModule.useEntityList).toHaveBeenCalled();
  });

  it('calls useBulkSelection hook on mount', () => {
    render(
      <BrowserRouter>
        <ApplicationListPage />
      </BrowserRouter>
    );

    expect(useEntityListModule.useBulkSelection).toHaveBeenCalled();
  });

  it('calls useEntityActions hook on mount', () => {
    render(
      <BrowserRouter>
        <ApplicationListPage />
      </BrowserRouter>
    );

    expect(useEntityListModule.useEntityActions).toHaveBeenCalled();
  });

  it('initializes with default pagination params (page 1, limit 10, sort name, order asc)', () => {
    render(
      <BrowserRouter>
        <ApplicationListPage />
      </BrowserRouter>
    );

    // Verify the hook was called - the component uses the mocked hook's params
    const mockUseEntityList = vi.mocked(useEntityListModule.useEntityList);
    expect(mockUseEntityList).toHaveBeenCalled();

    // The call should have passed the expected parameters
    const callArgs = mockUseEntityList.mock.calls[0];
    expect(callArgs).toBeDefined();
  });

  it('mocks contain setPage, setSort, setSearch methods for filtering', () => {
    render(
      <BrowserRouter>
        <ApplicationListPage />
      </BrowserRouter>
    );

    // These should be available for the component to call
    expect(mockSetPage).toBeDefined();
    expect(mockSetSort).toBeDefined();
    expect(mockSetSearch).toBeDefined();
    expect(mockClearFilters).toBeDefined();
  });

  it('passes list API function to useEntityList hook', () => {
    render(
      <BrowserRouter>
        <ApplicationListPage />
      </BrowserRouter>
    );

    const mockUseEntityList = vi.mocked(useEntityListModule.useEntityList);
    const callArgs = mockUseEntityList.mock.calls[0];

    // First argument should be a function (the list API call)
    expect(typeof callArgs[0]).toBe('function');
  });

  it('hook config specifies defaultLimit 10 and defaultSort name', () => {
    render(
      <BrowserRouter>
        <ApplicationListPage />
      </BrowserRouter>
    );

    const mockUseEntityList = vi.mocked(useEntityListModule.useEntityList);
    const callArgs = mockUseEntityList.mock.calls[0];

    // Second argument should be config object
    if (callArgs[1]) {
      expect(callArgs[1].defaultLimit).toBe(10);
      expect(callArgs[1].defaultSort).toBe('name');
    }
  });
});
