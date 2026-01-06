## Plan: Event Sourcing with Solace/MQTT Integration

Migrate from mutable state CRUD to event-sourced architecture with Solace/MQTT publishing for comprehensive audit trails and real-time event distribution to external systems.

### Steps

1. **Add event store infrastructure**: Create [events table](src/Infrastructure/Migrations/008_create_events.sql) with (stream_id, event_type, event_data, metadata, version, timestamp, correlation_id); implement [EventRepository.fs](src/Infrastructure/EventRepository.fs) with `appendEvent` and `getEvents` methods; add version columns to existing entity tables for optimistic locking.

2. **Define domain events**: Create [Events.fs](src/Domain/Events.fs) with discriminated unions per aggregate (`ApplicationEvent`, `OrganizationEvent`, etc.) covering Created/Updated/Deleted/StateChanged events; add Thoth JSON encoders/decoders in [Json.fs](src/Infrastructure/Json.fs); design event schemas to include full entity snapshots plus metadata (actor, timestamp, correlation_id).

3. **Integrate Solace messaging**: Create [SolacePublisher.fs](src/Infrastructure/SolacePublisher.fs) module wrapping Solace .NET SDK; configure connection (broker URL, VPN, credentials) via appsettings; implement `publishEvent` with topic pattern `eatool/{entity-type}/{event-type}/{entity-id}`; add retry logic and circuit breaker for resilience; support both Solace and MQTT protocols.

4. **Implement hybrid dual-write**: Modify repositories in [ApplicationRepository.fs](src/Infrastructure/ApplicationRepository.fs) (and all others) to: (a) append event to event store, (b) update projection table (current tables), (c) publish to Solace - all within try-catch to maintain backward compatibility; add feature flag `EVENT_SOURCING_ENABLED` to toggle behavior; maintain synchronous API responses during transition.

5. **Add projection rebuilders**: Create [ProjectionHandlers.fs](src/Infrastructure/ProjectionHandlers.fs) with event handlers that apply events to read models; implement `/admin/projections/{entity}/rebuild` endpoints to replay event streams; add background service to process events asynchronously for eventual consistency mode (future phase).

6. **Update API for command tracking**: Add `X-Correlation-Id` header support in [Program.fs](src/Program.fs); return command IDs in 202 Accepted responses for async operations; create `/commands/{id}/status` endpoint to query processing state; maintain backward-compatible 201 sync responses via feature flag.

### Further Considerations

1. **Event store choice**: SQLite with [008_create_events.sql](src/Infrastructure/Migrations/008_create_events.sql) for simplicity and consistency with current stack, or EventStoreDB for advanced features (subscriptions, projections, clustering)? Recommend SQLite initially to minimize operational complexity.

2. **Solace vs MQTT**: Solace PubSub+ provides enterprise features (guaranteed delivery, DMQ, replay) vs lightweight MQTT brokers (Mosquitto, HiveMQ); plan supports both - abstract behind `IMessagePublisher` interface; which protocols do downstream consumers need?

3. **Migration phases**: Phase 1 (dual-write with current API), Phase 2 (projection-only writes), Phase 3 (async commands with 202 responses), Phase 4 (retire old tables)? Each phase can run for weeks/months; define success criteria and rollback plans.

4. **Consistency guarantees**: Immediate consistency (event store write + projection update in transaction) vs eventual consistency (async projection); initial plan uses immediate consistency with try-catch fallback; how critical is projection lag for your use cases?

5. **Topic design for Solace**: Hierarchical topics like `eatool/v1/{env}/{entity-type}/{event-type}/{entity-id}` for filtering; include environment (dev/staging/prod) in topic to isolate streams; support wildcard subscriptions (`eatool/v1/prod/applications/>`); define retention policy and TTL?

6. **Event versioning strategy**: Schema evolution for events - use version field in metadata, maintain backward-compatible decoders, or support multiple event versions simultaneously? Recommend semantic versioning in event_type (e.g., `ApplicationCreated.v1`, `ApplicationCreated.v2`).

7. **Audit query API**: Expose `/entities/{type}/{id}/history` endpoint to retrieve event stream for audit purposes; add filtering by date range, actor, event type; include in OpenAPI spec with proper authorization (sensitive audit data).

8. **Testing strategy**: Add [integration/test_event_sourcing.py](tests/integration/test_event_sourcing.py) to verify event production, Solace publishing, and projection consistency; mock Solace in unit tests; how to handle eventual consistency in test assertions (polling with timeout)?