# Item-089: Implement Export & Import Functionality

**Status:** � Blocked  
**Priority:** P2 - MEDIUM  
**Effort:** 24-32 hours  
**Created:** 2026-01-17  
**Owner:** Full-Stack Team

---

## Problem Statement

Users need to export entity data for reporting, backup, and integration with external tools (Excel, BI tools). They also need to import entity data for bulk loading, migrations, and data synchronization.

Export and import functionality with multiple format support (CSV, JSON, Excel) and validation improves interoperability and data portability as specified in [spec-ui-entity-workflows.md](../spec/spec-ui-entity-workflows.md).

This is a Phase 2 advanced feature for data management.

---

## Affected Files

**Backend:**
- `src/EATool/Export/ExportTypes.fs` - Export domain types
- `src/EATool/Export/ExportService.fs` - Export business logic
- `src/EATool/Export/CsvExporter.fs` - CSV export implementation
- `src/EATool/Export/JsonExporter.fs` - JSON export implementation
- `src/EATool/Export/ExcelExporter.fs` - Excel export implementation (optional)
- `src/EATool/Import/ImportService.fs` - Import business logic
- `src/EATool/Import/CsvImporter.fs` - CSV import implementation
- `src/EATool/Import/JsonImporter.fs` - JSON import implementation
- `src/EATool/Import/ImportValidator.fs` - Import data validation
- `src/EATool/Api/ExportEndpoints.fs` - Export API endpoints
- `src/EATool/Api/ImportEndpoints.fs` - Import API endpoints

**Frontend:**
- `frontend/src/components/export/ExportModal.tsx` - Export configuration modal
- `frontend/src/components/import/ImportModal.tsx` - Import wizard
- `frontend/src/components/import/ImportPreview.tsx` - Preview import data
- `frontend/src/components/import/ImportProgress.tsx` - Import progress tracker
- `frontend/src/hooks/useExport.ts` - Export hook
- `frontend/src/hooks/useImport.ts` - Import hook
- `frontend/src/api/exportApi.ts` - Export API client
- `frontend/src/api/importApi.ts` - Import API client

---

## Specifications

- [spec/spec-ui-entity-workflows.md](../spec/spec-ui-entity-workflows.md) - Cross-cutting concerns including export
- [spec/spec-ui-advanced-patterns.md](../spec/spec-ui-advanced-patterns.md) - Progress tracking
- [FRONTEND-READINESS-REPORT.md](../FRONTEND-READINESS-REPORT.md) - Phase 2 feature

---

## Detailed Tasks

### Phase 1: Backend Export Implementation (12-16 hours)

**Export Endpoints:**
- [ ] GET /api/{entity_type}/export?format={csv|json|xlsx}&filters={json} - Export entities
- [ ] GET /api/export/{export_id}/download - Download completed export
- [ ] GET /api/export/{export_id}/status - Check export progress (for large datasets)

