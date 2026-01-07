# ADR-0008: Reproducible Development Environment via Docker Compose

## Status
Accepted

## Context
The Payment Orchestrator depends on external infrastructure (SQL Server, a message broker, and a NoSQL store). In enterprise projects, environment drift (versions, ports, credentials, local installs) causes slow onboarding and fragile debugging and integration testing.

We need a dev environment that is:
- reproducible (single command),
- stable (pinned versions + health checks),
- close to production (real services, not mocks),
- secure (no secrets committed),
- CI-friendly.

## Decision
Use **Docker Compose** as the standard way to run development dependencies:
- SQL Server (transactional source of truth)
- RabbitMQ (asynchronous messaging)
- MongoDB (read models/projections and/or document storage)

Operational principles:
1. Single command bootstrap: `docker compose up -d`
2. Service readiness via health checks
3. Secrets via `.env` (ignored) + `.env.example` (committed)
4. Pinned image versions
5. Optional local persistence via volumes
6. Explicit port mappings for tooling access

## Alternatives Considered
- Local installs (SQL/Rabbit/Mongo): high drift, slow onboarding.
- In-memory/mocks: not representative for outbox/webhooks/retries.
- Local Kubernetes/dev containers: higher parity but too much complexity for the current stage.

## Consequences
### Positive
- Fast and deterministic onboarding.
- Fewer "it works on my machine" issues.
- Supports distributed features (outbox publisher, reconciliation, webhooks).
- Foundation for integration tests (Testcontainers/CI).

### Negative / Trade-offs
- Requires Docker installed and enough local resources.
- Not identical to cloud-managed services, but Infrastructure abstractions allow swapping later.

## Notes
- Use a non-guest RabbitMQ user to avoid common container networking restrictions.
- Document ports/credentials/diagnostics in README.
