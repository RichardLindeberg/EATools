import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { ApplicationServiceFormPage } from './ApplicationServiceFormPage';
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

describe('ApplicationServiceFormPage', () => {
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
          <ApplicationServiceFormPage isEdit={false} />
        </BrowserRouter>
      );

      expect(screen.getByRole('heading', { name: /Create Application Service/i })).toBeInTheDocument();
      expect(screen.getByLabelText(/Service Name/i)).toHaveValue('');
      expect(
        screen.getByRole('button', { name: /Create Application Service/i })
      ).toBeInTheDocument();
    });

    it('creates application service successfully', async () => {
      const user = userEvent.setup();
      mockApiClient.post = vi.fn().mockResolvedValue({
        data: { id: '456', name: 'Test Service' },
      });

      render(
        <BrowserRouter>
          <ApplicationServiceFormPage isEdit={false} />
        </BrowserRouter>
      );

      await user.type(screen.getByLabelText(/Service Name/i), 'Test Service');
      await user.type(screen.getByLabelText(/Owner/i), 'user123');

      const submitButton = screen.getByRole('button', { name: /Create Application Service/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockApiClient.post).toHaveBeenCalledWith(
          '/application-services',
          expect.objectContaining({ name: 'Test Service' })
        );
        expect(mockNavigate).toHaveBeenCalledWith('/entities/application-services/456');
      });
    });
  });

  describe('Edit Mode', () => {
    const mockExistingService = {
      id: '123',
      name: 'Existing Service',
      description: 'Test service',
      owner: 'user123',
      serviceType: 'REST API',
      protocol: 'HTTPS',
      version: '1.0.0',
    };

    beforeEach(() => {
      mockUseEntity.mockReturnValue({
        data: mockExistingService,
        isLoading: false,
        error: null,
        refetch: vi.fn(),
      });
    });

    it('renders edit form with existing data', async () => {
      render(
        <BrowserRouter>
          <ApplicationServiceFormPage isEdit={true} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByRole('heading', { name: /Edit Application Service/i })).toBeInTheDocument();
        expect(screen.getByLabelText(/Service Name/i)).toHaveValue('Existing Service');
      });
    });

    it('updates application service successfully', async () => {
      const user = userEvent.setup();
      mockApiClient.patch = vi.fn().mockResolvedValue({
        data: { id: '123', name: 'Updated Service' },
      });

      render(
        <BrowserRouter>
          <ApplicationServiceFormPage isEdit={true} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByLabelText(/Service Name/i)).toHaveValue('Existing Service');
      });

      const nameInput = screen.getByLabelText(/Service Name/i);
      await user.clear(nameInput);
      await user.type(nameInput, 'Updated Service');

      const submitButton = screen.getByRole('button', { name: /Save Changes/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockNavigate).toHaveBeenCalledWith('/entities/application-services/123');
      });
    });
  });
});
