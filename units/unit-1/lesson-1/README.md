# Lesson 1.1: Your first call (REST + OpenAPI)

> **Difficulty:** Beginner | **Duration:** 5 min | **Prerequisites:** [Unit 0](../../unit-0/README.md) complete, [.NET 10 SDK](https://dotnet.microsoft.com/download), Bowire CLI installed

## Overview

Run a sample REST API on `localhost`, point the `bowire` CLI at it, invoke a method from the browser UI, see the response. That's the entire workbench loop — same shape no matter which protocol you're working with later.

## How auto-discovery works

Bowire never asks you to import a collection or upload a `.proto` file when the server already advertises its own schema. The bundled plugins each know one or two conventional discovery endpoints; pointing `bowire --url <host>` at the server is enough to fetch them.

| Protocol | What the plugin probes for |
|---|---|
| REST | `/openapi/v1.json`, `/swagger/v1/swagger.json`, `/openapi.json`, `/swagger.json` |
| gRPC | Server Reflection (`grpc.reflection.v1.ServerReflection/ServerReflectionInfo`) |
| GraphQL | `POST /graphql` with `IntrospectionQuery` |
| MCP | Streamable-HTTP handshake (`initialize` → `tools/list`, `resources/list`, `prompts/list`) |

This lesson exercises the REST flavour.

## Steps

### 1. Start the sample API

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

### 2. Point Bowire at it

Open a second terminal:

```bash
bowire --url http://localhost:5001
```

This:

- Boots a local workbench UI on `http://localhost:5080/bowire`
- Auto-opens your browser to it
- Tells the workbench: "the server is at `http://localhost:5001`"

The REST plugin probes for an OpenAPI document at the conventional paths, finds the one `.NET 10`'s `MapOpenApi()` generated, parses it, and renders each operation as a method node in the sidebar.

### 3. Invoke a method

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
3. **Two-process model** — your service runs in one terminal, the workbench in another. The workbench is a debugger, not a runtime — it doesn't replace your server.
4. **Same UI primitives across protocols** — what you learn here transfers verbatim to gRPC / GraphQL / MQTT / SignalR / &c.

## What's Next

You're ready to add a second protocol next to the REST one and see them both side-by-side in the same workbench.

**Test your knowledge:** → [Knowledge Assessment](KNOWLEDGE_ASSESSMENT.md)
**Continue:** → [Lesson 1.2: Multi-protocol session](../lesson-2/README.md)

## Reference

- [REST plugin docs](https://bowire.io/docs/protocols/rest.html)
- [Auto-discovery](https://bowire.io/docs/features/auto-discovery.html)
- [Form ↔ JSON input](https://bowire.io/docs/features/form-json-input.html)
