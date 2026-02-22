# MCP Gateway Compatibility

Hodor implements endpoints and MCP methods compatible with [Microsoft mcp-gateway](https://github.com/microsoft/mcp-gateway) and [Mcp.Gateway.Tools](https://github.com/eyjolfurgudnivatne/mcp.gateway).

## Endpoint Comparison

| Microsoft mcp-gateway | Mcp.Gateway.Tools | Hodor |
|----------------------|-------------------|-------|
| `POST /adapters/{name}/mcp` | — | `GET /adapters/{name}/mcp` → redirects to `/sse` |
| `POST /mcp` | `POST /mcp` | `POST /mcp` ✅ |
| `GET /adapters` | — | `GET /adapters` ✅ |
| `GET /adapters/{name}` | — | `GET /adapters/{name}` ✅ |
| `GET /adapters/{name}/status` | — | `GET /adapters/{name}/status` ✅ |
| `GET /adapters/{name}/logs` | — | `GET /adapters/{name}/logs` ✅ |
| `POST /adapters` | — | `POST /adapters` ✅ |
| `PUT /adapters/{name}` | — | `PUT /adapters/{name}` ✅ |
| `DELETE /adapters/{name}` | — | `DELETE /adapters/{name}` ✅ |
| `GET /tools` | — | `GET /tools` ✅ |
| `GET /tools/{name}` | — | `GET /tools/{name}` ✅ |
| `GET /tools/{name}/status` | — | `GET /tools/{name}/status` ✅ |
| `GET /tools/{name}/logs` | — | `GET /tools/{name}/logs` ✅ |
| — | `GET /mcp` (SSE) | `GET /mcp` → redirects to `/sse` ✅ |
| — | `POST /mcp` | `POST /mcp` ✅ |
| — | cursor + pageSize | `?cursor=&pageSize=` on tools ✅ |
| — | prompts/list, prompts/get | Forwarded to backend ✅ |
| — | resources/list, resources/read | Forwarded to backend ✅ |
| — | MCP-Session-Id | Response header on `/sse` ✅ |
| — | MCP-Protocol-Version | Response header on `/sse` ✅ |

## Pagination (cursor + pageSize)

MCP 2025-11-25 style pagination:

**Request:**
```
GET /api/tools?cursor=eyJvZmZzZXQiOjUwfQ==&pageSize=25
```

**Response:**
```json
{
  "tools": [ ... ],
  "nextCursor": "eyJvZmZzZXQiOjc1fQ=="
}
```

- Cursor: base64-encoded `{"offset": N}`
- Default page size: 100
- Max page size: 500

## MCP JSON-RPC Methods

| Method | Hodor | Notes |
|--------|-------|-------|
| `initialize` | ✅ | Returns protocolVersion, capabilities |
| `tools/list` | ✅ | With optional `cursor`, `pageSize` in params |
| `tools/call` | ✅ | Via hodor-exec or direct |
| `prompts/list` | ✅ | Aggregated from all backend servers |
| `prompts/get` | ✅ | Forwarded to first supporting server |
| `resources/list` | ✅ | Aggregated from all backend servers |
| `resources/read` | ✅ | Forwarded to first supporting server |
| `notifications/initialized` | ✅ | Accepted |

## POST /mcp (Streamable HTTP)

Single request/response without session:

```bash
curl -X POST http://localhost:8080/mcp \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","id":1,"method":"tools/list","params":{}}'
```

Response:
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "tools": [ ... ]
  }
}
```
