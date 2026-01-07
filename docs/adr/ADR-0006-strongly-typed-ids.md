# ADR-0006: Strongly Typed IDs

## Status
Accepted

## Context
In systems with many identifiers, using `Guid`/`string` everywhere leads to wiring bugs (mixing IDs across aggregates, passing wrong values, and losing intent).

## Decision
Use strongly typed IDs for aggregates (e.g., `PaymentId`) instead of raw primitives.

Implementation notes:
- IDs wrap a `Guid` and support safe construction/serialization.
- EF Core mapping will use value conversions to store the underlying `Guid`.
- Public API contracts may expose IDs as strings/Guids depending on conventions, but Application maps them back to typed IDs.

## Alternatives Considered
- **Primitive IDs everywhere**: simplest but error-prone.
- **Generic typed ID library**: may add dependency/cognitive overhead for a portfolio project.

## Consequences
### Positive
- Improved type safety and readability.
- Reduces accidental cross-aggregate ID misuse.

### Negative / Trade-offs
- Adds some friction in ORM mappings and serialization layers.
