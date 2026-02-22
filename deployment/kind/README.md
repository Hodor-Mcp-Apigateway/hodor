# Hodor - Kind Cluster Deployment

Deploy Hodor MCP Gateway on a local Kind (Kubernetes in Docker) cluster.

## Prerequisites

- Docker
- Kind (`brew install kind` or https://kind.sigs.k8s.io/)
- kubectl

## Quick Start

```bash
cd deployment/kind
chmod +x setup.sh
./setup.sh all
```

This will:
1. Create Kind cluster `hodor-cluster`
2. Build Hodor Docker image and load into Kind
3. Deploy PostgreSQL (pgvector) and Hodor

## Endpoints

| Service   | URL                    | Description        |
|-----------|------------------------|--------------------|
| Health    | http://localhost:8080/health | Liveness check  |
| Ready     | http://localhost:8080/ready  | Readiness check |
| Tools     | http://localhost:8080/api/tools | MCP tools list |
| SSE       | http://localhost:8080/sse     | MCP SSE (Claude/Cursor) |
| PostgreSQL| localhost:5432               | DB (user: postgres, db: hodor) |

## Commands

```bash
# Create cluster only
./setup.sh cluster-create

# Build and load image (after code changes)
./setup.sh build-load

# Deploy/update manifests
./setup.sh deploy

# Run migrations (from Hodor root)
ConnectionStrings__PostgreSQL="Host=localhost;Port=5432;Database=hodor;Username=postgres;Password=postgres" make migrate

# Port-forward fallback (if NodePort not working)
./port-forward.sh
```

## Cleanup

```bash
kind delete cluster --name hodor-cluster
```
