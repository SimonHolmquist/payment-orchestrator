# ADR-0001: Clean Architecture + Domain-Driven Design (DDD)

## Status
Accepted

## Context
The system must orchestrate payments at scale: multiple integrations (PSPs/banks), asynchronous processing (outbox/reconciliation), and high testability. We need strong separation between business rules and infrastructure concerns to avoid coupling payment logic to HTTP/EF/brokers.

## Decision
Adopt a layered architecture aligned with Clean Architecture and DDD:

- **Domain**: pure business rules (Aggregates, Value Objects, Domain Events). No I/O.
- **Application**: use cases and orchestration (CQRS/MediatR when it adds value), validation, transactional boundaries, and cross-cutting behaviors.
- **Infrastructure**: EF Core, repositories, messaging broker clients, external integrations, and implementations of abstractions.
- **API**: REST endpoints, transport concerns (auth, versioning, ProblemDetails), request/response models.
- **Workers**: background processes for asynchronous workflows (Outbox Publisher, Reconciliation).

## Alternatives Considered
- **Monolithic layered app without strict boundaries**: faster initially, but business logic becomes scattered and hard to test.
- **Anemic domain model + services everywhere**: reduces domain richness and increases orchestration complexity.
- **Single process for API + background jobs**: simpler deployment, but mixes concerns and reduces operational clarity.

## Consequences
### Positive
- Fast, reliable unit tests (Domain without I/O).
- Reduced coupling to persistence/messaging stack.
- Clear ownership boundaries and easier evolution (new PSPs, new workers, new projections).

### Negative / Trade-offs
- More boilerplate and conventions to follow.
- Requires discipline to keep the Domain pure and avoid leaking infrastructure concerns.
