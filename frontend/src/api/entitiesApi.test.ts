/**
 * Entities API Client Tests
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { buildQueryString, getApiClient } from '../../api/entitiesApi';
import { EntityType } from '../../types/entities';

describe('buildQueryString', () => {
  it('builds query string with pagination params', () => {
    const query = buildQueryString({
      skip: 0,
      take: 10,
    });

    expect(query).toContain('skip=0');
    expect(query).toContain('take=10');
  });

  it('builds query string with sort params', () => {
    const query = buildQueryString({
      sort: 'name',
      order: 'asc',
    });

    expect(query).toContain('sort=name');
    expect(query).toContain('order=asc');
  });

  it('builds query string with search param', () => {
    const query = buildQueryString({
      search: 'test',
    });

    expect(query).toContain('search=test');
  });

  it('builds query string with filter params', () => {
    const query = buildQueryString({
      type: 'active',
      status: 'online',
    });

    expect(query).toContain('type=active');
    expect(query).toContain('status=online');
  });

  it('encodes special characters', () => {
    const query = buildQueryString({
      search: 'test&value',
    });

    expect(query).toContain('search=test%26value');
  });

  it('handles multiple filter values', () => {
    const query = buildQueryString({
      skip: 20,
      take: 25,
      sort: 'id',
      order: 'desc',
      search: 'query',
      type: 'test',
    });

    expect(query).toContain('skip=20');
    expect(query).toContain('take=25');
    expect(query).toContain('sort=id');
    expect(query).toContain('order=desc');
    expect(query).toContain('search=query');
    expect(query).toContain('type=test');
  });

  it('returns empty string when no params provided', () => {
    const query = buildQueryString({});
    expect(query).toBe('');
  });
});

describe('getApiClient', () => {
  it('returns applicationsApi for EntityType.Application', () => {
    const client = getApiClient(EntityType.Application);
    expect(client).toBeDefined();
    expect(client.list).toBeDefined();
  });

  it('returns serversApi for EntityType.Server', () => {
    const client = getApiClient(EntityType.Server);
    expect(client).toBeDefined();
    expect(client.list).toBeDefined();
  });

  it('returns integrationsApi for EntityType.Integration', () => {
    const client = getApiClient(EntityType.Integration);
    expect(client).toBeDefined();
    expect(client.list).toBeDefined();
  });

  it('returns dataEntitiesApi for EntityType.DataEntity', () => {
    const client = getApiClient(EntityType.DataEntity);
    expect(client).toBeDefined();
    expect(client.list).toBeDefined();
  });

  it('returns businessCapabilitiesApi for EntityType.BusinessCapability', () => {
    const client = getApiClient(EntityType.BusinessCapability);
    expect(client).toBeDefined();
    expect(client.list).toBeDefined();
  });

  it('returns organizationsApi for EntityType.Organization', () => {
    const client = getApiClient(EntityType.Organization);
    expect(client).toBeDefined();
    expect(client.list).toBeDefined();
  });

  it('returns relationsApi for EntityType.Relation', () => {
    const client = getApiClient(EntityType.Relation);
    expect(client).toBeDefined();
    expect(client.list).toBeDefined();
  });

  it('returns applicationServicesApi for EntityType.ApplicationService', () => {
    const client = getApiClient(EntityType.ApplicationService);
    expect(client).toBeDefined();
    expect(client.list).toBeDefined();
  });

  it('returns applicationInterfacesApi for EntityType.ApplicationInterface', () => {
    const client = getApiClient(EntityType.ApplicationInterface);
    expect(client).toBeDefined();
    expect(client.list).toBeDefined();
  });

  it('all API clients have required methods', () => {
    Object.values(EntityType).forEach((entityType) => {
      const client = getApiClient(entityType as EntityType);
      expect(client.list).toBeDefined();
      expect(client.getById).toBeDefined();
      expect(client.create).toBeDefined();
      expect(client.update).toBeDefined();
      expect(client.delete).toBeDefined();
      expect(client.bulkDelete).toBeDefined();
    });
  });
});
