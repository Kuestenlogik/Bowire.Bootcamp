# Lesson 3.3: Regression Coverage — what have I actually tested?

> **Difficulty:** Beginner | **Duration:** 15 min | **Prerequisites:** [Unit 1](../../unit-1/README.md), a backend with several discovered methods (the Unit 1 REST + gRPC samples together give ~10)

## Overview

A running backend has, say, 40 methods. You've been driving Bowire for a week. Which 40 have you actually touched? Which have you touched *recently*? Which failed the last time you touched them? Which have you *never* touched?

That's the Regression Coverage surface. v2.2 quietly instruments every runner in Bowire — the Discover-tab invoke button, the Compose runner, the Benchmark runner, Recording replay, and the Flow runner (Lesson 3.2) — to log a per-method run-history entry every time it fires. The workbench then aggregates that history into four states per method:

| State | Meaning | Sidebar glyph |
|---|---|---|
| **recent** | Last successful run within the last 7 days | ● |
| **stale** | Last successful run 8–30 days ago | ◐ |
| **failing** | Most recent run was `fail` or `error` | ✕ |
| **uncovered** | Never run, or last run > 30 days ago | ○ |

Every discovered method in the sidebar gets a chip. The Settings → Data pane has a full run-history view with filters + a summary card. Colour-blind-friendly glyphs; hover for the numbers.

## Concepts

The run-history is a per-workspace ring buffer capped at **500 entries** by default (`bowire_run_history_cap` in localStorage; adjustable between 50 and 5000). FIFO eviction: when the buffer fills, the oldest entry is dropped when a new one lands.

Each entry looks like:

```json
{
  "runId": "r-abc123",
  "methodId": "GreetingService::SayHello",
  "service": "GreetingService",
  "method": "SayHello",
  "source": "discover",
  "startedAt": 1719840000000,
  "durationMs": 12,
  "outcome": "ok"
}
```

`source` is one of `discover` (workbench invoke tab), `compose` (Compose runner), `benchmark` (Benchmark runner), `recording-replay`, or `flow`. `outcome` is `ok`, `fail`, or `error`; anything else gets bucketed as `error` so a miscategorised run surfaces as a red chip rather than silently inflating the pass-rate.

The state per `(service, method)` is computed *lazily on read* — there's no derived state on disk to keep in sync, no migration when buckets churn, and no per-run write amplification. Cheap.

## Steps

### 1. Start the Unit 1 samples

Two servers, ~10 methods between them:

```bash
# Terminal A — REST
cd ../../unit-1-samples/HelloApi
dotnet run                                    # → http://localhost:5001

# Terminal B — gRPC
cd ../../unit-1-samples/HelloGrpc
dotnet run                                    # → http://localhost:5002
```

Open the workbench pointed at both (`bowire --url http://localhost:5001 --url http://localhost:5002`, or the equivalent embedded mount). The Discover sidebar shows all services + methods.

### 2. Observe the initial state — every method is uncovered

Every method row in the sidebar has a small circle glyph — `○` — to the right of its name. That's the *uncovered* chip. Hover over any of them: the tooltip reads *No runs recorded — Invoke the method to start tracking*.

### 3. Invoke a method — watch the chip flip

Click into `SayHello`, fill in a body, click **Invoke**. The response comes back green.

Look at the sidebar entry for `SayHello`. The chip is now `●` — *recent*. Hover: *Last run: `<timestamp>` (ok) / Runs: 1 in 7d / 1 in 30d / 1 total / 30d pass-rate: 100% / Sources: discover*.

That's the run history at work. Every invoke — from any source — leaves a trail.

### 4. Fire a failing call — watch the chip go red

Deliberately break something: change the endpoint URL to a wrong port, or call a method with a malformed body that the server will reject. Invoke.

The chip is now `✕` — *failing*. The tooltip carries the (truncated to 240 chars) error message and marks the run as `fail` or `error`. The chip stays red until the *next* successful run bumps it back to `recent`.

### 5. Open the full run-history view

Gear icon → **Settings → Data**. Scroll to **Run history** (below the Data Management row). You get:

- **Summary card** — "N of M methods exercised in the last 7 days" + counts for stale / failing / uncovered.
- **Filter bar** — Source (Discover / Compose / Benchmark / Recording replay / Flow / All), Outcome (Pass / Fail / Error / All), and a text search over service/method/error text.
- **Table** — up to 50 rows, newest-first, with `When`, `Method`, `Source`, `Outcome`, `Duration`. Click a row → the workbench opens that method's tab.
- **Clear history** button — wipes the workspace's run log.

