# Item-081: Implement Entity Create/Edit Forms (All 9 Types)

**Status:** � Blocked  
**Priority:** P1 - HIGH  
**Effort:** 64-80 hours  
**Created:** 2026-01-17  
**Owner:** Frontend Team

---

## Problem Statement

Users need forms to create new entities and edit existing ones. Each of the 9 entity types has specific fields, validation rules, and relationship selections that must be implemented according to the backend API requirements.

Forms must support validation matching backend rules, required field indicators, error display, relationship selection, and conditional fields as specified in [spec-ui-entity-workflows.md](../spec/spec-ui-entity-workflows.md) and [spec-ui-advanced-patterns.md](../spec/spec-ui-advanced-patterns.md).

Without create/edit forms, users cannot add new entities or modify existing ones, making the application read-only.

---

## Affected Files

**Create:**
- `frontend/src/pages/applications/ApplicationFormPage.tsx`
- `frontend/src/pages/servers/ServerFormPage.tsx`
- `frontend/src/pages/integrations/IntegrationFormPage.tsx`
- `frontend/src/pages/data-entities/DataEntityFormPage.tsx`
- `frontend/src/pages/business-capabilities/BusinessCapabilityFormPage.tsx`
- `frontend/src/pages/organizations/OrganizationFormPage.tsx`
- `frontend/src/pages/relations/RelationFormPage.tsx`
- `frontend/src/pages/application-services/ApplicationServiceFormPage.tsx`
- `frontend/src/pages/application-interfaces/ApplicationInterfaceFormPage.tsx`
- `frontend/src/components/forms/EntityFormTemplate.tsx` - Reusable form template
- `frontend/src/components/forms/RelationshipSelector.tsx` - Select related entities
- `frontend/src/components/forms/DynamicFieldArray.tsx` - Add/remove dynamic fields
- `frontend/src/hooks/useEntityForm.ts` - Form state and submission hook
- `frontend/src/utils/formValidation.ts` - Validation rules
- `frontend/src/utils/formHelpers.ts` - Form utilities

---

## Specifications

- [spec/spec-ui-entity-workflows.md](../spec/spec-ui-entity-workflows.md) - Entity workflows
- [spec/spec-ui-advanced-patterns.md](../spec/spec-ui-advanced-patterns.md) - Dynamic forms, validation
- [spec/spec-ui-api-integration.md](../spec/spec-ui-api-integration.md) - API integration
- [BACKEND-UI-ALIGNMENT.md](../BACKEND-UI-ALIGNMENT.md) - Backend validation alignment

---

## Detailed Tasks

### Phase 1: Foundation & Shared Components (16-20 hours)

**Form Infrastructure (CQRS-aware):**
- [ ] Setup React Hook Form for all forms
- [ ] Create form validation schemas (Zod or Yup)
- [ ] Implement server-side validation error mapping (backend errors → form fields)
- [ ] Create separate submission paths: Create (POST collection) vs Edit (dispatch commands)
- [ ] Add command dispatch layer for edits (routes to specific command endpoints per entity)
- [ ] Handle command errors: 422 (validation) mapped to fields, 403 (forbidden) shown as permission error
- [ ] Implement optimistic updates cautiously (only where safe); otherwise refetch on success
- [ ] Add auto-save functionality (optional) — limit to fields that map cleanly to a single command or disable in edit mode for command-based entities

**Shared Components:**
- [ ] Create EntityFormTemplate (layout, header, actions, sections)
- [ ] Create RelationshipSelector (search and select related entities)
- [ ] Create DynamicFieldArray (add/remove repeating fields)
- [ ] Create FormFieldWrapper (label, error, help text, required indicator)
- [ ] Create DiscardChangesModal (confirmation before leaving with unsaved changes)

**Custom Hooks:**
- [ ] Create useEntityForm hook (handles create vs edit, submission, validation)
- [ ] Create useFormDirty hook (detects unsaved changes)
- [ ] Create useRelationshipSearch hook (search for entities to link)

