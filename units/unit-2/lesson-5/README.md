# Lesson 2.5: Intercept rail — four postures

> **Difficulty:** Intermediate | **Duration:** 12 min | **Prerequisites:** [Unit 1](../../unit-1/README.md)

## Overview

The **Intercept** rail is where Bowire sits *in the path* of real traffic — capturing it, and optionally shaping it. It has **four postures**; knowing which to reach for is most of the skill. This lesson is the UI tour of the rail. **Standing up** the interception path (a standalone reverse-proxy, or middleware in your host) is a shape concern — this lesson links to those units rather than opening them inline.

> **Getting traffic into the rail:**
> - **Standalone reverse-proxy** (front any upstream from the CLI) → [Unit 3: CLI & operations](../../unit-3/README.md).
> - **Embedded middleware** (`UseBowireInterceptor()` in your host) → [Unit 4: Embed Bowire](../../unit-4/README.md).
>
> Once traffic flows, everything below is the same UI.

## The four postures

| Posture | What it's for |
|---|---|
| **Captured** | Read-only inspection of intercepted flows — request/response pairs as they really happened. Your default lens. |
| **Live overrides** | Per-route substitutions *inside* the intercept pipeline — rewrite a response, inject a status/latency, on live traffic. |
| **Mock servers** | Standalone replay hosts on their own port (the Unit 2.1 mock, surfaced here). Not in the live path — a separate listener. |
| **Settings** | Configure the interceptor — capacity, body-capture caps, TLS, upstream — for the current mode. |

The distinction that trips people up: **Live overrides ≠ Mock servers.** Overrides are per-route edits *inside* the interceptor pipeline on live traffic; mock servers are separate replay hosts on their own port. Pick the one whose deployment shape matches your job.

## Steps

### 1. Inspect captured flows

With traffic flowing (via a reverse-proxy or middleware — see the links above), open the **Intercept** rail → **Captured**. Each row is a real request/response pair; click one to see headers, body, timing.

### 2. Add a live override

Switch to **Live overrides**. Add a rule for one route — e.g. force a `503`, add latency, or rewrite a field. Send traffic again: the override applies *in the live path*. Remove it to restore pass-through.

### 3. Contrast with a mock server

Under **Mock servers**, note the Unit 2.1 replay host: it serves a recording on its own port, entirely outside the live path. Same "fake responses" outcome, different mechanism and different deployment shape.

### 4. Tune it

**Settings** exposes capacity (FIFO retention), per-side body caps, TLS/upstream. In embedded mode these come from `BowireInterceptorOptions` (code, [Unit 4](../../unit-4/README.md)); the standalone proxy takes them as CLI flags ([Unit 3](../../unit-3/README.md)).

## Key Takeaways

1. **Four postures: Captured · Live overrides · Mock servers · Settings.**
2. **Live overrides shape *live* traffic in-pipeline; mock servers are separate replay hosts.** Same effect, different shape — pick by job.
3. **The rail is UI; the interception path is a shape concern** — reverse-proxy (Unit 3) or middleware (Unit 4).

## What's Next

That closes the workbench testing arc. Where you go next depends on your course — the CLI/ops story (Unit 3), embedding (Unit 4), or extending (Unit 5). See [Learning Paths](../../LEARNING_PATHS.md).

## Reference

- [Interceptor docs](https://bowire.io/docs/)
