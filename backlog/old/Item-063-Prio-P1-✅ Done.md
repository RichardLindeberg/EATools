# Item-063: Standardized API Error Response Format

**Status:** 🟢 Ready  
**Priority:** P1 - HIGH  
**Effort:** 4-6 hours  
**Created:** 2026-01-07  
**Owner:** TBD

---

## Problem Statement

The specification requires standardized error responses (REQ-006) with consistent error codes and messages, but currently error responses are inconsistent across the API. This causes:

- Inconsistent error handling in clients
- Difficult debugging and error tracking
- Unclear error messages to API consumers
- Problems with observability and error categorization
- Non-compliance with API contract

Current issues:
- Some endpoints return 500 for validation errors instead of 400
- Error messages vary in format and content
- No error codes for categorization
- No request ID linking for debugging

---

## Affected Files

**API:**
- `src/Api/Responses/ErrorResponse.fs` - Standardized error response format
- `src/Api/Middleware/ErrorHandlingMiddleware.fs` - Central error handling
- `src/Api/Handlers/*Handlers.fs` - All handlers use standardized responses

**Tests:**
- Create `tests/ErrorResponseTests.fs` - Test error response format
- `tests/*CommandTests.fs` - Update tests for standard error responses

**Documentation:**
- `openapi.yaml` - Define error response schema
- `docs/api-error-codes.md` - Document all error codes and meanings
- `spec/spec-tool-api-contract.md` - Update with error response format

---

## Specifications

- [spec/spec-tool-api-contract.md](../spec/spec-tool-api-contract.md) - REQ-006: Error response format
- [openapi.yaml](../openapi.yaml) - Add error schemas

---

## Detailed Tasks

- [ ] **Create error response model**:
  - `src/Api/Responses/ErrorResponse.fs`
  - Type ErrorResponse:
    ```fsharp
    {
      error_code: string          // e.g., "VALIDATION_ERROR", "NOT_FOUND"
      message: string             // user-friendly message
      details: string option      // additional context
      request_id: string          // trace ID for debugging
      timestamp: System.DateTime  // when error occurred
      path: string               // API endpoint that failed
      field_errors: FieldError[] option  // validation errors per field
    }
    ```
  - Type FieldError:
    ```fsharp
    {
      field: string
      code: string
      message: string
    }
    ```

