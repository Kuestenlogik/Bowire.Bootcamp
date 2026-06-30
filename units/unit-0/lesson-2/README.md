# Lesson 0.2: Setup

> **Difficulty:** Beginner | **Duration:** 5 min (CLI) · 10 min (embedded) | **Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Overview

Bowire ships in two deployment shapes (see [Lesson 0.1](../lesson-1/README.md)). This lesson sets up whichever one you picked:

- **Path A: CLI** — install the global tool. Five minutes. Best when you want to point at any URL.
- **Path B: Embedded** — scaffold a sample ASP.NET service and mount the workbench inside it. Ten minutes. Best when you're building / debugging your own service.

You can come back and add the other path later — the bootcamp's remaining lessons annotate which path each one targets.

## Common: verify the .NET SDK

```bash
dotnet --version
```

Expect 10.0.x or newer. If not, install from [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download).

---

## Path A — CLI (two-process)

### A1. Install the global tool

```bash
dotnet tool install --global Kuestenlogik.Bowire.Tool
```

The tool packs the workbench host + every bundled protocol plugin (REST, gRPC, GraphQL, MCP, MQTT, NATS, SignalR, WebSocket, Socket.IO, SOAP, Pulsar, JSON-RPC) into a single 50 MB tool. No per-protocol opt-in.

Verify:

```bash
bowire --version
```

You should see something like `1.6.1+<commit-sha>`.

### A2. List the bundled plugins

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

### A3. Find the user-managed plugin directory

```bash
bowire plugin list
```

Note the **Plugin directory** line at the bottom — it's typically:

- **Windows:** `C:\Users\<you>\.bowire\plugins\`
- **macOS / Linux:** `~/.bowire/plugins/`

This is where `bowire plugin install` lays out installed-from-NuGet plugins (Unit 4.1) and extracted sidecar zips (Unit 4.2). Today it should be empty.

### A4. Find the rest of the `~/.bowire/` user-state directory

| Path | What's there |
|---|---|
| `~/.bowire/plugins/` | User-installed plugins (Unit 4) |
| `~/.bowire/recordings.json` | Recording store (Unit 2.1) |
| `~/.bowire/environments.json` | Saved environments / variables (workbench UI) |
| `~/.bowire/secrets/` | Encrypted secret store for `--auth-provider` plugins |
| `~/.bowire/config.json` | Per-user defaults (overridable per-invocation via `--config`) |

The directory is created lazily — most files don't exist until you produce them through the workbench.

---

## Path B — Embedded (single-process)

### B1. Scaffold an empty ASP.NET host (skip if you have one)

If you don't already have an ASP.NET host to mount the workbench inside, create a bare-bones one to follow along:

```bash
dotnet new web -o BowireEmbeddedHost
cd BowireEmbeddedHost
```

You'll end up with a `Program.cs`, a `BowireEmbeddedHost.csproj`, and a default route printing `Hello World!`. Bowire mounts alongside whatever routes the host already exposes.

### B2. Add the Bowire NuGet package

```bash
dotnet add package Kuestenlogik.Bowire
```

This is the embedded-mode package — it pulls in the workbench host + every bundled protocol plugin. Same plugin surface as the CLI, no separate distribution.

### B3. Wire it up in `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

// Your existing service registrations stay where they are.
builder.Services.AddBowire();

var app = builder.Build();

// Your existing middleware stays where it is.
app.MapGet("/", () => "Hello World!");
app.MapBowire();   // mounts the workbench at /bowire

app.Run();
```

`AddBowire()` registers the plugin host into DI. `MapBowire()` mounts the workbench at `/bowire`. No further config required for the default discovery — Bowire reads endpoint sources directly through DI at request time.

### B4. Run + verify

```bash
dotnet run
```

Open <http://localhost:5000/bowire> in your browser (or whatever port Kestrel printed). You should see the workbench rendered as part of your service.

### B5. Discovery in embedded mode is in-process

Embedded mode doesn't need a `~/.bowire/plugins/` directory — every plugin ships in-package with `Kuestenlogik.Bowire`. State (recordings, environments, secrets) is per-process by default and survives restarts via the host's regular config / data tier — not a process-wide `~/.bowire/` tree.

If you want to add a plugin that isn't bundled, reference its NuGet next to `Kuestenlogik.Bowire` and the host's `AssemblyLoadContext` picks it up automatically. No `bowire plugin install` step.

---

## Troubleshooting

| Symptom | Path | Fix |
|---|---|---|
| `bowire: command not found` after install | A | The `dotnet tool` install dir isn't on PATH. Add `$HOME/.dotnet/tools` (Linux/macOS) or `%USERPROFILE%\.dotnet\tools` (Windows). |
| `bowire --version` shows a much older version than expected | A | `dotnet tool update -g Kuestenlogik.Bowire.Tool`. |
| `bowire plugin list` errors with `Access denied` | A | Check `~/.bowire/plugins/` exists and is writable. The directory is created on first install; manual creation needs `mkdir -p`. |
| Tool install times out behind a corporate proxy | A · B | Configure `dotnet nuget` proxy settings (`dotnet nuget config -s <feed>`) or set `HTTP_PROXY` / `HTTPS_PROXY`. |
| `/bowire` returns 404 in the embedded host | B | Make sure `app.MapBowire()` runs **after** `var app = builder.Build()` but **before** `app.Run()`. ASP.NET ignores route registrations after `Run()` starts the host. |
| Workbench renders but discovers no endpoints | B | The host has no services registered yet. Either add a sample route (`app.MapGet("/api/ping", () => "pong")`) or skip ahead to [Unit 1 — Lesson 1.1](../../unit-1/lesson-1/README.md) (Embedded shape section) which adds a discovery target. |

## Key Takeaways

1. **One tool, every protocol.** Bundled plugins ship in the same NuGet (`Kuestenlogik.Bowire.Tool` for CLI, `Kuestenlogik.Bowire` for embedded) — no per-protocol install step on either path.
2. **CLI state lives under `~/.bowire/`.** Plugins, recordings, environments, secrets — all under the user's home directory. Embedded mode skips that; state lives in the host process.
3. **Embedded is two lines: `AddBowire()` + `MapBowire()`.** No separate distribution, no companion container.
4. **The two paths are not exclusive.** Most teams have both — embedded for their own service, CLI for external targets and scripted use cases.
5. **Updates differ by path.** CLI: `dotnet tool update -g Kuestenlogik.Bowire.Tool`. Embedded: bump the `Kuestenlogik.Bowire` PackageReference + redeploy.

## What's Next

You're ready to point the freshly-installed `bowire` at a real REST API and see the workbench render.

**Continue:** → [Lesson 0.3: Hello Bowire](../lesson-3/README.md)

## Reference

- [Setup docs](https://bowire.io/docs/setup/)
- [Updating Bowire and its plugins](https://bowire.io/docs/setup/updating.html)
- [Plugin system](https://bowire.io/docs/features/plugin-system.html)
