# ADR-0005: Money Value Object

## Status
Accepted

## Context
Money handling is critical in payment systems. Using raw `decimal` values everywhere leads to bugs: missing currency, negative amounts, currency mismatches, and inconsistent rounding rules.

## Decision
Represent monetary values with a dedicated **Money Value Object**:
- Holds `Amount` and `Currency`
- Enforces invariants (currency format; non-negative or positive where applicable)
- Provides safe arithmetic that prevents currency mismatch

Rounding/precision:
- Amounts are represented as `decimal` in the domain.
- Persistence and APIs must define a consistent scale (e.g., 2 decimals for most currencies). Any PSP-specific minor unit conversion belongs in Infrastructure.

## Alternatives Considered
- **Use decimal + string everywhere**: faster to code, error-prone long-term.
- **Use minor units (long cents)** everywhere: safer for arithmetic but adds friction and conversion complexity; can be introduced later if needed.

## Consequences
### Positive
- Fewer monetary bugs and clearer intent.
- Stronger invariants and easier validation.

### Negative / Trade-offs
- Requires explicit mapping in persistence/serialization.
