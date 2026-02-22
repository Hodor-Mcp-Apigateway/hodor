#!/bin/bash
# Optional: Install PostgreSQL, Kafka, Redis for future extensions
# Hodor MCP Gateway runs standalone without these dependencies

set -Eeuo pipefail

echo "Hodor MCP Gateway - Kubernetes deployment"
echo "Run: helm upgrade --install hodor deployment/k8s/.helm/template-service"
echo ""
echo "For optional infrastructure (PostgreSQL, Kafka, Redis), uncomment below:"
echo "# helm repo add bitnami https://charts.bitnami.com/bitnami"
echo "# helm install kafka bitnami/kafka"
echo "# helm install postgresql bitnami/postgresql"
echo "# helm install redis bitnami/redis"
