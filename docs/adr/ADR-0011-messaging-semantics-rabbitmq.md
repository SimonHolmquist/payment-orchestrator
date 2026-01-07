# ADR-0011: Messaging Semantics (RabbitMQ, At-Least-Once)

## Status
Proposed

## Context
Outbox publishing and downstream consumers must operate reliably under failures. Broker delivery is not exactly-once in practice; consumers must tolerate duplicates.

## Decision
- Use RabbitMQ with explicit topology (exchange + queues per consumer).
- Assume **at-least-once** delivery end-to-end.
- Implement retry strategy + DLQ and attach correlation/idempotency headers.

## Alternatives Considered
- Exactly-once delivery: not realistic operationally without heavy constraints.
- Fire-and-forget messaging: unreliable for payments.

## Consequences
### Positive
- Robustness under transient failures.
- Clear operational behavior (retries, DLQ).

### Negative / Trade-offs
- Requires idempotent consumers and dedupe mechanisms.
