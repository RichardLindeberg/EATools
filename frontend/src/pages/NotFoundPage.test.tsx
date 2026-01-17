/**
 * NotFoundPage Component tests
 */

import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { NotFoundPage } from './NotFoundPage';

const renderNotFoundPage = () => {
  return render(
    <BrowserRouter>
      <NotFoundPage />
    </BrowserRouter>
  );
};

describe('NotFoundPage Component', () => {
  it('should render 404 page', () => {
    renderNotFoundPage();
    expect(screen.getByText('404')).toBeInTheDocument();
  });

  it('should display not found message', () => {
    renderNotFoundPage();
    expect(screen.getByText(/Page Not Found/)).toBeInTheDocument();
  });

  it('should have navigation buttons', () => {
    renderNotFoundPage();
    const buttons = screen.getAllByRole('button');
    expect(buttons.length).toBeGreaterThanOrEqual(2);
  });

  it('should have Go Home button', () => {
    renderNotFoundPage();
    expect(screen.getByText('Go to Home')).toBeInTheDocument();
  });

  it('should have Go Back button', () => {
    renderNotFoundPage();
    expect(screen.getByText('Go Back')).toBeInTheDocument();
  });

  it('should render error container', () => {
    renderNotFoundPage();
    const errorPage = document.querySelector('.not-found-page');
    expect(errorPage).toBeInTheDocument();
  });
});
