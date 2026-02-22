#!/usr/bin/env node
/**
 * Hodor API - Node.js sample (Linux, Mac, Windows)
 * Run: node javascript.js
 */
const BASE_URL = process.env.HODOR_URL || "http://localhost:8080";

async function get(path) {
  const res = await fetch(`${BASE_URL}${path}`);
  return res.json();
}

async function main() {
  console.log("=== Hodor MCP Gateway - Node.js sample ===");
  console.log(`Base URL: ${BASE_URL}\n`);

  console.log("1. Health:", await get("/health"));
  console.log("2. Ready:", await get("/ready"));
  console.log("3. Tools:", JSON.stringify(await get("/api/tools"), null, 2).slice(0, 400) + "...");
}

main().catch(console.error);
