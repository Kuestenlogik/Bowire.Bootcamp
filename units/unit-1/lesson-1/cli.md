# Lesson 1.1 — Path A: CLI (two-process)

> **Walking the standalone-CLI path.** Lesson context, prerequisites, and the "why Bowire" framing are on the [Lesson 1.1 landing page](README.md). This file covers the CLI walkthrough only.

## What you'll do

Start a sample REST API on `localhost:5001`, point the `bowire` CLI at it from a second terminal, invoke a method from the browser UI.

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

## What's Next

**Test your knowledge:** → [Knowledge Assessment](KNOWLEDGE_ASSESSMENT.md)
**Continue on the CLI path:** → [Lesson 1.2 — Path A (CLI)](../lesson-2/cli.md)

Switched your mind? → [Path B (Embedded) walkthrough](embedded.md)
