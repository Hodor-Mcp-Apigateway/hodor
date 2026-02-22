#!/usr/bin/env python3
"""
Hodor Quick Start - MCP Tool Client (Python)

Connects to Hodor and runs hodor-find -> hodor-exec flow.
Copy this file into your project to use Hodor from Python.

Usage: python app.py [--query "memory"] [--execute "time:now"]
"""
import argparse
import json
import os
import sys
import urllib.request
import urllib.error

BASE_URL = os.environ.get("HODOR_URL", "http://localhost:8080")


class HodorClient:
    """Minimal MCP client for Hodor. No external dependencies."""

    def __init__(self, base_url):
        self.base_url = base_url.rstrip("/")
        self.stream = None
        self.messages_url = None

    def connect(self):
        """Connect to Hodor SSE, get messages endpoint."""
        req = urllib.request.Request(f"{self.base_url}/sse")
        req.add_header("Accept", "text/event-stream")
        self.stream = urllib.request.urlopen(req, timeout=10)
        for line in self.stream:
            line = line.decode("utf-8").strip()
            if line.startswith("data:"):
                data = json.loads(line[5:].strip())
                self.messages_url = data.get("url")
                return self.messages_url
        return None

    def _request(self, method, params, req_id=1):
        """Send MCP JSON-RPC request."""
        payload = {"jsonrpc": "2.0", "id": req_id, "method": method, "params": params}
        r = urllib.request.Request(
            self.messages_url,
            data=json.dumps(payload).encode("utf-8"),
            headers={"Content-Type": "application/json"},
            method="POST",
        )
        urllib.request.urlopen(r, timeout=60)

    def _read_response(self):
        """Read next SSE message."""
        current = None
        for line in self.stream:
            line = line.decode("utf-8").rstrip("\n\r")
            if line.startswith("data:"):
                current = line[5:].strip()
            elif line == "" and current:
                try:
                    return json.loads(current)
                except json.JSONDecodeError:
                    pass
                current = None
        return None

    def find_tools(self, query=""):
        """hodor-find: Search tools by query."""
        self._request("tools/call", {"name": "hodor-find", "arguments": {"query": query}}, 2)
        resp = self._read_response()
        if not resp or "result" not in resp:
            return []
        r = resp["result"]
        if r.get("content"):
            r = json.loads(r["content"][0].get("text", "{}"))
        return r.get("tools", [])

    def exec_tool(self, tool, arguments=None):
        """hodor-exec: Execute a tool by server:tool_name."""
        self._request("tools/call", {"name": "hodor-exec", "arguments": {"tool": tool, "arguments": arguments or {}}}, 3)
        resp = self._read_response()
        if not resp or "result" not in resp:
            raise RuntimeError(resp.get("error", "No result"))
        r = resp["result"]
        if r.get("content"):
            return json.loads(r["content"][0].get("text", "{}"))
        return r


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--query", "-q", default="memory")
    parser.add_argument("--execute", "-e", help="Tool to run (server:tool_name)")
    parser.add_argument("--url", default=BASE_URL)
    args = parser.parse_args()

    url = args.url.rstrip("/")
    print("Hodor Quick Start - Connecting to", url)

    try:
        client = HodorClient(url)
        client.connect()
    except urllib.error.URLError as e:
        print("Error: Cannot connect. Is Hodor running? (make docker-compose-up)")
        sys.exit(1)

    # Initialize
    client._request("initialize", {}, 1)
    client._read_response()

    # Find tools
    tools = client.find_tools(args.query)
    print(f"Found {len(tools)} tools for query '{args.query}'")
    for t in tools[:5]:
        name = t.get("FullName", t.get("fullName", "?"))
        print(f"  - {name}")

    if not tools:
        print("No tools found.")
        return

    # Pick tool
    tool = args.execute
    if not tool:
        for t in tools:
            n = t.get("FullName", t.get("fullName", ""))
            if "time" in n.lower():
                tool = n
                break
        if not tool:
            tool = tools[0].get("FullName", tools[0].get("fullName"))

    # Execute
    args_dict = {}
    if "create" in tool.lower():
        args_dict = {"type": "create", "content": "Hello from Quick Start!"}
    elif "fetch" in tool.lower() or "url" in tool.lower():
        args_dict = {"url": "https://example.com"}
    print(f"\nExecuting {tool}...")
    result = client.exec_tool(tool, args_dict)
    print("Result:", json.dumps(result, indent=2)[:400])
    print("\nDone.")


if __name__ == "__main__":
    main()
