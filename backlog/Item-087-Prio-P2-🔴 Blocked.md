# Item-087: Implement Bulk Operations API & UI

**Status:** � Blocked  
**Priority:** P2 - MEDIUM  
**Effort:** 32-48 hours  
**Created:** 2026-01-17  
**Owner:** Full-Stack Team

---

## Problem Statement

Users frequently need to perform operations on multiple entities simultaneously (bulk delete, bulk archive, bulk update status). Currently, users must perform actions one-by-one, which is time-consuming and inefficient for large datasets.

Bulk operations with progress tracking, undo capability, and error handling improve productivity and user experience as specified in [spec-ui-advanced-patterns.md](../spec/spec-ui-advanced-patterns.md).

This is a Phase 2 advanced feature identified in the FRONTEND-READINESS-REPORT.md.

---

## Affected Files

**Backend:**
- `src/EATool/Api/BulkOperations.fs` - Bulk operation endpoints
- `src/EATool/Domain/BulkOperationTypes.fs` - Bulk operation types
- `src/EATool/Services/BulkOperationService.fs` - Bulk operation business logic
- `src/EATool/Services/BulkOperationQueue.fs` - Queue for processing operations
- `src/EATool/Api/ErrorHandlers.fs` - Bulk operation error handling

**Frontend:**
- `frontend/src/components/entity/BulkOperationBar.tsx` - Bulk action toolbar (from Item-079, enhance)
- `frontend/src/components/entity/BulkProgressModal.tsx` - Progress tracker modal
- `frontend/src/components/entity/BulkUndoToast.tsx` - Undo notification
- `frontend/src/hooks/useBulkOperation.ts` - Bulk operation hook
- `frontend/src/api/bulkOperationsApi.ts` - Bulk operation API client

---

## Specifications

- [spec/spec-ui-advanced-patterns.md](../spec/spec-ui-advanced-patterns.md) - Bulk operations specification
- [spec/spec-ui-entity-workflows.md](../spec/spec-ui-entity-workflows.md) - Entity workflows
- [FRONTEND-READINESS-REPORT.md](../FRONTEND-READINESS-REPORT.md) - Phase 2 feature identification

---

## Detailed Tasks

### Phase 1: Backend Bulk Operations API (16-20 hours)

**Bulk Operation Types:**
- [ ] Define BulkOperationRequest type (operation, entity_type, entity_ids, parameters)
- [ ] Define BulkOperationResponse type (operation_id, status, progress, results, errors)
- [ ] Define supported operations: DELETE, ARCHIVE, UPDATE_STATUS, UPDATE_FIELD, EXPORT
- [ ] Define BulkOperationResult (success_count, failure_count, errors)

**Bulk Endpoints:**
- [ ] POST /api/bulk/delete - Bulk delete entities
- [ ] POST /api/bulk/archive - Bulk archive entities (soft delete)
- [ ] POST /api/bulk/update-status - Bulk update status field
- [ ] POST /api/bulk/update-field - Bulk update specific field
- [ ] POST /api/bulk/export - Bulk export to CSV/JSON
- [ ] GET /api/bulk/{operation_id}/status - Get operation progress
- [ ] POST /api/bulk/{operation_id}/cancel - Cancel ongoing operation
- [ ] POST /api/bulk/{operation_id}/undo - Undo completed operation

**Bulk Operation Service:**
- [ ] Implement bulk delete logic (validate permissions, delete each entity)
- [ ] Implement bulk archive logic (soft delete with archived_at timestamp)
- [ ] Implement bulk update logic (validate, update each entity)
- [ ] Implement bulk export logic (stream to CSV/JSON)
- [ ] Add transaction support (all-or-nothing option)
- [ ] Add validation (check permissions for each entity)
- [ ] Handle partial failures (continue on error option)
- [ ] Return detailed error list (which entities failed and why)

**Operation Queue:**
- [ ] Create in-memory operation queue (or use job queue like Hangfire)
- [ ] Process operations asynchronously
- [ ] Track operation progress (percentage, current item)
- [ ] Support cancellation (stop processing remaining items)
- [ ] Support pause/resume (optional)
- [ ] Clean up completed operations after 24 hours

**Undo Support:**
- [ ] Store operation history (operation type, entity IDs, original values)
- [ ] Implement undo for bulk delete (restore deleted entities)
- [ ] Implement undo for bulk update (revert to original values)
- [ ] Set undo window (30 minutes after operation)
- [ ] Clean up undo history after expiration

### Phase 2: Frontend Bulk Operation UI (16-24 hours)

**Bulk Action Bar (Enhance from Item-079):**
- [ ] Show when items selected in list
- [ ] Display selected count (e.g., "5 items selected")
- [ ] Add "Select all on page" button
- [ ] Add "Select all in list" button (across all pages)
- [ ] Add "Clear selection" button
- [ ] Add bulk action buttons: Delete, Archive, Update Status, Export
- [ ] Disable actions if user lacks permissions

