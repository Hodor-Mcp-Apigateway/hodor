#!/usr/bin/env python3
"""Hodor API - Python sample (Linux, Mac, Windows)"""
import os
import json
import urllib.request

BASE_URL = os.environ.get("HODOR_URL", "http://localhost:8080")


def get(path: str) -> dict:
    req = urllib.request.Request(f"{BASE_URL}{path}")
    with urllib.request.urlopen(req, timeout=5) as r:
        return json.loads(r.read().decode())


def main():
    print("=== Hodor MCP Gateway - Python sample ===")
    print(f"Base URL: {BASE_URL}\n")

    print("1. Health:", get("/health"))
    print("2. Ready:", get("/ready"))
    print("3. Tools:", json.dumps(get("/api/tools"), indent=2)[:500], "...")


if __name__ == "__main__":
    main()
