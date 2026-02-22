# Hodor Benchmark

Latency and throughput benchmark for Hodor MCP Gateway.

## Prerequisites

- Hodor running: `make docker-compose-up`

## Run

```bash
cd benchmarks
python run_benchmark.py
```

## Options

| Option | Default | Description |
|--------|---------|-------------|
| `--url` | `HODOR_URL` or `http://localhost:8080` | Base URL |
| `--requests`, `-n` | 100 | Requests per endpoint |
| `--warmup`, `-w` | 5 | Warmup requests |
| `--throughput-duration` | 5.0 | Throughput test duration (seconds) |
| `--json` | — | Output JSON only |

## Example

```bash
python run_benchmark.py --requests 200 --json > results.json
HODOR_URL=http://prod:8080 python run_benchmark.py
```
