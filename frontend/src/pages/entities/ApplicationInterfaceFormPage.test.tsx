import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { ApplicationInterfaceFormPage } from './ApplicationInterfaceFormPage';
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

describe('ApplicationInterfaceFormPage', () => {
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
          <ApplicationInterfaceFormPage isEdit={false} />
        </BrowserRouter>
      );

      expect(screen.getByRole('heading', { name: /Create Application Interface/i })).toBeInTheDocument();
      expect(screen.getByLabelText(/Interface Name/i)).toHaveValue('');
      expect(
        screen.getByRole('button', { name: /Create Application Interface/i })
      ).toBeInTheDocument();
    });

    it('creates application interface successfully', async () => {
      const user = userEvent.setup();
      mockApiClient.post = vi.fn().mockResolvedValue({
        data: { id: '456', name: 'Test Interface' },
      });

      render(
        <BrowserRouter>
          <ApplicationInterfaceFormPage isEdit={false} />
        </BrowserRouter>
      );

      await user.type(screen.getByLabelText(/Interface Name/i), 'Test Interface');
      await user.type(screen.getByLabelText(/Owner/i), 'user123');

      const submitButton = screen.getByRole('button', { name: /Create Application Interface/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockApiClient.post).toHaveBeenCalledWith(
          '/application-interfaces',
          expect.objectContaining({ name: 'Test Interface' })
        );
        expect(mockNavigate).toHaveBeenCalledWith('/entities/application-interfaces/456');
      });
    });
  });

  describe('Edit Mode', () => {
    const mockExistingInterface = {
      id: '123',
      name: 'Existing Interface',
      description: 'Test interface',
      owner: 'user123',
      interfaceType: 'REST',
      protocol: 'HTTP',
      endpoint: '/api/v1/users',
    };

    beforeEach(() => {
      mockUseEntity.mockReturnValue({
        data: mockExistingInterface,
        isLoading: false,
        error: null,
        refetch: vi.fn(),
      });
    });

    it('renders edit form with existing data', async () => {
      render(
        <BrowserRouter>
          <ApplicationInterfaceFormPage isEdit={true} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByRole('heading', { name: /Edit Application Interface/i })).toBeInTheDocument();
        expect(screen.getByLabelText(/Interface Name/i)).toHaveValue('Existing Interface');
      });
    });

    it('updates application interface successfully', async () => {
      const user = userEvent.setup();
      mockApiClient.patch = vi.fn().mockResolvedValue({
        data: { id: '123', name: 'Updated Interface' },
      });

      render(
        <BrowserRouter>
          <ApplicationInterfaceFormPage isEdit={true} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByLabelText(/Interface Name/i)).toHaveValue('Existing Interface');
      });

      const nameInput = screen.getByLabelText(/Interface Name/i);
      await user.clear(nameInput);
      await user.type(nameInput, 'Updated Interface');

      const submitButton = screen.getByRole('button', { name: /Save Changes/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockNavigate).toHaveBeenCalledWith('/entities/application-interfaces/123');
      });
    });
  });
});
