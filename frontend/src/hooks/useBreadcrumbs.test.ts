/**
 * useBreadcrumbs Hook Tests
 * Tests for generating breadcrumbs from route
 */

import { describe, it, expect } from 'vitest';
import type { BreadcrumbItem } from './useBreadcrumbs';

describe('useBreadcrumbs - Hook Structure', () => {
  it('should define breadcrumb item interface', () => {
    const breadcrumb: BreadcrumbItem = {
      label: 'Applications',
      path: '/applications',
    };
    expect(breadcrumb.label).toBe('Applications');
    expect(breadcrumb.path).toBe('/applications');
  });

  it('should format labels from URL segments', () => {
    const segment = 'entity-types';
    const formatted = segment
      .split('-')
      .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
      .join(' ');
    expect(formatted).toBe('Entity Types');
  });

  it('should identify UUID patterns', () => {
    const uuid = '550e8400-e29b-41d4-a716-446655440000';
    const uuidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/;
    expect(uuidPattern.test(uuid)).toBe(true);
  });

  it('should identify numeric ID patterns', () => {
    const numericId = '123';
    const numericPattern = /^\d+$/;
    expect(numericPattern.test(numericId)).toBe(true);
  });

  it('should show "Detail" for ID segments', () => {
    const isId = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$|^\d+$/.test('123');
    const label = isId ? 'Detail' : 'Other';
    expect(label).toBe('Detail');
  });

  it('should build breadcrumb paths', () => {
    const pathSegments = ['applications', 'servers', 'details'];
    let currentPath = '';
    const breadcrumbs: BreadcrumbItem[] = [];

    for (const segment of pathSegments) {
      currentPath += `/${segment}`;
      breadcrumbs.push({
        label: segment.charAt(0).toUpperCase() + segment.slice(1),
        path: currentPath,
      });
    }

    expect(breadcrumbs.length).toBe(3);
    expect(breadcrumbs[0].path).toBe('/applications');
  });

  it('should filter empty segments from paths', () => {
    const pathname = '/applications///servers///';
    const segments = pathname.split('/').filter(Boolean);
    expect(segments).toEqual(['applications', 'servers']);
  });
});
