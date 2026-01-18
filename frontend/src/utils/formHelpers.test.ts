/**
 * formHelpers tests
 */
import { describe, it, expect } from 'vitest';
import {
  hasUnsavedChanges,
  deepClone,
  camelToSnakeCase,
  snakeToCamelCase,
  getDiff,
  formatErrorMessage,
  isValidationError,
  isPermissionError,
  isConflictError,
  extractFieldErrors,
  extractIdArray,
  formatDateForInput,
  parseDateFromInput,
} from './formHelpers';

describe('formHelpers - change detection and cloning', () => {
  it('hasUnsavedChanges: detects changes', () => {
    const original = { a: 1, b: { c: 2 } };
    const currentSame = { a: 1, b: { c: 2 } };
    const currentChanged = { a: 1, b: { c: 3 } };
    expect(hasUnsavedChanges(original, currentSame)).toBe(false);
    expect(hasUnsavedChanges(original, currentChanged)).toBe(true);
  });

  it('deepClone: returns equal but not same reference', () => {
    const obj = { a: 1, b: { c: 2 } };
    const clone = deepClone(obj);
    expect(clone).toEqual(obj);
    (clone as any).b.c = 5;
    expect(obj.b.c).toBe(2);
  });
});

describe('formHelpers - key conversions', () => {
  it('camelToSnakeCase: converts keys', () => {
    const input = { ownerId: '123', createdAt: '2026-01-18' };
    expect(camelToSnakeCase(input)).toEqual({ owner_id: '123', created_at: '2026-01-18' });
  });

  it('snakeToCamelCase: converts keys', () => {
    const input = { owner_id: '123', created_at: '2026-01-18' };
    expect(snakeToCamelCase(input)).toEqual({ ownerId: '123', createdAt: '2026-01-18' });
  });
});

describe('formHelpers - diffs and errors', () => {
  it('getDiff: returns only changed fields', () => {
    const original = { name: 'A', tags: ['x'], count: 1 };
    const current = { name: 'B', tags: ['x'], count: 1 };
    expect(getDiff(original, current)).toEqual({ name: 'B' });
  });

  it('formatErrorMessage: formats common error shapes', () => {
    expect(formatErrorMessage('Oops')).toBe('Oops');
    expect(formatErrorMessage({ message: 'Bad' })).toBe('Bad');
    expect(formatErrorMessage({ detail: 'Bad detail' })).toBe('Bad detail');
    expect(formatErrorMessage({})).toBe('An error occurred');
  });

  it('error type guards: detects 422, 403, 409', () => {
    expect(isValidationError({ status: 422 })).toBe(true);
    expect(isPermissionError({ status: 403 })).toBe(true);
    expect(isConflictError({ status: 409 })).toBe(true);
    expect(isValidationError({ response: { status: 422 } })).toBe(true);
    expect(isPermissionError({ response: { status: 403 } })).toBe(true);
    expect(isConflictError({ response: { status: 409 } })).toBe(true);
  });

  it('extractFieldErrors: maps array or detail to errors', () => {
    const arrayErr = {
      response: {
        data: {
          errors: [
            { field: 'name', message: 'Name required' },
            { field: 'owner', message: 'Invalid owner' },
          ],
        },
      },
    };
    expect(extractFieldErrors(arrayErr)).toEqual({
      name: 'Name required',
      owner: 'Invalid owner',
    });

    const detailErr = { response: { data: { detail: 'General message' } } };
    expect(extractFieldErrors(detailErr)).toEqual({ _general: 'General message' });
  });
});

describe('formHelpers - selections and dates', () => {
  it('extractIdArray: handles strings and objects', () => {
    expect(extractIdArray(['a', { id: 'b' }, { id: 'c' }])).toEqual(['a', 'b', 'c']);
  });

  it('formatDateForInput: formats date to YYYY-MM-DD', () => {
    const date = new Date('2026-01-18T12:00:00Z');
    expect(formatDateForInput(date)).toBe('2026-01-18');
  });

  it('parseDateFromInput: returns a Date at midnight UTC', () => {
    const d = parseDateFromInput('2026-01-18');
    expect(d.toISOString()).toBe('2026-01-18T00:00:00.000Z');
  });
});
