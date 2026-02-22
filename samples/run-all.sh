#!/bin/sh
# Run all samples (Linux, Mac)
# Usage: ./run-all.sh

BASE="${HODOR_URL:-http://localhost:8080}"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$SCRIPT_DIR"

echo "=== Hodor Samples (base: $BASE) ==="
echo ""

echo "--- curl ---"
bash curl.sh 2>/dev/null || sh curl.sh
echo ""

echo "--- Python ---"
python3 python.py 2>/dev/null || python python.py 2>/dev/null || echo "  (python not found)"
echo ""

echo "--- Node.js ---"
node javascript.js 2>/dev/null || echo "  (node not found)"
echo ""

echo "--- Go ---"
go run go.go 2>/dev/null || echo "  (go not found)"
echo ""

echo "--- Ruby ---"
ruby ruby.rb 2>/dev/null || echo "  (ruby not found)"
echo ""

echo "--- Rust ---"
(cd "$SCRIPT_DIR" && cargo run --quiet 2>/dev/null) || echo "  (rust/cargo not found)"
echo ""

echo "Done."
