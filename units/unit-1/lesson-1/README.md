# Lesson 1.1: First call

> **Difficulty:** Beginner | **Duration:** 10 min | **Prerequisites:** .NET 10 SDK, `bowire` CLI installed (see [Unit 0 — Toolchain](../../unit-0/lesson-3/README.md))

## Overview

You'll start a tiny REST service on `localhost:5001`, point the `bowire` CLI at it from a second terminal, and invoke a method from the browser-based workbench. This is the classic two-process shape: your service is one process, Bowire is another, the workbench UI is the bridge.

## Why this beats a generic API client

A generic API client (Postman, curl, &c.) makes you *describe* the service before you can call it — type the URL, name the method, fill in the schema. Bowire's CLI auto-discovers all of that from the wire: the REST plugin probes for an OpenAPI document at the conventional paths, parses it, and renders each operation as a method node in the sidebar. You skip straight to the click-and-invoke step.

This pattern works for every protocol Bowire bundles (REST via OpenAPI, gRPC via reflection, GraphQL via introspection, MQTT/NATS/Kafka via schema-discovery, &c.) — and for plugins you install later. The two-process shape stays identical; only the discovery mechanism changes.

## Steps

### 1. Start the sample API

```bash
cd units/unit-1-samples/HelloApi
dotnet run
```

You should see:

```
Now listening on: http://localhost:5001
Application started. Press Ctrl+C to shut down.
```

Leave this terminal open.

### 2. Point Bowire at it

Open a second terminal:

```bash
bowire --url http://localhost:5001
```

This:

- Boots a local workbench UI on `http://localhost:5080/bowire`
- Auto-opens your browser to it
- Tells the workbench: "the server is at `http://localhost:5001`"

The REST plugin probes for an OpenAPI document at the conventional paths, finds the one .NET 10's `MapOpenApi()` generated, parses it, and renders each operation as a method node in the sidebar.

### 3. Invoke a method

Open <http://localhost:5080/bowire> if it didn't auto-open. In the workbench:

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

1. **`bowire --url <service>` is the entry point.** The CLI spins up a workbench, points it at the URL, opens the browser. Three flags, no config file required.
2. **Auto-discovery is the headline.** The sidebar populates from the service's own OpenAPI document — no manual schema upload, no hand-typed operations.
3. **Two processes is the shape.** The service runs in terminal 1; Bowire runs in terminal 2. They communicate over the wire, not via shared memory — so anything you can `curl` from a separate terminal, Bowire can drive from a separate terminal.

## What's Next

**Continue:** → [Lesson 1.2 — Multi-protocol (REST + gRPC)](../lesson-2/README.md)

## Reference

- [`bowire` CLI docs](https://bowire.io/docs/features/cli-mode.html)
- [REST plugin: auto-discovery via OpenAPI](https://bowire.io/docs/protocols/rest.html#auto-discovery)

---

### Embedded shape

> The lesson body above walks the **CLI shape** — `bowire --url <service>` from a separate terminal. If you'd rather mount the workbench *inside* your ASP.NET host instead (single process, no second terminal, no `--url` flag), the setup steps below replace Steps 1-2; the workbench walkthrough in Step 3 is identical regardless of which shape you picked.
>
> Side-by-side tabbed code blocks land in a follow-up content PR; for now both shapes coexist in this file with the embedded shape preserved verbatim below.

#### Overview (Embedded shape)

Add the `Kuestenlogik.Bowire` NuGet to a small REST service and mount the workbench at `/bowire` on the host's own port. One process, no second terminal, no `--url` flag — Bowire reads the same `IServiceProvider` your own routes do.

#### Why mount inside the host

The CLI shape (`bowire --url ...`) makes Bowire an external probe — it talks to your service across the wire. Embedded flips that: the workbench *is* a regular ASP.NET endpoint of your host, so it can see anything the host's DI container exposes:

- the OpenAPI document `MapOpenApi()` generates,
- the gRPC service registry from `AddGrpc()`,
- SignalR hubs from `AddSignalR()`,
- custom protocols you register through `IBowireProtocol`,

…all in one pass, without you having to enumerate URLs. The trade-off: the workbench's lifecycle becomes the host's lifecycle.

#### Steps (Embedded shape)

##### 1. Take the sample REST service

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

##### 2. Add the embedded NuGet

```bash
dotnet add package Kuestenlogik.Bowire
```

This pulls in the workbench, its bundled protocols (REST, gRPC, SignalR, &c.), and the plugin host. Roughly 18 MB of static assets ship inside the NuGet — they're served from the package without you touching `wwwroot/`.

##### 3. Wire the workbench into `Program.cs`

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

##### 4. Run the host

```bash
dotnet run
```

You should see:

```
Now listening on: http://localhost:5001
Application started. Press Ctrl+C to shut down.
```

One process — no second terminal, no `bowire` CLI involved.

From here, open <http://localhost:5001/bowire> and follow Step 3 ("Invoke a method") from the CLI walkthrough above — the workbench UI is identical regardless of which shape mounted it.

#### Key Takeaways (Embedded shape)

1. **Two calls — `AddBowire()` and `MapBowire()`.** That's the whole integration surface from the host's side. Everything else is auto-discovery against the same DI container the host already populates.
2. **The workbench shares the host's port + lifecycle.** Same `http://localhost:5001`, same `Ctrl+C`, same deploy bundle. No second process to orchestrate.
3. **Your existing routes don't move.** `AddBowire()` doesn't claim any existing route; `MapBowire()` mounts at `/bowire` (configurable via `Bowire:MountPath`). Existing endpoints, middleware, auth are untouched.
4. **The same NuGet powers production deploys.** Wrap the registrations in `IsDevelopment()` or `#if DEBUG` to gate the workbench out of release builds — same package, two configurations.

#### Reference (Embedded shape)

- [`AddBowire()` / `MapBowire()` API docs](https://bowire.io/docs/features/embedded-mode.html)
- [Bowire configuration reference](https://bowire.io/docs/reference/configuration.html)
