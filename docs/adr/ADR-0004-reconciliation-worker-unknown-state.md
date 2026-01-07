# ADR-0004: Reconciliation Worker and "Unknown" Payment State

## Status
Accepted

## Context
PSPs/banks can leave operations in uncertain states due to timeouts, missed callbacks/webhooks, or partial failures. Without an explicit recovery mechanism, payments can remain "stuck" and require manual intervention.

## Decision
When confirmation is not received within a defined time window, mark the Payment as **Unknown** (Domain Event: `PaymentMarkedUnknown`).
A background worker (**Reconciliation**) periodically queries the PSP and reconciles the Payment state (e.g., Unknown → Captured/Failed) in an idempotent way.

Operational constraints (to be implemented):
- Configurable schedule/frequency and time windows
- Rate limiting / circuit breakers against PSPs
- Audit trail and correlation IDs

## Alternatives Considered
- **No reconciliation** (manual ops): does not scale, high operational cost.
- **Aggressive retries in API path**: increases latency and still fails under many real-world conditions.

## Consequences
### Positive
- Reduces "stuck payments" and operational load.
- Increases reliability under intermittent failures.

### Negative / Trade-offs
- Requires careful limits (windows, rate limiting, observability).
- Adds complexity in workflows and monitoring.
