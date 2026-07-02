# Developer capstone: Ship a Bowire plugin (and use it on a multi-protocol stack)

*Difficulty: Intermediate • Time: ~2 hours • Prerequisites: the [Developer — embed & extend course](../../LEARNING_PATHS.md#3-developer--embed--extend) — Units 0 → 1 → 4 → 5.*

The Developer audience's capstone is **shipping a Bowire plugin**: a NuGet package implementing one of Bowire's extension points — a protocol plugin (most common), a UI extension, a rail contribution, or a Settings module. You build the plugin against a realistic multi-protocol scenario ("Harbor Tour" below); the plugin is the deliverable that proves you can extend Bowire, the scenario is the stage that proves it lights up in a real workbench.

You then weave the plugin into the rest of the toolchain: capture a representative session as a `.bwr` recording, replay it as a mock for downstream consumers, expose the mock via the AI-agent MCP integration, and pin the whole thing as a CI fixture so the plugin doesn't regress on the next refactor.

The optional second half — "wire it into CI as a fixture for other teams" — is in scope for the capstone but not the *point* of it; the point is that the plugin exists, ships through NuGet, and lights up in a real workbench against a real backend.

> **On the CLI.** This capstone reaches for CLI verbs — `bowire plugin install`, `bowire mock`, `bowire test`, `bowire mcp serve`. They're all introduced in [Unit 3: CLI & operations](../../units/unit-3/README.md); the Developer course cross-links Unit 3 as the plugin author's ship-and-verify toolchain, so treat it as a capability reference if you skipped it.

## Scenario

You're a platform engineer on a team that ships a hybrid REST + gRPC + MQTT stack — "Harbor Tour", a shipping-yard backend that exposes container manifests over REST, live crane telemetry over gRPC server-streaming, and dock-arrival events over MQTT. A new frontend team needs:

- A self-service way to **explore the API surface** without standing up the real backend.
- A **mock fixture** they can run in their own dev environment + CI.
- An **AI-driven testing harness** so QA can write checks against the API in natural language.

You build it with Bowire — and if your real stack speaks a wire Bowire doesn't already bundle, you ship the missing protocol as a plugin.

## What you'll deliver

| Deliverable | Built in |
|---|---|
| **A custom protocol plugin (or UI / rail / settings extension)** — the primary deliverable for this audience | Unit 5.1 / 5.2 / 5.3 |
| A captured `.bwr` recording covering REST + gRPC + MQTT, exercising the plugin's surface where applicable | Unit 2.1 |
| A mock-server setup serving the same recording, with the original OpenAPI / AsyncAPI / `.proto` reflection re-emitted to peer discoverers | Unit 2.2 (UI) · `bowire mock` (Unit 3.2) |
| A Claude Desktop / Cursor MCP config pointing at the mock | Unit 3.3 |
| A GitHub Actions workflow running `bowire test` against the recording and bringing up the mock as a service container for downstream integration tests | Unit 3.2 |

## Reference materials

Everything you need ships in this capstone directory:

- The [**Harbor Tour sample**](sample/HarborTour/) — a runnable .NET app in the Harbor Control Center domain: container manifests over REST, live crane telemetry over gRPC server-streaming, dock-arrival events over MQTT. Record a representative session against it.
- The [**Architecture**](ARCHITECTURE.md) — the end-to-end data-flow (discover → record → mock → MCP → CI).
- The [**Requirements + grading checklist**](REQUIREMENTS.md).
- The [**reference solution**](solution/README.md) — a captured `recording.bwr`, an `mcp-config.json` snippet, and a `capstone-ci.yml` GitHub Actions workflow.

The domain mirrors the Harbor demo in [`Bowire.Samples`](https://github.com/Kuestenlogik/Bowire.Samples) (`Ship` / `Dock` / `Crane` / `Container` / `PortCall`), so the plugin you ship extends a model you already met in the units.

---

**Back to:** [All units](../../units-overview.md) · [Learning paths](../../LEARNING_PATHS.md)
