# Payment Orchestrator (.NET 8)

Clean Architecture  DDD  event-driven. Roadmap M0–M11 en `docs/`.

## Quickstart local

### Infra (SQL  Rabbit  Mongo)
```bash
cp .env.example .env
# Editar .env y setear contraseñas fuertes

docker compose up -d
docker compose ps
```

### Config local (NO commitear)
```bash
# API
cp src/PaymentOrchestrator.Api/appsettings.Development.sample.json \
   src/PaymentOrchestrator.Api/appsettings.Development.json

# Workers
cp src/PaymentOrchestrator.Workers.OutboxPublisher/appsettings.Development.sample.json \
   src/PaymentOrchestrator.Workers.OutboxPublisher/appsettings.Development.json
cp src/PaymentOrchestrator.Workers.Reconciliation/appsettings.Development.sample.json \
   src/PaymentOrchestrator.Workers.Reconciliation/appsettings.Development.json
```

### Build/Test
```bash
dotnet tool restore

dotnet restore
dotnet build -c Release
dotnet test -c Release
```

### Run
```bash
dotnet run --project src/PaymentOrchestrator.Api
# En otra terminal:
dotnet run --project src/PaymentOrchestrator.Workers.OutboxPublisher
# En otra terminal:
dotnet run --project src/PaymentOrchestrator.Workers.Reconciliation
```

## Scripts
- `./scripts/verify-m0.sh` (Linux/macOS)
- `./scripts/verify-m0.ps1` (PowerShell)

## Runbook detallado
Ver `docs/runbook-local.md`.