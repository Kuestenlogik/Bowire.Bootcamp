# Lesson 2.1: Record & Replay

> **Difficulty:** Beginner | **Duration:** 10 min | **Prerequisites:** [Lesson 1.1](../../unit-1/lesson-1/README.md) (Lesson 1.2 optional)

## Overview

Capture a sequence of API calls in the Bowire workbench, save the trace as a `.bwr` file, then run that file back as a **local mock server** — same sidebar, same methods, no live backend needed.

This is the "scaffolding for frontend work" / "fixture for tests" / "demo without the stack" loop, end-to-end in one lesson.

The **recording step** differs by path:
- **Path A (CLI)** — record from the separate workbench, save to `~/.bowire/recordings.json`, export a `.bwr`.
- **Path B (Embedded)** — record from the workbench mounted at `/bowire` inside your host; same button, same export, but the recording lives in the host's per-process state by default.

The **mock-replay step** is shared. `bowire mock --recording <file>` runs as a standalone process on its own port regardless of how the recording got captured — that's by design: the mock is what your downstream clients (frontend, CI, peer Bowire workbench) point at, so it shouldn't live inside the service it's standing in for.

## Path A — CLI (record via standalone workbench)

### A1. Start the REST API from Lesson 1.1

```bash
cd ../../unit-1/lesson-1/sample/HelloApi
dotnet run
```

You should see `Now listening on: http://localhost:5001`. Leave it running.

### A2. Open Bowire pointed at it

In a second terminal:

```bash
bowire --url http://localhost:5001
```

The browser opens at `http://localhost:5050/bowire` with the `HelloApi` sidebar tree from Lesson 01.

### A3. Record three calls

1. Click the red **● Record** button in the bottom action bar. It switches to **■ Stop** and pulses.
2. **GetGreeting** — fill `name = "Bowire"`, click **Invoke**.
3. **PostEcho** — fill `message = "Hello mock"`, click **Invoke**.
4. **GetHealth** — no fields, click **Invoke**.
5. Click **■ Stop**.

A toast confirms the recording was saved to `~/.bowire/recordings.json` with a name like `Recording 2026-05-31T20:00:05Z`.

### A4. Export it as a `.bwr` file

Shift-click (or right-click) the record button to open the **Recordings manager**. Select the recording you just made and click **Export JSON**. Save it as `hello-tour.bwr` in this lesson's directory.

> Skipped the recording phase? Use the pre-baked `sample/hello-tour.bwr` next to this README — same three steps, ready to mock.

Skip Path B below; jump to **Run the mock server**.

## Path B — Embedded (record via in-host workbench)

### B1. Run the embedded `HelloApi` from Lesson 1.1

```bash
cd units/unit-1/lesson-1/sample/HelloApi
dotnet run
```

You should see `Now listening on: http://localhost:5001`. The host has both your REST routes *and* the workbench at `/bowire` from the Lesson 1.1 Path B wire-in. One process, no separate CLI.

### B2. Open the embedded workbench

Browser: <http://localhost:5001/bowire> — the `HelloApi` sidebar appears (REST methods discovered through the host's OpenAPI document provider, no URL round-trip).

### B3. Record three calls (same UI as Path A)

The record button, the recordings manager, and the export-JSON dialog are identical to Path A — they're the same React component, just served from the embedded host instead of the standalone CLI host. Repeat A3 verbatim from inside the embedded workbench: hit Record, fire **GetGreeting / PostEcho / GetHealth**, hit Stop.

> **Where the recording lives.** Embedded mode persists recordings through the host's regular data tier (in-memory by default; opt into disk persistence via the bootstrap config — see [embedded docs](https://bowire.io/docs/setup/embedded.html)). It does **not** auto-populate `~/.bowire/recordings.json` the way the CLI does — that file is the standalone CLI's user-state store.

### B4. Export the `.bwr`

Recordings manager → **Export JSON** → save as `hello-tour.bwr`. The file shape is identical across paths; a recording captured embedded is byte-compatible with `bowire mock` and vice versa.

## Run the mock server

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

## Point a second Bowire workbench at the mock

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

## Kill the real backend

Go back to the terminal running `HelloApi` and Ctrl+C it.

In the mock-pointed Bowire workbench, click **GetGreeting** again. Same response. **GetHealth** → `{"status":"up"}`. The mock keeps serving with the real backend gone — that's the point: a recording is a self-contained snapshot.

## Hit the mock from anything else

The mock is just an HTTP server. Plain `curl` works:

```bash
curl http://127.0.0.1:7070/hello/anyone
# → {"greeting":"Hello, Bowire!","receivedAt":"2026-05-31T20:00:05.123Z"}
```

Note the path template — the recorded step's `httpPath` was `/hello/{name}`, so anything you put in the `{name}` slot matches the same response. Path templates are auto-detected when the recorded path contains `{`; literal paths stay literal.

## Key Takeaways

1. **One button to capture, one command to replay.** No fixture writing, no protocol-specific serialisation — Bowire records what flows through it, the mock plays it back.
2. **Recording works on either deployment shape.** CLI workbench at `localhost:5080/bowire` or embedded workbench at `<your-host>/bowire` — same record button, byte-compatible `.bwr` export.
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