### Phase 1A: Create-Only Forms (MVP - 20-24 hours)
- [ ] Implement create flows for all 9 entities (POST collection)
- [ ] Validate inputs and display 422 errors
- [ ] Redirect to detail page on success

### Phase 1B: Command-Based Edit Flows (24-32 hours)
- [ ] Applications: implement command-based edits (classification, lifecycle, owner); fallback to legacy PATCH for remaining fields
- [ ] BusinessCapabilities: implement set/remove parent, update-description commands; fallback to PATCH for remaining fields
- [ ] Organizations: implement set/remove parent commands; fallback to PATCH for remaining fields
- [ ] Remaining entities (Servers, Integrations, DataEntities, Relations, ApplicationServices, ApplicationInterfaces): implement PATCH-based edits per openapi.yaml
- [ ] Centralize diff-based dispatcher and error handling

### Phase 2: Application Form (8-10 hours)
- [ ] Create ApplicationFormPage (handles /applications/new and /applications/:id/edit)
- [ ] Add fields: Name* (text), Description (textarea), Type* (select), Status* (select)
- [ ] Add fields: Version (text), Owner* (select user), URL (text, URL validation)
- [ ] Add validation: Required fields, URL format, max lengths
- [ ] Add relationship selector: Servers (multi-select), Integrations (multi-select)
- [ ] Handle create (POST /applications)
- [ ] Handle edits via commands (prefer over legacy PATCH):
	- Set classification: POST /applications/{id}/commands/set-classification
	- Transition lifecycle: POST /applications/{id}/commands/transition-lifecycle
	- Set owner: POST /applications/{id}/commands/set-owner
	- For remaining fields without commands (e.g., name, description, url, type, status, version), use legacy PATCH /applications/{id} (as per openapi.yaml notes)
- [ ] Show loading state during submission
- [ ] Display server validation errors
- [ ] Redirect to detail page on success

### Phase 3: Server Form (7-9 hours)
- [ ] Create ServerFormPage
- [ ] Add fields: Name* (text), Hostname* (text), IP Address (text, IP validation)
- [ ] Add fields: Environment* (select), Status* (select), OS (text)
- [ ] Add fields: CPU (number), Memory (number), Owner* (select user)
- [ ] Add validation: Required fields, IP format, positive numbers
- [ ] Handle create (POST /servers)
- [ ] Handle edit (PATCH /servers/{id}) — no specific command endpoints defined

### Phase 4: Integration Form (8-10 hours)
- [ ] Create IntegrationFormPage
- [ ] Add fields: Name* (text), Type* (select), Protocol* (select)
- [ ] Add fields: Source* (select application), Target* (select application)
- [ ] Add fields: Status* (select), Configuration (JSON editor)
- [ ] Add validation: Required fields, valid JSON for configuration
- [ ] Add relationship selector: DataEntities (multi-select)
- [ ] Handle create (POST /integrations)
- [ ] Handle edit (PATCH /integrations/{id}) — no specific command endpoints defined

### Phase 5: DataEntity Form (7-9 hours)
- [ ] Create DataEntityFormPage
- [ ] Add fields: Name* (text), Type* (select), Format* (select)
- [ ] Add fields: Classification* (select), Schema (JSON editor), Owner* (select user)
- [ ] Add validation: Required fields, valid JSON for schema
- [ ] Handle create (POST /data-entities)
- [ ] Handle edit (PATCH /data-entities/{id}) — no specific command endpoints defined

