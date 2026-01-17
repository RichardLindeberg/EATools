/**
 * useBreadcrumbs Hook
 * Generate breadcrumbs from current route
 */

import { useMemo } from 'react';
import { useLocation } from 'react-router-dom';

export interface BreadcrumbItem {
  label: string;
  path: string;
}

/**
 * Format label from URL segment
 */
const formatLabel = (segment: string): string => {
  // Remove IDs (look like UUIDs or numbers)
  if (/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$|^\d+$/.test(segment)) {
    return 'Detail';
  }

  // Convert kebab-case to Title Case
  return segment
    .split('-')
    .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ');
};

/**
 * Hook for generating breadcrumbs from route
 * @returns Array of breadcrumb items
 */
export const useBreadcrumbs = (): BreadcrumbItem[] => {
  const location = useLocation();

  const breadcrumbs = useMemo(() => {
    const pathSegments = location.pathname.split('/').filter(Boolean);

    if (pathSegments.length === 0) {
      return [];
    }

    const items: BreadcrumbItem[] = [
      {
        label: 'Home',
        path: '/',
      },
    ];

    let currentPath = '';
    for (const segment of pathSegments) {
      currentPath += `/${segment}`;

      // Skip IDs (UUIDs or numbers)
      if (!/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$|^\d+$/.test(segment)) {
        items.push({
          label: formatLabel(segment),
          path: currentPath,
        });
      }
    }

    return items;
  }, [location.pathname]);

  return breadcrumbs;
};
