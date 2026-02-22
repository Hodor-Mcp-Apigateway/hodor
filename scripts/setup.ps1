# Hodor MCP Gateway - Setup (Windows PowerShell, Mac, Linux)
# Usage: .\setup.ps1  or  pwsh setup.ps1
# Requires: Docker, Docker Compose

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$Root = Split-Path -Parent $ScriptDir
Set-Location $Root

Write-Host "=== Hodor MCP Gateway - Setup ===" -ForegroundColor Cyan
Write-Host ""

# Check Docker
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "Error: Docker not found. Install: https://docs.docker.com/get-docker/" -ForegroundColor Red
    exit 1
}

# Start with Docker Compose
Write-Host "[1/3] Starting PostgreSQL + Hodor..." -ForegroundColor Yellow
docker compose -f deployment/docker/docker-compose-infrastructure.yaml `
               -f deployment/docker/docker-compose-app.yaml up -d --build

Write-Host "[2/3] Waiting for Hodor..." -ForegroundColor Yellow
$url = "http://localhost:8080/health"
for ($i = 1; $i -le 10; $i++) {
    try {
        $r = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 2 -ErrorAction SilentlyContinue
        if ($r.StatusCode -eq 200) {
            Write-Host "  Hodor is ready!" -ForegroundColor Green
            break
        }
    } catch {}
    Write-Host "  Waiting... ($i/10)"
    Start-Sleep -Seconds 3
}

Write-Host "[3/3] Quick test..." -ForegroundColor Yellow
try {
    (Invoke-WebRequest -Uri $url -UseBasicParsing).Content
} catch {
    Write-Host "  Could not reach Hodor. Check: docker compose logs" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Setup complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "  Health:  http://localhost:8080/health"
Write-Host "  Tools:   http://localhost:8080/api/tools"
Write-Host "  SSE:     http://localhost:8080/sse"
Write-Host ""
Write-Host "  Samples: cd samples; bash curl.sh" -ForegroundColor Gray
Write-Host ""
