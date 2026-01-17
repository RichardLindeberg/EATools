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

**Form Infrastructure:**
- [ ] Setup React Hook Form for all forms
- [ ] Create form validation schemas (Zod or Yup)
- [ ] Implement server-side validation error mapping (backend errors → form fields)
- [ ] Create form submission handler with loading states
- [ ] Implement optimistic updates pattern
- [ ] Add auto-save functionality (500ms debounce, optional)

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

### Phase 2: Application Form (8-10 hours)
- [ ] Create ApplicationFormPage (handles /applications/new and /applications/:id/edit)
- [ ] Add fields: Name* (text), Description (textarea), Type* (select), Status* (select)
- [ ] Add fields: Version (text), Owner* (select user), URL (text, URL validation)
- [ ] Add validation: Required fields, URL format, max lengths
- [ ] Add relationship selector: Servers (multi-select), Integrations (multi-select)
- [ ] Handle create (POST /applications)
- [ ] Handle edit (PATCH /applications/{id})
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
- [ ] Handle edit (PATCH /servers/{id})

### Phase 4: Integration Form (8-10 hours)
- [ ] Create IntegrationFormPage
- [ ] Add fields: Name* (text), Type* (select), Protocol* (select)
- [ ] Add fields: Source* (select application), Target* (select application)
- [ ] Add fields: Status* (select), Configuration (JSON editor)
- [ ] Add validation: Required fields, valid JSON for configuration
- [ ] Add relationship selector: DataEntities (multi-select)
- [ ] Handle create (POST /integrations)
- [ ] Handle edit (PATCH /integrations/{id})

### Phase 5: DataEntity Form (7-9 hours)
- [ ] Create DataEntityFormPage
- [ ] Add fields: Name* (text), Type* (select), Format* (select)
- [ ] Add fields: Classification* (select), Schema (JSON editor), Owner* (select user)
- [ ] Add validation: Required fields, valid JSON for schema
- [ ] Handle create (POST /data-entities)
- [ ] Handle edit (PATCH /data-entities/{id})

### Phase 6: BusinessCapability Form (7-9 hours)
- [ ] Create BusinessCapabilityFormPage
- [ ] Add fields: Name* (text), Description (textarea), Level* (select)
- [ ] Add fields: Parent (select business capability), Owner* (select user), Status* (select)
- [ ] Add validation: Required fields, prevent circular parent references
- [ ] Add relationship selector: Applications (multi-select), Organizations (multi-select)
- [ ] Handle create (POST /business-capabilities)
- [ ] Handle edit (PATCH /business-capabilities/{id})

### Phase 7: Organization Form (6-8 hours)
- [ ] Create OrganizationFormPage
- [ ] Add fields: Name* (text), Type* (select), Parent (select organization)
- [ ] Add fields: Owner* (select user), Contact (email, email validation)
- [ ] Add validation: Required fields, email format, prevent circular parent references
- [ ] Handle create (POST /organizations)
- [ ] Handle edit (PATCH /organizations/{id})

### Phase 8: Relation Form (6-8 hours)
- [ ] Create RelationFormPage
- [ ] Add fields: Type* (select), Source Entity* (dynamic entity selector)
- [ ] Add fields: Target Entity* (dynamic entity selector), Metadata (JSON editor)
- [ ] Add validation: Required fields, valid JSON for metadata
- [ ] Implement dynamic entity selector (select entity type, then select specific entity)
- [ ] Handle create (POST /relations)
- [ ] Handle edit (PATCH /relations/{id})

### Phase 9: ApplicationService Form (6-8 hours)
- [ ] Create ApplicationServiceFormPage
- [ ] Add fields: Name* (text), Application* (select application), Protocol* (select)
- [ ] Add fields: Endpoint (text, URL validation), Status* (select)
- [ ] Add validation: Required fields, URL format
- [ ] Handle create (POST /application-services)
- [ ] Handle edit (PATCH /application-services/{id})

### Phase 10: ApplicationInterface Form (7-9 hours)
- [ ] Create ApplicationInterfaceFormPage
- [ ] Add fields: Name* (text), Type* (select), Protocol* (select)
- [ ] Add fields: Source Application* (select), Target Application* (select), Status* (select)
- [ ] Add validation: Required fields, source ≠ target
- [ ] Add relationship selector: ApplicationServices (multi-select)
- [ ] Handle create (POST /application-interfaces)
- [ ] Handle edit (PATCH /application-interfaces/{id})

---

## Acceptance Criteria

**For Each Entity Form:**
- [ ] Form displays in create mode (/entities/new) and edit mode (/entities/:id/edit)
- [ ] All required fields marked with asterisk (*)
- [ ] Form validation matches backend requirements
- [ ] Client-side validation runs on blur and on submit
- [ ] Server-side validation errors mapped to form fields
- [ ] Required fields show error if empty
- [ ] Format validation (email, URL, IP, JSON) works correctly
- [ ] Relationship selectors allow searching and selecting related entities
- [ ] Submit button disabled during submission
- [ ] Loading state shown during submission
- [ ] Success message shown after successful create/edit
- [ ] Redirect to detail page after successful submission
- [ ] Cancel button navigates back to previous page
- [ ] Unsaved changes prompt shown if navigating away with dirty form

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
