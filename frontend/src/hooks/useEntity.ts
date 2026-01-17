import { useQuery, useMutation, UseQueryResult, UseMutationResult } from '@tanstack/react-query'
import { apiClient } from './client'
import { Entity, PaginatedResponse, QueryParams } from '../types'

// Generic query hook for fetching entities
export const useEntity = <T extends Entity>(
  entityType: string,
  id: string
): UseQueryResult<T, Error> => {
  return useQuery({
    queryKey: [entityType, id],
    queryFn: async () => {
      const response = await apiClient.get<T>(`/${entityType}/${id}`)
      return response.data
    },
    enabled: !!id,
  })
}

// Generic query hook for listing entities
export const useEntityList = <T extends Entity>(
  entityType: string,
  params?: QueryParams
): UseQueryResult<PaginatedResponse<T>, Error> => {
  return useQuery({
    queryKey: [entityType, 'list', params],
    queryFn: async () => {
      const response = await apiClient.get<PaginatedResponse<T>>(
        `/${entityType}`,
        { params }
      )
      return response.data
    },
  })
}

// Generic mutation hook for creating entities
export const useCreateEntity = <T extends Entity>(
  entityType: string
): UseMutationResult<T, Error, Partial<T>, unknown> => {
  return useMutation({
    mutationFn: async (data: Partial<T>) => {
      const response = await apiClient.post<T>(`/${entityType}`, data)
      return response.data
    },
  })
}

// Generic mutation hook for updating entities
export const useUpdateEntity = <T extends Entity>(
  entityType: string
): UseMutationResult<T, Error, { id: string; data: Partial<T> }, unknown> => {
  return useMutation({
    mutationFn: async ({ id, data }) => {
      const response = await apiClient.put<T>(`/${entityType}/${id}`, data)
      return response.data
    },
  })
}

// Generic mutation hook for deleting entities
export const useDeleteEntity = (
  entityType: string
): UseMutationResult<void, Error, string, unknown> => {
  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.delete(`/${entityType}/${id}`)
    },
  })
}
