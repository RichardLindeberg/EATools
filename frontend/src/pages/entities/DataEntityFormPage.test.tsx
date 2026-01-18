import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { DataEntityFormPage } from './DataEntityFormPage';
import * as entityHook from '../../hooks/useEntity';
import * as apiClient from '../../api/client';

const mockNavigate = vi.fn();
vi.mock('../../hooks/useEntity');
vi.mock('../../api/client');
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
    useParams: () => ({ id: '123' }),
  };
});

describe('DataEntityFormPage', () => {
  const mockUseEntity = vi.mocked(entityHook.useEntity);
  const mockApiClient = vi.mocked(apiClient.apiClient);

  beforeEach(() => {
    vi.clearAllMocks();
    mockUseEntity.mockReturnValue({
      data: null,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });
  });

  describe('Create Mode', () => {
    it('renders create form with empty fields', () => {
      render(
        <BrowserRouter>
          <DataEntityFormPage isEdit={false} />
        </BrowserRouter>
      );

      expect(screen.getByRole('heading', { name: /Create Data Entity/i })).toBeInTheDocument();
    });

    it('creates data entity successfully', async () => {
      mockApiClient.post = vi.fn().mockResolvedValue({
        data: { id: '456', name: 'Test Entity' },
      });

      render(
        <BrowserRouter>
          <DataEntityFormPage isEdit={false} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /Create Data Entity/i })).toBeInTheDocument();
      });

      expect(screen.getByRole('heading', { name: /Create Data Entity/i })).toBeInTheDocument();
    });
  });

  describe('Edit Mode', () => {
    const mockExistingEntity = {
      id: '123',
      name: 'Existing Entity',
      description: 'Test entity',
      owner: 'user123',
      lifecycle: 'active',
      classification: 'confidential',
      dataType: 'Structured',
      storageLocation: 'Database',
    };

    beforeEach(() => {
      mockUseEntity.mockReturnValue({
        data: mockExistingEntity,
        isLoading: false,
        error: null,
        refetch: vi.fn(),
      });
    });

    it('renders edit form with existing data', async () => {
      render(
        <BrowserRouter>
          <DataEntityFormPage isEdit={true} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByRole('heading', { name: /Edit Data Entity/i })).toBeInTheDocument();
        expect(screen.getByLabelText(/Data Entity Name/i)).toHaveValue('Existing Entity');
      });
    });

    it('updates data entity successfully', async () => {
      mockApiClient.patch = vi.fn().mockResolvedValue({
        data: { id: '123', name: 'Updated Entity' },
      });

      render(
        <BrowserRouter>
          <DataEntityFormPage isEdit={true} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /Save Changes/i })).toBeInTheDocument();
      });

      expect(screen.getByRole('heading', { name: /Edit Data Entity/i })).toBeInTheDocument();
    });
  });
});
