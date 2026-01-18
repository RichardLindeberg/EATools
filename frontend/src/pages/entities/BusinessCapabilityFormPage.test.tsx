import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { BusinessCapabilityFormPage } from './BusinessCapabilityFormPage';
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

describe('BusinessCapabilityFormPage', () => {
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
          <BusinessCapabilityFormPage isEdit={false} />
        </BrowserRouter>
      );

      expect(screen.getByRole('heading', { name: /Create Business Capability/i })).toBeInTheDocument();
      expect(screen.getByLabelText(/Capability Name/i)).toHaveValue('');
      expect(
        screen.getByRole('button', { name: /Create Business Capability/i })
      ).toBeInTheDocument();
    });

    it('creates business capability successfully', async () => {
      const user = userEvent.setup();
      mockApiClient.post = vi.fn().mockResolvedValue({
        data: { id: '456', name: 'Test Capability' },
      });

      render(
        <BrowserRouter>
          <BusinessCapabilityFormPage isEdit={false} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByLabelText(/Capability Name/i)).toBeInTheDocument();
      });

      await user.type(screen.getByLabelText(/Capability Name/i), 'Test Capability');
      
      const ownerInput = document.getElementById('owner') as HTMLInputElement;
      if (ownerInput) {
        await user.type(ownerInput, 'user123');
      }

      const submitButton = screen.getByRole('button', { name: /Create Business Capability/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockApiClient.post).toHaveBeenCalled();
      });
    });
  });

  describe('Edit Mode', () => {
    const mockExistingCapability = {
      id: '123',
      name: 'Existing Capability',
      description: 'Test capability',
      owner: 'user123',
      lifecycle: 'active',
      level: 2,
      parent_id: null,
    };

    beforeEach(() => {
      mockUseEntity.mockReturnValue({
        data: mockExistingCapability,
        isLoading: false,
        error: null,
        refetch: vi.fn(),
      });
    });

    it('renders edit form with existing data', async () => {
      render(
        <BrowserRouter>
          <BusinessCapabilityFormPage isEdit={true} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByRole('heading', { name: /Edit Business Capability/i })).toBeInTheDocument();
        expect(screen.getByLabelText(/Capability Name/i)).toHaveValue('Existing Capability');
      });
    });

    it('updates business capability successfully', async () => {
      const user = userEvent.setup();
      mockApiClient.patch = vi.fn().mockResolvedValue({
        data: { id: '123', name: 'Updated Capability' },
      });

      render(
        <BrowserRouter>
          <BusinessCapabilityFormPage isEdit={true} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByLabelText(/Capability Name/i)).toHaveValue('Existing Capability');
      });

      const nameInput = screen.getByLabelText(/Capability Name/i);
      await user.clear(nameInput);
      await user.type(nameInput, 'Updated Capability');

      const submitButton = screen.getByRole('button', { name: /Save Changes/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockNavigate).toHaveBeenCalledWith('/entities/business-capabilities/123');
      });
    });
  });
});
