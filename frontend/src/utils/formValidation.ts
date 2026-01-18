import { z } from 'zod';

/**
 * Common validation schemas used across all entity forms
 */

// Common field validations
export const NameFieldSchema = z
  .string()
  .min(2, 'Name must be at least 2 characters')
  .max(100, 'Name must be at most 100 characters');

export const DescriptionFieldSchema = z
  .string()
  .max(1000, 'Description must be at most 1000 characters')
  .optional()
  .or(z.literal(''));

export const URLFieldSchema = z
  .string()
  .url('Must be a valid URL')
  .optional()
  .or(z.literal(''));

export const PercentageFieldSchema = z
  .number()
  .min(0, 'Must be at least 0')
  .max(100, 'Must be at most 100');

export const SLAFieldSchema = z
  .number()
  .min(99.0, 'SLA must be at least 99.0%')
  .max(99.99, 'SLA must be at most 99.99%');

// Entity type schemas for create/edit forms

export const ApplicationFormSchema = z.object({
  name: NameFieldSchema,
  description: DescriptionFieldSchema,
  owner: z.string().uuid('Owner must be a valid user ID'),
  lifecycle: z.enum(['planned', 'active', 'deprecated', 'retired']),
  classification: z.enum(['public', 'internal', 'confidential', 'restricted']),
  classificationReason: z.string().optional().or(z.literal('')),
  sunsetDate: z.string().optional().or(z.literal('')),
  environment: z.enum(['Production', 'Staging', 'Development', 'Test']),
  type: z.string().min(1, 'Type is required'),
  technologyStack: z.array(z.string()).optional().default([]),
  department: z.string().optional(),
  businessOwner: z.string().optional(),
  critical: z.boolean().optional().default(false),
  url: URLFieldSchema,
});

export type ApplicationFormData = z.infer<typeof ApplicationFormSchema>;

export const ServerFormSchema = z.object({
  name: NameFieldSchema,
  host: z.string().min(1, 'Host is required'),
  ipAddress: z
    .string()
    .regex(/^(\d{1,3}\.){3}\d{1,3}$|^[a-f0-9:]+$/i, 'Must be a valid IPv4 or IPv6 address')
    .optional()
    .or(z.literal('')),
  environment: z.enum(['Production', 'Staging', 'Development', 'Test']),
  osType: z.enum(['Linux', 'Windows', 'macOS', 'Cloud']),
  osVersion: z.string().optional(),
  owner: z.string().uuid('Owner must be a valid user ID'),
  backupSchedule: z.enum(['Never', 'Daily', 'Weekly', 'Monthly']).optional().default('Never'),
  description: DescriptionFieldSchema,
  tags: z.array(z.string()).optional().default([]),
});

export type ServerFormData = z.infer<typeof ServerFormSchema>;

export const IntegrationFormSchema = z.object({
  name: NameFieldSchema,
  description: DescriptionFieldSchema,
  sourceSystem: z.string().min(1, 'Source system is required'),
  targetSystem: z.string().min(1, 'Target system is required'),
  protocol: z.enum(['REST', 'SOAP', 'GraphQL', 'Database', 'File', 'Message']),
  frequency: z.enum(['RealTime', 'Hourly', 'Daily', 'Weekly', 'Monthly']),
  sla: SLAFieldSchema,
  direction: z.enum(['Unidirectional', 'Bidirectional']),
  owner: z.string().uuid('Owner must be a valid user ID'),
  dataClassification: z.enum(['Public', 'Internal', 'Confidential']),
  errorThreshold: z.number().min(0).max(100).optional(),
  retryPolicy: z.string().optional(),
  tags: z.array(z.string()).optional().default([]),
});

export type IntegrationFormData = z.infer<typeof IntegrationFormSchema>;

export const DataEntityFormSchema = z.object({
  name: NameFieldSchema,
  description: DescriptionFieldSchema,
  owner: z.string().min(1, 'Owner is required'),
  classification: z.enum(['Public', 'Internal', 'Confidential', 'Restricted']),
  sensitivityLevel: z.enum(['Low', 'Medium', 'High']).optional(),
  hasPii: z.boolean().optional().default(false),
  encrypted: z.boolean().optional().default(false),
  retentionPeriod: z.enum(['1y', '3y', '5y', '7y', 'Indefinite']),
  retentionUnit: z.enum(['Years', 'Months', 'Days']).optional().default('Years'),
  systemOfRecord: z.string().min(1, 'System of record is required'),
  relatedSystems: z.array(z.string()).optional().default([]),
  backupRequired: z.boolean().optional().default(false),
  backupFrequency: z.string().optional(),
  complianceRules: z.array(z.string()).optional().default([]),
  tags: z.array(z.string()).optional().default([]),
});

export type DataEntityFormData = z.infer<typeof DataEntityFormSchema>;

