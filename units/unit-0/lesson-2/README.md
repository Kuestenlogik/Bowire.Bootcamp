# Lesson 0.2: Setup

> **Difficulty:** Beginner | **Duration:** 5 min | **Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Overview

Install the `bowire` global tool, confirm it runs, list the bundled protocol plugins, find where the user-managed plugin directory lives. By the end you'll have a working install ready to drive against a real API in [Lesson 0.3](../lesson-3/README.md).

## Steps

### 1. Verify the .NET SDK

```bash
dotnet --version
```

Expect 10.0.x or newer. If not, install from [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download).

### 2. Install the global tool

```bash
dotnet tool install --global Kuestenlogik.Bowire.Tool
```

The tool packs the workbench host + every bundled protocol plugin (REST, gRPC, GraphQL, MCP, MQTT, NATS, SignalR, WebSocket, Socket.IO, SOAP, Pulsar, JSON-RPC) into a single 50 MB tool. No per-protocol opt-in.

Verify:

```bash
bowire --version
```

You should see something like `1.6.1+<commit-sha>`.

### 3. List the bundled plugins

```bash
bowire plugin list --bundled
```

Expect a table like:

```
  Bundled plugins (12):

    Kuestenlogik.Bowire.Protocol.Rest       (built-in)
    Kuestenlogik.Bowire.Protocol.Grpc       (built-in)
    Kuestenlogik.Bowire.Protocol.GraphQL    (built-in)
    Kuestenlogik.Bowire.Protocol.Mcp        (built-in)
    Kuestenlogik.Bowire.Protocol.Mqtt       (built-in)
    Kuestenlogik.Bowire.Protocol.Nats       (built-in)
    Kuestenlogik.Bowire.Protocol.SignalR    (built-in)
    Kuestenlogik.Bowire.Protocol.WebSocket  (built-in)
    Kuestenlogik.Bowire.Protocol.SocketIo   (built-in)
    Kuestenlogik.Bowire.Protocol.Soap       (built-in)
    Kuestenlogik.Bowire.Protocol.Pulsar     (built-in)
    Kuestenlogik.Bowire.Protocol.JsonRpc    (built-in)
```

Bundled plugins update via `dotnet tool update -g Kuestenlogik.Bowire.Tool`.

### 4. Find the user-managed plugin directory

```bash
bowire plugin list
```

Note the **Plugin directory** line at the bottom — it's typically:

- **Windows:** `C:\Users\<you>\.bowire\plugins\`
- **macOS / Linux:** `~/.bowire/plugins/`

This is where `bowire plugin install` lays out installed-from-NuGet plugins (Unit 4.1) and extracted sidecar zips (Unit 4.2). Today it should be empty.

### 5. Find the rest of the `~/.bowire/` user-state directory

| Path | What's there |
|---|---|
| `~/.bowire/plugins/` | User-installed plugins (Unit 4) |
| `~/.bowire/recordings.json` | Recording store (Unit 2.1) |
| `~/.bowire/environments.json` | Saved environments / variables (workbench UI) |
| `~/.bowire/secrets/` | Encrypted secret store for `--auth-provider` plugins |
| `~/.bowire/config.json` | Per-user defaults (overridable per-invocation via `--config`) |

The directory is created lazily — most files don't exist until you produce them through the workbench.

## Troubleshooting

| Symptom | Fix |
|---|---|
| `bowire: command not found` after install | The `dotnet tool` install dir isn't on PATH. Add `$HOME/.dotnet/tools` (Linux/macOS) or `%USERPROFILE%\.dotnet\tools` (Windows). |
| `bowire --version` shows a much older version than expected | `dotnet tool update -g Kuestenlogik.Bowire.Tool`. |
| `bowire plugin list` errors with `Access denied` | Check `~/.bowire/plugins/` exists and is writable. The directory is created on first install; manual creation needs `mkdir -p`. |
| Tool install times out behind a corporate proxy | Configure `dotnet nuget` proxy settings (`dotnet nuget config -s <feed>`) or set `HTTP_PROXY` / `HTTPS_PROXY`. |

## Key Takeaways

1. **One tool, every protocol.** Bundled plugins ship in the same NuGet — no per-protocol install step.
2. **User-managed plugins go under `~/.bowire/plugins/`.** That's where Units 4.1 + 4.2 drop their build output.
3. **State is local-first.** Recordings, environments, secrets all sit under `~/.bowire/`. No cloud, no telemetry beyond an opt-in update check.
4. **`dotnet tool` updates the whole bundle.** `dotnet tool update -g Kuestenlogik.Bowire.Tool` refreshes the workbench + every bundled plugin in one step.

## What's Next

You're ready to point the freshly-installed `bowire` at a real REST API and see the workbench render.

**Test your knowledge:** → [Knowledge Assessment](KNOWLEDGE_ASSESSMENT.md)
**Continue:** → [Lesson 0.3: Hello Bowire](../lesson-3/README.md)

## Reference

- [Setup docs](https://bowire.io/docs/setup/)
- [Updating Bowire and its plugins](https://bowire.io/docs/setup/updating.html)
- [Plugin system](https://bowire.io/docs/features/plugin-system.html)