**CSV Exporter:**
- [ ] Implement CSV export for all 9 entity types
- [ ] Include all fields (ID, Name, Description, etc.)
- [ ] Escape special characters (commas, quotes, newlines)
- [ ] Add header row with column names
- [ ] Support custom column selection (export specific fields only)
- [ ] Handle relationships (export IDs or names)
- [ ] Stream large datasets (don't load all into memory)

**JSON Exporter:**
- [ ] Implement JSON export for all 9 entity types
- [ ] Export as JSON array of objects
- [ ] Include nested relationships (optional)
- [ ] Support pretty-print formatting
- [ ] Support compact formatting (no whitespace)
- [ ] Stream large datasets (newline-delimited JSON)

**Excel Exporter (Optional):**
- [ ] Implement Excel (.xlsx) export using ClosedXML or similar
- [ ] Create worksheet per entity type
- [ ] Format headers (bold, freeze pane)
- [ ] Add filters to header row
- [ ] Support cell formatting (dates, numbers)

**Export Service:**
- [ ] Apply filters to export (same as list page filters)
- [ ] Respect user permissions (only export visible entities)
- [ ] Support pagination for large exports (chunked download)
- [ ] Add export audit logging (who exported what, when)
- [ ] Clean up temporary export files after download

### Phase 2: Backend Import Implementation (12-16 hours)

**Import Endpoints:**
- [ ] POST /api/{entity_type}/import - Upload import file
- [ ] POST /api/import/{import_id}/validate - Validate import data
- [ ] POST /api/import/{import_id}/execute - Execute import
- [ ] GET /api/import/{import_id}/status - Check import progress
- [ ] GET /api/import/{import_id}/results - Get import results (success/failure counts, errors)

**CSV Importer:**
- [ ] Parse CSV file (handle different delimiters: comma, semicolon, tab)
- [ ] Validate header row (check required columns present)
- [ ] Parse data rows
- [ ] Handle quoted values, escaped characters
- [ ] Support different encodings (UTF-8, ISO-8859-1)

**JSON Importer:**
- [ ] Parse JSON file (array of objects)
- [ ] Validate JSON structure
- [ ] Support nested objects (flatten or import as relationships)

**Import Validator:**
- [ ] Validate required fields present
- [ ] Validate data types (string, number, date, enum)
- [ ] Validate field formats (email, URL, IP)
- [ ] Validate enum values (status, type, etc.)
- [ ] Check for duplicate IDs or names
- [ ] Validate relationships (referenced entities exist)
- [ ] Return validation errors per row (row number, field, error message)

**Import Service:**
- [ ] Support create-only mode (skip existing entities)
- [ ] Support update-only mode (update existing, skip new)
- [ ] Support upsert mode (create new, update existing)
- [ ] Process import in batches (e.g., 100 rows at a time)
- [ ] Track progress (rows processed, success, failure counts)
- [ ] Return detailed results (success count, failure count, error list)
- [ ] Respect user permissions (check create/update permissions)
- [ ] Add import audit logging

### Phase 3: Frontend Export UI (8-12 hours)

**Export Button:**
- [ ] Add "Export" button to entity list pages
- [ ] Add "Export" option to bulk action bar (export selected)
- [ ] Add "Export" button to search results page

**Export Modal:**
- [ ] Create ExportModal component
- [ ] Format selector (CSV, JSON, Excel)
- [ ] Scope selector (All, Filtered, Selected)
- [ ] Field selector (All fields, Selected fields)
- [ ] Preview (show first 5 rows)
- [ ] "Export" button to trigger download
- [ ] Show progress for large exports (optional)

**Download Handling:**
- [ ] Trigger browser download on export completion
- [ ] Use appropriate filename (e.g., applications_2024-01-17.csv)
- [ ] Show success toast notification
- [ ] Handle errors (file too large, export failed)

### Phase 4: Frontend Import UI (8-12 hours)

**Import Button:**
- [ ] Add "Import" button to entity list pages (header)
- [ ] Require appropriate permissions (create or update)

**Import Wizard:**
- [ ] Step 1: File upload (drag-and-drop or file picker)
- [ ] Step 2: Validate import (show validation errors)
- [ ] Step 3: Configure import (create/update/upsert mode)
- [ ] Step 4: Execute import (show progress)
- [ ] Step 5: Review results (success/failure counts, error list)

**Import Preview:**
- [ ] Show preview of import data (first 10 rows)
- [ ] Display column mapping (CSV columns → entity fields)
- [ ] Allow manual column mapping adjustment
- [ ] Highlight validation errors in preview

**Import Progress:**
- [ ] Show progress bar (percentage)
- [ ] Display item count (50 of 100 imported)
- [ ] Show success/failure counts in real-time
- [ ] Support cancel during import

**Import Results:**
- [ ] Display success count, failure count
- [ ] Show error list (row number, error message)
- [ ] Allow export of error list (CSV)
- [ ] Offer retry for failed rows

---

## Acceptance Criteria

**Export - Backend:**
- [ ] GET /api/{entity_type}/export?format=csv exports to CSV
- [ ] GET /api/{entity_type}/export?format=json exports to JSON
- [ ] Export respects filters (only export filtered entities)
- [ ] Export respects user permissions (no unauthorized entities)
- [ ] Export handles large datasets (1000+ entities)
- [ ] CSV format correctly escaped (commas, quotes, newlines)
- [ ] JSON format valid and parsable

**Export - Frontend:**
- [ ] "Export" button visible on list pages
- [ ] Export modal allows format selection
- [ ] Export modal allows scope selection (all, filtered, selected)
- [ ] Export triggers browser download
- [ ] Filename appropriate (entity_type_date.format)
- [ ] Success notification shown
- [ ] Error handling works (file too large, export failed)

**Import - Backend:**
- [ ] POST /api/{entity_type}/import accepts CSV/JSON files
- [ ] Validation checks required fields, data types, formats
- [ ] Validation returns detailed error list (row, field, error)
- [ ] Import supports create, update, upsert modes
- [ ] Import processes in batches (no memory issues)
- [ ] Import tracks progress (rows processed, success, failure)
- [ ] Import respects user permissions
- [ ] Import results include detailed error list

**Import - Frontend:**
- [ ] "Import" button visible on list pages
- [ ] Import wizard guides user through steps
- [ ] File upload works (drag-and-drop and file picker)
- [ ] Validation errors displayed clearly
- [ ] Import mode selectable (create, update, upsert)
- [ ] Progress tracker shows during import
- [ ] Results displayed (success/failure counts, error list)
- [ ] Error list exportable

**Performance:**
- [ ] Export 1000 entities completes in <10 seconds
- [ ] Import 1000 entities completes in <30 seconds
- [ ] No memory issues with large files (stream processing)

---

## Dependencies

**Blocked by:**  
- Item-075 (Frontend project setup)
- Item-076 (Component library - needs Modal, ProgressBar, FileUpload)
- Items 079-081 (Entity pages to add export/import)

**Blocks:** None (enhances existing features)

---

## Notes

- **Phase 2 Feature:** Can be implemented after MVP Phase 1
- Consider file size limits (e.g., 10MB max upload)
- Add support for Excel format if users request it
- Test with various CSV formats (different delimiters, encodings)
- Test with large files (10,000+ rows)
- Consider adding import templates (download template CSV with headers)
- Add import dry-run mode (validate without importing)
- Consider adding scheduled imports (optional, advanced)
- Test error handling thoroughly (invalid files, network errors)
- Add import/export to audit log
- Consider compression for large exports (ZIP)
- Test with non-ASCII characters (Unicode)
- Consider adding field mapping UI (drag-and-drop column mapping)
