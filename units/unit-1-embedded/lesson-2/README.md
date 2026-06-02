# Lesson 1.2 (Embedded track): Add a second protocol — gRPC

> **Difficulty:** Beginner | **Duration:** 12 min | **Prerequisites:** [Lesson 1.1](../lesson-1/README.md)

## Overview

Co-host **gRPC** alongside the REST routes you already have. One ASP.NET process, two wires, one workbench — the embedded shape's headline. `MapBowire()` sees both protocols by reading the host's `IServiceProvider`: gRPC's reflection registry on one side, the OpenAPI document provider on the other. No `--url` flag, no probing — the host already knows what it exposes.

You'll also fire your first **server-streaming** call and watch frames arrive live in the workbench's stream pane.

## When this shape doesn't fit

If your two services are genuinely separate microservices in separate repos (and separate processes), the embedded shape can't bridge them — `MapBowire()` inside service A can't see service B's endpoints without cross-process IPC. Use the [CLI track](../../unit-1-cli/lesson-2/README.md) to span that boundary instead.

## Steps

### 1. Start from the embedded `HelloApi`

You already added `AddBowire()` + `MapBowire()` to `units/unit-1-samples/HelloApi` in Lesson 1.1. Keep that. We extend it with a second protocol.

### 2. Add the gRPC packages

```bash
cd units/unit-1-samples/HelloApi
dotnet add package Grpc.AspNetCore
dotnet add package Grpc.AspNetCore.Server.Reflection
```

### 3. Bring in the proto + service

Copy `Protos/greeter.proto` and the `GreeterService` class from `units/unit-1-samples/HelloGrpc/` into the `HelloApi` project. The service:

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

### 4. Wire gRPC into `Program.cs`

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
app.MapPost("/echo", …);
app.MapGet("/health", …);

app.Run("http://localhost:5001");
```

### 5. Run + verify both protocols show up

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
   └─ Hello              (unary)
   └─ HelloStream        (server-streaming)
```

No CLI, no second terminal, no `--url` flag — both wires are intrinsic to this one host.

### 6. Invoke the gRPC unary method

Click **Hello** under `greeter.Greeter`. The form is built from the proto schema: a string `name` field, an int32 `count` field (unused for unary; ignore it). Fill `name = "Bowire"`, click **Invoke**.

You get a single response:

```json
{
  "greeting": "Hello, Bowire!",
  "receivedAt": "2026-06-01T08:30:14.1234567+00:00"
}
```

### 7. Fire the streaming method

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

## Key Takeaways

1. **Every protocol you `AddXxx()` is automatically visible.** No per-protocol Bowire registration step — `AddGrpc()` makes the gRPC service discoverable via reflection; `AddSignalR()` makes hubs discoverable; REST routes via `MapXxx` are read off the OpenAPI document. `MapBowire()` sees them all.
2. **One host, two wires, one workbench.** REST and gRPC share the same port, the same lifecycle, and the same UI primitives in the workbench (sidebar, invoke pane, response viewer, recording recorder).
3. **gRPC discovery via reflection.** `AddGrpcReflection()` + `MapGrpcReflectionService()` is the standard `Grpc.AspNetCore.Server.Reflection` setup — nothing Bowire-specific. Bowire's gRPC plugin reads what reflection exposes, same as `grpcurl` or any other gRPC client.
4. **Streaming UI is automatic.** Server-streaming methods render with a stream-frames pane; unary methods get a single-response pane. The form, the invoke button, the cancel-stream affordance — all stock.

## What's Next

Both setup tracks merge here. From this point on, the workbench UI is identical regardless of how Bowire got mounted — so the rest of the bootcamp is shared.

**Continue:** → [Unit 2: Record, Replay, Mock](../../unit-2/README.md)

## Reference

- [Embedded mode docs](https://bowire.io/docs/features/embedded-mode.html)
- [gRPC plugin docs](https://bowire.io/docs/protocols/grpc.html)
- [Streaming UI](https://bowire.io/docs/features/streaming.html)
