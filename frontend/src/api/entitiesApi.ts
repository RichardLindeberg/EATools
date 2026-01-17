/**
 * Entities API Client
 * Provides CRUD operations and query support for all entity types
 * Implements CQRS pattern with command endpoints where applicable
 * Reference: openapi.yaml
 */

import axios from 'axios';
import {
  Application,
  DataEntity,
  Server,
  Integration,
  BusinessCapability,
  Organization,
  Relation,
  ApplicationService,
  ApplicationInterface,
  ListResponse,
  EntityType,
} from '../types/entities';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

/**
 * Build query string from parameters
 * Supports pagination (skip/take), sorting, filtering, and search
 */
export const buildQueryString = (params: Record<string, any> = {}): string => {
  const queryParams = new URLSearchParams();

  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      if (typeof value === 'object') {
        // Handle nested objects for filters
        Object.entries(value).forEach(([nestedKey, nestedValue]) => {
          if (nestedValue !== undefined && nestedValue !== null && nestedValue !== '') {
            queryParams.append(`${key}[${nestedKey}]`, String(nestedValue));
          }
        });
      } else {
        queryParams.append(key, String(value));
      }
    }
  });

  return queryParams.toString();
};

/**
 * Generic bulk action handler
 * Dispatches bulk operations (delete, archive, etc.) to the API
 */
const bulkAction = async (
  entityType: string,
  action: 'delete' | 'archive' | string,
  ids: string[]
): Promise<any> => {
  const response = await axios.post(
    `${API_BASE_URL}/${entityType}/bulk-action`,
    { action, ids }
  );
  return response.data;
};

/**
 * Applications API
 * Reference: /applications paths in openapi.yaml
 */
export const applicationsApi = {
  async list(params: any = {}): Promise<ListResponse<Application>> {
    const query = buildQueryString(params);
    const url = `${API_BASE_URL}/applications${query ? '?' + query : ''}`;
    const response = await axios.get(url);
    return response.data;
  },

  async getById(id: string): Promise<Application> {
    const response = await axios.get(`${API_BASE_URL}/applications/${id}`);
    return response.data;
  },

  async create(data: Omit<Application, 'id' | 'createdAt' | 'updatedAt'>): Promise<Application> {
    const response = await axios.post(`${API_BASE_URL}/applications`, data);
    return response.data;
  },

  async update(id: string, data: Partial<Application>): Promise<Application> {
    const response = await axios.patch(`${API_BASE_URL}/applications/${id}`, data);
    return response.data;
  },

  async delete(id: string, approvalId: string, reason: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/applications/${id}`, {
      params: { approval_id: approvalId, reason },
    });
  },

  async bulkDelete(ids: string[]): Promise<any> {
    return bulkAction('applications', 'delete', ids);
  },

  // CQRS command endpoints
  async setClassification(id: string, classification: string, reason: string): Promise<Application> {
    const response = await axios.post(
      `${API_BASE_URL}/applications/${id}/commands/set-classification`,
      { classification, reason }
    );
    return response.data;
  },

  async transitionLifecycle(id: string, targetLifecycle: string, sunsetDate?: string): Promise<Application> {
    const response = await axios.post(
      `${API_BASE_URL}/applications/${id}/commands/transition-lifecycle`,
      { target_lifecycle: targetLifecycle, sunset_date: sunsetDate }
    );
    return response.data;
  },

  async setOwner(id: string, owner: string, reason?: string): Promise<Application> {
    const response = await axios.post(
      `${API_BASE_URL}/applications/${id}/commands/set-owner`,
      { owner, reason }
    );
    return response.data;
  },
};

/**
 * Servers API
 * Reference: /servers paths in openapi.yaml
 */
export const serversApi = {
  async list(params: any = {}): Promise<ListResponse<Server>> {
    const query = buildQueryString(params);
    const url = `${API_BASE_URL}/servers${query ? '?' + query : ''}`;
    const response = await axios.get(url);
    return response.data;
  },

  async getById(id: string): Promise<Server> {
    const response = await axios.get(`${API_BASE_URL}/servers/${id}`);
    return response.data;
  },

  async create(data: Omit<Server, 'id' | 'createdAt' | 'updatedAt'>): Promise<Server> {
    const response = await axios.post(`${API_BASE_URL}/servers`, data);
    return response.data;
  },

  async update(id: string, data: Partial<Server>): Promise<Server> {
    const response = await axios.patch(`${API_BASE_URL}/servers/${id}`, data);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/servers/${id}`);
  },

  async bulkDelete(ids: string[]): Promise<any> {
    return bulkAction('servers', 'delete', ids);
  },
};

