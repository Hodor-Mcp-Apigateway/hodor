# Hodor - Project Status

## Current State

Hodor is a **production-ready** MCP gateway with Dynamic MCP, Process Manager, full SSE transport, webhooks, and best practices (rate limiting, retry, circuit breaker).

## Feature Matrix

| Feature | Status | Notes |
|---------|--------|-------|
| **Meta-tools** (find, exec, schema) | ✅ | hodor-find, hodor-exec, hodor-schema |
| **Tool execution** | ✅ | Via Process Manager |
| **Process Manager** | ✅ | npx, uvx, HOT/COLD modes |
| **SSE endpoint** | ✅ | Full MCP SSE (Claude/Cursor) |
| **Auto-enable** | ✅ | Disabled servers start on hodor-exec |
| **REST API** | ✅ | tools, combined, status, process/servers |
| **PostgreSQL + pgvector** | ✅ | Schema ready, auto-migrations |
| **Prometheus metrics** | ✅ | /metrics |
| **API key auth** | ✅ | Optional HodorApiKey |
| **Webhooks** | ✅ | tool.call, server.started, server.stopped |
| **Rate limiting** | ✅ | Configurable, health excluded |
| **Retry + backoff** | ✅ | Tool calls |
| **Circuit breaker** | ✅ | 30s cooldown |
| **Correlation ID** | ✅ | X-Correlation-ID |
| **gRPC** | ❌ | Planned |
| **GraphQL** | ❌ | Planned |
| **WebSocket** | ❌ | Planned |
| **Admin UI** | ❌ | Planned |

## API Endpoints

| Endpoint | Status | Notes |
|----------|--------|------|
| `GET /health` | ✅ | Liveness |
| `GET /ready` | ✅ | Readiness + tools count |
| `GET /health/info` | ✅ | Detailed (PostgreSQL) |
| `GET /api/tools` | ✅ | Meta-tools or all tools (cursor, pageSize) |
| `GET /api/tools/combined` | ✅ | All tools (cursor, pageSize) |
| `GET /api/tools/status` | ✅ | Server status |
| `GET /process/servers` | ✅ | Process server list |
| `GET /sse` | ✅ | MCP SSE transport (MCP-Session-Id, MCP-Protocol-Version) |
| `POST /messages` | ✅ | MCP JSON-RPC (SSE session) |
| `GET /mcp` | ✅ | Redirects to /sse |
| `POST /mcp` | ✅ | MCP streamable HTTP (single request/response) |
| `GET /adapters` | ✅ | List adapters (mcp-gateway compatible) |
| `GET /adapters/{name}` | ✅ | Get adapter |
| `GET /adapters/{name}/status` | ✅ | Adapter status |
| `GET /adapters/{name}/logs` | ✅ | Adapter logs |
| `GET /adapters/{name}/mcp` | ✅ | Redirects to /sse |
| `POST /adapters` | ✅ | Create adapter |
| `PUT /adapters/{name}` | ✅ | Update adapter |
| `DELETE /adapters/{name}` | ✅ | Delete adapter |
| `GET /tools` | ✅ | List tools (cursor, pageSize) |
| `GET /tools/{name}` | ✅ | Get tool |
| `GET /tools/{name}/status` | ✅ | Tool status |
| `GET /tools/{name}/logs` | ✅ | Tool logs |
| `GET /config/claude` | ✅ | Ready-to-paste Claude config |
| `GET /config/cursor` | ✅ | Ready-to-paste Cursor config |
| `POST /webhooks` | ✅ | Register webhook |
| `GET /webhooks` | ✅ | List webhooks |
| `DELETE /webhooks/{id}` | ✅ | Unregister webhook |
| `GET /webhooks/events` | ✅ | Event types |
| `GET /metrics` | ✅ | Prometheus |
| DB migrations | ✅ | Auto-runs on startup |

### MCP JSON-RPC Methods (via POST /messages or POST /mcp)

| Method | Status | Notes |
|--------|--------|-------|
| `initialize` | ✅ | Protocol version, capabilities |
| `tools/list` | ✅ | With optional cursor, pageSize |
| `tools/call` | ✅ | With webhook dispatch |
| `prompts/list` | ✅ | Forwarded to backend, aggregated |
| `prompts/get` | ✅ | Forwarded to backend |
| `resources/list` | ✅ | Forwarded to backend, aggregated |
| `resources/read` | ✅ | Forwarded to backend |
| `notifications/initialized` | ✅ | Accepted |
