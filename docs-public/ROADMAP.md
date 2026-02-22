# Hodor Roadmap

## Phase 1: Core ✅ (Complete)

- [x] Clean architecture skeleton
- [x] PostgreSQL + pgvector schema
- [x] REST health, ready, tools endpoints
- [x] Meta-tools (hodor-find, hodor-exec, hodor-schema)
- [x] Process Manager (spawn MCP servers)
- [x] Full MCP SSE (Claude/Cursor compatible)
- [x] Webhooks (tool.call, server.started, server.stopped)
- [x] Rate limiting, retry, circuit breaker
- [x] Unit and integration tests

## Phase 2: Protocol Adapters

- [ ] gRPC transport
- [ ] GraphQL
- [ ] WebSocket
- [ ] Admin UI (optional dashboard)

## Phase 3: Production Hardening

- [ ] MCP 2025-06-18 full compliance (elicitation, resource links)
- [ ] OAuth 2.0/2.1 for enterprise
- [ ] Semantic search in hodor-find (pgvector embeddings)