**Bulk Operation Confirmation:**
- [ ] Show confirmation modal before executing bulk operation
- [ ] Display number of items affected
- [ ] Show warning for destructive operations (delete)
- [ ] Allow user to review selected items
- [ ] Add "Don't ask again" checkbox (optional)

**Progress Tracking:**
- [ ] Create BulkProgressModal component
- [ ] Show progress bar (percentage completed)
- [ ] Display operation details (e.g., "Deleting 50 applications")
- [ ] Show item count (e.g., "25 of 50 completed")
- [ ] Display estimated time remaining
- [ ] Show success/error counts
- [ ] List failed items with error messages
- [ ] Add "Cancel" button to stop operation
- [ ] Add "Pause" button (optional)
- [ ] Close modal automatically on completion

**Undo Functionality:**
- [ ] Show undo toast after successful bulk operation
- [ ] Display "Undo" button in toast
- [ ] Show countdown timer (e.g., "Undo available for 29:45")
- [ ] Implement undo action (calls POST /bulk/{operation_id}/undo)
- [ ] Show success message after undo
- [ ] Auto-dismiss toast after timeout

**Error Handling:**
- [ ] Display partial failure results (X succeeded, Y failed)
- [ ] Show error list with entity names and error messages
- [ ] Offer retry for failed items
- [ ] Allow export of error list (CSV)
- [ ] Provide actionable error messages

**Bulk Update Field:**
- [ ] Create BulkUpdateModal for updating specific field
- [ ] Show field selector dropdown
- [ ] Show value input (text, select, etc. based on field type)
- [ ] Validate new value
- [ ] Execute bulk update
- [ ] Show progress

---

## Acceptance Criteria

**Backend:**
- [ ] POST /api/bulk/delete endpoint accepts entity IDs and deletes them
- [ ] POST /api/bulk/archive endpoint archives entities
- [ ] POST /api/bulk/update-status endpoint updates status for entities
- [ ] POST /api/bulk/update-field endpoint updates specific field
- [ ] POST /api/bulk/export endpoint exports entities to CSV/JSON
- [ ] GET /api/bulk/{operation_id}/status returns operation progress
- [ ] POST /api/bulk/{operation_id}/cancel cancels ongoing operation
- [ ] POST /api/bulk/{operation_id}/undo reverts completed operation
- [ ] Operations processed asynchronously (don't block request)
- [ ] Operations respect user permissions (check each entity)
- [ ] Partial failures handled (return success/failure counts)
- [ ] Detailed error list returned (entity ID + error message)
- [ ] Undo available for 30 minutes after operation

**Frontend:**
- [ ] Bulk action bar shows when items selected
- [ ] Selected count displayed accurately
- [ ] Select all buttons work (page and entire list)
- [ ] Clear selection button works
- [ ] Bulk actions trigger confirmation modal
- [ ] Progress modal shows during operation
- [ ] Progress bar updates in real-time
- [ ] Success/error counts displayed
- [ ] Failed items listed with error messages
- [ ] Cancel button stops ongoing operation
- [ ] Undo toast shows after successful operation
- [ ] Undo countdown timer displayed
- [ ] Undo action reverts changes successfully
- [ ] Error handling displays partial failures clearly
- [ ] Bulk update field modal allows field selection and value input

**Performance:**
- [ ] Bulk operations on 100 entities complete in <10 seconds
- [ ] Progress updates at least every 1 second
- [ ] No UI freezing during operations (async)
- [ ] Export of 1000 entities completes in <30 seconds

**UX:**
- [ ] Clear feedback for all operations
- [ ] No data loss (undo available)
- [ ] Errors explained clearly
- [ ] Operations can be cancelled
- [ ] Loading states prevent duplicate operations

---

## Dependencies

**Blocked by:**  
- Item-075 (Frontend project setup)
- Item-076 (Component library - needs Modal, ProgressBar, Toast)
- Item-079 (Entity list pages - needs BulkActionBar foundation)

**Blocks:** None (enhances existing features)

---

## Notes

- **Phase 2 Feature:** Can be implemented after MVP Phase 1
- Consider using job queue (Hangfire, Quartz) for backend processing
- Test with large datasets (1000+ entities)
- Add rate limiting to prevent abuse
- Consider batch size optimization (process in chunks of 50)
- Add telemetry for bulk operation performance
- Consider adding scheduled bulk operations (e.g., nightly archive)
- Test undo with various operations (delete, update)
- Consider WebSocket notifications for progress updates (see Item-085)
- Add bulk operation audit logging
- Test cancellation mid-operation
- Consider adding bulk import functionality (CSV/JSON upload)
- Test with slow backend (simulated latency)
