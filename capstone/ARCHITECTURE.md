# Capstone Architecture

The Multi-Protocol API Tour capstone weaves three protocol surfaces, two recording/mock layers, one MCP agent and one CI pipeline into a single end-to-end scenario. The diagram below covers all four shapes; each lane below the diagram explains what flows through one of the arrows.

## Diagram

```
       ┌────────────────────────────────────────────────────────────────┐
       │                          DEV LAPTOP                            │
       │                                                                │
       │   ┌─────────────────┐                                          │
       │   │ HarborTour      │ ── REST  ───┐                            │
       │   │ (sample backend)│             │                            │
       │   │ :5101           │ ── gRPC  ───┤                            │
       │   └─────────────────┘             │                            │
       │                                   ▼                            │
       │                    ┌──────────────────────────┐                │
       │                    │ Bowire workbench         │                │
       │                    │ :5080/bowire             │                │
       │                    │   • REST plugin          │                │
       │                    │   • gRPC plugin          │ ── HTTP ───────┼─► Browser
       │                    │   • Recorder             │                │
       │                    └──┬───────────────────────┘                │
       │                       │                                        │
       │                       ▼                                        │
       │              recording.bwr  ──── sourceSchema (OpenAPI verbatim)│
       │              (3 REST + 1 gRPC stream)                          │
       │                                                                │
       │                       │                                        │
       │                       ▼                                        │
       │                ┌────────────────────────┐                      │
       │                │ bowire mock            │ ── /openapi.json ───►│
       │                │ :7090                  │ ── replay REST   ───►│
       │                │   replays recording    │ ── replay gRPC   ───►│
       │                │   serves source schema │                      │
       │                └────────────────────────┘                      │
       │                       ▲                                        │
       │                       │                                        │
       │                       │     ┌──────────────────────────────┐   │
       │                       │     │ bowire mcp serve (stdio)     │   │
       │                       └─────│   bowire.discover            │◄──┤── Claude Desktop /
       │                             │   bowire.invoke              │   │   Cursor (agent host)
       │                             │   bowire.subscribe           │   │
       │                             │   bowire.mock.start          │   │
       │                             └──────────────────────────────┘   │
       │                                                                │
       └────────────────────────────────────────────────────────────────┘

       ┌────────────────────────────────────────────────────────────────┐
       │                    GITHUB ACTIONS                              │
       │                                                                │
       │   bowire-test job:                                              │
       │     dotnet run HarborTour      ──► bowire test --recording …   │
       │                                                                │
       │   integration-tests job (needs: bowire-test):                   │
       │     bowire mock --recording …  ──► downstream test suite ───►  │
       │                                                                │
       └────────────────────────────────────────────────────────────────┘
```

## Arrows

### Backend → workbench

`HarborTour` runs on `localhost:5101` and exposes:

- A REST surface (`/api/containers*`, `/api/health`) with an auto-published OpenAPI document at `/openapi/v1.json`.
- A gRPC surface (`crane.CraneTelemetry/GetLatest`, `crane.CraneTelemetry/Watch`) plus Server Reflection.

`bowire --url http://localhost:5101` boots the workbench host on `:5080/bowire`. Each plugin (REST + gRPC) probes its conventional discovery endpoints against `:5101`, builds a `BowireServiceInfo` tree per protocol, and renders both into the same sidebar.

### Workbench → recording

Hitting the workbench's red Record button captures every successful invocation that follows — request body, metadata, server URL, status, duration, response payload — into a `BowireRecordingStep` array. On Stop, the recorder calls `PUT /api/recordings`, which:

1. Persists the recording into `~/.bowire/recordings.json`.
2. Pulls the verbatim OpenAPI text from the REST discovery cache.
3. Attaches it to the recording as `sourceSchema = { format: "openapi-3.0", content: "<YAML>", sourceUrl: "http://localhost:5101/openapi/v1.json" }`.

You then **Export JSON** the recording out of the manager into a portable `.bwr` file (this capstone's `solution/recording.bwr`).

### Recording → mock

`bowire mock --recording solution/recording.bwr --port 7090` boots the standalone mock host. The host:

1. Validates the recording's `recordingFormatVersion`.
2. Loads the steps into the path-template matcher (REST steps key on `(verb, path-template)`; gRPC steps key on `/<service>/<method>` plus the `responseBinary` for byte-for-byte replay).
3. Mounts the source-schema endpoints — `/openapi.json`, `/openapi.yaml`, `/openapi.yml`, `/swagger.json` for REST; the gRPC reflection service for the captured proto descriptors.
4. Starts listening.

Peer Bowires pointed at the mock URL discover the *full* surface from the re-emitted schema, then invoke against the replayed steps.

### Mock → MCP → Agent

`bowire mcp serve --allow-arbitrary-urls` exposes the workbench's toolset (`bowire.discover`, `bowire.invoke`, `bowire.subscribe`, `bowire.recordings.list`, `bowire.mock.start`, ...) over JSON-RPC on stdin/stdout. Claude Desktop / Cursor spawn the process from their MCP config, list the tools, and from there the agent can:

- Ask `bowire.discover` for the contract at any URL — including the mock's port.
- Issue `bowire.invoke` calls to the mock and inspect the responses.
- Issue `bowire.subscribe` for the streaming side of the gRPC surface (a bounded window of frames).
- List existing recordings, start/stop mocks on demand.

End result: the user types "list the containers in the harbour" and the agent answers from the replayed mock, with no live backend running.

### CI: bowire-test job

The first GitHub Actions job:

1. Boots `HarborTour` against the live backend.
2. Runs `bowire test --recording solution/recording.bwr --target http://localhost:5101`. Each step's captured `(status, response)` becomes an expectation; drift fails the build.
3. Exit 0 → recording still matches reality → tag this commit as "recording is fresh".

This job protects against backend drift: if the team changes the REST contract or a gRPC field, the next CI run catches it instead of leaving the mock silently stale.

### CI: integration-tests job

Gated behind `bowire-test`. Brings the mock up as a service:

```
bowire mock --recording solution/recording.bwr --port 7090
```

The downstream service-under-test (or this capstone's integration tests, if you ship any) points at `http://localhost:7090` instead of a real backend, runs its checks, tears down. No external dependencies, no rate limits, deterministic upstream.

## Design choices

- **One backend process, two wires.** REST and gRPC live in the same `HarborTour` ASP.NET host so the capstone can demonstrate multi-protocol discovery with the simplest possible runtime. A real harbour backend would split them; the demo doesn't need to.
- **Frozen `capturedAt` timestamps in the gRPC service.** `CraneTelemetryService` emits `2026-06-01T08:00:NNZ` instead of `DateTimeOffset.UtcNow` so a re-captured recording has zero diff noise. The capstone's drift story (Lesson 5.1) is about contract drift, not wall-clock drift.
- **In-memory store, seeded.** `ContainerManifestStore` ships with three containers so a fresh `dotnet run` is immediately useful — no DB to provision, no migration to run.
- **No MQTT.** The original Harbor Tour scenario in the [Capstone README](README.md) mentions MQTT for dock arrivals; the sample backend skips it to keep the capstone runnable without a broker. Adding MQTT is a fine extension — see Lesson 4 for the polyglot-plugin path if your message broker's library lives outside .NET.