/**
 * Integrations API
 * Reference: /integrations paths in openapi.yaml
 */
export const integrationsApi = {
  async list(params: any = {}): Promise<ListResponse<Integration>> {
    const query = buildQueryString(params);
    const url = `${API_BASE_URL}/integrations${query ? '?' + query : ''}`;
    const response = await axios.get(url);
    return response.data;
  },

  async getById(id: string): Promise<Integration> {
    const response = await axios.get(`${API_BASE_URL}/integrations/${id}`);
    return response.data;
  },

  async create(data: Omit<Integration, 'id' | 'createdAt' | 'updatedAt'>): Promise<Integration> {
    const response = await axios.post(`${API_BASE_URL}/integrations`, data);
    return response.data;
  },

  async update(id: string, data: Partial<Integration>): Promise<Integration> {
    const response = await axios.patch(`${API_BASE_URL}/integrations/${id}`, data);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/integrations/${id}`);
  },

  async bulkDelete(ids: string[]): Promise<any> {
    return bulkAction('integrations', 'delete', ids);
  },
};

/**
 * Data Entities API
 * Reference: /data-entities paths in openapi.yaml
 */
export const dataEntitiesApi = {
  async list(params: any = {}): Promise<ListResponse<DataEntity>> {
    const query = buildQueryString(params);
    const url = `${API_BASE_URL}/data-entities${query ? '?' + query : ''}`;
    const response = await axios.get(url);
    return response.data;
  },

  async getById(id: string): Promise<DataEntity> {
    const response = await axios.get(`${API_BASE_URL}/data-entities/${id}`);
    return response.data;
  },

  async create(data: Omit<DataEntity, 'id' | 'createdAt' | 'updatedAt'>): Promise<DataEntity> {
    const response = await axios.post(`${API_BASE_URL}/data-entities`, data);
    return response.data;
  },

  async update(id: string, data: Partial<DataEntity>): Promise<DataEntity> {
    const response = await axios.patch(`${API_BASE_URL}/data-entities/${id}`, data);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/data-entities/${id}`);
  },

  async bulkDelete(ids: string[]): Promise<any> {
    return bulkAction('data-entities', 'delete', ids);
  },
};

/**
 * Business Capabilities API
 * Reference: /business-capabilities paths in openapi.yaml
 */
export const businessCapabilitiesApi = {
  async list(params: any = {}): Promise<ListResponse<BusinessCapability>> {
    const query = buildQueryString(params);
    const url = `${API_BASE_URL}/business-capabilities${query ? '?' + query : ''}`;
    const response = await axios.get(url);
    return response.data;
  },

  async getById(id: string): Promise<BusinessCapability> {
    const response = await axios.get(`${API_BASE_URL}/business-capabilities/${id}`);
    return response.data;
  },

  async create(data: Omit<BusinessCapability, 'id' | 'createdAt' | 'updatedAt'>): Promise<BusinessCapability> {
    const response = await axios.post(`${API_BASE_URL}/business-capabilities`, data);
    return response.data;
  },

  async update(id: string, data: Partial<BusinessCapability>): Promise<BusinessCapability> {
    const response = await axios.patch(`${API_BASE_URL}/business-capabilities/${id}`, data);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/business-capabilities/${id}`);
  },

  async bulkDelete(ids: string[]): Promise<any> {
    return bulkAction('business-capabilities', 'delete', ids);
  },
};

/**
 * Organizations API
 * Reference: /organizations paths in openapi.yaml
 */
export const organizationsApi = {
  async list(params: any = {}): Promise<ListResponse<Organization>> {
    const query = buildQueryString(params);
    const url = `${API_BASE_URL}/organizations${query ? '?' + query : ''}`;
    const response = await axios.get(url);
    return response.data;
  },

  async getById(id: string): Promise<Organization> {
    const response = await axios.get(`${API_BASE_URL}/organizations/${id}`);
    return response.data;
  },

  async create(data: Omit<Organization, 'id' | 'createdAt' | 'updatedAt'>): Promise<Organization> {
    const response = await axios.post(`${API_BASE_URL}/organizations`, data);
    return response.data;
  },

  async update(id: string, data: Partial<Organization>): Promise<Organization> {
    const response = await axios.patch(`${API_BASE_URL}/organizations/${id}`, data);
    return response.data;
  },

  async delete(id: string, reason?: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/organizations/${id}`, {
      params: { reason },
    });
  },

  async bulkDelete(ids: string[]): Promise<any> {
    return bulkAction('organizations', 'delete', ids);
  },

  // CQRS command endpoints
  async setParent(id: string, parentId: string): Promise<Organization> {
    const response = await axios.post(
      `${API_BASE_URL}/organizations/${id}/commands/set-parent`,
      { parent_id: parentId }
    );
    return response.data;
  },

  async removeParent(id: string): Promise<Organization> {
    const response = await axios.post(
      `${API_BASE_URL}/organizations/${id}/commands/remove-parent`,
      {}
    );
    return response.data;
  },
};

