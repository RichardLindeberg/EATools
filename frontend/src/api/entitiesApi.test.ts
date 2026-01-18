/**
 * Entities API Client Tests
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { buildQueryString, getApiClient } from './entitiesApi';
import { EntityType } from '../types/entities';

describe('buildQueryString', () => {
  it('builds query string with pagination params', () => {
    const query = buildQueryString({
      skip: 0,
      take: 10,
    });

    // skip/take are converted to page/limit
    expect(query).toContain('page=1');
    expect(query).toContain('limit=10');
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

    // skip 20 with take 25 = page 1 (0-24), but since skip=20, page should be calculated
    // skip=20, take=25: page = floor(20/25) + 1 = 0 + 1 = 1
    expect(query).toContain('page=1');
    expect(query).toContain('limit=25');
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
  it('returns applicationsApi for EntityType.APPLICATION', () => {
    const client = getApiClient(EntityType.APPLICATION);
    expect(client).toBeDefined();
    expect(client.list).toBeDefined();
  });

  it('returns serversApi for EntityType.SERVER', () => {
    const client = getApiClient(EntityType.SERVER);
    expect(client).toBeDefined();
    expect(client.list).toBeDefined();
  });

  it('returns integrationsApi for EntityType.INTEGRATION', () => {
    const client = getApiClient(EntityType.INTEGRATION);
    expect(client).toBeDefined();
    expect(client.list).toBeDefined();
  });

  it('returns dataEntitiesApi for EntityType.DATA_ENTITY', () => {
    const client = getApiClient(EntityType.DATA_ENTITY);
    expect(client).toBeDefined();
    expect(client.list).toBeDefined();
  });

  it('returns businessCapabilitiesApi for EntityType.BUSINESS_CAPABILITY', () => {
    const client = getApiClient(EntityType.BUSINESS_CAPABILITY);
    expect(client).toBeDefined();
    expect(client.list).toBeDefined();
  });

  it('returns organizationsApi for EntityType.ORGANIZATION', () => {
    const client = getApiClient(EntityType.ORGANIZATION);
    expect(client).toBeDefined();
    expect(client.list).toBeDefined();
  });

  it('returns relationsApi for EntityType.RELATION', () => {
    const client = getApiClient(EntityType.RELATION);
    expect(client).toBeDefined();
    expect(client.list).toBeDefined();
  });

  it('returns applicationServicesApi for EntityType.APPLICATION_SERVICE', () => {
    const client = getApiClient(EntityType.APPLICATION_SERVICE);
    expect(client).toBeDefined();
    expect(client.list).toBeDefined();
  });

  it('returns applicationInterfacesApi for EntityType.APPLICATION_INTERFACE', () => {
    const client = getApiClient(EntityType.APPLICATION_INTERFACE);
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
