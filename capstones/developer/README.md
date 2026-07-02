# Developer capstone: Ship a Bowire plugin (and use it on a multi-protocol stack)

*Difficulty: Intermediate • Time: ~2 hours • Prerequisites: the [Developer — embed & extend course](../../LEARNING_PATHS.md#3-developer--embed--extend) — Units 0 → 1 → 4 → 5.*

The Developer audience's capstone is **shipping a Bowire plugin**: a NuGet package implementing one of Bowire's extension points — a protocol plugin (most common), a UI extension, a rail contribution, or a Settings module. You build the plugin against a realistic multi-protocol scenario ("Harbor Tour" below); the plugin is the deliverable that proves you can extend Bowire, the scenario is the stage that proves it lights up in a real workbench.

You then weave the plugin into the rest of the toolchain: capture a representative session as a `.bwr` recording, replay it as a mock for downstream consumers, expose the mock via the AI-agent MCP integration, and pin the whole thing as a CI fixture so the plugin doesn't regress on the next refactor.

The optional second half — "wire it into CI as a fixture for other teams" — is in scope for the capstone but not the *point* of it; the point is that the plugin exists, ships through NuGet, and lights up in a real workbench against a real backend.

## Scenario

You're a platform engineer on a team that ships a hybrid REST + gRPC + MQTT stack — "Harbor Tour", a shipping-yard backend that exposes container manifests over REST, live crane telemetry over gRPC server-streaming, and dock-arrival events over MQTT. A new frontend team needs:

- A self-service way to **explore the API surface** without standing up the real backend.
- A **mock fixture** they can run in their own dev environment + CI.
- An **AI-driven testing harness** so QA can write checks against the API in natural language.

You build it with Bowire — and if your real stack speaks a wire Bowire doesn't already bundle, you ship the missing protocol as a plugin.

## What you'll deliver

| Deliverable | Built in |
|---|---|
| **A custom protocol plugin (or UI / rail / settings extension)** — the primary deliverable for this audience | Unit 4.1 / 4.2 |
| A captured `.bwr` recording covering REST + gRPC + MQTT, exercising the plugin's surface where applicable | Unit 2.1 |
| A mock-server setup serving the same recording, with the original OpenAPI / AsyncAPI / `.proto` reflection re-emitted to peer discoverers | Unit 2.2 + Unit 4 (sidecar story) |
| A Claude Desktop / Cursor MCP config pointing at the mock | Unit 3.1 |
| A GitHub Actions workflow running `bowire test` against the recording and bringing up the mock as a service container for downstream integration tests | Unit 5.1 |

## Reference materials

Everything you need is already in the bootcamp:

- The [Harbor Tour sample](#) (TBD — to be added under `capstones/developer/sample/`) — runnable .NET app exposing the three protocols, used to record a representative session against.
- The DocFX site's [Architecture diagram](#) (TBD — to be added under `ARCHITECTURE.md`).
- The [Requirements list](#) (TBD — to be added under `REQUIREMENTS.md`).

## Status

The capstone scaffolding is in place; the sample backend + reference solution + walkthrough text are tracked on the [roadmap](../ROADMAP.md#next-up) under "Capstone reference solution". Until that lands, you can complete the capstone end-to-end against any real or recorded multi-protocol target you have at hand — every step is covered by the existing lessons.

---

**Back to:** [Curriculum](../README.md#curriculum)
