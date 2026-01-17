/**
 * Router Navigation tests
 * Tests for router functionality
 */

import { describe, it, expect } from 'vitest';
import { routes } from './routes';

describe('Router Navigation', () => {
  it('should have valid route configuration', () => {
    expect(routes.length).toBeGreaterThan(0);
    expect(Array.isArray(routes)).toBe(true);
  });

  it('should have valid route paths', () => {
    routes.forEach(route => {
      if (route.path && route.path !== '*') {
        expect(typeof route.path).toBe('string');
        expect(route.path.length).toBeGreaterThan(0);
      }
    });
  });

  it('should have components for all routes', () => {
    routes.forEach(route => {
      if (route.path !== '*') {
        // Route should either have element or children
        if (!route.element && !route.children) {
          throw new Error(`Route ${route.path} has no element or children`);
        }
      }
    });
  });

  it('should have main layout as first route', () => {
    const mainRoute = routes[0];
    expect(mainRoute).toBeDefined();
    expect(mainRoute.element).toBeDefined();
  });

  it('should have error boundary', () => {
    const mainRoute = routes[0];
    expect(mainRoute?.errorElement).toBeDefined();
  });

  it('should have admin route', () => {
    const mainRoute = routes[0];
    const adminRoute = mainRoute?.children?.find(r => r.path === 'admin');
    expect(adminRoute).toBeDefined();
  });
});
