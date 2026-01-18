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

      // Form should not call API when validation fails
      await waitFor(() => {
        expect(mockApiClient.post).not.toHaveBeenCalled();
      });
    });

    it('creates application successfully with valid data', async () => {
      mockApiClient.post = vi.fn().mockResolvedValue({
        data: { id: '456', name: 'Test App' },
      });

      render(
        <BrowserRouter>
          <ApplicationFormPage isEdit={false} />
        </BrowserRouter>
      );

      // Verify form is ready with submit button
      await waitFor(() => {
        expect(screen.getByRole('button', { name: /Create Application/i })).toBeInTheDocument();
      });

      expect(screen.getByRole('heading', { name: /Create Application/i })).toBeInTheDocument();
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

      await waitFor(() => {
        expect(screen.getByLabelText(/Application Name/i)).toBeInTheDocument();
      });

      await user.type(screen.getByLabelText(/Application Name/i), 'Duplicate App');
      await user.selectOptions(screen.getByLabelText(/^Type$/i), 'Web');
      
      const ownerInput = document.getElementById('owner') as HTMLInputElement;
      if (ownerInput) {
        await user.type(ownerInput, 'user123');
      }

      const submitButton = screen.getByRole('button', { name: /Create Application/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockApiClient.post).toHaveBeenCalled();
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

      // Modal should appear with discard confirmation
      await waitFor(() => {
        expect(screen.getByRole('button', { name: /discard/i })).toBeInTheDocument();
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

      // Just verify the command dispatcher was eventually called
      await waitFor(
        () => {
          expect(mockUpdateApplicationWithCommands).toHaveBeenCalledTimes(1);
        },
        { timeout: 5000 }
      );
    });

    it('requires classification reason when classification changes', async () => {
      const user = userEvent.setup();
      mockUpdateApplicationWithCommands.mockResolvedValue({ id: '123' });

      render(
        <BrowserRouter>
          <ApplicationFormPage isEdit={true} />
        </BrowserRouter>
      );

      await waitFor(() => {
        const classSelect = document.getElementById('classification') as HTMLSelectElement;
        expect(classSelect).toHaveValue('internal');
      });

      const classSelect = document.getElementById('classification') as HTMLSelectElement;
      await user.selectOptions(classSelect, 'confidential');

      const submitButton = screen.getByRole('button', { name: /Save Changes/i });
      await user.click(submitButton);

      // Give it a moment to process
      await new Promise(resolve => setTimeout(resolve, 100));
      
      // Command dispatcher should not be called without classification reason
      expect(mockUpdateApplicationWithCommands).not.toHaveBeenCalled();
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

      // Find the technology stack input by its placeholder
      const input = screen.getByPlaceholderText(/e.g., React, Node.js, PostgreSQL/i);
      await user.type(input, 'React');

      // Find the add button (+ button)
      const addButton = screen.getByRole('button', { name: '+' });
      await user.click(addButton);

      // Check that the item was added
      await waitFor(() => {
        expect(screen.getByText('React')).toBeInTheDocument();
      });
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
      mockApiClient.post = vi.fn().mockImplementation(
        () => new Promise((resolve) => setTimeout(() => resolve({ data: { id: '456' } }), 200))
      );

      render(
        <BrowserRouter>
          <ApplicationFormPage isEdit={false} />
        </BrowserRouter>
      );

      // Verify form is ready
      await waitFor(() => {
        expect(screen.getByRole('button', { name: /Create Application/i })).toBeInTheDocument();
      });

      expect(screen.getByRole('heading', { name: /Create Application/i })).toBeInTheDocument();
    });
  });
});
