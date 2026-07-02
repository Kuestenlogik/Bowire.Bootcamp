# Lesson 2.1: Record & Replay

> **Difficulty:** Beginner | **Duration:** 12 min | **Prerequisites:** [Unit 1](../../unit-1/README.md)

## Overview

Capture a sequence of calls against the Harbor domain, save the trace as a `.bwr` file, then replay it as a **local mock** — same sidebar, same methods, no live backend. This is the "scaffolding for frontend work / fixture for tests / demo without the stack" loop, end to end, from the UI.

Keep the Harbor **Combined** sample running (`:5101/bowire`, from [Unit 1](../../unit-1/lesson-1/README.md)).

## Steps

### 1. Record a few calls

1. In the **Compose** rail, click the red **● Record** button in the action bar — it switches to **■ Stop** and pulses.
2. Invoke three Harbor reads/writes — e.g. *list docks*, *get a ship by id*, *berth a ship*.
3. Click **■ Stop**. A toast confirms the recording was saved (scoped to the current **Workspace**).

### 2. Export the `.bwr`

Switch to the **Recordings** rail — your capture sits at the top. Select it and click **Export → JSON**; save it as `harbor-tour.bwr`. The `.bwr` is portable JSON — check it into a repo as a fixture, share it, drop it into CI.

### 3. Replay it as a mock — from the UI

In the **Mocks** rail (or the recording's **Use as mock** action), pick `harbor-tour.bwr` and **Start mock**. The workbench supervises the mock process and shows its URL (e.g. `http://127.0.0.1:7070`). Every response now comes from the recording, not from the live Combined host.

> **Scriptable equivalent (CLI).** The same replay runs headless as `bowire mock --recording harbor-tour.bwr --port 7070` — the portable form for frontends, CI and peer Bowires. That's a CLI workflow: see [Unit 3 → `bowire mock`](../../unit-3/README.md).

### 4. Point a second workbench at the mock

Open another workbench against the mock URL. The sidebar looks identical — same methods, same forms — but the responses are frozen: a recorded `capturedAt` timestamp is the tell. **A recording is a tape, not a service.** Stop the live Combined host and the mock keeps serving: the snapshot is self-contained.

## Key Takeaways

1. **One button to capture, one click to replay.** No fixture writing — Bowire records what flows through it and plays it back on the same wire.
2. **`.bwr` is portable JSON.** A self-contained snapshot you can commit, share, or run in CI.
3. **Replay is a separate listener.** Whether from the Mocks rail (UI) or `bowire mock` (CLI), the mock is its own port so clients point at *it*, not at the recorded host.

## What's Next

**Continue:** → [Lesson 2.2: Schema-backed mocks](../lesson-2/README.md)

## Reference

- [Recording feature docs](https://bowire.io/docs/features/recording.html)
- [Mock-server docs](https://bowire.io/docs/features/mock-server.html)
