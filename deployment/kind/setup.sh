#!/bin/bash
# Hodor MCP Gateway - Kind cluster setup
# Usage: ./setup.sh [cluster-create|deploy|all]
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
HODOR_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
CLUSTER_NAME="${KIND_CLUSTER_NAME:-hodor-cluster}"
KIND_CONFIG="${SCRIPT_DIR}/kind-config.yaml"

echo "=== Hodor MCP Gateway - Kind Setup ==="
echo "  Cluster: $CLUSTER_NAME"
echo "  Hodor root: $HODOR_ROOT"
echo ""

cluster_create() {
    echo "[1/4] Creating Kind cluster..."
    if kind get clusters 2>/dev/null | grep -q "^${CLUSTER_NAME}$"; then
        echo "  Cluster $CLUSTER_NAME exists. Delete first: kind delete cluster --name $CLUSTER_NAME"
        read -p "  Delete and recreate? (y/N) " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            kind delete cluster --name "$CLUSTER_NAME"
        else
            echo "  Using existing cluster."
            return
        fi
    fi
    kind create cluster --name "$CLUSTER_NAME" --config "$KIND_CONFIG"
    echo "  Done."
}

build_load() {
    echo "[2/4] Building and loading Hodor image..."
    cd "$HODOR_ROOT"
    docker build -t hodor-mcp-gateway:latest -f Dockerfile .
    kind load docker-image hodor-mcp-gateway:latest --name "$CLUSTER_NAME"
    echo "  Done."
}

deploy_infra() {
    echo "[3/4] Deploying PostgreSQL + pgvector..."
    kubectl apply -f "$SCRIPT_DIR/namespace.yaml"
    kubectl apply -f "$SCRIPT_DIR/postgres.yaml"
    echo "  Waiting for PostgreSQL..."
    kubectl wait --for=condition=available --timeout=120s deployment/hodor-postgres -n hodor
    echo "  Done."
}

deploy_hodor() {
    echo "[4/4] Deploying Hodor..."
    kubectl apply -f "$SCRIPT_DIR/hodor.yaml"
    echo "  Waiting for Hodor..."
    kubectl wait --for=condition=available --timeout=120s deployment/hodor -n hodor || true
    echo "  Done."
}

case "${1:-all}" in
    cluster-create)
        cluster_create
        ;;
    build-load)
        build_load
        ;;
    deploy)
        deploy_infra
        deploy_hodor
        ;;
    all)
        cluster_create
        build_load
        deploy_infra
        deploy_hodor
        ;;
    *)
        echo "Usage: $0 [cluster-create|build-load|deploy|all]"
        exit 1
        ;;
esac

echo ""
echo "=== Hodor MCP Gateway ready ==="
echo ""
echo "  Health:    http://localhost:8080/health"
echo "  Ready:     http://localhost:8080/ready"
echo "  Tools:     http://localhost:8080/api/tools"
echo "  SSE:       http://localhost:8080/sse"
echo ""
echo "  PostgreSQL: localhost:5432 (user: postgres, db: hodor)"
echo ""
echo "  Pods: kubectl get pods -n hodor"
echo ""
