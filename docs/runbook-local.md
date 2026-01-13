# Runbook local (reproducible)

> Objetivo: poder clonar/abrir el repo y correrlo **desde cero** con una secuencia mínima y repetible.

## Requisitos
- Docker Engine / Docker Desktop
- .NET SDK **8.0.416** (pinneado por `global.json`)

## 1) Infra (SQL Server  RabbitMQ  Mongo)

1. Crear `.env` local (NO se commitea):

```bash
cp .env.example .env
# Editar .env y poner contraseñas fuertes
```

2. Levantar dependencias:

```bash
docker compose up -d
docker compose ps
```

### Puertos por defecto
- SQL Server: `localhost:1433`
- RabbitMQ (AMQP): `localhost:5672`
- RabbitMQ Management UI: `http://localhost:15672`
- MongoDB: `localhost:27017`

## 2) Config local de apps (sin commitear)

Este repo versiona **samples**. Para correr en local, copiá el sample al archivo de desarrollo (que está en `.gitignore`).

### API
```bash
cp src/PaymentOrchestrator.Api/appsettings.Development.sample.json \
   src/PaymentOrchestrator.Api/appsettings.Development.json
```

### Workers
```bash
cp src/PaymentOrchestrator.Workers.OutboxPublisher/appsettings.Development.sample.json \
   src/PaymentOrchestrator.Workers.OutboxPublisher/appsettings.Development.json

cp src/PaymentOrchestrator.Workers.Reconciliation/appsettings.Development.sample.json \
   src/PaymentOrchestrator.Workers.Reconciliation/appsettings.Development.json
```

## 3) Build y tests

```bash
dotnet restore
dotnet build -c Release --no-restore
dotnet test -c Release --no-build
```

## 4) Dotnet tools (M2)

El repo incluye tool-manifest para EF Core (útil a partir de M2):

```bash
dotnet tool restore
```

## 5) Ejecutar API y workers

En terminales separadas:

```bash
dotnet run --project src/PaymentOrchestrator.Api
```

```bash
dotnet run --project src/PaymentOrchestrator.Workers.OutboxPublisher
```

```bash
dotnet run --project src/PaymentOrchestrator.Workers.Reconciliation
```

## Troubleshooting rápido
- **SQL no levanta / password inválido**: SQL Server exige password fuerte para `sa` (ver `.env`).
- **Puertos ocupados**: cambiar mapeos en `docker-compose.yml`.
- **Rabbit UI**: usuario/password salen de `.env` (`RABBITMQ_DEFAULT_USER`, `RABBITMQ_DEFAULT_PASS`).