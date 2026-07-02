# Lesson 1.2: Multi-protocol in one workbench

> **Difficulty:** Beginner | **Duration:** 10 min | **Prerequisites:** [Lesson 1.1](../lesson-1/README.md)

## Overview

The Harbor **Combined** sample speaks REST, gRPC, SignalR, WebSocket and SSE against the *same* domain — so one workbench shows every wire side by side. You'll invoke a gRPC unary call, fire a **server-streaming** method and watch frames arrive live, then switch between protocols without re-pointing anything.

Keep the Combined sample from [Lesson 1.1](lesson-1/README.md) running (`:5101/bowire`).

## Steps

### 1. See every protocol in one sidebar

The sidebar groups the Harbor domain by protocol — REST operations, the gRPC `greeter`-style services, the SignalR hub, the WebSocket + SSE endpoints — all discovered from the one Combined host:

```
🔌 REST        (HarborStore over HTTP)
🟢 gRPC        (unary + streaming methods)
📡 SignalR     (hub methods + streams)
🔌 WebSocket   (text / binary frames)
📶 SSE         (event stream)
```

They share the same UI primitives — sidebar, invoke pane, response viewer, recorder — so a polyglot mesh is *one workbench, not five tabs*.

### 2. Invoke a gRPC unary method

Pick a unary gRPC method, fill the schema-built form, click **Invoke**. You get a single response in the same pane shape you saw for REST in Lesson 1.1 — the wire changed, the invoke loop didn't.

### 3. Fire a server-streaming method

Pick a **server-streaming** method (gRPC stream or the SSE endpoint). Invoke it: the response pane switches to **stream mode** and frames arrive live, one after another:

```
[#1] { … }
[#2] { … }
[#3] { … }
```

When the stream ends you see the frame count + total duration. A **Cancel** affordance stops an open stream early.

### 4. Switch between protocols

Click back into a REST method — no restart, no re-point. Every discovered service stays live in the same workbench simultaneously.

## Getting *multiple hosts* into one workbench

The Combined sample is a single host exposing many wires. When your services are **separate processes**, you bring them together outside the UI:

- **CLI:** `bowire --url <a> --url <b>` — `--url` is repeatable. → [Unit 3: CLI & operations](../../unit-3/README.md).
- **Embedded:** co-host protocols in one ASP.NET host so `MapBowire()` discovers them together. → [Unit 4: Embed Bowire](../../unit-4/README.md).

Either way, once discovered, the invoke/stream/switch flow above is unchanged.

## Key Takeaways

1. **One UI for every wire.** REST, gRPC, streaming, messaging — same sidebar, same invoke pane; only the response-pane shape adapts (single vs stream).
2. **Streaming is first-class.** Server-streaming methods render a live frames pane with a cancel affordance.
3. **Composing services is a shape concern, not a UI concern.** Multiple hosts come together via the CLI (`--url`) or embedded co-hosting — the workbench treats them identically afterwards.

## What's Next

You can drive any service across any protocol. Next: capture what you invoke and replay it.

**Continue:** → [Unit 2: The Workbench — record, mock, assert, cover](../../unit-2/README.md)

## Reference

- [Bowire.Samples — Harbor demo](https://github.com/Kuestenlogik/Bowire.Samples)
- [Streaming UI](https://bowire.io/docs/features/streaming.html)
