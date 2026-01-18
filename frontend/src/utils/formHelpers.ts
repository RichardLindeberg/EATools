/**
 * Form helper utilities
 */

/**
 * Check if a form has unsaved changes
 */
export function hasUnsavedChanges(original: any, current: any): boolean {
  return JSON.stringify(original) !== JSON.stringify(current);
}

/**
 * Deep clone an object for form default values
 */
export function deepClone<T>(obj: T): T {
  return JSON.parse(JSON.stringify(obj));
}

/**
 * Convert form data keys from camelCase to snake_case for API
 */
export function camelToSnakeCase(obj: Record<string, any>): Record<string, any> {
  const result: Record<string, any> = {};

  for (const key in obj) {
    const snakeKey = key.replace(/([A-Z])/g, '_$1').toLowerCase();
    result[snakeKey] = obj[key];
  }

  return result;
}

/**
 * Convert API response keys from snake_case to camelCase for form
 */
export function snakeToCamelCase(obj: Record<string, any>): Record<string, any> {
  const result: Record<string, any> = {};

  for (const key in obj) {
    const camelKey = key.replace(/_([a-z])/g, (match, letter) => letter.toUpperCase());
    result[camelKey] = obj[key];
  }

  return result;
}

/**
 * Calculate field diff between original and current values
 * Returns only changed fields
 */
export function getDiff(original: Record<string, any>, current: Record<string, any>): Record<string, any> {
  const diff: Record<string, any> = {};

  for (const key in current) {
    if (JSON.stringify(original[key]) !== JSON.stringify(current[key])) {
      diff[key] = current[key];
    }
  }

  return diff;
}

/**
 * Format form error messages
 */
export function formatErrorMessage(error: any): string {
  if (typeof error === 'string') {
    return error;
  }

  if (error?.message) {
    return error.message;
  }

  if (error?.detail) {
    return error.detail;
  }

  return 'An error occurred';
}

/**
 * Check if error is a validation error (422)
 */
export function isValidationError(error: any): boolean {
  return error?.status === 422 || error?.response?.status === 422;
}

/**
 * Check if error is a permission error (403)
 */
export function isPermissionError(error: any): boolean {
  return error?.status === 403 || error?.response?.status === 403;
}

/**
 * Check if error is a conflict error (409)
 */
export function isConflictError(error: any): boolean {
  return error?.status === 409 || error?.response?.status === 409;
}

/**
 * Extract field errors from API response
 */
export function extractFieldErrors(error: any): Record<string, string> {
  const fieldErrors: Record<string, string> = {};

  if (error?.response?.data?.errors && Array.isArray(error.response.data.errors)) {
    error.response.data.errors.forEach((err: any) => {
      if (err.field) {
        fieldErrors[err.field] = err.message || 'Validation error';
      }
    });
  } else if (error?.response?.data?.detail) {
    fieldErrors['_general'] = error.response.data.detail;
  }

  return fieldErrors;
}

/**
 * Convert array of field selections to IDs
 */
export function extractIdArray(selections: any[]): string[] {
  return selections.map((s) => (typeof s === 'string' ? s : s.id));
}

/**
 * Format date for input type="date"
 */
export function formatDateForInput(date: Date | string | null): string {
  if (!date) return '';

  const d = typeof date === 'string' ? new Date(date) : date;
  const year = d.getFullYear();
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');

  return `${year}-${month}-${day}`;
}

/**
 * Parse date from input type="date"
 */
export function parseDateFromInput(dateString: string): Date {
  return new Date(`${dateString}T00:00:00Z`);
}
