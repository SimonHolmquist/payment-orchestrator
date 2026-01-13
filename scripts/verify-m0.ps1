$ErrorActionPreference = "Stop"

Set-Location (Join-Path $PSScriptRoot "..")

Write-Host "== .NET SDK =="
dotnet --version

Write-Host "== Restore / Build / Test =="
dotnet restore
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
dotnet build -c Release --no-restore
dotnet test -c Release --no-build

Write-Host "== Local env =="
if (-not (Test-Path -Path ".env")) {
  Copy-Item ".env.example" ".env"
  Write-Host "Created .env from .env.example (update passwords before committing anything)."
}

Write-Host "== Docker Compose =="
docker compose up -d

docker compose ps

Write-Host "== Recent logs (tail=100) =="
docker compose logs --tail=100

Write-Host "OK: M0 verification completed."