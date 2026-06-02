# Lesson 1.2 (CLI track): Multi-protocol — REST + gRPC

> **Difficulty:** Beginner | **Duration:** 10 min | **Prerequisites:** [Lesson 1.1](../lesson-1/README.md)

## Overview

Add a **gRPC** service alongside the REST one from Lesson 1.1 and watch them light up in the same workbench. The trick: `--url` is repeatable — one CLI invocation, two backend URLs, one workbench.

You'll also fire your first **server-streaming** call and watch frames arrive live in the workbench's stream pane.

## Steps

### 1. Keep the REST API running

If the Lesson 1.1 sample (`HelloApi` on `localhost:5001`) is still up, leave it. Otherwise restart it:

```bash
cd units/unit-1-samples/HelloApi
dotnet run
```

### 2. Start the gRPC service in a second terminal

```bash
cd units/unit-1-samples/HelloGrpc
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

### 3. Point Bowire at both URLs at once

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

### 4. Invoke the gRPC unary method

Click **Hello** under `greeter.Greeter`. The form is built from the proto schema: a string `name` field, an int32 `count` field (unused for unary; ignore it). Fill `name = "Bowire"`, click **Invoke**.

You get a single response:

```json
{
  "greeting": "Hello, Bowire!",
  "receivedAt": "2026-06-01T08:30:14.1234567+00:00"
}
```

### 5. Fire the streaming method

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

### 6. Switch between the two services

Both services are alive in the same workbench — clicking back into REST methods works without re-pointing or restarting. That's the headline: REST and gRPC share the same UI primitives (sidebar, invoke pane, response viewer, recording recorder), so a polyglot service mesh is one workbench, not five tabs.

## Key Takeaways

1. **`--url` is repeatable.** Pass it as many times as you have services to wire up. Auto-discovery sorts out which plugin claims which URL.
2. **Same UI for unary and streaming.** The response pane shape changes; everything else stays put.
3. **gRPC discovery via reflection.** No `.proto` upload, no manual service descriptor — `AddGrpcReflection()` + `MapGrpcReflectionService()` on the server side tells Bowire everything it needs.
4. **Polyglot service mesh = one workbench.** Across any combination of bundled / installed protocols (REST + gRPC + GraphQL + MQTT + SignalR + &c.) — `--url` keeps being repeatable; auto-discovery scales linearly with services.

## What's Next

Both setup tracks merge here. From this point on, the workbench UI is identical regardless of how Bowire got mounted — so the rest of the bootcamp is shared.

**Continue:** → [Unit 2: Record, Replay, Mock](../../unit-2/README.md)

## Reference

- [gRPC plugin docs](https://bowire.io/docs/protocols/grpc.html)
- [Streaming UI](https://bowire.io/docs/features/streaming.html)
- [Multi-URL workbench mode](https://bowire.io/docs/features/auto-discovery.html#multi-url-sessions)
