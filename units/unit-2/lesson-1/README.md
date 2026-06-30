# Lesson 2.1: Record & Replay

> **Difficulty:** Beginner | **Duration:** 10 min | **Prerequisites:** [Unit 1 Lesson 1.1](../../unit-1/lesson-1/README.md) (CLI or Embedded shape — both work; Lesson 1.2 optional)

## Overview

Capture a sequence of API calls in the Bowire workbench, save the trace as a `.bwr` file, then run that file back as a **local mock server** — same sidebar, same methods, no live backend needed.

This is the "scaffolding for frontend work" / "fixture for tests" / "demo without the stack" loop, end-to-end in one lesson.

> **The workbench URL depends on your track.**
>
> | Track | Workbench URL |
> |---|---|
> | CLI | `http://localhost:5080/bowire` (after `bowire --url http://localhost:5001`) |
> | Embedded | `http://localhost:5001/bowire` (mounted in the host) |
>
> Everything else — the record button, the recordings manager, the `.bwr` export — is the same UI; the file format is byte-compatible across tracks. The `bowire mock` half (below) runs as a standalone CLI command on both tracks: the mock is a separate process by design, so downstream clients (frontend, CI, peer Bowire) can point at it without going through the recorded host.

## Steps

### 1. Start the REST API from Lesson 1.1

```bash
cd ../../unit-1-samples/HelloApi
dotnet run
```

You should see `Now listening on: http://localhost:5001`. Leave it running.

### 2. Open the workbench

Open the URL for your track from the table above. The `HelloApi` sidebar tree from Lesson 1.1 is there.

### 3. Record three calls

1. Click the red **● Record** button in the bottom action bar. It switches to **■ Stop** and pulses.
2. **GetGreeting** — fill `name = "Bowire"`, click **Invoke**.
3. **PostEcho** — fill `message = "Hello mock"`, click **Invoke**.
4. **GetHealth** — no fields, click **Invoke**.
5. Click **■ Stop**.

A toast confirms the recording was saved. CLI mode persists it to `~/.bowire/recordings.json`; embedded mode keeps it in the host's per-process state by default (opt into disk persistence via `Bowire:Recording:Persist` — see the [embedded config reference](https://bowire.io/docs/setup/embedded.html)).

### 4. Export the recording as a `.bwr` file

Shift-click (or right-click) the record button to open the **Recordings manager**. Select the recording you just made and click **Export JSON**. Save it as `hello-tour.bwr` in this lesson's directory.

> Skipped the recording phase? Use the pre-baked `sample/hello-tour.bwr` next to this README — same three steps, ready to mock.

### 5. Run the mock server

In a separate terminal:

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

## Key Takeaways

1. **One button to capture, one command to replay.** No fixture writing, no protocol-specific serialisation — Bowire records what flows through it, the mock plays it back.
2. **The recording UI is identical on both tracks.** Same record button, same export dialog, byte-compatible `.bwr` files. The only difference is which URL the workbench lives at and where the in-progress recording is stored.
3. **Mock-replay is intentionally CLI-only.** `bowire mock --recording` runs as a standalone process on its own port — clients (frontend, CI, peer Bowire) point at *that* port, not at the recorded host's port. Embedding the mock alongside the real routes would mix replay and live traffic in the same listener; we don't.
4. **Self-contained on disk.** The `.bwr` file is portable JSON. Check it into your repo as a test fixture, share it with a teammate, drop it into a CI job.
5. **Same wire on replay.** REST recording → REST mock, gRPC recording → gRPC mock, &c. The mock isn't a generic HTTP echo; it speaks each protocol's contract correctly.
6. **Path-template matching.** `/users/{id}` style placeholders auto-bind on replay, so a recording made with one ID matches calls with any ID. Literal paths stay strict.

## What's Next

You're ready to extend the recording with the source schema, so the mock also exposes the *full* original contract to peer-discovery clients.

**Continue:** → [Lesson 2.2: Schema export + mock-as-stand-in](../lesson-2/README.md)

## Reference

- [Recording feature docs](https://bowire.io/docs/features/recording.html) — Capture, replay, convert-to-tests, HAR/JSON export.
- [Mock-server docs](https://bowire.io/docs/features/mock-server.html) — CLI flags, ASP.NET middleware, matcher semantics, path-template behaviour.
- [Recording file format](https://bowire.io/docs/architecture/file-formats.html) — the on-disk shape `BowireRecording` serialises to.
