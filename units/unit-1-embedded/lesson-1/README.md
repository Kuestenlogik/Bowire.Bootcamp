# Lesson 1.1 (Embedded track): Mount the workbench in your host

> **Difficulty:** Beginner | **Duration:** 12 min | **Prerequisites:** .NET 10 SDK; familiar with ASP.NET `WebApplication` / `Program.cs` shape

## Overview

Add the `Kuestenlogik.Bowire` NuGet to a small REST service and mount the workbench at `/bowire` on the host's own port. One process, no second terminal, no `--url` flag — Bowire reads the same `IServiceProvider` your own routes do.

## Why mount inside the host

The CLI shape (`bowire --url ...`) makes Bowire an external probe — it talks to your service across the wire. Embedded flips that: the workbench *is* a regular ASP.NET endpoint of your host, so it can see anything the host's DI container exposes:

- the OpenAPI document `MapOpenApi()` generates,
- the gRPC service registry from `AddGrpc()`,
- SignalR hubs from `AddSignalR()`,
- custom protocols you register through `IBowireProtocol`,

…all in one pass, without you having to enumerate URLs. The trade-off: the workbench's lifecycle becomes the host's lifecycle.

## Steps

### 1. Take the sample REST service

```bash
cd units/unit-1-samples/HelloApi
```

`Program.cs` already looks like this:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
var app = builder.Build();

app.MapOpenApi();

app.MapGet("/hello/{name}", (string name) =>
    new { greeting = $"Hello, {name}!", receivedAt = DateTimeOffset.UtcNow });

app.MapPost("/echo", (EchoRequest req) =>
    new { message = req.Message, receivedAt = DateTimeOffset.UtcNow });

app.MapGet("/health", () => new { status = "ok" });

app.Run("http://localhost:5001");
```

Two routes plus a health probe. We'll keep all of that — only add Bowire alongside.

### 2. Add the embedded NuGet

```bash
dotnet add package Kuestenlogik.Bowire
```

This pulls in the workbench, its bundled protocols (REST, gRPC, SignalR, &c.), and the plugin host. Roughly 18 MB of static assets ship inside the NuGet — they're served from the package without you touching `wwwroot/`.

### 3. Wire the workbench into `Program.cs`

Two changes — one line near `AddOpenApi()`, one line near `MapOpenApi()`:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddBowire();        // ← new
var app = builder.Build();

app.MapOpenApi();
app.MapBowire();                     // ← new

app.MapGet("/hello/{name}", …);      // your existing routes stay put
app.MapPost("/echo", …);
app.MapGet("/health", …);

app.Run("http://localhost:5001");
```

`AddBowire()` registers the plugin host into DI; `MapBowire()` mounts the workbench at `/bowire`. Your existing routes stay exactly where they are.

### 4. Run the host

```bash
dotnet run
```

You should see:

```
Now listening on: http://localhost:5001
Application started. Press Ctrl+C to shut down.
```

One process — no second terminal, no `bowire` CLI involved.

### 5. Invoke a method

Open <http://localhost:5001/bowire> in your browser. The workbench reads the same OpenAPI document `MapOpenApi()` already serves; the sidebar populates without you doing anything.

1. Click **HelloApi** (or whatever your sample's `info.title` says) in the sidebar.
2. Click **GetGreeting**.
3. The right pane shows a form — fill in `name = "Bowire"`.
4. Click **Invoke**.

You'll see the JSON response:

```json
{
  "greeting": "Hello, Bowire!",
  "receivedAt": "2026-05-31T08:30:14.123Z"
}
```

Try **PostEcho** too — the form auto-builds a JSON body editor from the `EchoRequest` schema.

## Key Takeaways

1. **Two calls — `AddBowire()` and `MapBowire()`.** That's the whole integration surface from the host's side. Everything else is auto-discovery against the same DI container the host already populates.
2. **The workbench shares the host's port + lifecycle.** Same `http://localhost:5001`, same `Ctrl+C`, same deploy bundle. No second process to orchestrate.
3. **Your existing routes don't move.** `AddBowire()` doesn't claim any existing route; `MapBowire()` mounts at `/bowire` (configurable via `Bowire:MountPath`). Existing endpoints, middleware, auth are untouched.
4. **The same NuGet powers production deploys.** Wrap the registrations in `IsDevelopment()` or `#if DEBUG` to gate the workbench out of release builds — same package, two configurations.

## What's Next

**Continue:** → [Lesson 1.2 — Add gRPC to the same host](../lesson-2/README.md)

## Reference

- [`AddBowire()` / `MapBowire()` API docs](https://bowire.io/docs/features/embedded-mode.html)
- [REST plugin: auto-discovery via OpenAPI](https://bowire.io/docs/protocols/rest.html#auto-discovery)
- [Bowire configuration reference](https://bowire.io/docs/reference/configuration.html)
