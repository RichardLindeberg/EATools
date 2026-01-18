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

  if (Object.prototype.hasOwnProperty.call(diff, 'classification')) {
    const classification = diff.classification;
    delete remaining.classification;
    delete remaining.classificationReason;

    const reason = current.classificationReason || 'Updated classification via UI';

    await apiClient.post(`/applications/${id}/commands/set-classification`, {
      classification,
      reason,
    });
  }

  if (
    Object.prototype.hasOwnProperty.call(diff, 'lifecycle') ||
    Object.prototype.hasOwnProperty.call(diff, 'sunsetDate')
  ) {
    const targetLifecycle = current.lifecycle;
    const sunsetDate = current.sunsetDate || undefined;

    delete remaining.lifecycle;
    delete remaining.sunsetDate;

    await apiClient.post(`/applications/${id}/commands/transition-lifecycle`, {
      target_lifecycle: targetLifecycle,
      sunset_date: sunsetDate || undefined,
    });
  }

  if (Object.prototype.hasOwnProperty.call(diff, 'owner')) {
    const owner = diff.owner;
    delete remaining.owner;

    await apiClient.post(`/applications/${id}/commands/set-owner`, { owner });
  }

  if (Object.prototype.hasOwnProperty.call(remaining, 'classificationReason')) {
    delete remaining.classificationReason;
  }

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
        parent_id: parentId,
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
        parent_id: parentId,
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

export async function updateApplicationServiceWithCommands(
  id: string,
  original: Record<string, any>,
  current: Record<string, any>
) {
  const diff = getDiff(original, current);
  const remaining: Record<string, any> = { ...diff };

  if (Object.prototype.hasOwnProperty.call(diff, 'businessCapabilityId')) {
    const businessCapabilityId = diff.businessCapabilityId;
    delete remaining.businessCapabilityId;

    await apiClient.post(`/application-services/${id}/commands/set-business-capability`, {
      business_capability_id: businessCapabilityId,
    });
  }

  if (Object.prototype.hasOwnProperty.call(diff, 'consumerAppId')) {
    const appId = diff.consumerAppId;
    delete remaining.consumerAppId;

    await apiClient.post(`/application-services/${id}/commands/add-consumer`, {
      app_id: appId,
    });
  }

  if (Object.keys(remaining).length > 0) {
    await apiClient.post(`/application-services/${id}/commands/update`, remaining);
  }

  return { id, ...current };
}

export async function updateApplicationInterfaceWithCommands(
  id: string,
  original: Record<string, any>,
  current: Record<string, any>
) {
  const diff = getDiff(original, current);
  const remaining: Record<string, any> = { ...diff };

  if (Object.prototype.hasOwnProperty.call(diff, 'serviceIds')) {
    const serviceIds = diff.serviceIds;
    delete remaining.serviceIds;

    await apiClient.post(`/application-interfaces/${id}/commands/set-service`, {
      service_ids: serviceIds,
    });
  }

  if (Object.prototype.hasOwnProperty.call(diff, 'status')) {
    const status = diff.status;
    delete remaining.status;

    if (status === 'deprecated') {
      await apiClient.post(`/application-interfaces/${id}/commands/deprecate`, {});
    } else if (status === 'retired') {
      await apiClient.post(`/application-interfaces/${id}/commands/retire`, {});
    }
  }

  if (Object.keys(remaining).length > 0) {
    await apiClient.post(`/application-interfaces/${id}/commands/update`, remaining);
  }

  return { id, ...current };
}

export async function updateRelationWithCommands(
  id: string,
  original: Record<string, any>,
  current: Record<string, any>
) {
  const diff = getDiff(original, current);
  const remaining: Record<string, any> = { ...diff };

  if (Object.prototype.hasOwnProperty.call(diff, 'confidence')) {
    const confidence = diff.confidence;
    delete remaining.confidence;

    await apiClient.post(`/relations/${id}/commands/update-confidence`, {
      confidence,
    });
  }

  if (
    Object.prototype.hasOwnProperty.call(diff, 'effectiveFrom') ||
    Object.prototype.hasOwnProperty.call(diff, 'effectiveTo')
  ) {
    const effectiveFrom = current.effectiveFrom || undefined;
    const effectiveTo = current.effectiveTo || undefined;

    delete remaining.effectiveFrom;
    delete remaining.effectiveTo;

    await apiClient.post(`/relations/${id}/commands/set-effective-dates`, {
      effective_from: effectiveFrom,
      effective_to: effectiveTo,
    });
  }

  if (Object.prototype.hasOwnProperty.call(diff, 'description')) {
    const description = diff.description ?? '';
    delete remaining.description;

    await apiClient.post(`/relations/${id}/commands/update-description`, {
      description,
    });
  }

  if (Object.keys(remaining).length > 0) {
    await apiClient.patch(`/relations/${id}`, remaining);
  }

  return { id, ...current };
}
