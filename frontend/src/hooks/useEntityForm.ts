import { useCallback, useState } from 'react';
import { useForm, UseFormProps, UseFormReturn } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { ZodSchema, ZodError } from 'zod';
import { useNavigate } from 'react-router-dom';
import { apiClient } from '../api/client';
import { extractFieldErrors, isValidationError, isPermissionError } from '../utils/formHelpers';

interface UseEntityFormOptions<T> extends UseFormProps<T> {
  schema: ZodSchema;
  entityType: string;
  entityId?: string;
  onSuccess?: (data: any) => void;
  onError?: (error: any) => void;
}

/**
 * Custom hook for entity form management
 * Handles both create and edit workflows with CQRS awareness
 */
export function useEntityForm<T extends Record<string, any>>(
  options: UseEntityFormOptions<T>
): UseFormReturn<T> & {
  isSubmitting: boolean;
  submitError: string | null;
  fieldErrors: Record<string, string>;
  handleSubmit: (data: T) => Promise<void>;
  isEdit: boolean;
} {
  const navigate = useNavigate();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});

  const isEdit = !!options.entityId;

  const form = useForm<T>({
    ...options,
    resolver: zodResolver(options.schema as any),
  });

  const handleSubmit = useCallback(
    async (data: T) => {
      setIsSubmitting(true);
      setSubmitError(null);
      setFieldErrors({});

      try {
        let response;

        if (isEdit) {
          // Edit workflow: POST to commands endpoint or PATCH
          // For now, use PATCH as default
          response = await apiClient.patch(`/${options.entityType}/${options.entityId}`, data);
        } else {
          // Create workflow: POST to collection endpoint
          response = await apiClient.post(`/${options.entityType}`, data);
        }

        if (options.onSuccess) {
          options.onSuccess(response.data);
        }

        // Navigate to detail page
        if (response.data?.id) {
          navigate(`/${options.entityType}/${response.data.id}`);
        }
      } catch (error: any) {
        const errorMessage =
          error?.response?.data?.detail ||
          error?.message ||
          'An error occurred while saving';

        if (isValidationError(error)) {
          const fieldErrs = extractFieldErrors(error);
          setFieldErrors(fieldErrs);

          // Set form errors from backend validation
          Object.entries(fieldErrs).forEach(([field, message]) => {
            if (field !== '_general') {
              form.setError(field as any, {
                type: 'server',
                message,
              });
            }
          });

          setSubmitError(fieldErrs['_general'] || 'Validation failed');
        } else if (isPermissionError(error)) {
          setSubmitError('You do not have permission to perform this action');
        } else {
          setSubmitError(errorMessage);
        }

        if (options.onError) {
          options.onError(error);
        }
      } finally {
        setIsSubmitting(false);
      }
    },
    [isEdit, options, navigate, form]
  );

  return {
    ...form,
    isSubmitting,
    submitError,
    fieldErrors,
    handleSubmit,
    isEdit,
  };
}

/**
 * Hook for detecting unsaved form changes
 */
export function useFormDirty(
  values: Record<string, any>,
  originalValues: Record<string, any>
): boolean {
  return JSON.stringify(values) !== JSON.stringify(originalValues);
}

/**
 * Hook for searching related entities (for relationship selectors)
 */
export function useRelationshipSearch(
  entityType: string,
  debounceMs: number = 300
) {
  const [searchTerm, setSearchTerm] = useState('');
  const [results, setResults] = useState<any[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const search = useCallback(
    async (term: string) => {
      if (!term) {
        setResults([]);
        return;
      }

      setIsLoading(true);
      setError(null);

      try {
        const response = await apiClient.get(`/${entityType}`, {
          params: {
            search: term,
            limit: 20,
          },
        });

        setResults(response.data.items || response.data);
      } catch (err: any) {
        setError('Failed to search entities');
      } finally {
        setIsLoading(false);
      }
    },
    [entityType]
  );

  // Debounce search
  const debouncedSearch = useCallback(
    (term: string) => {
      setSearchTerm(term);
      const timeout = setTimeout(() => {
        search(term);
      }, debounceMs);

      return () => clearTimeout(timeout);
    },
    [search, debounceMs]
  );

  return {
    searchTerm,
    results,
    isLoading,
    error,
    search: debouncedSearch,
    setSearchTerm,
  };
}
