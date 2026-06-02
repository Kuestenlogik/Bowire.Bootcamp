# Lesson 1.1: Your first call (REST + OpenAPI)

> **Difficulty:** Beginner | **Duration:** 5 min (CLI) · 10 min (Embedded) | **Prerequisites:** [Unit 0](../../unit-0/README.md) complete, [.NET 10 SDK](https://dotnet.microsoft.com/download), one of CLI install (Path A) or embedded NuGet (Path B)

## Overview

Run a sample REST API and invoke its methods from the workbench. Same loop, two ways to wire it:

- **Path A (CLI)** — start the sample on its own port, point `bowire --url …` at it from a second terminal. Same as you did against Petstore in [Lesson 0.3](../../unit-0/lesson-3/README.md), but now against a service on `localhost`.
- **Path B (Embedded)** — call `AddBowire()` + `MapBowire()` from inside the same sample so the workbench mounts at `/bowire` alongside the API routes. One process, no second terminal, no second port.

Both end up at the same screen — every operation discovered, form-driven invoke. Pick whichever matches your day-to-day; the rest of the bootcamp's wire-level behaviour is identical either way.

## How auto-discovery works

Bowire never asks you to import a collection or upload a `.proto` file when the server already advertises its own schema. The bundled plugins each know one or two conventional discovery endpoints; pointing `bowire --url <host>` at the server is enough to fetch them.

| Protocol | What the plugin probes for |
|---|---|
| REST | `/openapi/v1.json`, `/swagger/v1/swagger.json`, `/openapi.json`, `/swagger.json` |
| gRPC | Server Reflection (`grpc.reflection.v1.ServerReflection/ServerReflectionInfo`) |
| GraphQL | `POST /graphql` with `IntrospectionQuery` |
| MCP | Streamable-HTTP handshake (`initialize` → `tools/list`, `resources/list`, `prompts/list`) |

This lesson exercises the REST flavour.

## Path A — CLI (two-process)

### A1. Start the sample API

```bash
cd units/unit-1/lesson-1/sample/HelloApi
dotnet run
```

You should see:

```
Now listening on: http://localhost:5001
Application started. Press Ctrl+C to shut down.
```

Leave this terminal open.

### A2. Point Bowire at it

Open a second terminal:

```bash
bowire --url http://localhost:5001
```

This:

- Boots a local workbench UI on `http://localhost:5080/bowire`
- Auto-opens your browser to it
- Tells the workbench: "the server is at `http://localhost:5001`"

The REST plugin probes for an OpenAPI document at the conventional paths, finds the one `.NET 10`'s `MapOpenApi()` generated, parses it, and renders each operation as a method node in the sidebar.

Skip Path B below; jump to **Invoke a method**.

## Path B — Embedded (single-process)

### B1. Add the embedded NuGet to the sample

Open `units/unit-1/lesson-1/sample/HelloApi/HelloApi.csproj` and add the `Kuestenlogik.Bowire` package next to the existing OpenAPI one:

```bash
cd units/unit-1/lesson-1/sample/HelloApi
dotnet add package Kuestenlogik.Bowire
```

### B2. Wire the workbench into `Program.cs`

Edit `Program.cs`. Two changes — one line near the `AddOpenApi()` registration, one line near `MapOpenApi()`:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddBowire();        // ← new
var app = builder.Build();

app.MapOpenApi();
app.MapBowire();                     // ← new

app.MapGet("/hello/{name}", …);      // your existing routes stay put
// ...
app.Run("http://localhost:5001");
```

`AddBowire()` registers the plugin host into DI; `MapBowire()` mounts the workbench at `/bowire`. Your existing routes stay exactly where they are.

### B3. Run the host

```bash
dotnet run
```

You should see:

```
Now listening on: http://localhost:5001
Application started. Press Ctrl+C to shut down.
```

One process now — no second terminal needed. Open <http://localhost:5001/bowire> in your browser. The workbench reads the same OpenAPI document `MapOpenApi()` already serves, plus any further endpoint sources the host registered through DI (gRPC reflection, SignalR hubs, &c.).

> **Note.** Both paths target the same `HelloApi`. If you started with Path A and now want to try Path B, stop the CLI (`Ctrl+C` in the second terminal) and the host (`Ctrl+C` in the first terminal), make the `Program.cs` edits above, then `dotnet run` once. Embedded mode replaces the CLI in this context — the workbench is now part of the host.

## Invoke a method

The next steps are identical on both paths — only the URL differs:

- Path A: <http://localhost:5080/bowire>
- Path B: <http://localhost:5001/bowire>

In the workbench:

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

## Why Bowire over a generic API client?

| Workflow | Postman / Insomnia / Bruno | Bowire |
|---|---|---|
| Add an endpoint | Manually create a request, fill in URL + method + headers | Pointed at a server → auto-discovered |
| Change a contract | Update each saved request to match | Re-discover, the UI re-renders against the new schema |
| Multi-protocol | One tool per protocol family (REST in client A, gRPC in client B, MQTT in client C) | One workbench, every wire |
| Hand it to an AI agent | Build an integration per tool | Already an MCP server (Unit 3) |

## Key Takeaways

1. **Auto-discovery beats hand-curated collections** — the server already knows its own surface; the workbench just asks.
2. **Form-driven invoke** — every operation has a form built from the schema. No hand-crafting JSON bodies for the simple cases.
3. **Two deployment shapes, same workbench** — CLI mounts a separate process at `localhost:5080/bowire` (great for external targets); embedded mounts the workbench at `/bowire` next to your own routes (great when you're building / debugging your own service). The discovered surface, the invoke form, and the response rendering are identical.
4. **Same UI primitives across protocols** — what you learn here transfers verbatim to gRPC / GraphQL / MQTT / SignalR / &c.

## What's Next

You're ready to add a second protocol next to the REST one and see them both side-by-side in the same workbench.

**Test your knowledge:** → [Knowledge Assessment](KNOWLEDGE_ASSESSMENT.md)
**Continue:** → [Lesson 1.2: Multi-protocol session](../lesson-2/README.md)

## Reference

- [REST plugin docs](https://bowire.io/docs/protocols/rest.html)
- [Auto-discovery](https://bowire.io/docs/features/auto-discovery.html)
- [Form ↔ JSON input](https://bowire.io/docs/features/form-json-input.html)
