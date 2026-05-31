# Lesson 01 — Your first call

**Time:** 5 minutes • **Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download), Bowire installed (`dotnet tool install --global Kuestenlogik.Bowire.Tool`)

## Goal

Run a sample REST API on `localhost`, point `bowire` at it, invoke a method from the browser UI, see the response. That's the whole loop — same shape no matter which protocol you're working with later.

## Steps

### 1. Start the sample API

```bash
cd lesson-01-first-call/sample/HelloApi
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

The REST plugin probes for an OpenAPI document at the conventional paths (`/openapi/v1.json`, `/swagger.json`, …), finds the one `.NET 10`'s `MapOpenApi()` generated, parses it, and renders each operation as a method node in the sidebar.

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

## What you just saw

- **Auto-discovery.** Bowire didn't need a manual collection or a `.proto` import. It saw the OpenAPI doc the API publishes, walked it, and rendered the UI.
- **Form-driven invoke.** Every operation has a form built from the schema. No hand-crafting JSON bodies for the simple cases.
- **Two-process model.** Your service runs in one terminal, the workbench in another. The workbench is a debugger, not a runtime — it doesn't replace your server.

## What's next

[Lesson 02 — Multi-protocol session](../lesson-02-multi-protocol/) brings up a gRPC service alongside the REST one and shows them both in the same workbench. This is where Bowire's "one tool for every wire" story starts paying off.

## Reference

- [REST plugin docs](https://bowire.io/docs/protocols/rest.html)
- [Auto-discovery](https://bowire.io/docs/features/auto-discovery.html)
- [Form ↔ JSON input](https://bowire.io/docs/features/form-json-input.html)
