import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { ServerFormPage } from './ServerFormPage';
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

describe('ServerFormPage', () => {
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
          <ServerFormPage isEdit={false} />
        </BrowserRouter>
      );

      expect(screen.getByRole('heading', { name: /Create Server/i })).toBeInTheDocument();
      expect(screen.getByLabelText(/Server Name/i)).toHaveValue('');
      expect(screen.getByRole('button', { name: /Create Server/i })).toBeInTheDocument();
    });

    it('creates server successfully with valid data', async () => {
      const user = userEvent.setup();
      mockApiClient.post = vi.fn().mockResolvedValue({
        data: { id: '456', name: 'Test Server' },
      });

      render(
        <BrowserRouter>
          <ServerFormPage isEdit={false} />
        </BrowserRouter>
      );

      await user.type(screen.getByLabelText(/Server Name/i), 'Test Server');
      await user.type(screen.getByLabelText(/Owner/i), 'user123');

      const submitButton = screen.getByRole('button', { name: /Create Server/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockApiClient.post).toHaveBeenCalledWith(
          '/servers',
          expect.objectContaining({
            name: 'Test Server',
            owner: 'user123',
          })
        );
        expect(mockNavigate).toHaveBeenCalledWith('/entities/servers/456');
      });
    });

    it('handles validation errors', async () => {
      const user = userEvent.setup();

      render(
        <BrowserRouter>
          <ServerFormPage isEdit={false} />
        </BrowserRouter>
      );

      const submitButton = screen.getByRole('button', { name: /Create Server/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(screen.getByText(/name is required/i)).toBeInTheDocument();
      });
    });
  });

  describe('Edit Mode', () => {
    const mockExistingServer = {
      id: '123',
      name: 'Existing Server',
      description: 'Test server',
      owner: 'user123',
      lifecycle: 'active',
      classification: 'internal',
      environment: 'Production',
      hostname: 'server01.example.com',
      ipAddress: '192.168.1.1',
      operatingSystem: 'Linux',
      location: 'Data Center A',
    };

    beforeEach(() => {
      mockUseEntity.mockReturnValue({
        data: mockExistingServer,
        isLoading: false,
        error: null,
        refetch: vi.fn(),
      });
    });

    it('renders edit form with existing data', async () => {
      render(
        <BrowserRouter>
          <ServerFormPage isEdit={true} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByRole('heading', { name: /Edit Server/i })).toBeInTheDocument();
        expect(screen.getByLabelText(/Server Name/i)).toHaveValue('Existing Server');
      });
    });

    it('updates server successfully', async () => {
      const user = userEvent.setup();
      mockApiClient.patch = vi.fn().mockResolvedValue({ data: { id: '123', name: 'Updated Server' } });

      render(
        <BrowserRouter>
          <ServerFormPage isEdit={true} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByLabelText(/Server Name/i)).toHaveValue('Existing Server');
      });

      const nameInput = screen.getByLabelText(/Server Name/i);
      await user.clear(nameInput);
      await user.type(nameInput, 'Updated Server');

      const submitButton = screen.getByRole('button', { name: /Save Changes/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockNavigate).toHaveBeenCalledWith('/entities/servers/123');
      });
    });
  });
});
