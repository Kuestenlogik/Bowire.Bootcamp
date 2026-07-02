# User capstone — Requirements

> **Difficulty:** Intermediate · **Time:** ~3–4 hours · **Audience:** User path graduates (Units 0–3).

## Software prerequisites

| Tool | Min version | Reason |
| --- | --- | --- |
| .NET SDK | 10.0 | Build + run the three-hop scenario stack under `scenario/`. |
| Bowire Tool | 2.1.0+ | `dotnet tool install -g Kuestenlogik.Bowire.Tool` (see Unit 0.2). The standalone Tool ships every plugin you need pre-bundled. |
| A modern browser | Chrome / Edge / Firefox current | Workbench host. |
| (Optional) Claude Desktop / Cursor | current | If you want to wire the AI Assistant via stdio MCP (Unit 3.1 Path A). The Tool's in-process AI Assistant (Ollama / LM Studio default per `Bowire:Ai`) works without these. |

No Docker, no Kubernetes, no extra services. Everything runs on the laptop.

## Knowledge prerequisites

You should be comfortable with every concept introduced in:

- **Unit 0** — What Bowire is, the two deployment shapes, and getting a workbench running.
- **Unit 1** — Discover, the Compose invoke pane, multi-protocol (REST + gRPC) side by side.
- **Unit 2** — Recordings rail, `.bwr` format, replaying a recording from the **Mocks rail** (UI).

The chaos-injected CLI replay (`bowire mock --chaos`) and driving Bowire from an external AI agent are **capabilities**, not prerequisites — they live in the CLI course (Unit 3) and are optional here. The in-workbench AI Assistant panel is a UI feature you can use without any of that.

If any of the above is rusty, the relevant unit's "Steps" section is a 10-minute refresher each.

## Plugins required

The Tool standalone bundles every plugin used in this capstone via `Kuestenlogik.Bowire.Bundle.Workbench`. Specifically you'll touch:

- `Kuestenlogik.Bowire.Protocol.Rest` (REST + OpenAPI discovery on `/openapi/v1.json`).
- `Kuestenlogik.Bowire.Protocol.Grpc` (gRPC + Server Reflection discovery).
- `Kuestenlogik.Bowire.Protocol.WebSocket` (the `ws://` source for `portcalls/stream`).
- `Kuestenlogik.Bowire.Workspaces` (the Workspaces rail + `.bww` export).
- `Kuestenlogik.Bowire.Recordings` (the Recordings rail + `.bwr` export).
- `Kuestenlogik.Bowire.Mock` (Mocks-rail replay in the UI; the CLI `bowire mock` + chaos injection is a Unit 3.2 capability).
- `Kuestenlogik.Bowire.Ai` (the AI Assistant panel — optional but recommended).
- `Kuestenlogik.Bowire.Compose` (the Compose rail — already implied; you can't avoid it).

If you embed Bowire in your own host instead of using the Tool standalone, you'd pull these in as PackageReferences explicitly; the User path uses the Tool, so you don't.

## What you deliver

| Artefact | Path in your fork | Format |
| --- | --- | --- |
| Workspace export | `capstones/user/solution/berth-flake.bww` | `bowire-workspace` v2 JSON envelope (canonical format, `WorkspaceCommand.CanonicalFormatVersion = 2`). |
| Diagnosis writeup | `capstones/user/solution/RUNBOOK.md` | Plain markdown, five sections (see `README.md` Step 10). |

## Grading checklist

Tick each item off as you go. The reference solution under `solution/` meets every item; use it as the gold standard once you've made your own pass.

### Workspace (the `.bww`)

- [ ] **Three sources pinned**: `http://localhost:5301` (REST), `grpc@http://localhost:5302` (gRPC), `ws://localhost:5303/portcalls/stream` (WebSocket).
- [ ] **Workspace name + colour set** (`berth-flake`, any colour — both land in the export's `workspace` block).
- [ ] **At least one saved Compose tab per source** — the `data.requestBuilderHistory` (or the workspace's collections array, depending on how you saved them) shows the tabs survive the roundtrip.
- [ ] **At least one recording referenced** with the failure pattern captured.
- [ ] **Roundtrips cleanly** through `bowire workspace import` + `bowire workspace export` (diff modulo `exportedAt`).

### Runbook (the markdown)

- [ ] **"What's broken" section** in one paragraph a non-engineer can read.
- [ ] **"What I checked" bullets** — every Discover surface, every Compose tab, the recording, the mock replay.
- [ ] **"Where the seam sits" paragraph** that concludes the diagnosis is in `CraneOps.Reserve`, with the recording-step IDs that prove it.
- [ ] **"Reproduce offline" block** — replay the recording from the Mocks rail; optionally the portable `bowire mock … --chaos "latency:100-500,fail-rate:0.30" --port 7090` line (a Unit 3.2 capability).
- [ ] **"What to ask the backend team"** — three concrete questions, each anchored to a recording-step id.
- [ ] **Cross-links back to the lessons** (Unit 1.1 Discover + invoke, Unit 2.1 Record & Replay + Mocks rail; the CLI chaos repro is Unit 3.2).

### Diagnosis quality

- [ ] **The seam is upstream of Berthing, not downstream.** Wrong answers: "the WebSocket stream is flaky", "the OpenAPI doc is wrong", "Berthing itself is broken". Right answer: `CraneOps.Reserve` returns `RESOURCE_EXHAUSTED` under the 30 % failure rate, and `Berthing` propagates it as a 502.
- [ ] **The mock-plus-chaos reproduces the same failure shape** without the live `CraneOps` host running.
- [ ] **A reader who hasn't seen the answer can find it** from the recording-step IDs + the side-by-side comparison in the runbook.

## What's intentionally not covered

- **Fixing the backend.** Diagnosis is the deliverable.
- **Production deployment.** That's the Administrator capstone.
- **Plugin authoring.** That's the Developer capstone.
- **The embedded shape.** The User path uses the Tool standalone; if your team embeds Bowire in their own host, the workflow is identical but the workbench URL is `/<host-base>/bowire` instead of `/`.

---

**Back to:** [User capstone README](README.md) · [Curriculum](../../units-overview.md)
