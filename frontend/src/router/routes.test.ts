/**
 * Router tests
 * Tests for route configuration and navigation
 */

import { describe, it, expect } from 'vitest';
import { routes } from './routes';

describe('Router - Routes Configuration', () => {
  it('should define main layout route', () => {
    const mainRoute = routes.find(r => !r.path);
    expect(mainRoute).toBeDefined();
  });

  it('should have child routes under main layout', () => {
    const mainRoute = routes.find(r => !r.path);
    expect(mainRoute?.children).toBeDefined();
    expect(mainRoute?.children?.length).toBeGreaterThan(0);
  });

  it('should define admin routes', () => {
    const mainRoute = routes.find(r => !r.path);
    const adminRoute = mainRoute?.children?.find(r => r.path === 'admin');
    expect(adminRoute).toBeDefined();
    expect(adminRoute?.children).toBeDefined();
  });

  it('should define applications entity routes', () => {
    const mainRoute = routes.find(r => !r.path);
    const appRoute = mainRoute?.children?.find(r => r.path === 'entities/applications');
    expect(appRoute).toBeDefined();
  });

  it('should define servers entity routes', () => {
    const mainRoute = routes.find(r => !r.path);
    const serversRoute = mainRoute?.children?.find(r => r.path === 'entities/servers');
    expect(serversRoute).toBeDefined();
  });

  it('should define integrations entity routes', () => {
    const mainRoute = routes.find(r => !r.path);
    const intRoute = mainRoute?.children?.find(r => r.path === 'entities/integrations');
    expect(intRoute).toBeDefined();
  });

  it('should have correct route structure', () => {
    expect(routes.length).toBeGreaterThan(0);
    expect(Array.isArray(routes)).toBe(true);
  });

  it('should have error element for 404', () => {
    const mainRoute = routes.find(r => !r.path);
    expect(mainRoute?.errorElement).toBeDefined();
  });

  it('should have wildcard route', () => {
    const mainRoute = routes.find(r => !r.path);
    const wildcardRoute = mainRoute?.children?.find(r => r.path === '*');
    expect(wildcardRoute).toBeDefined();
  });
});
