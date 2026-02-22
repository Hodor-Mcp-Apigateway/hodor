#!/usr/bin/env node
/**
 * Hodor MCP Gateway - hodor-find → hodor-exec flow (Node.js)
 *
 * Demonstrates the full Dynamic MCP flow:
 * 1. Connect to Hodor via MCP SSE
 * 2. hodor-find: Search for tools by query
 * 3. hodor-schema: Get input schema for a tool (optional)
 * 4. hodor-exec: Execute a tool (e.g. memory:create_scratchpad, time:now)
 *
 * Usage: node hodor_find_exec.js [--query "memory"]
 * Requires: Hodor running (make docker-compose-up)
 */

let BASE_URL = process.env.HODOR_URL || "http://localhost:8080";

async function parseSseEndpoint(reader) {
  const decoder = new TextDecoder();
  let buffer = "";
  for await (const chunk of reader) {
    buffer += decoder.decode(chunk);
    const match = buffer.match(/data:\s*(\{.*\})/);
    if (match) {
      const data = JSON.parse(match[1]);
      return data.url;
    }
  }
  return null;
}

async function readSseMessage(reader) {
  const decoder = new TextDecoder();
  let buffer = "";
  let currentData = null;
  for await (const chunk of reader) {
    buffer += decoder.decode(chunk);
    const lines = buffer.split(/\r?\n/);
    buffer = lines.pop() || "";
    for (const line of lines) {
      if (line.startsWith("data:")) {
        currentData = line.slice(5).trim();
      } else if (line === "" && currentData) {
        try {
          return JSON.parse(currentData);
        } catch (_) {}
        currentData = null;
      }
    }
  }
  return null;
}

async function mcpRequest(messagesUrl, method, params, id = 1) {
  await fetch(messagesUrl, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ jsonrpc: "2.0", id, method, params }),
  });
}

async function runHodorFlow(query = "memory", executeTool = null) {
  const sseUrl = `${BASE_URL.replace(/\/$/, "")}/sse`;

  console.log("=== Hodor MCP - hodor-find → hodor-exec flow ===\n");
  console.log(`1. Connecting to ${sseUrl}...`);

  const sseRes = await fetch(sseUrl, { headers: { Accept: "text/event-stream" } });
  if (!sseRes.ok) {
    console.error(`   Error: Cannot connect (${sseRes.status}). Is Hodor running? (make docker-compose-up)`);
    process.exit(1);
  }

  const reader = sseRes.body.getReader();
  const messagesUrl = await parseSseEndpoint(reader);
  if (!messagesUrl) {
    console.error("   Error: Could not parse endpoint from SSE");
    process.exit(1);
  }
  console.log(`   Messages URL: ${messagesUrl}\n`);

  // Initialize
  console.log("2. Sending initialize...");
  await mcpRequest(messagesUrl, "initialize", {}, 1);
  const initResp = await readSseMessage(reader);
  if (initResp?.result) {
    console.log(`   Server: ${initResp.result.serverInfo?.name || "?"}\n`);
  }

  // hodor-find
  console.log(`3. hodor-find(query="${query}")...`);
  await mcpRequest(messagesUrl, "tools/call", { name: "hodor-find", arguments: { query } }, 2);
  const findResp = await readSseMessage(reader);
  if (!findResp?.result) {
    console.error("   Error: No result from hodor-find");
    process.exit(1);
  }

  let toolsData = findResp.result;
  if (toolsData.content?.[0]?.text) {
    toolsData = JSON.parse(toolsData.content[0].text);
  }
  const tools = toolsData.tools || [];
  console.log(`   Found ${tools.length} tool(s):`);
  tools.slice(0, 10).forEach((t) => {
    const name = t.FullName || t.fullName || "?";
    const desc = (t.Description || t.description || "").slice(0, 50);
    console.log(`      - ${name}: ${desc}...`);
  });
  if (tools.length > 10) console.log(`      ... and ${tools.length - 10} more`);
  console.log();

  if (tools.length === 0) {
    console.log("   No tools found. Try a different query.");
    return;
  }

  // Pick tool
  let toolToExec = executeTool;
  if (!toolToExec) {
    const mem = tools.find((t) => {
      const n = (t.FullName || t.fullName || "").toLowerCase();
      return n.includes("memory") && n.includes("create");
    });
    if (mem) toolToExec = mem.FullName || mem.fullName;
    else {
      const time = tools.find((t) => (t.FullName || t.fullName || "").toLowerCase().includes("time"));
      toolToExec = time ? time.FullName || time.fullName : tools[0].FullName || tools[0].fullName;
    }
  }

  // hodor-schema
  console.log(`4. hodor-schema(tool="${toolToExec}")...`);
  await mcpRequest(messagesUrl, "tools/call", { name: "hodor-schema", arguments: { tool: toolToExec } }, 3);
  await readSseMessage(reader);
  console.log("   Schema received\n");

  // hodor-exec
  let execArgs = {};
  if (toolToExec.includes("create_scratchpad") || toolToExec.toLowerCase().includes("create")) {
    execArgs = { type: "create", content: "Hello from Hodor Node.js sample!" };
  } else if (toolToExec.toLowerCase().includes("fetch") || toolToExec.toLowerCase().includes("url")) {
    execArgs = { url: "https://example.com" };
  }

  console.log(`5. hodor-exec(tool="${toolToExec}", arguments=${JSON.stringify(execArgs)})...`);
  await mcpRequest(messagesUrl, "tools/call", {
    name: "hodor-exec",
    arguments: { tool: toolToExec, arguments: execArgs },
  }, 4);
  const execResp = await readSseMessage(reader);
  if (!execResp?.result) {
    if (execResp?.error) console.error("   Error:", execResp.error);
    else console.error("   Error: No result from hodor-exec");
    process.exit(1);
  }

  let result = execResp.result;
  if (result.content?.[0]?.text) {
    try {
      result = JSON.parse(result.content[0].text);
    } catch (_) {
      result = result.content[0].text;
    }
  }
  console.log(`   Result: ${JSON.stringify(result, null, 2).slice(0, 500)}`);
  console.log("\n=== Done ===");
}

const args = process.argv.slice(2);
let query = "memory";
let execute = null;
for (let i = 0; i < args.length; i++) {
  if (args[i] === "--query" || args[i] === "-q") query = args[++i] || "memory";
  else if (args[i] === "--execute" || args[i] === "-e") execute = args[++i];
  else if (args[i] === "--url") BASE_URL = args[++i] ?? BASE_URL;
}

runHodorFlow(query, execute).catch((err) => {
  console.error(err);
  process.exit(1);
});
