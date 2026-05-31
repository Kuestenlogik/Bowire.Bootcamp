# Lesson 03 — Record & replay

**Time:** 10 minutes • **Prerequisites:** Lesson 01 done (Lesson 02 optional)

## Goal

Capture a sequence of API calls in the Bowire workbench, save the trace as a `.bwr` file, then run that file back as a **local mock server** — same sidebar, same methods, no live backend needed.

This is the "scaffolding for frontend work" / "fixture for tests" / "demo without the stack" loop, end-to-end in one lesson.

## Steps

### 1. Start the REST API from Lesson 01

```bash
cd ../lesson-01-first-call/sample/HelloApi
dotnet run
```

You should see `Now listening on: http://localhost:5001`. Leave it running.

### 2. Open Bowire pointed at it

In a second terminal:

```bash
bowire --url http://localhost:5001
```

The browser opens at `http://localhost:5050/bowire` with the `HelloApi` sidebar tree from Lesson 01.

### 3. Record three calls

1. Click the red **● Record** button in the bottom action bar. It switches to **■ Stop** and pulses.
2. **GetGreeting** — fill `name = "Bowire"`, click **Invoke**.
3. **PostEcho** — fill `message = "Hello mock"`, click **Invoke**.
4. **GetHealth** — no fields, click **Invoke**.
5. Click **■ Stop**.

A toast confirms the recording was saved to `~/.bowire/recordings.json` with a name like `Recording 2026-05-31T20:00:05Z`.

### 4. Export it as a `.bwr` file

Shift-click (or right-click) the record button to open the **Recordings manager**. Select the recording you just made and click **Export JSON**. Save it as `hello-tour.bwr` in this lesson's directory.

> Skipped the recording phase? Use the pre-baked `sample/hello-tour.bwr` next to this README — same three steps, ready to mock.

### 5. Run the mock server

In a third terminal:

```bash
bowire mock --recording hello-tour.bwr --port 7070
```

Output:

```
🟢 Mock server listening on http://127.0.0.1:7070
   Replaying: Hello tour (3 steps)
   Press Ctrl+C to stop.
```

### 6. Point a second Bowire workbench at the mock

```bash
bowire --url http://127.0.0.1:7070
```

The sidebar looks identical to step 2 — same three methods, same form shapes, same Invoke button. The difference: every response now comes from the recording, not from `HelloApi`.

Click **GetGreeting** with `name = "Bowire"`. The response pane shows:

```json
{
  "greeting": "Hello, Bowire!",
  "receivedAt": "2026-05-31T20:00:05.123Z"
}
```

The `receivedAt` matches the moment you (or the sample author) captured the recording — that's the tell: you're seeing a frozen response, not a fresh one. The mock is a tape, not a service.

### 7. Kill the real backend

Go back to the terminal running `HelloApi` and Ctrl+C it.

In the mock-pointed Bowire workbench, click **GetGreeting** again. Same response. **GetHealth** → `{"status":"up"}`. The mock keeps serving with the real backend gone — that's the point: a recording is a self-contained snapshot.

### 8. Hit the mock from anything else

The mock is just an HTTP server. Plain `curl` works:

```bash
curl http://127.0.0.1:7070/hello/anyone
# → {"greeting":"Hello, Bowire!","receivedAt":"2026-05-31T20:00:05.123Z"}
```

Note the path template — the recorded step's `httpPath` was `/hello/{name}`, so anything you put in the `{name}` slot matches the same response. Path templates are auto-detected when the recorded path contains `{`; literal paths stay literal.

## What you just saw

- **One button to capture, one command to replay.** No fixture writing, no protocol-specific serialisation — Bowire records what flows through it, the mock plays it back.
- **Self-contained on disk.** The `.bwr` file is portable JSON. Check it into your repo as a test fixture; share it with a teammate; drop it into a CI job.
- **Same wire on replay.** Recording captures REST → replay serves REST; same with gRPC, GraphQL, &c. The mock isn't a generic HTTP echo — it speaks each protocol's contract correctly.
- **Path-template matching** — `/users/{id}` style placeholders auto-bind on replay, so a recording made with one ID matches calls with any ID. Literal paths stay strict.

## What's next

[Lesson 04 — AI-agent integration](../lesson-04-ai-agents/) wires Bowire into Claude Desktop / Cursor over MCP so an agent can call any of these protocols on your behalf — and yes, the agent can record sessions and run them back too.

## Reference

- [Recording feature docs](https://bowire.io/docs/features/recording.html) — Capture, replay, convert-to-tests, HAR/JSON export.
- [Mock-server docs](https://bowire.io/docs/features/mock-server.html) — CLI flags, ASP.NET middleware, matcher semantics, path-template behaviour.
- [Recording file format](https://bowire.io/docs/architecture/file-formats.html) — the on-disk shape `BowireRecording` serialises to.
