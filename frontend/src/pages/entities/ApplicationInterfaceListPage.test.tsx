import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { ApplicationInterfaceListPage } from './ApplicationInterfaceListPage';
import * as useEntityListModule from '../../hooks/useEntityList';

vi.mock('../../hooks/useEntityList');
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => vi.fn(),
  };
});

// Mock EntityListTemplate to simplify tests
vi.mock('../../components/entity/EntityListTemplate', () => {
  return {
    EntityListTemplate: ({ title }: any) => (
      <div data-testid="entity-list-template">
        <h1>{title}</h1>
        <div data-testid="placeholder">Entity List Template</div>
      </div>
    ),
  };
});

describe('ApplicationInterfaceListPage', () => {
  const mockSetPage = vi.fn();
  const mockSetSort = vi.fn();
  const mockSetSearch = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useEntityListModule.useEntityList).mockReturnValue({
      items: [{ id: '1', name: 'Test ApplicationInterface' }],
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
      setSort: mockSetSort,
      setSearch: mockSetSearch,
      setLimit: vi.fn(),
      clearFilters: vi.fn(),
      refetch: vi.fn(),
    });
    
    // Mock useBulkSelection
    vi.mocked(useEntityListModule.useBulkSelection).mockReturnValue({
      selectedIds: new Set(),
      toggleSelect: vi.fn(),
      selectAll: vi.fn(),
      clearSelection: vi.fn(),
      isAllSelected: false,
    });
    
    // Mock useEntityActions
    vi.mocked(useEntityListModule.useEntityActions).mockReturnValue({
      deleteEntity: vi.fn(),
      bulkDelete: vi.fn(),
      loading: false,
    });
  });

  it('renders list page', () => {
    render(
      <BrowserRouter>
        <ApplicationInterfaceListPage />
      </BrowserRouter>
    );
    expect(screen.getByTestId('entity-list-template')).toBeInTheDocument();
  });

  it('displays entity list template', () => {
    render(
      <BrowserRouter>
        <ApplicationInterfaceListPage />
      </BrowserRouter>
    );
    expect(screen.getByText(/Entity List Template/i)).toBeInTheDocument();
  });

  it('displays loading state', () => {
    vi.mocked(useEntityListModule.useEntityList).mockReturnValue({
      items: [],
      total: 0,
      loading: true,
      error: null,
      params: {
        page: 1,
        limit: 10,
        sort: 'name',
        order: 'asc',
        search: '',
      },
      setPage: mockSetPage,
      setSort: mockSetSort,
      setSearch: mockSetSearch,
      setLimit: vi.fn(),
      clearFilters: vi.fn(),
      refetch: vi.fn(),
    });
    render(
      <BrowserRouter>
        <ApplicationInterfaceListPage />
      </BrowserRouter>
    );
    expect(screen.getByTestId('entity-list-template')).toBeInTheDocument();
  });

  it('displays error state', () => {
    vi.mocked(useEntityListModule.useEntityList).mockReturnValue({
      items: [],
      total: 0,
      loading: false,
      error: { message: 'Failed to fetch' },
      params: {
        page: 1,
        limit: 10,
        sort: 'name',
        order: 'asc',
        search: '',
      },
      setPage: mockSetPage,
      setSort: mockSetSort,
      setSearch: mockSetSearch,
      setLimit: vi.fn(),
      clearFilters: vi.fn(),
      refetch: vi.fn(),
    });
    render(
      <BrowserRouter>
        <ApplicationInterfaceListPage />
      </BrowserRouter>
    );
    expect(screen.getByTestId('entity-list-template')).toBeInTheDocument();
  });

  it('renders with empty items', () => {
    vi.mocked(useEntityListModule.useEntityList).mockReturnValue({
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
      setPage: mockSetPage,
      setSort: mockSetSort,
      setSearch: mockSetSearch,
      setLimit: vi.fn(),
      clearFilters: vi.fn(),
      refetch: vi.fn(),
    });
    render(
      <BrowserRouter>
        <ApplicationInterfaceListPage />
      </BrowserRouter>
    );
    expect(screen.getByTestId('entity-list-template')).toBeInTheDocument();
  });
});
