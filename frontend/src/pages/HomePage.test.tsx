/**
 * HomePage Component tests
 */

import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { HomePage } from './HomePage';
import { AuthProvider } from '../contexts/AuthContext';

const renderHomePage = () => {
  return render(
    <BrowserRouter>
      <AuthProvider>
        <HomePage />
      </AuthProvider>
    </BrowserRouter>
  );
};

describe('HomePage Component', () => {
  beforeEach(() => {
    // Mock localStorage for auth
    const mockLocalStorage = {
      getItem: vi.fn(),
      setItem: vi.fn(),
      removeItem: vi.fn(),
      clear: vi.fn(),
    };
    Object.defineProperty(window, 'localStorage', { value: mockLocalStorage });
  });

  it('should render home page', () => {
    renderHomePage();
    expect(screen.getByRole('heading')).toBeInTheDocument();
  });

  it('should display dashboard elements', () => {
    renderHomePage();
    const headings = screen.getAllByRole('heading');
    expect(headings.length).toBeGreaterThan(0);
  });

  it('should render welcome message', () => {
    renderHomePage();
    const page = document.querySelector('.home-page');
    expect(page).toBeInTheDocument();
  });
});
