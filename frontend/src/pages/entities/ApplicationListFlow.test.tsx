import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import axios from 'axios';
import { ApplicationListPage } from './ApplicationListPage';

describe('ApplicationListPage integration: filters & sorting', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('calls list API with default params on initial render', async () => {
    const getSpy = vi
      .spyOn(axios, 'get')
      .mockResolvedValue({ data: { items: [], total: 0 } });

    render(
      <BrowserRouter>
        <ApplicationListPage />
      </BrowserRouter>
    );

    // Wait for initial request
    expect(getSpy).toHaveBeenCalled();
    const url = (getSpy.mock.calls[0] as any[])[0] as string;
    expect(url).toMatch(/\/(applications)(\?|$)/);
    expect(url).toMatch(/page=1/);
    expect(url).toMatch(/limit=10/);
    expect(url).toMatch(/sort=name/);
    expect(url).toMatch(/order=asc/);
  });

  it('applies filters and sorting and includes them in query params', async () => {
    const user = userEvent.setup();

    const getSpy = vi
      .spyOn(axios, 'get')
      .mockResolvedValue({ data: { items: [], total: 0 } });

    render(
      <BrowserRouter>
        <ApplicationListPage />
      </BrowserRouter>
    );

    // Interact with filter panel
    const typeSelect = screen.getByLabelText(/Application Type/i);
    await user.selectOptions(typeSelect, 'web');

    const statusSelect = screen.getByLabelText(/Status/i);
    await user.selectOptions(statusSelect, 'active');

    const ownerInput = screen.getByLabelText(/Owner/i);
    await user.clear(ownerInput);
    await user.type(ownerInput, 'alice');

    // Trigger sort by Name to toggle to desc
    const sortButton = screen.getByRole('button', { name: /Sort by Name/i });
    await user.click(sortButton);

    // Expect subsequent request to include filters and order=desc
    expect(getSpy.mock.calls.length).toBeGreaterThanOrEqual(2);
    const lastUrl = (getSpy.mock.calls[getSpy.mock.calls.length - 1] as any[])[0] as string;
    expect(lastUrl).toMatch(/type=web/);
    expect(lastUrl).toMatch(/status=active/);
    expect(lastUrl).toMatch(/owner=alice/);
    expect(lastUrl).toMatch(/sort=name/);
    expect(lastUrl).toMatch(/order=desc/);
    expect(lastUrl).toMatch(/page=1/); // page reset on sort
  });
});
