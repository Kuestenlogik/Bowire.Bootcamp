# Lesson 3.1: AI-Agent Integration (Claude Desktop + Cursor over MCP)

> **Difficulty:** Intermediate | **Duration:** 10 min (CLI) · 12 min (Embedded) | **Prerequisites:** [Unit 1](../../unit-1/README.md) complete, Claude Desktop **or** Cursor installed

## Overview

Wire Bowire into an AI agent over **MCP** (Model Context Protocol). The agent gains a `bowire.discover` / `bowire.invoke` / `bowire.recordings.list` toolset and can drive your APIs in plain English — list services, call methods, inspect recordings — without leaving the chat window.

Two paths, same agent experience:

- **Path A (CLI / stdio)** — agent spawns `bowire mcp serve` as a subprocess and pipes JSON-RPC over stdin/stdout. The Claude-Desktop / Cursor default; no port to manage.
- **Path B (Embedded / HTTP)** — your ASP.NET host exposes Bowire's discovered services as MCP tools at `<host>/bowire/mcp` via `AddBowireMcpAdapter()` + `MapBowireMcpAdapter()`. Agent connects over HTTP; one shared endpoint for every agent that can reach the URL.

When to pick which:
- Desktop AI on your laptop → Path A (subprocess lifecycle handled by the agent).
- Hosted Bowire workbench (internal-tools, multi-user) → Path B (remote agents, no per-laptop install).
- Want both? Run both — Claude Desktop reads stdio (Path A) while a remote Cursor or a CI agent hits the HTTP endpoint (Path B).

## How this fits

