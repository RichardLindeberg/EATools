# Item-065: API Rate Limiting & Throttling

**Status:** 🟢 Ready  
**Priority:** P2 - MEDIUM  
**Effort:** 4-6 hours  
**Created:** 2026-01-07  
**Owner:** TBD

---

## Problem Statement

The specification requires rate limiting (REQ-009) to protect the API from abuse and ensure fair resource usage, but currently no rate limiting is implemented. This causes:

- No protection from DoS attacks
- Unfair resource usage (one client monopolizes server)
- No SLA enforcement for different client tiers
- Inability to prevent runaway clients
- Lack of quota management for service accounts

---

## Affected Files

**Rate Limiting:**
- Create `src/Api/Middleware/RateLimitingMiddleware.fs` - Rate limit enforcement
- Create `src/Infrastructure/RateLimiting/RateLimiter.fs` - Rate limiting logic
- Create `src/Infrastructure/RateLimiting/RateLimitStore.fs` - Track usage
- `src/Api/Program.fs` - Register rate limiting middleware

**API:**
- `src/Api/Handlers/*Handlers.fs` - Adjust endpoints for rate limits
- Update responses with rate limit headers

**Configuration:**
- `appsettings.json` - Rate limit configuration

**Tests:**
- Create `tests/RateLimitingTests.fs` - Test rate limiting

**Documentation:**
- `docs/api-rate-limiting.md` - Rate limiting documentation
- `openapi.yaml` - Document rate limit headers

---

## Specifications

- [spec/spec-tool-api-contract.md](../spec/spec-tool-api-contract.md) - REQ-009: Rate limiting

---

## Detailed Tasks

- [ ] **Define rate limiting strategy**:
  - Token bucket algorithm (flexible, industry-standard)
  - Rate limits per principal (user/service account)
  - Rate limits per endpoint (different limits for different operations)
  - Rate limits per organization
  - Backoff strategy (exponential for repeated violations)

- [ ] **Rate limit configuration**:
  - `appsettings.json` - Configure rate limits:
    ```json
    {
      "RateLimiting": {
        "Enabled": true,
        "Global": {
          "RequestsPerSecond": 1000,
          "RequestsPerMinute": 60000
        },
        "PerUser": {
          "RequestsPerSecond": 10,
          "RequestsPerMinute": 600
        },
        "PerOrganization": {
          "RequestsPerSecond": 100,
          "RequestsPerMinute": 6000
        },
        "EndpointLimits": {
          "POST /entities": { "RequestsPerMinute": 100 },
          "DELETE /entities": { "RequestsPerMinute": 50 },
          "GET /entities": { "RequestsPerMinute": 1000 }
        },
        "ServiceAccounts": {
          "RequestsPerSecond": 50,
          "RequestsPerMinute": 3000
        }
      }
    }
    ```

- [ ] **Implement rate limiter**:
  - `src/Infrastructure/RateLimiting/RateLimiter.fs`
  - Token bucket implementation:
    - Bucket capacity = max tokens
    - Refill rate = tokens per second
    - Each request consumes tokens
    - Reject if insufficient tokens
  - Support different limits for different resources
  - Support burst allowance (exceed rate briefly)

- [ ] **Rate limit storage**:
  - `src/Infrastructure/RateLimiting/RateLimitStore.fs`
  - In-memory store for local deployments
  - Redis store for distributed deployments
  - Track tokens per principal
  - Track tokens per organization
  - Clean up stale entries

- [ ] **Rate limiting middleware**:
  - `src/Api/Middleware/RateLimitingMiddleware.fs`
  - Extract principal from auth context
  - Check rate limit before processing request
  - Return 429 Too Many Requests if exceeded
  - Include rate limit headers in response

- [ ] **Rate limit response headers**:
  - Add headers to all responses:
    - X-RateLimit-Limit: Total requests allowed
    - X-RateLimit-Remaining: Requests remaining
    - X-RateLimit-Reset: When limit resets (Unix timestamp)
    - X-RateLimit-Retry-After: Seconds to wait before retry
  - On 429 response:
    - Include Retry-After header
    - Suggest exponential backoff
    - Include reset time

- [ ] **Endpoint-specific limits**:
  - More lenient for GET (read operations)
  - Stricter for POST/PUT (write operations)
  - Very strict for DELETE (dangerous operations)
  - Example:
    - GET: 1000 req/min per user
    - POST: 100 req/min per user
    - DELETE: 50 req/min per user

- [ ] **Organization-level quotas**:
  - Track usage per organization
  - Organizations can't exceed total quota
  - Distribute quota across users in org
  - Admin can adjust quotas per org
  - Alert when org approaches limit

- [ ] **Service account handling**:
  - Service accounts get higher limits
  - Separate quota from user limits
  - Monitor for abuse (spike detection)
  - Automatic throttling for runaway service accounts

- [ ] **Graceful degradation**:
  - If rate limit store unavailable, allow requests
  - Log warning when falling back
  - Don't block traffic for rate limit system failures

- [ ] **OpenAPI documentation**:
  - Document rate limit response headers
  - Document 429 response
  - Document different limits per endpoint
  - Provide rate limit headers in response examples

- [ ] **Rate limit documentation**:
  - `docs/api-rate-limiting.md`
  - Document rate limits per endpoint
  - Show retry logic examples
  - Show exponential backoff implementation
  - Document quota increase requests

- [ ] **Monitoring & alerts**:
  - Track rate limit violations per user
  - Track rate limit violations per org
  - Alert if user exceeds limits repeatedly
  - Alert if service account behavior changes
  - Dashboard showing rate limit usage

- [ ] **Test coverage**:
  - Under limit: requests succeed
  - At limit: last request succeeds
  - Over limit: returns 429
  - Rate limit headers present and correct
  - Reset time calculated correctly
  - Different limits for different endpoints
  - Service account higher limits
  - Bucket refills over time
  - Burst allowance works correctly

- [ ] **Integration with auth**:
  - Rate limiting applies per authenticated user
  - Unauthenticated requests (if allowed) have strict limits
  - IP-based fallback if auth fails
  - Service accounts tracked separately

---

## Acceptance Criteria

- [ ] Rate limiting middleware implemented
- [ ] Token bucket algorithm working
- [ ] All requests checked against rate limit
- [ ] 429 Too Many Requests returned when over limit
- [ ] Rate limit headers included in responses
- [ ] Per-user rate limits enforced
- [ ] Per-organization rate limits enforced
- [ ] Per-endpoint rate limits work correctly
- [ ] Service accounts get higher limits
- [ ] Retry-After header included on 429
- [ ] Rate limit headers present in all responses
- [ ] Endpoint-specific limits applied (stricter for DELETE)
- [ ] All tests pass (rate limiting tests)
- [ ] OpenAPI spec updated with rate limits
- [ ] Documentation complete
- [ ] Build succeeds with 0 errors, 0 warnings

---

## Dependencies

**Blocks:**
- None

**Depends On:**
- Item-064 - Authentication (rate limit per principal)

**Related:**
- Item-049 - OTel integration (rate limit metrics)
- Item-050 - Structured logging (rate limit events logged)
- Item-054 - Alerting (alert on rate limit abuse)

---

## Notes

Rate limiting is critical for API stability and security. Use token bucket algorithm for flexibility. Store rate limit state in Redis for distributed systems. Different endpoints need different limits (reads vs writes vs deletes). Monitor rate limit abuse and provide dashboards for visibility. Consider implementing API key-based tiers (free, professional, enterprise) with different limits.