/**
 * Relations API
 * Reference: /relations paths in openapi.yaml
 */
export const relationsApi = {
  async list(params: any = {}): Promise<ListResponse<Relation>> {
    const query = buildQueryString(params);
    const url = `${API_BASE_URL}/relations${query ? '?' + query : ''}`;
    const response = await axios.get(url);
    return response.data;
  },

  async getById(id: string): Promise<Relation> {
    const response = await axios.get(`${API_BASE_URL}/relations/${id}`);
    return response.data;
  },

  async create(data: Omit<Relation, 'id' | 'createdAt' | 'updatedAt'>): Promise<Relation> {
    const response = await axios.post(`${API_BASE_URL}/relations`, data);
    return response.data;
  },

  async update(id: string, data: Partial<Relation>): Promise<Relation> {
    const response = await axios.patch(`${API_BASE_URL}/relations/${id}`, data);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/relations/${id}`);
  },

  async bulkDelete(ids: string[]): Promise<any> {
    return bulkAction('relations', 'delete', ids);
  },
};

/**
 * Application Services API
 * Reference: /application-services paths in openapi.yaml
 */
export const applicationServicesApi = {
  async list(params: any = {}): Promise<ListResponse<ApplicationService>> {
    const query = buildQueryString(params);
    const url = `${API_BASE_URL}/application-services${query ? '?' + query : ''}`;
    const response = await axios.get(url);
    return response.data;
  },

  async getById(id: string): Promise<ApplicationService> {
    const response = await axios.get(`${API_BASE_URL}/application-services/${id}`);
    return response.data;
  },

  async create(data: Omit<ApplicationService, 'id' | 'createdAt' | 'updatedAt'>): Promise<ApplicationService> {
    const response = await axios.post(`${API_BASE_URL}/application-services`, data);
    return response.data;
  },

  async update(id: string, data: Partial<ApplicationService>): Promise<ApplicationService> {
    const response = await axios.patch(`${API_BASE_URL}/application-services/${id}`, data);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/application-services/${id}`);
  },

  async bulkDelete(ids: string[]): Promise<any> {
    return bulkAction('application-services', 'delete', ids);
  },
};

/**
 * Application Interfaces API
 * Reference: /application-interfaces paths in openapi.yaml
 */
export const applicationInterfacesApi = {
  async list(params: any = {}): Promise<ListResponse<ApplicationInterface>> {
    const query = buildQueryString(params);
    const url = `${API_BASE_URL}/application-interfaces${query ? '?' + query : ''}`;
    const response = await axios.get(url);
    return response.data;
  },

  async getById(id: string): Promise<ApplicationInterface> {
    const response = await axios.get(`${API_BASE_URL}/application-interfaces/${id}`);
    return response.data;
  },

  async create(data: Omit<ApplicationInterface, 'id' | 'createdAt' | 'updatedAt'>): Promise<ApplicationInterface> {
    const response = await axios.post(`${API_BASE_URL}/application-interfaces`, data);
    return response.data;
  },

  async update(id: string, data: Partial<ApplicationInterface>): Promise<ApplicationInterface> {
    const response = await axios.patch(`${API_BASE_URL}/application-interfaces/${id}`, data);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/application-interfaces/${id}`);
  },

  async bulkDelete(ids: string[]): Promise<any> {
    return bulkAction('application-interfaces', 'delete', ids);
  },
};

/**
 * Get API client for entity type
 * Factory function to retrieve the correct API client
 */
export const getApiClient = (entityType: EntityType): any => {
  switch (entityType) {
    case EntityType.Application:
      return applicationsApi;
    case EntityType.Server:
      return serversApi;
    case EntityType.Integration:
      return integrationsApi;
    case EntityType.DataEntity:
      return dataEntitiesApi;
    case EntityType.BusinessCapability:
      return businessCapabilitiesApi;
    case EntityType.Organization:
      return organizationsApi;
    case EntityType.Relation:
      return relationsApi;
    case EntityType.ApplicationService:
      return applicationServicesApi;
    case EntityType.ApplicationInterface:
      return applicationInterfacesApi;
    default:
      throw new Error(`Unknown entity type: ${entityType}`);
  }
};
    pagination?: PaginationParams,
    sort?: SortParams,
    filters?: FilterParams,
    search?: string
  ): Promise<ListResponse<Integration>> {
    const query = buildQueryString(pagination, sort, filters, search);
    const url = `${API_BASE_URL}/integrations${query ? '?' + query : ''}`;
    const response = await axios.get(url);
    return response.data;
  },

  async getById(id: string): Promise<Integration> {
    const response = await axios.get(`${API_BASE_URL}/integrations/${id}`);
    return response.data;
  },

  async create(data: Omit<Integration, 'id' | 'createdAt' | 'updatedAt'>): Promise<Integration> {
    const response = await axios.post(`${API_BASE_URL}/integrations`, data);
    return response.data;
  },

  async update(id: string, data: Partial<Integration>): Promise<Integration> {
    const response = await axios.patch(`${API_BASE_URL}/integrations/${id}`, data);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/integrations/${id}`);
  },

  async bulkDelete(ids: string[]): Promise<void> {
    await axios.post(`${API_BASE_URL}/integrations/bulk-delete`, { ids });
  },
};

