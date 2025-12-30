#!/bin/bash

set -Eeuo pipefail
trap clean_up SIGINT SIGTERM ERR EXIT

function clean_up {
  exit 0
}

helm repo add bitnami https://charts.bitnami.com/bitnami

helm install kafka bitnami/kafka
helm install postgresql bitnami/postgresql
helm install redis bitnami/redis

export POSTGRES_PASSWORD=$(kubectl get secret postgresql -o json | jq -r '.data["postgres-password"]' | base64 -d)
export REDIS_PASSWORD=$(kubectl get secret redis  -o json | jq -r '.data["redis-password"]'| base64 -d)

cat <<EOF > ./appsettings.secrets.json
{
    "ConnectionStrings": {
        "PostgresConnection": {
          "ConnectionString": "Host=postgresql;Port=5432;Uid=postgres;Password=$POSTGRES_PASSWORD;Database=template_db;",
          "HealthCheckEnabled" : true,
          "LoggingEnabled": true
        },
        "RedisConnection": {
          "ConnectionString": "redis-headless,password=$REDIS_PASSWORD,ssl=False,abortConnect=False",
          "HealthCheckEnabled" : true
        }
    },
    "Kafka": {
        "BootstrapServers": "kafka:9092",
        "ConsumerGroupId": "template-service",
        "HealthCheckEnabled": true
    }
}
EOF

kubectl create secret generic secret-appsettings --from-file=./appsettings.secrets.json
