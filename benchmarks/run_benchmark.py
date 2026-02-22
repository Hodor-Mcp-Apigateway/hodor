#!/usr/bin/env python3
"""
Hodor MCP Gateway - Latency & Throughput Benchmark

Compares Hodor against typical MCP gateway performance targets.
Usage: python run_benchmark.py [--url URL] [--requests N] [--warmup N]

Environment: HODOR_URL (default: http://localhost:8080)
"""
import argparse
import json
import os
import statistics
import sys
import time
import urllib.error
import urllib.request
from typing import List, Tuple

BASE_URL = os.environ.get("HODOR_URL", "http://localhost:8080")

ENDPOINTS = [
    ("GET", "/health", "Liveness probe"),
    ("GET", "/ready", "Readiness + tools count"),
    ("GET", "/api/tools", "Meta-tools list"),
    ("GET", "/api/tools/combined", "All tools from all servers"),
    ("GET", "/api/tools/status", "Server status overview"),
    ("GET", "/metrics", "Prometheus metrics"),
]


def request(method: str, path: str) -> Tuple[float, int]:
    """Execute request, return (latency_ms, status_code)."""
    url = f"{BASE_URL.rstrip('/')}{path}"
    req = urllib.request.Request(url, method=method)
    start = time.perf_counter()
    try:
        with urllib.request.urlopen(req, timeout=30) as r:
            _ = r.read()
        status = r.status
    except urllib.error.HTTPError as e:
        status = e.code
    except Exception:
        status = -1
    elapsed_ms = (time.perf_counter() - start) * 1000
    return elapsed_ms, status


def percentile(data: List[float], p: float) -> float:
    """Compute percentile (p in 0..100)."""
    if not data:
        return 0.0
    sorted_data = sorted(data)
    k = (len(sorted_data) - 1) * p / 100
    f = int(k)
    c = min(f + 1, len(sorted_data) - 1)
    return sorted_data[f] + (k - f) * (sorted_data[c] - sorted_data[f])


def run_latency_test(method: str, path: str, desc: str, n: int, warmup: int) -> dict:
    """Run latency benchmark, return stats dict."""
    latencies: List[float] = []
    errors = 0

    for _ in range(warmup):
        request(method, path)

    for _ in range(n):
        lat_ms, status = request(method, path)
        if status == 200:
            latencies.append(lat_ms)
        else:
            errors += 1

    if not latencies:
        return {
            "endpoint": path,
            "description": desc,
            "requests": n,
            "errors": errors,
            "latency_ms": {"min": 0, "avg": 0, "p50": 0, "p95": 0, "p99": 0, "max": 0},
        }

    return {
        "endpoint": path,
        "description": desc,
        "requests": n,
        "errors": errors,
        "latency_ms": {
            "min": min(latencies),
            "avg": statistics.mean(latencies),
            "p50": percentile(latencies, 50),
            "p95": percentile(latencies, 95),
            "p99": percentile(latencies, 99),
            "max": max(latencies),
        },
    }


def run_throughput_test(path: str, duration_sec: float) -> dict:
    """Run throughput benchmark (requests per second)."""
    count = 0
    errors = 0
    start = time.perf_counter()
    while time.perf_counter() - start < duration_sec:
        _, status = request("GET", path)
        count += 1
        if status != 200:
            errors += 1
    elapsed = time.perf_counter() - start
    return {
        "endpoint": path,
        "duration_sec": round(elapsed, 2),
        "total_requests": count,
        "errors": errors,
        "req_per_sec": round(count / elapsed, 1),
    }


def print_results(latency_results: List[dict], throughput_result: dict):
    """Print formatted results."""
    print("\n" + "=" * 70)
    print("Hodor MCP Gateway - Benchmark Results")
    print("=" * 70)
    print(f"Base URL: {BASE_URL}\n")

    print("LATENCY (ms)")
    print("-" * 70)
    print(f"{'Endpoint':<30} {'p50':>8} {'p95':>8} {'p99':>8} {'avg':>8}")
    print("-" * 70)
    for r in latency_results:
        lat = r["latency_ms"]
        print(
            f"{r['endpoint']:<30} {lat['p50']:>8.1f} {lat['p95']:>8.1f} "
            f"{lat['p99']:>8.1f} {lat['avg']:>8.1f}"
        )
    print("-" * 70)

    print("\nTHROUGHPUT")
    print("-" * 70)
    print(f"Endpoint: {throughput_result['endpoint']}")
    print(f"Duration: {throughput_result['duration_sec']}s")
    print(f"Requests: {throughput_result['total_requests']} ({throughput_result['errors']} errors)")
    print(f"Throughput: {throughput_result['req_per_sec']} req/s")
    print("=" * 70)


def main():
    parser = argparse.ArgumentParser(description="Hodor MCP Gateway benchmark")
    parser.add_argument("--url", default=BASE_URL, help="Base URL")
    parser.add_argument("--requests", "-n", type=int, default=100, help="Requests per endpoint")
    parser.add_argument("--warmup", "-w", type=int, default=5, help="Warmup requests")
    parser.add_argument("--throughput-duration", type=float, default=5.0, help="Throughput test duration (s)")
    parser.add_argument("--json", action="store_true", help="Output JSON only")
    args = parser.parse_args()

    global BASE_URL
    BASE_URL = args.url.rstrip("/")

    try:
        _, status = request("GET", "/health")
        if status != 200:
            print("Error: Hodor not healthy. Start with: make docker-compose-up", file=sys.stderr)
            sys.exit(1)
    except Exception as e:
        print(f"Error: Cannot reach {BASE_URL}/health - {e}", file=sys.stderr)
        sys.exit(1)

    latency_results = []
    for method, path, desc in ENDPOINTS:
        r = run_latency_test(method, path, desc, args.requests, args.warmup)
        latency_results.append(r)

    throughput_result = run_throughput_test("/health", args.throughput_duration)

    if args.json:
        print(json.dumps({"latency": latency_results, "throughput": throughput_result}, indent=2))
    else:
        print_results(latency_results, throughput_result)


if __name__ == "__main__":
    main()
