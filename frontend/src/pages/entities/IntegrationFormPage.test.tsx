import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { IntegrationFormPage } from './IntegrationFormPage';
import * as entityHook from '../../hooks/useEntity';
import * as commandDispatcher from '../../utils/commandDispatcher';

const mockNavigate = vi.fn();
vi.mock('../../hooks/useEntity');
vi.mock('../../utils/commandDispatcher');
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
    useParams: () => ({ id: 'integration-001' }),
  };
});

describe('IntegrationFormPage', () => {
  const mockUseEntity = vi.mocked(entityHook.useEntity);

  beforeEach(() => {
    vi.clearAllMocks();
    mockUseEntity.mockReturnValue({
      data: null,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });
  });


  it('renders submit button', () => {
    render(
      <BrowserRouter>
        <IntegrationFormPage isEdit={false} />
      </BrowserRouter>
    );
    expect(screen.getByRole('button', { name: /Submit|Create|Save/i })).toBeInTheDocument();
  });

  it('renders cancel button', () => {
    render(
      <BrowserRouter>
        <IntegrationFormPage isEdit={false} />
      </BrowserRouter>
    );
    expect(screen.getByRole('button', { name: /Cancel|Back/i })).toBeInTheDocument();
  });

  it('calls navigate on cancel', async () => {
    const user = userEvent.setup();
    render(
      <BrowserRouter>
        <IntegrationFormPage isEdit={false} />
      </BrowserRouter>
    );
    const cancelButton = screen.getByRole('button', { name: /Cancel|Back/i });
    await user.click(cancelButton);
    expect(mockNavigate).toHaveBeenCalled();
  });

  it('renders edit mode for existing entity', () => {
    mockUseEntity.mockReturnValue({
      data: { id: 'integration-001', name: 'Test Integration' },
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });
    render(
      <BrowserRouter>
        <IntegrationFormPage isEdit={true} />
      </BrowserRouter>
    );
    expect(screen.getByRole('heading', { name: /Edit|Update/i })).toBeInTheDocument();
  });
});
