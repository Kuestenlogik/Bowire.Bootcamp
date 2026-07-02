# Lesson 4.1: Embed the workbench

> **Difficulty:** Intermediate | **Duration:** 15 min | **Prerequisites:** [Unit 0](../../unit-0/README.md); .NET 10 SDK; an ASP.NET host you control

## Overview

Mount the Bowire workbench inside your own ASP.NET service — one process, no second terminal, no `--url`. The workbench reads the **same `IServiceProvider`** your own routes do, so discovery sees your OpenAPI document, gRPC reflection registry, SignalR hubs and any `IBowireProtocol` you registered — in one pass, no URL enumeration.

## Steps

### 1. Reference the package

```bash
dotnet add package Kuestenlogik.Bowire
```

This pulls in the workbench + bundled protocols + plugin host. The ~18 MB of static assets ship inside the NuGet — served without you touching `wwwroot/`.

### 2. Two lines in `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddBowire();     // ← register the workbench + plugin host

var app = builder.Build();

app.MapOpenApi();
app.MapBowire();                  // ← mount the workbench at /bowire

// your existing routes stay exactly where they are
app.MapGet("/hello/{name}", (string name) => new { greeting = $"Hello, {name}!" });

app.Run();
```

`AddBowire()` registers into DI; `MapBowire()` mounts at `/bowire` (configurable via `Bowire:MountPath`). Your routes, middleware and auth are untouched. Run the host and open `http://localhost:<port>/bowire` — from here the UI is the same one [Unit 1](../../unit-1/README.md) walks.

> **Real reference:** every `harbor-demo/src/*/Program.cs` in `Bowire.Samples` embeds Bowire exactly this way — the Combined host adds REST + gRPC + SignalR + WebSocket + SSE and mounts one workbench over all of them.

### 3. It inherits your host

Because the workbench is a normal endpoint of your host:

- **DI** — discovery reads your registered endpoint sources directly; no schema round-trip, no version drift.
- **Auth** — `[Authorize]` policies and your auth middleware apply to `/bowire` like any route. Gate access with the auth-provider SPI (`--auth-provider`) when needed.
- **Config** — `IOptions<T>`, `appsettings.json` (`Bowire:*`), logging providers — all shared.

### 4. Gate it out of production

The same NuGet powers dev and prod — wrap the registrations so the workbench only mounts where you want it:

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapBowire();
}
```

…or `#if DEBUG`. One package, two configurations.

## Key Takeaways

1. **Two calls — `AddBowire()` + `MapBowire()`** — is the whole integration surface.
2. **The workbench shares the host's port, lifecycle, DI, auth and config.** No second process.
3. **Your routes don't move; gate with `IsDevelopment()` / `#if DEBUG`** to keep it out of release builds.

## What's Next

**Continue:** → [Lesson 4.2: Embedded MCP adapter](../lesson-2/README.md)

## Reference

- [Embedded mode](https://bowire.io/docs/features/embedded-mode.html)
- `Bowire.Samples/harbor-demo/src/*/Program.cs`
