/**
 * Global TypeScript type definitions for EATool Frontend
 */

// API Response Types
export interface ApiResponse<T> {
  data: T
  status: number
  message?: string
}

export interface ApiError {
  code: string
  message: string
  details?: Record<string, unknown>
}

// Entity Types
export enum LifecycleState {
  Planned = 'Planned',
  Active = 'Active',
  Deprecated = 'Deprecated',
  Sunset = 'Sunset',
}

export interface Entity {
  id: string
  name: string
  description?: string
  status: LifecycleState
  createdAt: string
  modifiedAt: string
  createdBy?: string
  modifiedBy?: string
}

// Domain Model Types
export interface DomainEntity extends Entity {
  businessElements: string[]
  relationships: string[]
}

export interface ServerEntity extends Entity {
  environment?: string
  ipAddress?: string
  port?: number
}

export interface IntegrationEntity extends Entity {
  integrationType: string
  endpoint?: string
}

export interface DataEntityRecord extends Entity {
  dataType: string
  source?: string
}

export interface ApplicationEntity extends Entity {
  owner?: string
  components?: string[]
}

// User/Auth Types
export interface User {
  id: string
  username: string
  email: string
  roles: string[]
}

export interface AuthToken {
  accessToken: string
  refreshToken?: string
  expiresIn: number
}

// Query/Filter Types
export interface QueryParams {
  page?: number
  pageSize?: number
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
  search?: string
  filters?: Record<string, unknown>
}

export interface PaginatedResponse<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
  hasMore: boolean
}
