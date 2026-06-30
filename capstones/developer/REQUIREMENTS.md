# Capstone Requirements

> **Difficulty:** Intermediate • **Time:** ~2 hours • **Prerequisites:** [Units 0–5](../README.md#curriculum) complete

The complete list of what the Multi-Protocol API Tour capstone has to deliver. Tick each item off as you go; the [reference solution](solution/README.md) gives one acceptable shape for each.

## Backend

- [ ] **Sample backend running locally on `localhost:5101`.** The `capstones/developer/sample/HarborTour` project in this repo is a working starting point — `dotnet run --project capstones/developer/sample/HarborTour`.
- [ ] **REST surface auto-discovered via OpenAPI.** Bowire pointed at `http://localhost:5101` finds the container-manifest endpoints (`ListContainers`, `GetContainer`, `UpsertContainer`, `GetHealth`).
- [ ] **gRPC surface auto-discovered via Server Reflection.** The `crane.CraneTelemetry` service shows up in the same Bowire sidebar with both `GetLatest` (unary) and `Watch` (server-streaming).

## Recording

- [ ] **A captured `.bwr` recording covering both wires.** At minimum: one `ListContainers`, one `GetContainer`, one `GetLatest`, one `Watch` (the streaming step's `ReceivedMessages` should have 3+ frames).
- [ ] **Source schema attached to the recording.** The `sourceSchema` block on the recording's root carries the OpenAPI doc verbatim — workbench-captured `.bwr` files attach this automatically in v1.7+.
- [ ] **The recording lives under `solution/recording.bwr`** in your fork, ready for the mock + CI steps.

## Mock

- [ ] **`bowire mock --recording solution/recording.bwr --port 7090` brings the mock up.**
- [ ] **The mock serves `/openapi.json` and the gRPC reflection endpoint** so a peer Bowire pointed at the mock URL sees the full original contract, not just the recorded slice.
- [ ] **Every recorded step replays correctly** — `curl` against the REST endpoints returns the captured response; a gRPC client (`grpcurl` or another Bowire workbench) against `Watch` gets the captured frames.

## AI integration

- [ ] **An MCP config snippet pointing Claude Desktop / Cursor at `bowire mcp serve`.** The agent has access to `bowire.discover`, `bowire.invoke`, `bowire.subscribe`, `bowire.recordings.list`, `bowire.mock.start` — enough to drive every other deliverable from the chat.
- [ ] **One worked conversation in the README** showing the agent invoking the mock end-to-end ("list the containers running through the harbour" → mock's response in the chat).

## CI

- [ ] **A GitHub Actions workflow** at `solution/.github/workflows/capstone-ci.yml` that:
  - Installs the .NET SDK + Bowire CLI.
  - Brings up `HarborTour` (the backend).
  - Runs `bowire test --recording solution/recording.bwr --target http://localhost:5101` as a regression check against the live backend.
  - Brings up the mock from the same recording as a service container.
  - (Optional) Runs a downstream integration test job that points at the mock URL.

## Plugin (optional)

- [ ] **A custom protocol plugin** wired into the workbench if your real stack speaks something Bowire doesn't already bundle. The Lesson 4.1 / 4.2 templates apply unchanged. Skip if the bundled REST + gRPC cover your target.

## Documentation

- [ ] **`solution/README.md`** walks through the full scenario end to end: bring up the backend, record, mock, AI, CI. Each step links back to the bootcamp lesson that introduced it.
- [ ] **`solution/ARCHITECTURE.md`** (or this repo's [`ARCHITECTURE.md`](ARCHITECTURE.md)) shows the data flow as a diagram + a paragraph per arrow.

## Acceptance criteria

You've completed the capstone when:

1. A fresh clone + `dotnet run --project capstones/developer/sample/HarborTour` + `bowire --url http://localhost:5101` shows the full sidebar (REST + gRPC).
2. `bowire mock --recording solution/recording.bwr --port 7090` + `bowire --url http://localhost:7090` shows the same sidebar against the mock.
3. The MCP integration: telling the agent "list the containers in the harbour" produces the manifest list.
4. The GitHub Actions workflow runs green.

The reference solution under [`solution/`](solution/README.md) already meets all four criteria; use it as the gold standard or write your own variant alongside.
