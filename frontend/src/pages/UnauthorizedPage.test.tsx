/**
 * UnauthorizedPage Component tests
 */

import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { UnauthorizedPage } from './UnauthorizedPage';

const renderUnauthorizedPage = () => {
  return render(
    <BrowserRouter>
      <UnauthorizedPage />
    </BrowserRouter>
  );
};

describe('UnauthorizedPage Component', () => {
  it('should render 403 page', () => {
    renderUnauthorizedPage();
    expect(screen.getByText('403')).toBeInTheDocument();
  });

  it('should display access denied message', () => {
    renderUnauthorizedPage();
    expect(screen.getByText('Access Denied')).toBeInTheDocument();
  });

  it('should explain permission issue', () => {
    renderUnauthorizedPage();
    expect(screen.getByText(/don't have permission/)).toBeInTheDocument();
  });

  it('should have navigation buttons', () => {
    renderUnauthorizedPage();
    const buttons = screen.getAllByRole('button');
    expect(buttons.length).toBeGreaterThanOrEqual(2);
  });

  it('should have admin contact info', () => {
    renderUnauthorizedPage();
    expect(screen.getByText(/contact your administrator/)).toBeInTheDocument();
  });

  it('should render error container', () => {
    renderUnauthorizedPage();
    const errorPage = document.querySelector('.unauthorized-page');
    expect(errorPage).toBeInTheDocument();
  });
});
