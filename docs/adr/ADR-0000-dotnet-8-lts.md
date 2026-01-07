# ADR-0000: Target Runtime is .NET 8 (LTS)

## Status
Accepted

## Context
The project targets a Senior .NET role and should be aligned with common enterprise stacks. .NET 8 is the current LTS baseline and maximizes compatibility with CI/CD and production environments.

## Decision
All projects target **net8.0**. Any newer SDK usage must not change the target framework without a deliberate ADR update.

## Alternatives Considered
- Target .NET 9: newer features, but reduces compatibility and increases deployment friction.
- Multi-targeting: unnecessary complexity at this stage.

## Consequences
### Positive
- Enterprise compatibility and predictable maintenance lifecycle.
- Consistent tooling across API, workers, and tests.

### Negative / Trade-offs
- Delays adoption of newest runtime features until they land in LTS.
