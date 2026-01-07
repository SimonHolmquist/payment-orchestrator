# ADR-0013: Observability (Structured Logs + Correlation + OpenTelemetry)

## Status
Proposed

## Context
Distributed payment flows require tracing across API, workers, DB, and broker. Without correlation IDs and traces, production debugging becomes slow and error-prone.

## Decision
- Use structured logging with consistent fields (paymentId, correlationId, idempotencyKey).
- Propagate correlation IDs through HTTP and broker headers.
- Adopt OpenTelemetry for traces/metrics (export target chosen per environment).

## Alternatives Considered
- Logs only, no correlation: insufficient for distributed systems.
- Vendor-specific tracing: harder to keep portable.

## Consequences
### Positive
- Faster incident response and clearer root-cause analysis.
- Better performance tuning and SLA visibility.

### Negative / Trade-offs
- Additional setup and operational overhead (collectors/exporters).
