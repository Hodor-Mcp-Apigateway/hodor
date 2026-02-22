# Hodor Deployment

## Quick Deploy

```bash
# Docker Compose (default)
make deploy
# or: make deploy-docker

# Helm (Kubernetes)
make deploy-helm

# Kind (local K8s)
make deploy-kind
```

## Docker Compose

```bash
# Start
make docker-compose-up
# or: cd deployment/docker && docker compose -f docker-compose.yaml up -d --build

# Scale Hodor to 3 replicas
make docker-compose-scale REPLICAS=3
# or: docker compose -f deployment/docker/docker-compose.yaml up -d --scale hodor=3

# Stop
make docker-compose-down
```

## Helm

```bash
# Install/upgrade
helm upgrade --install hodor deployment/helm/hodor

# With custom values
helm upgrade --install hodor deployment/helm/hodor \
  -f my-values.yaml \
  --set autoscaling.maxReplicas=20

# Enable ingress
helm upgrade --install hodor deployment/helm/hodor \
  --set ingress.enabled=true \
  --set ingress.hosts[0].host=hodor.example.com
```

### HPA (Horizontal Pod Autoscaler)

Enabled by default in `values.yaml`:

```yaml
autoscaling:
  enabled: true
  minReplicas: 1
  maxReplicas: 10
  targetCPUUtilizationPercentage: 70
```

## Kind

```bash
make deploy-kind
# Creates cluster, builds image, loads into Kind, installs Helm chart

# Port forward
kubectl port-forward svc/hodor-hodor 8080:8080 -n hodor
```

## Structure

```
deployment/
├── docker/
│   ├── docker-compose.yaml      # Unified (PostgreSQL + Hodor, scale)
│   ├── docker-compose-minimal.yaml
│   └── ...
├── helm/
│   └── hodor/                   # Helm chart
│       ├── Chart.yaml
│       ├── values.yaml
│       └── templates/
│           ├── deployment.yaml
│           ├── hpa.yaml         # Horizontal Pod Autoscaler
│           ├── service.yaml
│           ├── ingress.yaml
│           └── postgres.yaml
├── kind/
│   └── ...
└── scripts/
    └── deploy.sh                # Unified deploy script
```
