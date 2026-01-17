/**
 * useEntityList Hook
 * Reusable hook for fetching, paginating, filtering, and sorting entities
 */

import { useState, useEffect, useCallback } from 'react';
import { useQueryParams } from './useQueryParams';
import { ListResponse, PaginationParams, SortParams, FilterParams, Entity } from '../types/entities';

export interface EntityListState<T extends Entity> {
  items: T[];
  total: number;
  loading: boolean;
  error: Error | null;
}

export interface UseEntityListOptions {
  defaultLimit?: number;
  defaultSort?: string;
  defaultOrder?: 'asc' | 'desc';
}

/**
 * Hook for managing entity list state with API calls
 */
export const useEntityList = <T extends Entity>(
  fetchFn: (
    pagination?: PaginationParams,
    sort?: SortParams,
    filters?: FilterParams,
    search?: string
  ) => Promise<ListResponse<T>>,
  options: UseEntityListOptions = {}
) => {
  const { defaultLimit = 10, defaultSort, defaultOrder = 'asc' } = options;
  const { params, setPage, setLimit, setSort, setSearch, clearFilters } = useQueryParams({
    page: 1,
    limit: defaultLimit,
    sort: defaultSort,
    order: defaultOrder,
  });

  const [state, setState] = useState<EntityListState<T>>({
    items: [],
    total: 0,
    loading: true,
    error: null,
  });

  const fetchEntities = useCallback(async () => {
    try {
      setState(prev => ({ ...prev, loading: true, error: null }));

      const pagination: PaginationParams = {
        skip: ((params.page || 1) - 1) * (params.limit || defaultLimit),
        take: params.limit || defaultLimit,
      };

      const sort: SortParams = {
        sort: params.sort,
        order: params.order as 'asc' | 'desc',
      };

      const data = await fetchFn(pagination, sort, {}, params.search);

      setState({
        items: data.items,
        total: data.total,
        loading: false,
        error: null,
      });
    } catch (error) {
      setState(prev => ({
        ...prev,
        loading: false,
        error: error instanceof Error ? error : new Error(String(error)),
      }));
    }
  }, [fetchFn, params, defaultLimit]);

  useEffect(() => {
    fetchEntities();
  }, [fetchFn, params, defaultLimit]);

  const totalPages = Math.ceil(state.total / (params.limit || defaultLimit));

  return {
    ...state,
    params,
    setPage,
    setLimit,
    setSort,
    setSearch,
    clearFilters,
    totalPages,
    refetch: fetchEntities,
  };
};

/**
 * Hook for managing bulk selection
 */
export const useBulkSelection = () => {
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  const toggleSelect = useCallback((id: string) => {
    setSelectedIds(prev => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  }, []);

  const selectAll = useCallback((ids: string[]) => {
    setSelectedIds(new Set(ids));
  }, []);

  const clearSelection = useCallback(() => {
    setSelectedIds(new Set());
  }, []);

  const isSelected = useCallback((id: string) => {
    return selectedIds.has(id);
  }, [selectedIds]);

  const isAllSelected = useCallback(
    (ids: string[]) => {
      return ids.length > 0 && ids.every(id => selectedIds.has(id));
    },
    [selectedIds]
  );

  const isSomeSelected = useCallback(
    (ids: string[]) => {
      return ids.some(id => selectedIds.has(id));
    },
    [selectedIds]
  );

  return {
    selectedIds: Array.from(selectedIds),
    selectedCount: selectedIds.size,
    toggleSelect,
    selectAll,
    clearSelection,
    isSelected,
    isAllSelected,
    isSomeSelected,
  };
};

/**
 * Hook for entity actions (delete, archive, etc.)
 */
export const useEntityActions = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const deleteEntity = useCallback(async (deleteFn: () => Promise<void>) => {
    try {
      setLoading(true);
      setError(null);
      await deleteFn();
    } catch (err) {
      setError(err instanceof Error ? err : new Error(String(err)));
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const bulkDelete = useCallback(async (bulkDeleteFn: (ids: string[]) => Promise<void>, ids: string[]) => {
    try {
      setLoading(true);
      setError(null);
      await bulkDeleteFn(ids);
    } catch (err) {
      setError(err instanceof Error ? err : new Error(String(err)));
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    loading,
    error,
    deleteEntity,
    bulkDelete,
  };
};
