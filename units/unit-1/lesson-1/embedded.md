# Lesson 1.1 — Path B: Embedded (single-process)

> **Walking the embedded path.** Lesson context, prerequisites, and the "why Bowire" framing are on the [Lesson 1.1 landing page](README.md). This file covers the embedded walkthrough only.

## What you'll do

Mount the Bowire workbench *inside* the sample REST API via `AddBowire()` + `MapBowire()`. The workbench appears at `/bowire` on the same host:port as the API's own routes. One process, no second terminal.

## Steps

### 1. Add the embedded NuGet to the sample

Open `units/unit-1/lesson-1/sample/HelloApi/HelloApi.csproj` and add the `Kuestenlogik.Bowire` package next to the existing OpenAPI one:

```bash
cd units/unit-1/lesson-1/sample/HelloApi
dotnet add package Kuestenlogik.Bowire
```

### 2. Wire the workbench into `Program.cs`

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

### 3. Run the host

```bash
dotnet run
```

You should see:

```
Now listening on: http://localhost:5001
Application started. Press Ctrl+C to shut down.
```

One process now — no second terminal needed. Open <http://localhost:5001/bowire> in your browser. The workbench reads the same OpenAPI document `MapOpenApi()` already serves, plus any further endpoint sources the host registered through DI (gRPC reflection, SignalR hubs, &c.).

### 4. Invoke a method

In the workbench at <http://localhost:5001/bowire>:

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

## What's Next

**Continue on the embedded path:** → [Lesson 1.2 — Path B (Embedded)](../lesson-2/embedded.md)

Switched your mind? → [Path A (CLI) walkthrough](cli.md)