export const BusinessCapabilityFormSchema = z.object({
  name: NameFieldSchema,
  description: DescriptionFieldSchema,
  owner: z.string().min(1, 'Owner is required'),
  status: z.enum(['Planned', 'Building', 'Active', 'Retiring']),
  strategicValue: z.enum(['Critical', 'High', 'Medium', 'Low']).optional(),
  architectureStyle: z.enum(['Microservice', 'Monolith', 'Hybrid']).optional(),
  currentState: z.string().optional(),
  targetState: z.string().optional(),
  performanceKpi: z.string().optional(),
  timeline: z.string().optional(),
  supportingApplications: z.array(z.string()).optional().default([]),
  parent: z.string().optional().nullable(),
});

export type BusinessCapabilityFormData = z.infer<typeof BusinessCapabilityFormSchema>;

export const OrganizationFormSchema = z.object({
  name: NameFieldSchema,
  description: DescriptionFieldSchema,
  owner: z.string().uuid('Owner must be a valid user ID'),
  status: z.enum(['Active', 'Inactive', 'Pending']),
  type: z.enum(['Department', 'Team', 'Division']).optional(),
  parent: z.string().optional().nullable(),
  tags: z.array(z.string()).optional().default([]),
});

export type OrganizationFormData = z.infer<typeof OrganizationFormSchema>;

export const ApplicationServiceFormSchema = z.object({
  name: NameFieldSchema,
  description: DescriptionFieldSchema,
  application: z.string().min(1, 'Application is required'),
  type: z.enum(['Synchronous', 'Asynchronous', 'Webhook']),
  status: z.enum(['Available', 'Unavailable', 'Deprecated']),
  owner: z.string().uuid('Owner must be a valid user ID'),
  serviceContract: z.string().optional(),
  sla: SLAFieldSchema.optional(),
  timeout: z.number().min(0).optional(),
  retryPolicy: z.string().optional(),
  businessCapabilityId: z.string().optional(),
  consumerAppId: z.string().optional(),
  tags: z.array(z.string()).optional().default([]),
});

export type ApplicationServiceFormData = z.infer<typeof ApplicationServiceFormSchema>;

export const ApplicationInterfaceFormSchema = z.object({
  name: NameFieldSchema,
  description: DescriptionFieldSchema,
  application: z.string().min(1, 'Application is required'),
  type: z.enum(['REST', 'SOAP', 'GraphQL', 'Message']),
  protocol: z.enum(['HTTP', 'HTTPS', 'AMQP', 'JMS', 'Kafka', 'WebSocket']),
  status: z.enum(['active', 'deprecated', 'retired']),
  owner: z.string().uuid('Owner must be a valid user ID'),
  baseUrl: URLFieldSchema,
  apiVersion: z.string().optional(),
  rateLimit: z.number().min(0).optional(),
  authenticationType: z.string().optional(),
  serviceIds: z.array(z.string()).optional().default([]),
  tags: z.array(z.string()).optional().default([]),
});

export type ApplicationInterfaceFormData = z.infer<typeof ApplicationInterfaceFormSchema>;

export const RelationFormSchema = z.object({
  sourceEntity: z.string().uuid('Source entity must be a valid entity ID'),
  targetEntity: z.string().uuid('Target entity must be a valid entity ID'),
  type: z.string().min(1, 'Relationship type is required'),
  direction: z.enum(['Unidirectional', 'Bidirectional']),
  properties: z.record(z.unknown()).optional(),
  description: DescriptionFieldSchema,
  strength: z.enum(['Required', 'Optional']).optional(),
  cardinality: z.enum(['1:1', '1:N', 'N:N']).optional(),
  confidence: z
    .number()
    .min(0, 'Confidence must be at least 0')
    .max(1, 'Confidence must be at most 1')
    .optional(),
  effectiveFrom: z.string().optional().or(z.literal('')),
  effectiveTo: z.string().optional().or(z.literal('')),
});

export type RelationFormData = z.infer<typeof RelationFormSchema>;

/**
 * Validation error mapping utility
 * Converts Zod validation errors into field-keyed object
 */
export function getValidationErrors(error: z.ZodError): Record<string, string> {
  const errors: Record<string, string> = {};
  error.issues.forEach((issue) => {
    const path = issue.path.join('.');
    if (path) {
      errors[path] = issue.message;
    }
  });
  return errors;
}

/**
 * Server-side error mapping
 * Maps backend validation errors (422) to form fields
 */
export function mapServerErrorsToFields(errorData: any): Record<string, string> {
  const fieldErrors: Record<string, string> = {};

  if (errorData.errors && Array.isArray(errorData.errors)) {
    errorData.errors.forEach((err: any) => {
      if (err.field) {
        fieldErrors[err.field] = err.message || 'Validation error';
      }
    });
  } else if (errorData.detail) {
    // Generic error message
    fieldErrors['_general'] = errorData.detail;
  }

  return fieldErrors;
}