The table is capped at 50 visible rows. The buffer holds up to 500 entries total; the filter meta line tells you `N of M filtered runs shown (K total)` so you know when you're looking at a tail.

### 6. Cross-source aggregation

Fire the same method from three different runners:

- Discover tab: click **Invoke**.
- Compose: build a one-step flow that hits it, run it.
- Recording replay: record the call in a `.bwr`, replay it.

Reload the Settings → Data → Run history view. The `Sources` field in the method's coverage tooltip now lists all three (`discover, compose, recording-replay`). Coverage doesn't care *how* you exercised the method — every runner contributes.

## Exercise — get to 80% recent

Your goal: turn every method's chip green (`●`, recent).

1. Note the summary card's *uncovered* count.
2. Walk the sidebar service-by-service, invoking each method once. The Discover tab is fine; you don't need to compose flows for this.
3. Come back to the summary card. `recent` should equal `total`, everything else `0`.

Now wait 8 days (or fake it: open browser devtools → Application → Local Storage → find the `bowire_run_history` key for your workspace and edit the `startedAt` timestamps back by 8 days on half the entries). The chips flip from `●` to `◐` — those methods are now *stale*. Recovering `recent` means invoking them again.

That's the loop: **coverage is not a one-shot artefact, it's a decaying signal**. What was covered last week is stale this week; the summary card is telling you where to focus.

## Key Takeaways

1. **Four states per method: recent / stale / failing / uncovered.** Computed from the run-history ring buffer on read; no derived state on disk.
2. **Every runner contributes.** Discover invoke, Compose, Benchmark, Recording replay, Flow. `Sources` in the tooltip tells you *which* runners have exercised each method.
3. **Ring buffer, FIFO, per-workspace.** 500 entries default (50–5000 configurable). Fills up, oldest evicted. Cheap.
4. **`failing` beats `recent`.** The most recent outcome wins. A method with 10 recent passes and then one fail is red until the next successful run.
5. **Coverage decays.** Recent → stale → uncovered as time passes. The summary card is a *current* dashboard, not a historical archive.

## Knowledge Assessment

1. **The state machine.** A method has been invoked 20 times over the last two months. Last week you ran it four times, all passed. Yesterday you ran it once, it failed. What state is the chip in?
   *Answer:* **failing** (`✕`). The state rules check most-recent outcome first — if that's `fail` or `error`, the chip is failing regardless of how many prior successes there were. The pass-rate metric in the tooltip still reads a high percentage; the chip specifically signals *the last thing you saw*.

2. **Ring buffer eviction.** Your workspace has 500 runs in its buffer, cap set to the default. You invoke a method one more time. What happens?
   *Answer:* **The oldest entry is evicted (FIFO), the new entry lands at the tail.** The buffer never grows beyond `cap`; a write past the limit trims the head. That means very old runs eventually vanish from the summary card — a method that was invoked 500+ runs ago and hasn't been touched since will read as *uncovered* even though it *was* exercised in the deep past.

3. **Coverage source attribution.** You never open the Discover tab; you drive every call through Compose flows. Which sources will you see in a method's coverage tooltip?
   *Answer:* **`compose`** only. Sources is a set — it lists every distinct runner that fired the method within the ring-buffer window. If you never used Discover invoke, `discover` never appears. Same story for `benchmark` / `recording-replay` / `flow` — a source only shows up when it actually contributed a run.

4. **What does the summary card *not* tell you?** The card shows `12 of 40 methods exercised in the last 7 days`. Does that mean the other 28 are broken?
   *Answer:* **No.** The 28 are `stale` + `failing` + `uncovered` combined — but the card breaks those out separately. `stale` = "still passing, just haven't touched it lately"; `failing` = "the last run failed"; `uncovered` = "never run in the buffer window". Only the `failing` count implies broken. The 28-vs-12 delta is about *attention*, not correctness.

## What's Next

You've closed Unit 3 — Flows can now be authored, asserted (3.2), and their coverage measured (3.3). Unit 4 covers the four extension axes (protocols, sidecars, interceptor, UI widgets).

**Continue:** → [Unit 4 — Extending Bowire](../../unit-4/README.md)

## Reference

- `src/Kuestenlogik.Bowire/wwwroot/js/coverage.js` — the whole implementation: run-history storage, aggregation, chip renderer, run-history view.
- `bowire_run_history` — the per-workspace localStorage key holding the ring buffer.
- `bowire_run_history_cap` — the workspace-independent cap (50–5000; default 500).
- Main-Bowire issue [#343](https://github.com/Kuestenlogik/Bowire/issues/343) — the Regression Coverage initiative.
