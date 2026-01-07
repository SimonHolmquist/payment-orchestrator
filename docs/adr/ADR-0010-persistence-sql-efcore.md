# ADR-0010: Persistence Strategy (SQL Server + EF Core)

## Status
Proposed

## Context
Payments require strong consistency for the source-of-truth state machine and for persisting the Outbox atomically with state changes.

## Decision
- Use **SQL Server** as the system of record for Payments and Outbox.
- Use **EF Core** for persistence with explicit mappings for Value Objects and typed IDs.
- Use migrations as the schema evolution mechanism.

## Alternatives Considered
- NoSQL as source of truth: weaker transactional guarantees for core payment state.
- Dapper-only: less abstraction, but more manual mapping and higher maintenance cost at scale.

## Consequences
### Positive
- Strong transactional boundaries and atomic outbox writes.
- Mature tooling for schema evolution and diagnostics.

### Negative / Trade-offs
- Mapping typed IDs/VOs requires explicit conversions/configuration.