- [ ] **Define error codes**:
  - Create `src/Api/ErrorCodes.fs` with all standard error codes:
    - VALIDATION_ERROR - Input validation failed
    - NOT_FOUND - Entity not found
    - ALREADY_EXISTS - Duplicate entity
    - FORBIDDEN - Access denied
    - CONFLICT - State conflict (e.g., can't transition state)
    - UNAUTHORIZED - Authentication required
    - INTERNAL_ERROR - Unexpected server error
    - INVALID_STATE_TRANSITION - Lifecycle state transition invalid
    - CONSTRAINT_VIOLATION - Database constraint violated
    - CIRCULAR_REFERENCE - Cycle detected in relationships

- [ ] **Implement error handling middleware**:
  - `src/Api/Middleware/ErrorHandlingMiddleware.fs`
  - Catch all exceptions and convert to ErrorResponse
  - Map domain errors to appropriate HTTP status codes:
    - ValidationError → 400 Bad Request
    - NotFoundError → 404 Not Found
    - ConflictError → 409 Conflict
    - StateTransitionError → 422 Unprocessable Entity
    - UnauthorizedError → 401 Unauthorized
    - ForbiddenError → 403 Forbidden
    - Unexpected error → 500 Internal Server Error
  - Extract request ID from header or generate new one
  - Include request ID in error response for tracing
  - Log error with full context (severity, request ID, stack trace)

- [ ] **Update all handlers**:
  - All command handlers return standardized error responses
  - Use error codes from ErrorCodes.fs
  - Include field-level errors for validation failures
  - Example:
    ```fsharp
    Result.mapError (fun err ->
      ErrorResponse.Create(
        errorCode = "VALIDATION_ERROR",
        message = "Invalid request",
        fieldErrors = [
          { field = "owner", code = "REQUIRED", message = "Owner is required" }
        ],
        requestId = ctx.TraceId
      )
    )
    ```

- [ ] **Field-level error messages**:
  - Validation errors include which field failed
  - Error codes for common validation failures:
    - REQUIRED - Field is required
    - FORMAT - Invalid format (e.g., email)
    - RANGE - Value out of range
    - DUPLICATE - Value already exists
    - INVALID_STATE_TRANSITION - Can't transition to this state
    - CONSTRAINT_VIOLATED - Constraint violation

- [ ] **HTTP status codes**:
  - Audit endpoints return correct status:
    - 400 Bad Request - Validation error
    - 401 Unauthorized - Missing authentication
    - 403 Forbidden - Insufficient permissions
    - 404 Not Found - Entity doesn't exist
    - 409 Conflict - Duplicate or state conflict
    - 422 Unprocessable Entity - Invalid state transition
    - 500 Internal Server Error - Unexpected error

- [ ] **Request ID tracking**:
  - Add X-Request-ID header support
  - Generate UUID if not provided
  - Include in error response for debugging
  - Log with request ID for tracing
  - Make request ID available in context

- [ ] **Update OpenAPI specification**:
  - Add standardized error response schema
  - Document error response for each endpoint
  - List possible error codes for each operation
  - Example error response in spec:
    ```yaml
    400:
      description: Validation error
      content:
        application/json:
          schema:
            $ref: '#/components/schemas/ErrorResponse'
    ```

- [ ] **Create error codes documentation**:
  - `docs/api-error-codes.md`
  - List all error codes with:
    - Description
    - HTTP status code
    - Example field errors
    - How to fix
  - Example:
    ```markdown
    ### VALIDATION_ERROR
    - HTTP: 400 Bad Request
    - Description: Input validation failed
    - Example: Missing required field
    - Fix: Provide all required fields with valid values
    ```

- [ ] **Test coverage**:
  - Validation errors return 400 with field-level errors
  - Not found errors return 404
  - State transition errors return 422
  - Conflict errors return 409
  - Unexpected errors return 500
  - Request ID included in all error responses
  - Error response includes timestamp
  - Error message is user-friendly
  - Field errors include field name, code, message
  - Sensitive data not exposed in error messages

- [ ] **Client-side guidance**:
  - Document error handling patterns in spec
  - Show how to handle each error code
  - Show retry logic for 500 errors
  - Show validation error handling example

---

## Acceptance Criteria

- [ ] StandardErrorResponse created with all required fields
- [ ] ErrorCodes.fs defines all standard error codes
- [ ] ErrorHandlingMiddleware catches and converts exceptions
- [ ] All handlers return standardized error responses
- [ ] Validation errors include field-level details
- [ ] HTTP status codes are correct for each error type
- [ ] Request ID header supported and included in responses
- [ ] Timestamp included in all error responses
- [ ] OpenAPI spec includes error response schema
- [ ] Error codes documentation created
- [ ] All tests pass (including error response tests)
- [ ] No sensitive data exposed in error messages
- [ ] Build succeeds with 0 errors, 0 warnings

---

## Dependencies

**Blocks:**
- None (but improves other items)

**Depends On:**
- None

**Related:**
- Item-049 - OTel Integration (error observability)
- Item-050 - Structured Logging (error logging)
- All API endpoints benefit from standardized errors

---

## Notes

This is foundational for good API design. Standardized error responses improve client experience, debugging, and observability. Ensure error messages are user-friendly but also informative for debugging. Include request ID for correlation with logs. Don't expose sensitive information in error messages (stack traces, internal paths, etc.). Consider creating error handling utilities to reduce boilerplate in handlers.
