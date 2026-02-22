# mcp-gateway Compatibility Guide

This document describes Hodor's compatibility with [Microsoft mcp-gateway](https://github.com/microsoft/mcp-gateway) and [Mcp.Gateway.Tools](https://github.com/eyjolfurgudnivatne/mcp.gateway).

## Adapter API (mcp-gateway compatible)

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/adapters` | GET | List all adapters (MCP servers) |
| `/adapters/{name}` | GET | Get adapter details |
| `/adapters/{name}/status` | GET | Adapter runtime status |
| `/adapters/{name}/logs` | GET | Adapter process logs |
| `/adapters/{name}/mcp` | GET | Redirect to SSE endpoint |
| `/adapters` | POST | Create new adapter |
| `/adapters/{name}` | PUT | Update adapter config |
| `/adapters/{name}` | DELETE | Remove adapter |

## Tools API (mcp-gateway compatible)

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/tools` | GET | List all tools (with pagination) |
| `/tools/{name}` | GET | Get tool details |
| `/tools/{name}/status` | GET | Tool server status |
| `/tools/{name}/logs` | GET | Tool server logs |

## MCP Streamable HTTP

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/mcp` | POST | MCP JSON-RPC (single request/response) |
| `/mcp` | GET | Redirect to `/sse` |
| `/sse` | GET | SSE transport (Claude/Cursor) |
| `/messages` | POST | MCP JSON-RPC via SSE session |

## Pagination

Cursor-based pagination on tools endpoints (MCP 2025-11-25 compatible):

```
GET /api/tools?cursor=eyJvZmZzZXQiOjUwfQ==&pageSize=25
```

Response includes `nextCursor` when more results are available:
```json
{
  "tools": [ ... ],
  "nextCursor": "eyJvZmZzZXQiOjc1fQ=="
}
```

- Cursor format: base64-encoded `{"offset": N}`
- Default page size: 100
- Max page size: 500

## MCP JSON-RPC Methods

Supported via `POST /mcp` and `POST /messages`:

| Method | Description |
|--------|-------------|
| `initialize` | Protocol handshake, returns capabilities |
| `tools/list` | List tools (with optional `cursor`, `pageSize` in params) |
| `tools/call` | Execute a tool |
| `prompts/list` | Aggregated from all backend servers |
| `prompts/get` | Forwarded to first supporting server |
| `resources/list` | Aggregated from all backend servers |
| `resources/read` | Forwarded to first supporting server |
| `notifications/initialized` | Accepted |

## Headers

| Header | Description |
|--------|-------------|
| `MCP-Session-Id` | Returned on `/sse` connection |
| `MCP-Protocol-Version` | Protocol version (`2024-11-05`) |
| `X-Correlation-ID` | Request tracing (auto-generated if not provided) |

## Create Adapter Example

```bash
curl -X POST http://localhost:8080/adapters \
  -H "Content-Type: application/json" \
  -d '{
    "name": "github",
    "command": "npx",
    "args": ["-y", "@modelcontextprotocol/server-github"],
    "enabled": true,
    "mode": "cold"
  }'
```

## POST /mcp Example

```bash
curl -X POST http://localhost:8080/mcp \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","id":1,"method":"tools/list","params":{}}'
```
