#!/bin/bash
# Port-forward fallback when NodePort is not accessible
# Run in background: ./port-forward.sh &
set -e
kubectl port-forward -n hodor svc/hodor 8080:8080 &
kubectl port-forward -n hodor svc/postgres 5432:5432 &
echo "Port-forward active. Stop with: pkill -f 'port-forward.*hodor'"
wait
