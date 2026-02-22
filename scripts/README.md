# Hodor - Setup Scripts

Cross-platform setup. Works on **Windows** (PowerShell), **Linux**, **Mac**.

## Quick Start

### .NET CLI (preferred)

```bash
cd Hodor
dotnet run --project src/Hodor.Cli -- install
# or: make hodor-install
```

### Linux / Mac (shell fallback)

```bash
cd Hodor
chmod +x scripts/setup.sh
./scripts/setup.sh
```

### Windows (PowerShell)

```powershell
cd Hodor
.\scripts\setup.ps1
```

### One-liner (if Docker already running)

```bash
# From Hodor repo root
./scripts/install.sh        # Prefers CLI, falls back to shell
./scripts/setup.sh          # Linux/Mac
pwsh scripts/setup.ps1      # Windows (PowerShell Core)
```

## What It Does

1. Checks Docker is installed
2. Starts PostgreSQL + Hodor via Docker Compose
3. Waits for health check
4. Prints endpoints

## After Setup

```bash
# Test
curl http://localhost:8080/health

# Run samples
cd samples
./curl.sh
python python.py
node javascript.js
```

## Hodor CLI Commands

| Command | Description |
|---------|-------------|
| `hodor install` | Install via Docker Compose |
| `hodor deploy --target docker` | Deploy Docker Compose |
| `hodor deploy --target helm` | Deploy Helm (K8s) |
| `hodor deploy --target kind` | Deploy Kind + Helm |
| `hodor health` | Check health endpoint |
| `hodor scale --replicas 3` | Scale Docker Compose |

```bash
dotnet run --project src/Hodor.Cli -- install
dotnet run --project src/Hodor.Cli -- deploy --target kind
dotnet run --project src/Hodor.Cli -- health
```

## Alternative: Kind

```bash
make kind-setup
# or: dotnet run --project src/Hodor.Cli -- deploy --target kind
```

## Stop

```bash
docker compose -f deployment/docker/docker-compose-infrastructure.yaml \
               -f deployment/docker/docker-compose-app.yaml down
```
