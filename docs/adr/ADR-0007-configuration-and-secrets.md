# ADR-0007: Configuration and Secrets Management

## Status
Accepted

## Context
`appsettings.Development.json` often ends up containing local secrets and credentials, which can be accidentally committed to source control.

## Decision
Do not commit secrets to the repository.

Configuration strategy:
- **Local development**: .NET User Secrets and/or environment variables.
- **Docker Compose**: environment variables via `.env` (ignored by git) and a committed `.env.example`.
- **Repository**: `appsettings.json` with safe defaults and optional `appsettings.Development.sample.json` for documentation.

Cloud strategy (future):
- Use managed secret storage (Azure Key Vault / AWS Secrets Manager) and inject via environment variables.

## Alternatives Considered
- **Commit development secrets**: unacceptable security risk.
- **Hardcode secrets**: unacceptable and non-portable.

## Consequences
### Positive
- Safer repository and easier CI/CD portability.
- Clear separation between code and environment configuration.

### Negative / Trade-offs
- Requires initial setup steps for developers and CI (documented in README).
