/**
 * Application Create Flow Integration Test
 */
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { ApplicationFormPage } from './ApplicationFormPage';
import * as apiClient from '../../api/client';
// Mock useEntity to avoid requiring React Query provider in create mode
vi.mock('../../hooks/useEntity', () => ({
  useEntity: vi.fn(() => ({ data: null, isLoading: false, error: null, refetch: vi.fn() })),
}));

const mockNavigate = vi.fn();
// Do not mock api client module; spy on the real instance instead
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
    useParams: () => ({}),
  };
});

describe('ApplicationCreateFlow', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockNavigate.mockClear();
  });

  it('submits minimal required fields and navigates to detail', async () => {
    const user = userEvent.setup();
    const postSpy = vi.spyOn(apiClient.apiClient, 'post');
    postSpy.mockResolvedValue({ data: { id: '999' } } as any);

    render(
      <BrowserRouter>
        <ApplicationFormPage isEdit={false} />
      </BrowserRouter>
    );

    // Fill required fields
    await user.type(screen.getByLabelText(/Application Name/i), 'New App');
    await user.selectOptions(screen.getByLabelText(/Type/i), 'Web');
    await user.selectOptions(screen.getByLabelText(/Lifecycle State/i), 'active');
    await user.selectOptions(screen.getByLabelText(/Environment/i), 'Development');
    await user.type(
      screen.getByLabelText(/Owner/i, { selector: '#owner' }),
      '123e4567-e89b-12d3-a456-426614174000'
    );

    // Submit
    await user.click(screen.getByRole('button', { name: /Create Application/i }));

    await waitFor(() => {
      expect(postSpy).toHaveBeenCalledTimes(1);
      const [url, payload] = postSpy.mock.calls[0] as [string, any];
      expect(url).toBe('/applications');
      expect(payload).toMatchObject({
        name: 'New App',
        type: 'Web',
        lifecycle: 'active',
        environment: 'Development',
        owner: '123e4567-e89b-12d3-a456-426614174000',
      });
      expect(mockNavigate).toHaveBeenCalledWith('/entities/applications/999');
    });
  });
});
