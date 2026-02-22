# Hodor Quick Start — MCP Tool Client

A minimal app that connects to Hodor and calls MCP tools. Use this as a template for your own projects.

## Prerequisites

- **Hodor running:** `make docker-compose-up` (from repo root)
- **Python 3.8+** or **Node.js 18+**

## Quick Run

### Python

```bash
cd samples/quickstart
python app.py
```

### Node.js

```bash
cd samples/quickstart
node app.js
```

## What It Does

1. Connects to Hodor via MCP SSE
2. **hodor-find** — Searches for tools (e.g. "memory", "time", "fetch")
3. **hodor-schema** — Gets input schema for a selected tool
4. **hodor-exec** — Executes a tool (e.g. `memory:create_scratchpad`, `time:now`)

## Customize

- **Query:** `python app.py --query "fetch"` or `node app.js --query fetch`
- **Execute specific tool:** `python app.py --execute "time:now"`
- **Hodor URL:** `HODOR_URL=http://your-host:8080 python app.py`

## Use in Your Project

Copy `app.py` or `app.js` into your project. The MCP client logic is self-contained (no extra dependencies for Python; Node.js uses built-in `fetch`).

## Integration Examples

| Use Case | Tool Example |
|----------|--------------|
| Store notes | `memory:create_scratchpad` |
| Get current time | `time:now` |
| Fetch URL | `fetch:fetch_url` |
| Search tools | `hodor-find` with query |
