# Capstone Reference Solution

One acceptable shape for the Multi-Protocol API Tour deliverables ([REQUIREMENTS.md](../REQUIREMENTS.md)).

This directory contains:

| File | What it is |
|------|------------|
| `recording.bwr` | A captured session against `HarborTour` covering 3 REST methods + 1 gRPC server-streaming method, with the OpenAPI document attached as `sourceSchema` |
| `mcp-config.json` | Snippet for Claude Desktop / Cursor that wires `bowire mcp serve` into the agent |
| `.github/workflows/capstone-ci.yml` | GitHub Actions workflow running the live regression + the mock-based integration tests |

Drop this folder into a fresh repo (or fork this one) and you have a working capstone end-to-end without writing a line of code beyond the lessons.

## Full walkthrough

### 1. Bring up the backend

```bash
dotnet run --project ../sample/HarborTour
# → Listening on http://localhost:5101
```

### 2. Discover both protocols in one Bowire workbench

```bash
bowire --url http://localhost:5101
```

The sidebar shows:

```
🌐 HarborTour (REST)
   ├─ ListContainers
   ├─ GetContainer
   ├─ UpsertContainer
   └─ GetHealth
🟢 crane.CraneTelemetry (gRPC)
   ├─ GetLatest               (unary)
   └─ Watch                   (server-streaming)
```

### 3. Replay the recording as a mock

```bash
bowire mock --recording recording.bwr --port 7090
```

The mock listens on `localhost:7090`. Test it:

```bash
# REST
curl http://localhost:7090/api/health
# → {"status":"up","service":"HarborTour"}

curl http://localhost:7090/api/containers
# → [{"id":"HLCU7654321",…},{"id":"MAEU1234567",…},{"id":"MSCU1112223",…}]

# Schema sidecar (peer-discovery)
curl http://localhost:7090/openapi.json | jq .info.title
# → "HarborTour"
```

### 4. Peer-discover the full surface through the mock

```bash
bowire --url http://localhost:7090
```

Both the REST and gRPC sidebars render — the OpenAPI sidecar covers REST, and the embedded protobuf descriptors (carried per gRPC step) re-emit Server Reflection on the mock so the gRPC plugin discovers the service too.

### 5. Wire the agent

Open Claude Desktop's `claude_desktop_config.json` (or Cursor's `~/.cursor/mcp.json`) and merge the `mcpServers` entry from `mcp-config.json`:

```bash
cat mcp-config.json
```

Restart the agent. In a fresh chat:

> List the containers running through the harbour at `http://localhost:7090`.

The agent calls `bowire.discover` → `bowire.invoke` against the mock and pastes the manifest list. Try:

> Stream live telemetry from `crane-01` for 3 seconds.

It calls `bowire.subscribe` against the mock's gRPC reflection-discovered Watch method, samples a bounded window of frames, and summarises them.

### 6. CI

Push to GitHub and the `.github/workflows/capstone-ci.yml` workflow runs. Two jobs:

1. **`bowire-test`** — boots `HarborTour`, asserts each recording step still matches the live backend. Catches contract drift.
2. **`integration-tests`** — gated behind the first, brings the mock up as a service, points whatever downstream tests you have at it.

The workflow file is annotated with the meaning of each step; copy it into your own repo and adjust as needed.

## Customising

- **Add MQTT** — drop in the [MQTT plugin](https://bowire.io/docs/protocols/mqtt.html) on the host side, register a Mosquitto broker as a sample target, capture additional steps. The bootcamp's Unit 4.2 covers the polyglot path if you'd rather host the MQTT side in a sidecar.
- **Add an authentication layer** — the [OIDC auth provider](https://bowire.io/docs/architecture/sidecar-plugins.html) plugs in. Adjust `--auth-provider oidc` on the workbench launch, the agent's MCP config inherits the headers.
- **Replace the in-memory store** — point `ContainerManifestStore` at PostgreSQL / Redis / your DB of choice. The recording stays valid as long as the schemas don't change.

## Status

The reference solution is **scaffolded** — the recording, MCP config, and CI workflow shells are in place. The bundled `recording.bwr` covers the unary REST methods + the gRPC `GetLatest` step; the streaming `Watch` step ships as a placeholder you fill in by capturing a fresh session against your local `HarborTour`. See the [bootcamp ROADMAP](../../ROADMAP.md#next-up) for the full-coverage plan.
