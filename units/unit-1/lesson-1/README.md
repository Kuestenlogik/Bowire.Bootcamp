# Lesson 1.1: Your first call (REST + OpenAPI)

> **Difficulty:** Beginner | **Duration:** 5 min (CLI) · 10 min (Embedded) | **Prerequisites:** [Unit 0](../../unit-0/README.md) complete, [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Overview

Run a sample REST API and invoke its methods from the workbench. Same loop, two ways to wire it. **Pick your path** below — the wire-in differs, but the discovered surface, the invoke form, and the response rendering are identical.

| Path | Walkthrough | Duration | What you'll do |
|---|---|---|---|
| **A — CLI (two-process)** | [→ `cli.md`](cli.md) | 5 min | Start the sample on its own port, point `bowire --url …` at it from a second terminal |
| **B — Embedded (single-process)** | [→ `embedded.md`](embedded.md) | 10 min | Call `AddBowire()` + `MapBowire()` from inside the sample so the workbench mounts at `/bowire` alongside the API routes |

[Lesson 0.1 — Two ways to run Bowire](../../unit-0/lesson-1/README.md) covers the choice in depth if you haven't picked yet.

## How auto-discovery works (both paths)

Bowire never asks you to import a collection or upload a `.proto` file when the server already advertises its own schema. The bundled plugins each know one or two conventional discovery endpoints; pointing the workbench at a server URL is enough to fetch them.

| Protocol | What the plugin probes for |
|---|---|
| REST | `/openapi/v1.json`, `/swagger/v1/swagger.json`, `/openapi.json`, `/swagger.json` |
| gRPC | Server Reflection (`grpc.reflection.v1.ServerReflection/ServerReflectionInfo`) |
| GraphQL | `POST /graphql` with `IntrospectionQuery` |
| MCP | Streamable-HTTP handshake (`initialize` → `tools/list`, `resources/list`, `prompts/list`) |

This lesson exercises the REST flavour.

## Why Bowire over a generic API client?

| Workflow | Postman / Insomnia / Bruno | Bowire |
|---|---|---|
| Add an endpoint | Manually create a request, fill in URL + method + headers | Pointed at a server → auto-discovered |
| Change a contract | Update each saved request to match | Re-discover, the UI re-renders against the new schema |
| Multi-protocol | One tool per protocol family (REST in client A, gRPC in client B, MQTT in client C) | One workbench, every wire |
| Hand it to an AI agent | Build an integration per tool | Already an MCP server (Unit 3) |

## Key Takeaways (both paths)

1. **Auto-discovery beats hand-curated collections** — the server already knows its own surface; the workbench just asks.
2. **Form-driven invoke** — every operation has a form built from the schema. No hand-crafting JSON bodies for the simple cases.
3. **Two deployment shapes, same workbench** — CLI mounts a separate process at `localhost:5080/bowire`; embedded mounts the workbench at `/bowire` next to your own routes. The discovered surface, the invoke form, and the response rendering are identical.
4. **Same UI primitives across protocols** — what you learn here transfers verbatim to gRPC / GraphQL / MQTT / SignalR / &c.

## What's Next

Once you've finished your chosen path:

**Continue:** → [Lesson 1.2: Multi-protocol session](../lesson-2/README.md) — picks up where your chosen path leaves off

## Reference

- [REST plugin docs](https://bowire.io/docs/protocols/rest.html)
- [Auto-discovery](https://bowire.io/docs/features/auto-discovery.html)
- [Form ↔ JSON input](https://bowire.io/docs/features/form-json-input.html)
