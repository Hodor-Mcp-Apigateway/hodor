# Hodor

**Universal MCP API Gateway** — .NET 10 API gateway for the [Model Context Protocol (MCP)](https://modelcontextprotocol.io). One command to add 60+ AI tools to Claude and Cursor. **~98% token savings**, PostgreSQL + pgvector vector DB, Dynamic MCP.

[![Build](https://github.com/Hodor-Mcp-Apigateway/hodor/actions/workflows/build-by-branch.yaml/badge.svg)](https://github.com/Hodor-Mcp-Apigateway/hodor/actions/workflows/build-by-branch.yaml)
[![Release](https://github.com/Hodor-Mcp-Apigateway/hodor/actions/workflows/build-by-tag.yaml/badge.svg)](https://github.com/Hodor-Mcp-Apigateway/hodor/actions/workflows/build-by-tag.yaml)
[![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)](https://github.com/Hodor-Mcp-Apigateway/hodor/releases)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

> **About the name:** Inspired by "Hold the Door" — Hodor is a small gateway that holds the door open between your AI clients and MCP servers. Gateway = gate = door.

---

## Table of Contents

- [Benefits](#benefits)
- [Quick Start](#quick-start)
- [Dynamic MCP & Token Savings](#dynamic-mcp--token-savings)
- [PostgreSQL + pgvector](#postgresql--pgvector-vector-db)
- [Why Hodor](#why-hodor)
- [Features](#features)
- [API Endpoints](#api-endpoints)
- [Configuration](#configuration)
- [Build & Run](#build--run)
- [Deployment](#deployment)
- [GitHub & CI/CD](#github--cicd)
- [Project Structure](#project-structure)
- [Samples](#samples)
- [Documentation](#documentation)

---

## Benefits

| Benefit | Description |
|---------|--------------|
| **~98% token savings** | 60+ tools × ~700 tokens ≈ 42,000 → 3 meta-tools × ~200 tokens ≈ 600 |
| **PostgreSQL + pgvector** | Relational + vector DB; semantic search on tool descriptions (embedding 1536) |
| **Dynamic MCP** | On-demand tool discovery via hodor-find, hodor-exec, hodor-schema |
| **Zero-config** | mcp-config.json optional; built-in memory, fetch, time |
| **Graceful degradation** | Gateway continues if PostgreSQL unavailable |
| **Best practices** | Rate limiting, retry + backoff, circuit breaker, correlation ID |

---

## Quick Start

### Install (one command)

```bash
curl -sSL https://raw.githubusercontent.com/Hodor-Mcp-Apigateway/hodor/main/scripts/install.sh | bash
```

### Or from repo

```bash
git clone https://github.com/Hodor-Mcp-Apigateway/hodor.git
cd hodor
bash scripts/install.sh
```

Alternative: `dotnet run --project src/Hodor.Cli -- install` or `make hodor-install`

### Connect to Claude

```bash
claude mcp add --scope user --transport sse hodor http://localhost:8080/sse
```

### Connect to Cursor

**Settings** → **MCP** → Add server → URL: `http://localhost:8080/sse`

Or: `curl -s http://localhost:8080/config/cursor` for ready-to-paste config.

### Done

Zero config. Built-in servers (memory, fetch, time) work out of the box.

---

## Dynamic MCP & Token Savings

Instead of exposing all tools (which bloats LLM context), Hodor exposes **3 meta-tools**:

| Meta-Tool | Description |
|-----------|-------------|
| **hodor-find** | Search for tools by name, description, or server |
| **hodor-exec** | Execute any tool by `server:tool_name` |
| **hodor-schema** | Get full input schema for a tool |

**Token savings:** 60+ tools × ~700 tokens ≈ 42,000 → 3 meta-tools × ~200 tokens ≈ 600 (**~98% reduction**)

### Auto-Enable on Demand

Disabled servers are discoverable via `hodor-find` and automatically started when you call `hodor-exec`.

---

## PostgreSQL + pgvector (Vector DB)

Hodor uses **PostgreSQL with pgvector** for:

| Capability | Use Case |
|------------|----------|
| **Relational storage** | McpServers, McpTools, ToolCalls |
| **Vector embeddings** | Tool descriptions with `vector(1536)` for semantic search |
| **Cosine similarity** | `vector_cosine_ops` index for fast similarity queries |

Schema: `McpTools` table with optional `Embedding` column for semantic tool discovery.

---

## Why Hodor

| Feature | Airis | Microsoft mcp-gateway | Docker MCP | IBM ContextForge | Composio | Hodor |
|---------|-------|------------------------|------------|------------------|----------|-------|
| **Install** | 6+ containers | K8s-native | Compose-first | Python stack | SaaS | 2 containers (PostgreSQL + Hodor) |
| **Config** | mcp-config required | Azure/K8s config | Docker config | Multi-transport | Hosted | Zero-config, built-in defaults |
| **Startup** | Blocked if unhealthy | Session affinity | 50–200ms overhead | DB-dependent | N/A | Graceful degradation |
| **Buffer** | 65KB stdout | — | — | — | — | 1MB+ configurable |
| **Platform** | Python, Node | C# | Go | Python | SaaS | .NET 10, cross-platform |
| **Token savings** | — | — | — | — | — | ~98% (meta-tools) |
| **Integration** | Manual | Entra ID | Docker Desktop | gRPC-to-MCP | 500+ apps | `/config/claude`, `/config/cursor` |
| **Failures** | Can block gateway | — | — | — | — | Isolated, circuit breaker |
| **Admin UI** | — | — | — | ✅ | ✅ | ❌ |
| **pgvector** | — | — | — | — | — | ✅ Semantic search |

---

## Features

| Feature | Status | Description |
|---------|--------|-------------|
| **.NET 10** | ✅ | Latest LTS |
| **Dynamic MCP** | ✅ | hodor-find, hodor-exec, hodor-schema |
| **Process Manager** | ✅ | npx, uvx, HOT/COLD modes |
| **Auto-enable** | ✅ | Disabled servers start on hodor-exec |
| **MCP SSE** | ✅ | Claude/Cursor compatible transport |
| **PostgreSQL + pgvector** | ✅ | Relational + vector DB, semantic search |
| **REST API** | ✅ | tools, combined, status, process/servers |
| **Prometheus** | ✅ | `/metrics` endpoint |
| **Webhooks** | ✅ | Event-based (tool.call, server.started, server.stopped) |
| **API key auth** | ✅ | Optional `HodorApiKey` Bearer |
| **Rate limiting** | ✅ | Fixed window, configurable, health excluded |
| **Retry + backoff** | ✅ | Tool calls: 2 retries, exponential backoff |
| **Circuit breaker** | ✅ | 30s cooldown after repeated failures |
| **Correlation ID** | ✅ | X-Correlation-ID for audit trail |
| **Unit tests** | ✅ | xUnit, FluentAssertions, Moq |
| **Coverage** | ✅ | Coverlet, cobertura, Codecov |

---

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Liveness probe |
| GET | `/ready` | Readiness + tools count |
| GET | `/health/info` | Detailed health (PostgreSQL) |
| GET | `/api/tools` | Meta-tools or all tools (`?cursor=&pageSize=`) |
| GET | `/api/tools/combined` | All tools (`?cursor=&pageSize=`) |
| GET | `/api/tools/status` | Server status overview |
| GET | `/process/servers` | Process server list |
| GET | `/sse` | MCP SSE (Claude/Cursor) |
| POST | `/messages` | MCP JSON-RPC (SSE session) |
| GET | `/mcp` | Redirects to /sse |
| POST | `/mcp` | MCP streamable HTTP (single request) |
| GET | `/adapters` | List adapters (mcp-gateway compatible) |
| GET/POST/PUT/DELETE | `/adapters/{name}` | Adapter CRUD |
| GET | `/adapters/{name}/mcp` | Redirects to /sse |
| GET | `/tools` | List tools (`?cursor=&pageSize=`) |
| GET | `/tools/{name}` | Get tool by name |
| GET | `/config/claude` | Ready-to-paste Claude config |
| GET | `/config/cursor` | Ready-to-paste Cursor config |
| POST | `/webhooks` | Register webhook URL |
| GET | `/webhooks` | List webhooks |
| DELETE | `/webhooks/{id}` | Unregister webhook |
| GET | `/webhooks/events` | Event types |
| GET | `/metrics` | Prometheus metrics |

---

## Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_URLS` | `http://+:8080` | Listen URLs |
| `ConnectionStrings__PostgreSQL` | See below | PostgreSQL connection |
| `McpConfigPath` | `mcp-config.json` | MCP server config (optional) |
| `DynamicMcp` | `true` | 3 meta-tools vs all tools |
| `ToolCallTimeout` | `90` | Fail-safe timeout (seconds) |
| `ToolCallMaxRetries` | `2` | Retries with exponential backoff |
| `StdoutBufferSize` | `1048576` | 1MB buffer for large MCP responses |
| `RateLimitPermitPerMinute` | `100` | Rate limit per IP (health/metrics excluded) |
| `HodorApiKey` | _(none)_ | Bearer auth (optional) |

**PostgreSQL:** `Host=localhost;Port=5432;Database=hodor;Username=postgres;Password=postgres`

### mcp-config.json (optional)

Zero-config: Hodor runs with built-in defaults (memory, fetch, time) if no config file exists. Override with `mcp-config.json` or `mcp-config.full.json` for 15+ servers.

---

## Build & Run

**Prerequisites:** .NET 10 SDK, Docker (for deploy), Make (optional)

```bash
# Restore & build
make restore && make build
# or: dotnet restore Hodor.slnx && dotnet build Hodor.slnx -c Release

# Test
make test
# or with coverage:
make test-coverage

# Benchmark (requires Hodor running)
make benchmark

# Run locally (requires PostgreSQL)
make run
```

---

## Deployment

### Docker Compose (recommended)

```bash
make docker-compose-up
# or: bash deployment/scripts/deploy.sh docker
```

Scale replicas:

```bash
make docker-compose-scale REPLICAS=3
# or: dotnet run --project src/Hodor.Cli -- scale --replicas 3
```

### Helm (Kubernetes)

```bash
make deploy-helm
# or: bash deployment/scripts/deploy.sh helm
```

### Kind (local Kubernetes)

```bash
make deploy-kind
# or: bash deployment/scripts/deploy.sh kind
```

### Hodor CLI

```bash
dotnet run --project src/Hodor.Cli -- install                    # Install via Docker Compose
dotnet run --project src/Hodor.Cli -- deploy --target docker     # or helm, or kind
dotnet run --project src/Hodor.Cli -- health
dotnet run --project src/Hodor.Cli -- scale --replicas 3
```

---

## GitHub & CI/CD

### Repository

- **GitHub:** [github.com/Hodor-Mcp-Apigateway/hodor](https://github.com/Hodor-Mcp-Apigateway/hodor)
- **Releases:** [Releases](https://github.com/Hodor-Mcp-Apigateway/hodor/releases)
- **Issues:** [Issues](https://github.com/Hodor-Mcp-Apigateway/hodor/issues)

### Build Pipelines

| Workflow | Trigger | Steps |
|----------|---------|-------|
| [build-by-branch.yaml](.github/workflows/build-by-branch.yaml) | Push/PR to `main`, `develop` | Restore → Build → Test with Coverage → Codecov |
| [build-by-tag.yaml](.github/workflows/build-by-tag.yaml) | Push tag `v*` | Restore → Build → Docker build |

**Version:** Set in `Directory.Build.props`. Tag releases as `v1.0.0`, `v1.1.0`, etc.

---

## Project Structure

```
Hodor/
├── src/
│   ├── Hodor.Core                    # MCP protocol, Tool model
│   ├── Hodor.Application.Mcp         # Gateway, meta-tools
│   ├── Hodor.Infrastructure.Core     # Serilog, health checks
│   ├── Hodor.Infrastructure.ProcessManager  # Process spawn, stdio
│   ├── Hodor.Persistence             # EF Core, PostgreSQL, pgvector
│   ├── Hodor.Host                    # ASP.NET Core entry point
│   └── Hodor.Cli                     # CLI (install, deploy, health)
├── tests/
│   ├── Hodor.Application.Mcp.Tests
│   ├── Hodor.Core.Tests
│   ├── Hodor.Host.Tests
│   └── Hodor.Infrastructure.ProcessManager.Tests
├── deployment/
│   ├── docker/                       # Docker Compose
│   ├── helm/hodor/                   # Helm chart
│   ├── kind/                         # Kind config
│   └── scripts/deploy.sh            # Deploy script
├── samples/                          # curl, Python, Go, Rust, etc.
├── mcp-config.json                   # MCP server config
└── docs-public/                      # Public documentation
```

---

## Samples

**Prerequisites:** Hodor running (`make docker-compose-up`), Python 3.8+ or Node.js 18+

**hodor-find → hodor-exec flow (MCP):**
```bash
cd samples
python hodor_find_exec.py --query "memory"
node hodor_find_exec.js -q time -e "time:now"
```

**Quick Start (copy into your project):**
```bash
cd samples/quickstart
python app.py          # Python
node app.js            # Node.js
```

**REST API samples:**
```bash
cd samples
bash curl.sh           # Shell
python python.py       # Python
node javascript.js     # Node.js
go run go.go           # Go
ruby ruby.rb           # Ruby
cargo run              # Rust (from samples/)
bash run-all.sh        # Run all (Linux/Mac)
```

See [samples/README.md](samples/README.md).

---

## Documentation

- [Architecture](docs-public/ARCHITECTURE.md)
- [Status](docs-public/STATUS.md)
- [Roadmap](docs-public/ROADMAP.md)
- [MCP Landscape 2026](docs-public/MCP_LANDSCAPE_2026.md) — Competitor analysis, alternatives, Hodor's position
- [MCP Compatibility](docs-public/MCP_COMPATIBILITY.md) — mcp-gateway compatible endpoints, pagination
- [Webhooks](docs-public/WEBHOOKS.md) — Event-based delivery
- [Benchmark](benchmarks/README.md) — Latency and throughput
- [Contributing](CONTRIBUTING.md)
- [Branch Protection Setup](.github/BRANCH_PROTECTION_SETUP.md)

---

## License

[MIT](LICENSE)
