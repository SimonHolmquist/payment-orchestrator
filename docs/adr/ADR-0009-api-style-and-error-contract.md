# ADR-0009: API Style and Error Contract (ProblemDetails)

## Status
Accepted

## Context
We need consistent REST APIs for multiple clients and predictable error handling across services. Payment APIs must communicate validation/state-transition errors clearly.

## Decision
- Use Controllers (or Minimal APIs) consistently across the API. Choose one style and apply it everywhere.
- Standardize error responses using RFC 7807 **ProblemDetails**.
- Use request validation in Application layer and map domain exceptions to ProblemDetails in API.

## Alternatives Considered
- Ad-hoc error objects: inconsistent and hard to integrate.
- Throwing raw exceptions to clients: security and UX issues.

## Consequences
### Positive
- Stable client contract and easier troubleshooting.
- Centralized error mapping and consistent HTTP semantics.

### Negative / Trade-offs
- Slight upfront setup (filters/middleware, conventions).
