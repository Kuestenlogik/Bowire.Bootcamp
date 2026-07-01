# Lesson 4.5: The Intercept rail — one rail, four postures

> **Difficulty:** Intermediate | **Duration:** 20 min | **Prerequisites:** [Lesson 4.3](../lesson-3/README.md) (`UseBowireInterceptor()` mounted) or the standalone `bowire proxy` / `bowire interceptor` CLI available, `curl` (or any HTTP client)

## Overview

Lesson 4.3 turned the interceptor on and pointed you at the Intercepted rail. v2.2 folded the old Mocks + Traffic rails into a single **Intercept** rail with four sub-tabs, and the day-to-day flow for a workbench operator now runs through those four tabs — not four separate places in the sidebar.

The tabs, in fixed order:

| Sub-tab | Posture | What lives here |
|---|---|---|
| **Captured** | Passive observation | Every request that flowed through the middleware / proxy, newest-first. Right-click any row → *Save as recording*. |
| **Live overrides** | Selective response substitution | Per-route rules that short-circuit the interceptor pipeline and serve a captured / hand-authored response in place of the upstream. |
| **Mock servers** | Standalone replay | The former Mocks rail — mock hosts spun up from `.bwr` recordings via `bowire mock` (or the workbench's *Run as mock*). Works even when the interceptor isn't running. |
| **Settings** | Config | Interceptor deployment posture (Embedded vs Standalone) and the sidecar-API URL for the standalone case. |

Same rail, three different jobs: **observe**, **intervene**, **replay**. This lesson walks each posture end-to-end so you know when to reach for which tab.

## Steps

### 1. Wire the sample host + drive some traffic

Reuse the Sample.Embedded host from Lesson 4.3 (or your own host with `app.UseBowireInterceptor()` mounted). Start it:

```bash
dotnet run --project samples/Kuestenlogik.Bowire.Sample.Embedded --urls http://localhost:5181
```

Then in another terminal:

```bash
curl http://localhost:5181/api/health
curl http://localhost:5181/api/users
curl -X POST http://localhost:5181/api/users \
  -H "Content-Type: application/json" \
  -d '{"name":"Ada Lovelace","role":"Pioneer"}'
```

Open `http://localhost:5181/bowire` and click the **Intercept** rail. The tab strip along the top shows `Captured · Live overrides · Mock servers · Settings`, each with a live count meta-chip.

### 2. Captured — the passive posture

The **Captured** sub-tab is the default. Each row is a `(method, status, path)` tuple; click one to open the detail pane (request headers + body, response headers + body, latency).

Two things to try:

1. **Right-click a captured row → *Save as recording*.** The flow is written into the active workspace's Recordings list — you can now replay, fuzz, or convert it to a test-collection without leaving the workbench.
2. **Streaming responses** get a `↻` badge in the row. The interceptor stores an empty body (buffering an open SSE / WebSocket / gRPC stream would deadlock the caller) but everything else — status, headers, latency — is captured normally.

### 3. Live overrides — the intervening posture

The **Live overrides** sub-tab lists rules that the interceptor pipeline consults *before* forwarding a request upstream. When a rule matches, the upstream call is short-circuited and the rule's response is served instead. Matching rows in Captured get an `M` badge so the detail pane still shows what the operator saw.

Seed an override from a real flow:

1. Back in Captured, click `GET /api/users`.
2. In the detail pane's action row, click **Override this route**.
3. The Live overrides tab activates with a new rule pre-populated from the captured response. Edit the body if you like.
4. Fire the request again from `curl` — the response is now served from the override, not the real handler. The captured flow gets the `M` badge and the detail pane's `Source` meta line shows *override rule*.

Turn it off by deleting the rule (hover the row, click the trash icon). The next request goes back to the real handler.

> **Kill-switch.** `UseBowireInterceptor(options => options.MocksEnabled = false)` disables the override short-circuit path entirely for production hosts that loaded the package for read-only observability. Capture still works; overrides silently fall through.

### 4. Mock servers — the replay posture

The **Mock servers** sub-tab manages *standalone* mock hosts spun up from a `.bwr` recording. Unlike overrides, a mock server binds its own port; clients point at that port instead of the real backend. This is the CI-fixture / frontend-dev-server / demo-stack posture — no live backend needed.

Walk it end-to-end:

1. From the Recordings rail, right-click any recording → **Run as mock**. The workbench spins up a mock host on the next free port.
2. Switch to the Intercept rail's Mock servers tab. Your new host is listed with its port + recording name.
3. Fire a request at the mock's port:
   ```bash
   curl http://localhost:6000/api/users
   ```
4. The mock replays the captured response, byte-for-byte.
5. Hover the row and click the trash icon to stop the host.

If the Mock package isn't referenced by the current host (embedded scenarios that only need capture), the sub-tab renders a `Mock package not loaded` empty state pointing at `Kuestenlogik.Bowire.Mock`. The standalone Bowire CLI ships with it included, so this only affects hand-composed embedded hosts.

### 5. Settings — the config posture

The **Settings** sub-tab adapts to how Bowire is running:

- **Embedded mode** (`MapBowire()` in your host): read-only view showing the deployment shape + the `/api/intercepted/*` endpoint base. Middleware config lives in `BowireInterceptorOptions` (see Lesson 4.3) — the rail can't edit code.
- **Standalone mode** (`bowire` CLI or `bowire proxy` / `bowire interceptor`): editable **External sidecar URL** so the workbench in one terminal can talk to a proxy / interceptor running in another. Default is loopback (`http://127.0.0.1:8889`); set to a LAN address to point the rail at a shared sidecar.

## Exercise — the three-posture round trip

1. **Capture.** Start the embedded host, fire a `POST /api/users` with a specific `role`. Verify the flow lands in Captured with the exact request body you sent.
2. **Override.** Seed an override from that flow, but change the response `role` to `"Guest"`. Fire the `POST` again. Confirm the response you get back carries the mutated `role`, and that the flow row shows the `M` badge.
3. **Replay.** Right-click the *original* captured flow → *Save as recording*. Then in the Recordings rail, run it as a mock server. Point `curl` at the mock's port and confirm the response matches the *original* body (not the override — you saved the recording before the override was seeded).

Three different tabs, three different postures, one continuous story: the same flow can be observed, mutated in place, and replayed from a standalone host without leaving the rail.

## Key Takeaways

1. **One rail, four locked postures.** Captured (observe) · Live overrides (intervene) · Mock servers (replay standalone) · Settings (config). Tab order is fixed; every operator sees the same shape.
2. **Overrides ≠ mock servers.** Overrides are per-route substitutions *inside* the interceptor pipeline; mock servers are standalone replay hosts on their own port. Pick the one whose deployment shape matches your job.
3. **The Mock-servers tab works regardless of interceptor state.** The other three tabs surface an activation empty state ("No interceptor running") when the middleware isn't wired and no standalone proxy is up. Mock servers are decoupled from that gate.
4. **Right-click Captured rows to persist.** *Save as recording* is the one-click path from a passively-observed flow to a replayable `.bwr` artifact.

## Knowledge Assessment

1. **Which sub-tab do you reach for?** You want to substitute one specific `GET /api/users/42` response with a hand-authored 500-error payload for a few minutes while you test how a downstream client handles it. Overrides or Mock servers?
   *Answer:* **Live overrides.** Overrides substitute a *single route* inside the running interceptor pipeline, are trivial to turn on and off, and don't require a separate host. A mock server would replay a whole `.bwr` recording on its own port — too heavyweight for a single-route toggle.

2. **The activation gate.** You mounted `AddBowire()` + `MapBowire()` but forgot `app.UseBowireInterceptor()`. You open the Intercept rail. Which sub-tabs work, which show an activation empty state?
   *Answer:* **Mock servers works** (it doesn't depend on the interceptor). **Captured / Live overrides / Settings all show the "No interceptor running" empty state** with a hint to add `app.UseBowireInterceptor()`. In standalone mode the empty state also offers a *Start Reverse-Proxy now* button that opens the reverse-proxy modal directly.

3. **The M badge.** You seeded a Live override for `GET /api/users`. You fire the request via `curl`. The captured flow appears in Captured with the `M` badge and the detail pane's `Source` meta says *override rule*. What was actually served — the real handler's response, or the override's?
   *Answer:* The **override's** response. The interceptor pipeline consults Live overrides *before* forwarding upstream; when a rule matches, the upstream is short-circuited entirely. The `M` badge marks the capture as mock-served.

4. **Sidecar URL editing.** You run `bowire proxy --api-port 9999` on host `dev-box.local`. In your workbench on your laptop, which sub-tab do you use to point the rail at that sidecar?
   *Answer:* **Settings.** In standalone mode it exposes an editable **External sidecar URL** field. Set it to `http://dev-box.local:9999` and the Captured / Live overrides tabs start pulling flows from the remote sidecar. The default is loopback (`http://127.0.0.1:8889`), which is why the rail defaults to the same-machine story.

## What's Next

You've walked the four postures of a single rail. **Lesson 4.6** covers the second v2.2 extension seam: **plugin lifecycle without a process restart** — Load / Unload / Restart / Reset-storage against a live registry.

**Continue:** → [Lesson 4.6: Plugin lifecycle — Restart, Unload, Load](../lesson-6/README.md)

## Reference

- `src/Kuestenlogik.Bowire.Interceptor/wwwroot/js/intercept-view.js` — the rail's sub-tab dispatcher; the source of truth for the four-posture model.
- `src/Kuestenlogik.Bowire.Interceptor/BowireInterceptorMiddleware.cs` — the pipeline that Captured observes and Live overrides intervene inside.
- `src/Kuestenlogik.Bowire.Interceptor/BowireInterceptRailContribution.cs` — how the rail contributes into the workbench's sidebar strip.
- `src/Kuestenlogik.Bowire.Mock/` — the standalone mock-host implementation the Mock-servers sub-tab drives.
- Main-Bowire issues [#334](https://github.com/Kuestenlogik/Bowire/issues/334) (rail-IA refactor) and [#336](https://github.com/Kuestenlogik/Bowire/issues/336) (activation gate).
