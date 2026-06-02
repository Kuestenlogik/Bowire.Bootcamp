# Lesson 1.2: Multi-protocol session (REST + gRPC)

> **Difficulty:** Beginner | **Duration:** 10 min (CLI) · 12 min (Embedded) | **Prerequisites:** [Lesson 1.1](../lesson-1/README.md)

## Overview

Get **REST** and **gRPC** rendered in the same workbench, with the same UI primitives. The two paths model this differently — same end result, different "where do the wires live":

- **Path A (CLI)** — two separate services on two ports (`HelloApi` on 5001, `HelloGrpc` on 5002). One `bowire` workbench in a third terminal, pointed at both URLs at once via repeated `--url` flags. Classic polyglot-microservices shape.
- **Path B (Embedded)** — REST and gRPC routes co-hosted in the *same* ASP.NET process; `MapBowire()` in that process picks both up through the host's `IServiceProvider`. Classic single-host-many-wires shape.

You'll also fire your first **server-streaming** call and watch frames arrive live in the workbench's stream pane — same UI on both paths.

## Path A — CLI (two services, two ports, one workbench)

### A1. Keep the REST API running

If the Lesson 1.1 sample (`HelloApi` on `localhost:5001`) is still up, leave it. Otherwise restart it:

```bash
cd ../../unit-1/lesson-1/sample/HelloApi
dotnet run
```

### A2. Start the gRPC service in a second terminal

```bash
cd units/unit-1/lesson-2/sample/HelloGrpc
dotnet run
```

Output:

```
Now listening on: http://localhost:5002
Application started. Press Ctrl+C to shut down.
```

The service registers two methods on the `greeter.Greeter` service:

- `Hello` — unary, returns one greeting.
- `HelloStream` — server-streaming, emits N greetings one per second.

It also enables **gRPC Server Reflection**, so Bowire's gRPC plugin can discover the service shape without you having to upload the `.proto` file.

### A3. Point Bowire at both URLs at once

In a third terminal:

```bash
bowire --url http://localhost:5001 --url http://localhost:5002
```

`--url` is repeatable. Each plugin probes both URLs; whichever protocol matches each URL claims it. The sidebar ends up with two top-level entries:

```
🔌 HelloApi (REST)
   └─ GetGreeting
   └─ PostEcho
   └─ GetHealth
🟢 greeter.Greeter (gRPC)
   └─ Hello              (unary)
   └─ HelloStream        (server-streaming)
```

Skip Path B below; jump to **Invoke the gRPC unary method**.

## Path B — Embedded (REST + gRPC in one host)

The embedded shape sees the same two protocols by **co-hosting** them in one ASP.NET process. That's the natural shape when your service exposes more than one wire (REST + gRPC together is the canonical .NET case). The workbench mounted via `MapBowire()` reads both endpoint sources straight off the `IServiceProvider` — gRPC's reflection registry on one side, the OpenAPI document provider on the other.

> **When this shape doesn't fit.** If your two services are genuinely separate microservices in separate repos (and separate processes), Path A is the right model — embedded inside service A can't see service B's endpoints without cross-process IPC. Use the CLI to span the boundary.

### B1. Take the embedded `HelloApi` from Lesson 1.1 as the starting point

You already added `AddBowire()` + `MapBowire()` to `units/unit-1/lesson-1/sample/HelloApi` in Lesson 1.1 Path B. Keep that. We extend it.

### B2. Add the gRPC packages and a sample service

```bash
cd units/unit-1/lesson-1/sample/HelloApi
dotnet add package Grpc.AspNetCore
dotnet add package Grpc.AspNetCore.Server.Reflection
```

Copy the `Protos/greeter.proto` file and the `GreeterService` class from `units/unit-1/lesson-2/sample/HelloGrpc/` into the `HelloApi` project (or paste the `.proto` + a minimal `GreeterService` directly — the contract is small):

```csharp
sealed class GreeterService : Greeter.GreeterBase
{
    public override Task<HelloReply> Hello(HelloRequest request, ServerCallContext context)
        => Task.FromResult(new HelloReply
        {
            Greeting = $"Hello, {request.Name}!",
            ReceivedAt = DateTimeOffset.UtcNow.ToString("O"),
        });

    public override async Task HelloStream(HelloRequest request,
        IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
    {
        var count = request.Count <= 0 ? 5 : Math.Min(request.Count, 20);
        for (var i = 1; i <= count; i++)
        {
            await responseStream.WriteAsync(new HelloReply
            {
                Greeting = $"Hello {i}/{count}, {request.Name}!",
                ReceivedAt = DateTimeOffset.UtcNow.ToString("O"),
            }, context.CancellationToken);
            await Task.Delay(1000, context.CancellationToken);
        }
    }
}
```

