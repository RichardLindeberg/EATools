import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { RelationFormPage } from './RelationFormPage';
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

describe('RelationFormPage', () => {
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
          <RelationFormPage isEdit={false} />
        </BrowserRouter>
      );

      expect(screen.getByRole('heading', { name: /Create Relation/i })).toBeInTheDocument();
      expect(screen.getByLabelText(/Relation Name/i)).toHaveValue('');
      expect(screen.getByRole('button', { name: /Create Relation/i })).toBeInTheDocument();
    });

    it('creates relation successfully', async () => {
      const user = userEvent.setup();
      mockApiClient.post = vi.fn().mockResolvedValue({
        data: { id: '456', name: 'Test Relation' },
      });

      render(
        <BrowserRouter>
          <RelationFormPage isEdit={false} />
        </BrowserRouter>
      );

      await user.type(screen.getByLabelText(/Relation Name/i), 'Test Relation');
      await user.type(screen.getByLabelText(/Owner/i), 'user123');

      const submitButton = screen.getByRole('button', { name: /Create Relation/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockApiClient.post).toHaveBeenCalledWith(
          '/relations',
          expect.objectContaining({ name: 'Test Relation' })
        );
        expect(mockNavigate).toHaveBeenCalledWith('/entities/relations/456');
      });
    });
  });

  describe('Edit Mode', () => {
    const mockExistingRelation = {
      id: '123',
      name: 'Existing Relation',
      description: 'Test relation',
      owner: 'user123',
      sourceId: 'app-1',
      targetId: 'app-2',
      relationType: 'depends-on',
    };

    beforeEach(() => {
      mockUseEntity.mockReturnValue({
        data: mockExistingRelation,
        isLoading: false,
        error: null,
        refetch: vi.fn(),
      });
    });

    it('renders edit form with existing data', async () => {
      render(
        <BrowserRouter>
          <RelationFormPage isEdit={true} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByRole('heading', { name: /Edit Relation/i })).toBeInTheDocument();
        expect(screen.getByLabelText(/Relation Name/i)).toHaveValue('Existing Relation');
      });
    });

    it('updates relation successfully', async () => {
      const user = userEvent.setup();
      mockApiClient.patch = vi.fn().mockResolvedValue({
        data: { id: '123', name: 'Updated Relation' },
      });

      render(
        <BrowserRouter>
          <RelationFormPage isEdit={true} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByLabelText(/Relation Name/i)).toHaveValue('Existing Relation');
      });

      const nameInput = screen.getByLabelText(/Relation Name/i);
      await user.clear(nameInput);
      await user.type(nameInput, 'Updated Relation');

      const submitButton = screen.getByRole('button', { name: /Save Changes/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockNavigate).toHaveBeenCalledWith('/entities/relations/123');
      });
    });
  });
});
