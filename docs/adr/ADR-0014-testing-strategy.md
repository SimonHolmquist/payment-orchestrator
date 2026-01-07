# ADR-0014: Testing Strategy (Unit + Integration)

## Status
Proposed

## Context
The Domain is heavily rule-based and must be protected by fast unit tests. Integration points (SQL/Rabbit/Mongo) require realistic integration tests to validate configuration and reliability patterns (outbox, retries).

## Decision
- Unit tests focus on Domain invariants and state transitions.
- Integration tests use ephemeral dependencies (prefer Testcontainers where possible).
- Keep integration tests separate and runnable in CI.

## Alternatives Considered
- Unit tests only: misses infrastructure misconfigurations and messaging issues.
- Full end-to-end tests only: slow and brittle.

## Consequences
### Positive
- Fast feedback loop + realistic coverage where needed.
- CI-friendly and scalable approach.

### Negative / Trade-offs
- Requires maintaining test infrastructure and some additional tooling.
