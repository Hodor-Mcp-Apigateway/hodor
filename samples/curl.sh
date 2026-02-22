#!/bin/sh
# Hodor API - curl samples (Linux, Mac)
# Usage: ./curl.sh  or  bash curl.sh

BASE_URL="${HODOR_URL:-http://localhost:8080}"

echo "=== Hodor MCP Gateway - curl samples ==="
echo "Base URL: $BASE_URL"
echo ""

echo "1. Health check"
curl -s "$BASE_URL/health" | head -1
echo -e "\n"

echo "2. Ready check"
curl -s "$BASE_URL/ready" | head -1
echo -e "\n"

echo "3. List tools"
curl -s "$BASE_URL/api/tools" | head -20
echo -e "\n"

echo "4. Health info (detailed)"
curl -s "$BASE_URL/health/info" | head -5
echo ""
