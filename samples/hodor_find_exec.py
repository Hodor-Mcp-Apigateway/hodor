#!/usr/bin/env python3
"""
Hodor MCP Gateway - hodor-find → hodor-exec flow (Python)

Demonstrates the full Dynamic MCP flow:
1. Connect to Hodor via MCP SSE
2. hodor-find: Search for tools by query
3. hodor-schema: Get input schema for a tool (optional)
4. hodor-exec: Execute a tool (e.g. memory:create_scratchpad, time:now)

Usage: python hodor_find_exec.py [--query "memory"]
Requires: Hodor running (make docker-compose-up)
"""
import argparse
import json
import os
import sys
import urllib.request
import urllib.error

BASE_URL = os.environ.get("HODOR_URL", "http://localhost:8080")


def parse_sse_endpoint(stream):
    """Parse first SSE event to get messages endpoint URL."""
    for line in stream:
        line = line.decode("utf-8").strip()
        if line.startswith("data:"):
            data = line[5:].strip()
            try:
                obj = json.loads(data)
                return obj.get("url")
            except json.JSONDecodeError:
                pass
    return None


def mcp_request(messages_url: str, method: str, params: dict, request_id: int = 1) -> dict:
    """Send MCP JSON-RPC request via POST /messages."""
    payload = {
        "jsonrpc": "2.0",
        "id": request_id,
        "method": method,
        "params": params,
    }
    req = urllib.request.Request(
        messages_url,
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    with urllib.request.urlopen(req, timeout=60) as r:
        # 202 Accepted - response comes via SSE
        pass
    return {"id": request_id}


def read_sse_response(stream) -> dict | None:
    """Read next SSE message event, return parsed JSON."""
    current_data = None
    for line in stream:
        line = line.decode("utf-8").rstrip("\n\r")
        if line.startswith("event:"):
            pass
        elif line.startswith("data:"):
            current_data = line[5:].strip()
        elif line == "" and current_data:
            try:
                return json.loads(current_data)
            except json.JSONDecodeError:
                pass
            current_data = None
    return None


def run_hodor_flow(query: str = "memory", execute_tool: str | None = None):
    """Full hodor-find → hodor-exec flow."""
    sse_url = f"{BASE_URL.rstrip('/')}/sse"

    print("=== Hodor MCP - hodor-find → hodor-exec flow ===\n")
    print(f"1. Connecting to {sse_url}...")

    req = urllib.request.Request(sse_url)
    req.add_header("Accept", "text/event-stream")
    try:
        stream = urllib.request.urlopen(req, timeout=10)
    except urllib.error.URLError as e:
        print(f"   Error: Cannot connect. Is Hodor running? (make docker-compose-up)\n   {e}")
        sys.exit(1)

    # Parse endpoint URL from first SSE event
    messages_url = parse_sse_endpoint(stream)
    if not messages_url:
        print("   Error: Could not parse endpoint from SSE")
        sys.exit(1)
    print(f"   Messages URL: {messages_url}\n")

    # Initialize (MCP handshake)
    print("2. Sending initialize...")
    mcp_request(messages_url, "initialize", {}, 1)
    resp = read_sse_response(stream)
    if resp and "result" in resp:
        print(f"   Server: {resp['result'].get('serverInfo', {}).get('name', '?')}\n")
    else:
        print("   (initialize response received)\n")

    # hodor-find
    print(f"3. hodor-find(query=\"{query}\")...")
    mcp_request(messages_url, "tools/call", {"name": "hodor-find", "arguments": {"query": query}}, 2)
    resp = read_sse_response(stream)
    if not resp or "result" not in resp:
        print("   Error: No result from hodor-find")
        sys.exit(1)

    # Extract tools from result (MCP wraps in content[].text)
    result = resp["result"]
    if "content" in result and result["content"]:
        text = result["content"][0].get("text", "{}")
        tools_data = json.loads(text)
    else:
        tools_data = result

    tools = tools_data.get("tools", [])
    print(f"   Found {len(tools)} tool(s):")
    for t in tools[:10]:
        print(f"      - {t.get('FullName', t.get('fullName', '?'))}: {t.get('Description', t.get('description', ''))[:50]}...")
    if len(tools) > 10:
        print(f"      ... and {len(tools) - 10} more")
    print()

    if not tools:
        print("   No tools found. Try a different query or ensure servers are enabled.")
        return

    # Pick tool to execute
    tool_to_exec = execute_tool
    if not tool_to_exec:
        # Prefer memory:create_scratchpad or time:now
        for t in tools:
            fn = t.get("FullName", t.get("fullName", ""))
            if "memory" in fn.lower() and "create" in fn.lower():
                tool_to_exec = fn
                break
        if not tool_to_exec:
            for t in tools:
                fn = t.get("FullName", t.get("fullName", ""))
                if "time" in fn.lower():
                    tool_to_exec = fn
                    break
        if not tool_to_exec:
            tool_to_exec = tools[0].get("FullName", tools[0].get("fullName", ""))

    # hodor-schema (optional - get input schema)
    print(f"4. hodor-schema(tool=\"{tool_to_exec}\")...")
    mcp_request(messages_url, "tools/call", {"name": "hodor-schema", "arguments": {"tool": tool_to_exec}}, 3)
    schema_resp = read_sse_response(stream)
    if schema_resp and "result" in schema_resp:
        print("   Schema received (see input parameters)")
    print()

    # hodor-exec
    exec_args = {}
    if "create_scratchpad" in tool_to_exec or "create" in tool_to_exec.lower():
        exec_args = {"type": "create", "content": "Hello from Hodor Python sample!"}
    elif "fetch" in tool_to_exec.lower() or "url" in tool_to_exec.lower():
        exec_args = {"url": "https://example.com"}
    elif "time" in tool_to_exec.lower():
        exec_args = {}

    print(f"5. hodor-exec(tool=\"{tool_to_exec}\", arguments={exec_args})...")
    mcp_request(messages_url, "tools/call", {"name": "hodor-exec", "arguments": {"tool": tool_to_exec, "arguments": exec_args}}, 4)
    exec_resp = read_sse_response(stream)
    if not exec_resp or "result" not in exec_resp:
        if exec_resp and "error" in exec_resp:
            print(f"   Error: {exec_resp['error']}")
        else:
            print("   Error: No result from hodor-exec")
        sys.exit(1)

    result = exec_resp["result"]
    if "content" in result and result["content"]:
        text = result["content"][0].get("text", "{}")
        try:
            out = json.loads(text)
            print(f"   Result: {json.dumps(out, indent=2)[:500]}")
        except json.JSONDecodeError:
            print(f"   Result: {text[:300]}")
    else:
        print(f"   Result: {result}")
    print("\n=== Done ===")


def main():
    parser = argparse.ArgumentParser(description="Hodor hodor-find → hodor-exec flow")
    parser.add_argument("--query", "-q", default="memory", help="Search query for hodor-find")
    parser.add_argument("--execute", "-e", help="Tool to execute (server:tool_name). Auto-picked if omitted.")
    parser.add_argument("--url", default=BASE_URL, help="Hodor base URL")
    args = parser.parse_args()

    global BASE_URL
    BASE_URL = args.url.rstrip("/")

    run_hodor_flow(query=args.query, execute_tool=args.execute)


if __name__ == "__main__":
    main()