/**
 * Data Entities API
 */
export const dataEntitiesApi = {
  async list(
    pagination?: PaginationParams,
    sort?: SortParams,
    filters?: FilterParams,
    search?: string
  ): Promise<ListResponse<DataEntity>> {
    const query = buildQueryString(pagination, sort, filters, search);
    const url = `${API_BASE_URL}/data-entities${query ? '?' + query : ''}`;
    const response = await axios.get(url);
    return response.data;
  },

  async getById(id: string): Promise<DataEntity> {
    const response = await axios.get(`${API_BASE_URL}/data-entities/${id}`);
    return response.data;
  },

  async create(data: Omit<DataEntity, 'id' | 'createdAt' | 'updatedAt'>): Promise<DataEntity> {
    const response = await axios.post(`${API_BASE_URL}/data-entities`, data);
    return response.data;
  },

  async update(id: string, data: Partial<DataEntity>): Promise<DataEntity> {
    const response = await axios.patch(`${API_BASE_URL}/data-entities/${id}`, data);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/data-entities/${id}`);
  },

  async bulkDelete(ids: string[]): Promise<void> {
    await axios.post(`${API_BASE_URL}/data-entities/bulk-delete`, { ids });
  },
};

/**
 * Business Capabilities API
 */
export const businessCapabilitiesApi = {
  async list(
    pagination?: PaginationParams,
    sort?: SortParams,
    filters?: FilterParams,
    search?: string
  ): Promise<ListResponse<BusinessCapability>> {
    const query = buildQueryString(pagination, sort, filters, search);
    const url = `${API_BASE_URL}/business-capabilities${query ? '?' + query : ''}`;
    const response = await axios.get(url);
    return response.data;
  },

  async getById(id: string): Promise<BusinessCapability> {
    const response = await axios.get(`${API_BASE_URL}/business-capabilities/${id}`);
    return response.data;
  },

  async create(
    data: Omit<BusinessCapability, 'id' | 'createdAt' | 'updatedAt'>
  ): Promise<BusinessCapability> {
    const response = await axios.post(`${API_BASE_URL}/business-capabilities`, data);
    return response.data;
  },

  async update(
    id: string,
    data: Partial<BusinessCapability>
  ): Promise<BusinessCapability> {
    const response = await axios.patch(`${API_BASE_URL}/business-capabilities/${id}`, data);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/business-capabilities/${id}`);
  },

  async bulkDelete(ids: string[]): Promise<void> {
    await axios.post(`${API_BASE_URL}/business-capabilities/bulk-delete`, { ids });
  },
};

/**
 * Organizations API
 */
