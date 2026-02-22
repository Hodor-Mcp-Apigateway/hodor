#!/bin/sh
# Hodor - Unified deploy script
# Usage: ./deploy.sh [docker|helm|kind|all]
# Prefers: dotnet run --project src/Hodor.Cli -- deploy --target <target>

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
cd "$ROOT"

# Prefer .NET CLI when built
CLI_PROJECT="$ROOT/src/Hodor.Cli/Hodor.Cli.csproj"
TARGET="${1:-docker}"
if [ -f "$CLI_PROJECT" ] && command -v dotnet >/dev/null 2>&1; then
  if dotnet build "$CLI_PROJECT" -v q 2>/dev/null; then
    case "$TARGET" in
      docker|helm|kind) exec dotnet run --project "$CLI_PROJECT" --no-build -- deploy --target "$TARGET" ;;
      all) dotnet run --project "$CLI_PROJECT" --no-build -- deploy --target docker
           dotnet run --project "$CLI_PROJECT" --no-build -- deploy --target helm 2>/dev/null || true ;;
      *) exec dotnet run --project "$CLI_PROJECT" --no-build -- deploy --target docker ;;
    esac
  fi
fi

# Fallback: shell deploy
deploy_docker() {
    echo "=== Deploy: Docker Compose ==="
    docker compose -f deployment/docker/docker-compose.yaml up -d --build
    echo ""
    echo "  Scale: docker compose -f deployment/docker/docker-compose.yaml up -d --scale hodor=3"
    echo "  Health: http://localhost:8080/health"
}

deploy_helm() {
    echo "=== Deploy: Helm ==="
    if ! command -v helm >/dev/null 2>&1; then
        echo "Error: helm required. Install: https://helm.sh/docs/intro/install/"
        exit 1
    fi
    helm upgrade --install hodor deployment/helm/hodor \
        --set image.tag=latest \
        --set autoscaling.enabled=true \
        --set autoscaling.minReplicas=1 \
        --set autoscaling.maxReplicas=10
    echo ""
    echo "  kubectl get pods -l app.kubernetes.io/name=hodor"
    echo "  kubectl get hpa"
}

deploy_kind() {
    echo "=== Deploy: Kind (cluster + Helm) ==="
    if ! command -v kind >/dev/null 2>&1; then
        echo "Error: kind required. Install: https://kind.sigs.k8s.io/"
        exit 1
    fi
    CLUSTER="${KIND_CLUSTER_NAME:-hodor-cluster}"
    echo "  Building image..."
    docker build -t hodor-mcp-gateway:latest .
    if kind get clusters 2>/dev/null | grep -q "^${CLUSTER}$"; then
        echo "  Loading image into Kind..."
        kind load docker-image hodor-mcp-gateway:latest --name "$CLUSTER"
    else
        echo "  Creating Kind cluster..."
        kind create cluster --name "$CLUSTER" --config "$ROOT/deployment/kind/kind-config.yaml"
        kind load docker-image hodor-mcp-gateway:latest --name "$CLUSTER"
    fi
    kubectl create namespace hodor 2>/dev/null || true
    echo "  Installing Helm chart..."
    helm upgrade --install hodor deployment/helm/hodor -n hodor \
        --set image.repository=hodor-mcp-gateway \
        --set image.tag=latest \
        --set image.pullPolicy=IfNotPresent \
        --set autoscaling.enabled=true
    kubectl rollout status deployment/hodor-hodor -n hodor --timeout=120s
    echo ""
    echo "  kubectl port-forward svc/hodor-hodor 8080:8080 -n hodor"
    echo "  Health: http://localhost:8080/health"
}

case "${1:-docker}" in
    docker) deploy_docker ;;
    helm) deploy_helm ;;
    kind) deploy_kind ;;
    all) deploy_docker; echo ""; deploy_helm 2>/dev/null || true ;;
    *) echo "Usage: $0 [docker|helm|kind|all]"; exit 1 ;;
esac

echo ""
echo "=== Done ==="
