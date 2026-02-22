# Kubernetes Deployment

## Kind (Recommended)

For local development, use Kind cluster:

```bash
make kind-setup
```

See [deployment/kind/README.md](../kind/README.md) for details.

## Helm (Generic K8s)

For production or non-Kind clusters:

```bash
# Install PostgreSQL (Bitnami chart with pgvector)
helm repo add bitnami https://charts.bitnami.com/bitnami
helm install postgresql bitnami/postgresql -n hodor --create-namespace \
  --set auth.username=postgres \
  --set auth.password=postgres \
  --set auth.database=hodor

# Deploy Hodor
helm upgrade --install hodor-mcp-gateway deployment/k8s/.helm/template-service -n hodor
```
