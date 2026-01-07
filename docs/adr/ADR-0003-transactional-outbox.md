# ADR-0003: Transactional Outbox

## Status
Accepted

## Context
Publishing messages directly to the broker within a DB transaction can produce inconsistencies:
- DB commit succeeds but publish fails (lost events)
- publish succeeds but DB commit fails (phantom events)

We need a reliable integration mechanism suitable for enterprise-grade payment workflows.

## Decision
Persist domain/integration events to an **Outbox** (SQL table) within the same transaction that commits the state change.
A dedicated worker (**OutboxPublisher**) publishes events asynchronously and idempotently.

Delivery semantics:
- Producer side: no lost events (eventual publish)
- Broker/consumer side: assume **at-least-once** delivery, use deduplication/idempotency keys

## Alternatives Considered
- **Publish directly to broker** (best-effort): simplest but inconsistent under failures.
- **Distributed transaction** (2PC): operationally heavy; often not supported across broker+DB.

## Consequences
### Positive
- Prevents event loss and improves reliability.
- Enables retries, exponential backoff, and DLQ strategies.

### Negative / Trade-offs
- Additional operational complexity: Outbox table, cleanup, dedupe.
- Requires clear idempotency strategy for consumers (ADR-0012).
