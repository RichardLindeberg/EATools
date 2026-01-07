# Item-054: Observability Alert Rules & Incident Response

**Status:** 🟢 Ready  
**Priority:** P2 - MEDIUM  
**Effort:** 6-8 hours  
**Created:** 2026-01-07  
**Owner:** TBD

---

## Problem Statement

Metrics and logs are only useful if we act on them. Without alert rules:
- High error rates go unnoticed (5xx errors)
- Performance regressions are not detected (latency spike)
- System capacity issues are discovered too late (high resource usage)
- Projection lag accumulates before anyone knows (CQRS consistency loss)
- On-call teams have no structured incident response (ALR-001 through ALR-005 violations)

This violates alerting requirements and leaves the system running degraded until users complain.

---

## Affected Files

**Create:**
- `ops/prometheus-alerts.yml` - Prometheus alert rules for all critical paths
- `ops/alert-rules/README.md` - Documentation of all alerts, thresholds, runbooks
- `docs/runbook-high-error-rate.md` - Incident response runbook for high error rates
- `docs/runbook-high-latency.md` - Incident response runbook for latency spikes
- `docs/runbook-projection-lag.md` - Incident response runbook for CQRS consistency
- `docs/runbook-event-store-failure.md` - Incident response runbook for event store issues

---

## Specifications

- [spec/spec-process-observability.md](../spec/spec-process-observability.md) - Section 3.6 (alerting requirements)

---

## Detailed Tasks

- [ ] Define alert rules in `ops/prometheus-alerts.yml`:
  - **Critical HTTP Error Rate**: Alert if 5xx error rate > 1% over 5 minutes
    - Severity: critical, Route to: backend-on-call
    - Runbook: docs/runbook-high-error-rate.md
  - **High Latency**: Alert if p99 HTTP latency > 500ms over 5 minutes
    - Severity: high, Route to: backend-on-call
    - Runbook: docs/runbook-high-latency.md
  - **High Command Error Rate**: Alert if command failure rate > 5% over 5 minutes
    - Severity: high, Route to: backend-on-call
  - **Event Store Append Slow**: Alert if event store append p99 > 1000ms
    - Severity: high, Route to: infrastructure-on-call
  - **Projection Lag High**: Alert if any projection lag > 1000 events
    - Severity: medium, Route to: backend-on-call
    - Runbook: docs/runbook-projection-lag.md
  - **Event Store Down**: Alert if event store is unreachable
    - Severity: critical, Route to: infrastructure-on-call
    - Runbook: docs/runbook-event-store-failure.md
  - **OTel Exporter Failures**: Alert if traces/metrics failed to export > 5 times in 5 minutes
    - Severity: medium, Route to: platform-on-call

- [ ] Configure alert notification routing:
  - Use Prometheus AlertManager or equivalent
  - Route critical alerts to immediate Slack notification
  - Route high alerts to daily summary and on-call escalation
  - Route medium alerts to dashboard visibility
  - Deduplicate repeated alerts within 15-minute window (ALR-004)

- [ ] Set thresholds per environment:
  - **Development**: Alert thresholds relaxed (10% error rate, 2000ms latency)
  - **Staging**: Alert thresholds moderate (5% error rate, 1000ms latency)
  - **Production**: Alert thresholds strict (1% error rate, 500ms latency)
  - Implement via environment variable or alert configuration per deployment

- [ ] Create incident response runbooks:

  **`docs/runbook-high-error-rate.md`**:
  - Check error logs for common error types
  - Look at trace details for failing requests
  - Identify if specific endpoints are affected
  - Check recent deployments or config changes
  - Escalation path and mitigation steps

  **`docs/runbook-high-latency.md`**:
  - Check which endpoints have high latency
  - Look at trace breakdown: where is time spent?
  - Check database query performance
  - Check external service latency
  - Check CPU/memory utilization
  - Mitigation: scale up, optimize query, cache

  **`docs/runbook-projection-lag.md`**:
  - Check which projection is lagging
  - Check projection processor logs for errors
  - Look for event store issues blocking projection
  - Restart projection processor or skip problematic events
  - Verify CQRS consistency after recovery

  **`docs/runbook-event-store-failure.md`**:
  - Check event store connectivity and health
  - Check database logs for errors
  - Verify network connectivity
  - Restart event store if safe
  - Failover to replica if configured

- [ ] Configure alert deduplication (ALR-004):
  - Same alert firing repeatedly triggers only one notification per time window
  - Use AlertManager grouping (e.g., group by alert name + environment)
  - Set resolve timeout to prevent flapping alerts (minimum 5 minutes breach)

- [ ] Test alert rules:
  - Create synthetic load to trigger high latency alert
  - Verify alert fires with correct severity and routing
  - Verify alert resolves when condition clears
  - Test alert deduplication (fire alert twice, should notify once)
  - Verify runbook links are correct and accessible

- [ ] Document alert meanings in `ops/alert-rules/README.md`:
  - Each alert: what it measures, typical causes, mitigation steps
  - Threshold rationale: why these values?
  - Environment-specific thresholds
  - How to add new alerts
  - How to modify thresholds

- [ ] Create alert dashboard:
  - Show all active alerts
  - Show alert history (last 7 days)
  - Show metrics that triggered alerts
  - Quick links to runbooks
  - Environment selector (dev, staging, prod)

- [ ] Integrate alerts into on-call rotation:
  - Configure PagerDuty/Opsgenie/similar with critical alert routing
  - Test on-call escalation (alert → notification → escalation)
  - Define escalation timeout (30 min → escalate to manager)

---

## Acceptance Criteria

- [ ] All critical path alert rules defined per ALR-001
- [ ] Alert thresholds are environment-specific per ALR-002
- [ ] Alert routing is configured for appropriate teams per ALR-003
- [ ] Alert deduplication prevents spam (15+ min window) per ALR-004
- [ ] Alerts require sustained breach (5+ min) to avoid flaps per ALR-005
- [ ] All runbooks are complete and accessible
- [ ] Alert rules tested and verified to fire correctly
- [ ] Alert resolution tested (clears when condition recovers)
- [ ] Runbooks document: what to check, common causes, mitigation steps
- [ ] Sample alerts captured in documentation
- [ ] On-call rotation integrated with alert platform
- [ ] All tests pass (including alert rule syntax validation)
- [ ] Build succeeds with 0 errors, 0 warnings

---

## Dependencies

**Blocks:**
- None (but enhances Items 052, 053)

**Depends On:**
- Item-049 - OTel SDK (metrics must be available)
- Item-052 - Metrics (alerts query metrics)
- Item-053 - Event store observability (needs event store metrics)

**Related:**
- Item-050 - Logging (logs referenced in runbooks)
- Item-051 - Tracing (traces referenced in incident response)

---

## Notes

Good alerting is critical for on-call reliability. Avoid alert fatigue with proper thresholds and deduplication. Every alert should have a runbook explaining what to do. Test alerts regularly (quarterly) to ensure they still make sense.
