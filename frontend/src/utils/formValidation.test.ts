/**
 * formValidation tests
 */
import { describe, it, expect } from 'vitest';
import {
  NameFieldSchema,
  URLFieldSchema,
  ServerFormSchema,
  ApplicationFormSchema,
  RelationFormSchema,
  getValidationErrors,
  mapServerErrorsToFields,
} from './formValidation';
import { z } from 'zod';

describe('formValidation - Field Schemas', () => {
  it('NameFieldSchema: enforces min and max', () => {
    expect(() => NameFieldSchema.parse('A')).toThrow();
    expect(() => NameFieldSchema.parse('A'.repeat(101))).toThrow();
    expect(NameFieldSchema.parse('Valid Name')).toBe('Valid Name');
  });

  it('URLFieldSchema: allows valid URLs and empty string', () => {
    expect(URLFieldSchema.parse('')).toBe('');
    expect(URLFieldSchema.parse('https://example.com')).toBe('https://example.com');
    expect(() => URLFieldSchema.parse('not-a-url')).toThrow();
  });

  it('ServerFormSchema: validates ipAddress IPv4/IPv6 patterns', () => {
    // Valid IPv4
    expect(
      ServerFormSchema.shape.ipAddress.safeParse('192.168.1.10').success
    ).toBe(true);
    // Valid IPv6 (simple form)
    expect(ServerFormSchema.shape.ipAddress.safeParse('fe80::1').success).toBe(true);
    // Invalid format (too few octets / trailing dot)
    expect(ServerFormSchema.shape.ipAddress.safeParse('192.168.1.').success).toBe(false);
  });
});

describe('formValidation - Form Schemas', () => {
  it('ApplicationFormSchema: owner must be UUID', () => {
    const invalid = {
      name: 'App',
      description: '',
      owner: 'not-a-uuid',
      lifecycle: 'active',
      classification: 'internal',
      classificationReason: '',
      sunsetDate: '',
      environment: 'Production',
      type: 'web',
      technologyStack: [],
      critical: false,
      url: '',
    };
    const result = ApplicationFormSchema.safeParse(invalid);
    expect(result.success).toBe(false);
  });

  it('RelationFormSchema: confidence must be within [0,1]', () => {
    const tooHigh = RelationFormSchema.safeParse({
      sourceEntity: '00000000-0000-0000-0000-000000000000',
      targetEntity: '00000000-0000-0000-0000-000000000001',
      type: 'depends-on',
      direction: 'Bidirectional',
      description: '',
      confidence: 2,
    });
    expect(tooHigh.success).toBe(false);
  });
});

describe('formValidation - Error Mapping', () => {
  it('getValidationErrors: maps Zod errors to field paths', () => {
    const Schema = z.object({
      name: z.string().min(2, 'Too short'),
      nested: z.object({ inner: z.string().min(1, 'Required') }),
    });
    const res = Schema.safeParse({ name: 'A', nested: { inner: '' } });
    expect(res.success).toBe(false);
    const errors = getValidationErrors(res.error as z.ZodError);
    expect(errors.name).toBe('Too short');
    expect(errors['nested.inner']).toBe('Required');
  });

  it('mapServerErrorsToFields: arrays map to fields, detail maps to _general', () => {
    const arrayErrs = {
      errors: [
        { field: 'name', message: 'Name required' },
        { field: 'owner', message: 'Invalid owner' },
      ],
    };
    expect(mapServerErrorsToFields(arrayErrs)).toEqual({
      name: 'Name required',
      owner: 'Invalid owner',
    });

    const detailErr = { detail: 'Something went wrong' };
    expect(mapServerErrorsToFields(detailErr)).toEqual({ _general: 'Something went wrong' });
  });
});
