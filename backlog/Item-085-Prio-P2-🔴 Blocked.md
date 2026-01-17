# Item-085: Implement WebSocket Real-Time Updates

**Status:** � Blocked  
**Priority:** P2 - MEDIUM  
**Effort:** 32-48 hours  
**Created:** 2026-01-17  
**Owner:** Full-Stack Team

---

## Problem Statement

Currently, users must manually refresh pages to see updates made by other users. For collaborative enterprise architecture work, real-time updates are essential so teams can see changes as they happen.

WebSocket support enables live notifications when entities are created, updated, or deleted, allowing multiple users to collaborate effectively without constantly refreshing pages as specified in [spec-ui-api-integration.md](../spec/spec-ui-api-integration.md).

This is a Phase 2 advanced feature identified in the FRONTEND-READINESS-REPORT.md.

---

## Affected Files

**Backend:**
- `src/EATool/WebSockets/WebSocketTypes.fs` - WebSocket message types
- `src/EATool/WebSockets/WebSocketHandler.fs` - WebSocket connection handler
- `src/EATool/WebSockets/SubscriptionManager.fs` - Manage client subscriptions
- `src/EATool/Events/EventBroadcaster.fs` - Broadcast events to WebSocket clients
- `src/EATool/Program.fs` - Add WebSocket middleware

**Frontend:**
- `frontend/src/api/websocketClient.ts` - WebSocket client implementation
- `frontend/src/hooks/useWebSocket.ts` - WebSocket hook
- `frontend/src/hooks/useEntitySubscription.ts` - Entity subscription hook
- `frontend/src/contexts/WebSocketContext.tsx` - WebSocket context provider
- `frontend/src/components/notifications/LiveUpdateNotification.tsx` - Live update toast

---

## Specifications

- [spec/spec-ui-api-integration.md](../spec/spec-ui-api-integration.md) - WebSocket requirements
- [spec/spec-ui-advanced-patterns.md](../spec/spec-ui-advanced-patterns.md) - Real-time update patterns
- [FRONTEND-READINESS-REPORT.md](../FRONTEND-READINESS-REPORT.md) - Phase 2 feature identification

---

## Detailed Tasks

### Phase 1: Backend WebSocket Infrastructure (12-16 hours)

**WebSocket Endpoint:**
- [ ] Create WebSocket endpoint: ws://localhost:8000/ws or wss:// for production
- [ ] Implement WebSocket connection handler (accept, maintain, close)
- [ ] Add authentication check (require valid JWT token in connection request)
- [ ] Implement heartbeat/ping-pong to keep connections alive
- [ ] Handle connection errors and disconnections gracefully
- [ ] Add connection logging (connect, disconnect, errors)

**Message Types:**
- [ ] Define message types: SUBSCRIBE, UNSUBSCRIBE, EVENT_NOTIFICATION
- [ ] Define subscription scopes: entity_type (e.g., "applications"), entity_id (e.g., "app123"), all
- [ ] Define event types: ENTITY_CREATED, ENTITY_UPDATED, ENTITY_DELETED
- [ ] Create message serialization (JSON)

**Subscription Manager:**
- [ ] Create SubscriptionManager to track client subscriptions
- [ ] Implement subscribe function (client can subscribe to entity types, specific entities, or all)
- [ ] Implement unsubscribe function
- [ ] Store subscriptions in memory (map of connectionId → subscriptions)
- [ ] Clean up subscriptions on disconnect

**Event Broadcasting:**
- [ ] Integrate with event sourcing system
- [ ] When entity created/updated/deleted, broadcast to subscribed clients
- [ ] Filter events based on client subscriptions
- [ ] Include event data: entity_type, entity_id, action, timestamp, payload
- [ ] Respect permission model (don't send events user can't access)

### Phase 2: Frontend WebSocket Client (10-14 hours)

**WebSocket Client:**
- [ ] Create WebSocket client class with connect/disconnect methods
- [ ] Implement automatic reconnection with exponential backoff (1s, 2s, 4s, 8s, max 30s)
- [ ] Add JWT token in WebSocket connection handshake (query param or header)
- [ ] Implement message sending (subscribe, unsubscribe)
- [ ] Implement message receiving and parsing
- [ ] Add connection state tracking (connecting, connected, disconnected, error)
- [ ] Implement heartbeat response (pong)

