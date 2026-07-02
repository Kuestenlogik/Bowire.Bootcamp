# User capstone — "Diagnose a flaky API endpoint" runbook

> **Difficulty:** Intermediate · **Time:** ~3–4 hours · **Prerequisites:** the [Workbench & API operator course](../../LEARNING_PATHS.md#1-workbench--api-operator-user) — Units 0 → 1 → 2.

You've worked through the Workbench & API operator course. This capstone proves you can drive the full Workbench loop — Discover, Compose, Recordings, Mocks, Workspaces, and the AI Assistant — against a *real* problem instead of a tutorial.

The deliverable is a Bowire workspace export (`.bww`) + a `RUNBOOK.md` that walks a teammate through your diagnosis. No new code, no plugin authoring — this is "operator at the workbench" end to end.

## Scenario

A backend team ships a small e-commerce stack. Their checkout endpoint started failing on roughly 30 % of requests last week, and nobody can pin down which layer is breaking. They hand you the source URLs and ask you to find the seam.

The stack has three hops:

| Hop | Wire | Source URL | Role |
| --- | --- | --- | --- |
| 1. Checkout | REST (HTTP/1.1, JSON) | `http://localhost:5301` | The flaky surface. `POST /api/checkout` accepts an order and is supposed to return `{ orderId, status: "confirmed" }`. |
| 2. Inventory | gRPC (HTTP/2 cleartext) | `http://localhost:5302` | Sub-call from `checkout`. `inventory.Inventory/Reserve` reserves stock for the order — flakiness shows up here under load. |
| 3. Order status | WebSocket | `ws://localhost:5303/orders/stream` | Sub-stream from the frontend. Pushes `{ orderId, status }` events as the order moves through `received → confirmed → shipped`. |

Your job: find which hop sits between "happy path" and "30 % failure" and produce a written diagnosis a frontend dev (who doesn't run the backend stack) can act on.

## Tools you use

The Workbench rails carry the whole workflow. You don't write any code.

| Rail / surface | Bowire calls it | Why it fits this scenario |
| --- | --- | --- |
| Workspaces rail | `Workspaces` | One workspace pins all three source URLs, so the rest of the capstone reads from a single configured context. |
| Discover rail | `Discover` | Walks the sidebar tree for each source and surfaces the `Checkout`, `Reserve`, and `orders/stream` methods. |
| Compose rail | `Compose` | The ad-hoc Request Builder — pokes each hop in isolation to localise the failure. |
| Recordings rail | `Recordings` | Captures the flaky session so the failure stops being a "happened once" anecdote and becomes a replayable artefact. |
| Mocks rail | `Mocks` | Replays the recording with `--chaos latency:100-500` to expose timing-sensitive failure modes the live host hides. |
| AI Assistant | `AI Assistant` panel (`Bowire:Ai`) | A Claude / Cursor / Ollama agent narrates the dataset and proposes the diagnosis. Optional but recommended — Unit 3.1 set this up. |

> **Where the rails live in the UI.** Open the Workbench (Tool standalone — `http://localhost:5080` — or embedded — `http://localhost:<host-port>/bowire`). The vertical strip on the left lists every rail in this order: Home, Workspaces, Discover, Compose, Recordings, Mocks, Help. The AI Assistant is a panel toggle on the right-hand drawer; you only see it when `Kuestenlogik.Bowire.Ai` is loaded (the standalone Tool ships it by default).

## Step-by-step lab

Estimated wall-clock: 90–120 minutes including the writeup.

### 1. Bring up the three-hop stack

The starter under [`scenario/`](scenario/) is a minimal .NET stack mirroring the layout above — `Checkout` (REST + OpenAPI), `Inventory` (gRPC with Server Reflection), `OrderStatus` (WebSocket). Open three terminals and run:

```bash
dotnet run --project capstones/user/scenario/Checkout      --urls http://localhost:5301
dotnet run --project capstones/user/scenario/Inventory     --urls http://localhost:5302
dotnet run --project capstones/user/scenario/OrderStatus   --urls http://localhost:5303
```

Each prints `Now listening on …`. Leave them running. The flakiness is seeded — exactly **30 %** of `Reserve` calls return `RESOURCE_EXHAUSTED` after a 200–500 ms stall; everything else is deterministic. See [`scenario/README.md`](scenario/README.md) for the chaos config knob.

### 2. Launch Bowire and create a workspace

```bash
bowire --port 5080
```

The browser opens at `http://localhost:5080`. In the rail strip, click **Workspaces** → **New workspace**. Name it `checkout-flake`, pick any colour, save.

> Why a workspace? Every URL, secret, recording, and Compose tab from here on is scoped to the workspace. The export at the end is one `.bww` file that captures all of it; without a workspace each of those items lives in the per-user `~/.bowire/` and can't travel with the writeup.

### 3. Add the three sources

In the URL bar at the top of the workbench, paste `http://localhost:5301` (Checkout) and press Enter. The **Discover** rail populates with the REST surface (Bowire reads `/openapi/v1.json`).

Add the other two with the **+** affordance next to the URL chip:

- `grpc@http://localhost:5302` — Bowire's gRPC plugin probes Server Reflection. Discover surfaces `inventory.Inventory` with one method, `Reserve`.
- `ws://localhost:5303/orders/stream` — the WebSocket plugin attaches; Discover surfaces a single subscription called `orders/stream`.

All three pins live in the workspace now.

### 4. Reproduce the happy path

Open **Compose** in the rail strip. Click **+ Tab**, pick **REST → POST /api/checkout** from the method picker. Fill the body with:

```json
{ "items": [{ "sku": "WIDGET-001", "qty": 1 }], "buyer": "ada" }
```

Click **Invoke**. The response panel shows `200 OK` with `{ "orderId": "...", "status": "confirmed" }`. Note the round-trip duration in the response footer.

Save the Compose tab (Ctrl-S) as `checkout — happy path`. The tab persists in the workspace.

### 5. Reproduce the failure

Re-fire the same Compose tab 10–15 times. Roughly a third of the responses come back as `502 Bad Gateway` with `{ "error": "inventory unavailable" }` and a duration spike to ~400 ms. The happy responses stay near ~30 ms.

This is the operator's "I can see it" moment, but it isn't yet a diagnosis — `checkout` could be flaky on its own, or it could be propagating an upstream failure.

### 6. Start a recording

Click the red **● Record** button at the bottom of the workbench. The label flips to **■ Stop** and pulses.

Now fire the Compose tab another ~20 times. Open a parallel Compose tab against the **gRPC** source (`inventory.Inventory/Reserve` with the same `WIDGET-001`/qty 1 payload) and fire that ~10 times directly. Open a third tab against the **WebSocket** source and subscribe to `orders/stream` for ~30 seconds; let it accumulate frames.

Click **■ Stop**. A toast confirms the recording. Open the **Recordings** rail and rename it `flake-session-1`. Note the **Steps** count — every Invoke landed as a step, and the WebSocket subscribe accumulated `ReceivedMessages` frames.

### 7. Read the recording

In the Recordings rail, expand `flake-session-1`. Each step has the request, response, duration, and (for streaming steps) the frame list. Open a few `Reserve` steps side by side:

- Successful `Reserve` calls finish in 5–10 ms with `{ "reservationId": "...", "ok": true }`.
- Failed `Reserve` calls finish in 200–500 ms with `gRPC status RESOURCE_EXHAUSTED`.
- Every failed `POST /api/checkout` lines up — in time — with a failed `Reserve` step within the same window.

The seam is *upstream of* `checkout`: when `Reserve` fails, `checkout` propagates a 502. The WebSocket stream is a passive bystander — every frame in the capture is well-formed.

### 8. Confirm the diagnosis offline

The live host's failure rate is 30 %. To confirm timing matters (and not e.g. request shape), stop the live hosts and replay the recording as a mock with chaos injected, then dial Bowire at the mock:

```bash
bowire mock --recording ~/.bowire/recordings/flake-session-1.bwr --port 7090 --chaos "latency:100-500,fail-rate:0.30"
```

In a second Bowire window, open the same Compose tabs against `http://localhost:7090`. The same fail rate appears — *without the live `Reserve` running*, just from the recording + chaos overlay. The chaos config is on the mock, not on the upstream — proving the operator can reproduce the failure offline.

> The `--chaos` flag accepts `latency:<min>-<max>,fail-rate:<0..1>` (see `bowire mock --help`). The values above mirror what the live `Inventory` host emits.

### 9. Let the AI Assistant narrate (optional but recommended)

Open the AI Assistant drawer (right-hand panel, brain-icon toggle). The Assistant has access to the workspace's recordings, methods, and Compose tabs via the in-process MCP adapter (Unit 3.1 Path B). Ask:

> Look at the steps in `flake-session-1`. For every `POST /api/checkout` step that returned 502, find the `inventory.Inventory/Reserve` step in the same wall-clock window and summarise the correlation in one paragraph.

The Assistant walks the recording and produces a paragraph you can paste into the runbook. If you wired Claude Desktop via `bowire mcp serve --bind stdio` (Unit 3.1 Path A), the same prompt works there — the tools are the same.

### 10. Write `RUNBOOK.md`

Drop a `RUNBOOK.md` next to your `.bww` (in your own fork). Five sections:

1. **What's broken** — one paragraph, no jargon. ("Checkout fails ~30 % of the time.")
2. **What I checked** — bullets, in order. Discover surfaces, Compose tabs, the recording, the mock replay.
3. **Where the seam sits** — one paragraph + one screenshot of two side-by-side recording steps showing the time alignment.
4. **How to reproduce offline** — the `bowire mock --chaos …` command line, exact recording path.
5. **What to ask the backend team** — three concrete questions, each anchored to a recording step id.

Cross-link every step back to the unit that introduced it (Unit 1.1 Discover, Unit 2.1 Recording, Unit 2.2 `bowire mock`, Unit 3.1 AI Assistant).

### 11. Export the workspace

In the **Workspaces** rail, right-click the `checkout-flake` workspace → **Export**. Bowire writes a `bowire-workspace` v2 envelope as JSON. Save it with the `.bww` extension. The envelope's `data` block carries every Compose tab, every recording reference, every URL, and the workspace's identity (name + colour). See `WorkspaceCommand.cs` (`CanonicalFormatVersion = 2`, top-level keys `format`, `version`, `exportedAt`, `workspace`, `data`).

Round-trip check — on a fresh laptop or in CI:

```bash
bowire workspace import checkout-flake.bww --to ./imported
bowire workspace export ./imported checkout-flake.roundtrip.bww
```

The roundtripped file should diff cleanly modulo the `exportedAt` timestamp.

### 12. Submit

In your fork, the capstone is two artefacts and one PR:

- `capstones/user/solution/checkout-flake.bww` (your exported workspace).
- `capstones/user/solution/RUNBOOK.md` (your diagnosis).

A reference of both lives under [`solution/`](solution/) — use it as the gold standard, not as a copy-paste source.

## Deliverable artefacts

| Path | Format | Source of truth |
| --- | --- | --- |
| `solution/checkout-flake.bww` | `bowire-workspace` v2 JSON envelope (`{ format: "bowire-workspace", version: 2, workspace, data }`). Verified against `src/Kuestenlogik.Bowire.Tool/Cli/WorkspaceCommand.cs::CanonicalFormatVersion`. | Workbench → Workspaces rail → Export. CLI: `bowire workspace export <dir> <out.bww>`. |
| `solution/RUNBOOK.md` | Plain markdown, five sections (above). | Hand-written from the recording. |

The starter under [`scenario/`](scenario/) is the three-hop backend the capstone is built around — `Checkout` (REST + OpenAPI), `Inventory` (gRPC + Reflection), `OrderStatus` (WebSocket). The Tool standalone (no embedded host) is the daily driver; the embedded shape is out of scope for the User audience.

## Acceptance criteria

You've completed the capstone when:

1. **One workspace, three sources.** Opening `checkout-flake.bww` in a fresh Bowire shows all three URLs in the URL list and the discovered surfaces populating Discover.
2. **Recording reproduces the failure.** `flake-session-1` carries enough `POST /api/checkout` + `inventory.Inventory/Reserve` steps that the correlation in the runbook is visible to a reader who doesn't know the answer.
3. **Mock-plus-chaos reproduces offline.** `bowire mock --recording … --chaos "latency:100-500,fail-rate:0.30" --port 7090` brings up a host a peer Bowire can dial — without the live `Inventory` running — and a fresh `POST /api/checkout` against the mock fails at the same ~30 % rate.
4. **Runbook is actionable.** A frontend dev reading `RUNBOOK.md` cold can find the seam, point at the exact `Reserve` step that confirms it, and rerun the mock-plus-chaos repro themselves from the command line in the runbook.
5. **The diagnosis is upstream, not downstream.** The runbook concludes the seam sits in `Inventory.Reserve`, not in `Checkout` (which is just propagating a 5xx), and not in the WebSocket stream (which is well-formed throughout).

## Out of scope

- Fixing `Inventory`. Diagnosis is the deliverable; the backend team owns the fix.
- Plugin authoring. If your real stack speaks a wire Bowire doesn't bundle, the Developer capstone covers that (`capstones/developer/`).
- Production deployment. The Administrator capstone (`capstones/administrator/`) covers ops.

---

**Back to:** [Curriculum](../../units-overview.md) · [Learning paths](../../LEARNING_PATHS.md)
