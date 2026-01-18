import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { ApplicationFormPage } from './ApplicationFormPage';
import * as entityHook from '../../hooks/useEntity';
import * as apiClient from '../../api/client';
import * as commandDispatcher from '../../utils/commandDispatcher';

// Mock dependencies
const mockNavigate = vi.fn();
vi.mock('../../hooks/useEntity');
vi.mock('../../api/client');
vi.mock('../../utils/commandDispatcher');
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
    useParams: () => ({ id: '123' }),
  };
});

describe('ApplicationFormPage', () => {
  const mockUseEntity = vi.mocked(entityHook.useEntity);
  const mockApiClient = vi.mocked(apiClient.apiClient);
  const mockUpdateApplicationWithCommands = vi.mocked(
    commandDispatcher.updateApplicationWithCommands
  );

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
          <ApplicationFormPage isEdit={false} />
        </BrowserRouter>
      );

      expect(screen.getByRole('heading', { name: /Create Application/i })).toBeInTheDocument();
      expect(screen.getByLabelText(/Application Name/i)).toHaveValue('');
      expect(screen.getByLabelText(/Type/i)).toHaveValue('');
      expect(screen.getByRole('button', { name: /Create Application/i })).toBeInTheDocument();
    });

    it('validates required fields on submit', async () => {
      const user = userEvent.setup();

      render(
        <BrowserRouter>
          <ApplicationFormPage isEdit={false} />
        </BrowserRouter>
      );

      const submitButton = screen.getByRole('button', { name: /Create Application/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(screen.getByText(/name is required/i)).toBeInTheDocument();
      });
    });

    it('creates application successfully with valid data', async () => {
      const user = userEvent.setup();
      mockApiClient.post = vi.fn().mockResolvedValue({
        data: { id: '456', name: 'Test App' },
      });

      render(
        <BrowserRouter>
          <ApplicationFormPage isEdit={false} />
        </BrowserRouter>
      );

      await user.type(screen.getByLabelText(/Application Name/i), 'Test App');
      await user.selectOptions(screen.getByLabelText(/Type/i), 'Web');
      await user.type(screen.getByLabelText(/Owner/i), 'user123');

      const submitButton = screen.getByRole('button', { name: /Create Application/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockApiClient.post).toHaveBeenCalledWith(
          '/applications',
          expect.objectContaining({
            name: 'Test App',
            type: 'Web',
            owner: 'user123',
          })
        );
        expect(mockNavigate).toHaveBeenCalledWith('/entities/applications/456');
      });
    });

    it('handles 422 validation errors from API', async () => {
      const user = userEvent.setup();
      mockApiClient.post = vi.fn().mockRejectedValue({
        response: {
          status: 422,
          data: {
            detail: 'Validation failed',
            errors: [{ field: 'name', message: 'Name already exists' }],
          },
        },
      });

      render(
        <BrowserRouter>
          <ApplicationFormPage isEdit={false} />
        </BrowserRouter>
      );

      await user.type(screen.getByLabelText(/Application Name/i), 'Duplicate App');
      await user.selectOptions(screen.getByLabelText(/Type/i), 'Web');
      await user.type(screen.getByLabelText(/Owner/i), 'user123');

      const submitButton = screen.getByRole('button', { name: /Create Application/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(screen.getByText(/Name already exists/i)).toBeInTheDocument();
      });
    });

    it('shows discard changes modal when cancelling with unsaved changes', async () => {
      const user = userEvent.setup();

      render(
        <BrowserRouter>
          <ApplicationFormPage isEdit={false} />
        </BrowserRouter>
      );

      await user.type(screen.getByLabelText(/Application Name/i), 'Test App');

      const cancelButton = screen.getByRole('button', { name: /Cancel/i });
      await user.click(cancelButton);

      await waitFor(() => {
        expect(screen.getByText(/discard your changes/i)).toBeInTheDocument();
      });
    });
  });

  describe('Edit Mode', () => {
    const mockExistingApp = {
      id: '123',
      name: 'Existing App',
      description: 'Test description',
      owner: 'user123',
      lifecycle: 'active',
      classification: 'internal',
      environment: 'Production',
      type: 'Web',
      technologyStack: ['React', 'Node.js'],
      department: 'Engineering',
      businessOwner: 'owner123',
      critical: true,
      url: 'https://example.com',
    };

    beforeEach(() => {
      mockUseEntity.mockReturnValue({
        data: mockExistingApp,
        isLoading: false,
        error: null,
        refetch: vi.fn(),
      });
    });

    it('renders edit form with existing data', async () => {
      render(
        <BrowserRouter>
          <ApplicationFormPage isEdit={true} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByRole('heading', { name: /Edit Application/i })).toBeInTheDocument();
        expect(screen.getByLabelText(/Application Name/i)).toHaveValue('Existing App');
        expect(screen.getByLabelText(/Type/i)).toHaveValue('Web');
        expect(screen.getByRole('button', { name: /Save Changes/i })).toBeInTheDocument();
      });
    });

    it('shows loading state while fetching data', () => {
      mockUseEntity.mockReturnValue({
        data: null,
        isLoading: true,
        error: null,
        refetch: vi.fn(),
      });

      render(
        <BrowserRouter>
          <ApplicationFormPage isEdit={true} />
        </BrowserRouter>
      );

      expect(screen.getByText(/Loading.../i)).toBeInTheDocument();
    });

    it('updates application successfully using command dispatcher', async () => {
      const user = userEvent.setup();
      mockUpdateApplicationWithCommands.mockResolvedValue({
        id: '123',
        name: 'Updated App',
      });

      render(
        <BrowserRouter>
          <ApplicationFormPage isEdit={true} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByLabelText(/Application Name/i)).toHaveValue('Existing App');
      });

      const nameInput = screen.getByLabelText(/Application Name/i);
      await user.clear(nameInput);
      await user.type(nameInput, 'Updated App');

      const submitButton = screen.getByRole('button', { name: /Save Changes/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockUpdateApplicationWithCommands).toHaveBeenCalledWith(
          '123',
          expect.objectContaining({ name: 'Existing App' }),
          expect.objectContaining({ name: 'Updated App' })
        );
        expect(mockNavigate).toHaveBeenCalledWith('/entities/applications/123');
      });
    });

    it('requires classification reason when classification changes', async () => {
      const user = userEvent.setup();

      render(
        <BrowserRouter>
          <ApplicationFormPage isEdit={true} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByLabelText(/Classification/i)).toHaveValue('internal');
      });

      await user.selectOptions(screen.getByLabelText(/Classification/i), 'confidential');

      const submitButton = screen.getByRole('button', { name: /Save Changes/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(
          screen.getByText(/Reason is required when changing classification/i)
        ).toBeInTheDocument();
      });
    });

    it('navigates to list page when cancelling without changes', async () => {
      const user = userEvent.setup();

      render(
        <BrowserRouter>
          <ApplicationFormPage isEdit={true} />
        </BrowserRouter>
      );

      await waitFor(() => {
        expect(screen.getByLabelText(/Application Name/i)).toHaveValue('Existing App');
      });

      const cancelButton = screen.getByRole('button', { name: /Cancel/i });
      await user.click(cancelButton);

      expect(mockNavigate).toHaveBeenCalledWith('/entities/applications');
    });
  });

  describe('Form Features', () => {
    it('handles technology stack array field', async () => {
      const user = userEvent.setup();

      render(
        <BrowserRouter>
          <ApplicationFormPage isEdit={false} />
        </BrowserRouter>
      );

      const addButton = screen.getByRole('button', { name: /Add Technology Stack/i });
      await user.click(addButton);

      const inputs = screen.getAllByPlaceholderText(/e.g., React, Node.js, PostgreSQL/i);
      await user.type(inputs[0], 'React');

      expect(inputs[0]).toHaveValue('React');
    });

    it('handles critical checkbox toggle', async () => {
      const user = userEvent.setup();

      render(
        <BrowserRouter>
          <ApplicationFormPage isEdit={false} />
        </BrowserRouter>
      );

      const checkbox = screen.getByRole('checkbox', { name: /This is a critical application/i });
      expect(checkbox).not.toBeChecked();

      await user.click(checkbox);
      expect(checkbox).toBeChecked();
    });

    it('displays submit button as "Saving..." during submission', async () => {
      const user = userEvent.setup();
      mockApiClient.post = vi.fn().mockImplementation(
        () => new Promise((resolve) => setTimeout(() => resolve({ data: { id: '456' } }), 100))
      );

      render(
        <BrowserRouter>
          <ApplicationFormPage isEdit={false} />
        </BrowserRouter>
      );

      await user.type(screen.getByLabelText(/Application Name/i), 'Test App');
      await user.selectOptions(screen.getByLabelText(/Type/i), 'Web');
      await user.type(screen.getByLabelText(/Owner/i), 'user123');

      const submitButton = screen.getByRole('button', { name: /Create Application/i });
      await user.click(submitButton);

      expect(screen.getByRole('button', { name: /Saving.../i })).toBeDisabled();
    });
  });
});
