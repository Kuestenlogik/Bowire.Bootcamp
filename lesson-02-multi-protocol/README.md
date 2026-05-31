# Lesson 02 — Multi-protocol session

**Time:** 10 minutes • **Prerequisites:** Lesson 01 done

## Goal

Run two services side by side — the **REST** API from Lesson 01 *and* a new **gRPC** service — and point a single Bowire workbench at both. Both protocols show up in the same sidebar; clicking either renders the same form-driven invoke UI. That's the "one tool for every wire" claim with one wire's worth of sweat.

You'll also fire your first **server-streaming** call and watch frames arrive live in the workbench's stream pane.

## Steps

### 1. Keep the REST API running

If the Lesson 01 sample (`HelloApi` on `localhost:5001`) is still up, leave it. Otherwise restart it:

```bash
cd ../lesson-01-first-call/sample/HelloApi
dotnet run
```

### 2. Start the gRPC service in a second terminal

```bash
cd lesson-02-multi-protocol/sample/HelloGrpc
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

## What you just saw

- **Two protocols, one URL list.** `--url` is repeatable; auto-discovery picks the right plugin per URL.
- **Same UI for unary and streaming.** The response pane shape changes; everything else stays put.
- **gRPC discovery via reflection.** No `.proto` upload, no manual service descriptor — Server Reflection on the gRPC service tells Bowire everything it needs.

## What's next

[Lesson 03 — Record & replay](../lesson-03-record-replay/) shows how to capture a session against either service, save it, and run the recording back as a local mock — same protocol on the wire, no real backend needed.

## Reference

- [gRPC plugin docs](https://bowire.io/docs/protocols/grpc.html)
- [Streaming UI](https://bowire.io/docs/features/streaming.html)
- [Multi-URL workbench mode](https://bowire.io/docs/features/auto-discovery.html#multi-url-sessions)
