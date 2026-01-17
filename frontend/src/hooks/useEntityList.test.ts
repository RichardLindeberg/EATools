/**
 * useEntityList Hook Tests
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useEntityList, useBulkSelection, useEntityActions } from '../../hooks/useEntityList';

describe('useEntityList', () => {
  it('returns initial state', () => {
    const mockFetch = vi.fn().mockResolvedValue({ items: [], total: 0 });
    const { result } = renderHook(() => useEntityList(mockFetch));

    expect(result.current.items).toEqual([]);
    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBeNull();
    expect(result.current.params.page).toBe(1);
    expect(result.current.params.limit).toBe(10);
  });

  it('fetches items on mount', async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      items: [{ id: '1', name: 'Item 1' }],
      total: 1,
    });

    const { result } = renderHook(() => useEntityList(mockFetch));

    expect(result.current.loading).toBe(true);

    await act(async () => {
      await new Promise((resolve) => setTimeout(resolve, 0));
    });

    expect(mockFetch).toHaveBeenCalled();
  });

  it('updates page', async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      items: [],
      total: 100,
    });

    const { result } = renderHook(() => useEntityList(mockFetch));

    await act(async () => {
      result.current.setPage(2);
    });

    expect(result.current.params.page).toBe(2);
  });

  it('updates limit', async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      items: [],
      total: 100,
    });

    const { result } = renderHook(() => useEntityList(mockFetch));

    await act(async () => {
      result.current.setLimit(25);
    });

    expect(result.current.params.limit).toBe(25);
  });

  it('updates sort', async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      items: [],
      total: 100,
    });

    const { result } = renderHook(() => useEntityList(mockFetch));

    await act(async () => {
      result.current.setSort('name', 'desc');
    });

    expect(result.current.params.sort).toBe('name');
    expect(result.current.params.order).toBe('desc');
  });

  it('updates search', async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      items: [],
      total: 100,
    });

    const { result } = renderHook(() => useEntityList(mockFetch));

    await act(async () => {
      result.current.setSearch('test');
    });

    expect(result.current.params.search).toBe('test');
  });

  it('clears filters', async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      items: [],
      total: 100,
    });

    const { result } = renderHook(() => useEntityList(mockFetch));

    await act(async () => {
      result.current.setSort('name');
      result.current.setSearch('test');
      result.current.clearFilters();
    });

    expect(result.current.params.sort).toBe('id');
    expect(result.current.params.search).toBe('');
  });

  it('handles fetch errors', async () => {
    const error = new Error('Fetch failed');
    const mockFetch = vi.fn().mockRejectedValue(error);

    const { result } = renderHook(() => useEntityList(mockFetch));

    await act(async () => {
      await new Promise((resolve) => setTimeout(resolve, 0));
    });

    expect(result.current.error).toBeTruthy();
  });
});

describe('useBulkSelection', () => {
  it('returns initial state', () => {
    const { result } = renderHook(() => useBulkSelection());

    expect(result.current.selectedIds).toEqual(new Set());
    expect(result.current.selectedCount).toBe(0);
  });

  it('toggles selection', () => {
    const { result } = renderHook(() => useBulkSelection());

    act(() => {
      result.current.toggleSelect('1');
    });

    expect(result.current.isSelected('1')).toBe(true);
    expect(result.current.selectedCount).toBe(1);
  });

  it('toggles off selected item', () => {
    const { result } = renderHook(() => useBulkSelection());

    act(() => {
      result.current.toggleSelect('1');
      result.current.toggleSelect('1');
    });

    expect(result.current.isSelected('1')).toBe(false);
    expect(result.current.selectedCount).toBe(0);
  });

  it('selects all items', () => {
    const { result } = renderHook(() => useBulkSelection());
    const ids = ['1', '2', '3'];

    act(() => {
      result.current.selectAll(ids);
    });

    ids.forEach((id) => {
      expect(result.current.isSelected(id)).toBe(true);
    });
    expect(result.current.selectedCount).toBe(3);
  });

  it('clears selection', () => {
    const { result } = renderHook(() => useBulkSelection());

    act(() => {
      result.current.toggleSelect('1');
      result.current.toggleSelect('2');
      result.current.clearSelection();
    });

    expect(result.current.selectedCount).toBe(0);
    expect(result.current.isSelected('1')).toBe(false);
  });

  it('checks if all items are selected', () => {
    const { result } = renderHook(() => useBulkSelection());
    const ids = ['1', '2', '3'];

    act(() => {
      result.current.selectAll(ids);
    });

    expect(result.current.isAllSelected(ids)).toBe(true);
  });

  it('checks if some items are selected', () => {
    const { result } = renderHook(() => useBulkSelection());
    const ids = ['1', '2', '3'];

    act(() => {
      result.current.toggleSelect('1');
    });

    expect(result.current.isSomeSelected(ids)).toBe(true);
  });
});

describe('useEntityActions', () => {
  it('returns initial state', () => {
    const { result } = renderHook(() => useEntityActions());

    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBeNull();
  });

  it('handles delete entity', async () => {
    const { result } = renderHook(() => useEntityActions());
    const mockDelete = vi.fn().mockResolvedValue(undefined);

    await act(async () => {
      await result.current.deleteEntity(mockDelete);
    });

    expect(mockDelete).toHaveBeenCalled();
    expect(result.current.error).toBeNull();
  });

  it('sets loading state during delete', async () => {
    const { result } = renderHook(() => useEntityActions());
    const mockDelete = vi.fn(
      () => new Promise((resolve) => setTimeout(resolve, 10))
    );

    act(() => {
      result.current.deleteEntity(mockDelete);
    });

    expect(result.current.loading).toBe(true);

    await act(async () => {
      await new Promise((resolve) => setTimeout(resolve, 20));
    });

    expect(result.current.loading).toBe(false);
  });

  it('handles delete errors', async () => {
    const { result } = renderHook(() => useEntityActions());
    const error = new Error('Delete failed');
    const mockDelete = vi.fn().mockRejectedValue(error);

    await act(async () => {
      await result.current.deleteEntity(mockDelete);
    });

    expect(result.current.error).toBeTruthy();
  });

  it('handles bulk delete', async () => {
    const { result } = renderHook(() => useEntityActions());
    const mockBulkDelete = vi.fn().mockResolvedValue(undefined);

    await act(async () => {
      await result.current.bulkDelete(mockBulkDelete, ['1', '2', '3']);
    });

    expect(mockBulkDelete).toHaveBeenCalled();
    expect(result.current.error).toBeNull();
  });

  it('handles bulk delete errors', async () => {
    const { result } = renderHook(() => useEntityActions());
    const error = new Error('Bulk delete failed');
    const mockBulkDelete = vi.fn().mockRejectedValue(error);

    await act(async () => {
      await result.current.bulkDelete(mockBulkDelete, ['1', '2', '3']);
    });

    expect(result.current.error).toBeTruthy();
  });
});
