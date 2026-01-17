/**
 * useQueryParams Hook
 * Manage query parameters for pagination, sorting, filtering
 */

import { useSearchParams } from 'react-router-dom';
import { useCallback } from 'react';

export interface QueryParams {
  page?: number;
  limit?: number;
  sort?: string;
  order?: 'asc' | 'desc';
  search?: string;
  filter?: Record<string, string | boolean | number>;
}

/**
 * Hook for managing query parameters
 * @param defaultParams Default query parameters
 * @returns Object with query params and setter functions
 */
export const useQueryParams = (defaultParams?: QueryParams) => {
  const [searchParams, setSearchParams] = useSearchParams();

  const params: QueryParams = {
    page: searchParams.get('page')
      ? parseInt(searchParams.get('page')!, 10)
      : defaultParams?.page ?? 1,
    limit: searchParams.get('limit')
      ? parseInt(searchParams.get('limit')!, 10)
      : defaultParams?.limit ?? 10,
    sort: searchParams.get('sort') || defaultParams?.sort || undefined,
    order: (searchParams.get('order') as 'asc' | 'desc') || defaultParams?.order || 'asc',
    search: searchParams.get('search') || defaultParams?.search || undefined,
  };

  const setPage = useCallback(
    (page: number) => {
      const newParams = new URLSearchParams(searchParams);
      newParams.set('page', page.toString());
      setSearchParams(newParams);
    },
    [searchParams, setSearchParams]
  );

  const setLimit = useCallback(
    (limit: number) => {
      const newParams = new URLSearchParams(searchParams);
      newParams.set('limit', limit.toString());
      newParams.set('page', '1'); // Reset to first page
      setSearchParams(newParams);
    },
    [searchParams, setSearchParams]
  );

  const setSort = useCallback(
    (sort: string, order: 'asc' | 'desc' = 'asc') => {
      const newParams = new URLSearchParams(searchParams);
      newParams.set('sort', sort);
      newParams.set('order', order);
      newParams.set('page', '1'); // Reset to first page
      setSearchParams(newParams);
    },
    [searchParams, setSearchParams]
  );

  const setSearch = useCallback(
    (search: string) => {
      const newParams = new URLSearchParams(searchParams);
      if (search) {
        newParams.set('search', search);
      } else {
        newParams.delete('search');
      }
      newParams.set('page', '1'); // Reset to first page
      setSearchParams(newParams);
    },
    [searchParams, setSearchParams]
  );

  const setFilter = useCallback(
    (filterKey: string, filterValue: string | boolean | number | undefined) => {
      const newParams = new URLSearchParams(searchParams);
      if (filterValue !== undefined && filterValue !== '' && filterValue !== false) {
        newParams.set(`filter_${filterKey}`, filterValue.toString());
      } else {
        newParams.delete(`filter_${filterKey}`);
      }
      newParams.set('page', '1'); // Reset to first page
      setSearchParams(newParams);
    },
    [searchParams, setSearchParams]
  );

  const clearFilters = useCallback(() => {
    const newParams = new URLSearchParams();
    newParams.set('page', '1');
    newParams.set('limit', defaultParams?.limit?.toString() || '10');
    setSearchParams(newParams);
  }, [searchParams, setSearchParams, defaultParams]);

  return {
    params,
    setPage,
    setLimit,
    setSort,
    setSearch,
    setFilter,
    clearFilters,
  };
};
