# MCP Gateway Landscape 2026 — Research Summary

This document summarizes the MCP (Model Context Protocol) gateway ecosystem for 2025–2026. It evaluates Hodor's position in this ecosystem and its unique features.

---

## 1. What is an MCP Gateway?

An MCP Gateway provides a central control layer between AI clients (Claude, Cursor, VS Code) and MCP servers. Core functions:

- **Security:** Authentication, authorization, credential management
- **Observability:** OpenTelemetry, logging, metrics
- **Cost control:** Rate limiting
- **Governance:** RBAC, audit trail

---

## 2. 2026 MCP Gateway Comparison

### Open Source / Self-Hosted

| Gateway | Language | Highlights | Gaps / Weaknesses |
|---------|----------|------------|-------------------|
| **Hodor** | .NET 10 | ~98% token savings (Dynamic MCP), pgvector, zero-config, circuit breaker, retry | No Admin UI, no gRPC/GraphQL |
| **Microsoft mcp-gateway** | C# | K8s-native, session affinity, Entra ID, tool router | Complex, Azure-focused |
| **Docker MCP Gateway** | Go | Docker Desktop integration, Compose-first | 50–200ms container overhead |
| **IBM ContextForge** | Python | Admin UI, multi-transport, federation, gRPC-to-MCP | Python stack, heavy |
| **Obot** | — | K8s-native, self-hosted, IdP support | You manage your own infra |
| **Lasso Security** | — | Threat detection, PII redaction | 100–250ms security overhead |

### Managed Services

| Gateway | Highlights | Target Audience |
|---------|------------|-----------------|
| **Composio** | 500+ integrations, unified auth | Default for most teams |
| **TrueFoundry** | <5ms p95 latency | Platform teams |
| **Lunar.dev MCPX** | RBAC, audit log, ~4ms p99 | Enterprise governance |
| **Zapier** | 8,000+ apps | Prototyping, SMB |
| **Workato** | 12,000+ enterprise apps | Workato users |
| **MintMCP** | SOC 2, HIPAA, GDPR | Regulated industries |

---

## 3. Hodor's Unique Features

### Missing or Weak in Other Gateways

| Feature | Hodor | Others |
|---------|-------|--------|
| **~98% token savings** | Yes | Most expose all tools |
| **.NET 10** | Yes | Mostly Go, Python |
| **Zero-config** | Yes | mcp-config usually required |
| **pgvector** | Yes | Rare |
| **Graceful degradation** | Yes | Most depend on DB |
| **1MB stdout buffer** | Yes | Airis 65KB limit |
| **Circuit breaker + retry** | Yes | Best practice, few have it |
| **One-command install** | Yes | Similar to Docker/Composio |

### Hodor's Gaps (Improvable)

| Feature | Status | Note |
|---------|--------|------|
| Admin UI | No | ContextForge, Obot have it |
| MCP 2025-06-18 | Partial | Elicitation, resource links not yet |
| OAuth 2.1 | No | API key only |
| gRPC transport | No | ContextForge has it |
| Dynamic tool registration | No | Static via mcp-config |

---

## 4. MCP Specification 2025-06-18

Key changes in the current MCP version:

- **Elicitation:** Servers can request additional info from the user
- **Resource links:** Resource links in tool results
- **Structured tool output:** Enhanced tool response
- **OAuth 2.1:** Authorization separation
- **JSON-RPC batching removed**
- **`title` field:** Human-friendly names

---

## 5. Recommendations

### Short-Term Improvements for Hodor

1. **MCP 2025-06-18 compliance** — Protocol version update
2. **Admin UI (optional)** — Simple dashboard
3. **OAuth 2.0/2.1** — For enterprise scenarios

### Preserve Competitive Advantages

- Token savings approach (Dynamic MCP) is unique; document it
- Emphasize zero-config and one-command install
- Highlight .NET 10, performance, and cross-platform benefits

---

## 6. Resources

- [MCP Specification 2025-06-18](https://modelcontextprotocol.io/specification/2025-06-18/basic)
- [MCP Best Practices](https://mcp-best-practice.github.io/mcp-best-practice/best-practice/)
- [Microsoft mcp-gateway](https://github.com/microsoft/mcp-gateway)
- [IBM ContextForge](https://ibm.github.io/mcp-context-forge/)
- [Docker MCP Gateway](https://docs.docker.com/ai/mcp-gateway)
- [Composio: 10 Best MCP Gateways 2026](https://composio.dev/blog/best-mcp-gateway-for-developers)
- [MCP C# SDK 2025-06-18](https://devblogs.microsoft.com/dotnet/mcp-csharp-sdk-2025-06-18-update/)

---

*Last updated: February 2026*
