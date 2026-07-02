# Lesson 3.3: AI agents over MCP

> **Difficulty:** Intermediate | **Duration:** 12 min | **Prerequisites:** [Lesson 3.1](../lesson-1/README.md); Claude Desktop or Cursor

## Overview

Expose Bowire's toolset to an AI agent over **MCP** (Model Context Protocol) so it can discover services, invoke methods and drive recordings/mocks in plain English. The CLI path is **stdio**: the agent spawns `bowire mcp serve` as a subprocess and pipes JSON-RPC — the Claude-Desktop / Cursor default, no port to manage.

> The **embedded / HTTP** variant (`AddBowireMcpAdapter()` / `MapBowireMcpAdapter()`, one shared endpoint) is coding — it lives in [Unit 4: Embed Bowire](../../unit-4/README.md).

## Steps

### 1. Sanity-check the stdio server

```bash
echo '{"jsonrpc":"2.0","id":1,"method":"tools/list"}' | bowire mcp serve --allow-arbitrary-urls
```

You'll see the tool surface: `bowire.discover`, `bowire.invoke`, `bowire.subscribe`, `bowire.recordings.list`, `bowire.mock.start`, … — Bowire's *own* toolset. The agent asks Bowire to do things; Bowire decides how.

### 2. Wire it into Claude Desktop

Add to `claude_desktop_config.json` (`%APPDATA%\Claude\` on Windows, `~/Library/Application Support/Claude/` on macOS, `~/.config/Claude/` on Linux):

```json
{
  "mcpServers": {
    "bowire": { "command": "bowire", "args": ["mcp", "serve", "--allow-arbitrary-urls"] }
  }
}
```

If `bowire` isn't on the agent's PATH, use the absolute path to the global tool (`%USERPROFILE%\.dotnet\tools\bowire.exe` / `~/.dotnet/tools/bowire`). Restart Claude Desktop — a new chat shows an MCP-connected badge.

### 3. Drive it from chat

> List the services at `http://localhost:5xxx`, then invoke `SayHello` with name "Alice".

Claude calls `bowire.discover` then `bowire.invoke` and shows the real response — protocol-agnostic at the LLM layer (it doesn't need to know REST from gRPC). Server-streaming comes back via `bowire.subscribe` (a bounded window of frames). If you have a recording, "list my recordings, start one as a mock on 7080 and call it" exercises `bowire.recordings.list` + `bowire.mock.start`.

## Security

`--allow-arbitrary-urls` lets the agent probe **any URL it can construct** — fine for `localhost:*` on your own machine, **not fine on a shared box**. For tighter setups, drop the flag and use the embedded adapter ([Unit 4](../../unit-4/README.md)) with an explicit allow-list. Per [outbound-calls policy], nothing is probed unless you opt in. **The MCP server is "shell access" for an agent — treat it that way.**

Auth for the HTTP bind: `bowire mcp serve --bind http --port 5081 --token <secret>` requires `Authorization: Bearer <secret>` on every request.

## Key Takeaways

1. **`bowire mcp serve` (stdio) exposes Bowire's own toolset** — discover / invoke / subscribe / recordings / mock.
2. **Stdio suits desktop agents** (the agent owns the subprocess); HTTP + embedded suits hosted setups (Unit 4).
3. **`--allow-arbitrary-urls` is opt-in and dangerous on shared machines** — pin targets via the embedded allow-list when it matters.

## What's Next

**Continue:** → [Lesson 3.4: Reverse-proxy interception](../lesson-4/README.md)

## Reference

- [MCP protocol docs](https://bowire.io/docs/protocols/mcp.html)
