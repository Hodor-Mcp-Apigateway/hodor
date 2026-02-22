#!/usr/bin/env node
/**
 * Hodor Quick Start — MCP Tool Client (Node.js)
 *
 * Connects to Hodor and runs hodor-find → hodor-exec flow.
 * Copy this file into your project to use Hodor from Node.js.
 *
 * Usage: node app.js [--query memory] [--execute time:now]
 */

const BASE_URL = process.env.HODOR_URL || "http://localhost:8080";

class HodorClient {
  constructor(baseUrl) {
    this.baseUrl = baseUrl.replace(/\/$/, "");
    this.reader = null;
    this.messagesUrl = null;
  }

  async connect() {
    const res = await fetch(`${this.baseUrl}/sse`, { headers: { Accept: "text/event-stream" } });
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    this.reader = res.body.getReader();
    const url = await this._parseEndpoint();
    this.messagesUrl = url;
    return url;
  }

  async _parseEndpoint() {
    const decoder = new TextDecoder();
    let buf = "";
    for (;;) {
      const { done, value } = await this.reader.read();
      if (done) return null;
      buf += decoder.decode(value);
      const m = buf.match(/data:\s*(\{.*\})/);
      if (m) return JSON.parse(m[1]).url;
    }
  }

  async _request(method, params, id = 1) {
    await fetch(this.messagesUrl, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ jsonrpc: "2.0", id, method, params }),
    });
  }

  async _readResponse() {
    const decoder = new TextDecoder();
    let buf = "";
    let current = null;
    for (;;) {
      const { done, value } = await this.reader.read();
      if (done) return null;
      buf += decoder.decode(value);
      const lines = buf.split(/\r?\n/);
      buf = lines.pop() || "";
      for (const line of lines) {
        if (line.startsWith("data:")) current = line.slice(5).trim();
        else if (line === "" && current) {
          try {
            return JSON.parse(current);
          } catch (_) {}
          current = null;
        }
      }
    }
  }

  async findTools(query = "") {
    await this._request("tools/call", { name: "hodor-find", arguments: { query } }, 2);
    const resp = await this._readResponse();
    if (!resp?.result) return [];
    let r = resp.result;
    if (r.content?.[0]?.text) r = JSON.parse(r.content[0].text);
    return r.tools || [];
  }

  async execTool(tool, args = {}) {
    await this._request("tools/call", { name: "hodor-exec", arguments: { tool, arguments: args } }, 3);
    const resp = await this._readResponse();
    if (!resp?.result) throw new Error(resp?.error?.message || "No result");
    let r = resp.result;
    if (r.content?.[0]?.text) r = JSON.parse(r.content[0].text);
    return r;
  }
}

async function main() {
  const args = process.argv.slice(2);
  let query = "memory";
  let execute = null;
  let url = BASE_URL;
  for (let i = 0; i < args.length; i++) {
    if (args[i] === "--query" || args[i] === "-q") query = args[++i] ?? "memory";
    else if (args[i] === "--execute" || args[i] === "-e") execute = args[++i];
    else if (args[i] === "--url") url = args[++i] ?? url;
  }

  console.log("Hodor Quick Start — Connecting to", url);

  const client = new HodorClient(url);
  try {
    await client.connect();
  } catch (e) {
    console.error("Error: Cannot connect. Is Hodor running? (make docker-compose-up)");
    process.exit(1);
  }

  await client._request("initialize", {}, 1);
  await client._readResponse();

  const tools = await client.findTools(query);
  console.log(`Found ${tools.length} tools for query '${query}'`);
  tools.slice(0, 5).forEach((t) => {
    const name = t.FullName || t.fullName || "?";
    console.log(`  - ${name}`);
  });

  if (tools.length === 0) {
    console.log("No tools found.");
    return;
  }

  let tool = execute;
  if (!tool) {
    const t = tools.find((x) => (x.FullName || x.fullName || "").toLowerCase().includes("time"));
    tool = t ? t.FullName || t.fullName : tools[0].FullName || tools[0].fullName;
  }

  let execArgs = {};
  if (tool.toLowerCase().includes("create")) execArgs = { type: "create", content: "Hello from Quick Start!" };
  else if (tool.toLowerCase().includes("fetch") || tool.toLowerCase().includes("url")) execArgs = { url: "https://example.com" };
  console.log(`\nExecuting ${tool}...`);
  const result = await client.execTool(tool, execArgs);
  console.log("Result:", JSON.stringify(result, null, 2).slice(0, 400));
  console.log("\nDone.");
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
