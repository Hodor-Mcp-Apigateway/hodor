# Hodor MCP Gateway - Usage Samples

Examples in multiple languages. Default base URL: `http://localhost:8080`

## Quick Test (REST)

```bash
# Health check
curl http://localhost:8080/health

# List tools
curl http://localhost:8080/api/tools
```

---

## hodor-find → hodor-exec Flow (MCP)

Full Dynamic MCP flow: search tools, get schema, execute.

| File | Language | Run |
|------|----------|-----|
| [hodor_find_exec.py](hodor_find_exec.py) | Python | `python hodor_find_exec.py --query "memory"` |
| [hodor_find_exec.js](hodor_find_exec.js) | Node.js | `node hodor_find_exec.js --query memory` |

**Options:**
- `--query`, `-q` — Search query (default: `memory`)
- `--execute`, `-e` — Tool to run (e.g. `time:now`, `memory:create_scratchpad`)
- `--url` — Hodor base URL

**Example:**
```bash
python hodor_find_exec.py --query "time" --execute "time:now"
node hodor_find_exec.js -q fetch -e "fetch:fetch_url"
```

---

## Quick Start Project

A minimal app that connects to Hodor and calls MCP tools. **Copy into your project.**

| File | Run |
|------|-----|
| [quickstart/app.py](quickstart/app.py) | `python quickstart/app.py` |
| [quickstart/app.js](quickstart/app.js) | `node quickstart/app.js` |

See [quickstart/README.md](quickstart/README.md) for details.

---

## REST API Samples by Language

| File | Language | Run |
|------|----------|-----|
| [curl.sh](curl.sh) | Shell | `./curl.sh` or `bash curl.sh` |
| [python.py](python.py) | Python | `python python.py` |
| [javascript.js](javascript.js) | Node.js | `node javascript.js` |
| [go.go](go.go) | Go | `go run go.go` |
| [rust.rs](rust.rs) | Rust | `cd samples && cargo run` |
| [ruby.rb](ruby.rb) | Ruby | `ruby ruby.rb` |
| [powershell.ps1](powershell.ps1) | PowerShell | `.\powershell.ps1` |
| [csharp.cs](csharp.cs) | C# | Copy to Console app |
| [http](http) | REST Client | VS Code REST Client extension |

**Run all REST samples:** `./run-all.sh` (Linux/Mac)

---

## Environment

```bash
export HODOR_URL=http://localhost:8080   # Linux/Mac
$env:HODOR_URL="http://localhost:8080"   # PowerShell
```

---

## Prerequisites

- **Hodor running:** `make docker-compose-up` (from repo root)
- **Python 3.8+** or **Node.js 18+** for MCP samples
