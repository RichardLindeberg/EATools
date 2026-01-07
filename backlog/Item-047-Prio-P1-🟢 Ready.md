# Item-047: Implement ApplicationService & ApplicationInterface CRUD Endpoints

**Status:** 🟢 Ready  
**Priority:** P1 - HIGH  
**Effort:** 4-5 hours  
**Created:** 2026-01-07  
**Owner:** TBD

---

## Problem Statement

ApplicationService and ApplicationInterface entities are defined in domain models and referenced by the relation validation matrix, but lack API endpoints and repositories. Clients cannot currently create, read, update, or delete these entities, blocking full service-layer modeling per ArchiMate specification.

---

## Detailed Tasks

### 1. Infrastructure Layer

#### ApplicationServiceRepository.fs
- [ ] Create `src/Infrastructure/ApplicationServiceRepository.fs`
- [ ] Implement CRUD operations:
  - `create (req: CreateApplicationServiceRequest) : ApplicationService`
  - `getAll (page: int) (limit: int) : PaginatedResponse<ApplicationService>`
  - `getById (id: string) : ApplicationService option`
  - `getByBusinessCapabilityId (capId: string) : ApplicationService list`
  - `update (id: string) (req: CreateApplicationServiceRequest) : ApplicationService option`
  - `delete (id: string) : bool`
- [ ] Add indexes on business_capability_id, created_at
- [ ] Handle JSON serialization for ExposedByAppIds, Consumers, Tags arrays

#### ApplicationInterfaceRepository.fs
- [ ] Create `src/Infrastructure/ApplicationInterfaceRepository.fs`
- [ ] Implement CRUD operations:
  - `create (req: CreateApplicationInterfaceRequest) : ApplicationInterface`
  - `getAll (page: int) (limit: int) : PaginatedResponse<ApplicationInterface>`
  - `getById (id: string) : ApplicationInterface option`
  - `getByApplicationId (appId: string) : ApplicationInterface list`
  - `update (id: string) (req: CreateApplicationInterfaceRequest) : ApplicationInterface option`
  - `delete (id: string) : bool`
- [ ] Add indexes on application_id (implied via serves_service_ids relationship), status, created_at
- [ ] Handle JSON arrays: serves_service_ids, rate_limits (if structured)

### 2. API Endpoints

#### ApplicationServicesEndpoints.fs
- [ ] Create `src/Api/ApplicationServicesEndpoints.fs`
- [ ] Implement routes:
  - `GET /application-services` — List with pagination
  - `GET /application-services?business_capability_id={id}` — Filter by capability
  - `GET /application-services/{id}` — Get by ID
  - `POST /application-services` — Create (command-based)
  - `POST /application-services/{id}/commands/update` — Update name, description, SLA, tags
  - `POST /application-services/{id}/commands/set-business-capability` — Assign capability
  - `POST /application-services/{id}/commands/add-consumer` — Add consuming app ID
  - `POST /application-services/{id}/commands/delete` — Delete
- [ ] Wire into main Endpoints.fs routing

#### ApplicationInterfacesEndpoints.fs
- [ ] Create `src/Api/ApplicationInterfacesEndpoints.fs`
- [ ] Implement routes:
  - `GET /application-interfaces` — List with pagination
  - `GET /application-interfaces?application_id={id}` — Filter by exposing app
  - `GET /application-interfaces?status={active|deprecated|retired}` — Filter by status
  - `GET /application-interfaces/{id}` — Get by ID
  - `POST /application-interfaces` — Create (command-based)
  - `POST /application-interfaces/{id}/commands/update` — Update name, protocol, endpoint, version
  - `POST /application-interfaces/{id}/commands/set-service` — Link to service(s)
  - `POST /application-interfaces/{id}/commands/deprecate` — Mark as deprecated
  - `POST /application-interfaces/{id}/commands/retire` — Mark as retired
  - `POST /application-interfaces/{id}/commands/delete` — Delete
- [ ] Wire into main Endpoints.fs routing

### 3. Command Handlers (Event-Sourced)

#### ApplicationServiceCommandHandler.fs
- [ ] Create `src/Domain/ApplicationServiceCommandHandler.fs`
- [ ] Define commands:
  - CreateApplicationService (name, description, business_capability_id, sla, exposed_by_app_ids, consumers, tags)
  - UpdateApplicationService (name, description, sla, tags)
  - SetBusinessCapability (business_capability_id)
  - AddConsumer (app_id)
  - RemoveConsumer (app_id)
  - DeleteApplicationService
- [ ] Define events:
  - ApplicationServiceCreated
  - ApplicationServiceUpdated
  - BusinessCapabilitySet
  - ConsumerAdded
  - ConsumerRemoved
  - ApplicationServiceDeleted
- [ ] Implement handlers with validation:
  - Name required, max 255 chars
  - Description optional, max 2000 chars
  - Business capability ID must reference existing capability
  - SLA optional
  - Exposed by app IDs must reference existing applications
  - Consumer IDs must reference existing applications

#### ApplicationInterfaceCommandHandler.fs
- [ ] Create `src/Domain/ApplicationInterfaceCommandHandler.fs`
- [ ] Define commands:
  - CreateApplicationInterface (name, protocol, endpoint, specification_url, version, authentication_method, serves_service_ids)
  - UpdateApplicationInterface (name, protocol, endpoint, version)
  - SetServedServices (service_ids)
  - SetStatus (status: active|deprecated|retired)
  - DeleteApplicationInterface
