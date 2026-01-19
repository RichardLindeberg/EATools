import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { ServerDetailPage } from './ServerDetailPage';
import * as detailHook from '../../hooks/useEntityDetail';

vi.mock('../../hooks/useEntityDetail');
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => vi.fn(),
    useParams: () => ({ id: 'server-001' }),
  };
});

describe('ServerDetailPage', () => {
  const mockUseEntityDetail = vi.mocked(detailHook.useEntityDetail);
  const mockUseEntityRelationships = vi.mocked(detailHook.useEntityRelationships);

  beforeEach(() => {
    vi.clearAllMocks();
    mockUseEntityDetail.mockReturnValue({
      entity: { id: 'server-001', name: 'Production Server' },
      loading: false,
      error: null,
      isNotFound: false,
      isForbidden: false,
    });
    mockUseEntityRelationships.mockReturnValue({
      relationships: [],
      loading: false,
    });
  });

  it('renders detail page title', () => {
    render(
      <BrowserRouter>
        <ServerDetailPage />
      </BrowserRouter>
    );
    expect(screen.getByText('Server Details')).toBeInTheDocument();
  });

  it('renders Overview tab', () => {
    render(
      <BrowserRouter>
        <ServerDetailPage />
      </BrowserRouter>
    );
    expect(screen.getByText('Overview')).toBeInTheDocument();
  });

  it('renders edit button', () => {
    render(
      <BrowserRouter>
        <ServerDetailPage />
      </BrowserRouter>
    );
    expect(screen.getByRole('button', { name: /Edit/i })).toBeInTheDocument();
  });

  it('shows not found error when entity missing', () => {
    mockUseEntityDetail.mockReturnValue({
      entity: null,
      loading: false,
      error: null,
      isNotFound: true,
      isForbidden: false,
    });
    render(
      <BrowserRouter>
        <ServerDetailPage />
      </BrowserRouter>
    );
    expect(screen.getByText(/not found/i)).toBeInTheDocument();
  });

  it('shows forbidden error when access denied', () => {
    mockUseEntityDetail.mockReturnValue({
      entity: null,
      loading: false,
      error: null,
      isNotFound: false,
      isForbidden: true,
    });
    render(
      <BrowserRouter>
        <ServerDetailPage />
      </BrowserRouter>
    );
    expect(screen.getByText(/forbidden|permission/i)).toBeInTheDocument();
  });
});
