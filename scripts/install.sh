#!/bin/sh
# Hodor - One-command install
# Usage: curl -sSL https://raw.githubusercontent.com/Hodor-Mcp-Apigateway/hodor/main/scripts/install.sh | bash
# Or from repo: ./scripts/install.sh
# Prefers: dotnet run --project src/Hodor.Cli -- install (when available)

set -e

HODOR_URL="${HODOR_URL:-http://localhost:8080}"
REPO_URL="${HODOR_REPO_URL:-https://github.com/Hodor-Mcp-Apigateway/hodor.git}"

# Resolve repo path (run from repo root, scripts/, or clone)
if [ -d "deployment/docker" ]; then
  ROOT="$(pwd)"
elif [ -d "../deployment/docker" ]; then
  ROOT="$(cd .. && pwd)"
else
  echo "[1/4] Cloning Hodor..."
  WORK_DIR="${HODOR_INSTALL_DIR:-$(mktemp -d hodor-install.XXXXXX)}"
  git clone --depth 1 "$REPO_URL" "$WORK_DIR"
  cd "$WORK_DIR"
  ROOT="$(pwd)"
fi

# Prefer .NET CLI when built
CLI_PROJECT="$ROOT/src/Hodor.Cli/Hodor.Cli.csproj"
if [ -f "$CLI_PROJECT" ] && command -v dotnet >/dev/null 2>&1; then
  if dotnet build "$CLI_PROJECT" -v q 2>/dev/null; then
    export HODOR_URL
    exec dotnet run --project "$CLI_PROJECT" --no-build -- install
  fi
fi

# Fallback: shell install
echo "=== Hodor MCP Gateway - Install ==="
echo ""

if ! command -v docker >/dev/null 2>&1; then
  echo "Error: Docker required. Install: https://docs.docker.com/get-docker/"
  exit 1
fi

echo "[2/4] Starting Hodor (PostgreSQL + Gateway)..."
cd "$ROOT"
docker compose -f deployment/docker/docker-compose.yaml up -d --build 2>/dev/null || \
docker compose -f deployment/docker/docker-compose-minimal.yaml up -d --build 2>/dev/null || \
docker compose -f deployment/docker/docker-compose-minimal.yaml up -d

echo "[3/4] Waiting for Hodor..."
for i in 1 2 3 4 5 6 7 8 9 10; do
  if curl -sf "$HODOR_URL/health" >/dev/null 2>&1; then
    echo "  Ready!"
    break
  fi
  printf "  Waiting... (%d/10)\n" "$i"
  sleep 2
done

echo "[4/4] Verifying..."
curl -sf "$HODOR_URL/health" | head -1 && echo "" || true

echo ""
echo "=== Hodor installed ==="
echo ""
echo "  Health:   $HODOR_URL/health"
echo "  Tools:    $HODOR_URL/api/tools"
echo "  Config:   $HODOR_URL/config/claude"
echo ""
echo "  Claude:   claude mcp add --scope user --transport sse hodor $HODOR_URL/sse"
echo "  Cursor:   Settings > MCP > Add: $HODOR_URL/sse"
echo ""
