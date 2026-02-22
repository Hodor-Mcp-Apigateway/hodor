# Hodor Benchmark — Methodology

## Overview

The benchmark measures latency (p50, p95, p99) and throughput (req/s) for Hodor's REST endpoints.

## Endpoints

| Endpoint | Description |
|----------|-------------|
| `/health` | Liveness (lightweight) |
| `/ready` | Readiness + tools count |
| `/api/tools` | Meta-tools list |
| `/api/tools/combined` | All tools from all servers |
| `/api/tools/status` | Server status |
| `/metrics` | Prometheus (excluded from rate limit) |

## Run

```bash
make benchmark
# or
python benchmarks/run_benchmark.py --requests 100 --warmup 5
```

## Comparison with Other Gateways

| Gateway | p95 Latency | Throughput | Notes |
|---------|-------------|------------|-------|
| **Hodor** | Run `make benchmark` | — | Self-hosted, .NET 10 |
| **TrueFoundry** | <5ms | — | Managed, platform |
| **Lunar.dev MCPX** | ~4ms p99 | — | Enterprise |
| **Docker MCP** | 50–200ms overhead | — | Container startup |
| **Lasso Security** | 100–250ms | — | Security layer |

To compare: run the same workload (e.g. 100 requests to `/health`) against each gateway and compare p95/p99.

## Token Savings (Hodor-Specific)

Hodor's ~98% token savings is not a latency metric—it reduces LLM context size. Measure by comparing:

- **Without Hodor:** 60+ tools × ~700 tokens ≈ 42,000 tokens in context
- **With Hodor:** 3 meta-tools × ~200 tokens ≈ 600 tokens
