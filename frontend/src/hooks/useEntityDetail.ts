/**
 * useEntityDetail Hook
 * Custom hook for fetching and managing entity detail data
 */

import { useQuery } from '@tanstack/react-query';
import { EntityType } from '../types/entities';
import {
  applicationsApi,
  serversApi,
  integrationsApi,
  dataEntitiesApi,
  businessCapabilitiesApi,
  organizationsApi,
  relationsApi,
  applicationServicesApi,
  applicationInterfacesApi,
} from '../api/entitiesApi';

interface UseEntityDetailOptions {
  entityType: EntityType;
  id: string;
  enabled?: boolean;
}

const getApiForEntityType = (entityType: EntityType) => {
  switch (entityType) {
    case EntityType.APPLICATION:
      return applicationsApi;
    case EntityType.SERVER:
      return serversApi;
    case EntityType.INTEGRATION:
      return integrationsApi;
    case EntityType.DATA_ENTITY:
      return dataEntitiesApi;
    case EntityType.BUSINESS_CAPABILITY:
      return businessCapabilitiesApi;
    case EntityType.ORGANIZATION:
      return organizationsApi;
    case EntityType.RELATION:
      return relationsApi;
    case EntityType.APPLICATION_SERVICE:
      return applicationServicesApi;
    case EntityType.APPLICATION_INTERFACE:
      return applicationInterfacesApi;
    default:
      throw new Error(`Unknown entity type: ${entityType}`);
  }
};

export function useEntityDetail<T = any>({ entityType, id, enabled = true }: UseEntityDetailOptions) {
  const api = getApiForEntityType(entityType);

  const query = useQuery<T, any>({
    queryKey: [entityType, id],
    queryFn: () => api.getById(id) as Promise<T>,
    enabled: enabled && !!id,
    staleTime: 30000, // 30 seconds
    retry: (failureCount, error: any) => {
      // Don't retry on 404 (not found) or 403 (forbidden)
      if (error?.response?.status === 404 || error?.response?.status === 403) {
        return false;
      }
      return failureCount < 2;
    },
  });

  return {
    entity: query.data,
    loading: query.isLoading,
    error: query.error,
    isNotFound: (query.error as any)?.response?.status === 404,
    isForbidden: (query.error as any)?.response?.status === 403,
    refetch: query.refetch,
  };
}

/**
 * useEntityRelationships Hook
 * Custom hook for fetching entity relationships
 */

interface Relationship {
  id: string;
  type: string;
  sourceEntityType: string;
  sourceEntityId: string;
  targetEntityType: string;
  targetEntityId: string;
  metadata?: Record<string, any>;
}

interface UseEntityRelationshipsOptions {
  entityType: EntityType;
  id: string;
  enabled?: boolean;
}

export function useEntityRelationships({ entityType, id, enabled = true }: UseEntityRelationshipsOptions) {
  const query = useQuery({
    queryKey: [entityType, id, 'relationships'],
    queryFn: async () => {
      // This endpoint will be implemented in the backend
      // For now, return empty array
      // const response = await entitiesApi[entityType].getRelationships(id);
      // return response;
      return [] as Relationship[];
    },
    enabled: enabled && !!id,
    staleTime: 60000, // 1 minute
  });

  return {
    relationships: query.data || [],
    loading: query.isLoading,
    error: query.error,
    refetch: query.refetch,
  };
}