### Phase 6: BusinessCapability Form (7-9 hours)
- [ ] Create BusinessCapabilityFormPage
- [ ] Add fields: Name* (text), Description (textarea), Level* (select)
- [ ] Add fields: Parent (select business capability), Owner* (select user), Status* (select)
- [ ] Add validation: Required fields, prevent circular parent references
- [ ] Add relationship selector: Applications (multi-select), Organizations (multi-select)
- [ ] Handle create (POST /business-capabilities)
- [ ] Handle edits via commands (prefer over PATCH where available):
	- Set parent: POST /business-capabilities/{id}/commands/set-parent
	- Remove parent: POST /business-capabilities/{id}/commands/remove-parent
	- Update description: POST /business-capabilities/{id}/commands/update-description
	- For remaining fields without commands (e.g., name, level, owner, status), use PATCH /business-capabilities/{id}

### Phase 7: Organization Form (6-8 hours)
- [ ] Create OrganizationFormPage
- [ ] Add fields: Name* (text), Type* (select), Parent (select organization)
- [ ] Add fields: Owner* (select user), Contact (email, email validation)
- [ ] Add validation: Required fields, email format, prevent circular parent references
- [ ] Handle create (POST /organizations)
- [ ] Handle edits via commands where applicable:
	- Set parent: POST /organizations/{id}/commands/set-parent
	- Remove parent: POST /organizations/{id}/commands/remove-parent
	- For remaining fields without commands, use PATCH /organizations/{id}

### Phase 8: Relation Form (6-8 hours)
- [ ] Create RelationFormPage
- [ ] Add fields: Type* (select), Source Entity* (dynamic entity selector)
- [ ] Add fields: Target Entity* (dynamic entity selector), Metadata (JSON editor)
- [ ] Add validation: Required fields, valid JSON for metadata
- [ ] Implement dynamic entity selector (select entity type, then select specific entity)
- [ ] Handle create (POST /relations)
- [ ] Handle edits via commands where available:
	- Confidence changes: POST /relations/{id}/commands/update-confidence
	- Effective dates changes: POST /relations/{id}/commands/set-effective-dates
	- Description changes: POST /relations/{id}/commands/update-description
	- For remaining fields, use PATCH /relations/{id}

### Phase 9: ApplicationService Form (6-8 hours)
- [ ] Create ApplicationServiceFormPage
- [ ] Add fields: Name* (text), Application* (select application), Protocol* (select)
- [ ] Add fields: Endpoint (text, URL validation), Status* (select)
- [ ] Add validation: Required fields, URL format
- [ ] Handle create (POST /application-services)
- [ ] Handle edits via commands where available:
	- Type/core field changes: POST /application-services/{id}/commands/update
	- Business capability changes: POST /application-services/{id}/commands/set-business-capability
	- Consumer additions: POST /application-services/{id}/commands/add-consumer
	- For remaining fields, use PATCH /application-services/{id}

### Phase 10: ApplicationInterface Form (7-9 hours)
- [ ] Create ApplicationInterfaceFormPage
- [ ] Add fields: Name* (text), Type* (select), Protocol* (select)
- [ ] Add fields: Source Application* (select), Target Application* (select), Status* (select)
- [ ] Add validation: Required fields, source ≠ target
- [ ] Add relationship selector: ApplicationServices (multi-select)
- [ ] Handle create (POST /application-interfaces)
- [ ] Handle edits via commands where available:
	- Type/core field changes: POST /application-interfaces/{id}/commands/update
	- Service changes: POST /application-interfaces/{id}/commands/set-service
	- Deprecation (status→deprecated): POST /application-interfaces/{id}/commands/deprecate
	- Retirement (status→retired): POST /application-interfaces/{id}/commands/retire
	- For remaining fields, use PATCH /application-interfaces/{id}

---

## Acceptance Criteria

**Create Operations (Commands):**
- [ ] Create uses POST to collection endpoints with correct create schema
- [ ] Success returns newly created entity and redirects to detail page
- [ ] Validation errors (422) map to correct fields with messages

**Edit Operations (Commands vs Legacy PATCH):**
- [ ] Applications: edits use commands where available; no generic PATCH for classification/lifecycle/owner
	- set-classification → POST /applications/{id}/commands/set-classification
	- transition-lifecycle → POST /applications/{id}/commands/transition-lifecycle
	- set-owner → POST /applications/{id}/commands/set-owner
	- fields without commands (name, description, url, type, status, version) use legacy PATCH /applications/{id}