export const organizationsApi = {
  async list(
    pagination?: PaginationParams,
    sort?: SortParams,
    filters?: FilterParams,
    search?: string
  ): Promise<ListResponse<Organization>> {
    const query = buildQueryString(pagination, sort, filters, search);
    const url = `${API_BASE_URL}/organizations${query ? '?' + query : ''}`;
    const response = await axios.get(url);
    return response.data;
  },

  async getById(id: string): Promise<Organization> {
    const response = await axios.get(`${API_BASE_URL}/organizations/${id}`);
    return response.data;
  },

  async create(data: Omit<Organization, 'id' | 'createdAt' | 'updatedAt'>): Promise<Organization> {
    const response = await axios.post(`${API_BASE_URL}/organizations`, data);
    return response.data;
  },

  async update(id: string, data: Partial<Organization>): Promise<Organization> {
    const response = await axios.patch(`${API_BASE_URL}/organizations/${id}`, data);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/organizations/${id}`);
  },

  async bulkDelete(ids: string[]): Promise<void> {
    await axios.post(`${API_BASE_URL}/organizations/bulk-delete`, { ids });
  },
};

/**
 * Relations API
 */
export const relationsApi = {
  async list(
    pagination?: PaginationParams,
    sort?: SortParams,
    filters?: FilterParams,
    search?: string
  ): Promise<ListResponse<Relation>> {
    const query = buildQueryString(pagination, sort, filters, search);
    const url = `${API_BASE_URL}/relations${query ? '?' + query : ''}`;
    const response = await axios.get(url);
    return response.data;
  },

  async getById(id: string): Promise<Relation> {
    const response = await axios.get(`${API_BASE_URL}/relations/${id}`);
    return response.data;
  },

  async create(data: Omit<Relation, 'id' | 'createdAt' | 'updatedAt'>): Promise<Relation> {
    const response = await axios.post(`${API_BASE_URL}/relations`, data);
    return response.data;
  },

  async update(id: string, data: Partial<Relation>): Promise<Relation> {
    const response = await axios.patch(`${API_BASE_URL}/relations/${id}`, data);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/relations/${id}`);
  },

  async bulkDelete(ids: string[]): Promise<void> {
    await axios.post(`${API_BASE_URL}/relations/bulk-delete`, { ids });
  },
};

/**
 * Application Services API
 */
export const applicationServicesApi = {
  async list(
    pagination?: PaginationParams,
    sort?: SortParams,
    filters?: FilterParams,
    search?: string
  ): Promise<ListResponse<ApplicationService>> {
    const query = buildQueryString(pagination, sort, filters, search);
    const url = `${API_BASE_URL}/application-services${query ? '?' + query : ''}`;
    const response = await axios.get(url);
    return response.data;
  },

  async getById(id: string): Promise<ApplicationService> {
    const response = await axios.get(`${API_BASE_URL}/application-services/${id}`);
    return response.data;
  },

  async create(
    data: Omit<ApplicationService, 'id' | 'createdAt' | 'updatedAt'>
  ): Promise<ApplicationService> {
    const response = await axios.post(`${API_BASE_URL}/application-services`, data);
    return response.data;
  },

  async update(id: string, data: Partial<ApplicationService>): Promise<ApplicationService> {
    const response = await axios.patch(`${API_BASE_URL}/application-services/${id}`, data);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/application-services/${id}`);
  },

  async bulkDelete(ids: string[]): Promise<void> {
    await axios.post(`${API_BASE_URL}/application-services/bulk-delete`, { ids });
  },
};

/**
 * Application Interfaces API
 */
export const applicationInterfacesApi = {
  async list(
    pagination?: PaginationParams,
    sort?: SortParams,
    filters?: FilterParams,
    search?: string
  ): Promise<ListResponse<ApplicationInterface>> {
    const query = buildQueryString(pagination, sort, filters, search);
    const url = `${API_BASE_URL}/application-interfaces${query ? '?' + query : ''}`;
    const response = await axios.get(url);
    return response.data;
  },

  async getById(id: string): Promise<ApplicationInterface> {
    const response = await axios.get(`${API_BASE_URL}/application-interfaces/${id}`);
    return response.data;
  },

  async create(
    data: Omit<ApplicationInterface, 'id' | 'createdAt' | 'updatedAt'>
  ): Promise<ApplicationInterface> {
    const response = await axios.post(`${API_BASE_URL}/application-interfaces`, data);
    return response.data;
  },

  async update(id: string, data: Partial<ApplicationInterface>): Promise<ApplicationInterface> {
    const response = await axios.patch(`${API_BASE_URL}/application-interfaces/${id}`, data);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/application-interfaces/${id}`);
  },

  async bulkDelete(ids: string[]): Promise<void> {
    await axios.post(`${API_BASE_URL}/application-interfaces/bulk-delete`, { ids });
  },
};

/**
 * Get API client for entity type
 */
export const getApiClient = (entityType: EntityType) => {
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
