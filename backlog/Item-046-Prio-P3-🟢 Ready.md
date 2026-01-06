# Item-046: Implement Event Bus for Webhooks

- Priority: P3 (Low)
- Status: 🟢 Ready
- Owner: Platform/Integrations
- Effort: 12-16h

## Goal
Publish domain events to a message broker to support webhooks and external subscribers.

## Scope
- Choose broker (RabbitMQ/Azure Service Bus/SNS-SQS)
- Event publisher hooked to Event Store append
- Webhook projection/dispatcher to push subscribed events
- Retry/backoff and dead-letter handling

## Deliverables
- Event bus publisher module
- Webhook dispatcher leveraging existing Webhook entity
- Tests for publish/subscribe flow and retries

## Acceptance Criteria
1) New events are published to broker reliably
2) Webhooks receive subscribed events with retries
3) Failed deliveries captured in dead-letter/alerting

## References
- [Event Sourcing & Command-Based Architecture](../spec/spec-architecture-event-sourcing.md)
- [spec-schema-entities-supporting.md](../spec/spec-schema-entities-supporting.md)
