/// Standardized error response types for API endpoints
module EATool.Api.ErrorResponse

open System

/// Field-level validation error
type FieldError = {
    Field: string
    Code: string
    Message: string
}

/// Standardized API error response
type ErrorResponse = {
    ErrorCode: string
    Message: string
    Details: string option
    RequestId: string
    Timestamp: DateTime
    Path: string
    FieldErrors: FieldError list option
}

/// Create error response with required fields
let create errorCode message requestId path =
    {
        ErrorCode = errorCode
        Message = message
        Details = None
        RequestId = requestId
        Timestamp = DateTime.UtcNow
        Path = path
        FieldErrors = None
    }

/// Create error response with details
let withDetails details errorResponse =
    { errorResponse with Details = Some details }

/// Create error response with field errors
let withFieldErrors fieldErrors errorResponse =
    { errorResponse with FieldErrors = Some fieldErrors }

/// Create validation error with field-level details
let validationError message fieldErrors requestId path =
    create "VALIDATION_ERROR" message requestId path
    |> withFieldErrors fieldErrors

/// Create not found error
let notFoundError entityType entityId requestId path =
    create "NOT_FOUND" $"{entityType} not found" requestId path
    |> withDetails $"No {entityType} found with ID: {entityId}"

/// Create conflict error
let conflictError message details requestId path =
    create "CONFLICT" message requestId path
    |> withDetails details

/// Create internal error
let internalError message requestId path =
    create "INTERNAL_ERROR" "An internal server error occurred" requestId path
    |> withDetails message
