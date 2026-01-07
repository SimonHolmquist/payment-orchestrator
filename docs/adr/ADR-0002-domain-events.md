# ADR-0002: Domain Events

## Status
Accepted

## Context
Payment state transitions must trigger side effects: publish integration events, audit, reconcile uncertain states, notify other subsystems, etc. Coupling those side effects to the domain model would introduce I/O and reduce testability.

## Decision
The Domain will raise **Domain Events** (implementing `IDomainEvent`) on meaningful state transitions, e.g.:
`PaymentCreated`, `PaymentAuthorized`, `PaymentCaptured`, `PaymentRefunded`, `PaymentCancelled`, `PaymentFailed`, `PaymentMarkedUnknown`.

Event lifecycle:
1. Domain adds events during state changes.
2. Application layer collects/dequeues them after successful use-case execution.
3. Events are persisted to the Outbox as part of the same transaction (see ADR-0003).
4. A worker publishes them asynchronously.

## Alternatives Considered
- **Direct broker publishing inside use cases**: risks inconsistency if DB commit and publish are not atomic.
- **Callbacks/hooks inside Domain**: introduces I/O and breaks purity/testability.

## Consequences
### Positive
- Clean separation between business rules and side effects.
- Enables Outbox pattern, auditing, observability, and eventual consistency.

### Negative / Trade-offs
- Requires discipline: no I/O in the Domain; publishing is asynchronous.
- At-least-once delivery must be assumed; consumers need idempotency (see ADR-0012).
