import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { IntegrationFormPage } from './IntegrationFormPage';
import * as entityHook from '../../hooks/useEntity';
import * as apiClient from '../../api/client';
import * as commandDispatcher from '../../utils/commandDispatcher';

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

describe('IntegrationFormPage', () => {
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
          <IntegrationFormPage isEdit={false} />
        </BrowserRouter>
      );

      expect(screen.getByRole('heading', { name: /Create Integration/i })).toBeInTheDocument();
      expect(screen.getByLabelText(/Integration Name/i)).toHaveValue('');
      expect(screen.getByRole('button', { name: /Create Integration/i })).toBeInTheDocument();
    });

    it('creates integration successfully', async () => {
      const user = userEvent.setup();
      mockApiClient.post = vi.fn().mockResolvedValue({
        data: { id: '456', name: 'Test Integration' },
      });

      render(
        <BrowserRouter>
          <IntegrationFormPage isEdit={false} />
        </BrowserRouter>
      );

      await user.type(screen.getByLabelText(/Integration Name/i), 'Test Integration');
      await user.type(screen.getByLabelText(/Owner/i), 'user123');

      const submitButton = screen.getByRole('button', { name: /Create Integration/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockApiClient.post).toHaveBeenCalledWith(
          '/integrations',
          expect.objectContaining({ name: 'Test Integration' })
        );
        expect(mockNavigate).toHaveBeenCalledWith('/entities/integrations/456');
      });
    });
  });

  describe('Edit Mode', () => {
    const mockExistingIntegration = {
      id: '123',
      name: 'Existing Integration',
      description: 'Test integration',
      owner: 'user123',
      lifecycle: 'active',
      integrationType: 'API',
      protocol: 'REST',
      frequency: 'Real-time',
    };

    beforeEach(() => {
      mockUseEntity.mockReturnValue({
        data: mockExistingIntegration,
        isLoading: false,
        error: null,
        refetch: vi.fn(),
      });
    });

    it('renders edit form with existing data', async () => {
      render(
        <BrowserRouter>
          <IntegrationFormPage isEdit={true} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByRole('heading', { name: /Edit Integration/i })).toBeInTheDocument();
        expect(screen.getByLabelText(/Integration Name/i)).toHaveValue('Existing Integration');
      });
    });

    it('updates integration successfully', async () => {
      const user = userEvent.setup();
      mockApiClient.patch = vi.fn().mockResolvedValue({
        data: { id: '123', name: 'Updated Integration' },
      });

      render(
        <BrowserRouter>
          <IntegrationFormPage isEdit={true} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByLabelText(/Integration Name/i)).toHaveValue('Existing Integration');
      });

      const nameInput = screen.getByLabelText(/Integration Name/i);
      await user.clear(nameInput);
      await user.type(nameInput, 'Updated Integration');

      const submitButton = screen.getByRole('button', { name: /Save Changes/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockNavigate).toHaveBeenCalledWith('/entities/integrations/123');
      });
    });
  });
});
