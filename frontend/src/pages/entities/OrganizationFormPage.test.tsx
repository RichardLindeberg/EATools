import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { OrganizationFormPage } from './OrganizationFormPage';
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

describe('OrganizationFormPage', () => {
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
          <OrganizationFormPage isEdit={false} />
        </BrowserRouter>
      );

      expect(screen.getByRole('heading', { name: /Create Organization/i })).toBeInTheDocument();
      expect(screen.getByLabelText(/Organization Name/i)).toHaveValue('');
      expect(screen.getByRole('button', { name: /Create Organization/i })).toBeInTheDocument();
    });

    it('creates organization successfully', async () => {
      const user = userEvent.setup();
      mockApiClient.post = vi.fn().mockResolvedValue({
        data: { id: '456', name: 'Test Org' },
      });

      render(
        <BrowserRouter>
          <OrganizationFormPage isEdit={false} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByLabelText(/Organization Name/i)).toBeInTheDocument();
      });

      await user.type(screen.getByLabelText(/Organization Name/i), 'Test Org');
      
      const ownerInput = document.getElementById('owner') as HTMLInputElement;
      if (ownerInput) {
        await user.type(ownerInput, 'user123');
      }

      const submitButton = screen.getByRole('button', { name: /Create Organization/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockApiClient.post).toHaveBeenCalled();
      });
    });
  });

  describe('Edit Mode', () => {
    const mockExistingOrg = {
      id: '123',
      name: 'Existing Org',
      description: 'Test organization',
      owner: 'user123',
      type: 'Department',
      parent_id: null,
    };

    beforeEach(() => {
      mockUseEntity.mockReturnValue({
        data: mockExistingOrg,
        isLoading: false,
        error: null,
        refetch: vi.fn(),
      });
    });

    it('renders edit form with existing data', async () => {
      render(
        <BrowserRouter>
          <OrganizationFormPage isEdit={true} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByRole('heading', { name: /Edit Organization/i })).toBeInTheDocument();
        expect(screen.getByLabelText(/Organization Name/i)).toHaveValue('Existing Org');
      });
    });

    it('updates organization successfully', async () => {
      const user = userEvent.setup();
      mockApiClient.patch = vi.fn().mockResolvedValue({
        data: { id: '123', name: 'Updated Org' },
      });

      render(
        <BrowserRouter>
          <OrganizationFormPage isEdit={true} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByLabelText(/Organization Name/i)).toHaveValue('Existing Org');
      });

      const nameInput = screen.getByLabelText(/Organization Name/i);
      await user.clear(nameInput);
      await user.type(nameInput, 'Updated Org');

      const submitButton = screen.getByRole('button', { name: /Save Changes/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockNavigate).toHaveBeenCalledWith('/entities/organizations/123');
      });
    });
  });
});
