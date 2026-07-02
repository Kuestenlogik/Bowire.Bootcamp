# Lesson 1.1: First contact

> **Difficulty:** Beginner | **Duration:** 10 min | **Prerequisites:** [Unit 0](../../unit-0/README.md); a running sample (see below)

## Overview

Your first hands-on time in the workbench: discover a service in the sidebar and invoke a unary method — all from the browser UI. This lesson is the **canonical invoke walkthrough**; the CLI, embedded and extension units link back here instead of repeating it.

> **You already have a workbench open** from [Lesson 0.3](../../unit-0/lesson-3/README.md). This unit uses the Harbor **Combined** sample from [`Bowire.Samples`](https://github.com/Kuestenlogik/Bowire.Samples) — it embeds Bowire and speaks REST + gRPC + SignalR + WebSocket + SSE against one domain, so open **<http://localhost:5101/bowire>**.
>
> Skipped setup? `cd harbor-demo/src/Kuestenlogik.Bowire.Samples.Combined && dotnet run` in a `Bowire.Samples` clone, then open the URL above — full options in [Lesson 0.3](../../unit-0/lesson-3/README.md). The UI below is identical however the workbench was mounted.

## The workbench at a glance

The workbench loads with the **vertical rail strip** down the left edge — one rail per activity (Compose, Recordings, Mocks, Flows, Interceptor, Benchmarking, Help, Workspaces, Settings; see [Lesson 0.1](../../unit-0/lesson-1/README.md)). For first contact you mostly live in two:

- **Compose rail** — the request builder + invoke pane (per-protocol layouts).
- **Workspaces rail** — the workspace picker (scopes env vars, recordings, saved tabs). First boot lands you in a `Default` workspace; nothing to create to follow along.

## Steps

### 1. Discover the service

With the workbench open against the Combined sample, the sidebar tree auto-populates from everything the host advertises — the Harbor domain (`Ship`, `Dock`, `Crane`, `Container`, `PortCall`) shows up as method nodes, grouped by protocol. No manual schema upload: the REST plugin read the host's OpenAPI document, the gRPC plugin read Server Reflection, and so on — all discovered in one pass.

### 2. Invoke a unary method

1. In the sidebar, expand the **REST** entry and pick a simple read — e.g. *list ships* or *get a dock by id*.
2. The Compose rail renders a **form built from the schema** — path/query params and, for a body, a JSON editor.
3. Fill any required fields and click **Invoke** (or `Ctrl+Enter`).
4. The response pane shows the status, headers, and the JSON body.

That's the whole loop: **Discover → pick a method → fill the schema-built form → Invoke → read the response.** It is identical for every protocol and every service you point the workbench at.

### 3. Try another operation

Pick a method that takes a request body (a create/update). Notice the invoke pane auto-builds a JSON body editor from the request schema — you never hand-write the envelope.

## Key Takeaways

1. **Auto-discovery is the headline.** The sidebar populates from the service's own schema surface (OpenAPI / reflection / introspection) — no manual operation authoring.
2. **One invoke primitive.** Every method — any protocol — gets the same schema-driven form + Invoke + response pane.
3. **The workbench is shape-agnostic.** Whether Bowire was embedded (this lesson) or launched via the CLI, the UI you just used is the same.

## What's Next

**Continue:** → [Lesson 1.2: Multi-protocol in one workbench](../lesson-2/README.md)

## Reference

- [Bowire.Samples — Harbor demo](https://github.com/Kuestenlogik/Bowire.Samples)
- [Auto-discovery](https://bowire.io/docs/features/auto-discovery.html)
