import { apiClient } from '../api/client';
import { getDiff } from './formHelpers';

/**
 * Dispatches command-based edits for CQRS-aware entities, with PATCH fallback for remaining fields.
 */
export async function updateApplicationWithCommands(
  id: string,
  original: Record<string, any>,
  current: Record<string, any>
) {
  const diff = getDiff(original, current);
  const remaining: Record<string, any> = { ...diff };

  if (Object.prototype.hasOwnProperty.call(diff, 'owner')) {
    const ownerId = diff.owner;
    delete remaining.owner;
    await apiClient.post(`/applications/${id}/commands/set-owner`, { ownerId });
  }

  // Future: classification, lifecycle commands can be added here when fields exist in the form.

  if (Object.keys(remaining).length > 0) {
    await apiClient.patch(`/applications/${id}`, remaining);
  }

  return { id, ...current };
}

export async function updateBusinessCapabilityWithCommands(
  id: string,
  original: Record<string, any>,
  current: Record<string, any>
) {
  const diff = getDiff(original, current);
  const remaining: Record<string, any> = { ...diff };

  if (Object.prototype.hasOwnProperty.call(diff, 'parent')) {
    const parentId = diff.parent;
    delete remaining.parent;

    if (parentId) {
      await apiClient.post(`/business-capabilities/${id}/commands/set-parent`, {
        parentId,
      });
    } else {
      await apiClient.post(`/business-capabilities/${id}/commands/remove-parent`, {});
    }
  }

  if (Object.prototype.hasOwnProperty.call(diff, 'description')) {
    const description = diff.description ?? '';
    delete remaining.description;
    await apiClient.post(`/business-capabilities/${id}/commands/update-description`, {
      description,
    });
  }

  if (Object.keys(remaining).length > 0) {
    await apiClient.patch(`/business-capabilities/${id}`, remaining);
  }

  return { id, ...current };
}

export async function updateOrganizationWithCommands(
  id: string,
  original: Record<string, any>,
  current: Record<string, any>
) {
  const diff = getDiff(original, current);
  const remaining: Record<string, any> = { ...diff };

  if (Object.prototype.hasOwnProperty.call(diff, 'parent')) {
    const parentId = diff.parent;
    delete remaining.parent;

    if (parentId) {
      await apiClient.post(`/organizations/${id}/commands/set-parent`, {
        parentId,
      });
    } else {
      await apiClient.post(`/organizations/${id}/commands/remove-parent`, {});
    }
  }

  if (Object.keys(remaining).length > 0) {
    await apiClient.patch(`/organizations/${id}`, remaining);
  }

  return { id, ...current };
}
