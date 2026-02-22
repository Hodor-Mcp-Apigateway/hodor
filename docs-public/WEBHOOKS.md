# Hodor Webhooks

Event-based delivery to registered URLs. Single endpoint design.

## Register

```bash
curl -X POST http://localhost:8080/webhooks \
  -H "Content-Type: application/json" \
  -d '{"url":"https://your-server.com/hooks/hodor","events":["tool.call"],"secret":"your-signing-secret"}'
```

**Request:**
```json
{
  "url": "https://example.com/hooks/hodor",
  "events": ["tool.call", "server.started"],
  "secret": "optional-for-hmac-signature",
  "description": "My integration"
}
```

**Response:**
```json
{
  "id": "a1b2c3d4e5f6g7h8",
  "url": "https://example.com/hooks/hodor",
  "events": ["tool.call", "server.started"],
  "createdAt": "2025-02-16T12:00:00Z"
}
```

## Event Types

| Event | When |
|-------|------|
| `tool.call` | After MCP tool execution |
| `server.started` | MCP server process started |
| `server.stopped` | MCP server process stopped |

## Payload Format

```json
{
  "id": "evt_abc123",
  "type": "tool.call",
  "timestamp": "2025-02-16T12:00:00Z",
  "data": {
    "tool": "memory:create_entities",
    "arguments": { "entities": [] },
    "result": { "status": "ok" },
    "durationMs": 123
  }
}
```

## Headers (Best Practice)

| Header | Description |
|--------|-------------|
| `X-Hodor-Event` | Event type |
| `X-Hodor-Delivery` | Event ID (idempotency) |
| `X-Idempotency-Key` | Same as delivery ID |
| `X-Hodor-Signature` | sha256=hmac (if secret set) |

## Verify Signature

```python
import hmac
import hashlib

def verify(payload: bytes, signature: str, secret: str) -> bool:
    expected = "sha256=" + hmac.new(secret.encode(), payload, hashlib.sha256).hexdigest()
    return hmac.compare_digest(expected, signature)
```

## Unregister

```bash
curl -X DELETE http://localhost:8080/webhooks/a1b2c3d4e5f6g7h8
```
