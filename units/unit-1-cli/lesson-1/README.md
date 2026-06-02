# Lesson 1.1 (CLI track): First call

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
