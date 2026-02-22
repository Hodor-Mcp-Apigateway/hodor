#!/bin/sh
# Run unit tests with coverage
# Usage: ./scripts/test.sh
# Output: TestResults/coverage.cobertura.xml

set -e

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

mkdir -p TestResults

echo "=== Building ==="
dotnet build Hodor.slnx -c Release

echo "=== Running tests with coverage ==="
dotnet test Hodor.slnx -c Release \
  --no-build \
  --verbosity normal \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults/ \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura

echo ""
echo "=== Coverage report: TestResults/ ==="
ls -la TestResults/
