# Runbook — Checkout endpoint flakiness

*Reference solution for the [User capstone](../README.md). Walks the diagnosis the way a frontend dev needs to read it: what's broken, what was checked, where the seam sits, how to reproduce offline, what to ask the backend team.*

---

## What's broken

Roughly 30 % of `POST /api/checkout` calls return `502 Bad Gateway` with `{ "error": "inventory unavailable" }` instead of the expected `200 OK { orderId, status: "confirmed" }`. The remaining 70 % succeed in ~30 ms. The failure rate is stable across SKUs and buyers; nothing about the request shape correlates with the failure.

## What I checked

- **Discover (Unit 1.1).** Pointed Bowire at the three sources from the [scenario stack](../scenario/README.md): `http://localhost:5301` (Checkout REST), `grpc@http://localhost:5302` (Inventory gRPC), `ws://localhost:5303/orders/stream` (OrderStatus WebSocket). All three surfaces populated cleanly — Checkout via OpenAPI at `/openapi/v1.json`, Inventory via Server Reflection, OrderStatus via the WebSocket plugin.
- **Compose (Unit 1.1).** Saved three Compose tabs in the `checkout-flake` workspace:
  - `checkout — happy path` — `POST /api/checkout` with `WIDGET-001 × 1`.
  - `reserve — direct probe` — `inventory.Inventory/Reserve` with the same SKU.
  - `order status — subscribe` — the WebSocket subscription.
- **Live reproduction.** Fired the `checkout — happy path` tab ~30 times in succession. ~10 calls came back as 502, with a duration spike from ~30 ms (success) to 200–500 ms (failure). The 502 body always carried `{ "error": "inventory unavailable", "orderId": "..." }`.
- **Direct probe of Inventory (Unit 1.2 mental model).** Fired the `reserve — direct probe` tab ~10 times. Same shape — ~3 of the 10 came back as `gRPC RESOURCE_EXHAUSTED` with the same 200–500 ms latency, even though the checkout layer was bypassed entirely. **That's the tell.**
- **Recording (Unit 2.1).** Captured `flake-session-1` with ~20 checkout calls, ~10 direct Reserve probes, and 30 s of `orders/stream` frames. The recording carries every step's request, response, duration, and (for the WebSocket) frame list.
- **Offline reproduction with chaos (Unit 2.2).** Replayed `flake-session-1` as a mock with `--chaos "latency:100-500,fail-rate:0.30"`; dialled a second Bowire at the mock; the same ~30 % checkout-fail rate appeared without the live `Inventory` host running. (Command line in *Reproduce offline* below.)
- **AI Assistant (Unit 3.1).** Asked the in-process AI Assistant ("for every 502 checkout step, find the matching Reserve step in the same wall-clock window"). The Assistant walked the recording and produced the time-aligned table at the bottom of this runbook.

## Where the seam sits

**`inventory.Inventory/Reserve` returns `RESOURCE_EXHAUSTED` on roughly 30 % of calls after a 200–500 ms stall. `Checkout` propagates the failure as a 502.** The WebSocket stream is well-formed throughout (60 frames in 30 s, every frame valid JSON with the documented shape).

The diagnostic evidence is in `flake-session-1`:

| Checkout step | Outcome | Reserve step | Outcome | Δt |
| --- | --- | --- | --- | --- |
| `step-003` (POST /api/checkout) | 502, 412 ms | `step-002` (Reserve) | RESOURCE_EXHAUSTED, 408 ms | matched |
| `step-007` (POST /api/checkout) | 502, 287 ms | `step-006` (Reserve) | RESOURCE_EXHAUSTED, 283 ms | matched |
| `step-011` (POST /api/checkout) | 502, 351 ms | `step-010` (Reserve) | RESOURCE_EXHAUSTED, 347 ms | matched |
| `step-014` (POST /api/checkout) | 200, 28 ms | `step-013` (Reserve) | OK, 7 ms | matched |
| `step-018` (POST /api/checkout) | 200, 31 ms | `step-017` (Reserve) | OK, 9 ms | matched |

(Every 502 checkout step has a Reserve step within ~5 ms of it, and the Reserve step's status code drives the checkout's outcome. Step ids are from the reference `flake-session-1` recording; your captured recording will have its own ids in the same shape.)

**Why this isn't a `Checkout` problem:** the *direct* probes against `inventory.Inventory/Reserve` (Compose tab `reserve — direct probe`, recorded as steps `step-021` through `step-030`) fail at the same ~30 % rate with no `Checkout` involvement at all. Take `Checkout` out of the path and the failure rate is unchanged.

**Why this isn't an OrderStatus problem:** every frame the WebSocket emitted in the recording is well-formed JSON with the documented `{ orderId, status, ts }` shape. No frame is dropped, no frame is malformed. The WebSocket is a passive bystander.

## Reproduce offline

The whole failure mode can be reproduced from the recording, with no live hosts running:

```bash
# Stop the three scenario processes first.

bowire mock \
  --recording ~/.bowire/recordings/flake-session-1.bwr \
  --port 7090 \
  --chaos "latency:100-500,fail-rate:0.30"

# In another terminal, dial the mock:
bowire --url http://localhost:7090 --port 5081
```

The mock serves every recorded step as-is and overlays the chaos profile on top. `POST /api/checkout` against the mock fails at the same ~30 % rate, with the same 200–500 ms latency on the failure path, because the chaos overlay reproduces the upstream behaviour exactly. Frontend devs can develop against `http://localhost:7090` without any of the three live services running.

## What to ask the backend team

Three questions, each pointing at a specific recording step:

1. **`flake-session-1::step-002` (Reserve) returned `RESOURCE_EXHAUSTED` after a 408 ms stall.** Is the inventory store hitting a connection-pool ceiling under load? The successful Reserve calls in this recording (e.g. `step-013`, `step-017`) come back in 5–10 ms; the failed ones consistently stall 200–500 ms before responding. That stall pattern looks like queue-wait, not a synchronous DB lookup.
2. **`flake-session-1::step-021` through `step-030` are direct Reserve probes** (no Checkout involvement). The fail rate there is the same ~30 % we see at the Checkout layer. Is there any per-caller throttling, or is the failure global to the Inventory service?
3. **`Checkout` propagates every Reserve failure as a 502 with `inventory unavailable`.** Is that the contract you want? A 503 (Service Unavailable) with a `Retry-After` header would tell upstream clients (and frontend retry logic) to back off; today's 502 reads as "Checkout itself is broken" and sends every alert to the wrong team.

---

## Cross-links to bootcamp units

- **Discover rail** — [Unit 1.1 first call](../../../units/unit-1-cli/lesson-1/README.md) (or the embedded variant — same UI).
- **Compose rail** — same lesson, the request-builder + the saved tabs.
- **Recordings rail + `.bwr`** — [Unit 2.1 Record & Replay](../../../units/unit-2/lesson-1/README.md).
- **`bowire mock --chaos`** — [Unit 2.2 schema export + mock](../../../units/unit-2/lesson-2/README.md) and `bowire mock --help`.
- **AI Assistant + MCP-driven narration** — [Unit 3.1 AI-Agent integration](../../../units/unit-3/lesson-1/README.md).
- **Workspaces rail + `.bww` export** — covered transversally; the `bowire workspace export` CLI mirrors the UI export (see `src/Kuestenlogik.Bowire.Tool/Cli/WorkspaceCommand.cs` in main Bowire).

---

**Back to:** [User capstone](../README.md) · [Curriculum](../../../units/README.md)
