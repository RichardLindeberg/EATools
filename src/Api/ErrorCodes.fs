/// Standard error codes for API responses
module EATool.Api.ErrorCodes

/// Input validation failed
let VALIDATION_ERROR = "VALIDATION_ERROR"

/// Entity not found
let NOT_FOUND = "NOT_FOUND"

/// Duplicate entity already exists
let ALREADY_EXISTS = "ALREADY_EXISTS"

/// Access denied - insufficient permissions
let FORBIDDEN = "FORBIDDEN"

/// State conflict (e.g., can't transition state)
let CONFLICT = "CONFLICT"

/// Authentication required
let UNAUTHORIZED = "UNAUTHORIZED"

/// Unexpected server error
let INTERNAL_ERROR = "INTERNAL_ERROR"

/// Invalid lifecycle state transition
let INVALID_STATE_TRANSITION = "INVALID_STATE_TRANSITION"

/// Database constraint violated
let CONSTRAINT_VIOLATION = "CONSTRAINT_VIOLATION"

/// Circular reference detected in relationships
let CIRCULAR_REFERENCE = "CIRCULAR_REFERENCE"

/// Field validation error codes
module FieldValidation =
    /// Field is required but missing
    let REQUIRED = "REQUIRED"
    
    /// Invalid format (e.g., email, date)
    let FORMAT = "FORMAT"
    
    /// Value out of valid range
    let RANGE = "RANGE"
    
    /// Value already exists (duplicate)
    let DUPLICATE = "DUPLICATE"
    
    /// Constraint violation
    let CONSTRAINT_VIOLATED = "CONSTRAINT_VIOLATED"
