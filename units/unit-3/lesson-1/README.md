# Lesson 3.1: AI-Agent Integration (Claude Desktop + Cursor over MCP)

> **Difficulty:** Intermediate | **Duration:** 10 min | **Prerequisites:** [Unit 1](../../unit-1/README.md) complete, Claude Desktop **or** Cursor installed

## Overview

Wire Bowire into an AI agent over **MCP** (Model Context Protocol). The agent gains a `bowire.discover` / `bowire.invoke` / `bowire.recordings.list` toolset and can drive your APIs in plain English — list services, call methods, inspect recordings — without leaving the chat window.

Same Bowire CLI you ran in Lessons 01-03, different consumer: instead of a browser, a language model.

## How this fits

Bowire and MCP cross paths in [four distinct ways](https://bowire.io/docs/protocols/mcp.html). This lesson uses the one that lets an agent **drive Bowire**:

| Role | What it does | This lesson? |
|---|---|---|
| 1. MCP client | Bowire connects to *external* MCP servers, surfaces their tools | no |
| 2. MCP adapter | Bowire wraps your discovered APIs as MCP tools | no |
| 3. Bowire-as-MCP-server (HTTP) | Bowire exposes its own toolset over HTTP | no |
| **4. Bowire-as-MCP-server (stdio)** | **Bowire exposes its own toolset over stdin/stdout** | **yes** |

Role 4 is the one Claude Desktop / Cursor speak natively — they spawn `bowire mcp serve` as a subprocess and pipe JSON-RPC over stdio. No HTTP server to manage, no port to pick.

## Steps

### 1. Keep the Unit 1 sample APIs running

```bash
# Terminal A — REST
cd ../../unit-1/lesson-1/sample/HelloApi
dotnet run                                    # → http://localhost:5001

# Terminal B — gRPC
cd ../../unit-1/lesson-2/sample/HelloGrpc
dotnet run                                    # → http://localhost:5002
```

Both stay up for the rest of the lesson.

### 2. Verify `bowire mcp serve` works standalone

In a third terminal, sanity-check the stdio server before wiring it into Claude:

```bash
bowire mcp serve --allow-arbitrary-urls
```

The process blocks on stdin waiting for JSON-RPC frames — that's correct. Send it the MCP `tools/list` request to see the tool surface:

```bash
echo '{"jsonrpc":"2.0","id":1,"method":"tools/list"}' | bowire mcp serve --allow-arbitrary-urls
```

You'll see `bowire.discover`, `bowire.invoke`, `bowire.subscribe`, `bowire.recordings.list`, `bowire.mock.start`, &c — the full agent toolset listed in [Bowire as MCP server](https://bowire.io/docs/protocols/mcp.html#tool-surface-roles-3--4).

`--allow-arbitrary-urls` means **any URL the agent asks about will be probed**. Fine for local dev against `localhost:*`; not fine on a shared dev box. See the security note at the bottom of the lesson.

### 3. Wire it into Claude Desktop

Open Claude Desktop's MCP config file:

- **macOS** — `~/Library/Application Support/Claude/claude_desktop_config.json`
- **Windows** — `%APPDATA%\Claude\claude_desktop_config.json`
- **Linux** — `~/.config/Claude/claude_desktop_config.json`

Add the `bowire` server to the `mcpServers` section (merge with anything already there):

```json
{
  "mcpServers": {
    "bowire": {
      "command": "bowire",
      "args": ["mcp", "serve", "--allow-arbitrary-urls"]
    }
  }
}
```

> Copy-pasteable snippet: `sample/claude-desktop-config-snippet.json` in this lesson.

If `bowire` isn't on Claude's `PATH` (the dotnet global tool dir varies), use the absolute path: `~/.dotnet/tools/bowire` on Linux/macOS, `%USERPROFILE%\.dotnet\tools\bowire.exe` on Windows.

Restart Claude Desktop. The new chat window should show a small **🔌 1 MCP server connected** badge (click it to confirm `bowire` is listed and 10 tools are exposed).

### 4. Drive REST from the chat

In a new Claude conversation, type:

> List the services running at `http://localhost:5001`.

Claude calls `bowire.discover` against the URL, reads the OpenAPI document, and replies with the three methods from Lesson 01 (`GetGreeting`, `PostEcho`, `GetHealth`).

Follow up:

> Invoke `GetGreeting` with `name = "Alice"`.

Claude picks the method, builds the JSON payload, and calls `bowire.invoke` — you'll see the actual response body in the chat:

```json
{
  "greeting": "Hello, Alice!",
  "receivedAt": "2026-05-31T20:30:14.567Z"
}
```

### 5. Drive gRPC from the same chat

Without restarting anything, ask:

> What's on `http://localhost:5002`?

`bowire.discover` runs the gRPC plugin against the second URL, picks up the Server Reflection from Lesson 02's `HelloGrpc`, and tells Claude about `greeter.Greeter/Hello` and `greeter.Greeter/HelloStream`.

> Call `Hello` on it with `name = "Bowire"`.

`bowire.invoke` dispatches through the gRPC plugin and returns the response. **One agent, two protocols, zero extra config** — that's the win.

> Note: server-streaming methods (`HelloStream`) come back through `bowire.subscribe`, which samples a bounded window of frames rather than streaming live into the chat. Ask Claude to "subscribe to HelloStream for 3 seconds" and it'll collect a handful of frames and summarise them.

### 6. Bonus — replay a recording through the agent

If you finished Lesson 03 with the `hello-tour.bwr` recording in hand:

> List my Bowire recordings.

→ Claude calls `bowire.recordings.list` and lists the on-disk store.

> Start the `Hello tour` recording as a mock on port 7080 and call `GetGreeting` against it.

→ Claude calls `bowire.mock.start` to spin the mock up in-process, then `bowire.invoke` against `http://localhost:7080`. You get the frozen response from the recording. Tell Claude to "stop the mock" and `bowire.mock.stop` shuts it down.

### 7. Cursor (optional)

Cursor uses the same `mcpServers` shape. Open `Settings → MCP` (or edit `~/.cursor/mcp.json`) and paste the same snippet from step 3.

## Key Takeaways

1. **The agent drives Bowire, not the other way around.** Roles 1 / 2 / 3 of MCP integration treat Bowire as the consumer; Role 4 treats the agent as the consumer. Same SDK, same toolset, different direction.
2. **Protocol-agnostic at the LLM layer.** Claude doesn't need to know what gRPC is. It just calls `bowire.discover` and `bowire.invoke`; the protocol-side semantics live in the Bowire plugins.
3. **Stdio over HTTP for desktop AI.** The agent owns the subprocess lifecycle (spawn, kill, restart). No port to manage, no firewall rule, no auth surface. The HTTP role (3) is for when *your own service* hosts the MCP endpoint.
4. **Recordings transit the toolset too.** `bowire.recordings.list` / `bowire.mock.start` let the agent reach into your saved sessions and stand up mocks on demand — closing the loop with Unit 2.

## Security

`--allow-arbitrary-urls` lets the agent probe **any URL it can construct**. Fine for `localhost:*` against your own sample services; **not fine on a shared dev box or anywhere the agent's input isn't from you alone.**

For tighter setups, drop `--allow-arbitrary-urls` and instead run the embedded variant (role 3) in your own ASP.NET host with `opts.AllowedServerUrls.Add("https://my-api")`. That pins the agent to a known list of targets.

The MCP server is "shell access" for an agent. Treat it that way.

## What's Next

You're ready to leave the AI side behind and start building protocol plugins yourself — first in .NET, then in Python.

**Test your knowledge:** → [Knowledge Assessment](KNOWLEDGE_ASSESSMENT.md)
**Continue:** → [Unit 4: Extending Bowire](../../unit-4/README.md)

## Reference

- [MCP protocol docs](https://bowire.io/docs/protocols/mcp.html) — the four-role taxonomy, tool surface, full config matrix.
- [Bowire AI integration ADR](https://bowire.io/docs/architecture/ai-integration.html) — why local-first MCP fits the "no cloud, no account" brand promise.
- [MCP spec](https://modelcontextprotocol.io/) — the upstream protocol.
