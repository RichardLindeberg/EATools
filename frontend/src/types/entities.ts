/**
 * Entity Types and Interfaces
 * TypeScript definitions for all 9 entity types in EATool
 */

/**
 * Base entity interface with common properties
 */
export interface BaseEntity {
  id: string;
  createdAt: string;
  updatedAt: string;
  createdBy?: string;
  updatedBy?: string;
}

/**
 * Pagination request parameters
 */
export interface PaginationParams {
  skip?: number;
  take?: number;
}

/**
 * Sorting request parameters
 */
export interface SortParams {
  sort?: string;
  order?: 'asc' | 'desc';
}

/**
 * Filter request parameters
 */
export interface FilterParams {
  [key: string]: string | number | boolean | undefined;
}

/**
 * API list response with pagination
 */
export interface ListResponse<T> {
  items: T[];
  total: number;
  skip: number;
  take: number;
}

/**
 * Application entity
 */
export interface Application extends BaseEntity {
  name: string;
  description?: string;
  type: 'web' | 'mobile' | 'desktop' | 'backend' | 'other';
  status: 'active' | 'inactive' | 'retired' | 'planned';
  owner?: string;
  version?: string;
  technology?: string[];
}

/**
 * Server entity
 */
export interface Server extends BaseEntity {
  name: string;
  hostname: string;
  ipAddress: string;
  port?: number;
  environment: 'dev' | 'staging' | 'prod' | 'test';
  status: 'active' | 'inactive' | 'maintenance' | 'decommissioned';
  owner?: string;
  osType?: string;
  osVersion?: string;
}

/**
 * Integration entity
 */
export interface Integration extends BaseEntity {
  name: string;
  type: 'api' | 'message-queue' | 'database' | 'file-transfer' | 'other';
  protocol: string;
  sourceSystemId?: string;
  targetSystemId?: string;
  status: 'active' | 'inactive' | 'planned' | 'deprecated';
  frequency?: string;
  owner?: string;
}

/**
 * Data Entity
 */
export interface DataEntity extends BaseEntity {
  name: string;
  type: 'table' | 'view' | 'document' | 'file' | 'other';
  format: 'relational' | 'document' | 'graph' | 'file' | 'other';
  classification: 'public' | 'internal' | 'confidential' | 'restricted';
  owner?: string;
  schema?: Record<string, unknown>;
  volume?: number;
}

/**
 * Business Capability
 */
export interface BusinessCapability extends BaseEntity {
  name: string;
  description?: string;
  level: 'strategic' | 'core' | 'supporting' | 'management';
  parentId?: string;
  owner?: string;
  status: 'active' | 'inactive' | 'planned' | 'deprecated';
  processes?: string[];
}

/**
 * Organization
 */
export interface Organization extends BaseEntity {
  name: string;
  type: 'department' | 'team' | 'business-unit' | 'division' | 'other';
  parentId?: string;
  owner?: string;
  description?: string;
  members?: number;
}

/**
 * Relation between entities
 */
export interface Relation extends BaseEntity {
  type: 'implements' | 'supports' | 'uses' | 'communicates-with' | 'depends-on' | 'other';
  sourceEntityType: string;
  sourceEntityId: string;
  targetEntityType: string;
  targetEntityId: string;
  strength?: 'strong' | 'medium' | 'weak';
  description?: string;
}

/**
 * Application Service
 */
export interface ApplicationService extends BaseEntity {
  name: string;
  applicationId: string;
  protocol: 'rest' | 'soap' | 'graphql' | 'grpc' | 'other';
  baseUrl?: string;
  status: 'active' | 'inactive' | 'deprecated' | 'planned';
  owner?: string;
  documentation?: string;
}

/**
 * Application Interface
 */
export interface ApplicationInterface extends BaseEntity {
  name: string;
  type: 'api' | 'ui' | 'event' | 'batch' | 'other';
  protocol: 'rest' | 'soap' | 'graphql' | 'grpc' | 'event' | 'other';
  sourceApplicationId: string;
  targetApplicationId: string;
  status: 'active' | 'inactive' | 'deprecated' | 'planned';
  owner?: string;
}

/**
 * Union type for all entity types
 */
export type Entity =
  | Application
  | Server
  | Integration
  | DataEntity
  | BusinessCapability
  | Organization
  | Relation
  | ApplicationService
  | ApplicationInterface;

/**
 * Entity type names
 */
export enum EntityType {
  APPLICATION = 'applications',
  SERVER = 'servers',
  INTEGRATION = 'integrations',
  DATA_ENTITY = 'data-entities',
  BUSINESS_CAPABILITY = 'business-capabilities',
  ORGANIZATION = 'organizations',
  RELATION = 'relations',
  APPLICATION_SERVICE = 'application-services',
  APPLICATION_INTERFACE = 'application-interfaces',
}

/**
 * Entity type display names
 */
export const ENTITY_TYPE_LABELS: Record<EntityType, string> = {
  [EntityType.APPLICATION]: 'Applications',
  [EntityType.SERVER]: 'Servers',
  [EntityType.INTEGRATION]: 'Integrations',
  [EntityType.DATA_ENTITY]: 'Data Entities',
  [EntityType.BUSINESS_CAPABILITY]: 'Business Capabilities',
  [EntityType.ORGANIZATION]: 'Organizations',
  [EntityType.RELATION]: 'Relations',
  [EntityType.APPLICATION_SERVICE]: 'Application Services',
  [EntityType.APPLICATION_INTERFACE]: 'Application Interfaces',
};
