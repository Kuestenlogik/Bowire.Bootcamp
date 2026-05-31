# Capstone: Multi-Protocol API Tour

*Difficulty: Intermediate • Time: ~2 hours • Prerequisites: [Unit 1–5](../README.md#curriculum) complete*

The capstone weaves every Bowire mechanic the bootcamp covered into one continuous scenario: discover a multi-protocol API surface, capture a representative session, replay the recording as a stand-in for the real backend, expose the stand-in through the agent integration, and ship the whole thing as a CI fixture.

## Scenario

You're a platform engineer on a team that ships a hybrid REST + gRPC + MQTT stack — "Harbor Tour", a shipping-yard backend that exposes container manifests over REST, live crane telemetry over gRPC server-streaming, and dock-arrival events over MQTT. A new frontend team needs:

- A self-service way to **explore the API surface** without standing up the real backend.
- A **mock fixture** they can run in their own dev environment + CI.
- An **AI-driven testing harness** so QA can write checks against the API in natural language.

You build it with Bowire.

## What you'll deliver

| Deliverable | Built in |
|---|---|
| A captured `.bwr` recording covering REST + gRPC + MQTT | Unit 2.1 |
| A mock-server setup serving the same recording, with the original OpenAPI / AsyncAPI / `.proto` reflection re-emitted to peer discoverers | Unit 2.2 + Unit 4 (sidecar story) |
| A Claude Desktop / Cursor MCP config pointing at the mock | Unit 3.1 |
| A GitHub Actions workflow running `bowire test` against the recording and bringing up the mock as a service container for downstream integration tests | Unit 5.1 |
| (Optional) A custom protocol plugin if your real stack uses a wire Bowire doesn't bundle | Unit 4.1 / 4.2 |

## Reference materials

Everything you need is already in the bootcamp:

- The [Harbor Tour sample](#) (TBD — to be added under `capstone/sample/`) — runnable .NET app exposing the three protocols, used to record a representative session against.
- The DocFX site's [Architecture diagram](#) (TBD — to be added under `ARCHITECTURE.md`).
- The [Requirements list](#) (TBD — to be added under `REQUIREMENTS.md`).

## Status

The capstone scaffolding is in place; the sample backend + reference solution + walkthrough text are tracked on the [roadmap](../ROADMAP.md#next-up) under "Capstone reference solution". Until that lands, you can complete the capstone end-to-end against any real or recorded multi-protocol target you have at hand — every step is covered by the existing lessons.

---

**Back to:** [Curriculum](../README.md#curriculum)
