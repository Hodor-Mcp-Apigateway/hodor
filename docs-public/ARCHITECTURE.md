# Hodor - Universal MCP Gateway Architecture

## Vision

**Hodor** is a universal Model Context Protocol (MCP) gateway built on .NET 10. Access MCP tools from any protocol—REST, gRPC, GraphQL, WebSocket, SSE, MQTT, Webhook, and SOAP.

## Design Principles

| Principle | Implementation |
|-----------|----------------|
| **Graceful degradation** | Optional servers; failures do not block the gateway |
| **Configurable buffers** | 1MB+ stdout buffer for large MCP responses |
| **Cross-platform** | Consistent line endings, Docker support |
| **Standard MCP spec** | Cursor/Claude compatible SSE |
| **Clean error logging** | Detailed exceptions, LastError for debugging |

## Protocol Support

| Protocol | Endpoint | Use Case |
|----------|----------|----------|
| **REST** | `/api/mcp/*` | JSON-RPC over HTTP |
| **gRPC** | `McpGatewayService` | High performance |
| **GraphQL** | `/graphql` | Tool discovery, subscription |
| **WebSocket** | `/ws/mcp` | Real-time, SSE alternative |
| **SSE** | `/sse` | Claude/Cursor compatible |
| **MQTT** | `hodor/mcp/request` | IoT, pub/sub |
| **Webhook** | `POST /webhook/{id}` | Event-driven |
| **SOAP** | `/soap/mcp` | Legacy integration |

## Layer Structure

```
Hodor.Core          → MCP protocol, Tool model, JSON-RPC
Hodor.Application  → Tool discovery, execution, process management
Hodor.Infrastructure → Process runner, external MCP clients
Hodor.Adapters      → REST, gRPC, GraphQL, WebSocket, MQTT, Webhook, SOAP
Hodor.Host          → Entry point, DI
```

## Data Storage

PostgreSQL with **pgvector** extension for relational and vector data:

| Table | Purpose |
|-------|---------|
| McpServers | Registered MCP servers (command, args, env) |
| McpTools | Discovered tools with optional embeddings (vector 1536) |
| ToolCalls | Execution history (arguments, result, duration) |

- **Vector DB**: pgvector for semantic search on tool descriptions
- **Relational**: All MCP metadata stored in PostgreSQL