**WebSocket Context:**
- [ ] Create WebSocketContext to provide connection to all components
- [ ] Initialize WebSocket on app load (after authentication)
- [ ] Clean up WebSocket on app unmount
- [ ] Provide connection status to components
- [ ] Handle reconnection on token refresh

**Hooks:**
- [ ] Create useWebSocket hook to access context
- [ ] Create useEntitySubscription hook (subscribe to entity type or specific entity)
- [ ] Automatically subscribe/unsubscribe based on active page
- [ ] Handle incoming events and trigger UI updates

### Phase 3: UI Integration (10-14 hours)

**Entity List Pages:**
- [ ] Subscribe to entity type when list page loads (e.g., "applications")
- [ ] On ENTITY_CREATED event, add new entity to list (or show "New item" toast)
- [ ] On ENTITY_UPDATED event, update entity in list
- [ ] On ENTITY_DELETED event, remove entity from list
- [ ] Unsubscribe when leaving list page

**Entity Detail Pages:**
- [ ] Subscribe to specific entity when detail page loads (e.g., "applications/123")
- [ ] On ENTITY_UPDATED event, refresh entity data or show "Updated by another user" toast
- [ ] On ENTITY_DELETED event, show "Entity deleted" message and redirect to list
- [ ] Unsubscribe when leaving detail page

**Notifications:**
- [ ] Create LiveUpdateNotification component (toast style)
- [ ] Show notification when entity is updated by another user
- [ ] Add "Refresh" button to immediately reload data
- [ ] Add "Dismiss" button to ignore update
- [ ] Auto-dismiss after 10 seconds

**Conflict Handling:**
- [ ] If user is editing an entity and receives ENTITY_UPDATED event, show conflict warning
- [ ] Prompt user to refresh or continue editing (risk overwriting changes)
- [ ] Integrate with conflict resolution UI from Item-082

---

## Acceptance Criteria

**Backend:**
- [ ] WebSocket endpoint accepts connections (ws://localhost:8000/ws)
- [ ] Connections require valid JWT token
- [ ] Clients can subscribe to entity types, specific entities, or all events
- [ ] Clients can unsubscribe from subscriptions
- [ ] Events broadcast to subscribed clients when entities change
- [ ] Events filtered by client subscriptions
- [ ] Events respect user permissions (no unauthorized access)
- [ ] Connections auto-close after inactivity or on client disconnect
- [ ] Heartbeat keeps connections alive

**Frontend:**
- [ ] WebSocket client connects on app load (after authentication)
- [ ] Client automatically reconnects on disconnect (with backoff)
- [ ] JWT token included in WebSocket connection
- [ ] Token refresh triggers WebSocket reconnection
- [ ] Components can subscribe to entity types or specific entities
- [ ] Subscriptions automatically cleaned up on unmount

**UI Updates:**
- [ ] List pages show new entities in real-time
- [ ] List pages update entities in real-time
- [ ] List pages remove deleted entities in real-time
- [ ] Detail pages show "Updated by another user" notification
- [ ] Detail pages handle entity deletion (redirect to list)
- [ ] Notifications shown with refresh/dismiss options
- [ ] Conflict warnings shown when editing entity being updated

**Performance:**
- [ ] WebSocket connection does not impact page load time
- [ ] Reconnection does not cause UI jank
- [ ] Events processed efficiently (no lag)
- [ ] No memory leaks from subscriptions

**Reliability:**
- [ ] WebSocket survives network interruptions
- [ ] WebSocket reconnects after token refresh
- [ ] No events lost during reconnection
- [ ] Graceful degradation if WebSocket unavailable (fallback to polling or manual refresh)

---

## Dependencies

**Blocked by:**  
- Item-075 (Frontend project setup)
- Item-077 (Authentication for WebSocket)
- Items 079-081 (Entity pages to integrate with)

**Blocks:** None (enhances existing features)

---

## Notes

- **Phase 2 Feature:** Can be implemented after MVP Phase 1
- Use SignalR for F# if available, or raw WebSocket
- Consider using Socket.IO for frontend (auto-reconnection, fallback)
- Test with multiple concurrent users
- Test reconnection scenarios (network drops, server restarts)
- Consider adding rate limiting to prevent spam subscriptions
- Add WebSocket connection monitoring (Grafana, Prometheus)
- Consider using server-sent events (SSE) as simpler alternative
- Test with slow connections (throttled network)
- Add WebSocket handshake logging for debugging
- Consider adding WebSocket compression for bandwidth savings
- Test scale: 100+ concurrent WebSocket connections
