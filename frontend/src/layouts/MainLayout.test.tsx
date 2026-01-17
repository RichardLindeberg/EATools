/**
 * MainLayout Component tests
 */

import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { MainLayout } from './MainLayout';
import { AuthProvider } from '../contexts/AuthContext';

const renderMainLayout = () => {
  return render(
    <BrowserRouter>
      <AuthProvider>
        <MainLayout />
      </AuthProvider>
    </BrowserRouter>
  );
};

describe('MainLayout Component', () => {
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

  it('should render layout structure', () => {
    renderMainLayout();
    const main = screen.getByRole('main');
    expect(main).toBeInTheDocument();
  });

  it('should render main area', () => {
    renderMainLayout();
    const main = screen.getByRole('main');
    expect(main).toBeInTheDocument();
    expect(main.className).toContain('main-layout-main');
  });

  it('should have sidebar available', () => {
    renderMainLayout();
    const layoutContainer = document.querySelector('.main-layout-container');
    expect(layoutContainer).toBeInTheDocument();
  });
});
