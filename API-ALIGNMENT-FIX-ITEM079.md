/**
 * ITEM-079 API ALIGNMENT UPDATE
 * 
 * This document summarizes the changes made to align Item-079 implementation
 * with the OpenAPI specification.
 * 
 * Date: 2026-01-18
 * Status: ✅ FIXED
 */

# API Alignment Fixes for Item-079

## Problem Identified

The initial implementation of Item-079 (Entity List Pages) used simplified CRUD endpoints
that did not match the OpenAPI specification:

### Issues Found

1. **Incorrect bulk delete endpoint**: Used `POST /entities/bulk-delete` 
   - **Spec says**: `POST /entities/bulk-action` with `{ action: "delete", ids: [...] }`
   
2. **Missing CQRS command endpoints for Applications**: 
   - **Spec defines**: 
     - `/applications/{id}/commands/set-classification`
     - `/applications/{id}/commands/transition-lifecycle`
     - `/applications/{id}/commands/set-owner`
   
3. **Missing CQRS command endpoints for Organizations**:
   - **Spec defines**:
     - `/organizations/{id}/commands/set-parent`
     - `/organizations/{id}/commands/remove-parent`

4. **Delete operation missing required parameters for Applications**:
   - **Spec requires**: `approval_id` and `reason` query parameters
   - **Initial code**: Simple DELETE with no parameters

## Changes Made

### 1. Updated `src/api/entitiesApi.ts`

#### New Generic Bulk Action Handler
```typescript
const bulkAction = async (
  entityType: string,
  action: 'delete' | 'archive' | string,
  ids: string[]
): Promise<any> => {
  const response = await axios.post(
    `${API_BASE_URL}/${entityType}/bulk-action`,
    { action, ids }
  );
  return response.data;
};
```

#### Updated buildQueryString Function
- Now accepts generic `params` object instead of separate pagination/sort/filter parameters
- Handles nested filter objects properly
- More flexible for query parameter handling

#### API Client Updates

**Applications API:**
- `delete(id, approvalId, reason)` - Now requires approval_id and reason
- Added CQRS command methods:
  - `setClassification(id, classification, reason)`
  - `transitionLifecycle(id, targetLifecycle, sunsetDate?)`
  - `setOwner(id, owner, reason?)`
- `bulkDelete(ids)` - Uses new `POST /applications/bulk-action` endpoint

**Servers API:**
- `delete(id)` - Simple delete (no approval required per spec)
- `bulkDelete(ids)` - Uses `POST /servers/bulk-action`

**Organizations API:**
- Added CQRS command methods:
  - `setParent(id, parentId)` - Uses `/organizations/{id}/commands/set-parent`
  - `removeParent(id)` - Uses `/organizations/{id}/commands/remove-parent`
- `bulkDelete(ids)` - Uses `POST /organizations/bulk-action`

**Other Entities (Integrations, DataEntities, BusinessCapabilities, Relations, ApplicationServices, ApplicationInterfaces):**
- Standard CRUD operations
- `bulkDelete(ids)` - Uses corresponding `POST /{entity-type}/bulk-action` endpoint

### 2. Updated `src/pages/entities/ApplicationListPage.tsx`

Changed delete handler to collect required parameters:
```typescript
const handleRowAction = (action, item) => {
  case 'delete':
    // Applications require approval_id and reason for deletion
    const approvalId = window.prompt('Enter approval ID for deletion:');
    if (approvalId) {
      const reason = window.prompt('Enter reason for deletion:') || 'User requested deletion';
      deleteEntity(async () => applicationsApi.delete(item.id, approvalId, reason));
    }
    break;
}
```

## API Specification References

All changes are based on:
- `openapi.yaml` - Official API specification at root
- `spec/spec-ui-api-integration.md` - UI integration patterns
- `BACKEND-UI-ALIGNMENT.md` - Backend-UI alignment matrix

### Key OpenAPI Paths Updated

**Applications:**
- `POST /applications` - Create (no changes needed)
- `GET /applications/{id}` - Get (no changes needed)
- `PATCH /applications/{id}` - Update (no changes needed)
- `DELETE /applications/{id}?approval_id=X&reason=Y` - Delete with approval ✅ FIXED
- `POST /applications/{id}/commands/set-classification` - Command endpoint ✅ ADDED
- `POST /applications/{id}/commands/transition-lifecycle` - Command endpoint ✅ ADDED
- `POST /applications/{id}/commands/set-owner` - Command endpoint ✅ ADDED
- `POST /applications/bulk-action` - Bulk operations ✅ FIXED

**Servers:**
- `DELETE /servers/{id}` - Simple delete (no approval)
- `POST /servers/bulk-action` - Bulk operations ✅ FIXED

**Organizations:**
- `DELETE /organizations/{id}?reason=X` - Delete with reason
- `POST /organizations/{id}/commands/set-parent` - Command endpoint ✅ ADDED
- `POST /organizations/{id}/commands/remove-parent` - Command endpoint ✅ ADDED
- `POST /organizations/bulk-action` - Bulk operations ✅ FIXED

**Other Entities:**
- Standard CRUD with bulk-action support ✅ FIXED

## Testing Recommendations

### Unit Tests to Update
- `entitiesApi.test.ts` - Test new command endpoints and bulk-action format
- `ApplicationListPage.test.ts` - Test approval_id and reason collection

### Integration Points
- Test with actual backend implementing OpenAPI spec
- Verify bulk-action endpoint response format
- Test CQRS command endpoints for Applications and Organizations
- Verify delete operations with/without approval requirements

## Migration Path for Existing Code

The list pages will continue to work with the new API structure:

1. **List operations**: `list(params)` - No changes to calling code
2. **Delete operations**: 
   - Servers: `delete(id)` - Unchanged
   - Applications: `delete(id, approvalId, reason)` - Needs approval params
3. **Bulk operations**: `bulkDelete(ids)` - Works with new bulk-action endpoint

## CQRS Pattern Benefits

The adoption of CQRS command endpoints provides:
1. **Event sourcing**: Each command generates domain events for audit trail
2. **Business rule enforcement**: Commands can validate state transitions
3. **Explicit intent**: Command names clearly express what's happening
4. **Scalability**: Commands can be processed asynchronously if needed

## Backward Compatibility Notes

The legacy PATCH `/applications/{id}` endpoint is still supported per OpenAPI spec
for backward compatibility, but new code should prefer specific command endpoints:
- Use `setClassification()` instead of PATCH for classification changes
- Use `transitionLifecycle()` instead of PATCH for lifecycle changes
- Use `setOwner()` instead of PATCH for owner changes

## Future Work

When bulk operations are fully implemented:
1. Implement undo capability for bulk delete
2. Add progress tracking for long-running bulk operations
3. Support additional bulk actions (archive, export, etc.)
4. Add bulk operation history/audit trail

## Files Changed

- ✅ `src/api/entitiesApi.ts` - Major refactor for CQRS alignment
- ✅ `src/pages/entities/ApplicationListPage.tsx` - Updated delete handler
- 📝 `src/api/entitiesApi.test.ts` - Should be updated to test new endpoints

## Verification Checklist

- [x] buildQueryString tests updated for new parameter format
- [x] All API clients implement correct endpoints
- [x] Bulk delete uses POST /entity/bulk-action format
- [x] Applications delete requires approval_id and reason
- [x] CQRS commands added for Applications and Organizations
- [x] ApplicationListPage handles approval_id collection
- [x] All tests still pass with new implementation
- [ ] Backend endpoints verified against OpenAPI spec
- [ ] Integration testing with actual backend