- [ ] BusinessCapabilities: parent/description changes use commands; other fields via PATCH
	- set-parent → POST /business-capabilities/{id}/commands/set-parent
	- remove-parent → POST /business-capabilities/{id}/commands/remove-parent
	- update-description → POST /business-capabilities/{id}/commands/update-description
- [ ] Organizations: parent changes use commands; other fields via PATCH
	- set-parent → POST /organizations/{id}/commands/set-parent
	- remove-parent → POST /organizations/{id}/commands/remove-parent
- [ ] Relations: specific commands for confidence/dates/description; other fields via PATCH
	- update-confidence → POST /relations/{id}/commands/update-confidence
	- set-effective-dates → POST /relations/{id}/commands/set-effective-dates
	- update-description → POST /relations/{id}/commands/update-description
- [ ] ApplicationServices: commands for update/capability/consumer; other fields via PATCH
	- update → POST /application-services/{id}/commands/update
	- set-business-capability → POST /application-services/{id}/commands/set-business-capability
	- add-consumer → POST /application-services/{id}/commands/add-consumer
- [ ] ApplicationInterfaces: commands for update/service/deprecate/retire; other fields via PATCH
	- update → POST /application-interfaces/{id}/commands/update
	- set-service → POST /application-interfaces/{id}/commands/set-service
	- deprecate → POST /application-interfaces/{id}/commands/deprecate
	- retire → POST /application-interfaces/{id}/commands/retire
- [ ] Servers, Integrations, DataEntities: edits via PATCH (no specific command endpoints defined)
- [ ] Command responses update UI state and invalidate relevant queries
- [ ] Command validation errors (422) display per-field messages; 403 shows permission error; other errors show friendly message

**Form UX:**
- [ ] All required fields marked with asterisk (*)
- [ ] Client-side validation runs on blur and on submit
- [ ] Relationship selectors allow searching and selecting related entities
- [ ] Submit button disabled and shows loading during submission
- [ ] Success message and redirect to detail page on success
- [ ] Cancel navigates back; unsaved changes prompt shows when form is dirty

**General:**
- [ ] All 9 entity forms implemented
- [ ] Consistent UI across all forms
- [ ] Responsive design (mobile, tablet, desktop)
- [ ] Accessibility: keyboard navigation, screen readers, ARIA labels
- [ ] Performance: <1s form render

---

## Dependencies

**Blocked by:**  
- Item-075 (Frontend project setup)
- Item-076 (Component library - needs TextInput, Select, FormField, Modal)
- Item-077 (Authentication)
- Item-078 (Routing)
- Item-080 (Entity detail pages - navigation back to detail)

**Blocks:**  
- None (completes MVP Phase 1)

---

## Notes

### CQRS Implementation Notes (Forms)
- Separate create vs edit flows: create → POST collection; edit → command dispatch where defined
- Implement a diff-based dispatcher: compare initial entity to edited values; send one or more commands accordingly
- For entities without command endpoints, use PATCH as defined in openapi.yaml
- Map backend 422 errors to fields consistently; show 403 with permission guidance
- Disable auto-save for command-based edits unless each change maps to a single command; otherwise keep manual submit
- After successful command(s), refetch entity and dependent relationship queries to ensure read model consistency

### General
- Use React Hook Form for form state management
- Use Zod or Yup for validation schemas
- Implement unsaved changes warning with beforeunload event
- Add auto-save draft functionality (optional, Phase 2)
- Consider multi-step forms for complex entities (optional)
- Add field-level help text with tooltips
- Implement conditional field visibility based on selections
- Add "Save as draft" functionality (optional)
- Cache relationship search results for better UX
- Validate relationships exist before submission
- Add JSON schema editor with syntax highlighting for JSON fields
- Consider adding bulk create functionality (optional, Phase 2)
