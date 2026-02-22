#!/bin/sh
# Hodor MCP Gateway - Setup (Linux, Mac)
# Usage: ./setup.sh  or  bash setup.sh
# Requires: Docker, Docker Compose

set -e
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$ROOT"

echo "=== Hodor MCP Gateway - Setup ==="
echo ""

# Check Docker
if ! command -v docker >/dev/null 2>&1; then
    echo "Error: Docker not found. Install: https://docs.docker.com/get-docker/"
    exit 1
fi

# Start with Docker Compose (PostgreSQL + Hodor)
echo "[1/3] Starting PostgreSQL + Hodor..."
docker compose -f deployment/docker/docker-compose-infrastructure.yaml \
               -f deployment/docker/docker-compose-app.yaml up -d --build 2>/dev/null || \
docker compose -f deployment/docker/docker-compose-infrastructure.yaml \
               -f deployment/docker/docker-compose-app.yaml up -d --build

echo "[2/3] Waiting for Hodor..."
for i in 1 2 3 4 5 6 7 8 9 10; do
    if curl -s http://localhost:8080/health >/dev/null 2>&1; then
        echo "  Hodor is ready!"
        break
    fi
    echo "  Waiting... ($i/10)"
    sleep 3
done

echo "[3/3] Quick test..."
curl -s http://localhost:8080/health && echo ""
echo ""
echo "=== Setup complete ==="
echo ""
echo "  Health:  http://localhost:8080/health"
echo "  Tools:   http://localhost:8080/api/tools"
echo "  SSE:     http://localhost:8080/sse"
echo ""
echo "  Samples: cd samples && ./curl.sh"
echo ""