- [ ] Define events:
  - ApplicationInterfaceCreated
  - ApplicationInterfaceUpdated
  - ServedServicesSet
  - StatusChanged
  - ApplicationInterfaceDeleted
- [ ] Implement handlers with validation:
  - Name required, max 255 chars
  - Protocol required (e.g., REST, SOAP, gRPC, GraphQL)
  - Endpoint optional but if provided, must be valid URL/path
  - Specification URL optional, must be valid URL if provided
  - Version required (e.g., v1, v2.0)
  - Authentication method optional (e.g., OAuth2, API-Key, mTLS)
  - Served service IDs must reference existing services

### 4. JSON Serialization

- [ ] Add encoders/decoders in `src/Infrastructure/Json.fs`:
  - `encodeApplicationService`, `decodeApplicationService`
  - `encodeApplicationInterface`, `decodeApplicationInterface`
  - Handle arrays: ExposedByAppIds, Consumers, ServesServiceIds, RateLimits

### 5. Integration Tests

#### test_application_services.py
- [ ] Create `tests/integration/test_application_services.py`
- [ ] Test CRUD:
  - POST /application-services → 200/201, returns id
  - GET /application-services → 200, paginated list
  - GET /application-services/{id} → 200 or 404
  - POST /application-services/{id}/commands/update → 200
  - POST /application-services/{id}/commands/set-business-capability → 200
  - POST /application-services/{id}/commands/add-consumer → 200
  - POST /application-services/{id}/commands/delete → 200
- [ ] Test filtering by business_capability_id
- [ ] Test validation:
  - Missing name → 422
  - Invalid business_capability_id → 422
  - Invalid consumer app ID → 422

#### test_application_interfaces.py
- [ ] Create `tests/integration/test_application_interfaces.py`
- [ ] Test CRUD:
  - POST /application-interfaces → 200/201, returns id
  - GET /application-interfaces → 200, paginated list
  - GET /application-interfaces/{id} → 200 or 404
  - POST /application-interfaces/{id}/commands/update → 200
  - POST /application-interfaces/{id}/commands/set-service → 200
  - POST /application-interfaces/{id}/commands/deprecate → 200, status=deprecated
  - POST /application-interfaces/{id}/commands/retire → 200, status=retired
  - POST /application-interfaces/{id}/commands/delete → 200
- [ ] Test filtering by application_id and status
- [ ] Test validation:
  - Missing name/protocol/version → 422
  - Invalid service ID → 422
  - Invalid status transition (e.g., active→planned) → 422

### 6. OpenAPI Updates

- [ ] Update `openapi.yaml`:
  - Add `/application-services` endpoints and schemas
  - Add `/application-interfaces` endpoints and schemas
  - Include command payloads
  - Reference ApplicationService and ApplicationInterface components

### 7. Documentation

- [ ] Update `docs/entity-guide.md`:
  - Add ApplicationService section with attributes and usage
  - Add ApplicationInterface section with attributes and usage
  - Link to relationship-modeling for service-layer patterns
- [ ] Update `docs/relationship-modeling.md`:
  - Add examples: application → application_service (realizes, uses)
  - Add examples: application → application_interface (exposes)
  - Add examples: application_interface → application_service (serves)

---

## Acceptance Criteria

- [x] ApplicationService repository implemented with full CRUD
- [x] ApplicationInterface repository implemented with full CRUD
- [x] Command-based endpoints for both entities
- [x] Event sourcing: commands → events → projections
- [x] All validation rules enforced (required fields, ID references)
- [x] Integration tests: 16+ tests covering CRUD, filtering, validation
- [x] OpenAPI updated with all endpoints and schemas
- [x] Documentation updated with examples and usage patterns
- [x] All tests passing: 89 + 16 = 105 total
- [x] Build succeeds without warnings

---

## Dependencies

**Depends On:**
- Item-028 (Event Store Infrastructure) ✅ Done
- Item-029 (Command Framework) ✅ Done
- Item-030 (CQRS Projection Framework) ✅ Done

**Blocks:**
- None (Item-015 expansion uses these in relation tests)

**Related:**
- [Item-037](Item-037-Prio-P2-🟢%20Ready.md) - Server endpoints migration (similar pattern)
- [Item-038](Item-038-Prio-P2-🟢%20Ready.md) - Integration endpoints migration
- [Item-039](Item-039-Prio-P2-🟢%20Ready.md) - DataEntity endpoints migration
- [docs/relationship-modeling.md](../../docs/relationship-modeling.md) - Service-layer patterns
- [spec/spec-schema-domain-model.md](../../spec/spec-schema-domain-model.md) - Entity definitions

---

## Definition of Done

- [ ] All code committed to feature branch
- [ ] All tests passing (integration + unit)
- [ ] Build succeeds (dotnet build)
- [ ] Code review completed
- [ ] Branch merged to main
- [ ] Documentation updated and committed
- [ ] OpenAPI in sync with implementation

---

## Notes

- Both entities follow the same command-based, event-sourced pattern as Relations, Applications, and Organizations
- ApplicationService.ExposedByAppIds tracks which apps realize/use this service
- ApplicationInterface.ServesServiceIds tracks which services this interface serves
- Status field for ApplicationInterface allows lifecycle tracking (active → deprecated → retired)
- Consider adding "Link Service" as a separate command to support application_interface → application_service relationship
- Mirrors existing endpoint patterns (RelationsEndpoints, ApplicationsEndpoints) for consistency

---

## History

| Date | Event |
|------|-------|
| 2026-01-07 | Created item |

