# ADR-0012: Idempotency Strategy (API + Messaging)

## Status
Proposed

## Context
Payment creation/capture/refund operations must be safe under retries (client retries, network timeouts, duplicated messages). Duplicate processing is a top source of incidents.

## Decision
- API supports **Idempotency-Key** for write operations (create/capture/refund).
- Persist idempotency records in SQL with a bounded retention policy.
- Outbox publisher and consumers use dedupe (message ID / event ID) to ensure side effects are applied once.

## Alternatives Considered
- No idempotency: unacceptable for payments.
- Rely solely on broker guarantees: insufficient.

## Consequences
### Positive
- Safe retries for clients and workers.
- Reduced risk of double-capture/double-refund.

### Negative / Trade-offs
- Additional storage and logic for idempotency records and retention.
