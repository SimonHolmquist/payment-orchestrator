#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

echo "== .NET SDK =="
dotnet --version

echo "== Restore / Build / Test =="
dotnet restore
# Build & tests in Release to detect drift early
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet build -c Release --no-restore
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet test -c Release --no-build

echo "== Local env =="
if [ ! -f .env ]; then
  cp .env.example .env
  echo "Created .env from .env.example (update passwords before committing anything)."
fi

echo "== Docker Compose =="
docker compose up -d

docker compose ps

echo "== Recent logs (tail=100) =="
docker compose logs --tail=100

echo "OK: M0 verification completed."