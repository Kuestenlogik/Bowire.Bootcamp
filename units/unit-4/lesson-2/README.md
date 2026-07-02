# Lesson 4.2: Embedded MCP adapter

> **Difficulty:** Intermediate | **Duration:** 12 min | **Prerequisites:** [Lesson 4.1](../lesson-1/README.md)

## Overview

Expose your host's discovered services to AI agents as MCP tools over **one shared HTTP endpoint** — the embedded counterpart to the CLI's stdio `bowire mcp serve` ([Unit 3.3](../../unit-3/lesson-3/README.md)). Remote agents and CI runners connect over HTTP; no per-machine install, no subprocess.

## Steps

### 1. Reference the MCP package

The adapter lives in a sibling package (not rolled into bare `Kuestenlogik.Bowire`):

```bash
dotnet add package Kuestenlogik.Bowire.Mcp
```

### 2. Wire it next to `AddBowire()` / `MapBowire()`

```csharp
builder.Services.AddBowire();
builder.Services.AddBowireMcpAdapter("http://localhost:5101");   // pin to the URL to discover against

var app = builder.Build();
app.MapBowire();
app.MapBowireMcpAdapter("");                                     // mounts /mcp at the site root
```

- `AddBowireMcpAdapter(serverUrl)` registers the MCP server and pins it to the URL the workbench discovers against (usually the host itself).
- `MapBowireMcpAdapter("")` mounts the streamable-HTTP endpoint at `/mcp`; pass a prefix (`"/bowire"`) to nest it.

### 3. Verify the endpoint

```bash
curl -s -X POST http://localhost:5101/mcp \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","id":1,"method":"tools/list"}'
```

You get one MCP tool **per discovered method** — `Harbor.ListDocks`, `Harbor.BerthShip`, … The agent calls a domain method as if it were a native tool; Bowire is invisible plumbing.

### 4. Connect an HTTP-MCP client (Cursor)

```json
{ "mcpServers": { "harbor": { "url": "http://localhost:5101/mcp" } } }
```

## Stdio vs HTTP — the tool-surface difference

| Path | Modality | Tool surface |
|---|---|---|
| `bowire mcp serve` (stdio) | CLI ([Unit 3.3](../../unit-3/lesson-3/README.md)) | Bowire's *own* toolset — `bowire.discover`, `bowire.invoke`, `bowire.recordings.list`, `bowire.mock.start`. Agent drives Bowire. |
| `MapBowireMcpAdapter()` (HTTP) | embedded (here) | The *discovered services* as tools — `Harbor.ListDocks`, … Agent doesn't know Bowire exists. |

Stdio suits desktop AI (agent owns the subprocess); HTTP + embedded suits hosted, multi-user setups. Pin targets with an allow-list rather than `--allow-arbitrary-urls` on shared hosts.

## Key Takeaways

1. **`AddBowireMcpAdapter()` + `MapBowireMcpAdapter()`** = one shared HTTP MCP endpoint at `/mcp`.
2. **It exposes discovered services as tools directly** (service-centric), vs the CLI's Bowire-centric toolset.
3. **HTTP for hosted/remote agents; stdio (Unit 3.3) for laptop agents.**

## What's Next

**Continue:** → [Lesson 4.3: Interceptor middleware](../lesson-3/README.md)

## Reference

- [MCP protocol docs](https://bowire.io/docs/protocols/mcp.html)