Update `HelloApi.csproj` to include the proto file so the build generates the `Greeter.GreeterBase` partial:

```xml
<ItemGroup>
  <Protobuf Include="Protos\greeter.proto" GrpcServices="Server" />
</ItemGroup>
```

### B3. Wire gRPC into `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddGrpc();                   // ← new
builder.Services.AddGrpcReflection();         // ← new (so Bowire's gRPC plugin can discover)
builder.Services.AddBowire();

var app = builder.Build();
app.MapOpenApi();
app.MapGrpcService<GreeterService>();         // ← new
app.MapGrpcReflectionService();               // ← new
app.MapBowire();

app.MapGet("/hello/{name}", …);
// ... rest of your REST routes ...

app.Run("http://localhost:5001");
```

### B4. Run + verify both protocols show up

```bash
dotnet run
```

Open <http://localhost:5001/bowire>. The sidebar now shows both services in one workbench — discovered from the same host's DI container in one pass:

```
🔌 HelloApi (REST)
   └─ GetGreeting
   └─ PostEcho
   └─ GetHealth
🟢 greeter.Greeter (gRPC)
   └─ Hello
   └─ HelloStream
```

No CLI, no second terminal, no `--url` flag — both wires are intrinsic to this one host.

## Invoke the gRPC unary method

The next steps work the same on both paths. Open the workbench:

- Path A: <http://localhost:5080/bowire>
- Path B: <http://localhost:5001/bowire>

Click **Hello** under `greeter.Greeter`. The form is built from the proto schema: a string `name` field, an int32 `count` field (unused for unary; ignore it). Fill `name = "Bowire"`, click **Invoke**.

You get a single response:

```json
{
  "greeting": "Hello, Bowire!",
  "receivedAt": "2026-06-01T08:30:14.1234567+00:00"
}
```

## Fire the streaming method

Click **HelloStream**. Fill `name = "Bowire"`, `count = 5`, click **Invoke**.

The response pane switches into stream mode and shows frames arriving live, one per second:

```
[#1] { "greeting": "Hello 1/5, Bowire!", "receivedAt": "..." }
[#2] { "greeting": "Hello 2/5, Bowire!", "receivedAt": "..." }
[#3] { "greeting": "Hello 3/5, Bowire!", "receivedAt": "..." }
[#4] { "greeting": "Hello 4/5, Bowire!", "receivedAt": "..." }
[#5] { "greeting": "Hello 5/5, Bowire!", "receivedAt": "..." }
```

After the fifth frame the stream closes and you see the total duration.

## Switch between the two services

Both services are alive in the same workbench — clicking back into REST methods works without re-pointing or restarting. That's the headline: REST and gRPC share the same UI primitives (sidebar, invoke pane, response viewer, recording recorder), so a polyglot service mesh is one workbench, not five tabs.

> **Path A vs Path B in one line:** Path A is "two URLs, one workbench". Path B is "one host, two wires, workbench inside". Both produce the same sidebar; pick whichever matches your real-world deployment shape.

## Key Takeaways

1. **Multi-protocol scales the same way on both paths.** CLI: `--url` is repeatable; auto-discovery picks the right plugin per URL. Embedded: every protocol you register in the host's DI (`AddGrpc()`, `AddSignalR()`, REST routes via `MapXxx`, &c.) lands in the workbench in one pass.
2. **Same UI for unary and streaming.** The response pane shape changes; everything else stays put.
3. **gRPC discovery via reflection.** No `.proto` upload, no manual service descriptor — `AddGrpcReflection()` + `MapGrpcReflectionService()` tells Bowire everything it needs, whether the workbench is in a separate process (CLI) or in-process (Embedded).
4. **Polyglot service mesh = one workbench.** Across any combination of bundled / installed protocols (REST + gRPC + GraphQL + MQTT + SignalR + &c.) — `--url` keeps being repeatable on the CLI; on Embedded, every host registration is automatically visible.

## What's Next

You're ready to capture a session against one of these services and replay it as a self-contained mock.

**Test your knowledge:** → [Knowledge Assessment](KNOWLEDGE_ASSESSMENT.md)
**Continue:** → [Unit 2: Record, Replay, Mock](../../unit-2/README.md)

## Reference

- [gRPC plugin docs](https://bowire.io/docs/protocols/grpc.html)
- [Streaming UI](https://bowire.io/docs/features/streaming.html)
- [Multi-URL workbench mode](https://bowire.io/docs/features/auto-discovery.html#multi-url-sessions)