Bowire and MCP cross paths in [four distinct ways](https://bowire.io/docs/protocols/mcp.html). This lesson uses the one that lets an agent **drive Bowire**:

| Role | What it does | This lesson? |
|---|---|---|
| 1. MCP client | Bowire connects to *external* MCP servers, surfaces their tools | no |
| 2. MCP adapter | Bowire wraps your discovered APIs as MCP tools | no |
| 3. Bowire-as-MCP-server (HTTP) | Bowire exposes its own toolset over HTTP | no |
| **4. Bowire-as-MCP-server (stdio)** | **Bowire exposes its own toolset over stdin/stdout** | **yes** |

Role 4 is the one Claude Desktop / Cursor speak natively — they spawn `bowire mcp serve` as a subprocess and pipe JSON-RPC over stdio. No HTTP server to manage, no port to pick.

## Path A — CLI / stdio (Claude-Desktop default)

### A1. Keep the Unit 1 sample APIs running

```bash
# Terminal A — REST
cd ../../unit-1/lesson-1/sample/HelloApi
dotnet run                                    # → http://localhost:5001

# Terminal B — gRPC
cd ../../unit-1/lesson-2/sample/HelloGrpc
dotnet run                                    # → http://localhost:5002
```

Both stay up for the rest of the lesson.

### A2. Verify `bowire mcp serve` works standalone

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

### A3. Wire it into Claude Desktop

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

Skip Path B below; jump to **Drive REST from the chat**.

## Path B — Embedded / HTTP (one shared MCP endpoint)

### B1. Add the MCP adapter to your embedded host

You already have the embedded `HelloApi` from Lesson 1.1 Path B with `AddBowire()` + `MapBowire()`. Add the MCP-adapter calls right next to them:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddBowire();
builder.Services.AddBowireMcpAdapter("http://localhost:5001");   // ← new

var app = builder.Build();
app.MapOpenApi();
app.MapBowire();
app.MapBowireMcpAdapter("");                                     // ← new (mounts /mcp)

// ...your routes...
app.Run("http://localhost:5001");
```

`AddBowireMcpAdapter(serverUrl)` registers the MCP server into DI and pins it to the URL the workbench discovers against (here the host itself). `MapBowireMcpAdapter("")` mounts the streamable-HTTP endpoint at `/mcp` at the site root; pass a prefix (e.g. `MapBowireMcpAdapter("/bowire")`) when you want it nested.

### B2. Run the host and verify the endpoint

```bash
dotnet run                                # → http://localhost:5001
```

Confirm the MCP endpoint responds to a discovery probe:

```bash
curl -s -X POST http://localhost:5001/mcp \
     -H "Content-Type: application/json" \
     -d '{"jsonrpc":"2.0","id":1,"method":"tools/list"}'
```

You'll get a JSON-RPC response listing tools that name every method discovered against the host — `HelloApi.GetGreeting`, `HelloApi.PostEcho`, &c. (One MCP tool per discovered method; the adapter pattern differs from the CLI's `bowire.discover` / `bowire.invoke` toolset on purpose — see the note below.)

### B3. Wire it into Cursor (or any HTTP-MCP client)

Cursor's MCP config takes HTTP endpoints:

```json
{
  "mcpServers": {
    "bowire-hello": {
      "url": "http://localhost:5001/mcp"
    }
  }
}
```

Open `Settings → MCP` and paste the snippet (or edit `~/.cursor/mcp.json` directly). Cursor connects over HTTP — no subprocess, no `command` field. Claude Desktop's stdio-only config (Step A3) doesn't accept HTTP URLs as of writing; Claude users stay on Path A.

> **Path A vs Path B tool surfaces differ on purpose.**
> Path A (CLI / stdio, role 4 in the [MCP docs](https://bowire.io/docs/protocols/mcp.html)) exposes Bowire's *own* toolset: `bowire.discover`, `bowire.invoke`, `bowire.recordings.list`, `bowire.mock.start`. The agent asks Bowire to do things; Bowire decides how.
> Path B (Embedded / HTTP, role 3) exposes the *discovered services* as MCP tools directly: `HelloApi.GetGreeting`, `HelloApi.PostEcho`. The agent calls a service method as if it were a native tool; Bowire is invisible plumbing.
> Both work; both have use-cases. Role 4 is broader (agent can drive Bowire end-to-end including recordings + mocks); role 3 is leaner (agent doesn't need to know Bowire exists, it just sees domain methods).

## Drive REST from the chat

Both paths work the same in the chat window — the only difference is whose name shows up in the MCP-server list (Path A: `bowire`; Path B: `bowire-hello`). For brevity the rest of this lesson uses Claude Desktop + Path A in the snippets; substitute your Cursor-with-Path-B equivalents word-for-word.

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

## Drive gRPC from the same chat

Without restarting anything, ask:

> What's on `http://localhost:5002`?

`bowire.discover` runs the gRPC plugin against the second URL, picks up the Server Reflection from Lesson 02's `HelloGrpc`, and tells Claude about `greeter.Greeter/Hello` and `greeter.Greeter/HelloStream`.

> Call `Hello` on it with `name = "Bowire"`.

`bowire.invoke` dispatches through the gRPC plugin and returns the response. **One agent, two protocols, zero extra config** — that's the win.

> Note: server-streaming methods (`HelloStream`) come back through `bowire.subscribe`, which samples a bounded window of frames rather than streaming live into the chat. Ask Claude to "subscribe to HelloStream for 3 seconds" and it'll collect a handful of frames and summarise them.

## Bonus — replay a recording through the agent (Path A only)

If you finished Lesson 03 with the `hello-tour.bwr` recording in hand:

> List my Bowire recordings.

→ Claude calls `bowire.recordings.list` and lists the on-disk store.

> Start the `Hello tour` recording as a mock on port 7080 and call `GetGreeting` against it.

→ Claude calls `bowire.mock.start` to spin the mock up in-process, then `bowire.invoke` against `http://localhost:7080`. You get the frozen response from the recording. Tell Claude to "stop the mock" and `bowire.mock.stop` shuts it down.

## Cursor (Path A stdio variant)

Cursor uses the same `mcpServers` shape. Open `Settings → MCP` (or edit `~/.cursor/mcp.json`) and paste the same snippet from step 3.

## Key Takeaways

1. **Two paths, two MCP shapes.** Path A (CLI / stdio) exposes Bowire's own toolset (`bowire.discover`, `bowire.invoke`, &c.) — agent drives Bowire. Path B (Embedded / HTTP) exposes the discovered services as MCP tools directly (`HelloApi.GetGreeting`, &c.) — agent doesn't see Bowire, just domain methods.
2. **Protocol-agnostic at the LLM layer.** Claude doesn't need to know what gRPC is — the protocol-side semantics live in the Bowire plugins either way.
3. **Stdio for desktop AI, HTTP for hosted setups.** Path A (the agent owns the subprocess lifecycle) fits laptop AI. Path B (one shared HTTP endpoint per Bowire host) fits internal-tools deployments where remote agents + CI runners want the same MCP surface without per-machine installs.
4. **Path A reaches into recordings + mocks; Path B doesn't (yet).** `bowire.recordings.list` / `bowire.mock.start` are Path-A tools — the CLI-side MCP server has access to Bowire's own state. Path B's adapter is service-centric; recording / mock control over the embedded MCP-adapter is tracked at [#37](https://github.com/Kuestenlogik/Bowire/issues/37).

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
