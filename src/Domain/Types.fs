/// Domain types and shared definitions for the EA Tool
namespace EATool.Domain

open System

/// ISO 8601 UTC timestamp format
type UtcTimestamp = string

/// Entity types in the system
type EntityType =
    | Organization
    | Application
    | ApplicationService
    | ApplicationInterface
    | Server
    | Integration
    | BusinessCapability
    | DataEntity
    | View

/// Lifecycle states for applications
type Lifecycle =
    | Planned
    | Active
    | Deprecated
    | Retired

/// Data classification levels
type DataClassification =
    | Public
    | Internal
    | Confidential
    | Restricted

/// Relation types between entities
type RelationType =
    | DependsOn
    | CommunicatesWith
    | Calls
    | PublishesEventTo
    | ConsumesEventFrom
    | DeployedOn
    | StoresDataOn
    | Reads
    | Writes
    | Owns
    | Supports
    | Implements
    | Realizes
    | Serves
    | ConnectedTo
    | Exposes
    | Uses

/// ArchiMate relationship types
type ArchiMateRelationship =
    | Assignment
    | Realization
    | Serving
    | Access
    | Flow
    | Triggering
    | Association
    | Composition
    | Aggregation
    | Specialization
    | Influence

/// Import/Export job status
type JobStatus =
    | Pending
    | Running
    | Completed
    | Failed

/// Application interface status
type InterfaceStatus =
    | Active
    | Deprecated
    | Retired

/// API error response
type ApiError =
    {
        Code: string
        Message: string
        TraceId: string option
    }

/// Validation error with field-level errors
type ValidationError =
    {
        Code: string
        Message: string
        TraceId: string option
        Errors: (string * string) list
    }

/// Pagination metadata
type PaginationMeta =
    {
        Page: int
        Limit: int
        Total: int
    }
