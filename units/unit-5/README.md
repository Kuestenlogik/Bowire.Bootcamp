# Unit 5: CI · deploy · operate

*Time: ~80 minutes • Lessons: 5 • Previous: [Unit 4](../unit-4/README.md)*

The Administrator unit. Lesson 5.1 wires Bowire into CI (recording-driven); 5.2 walks the two production deployment shapes (standalone CLI vs embedded host); 5.3 covers the observability + day-1 operations surface; 5.4 runs the v2.2 Flow-driven CI runner with JUnit + HTML reports; 5.5 covers the soft-vs-hard workspace-deletion posture.

## Prerequisites

- [Unit 0](../unit-0/README.md) complete — you can install + launch Bowire in at least one shape.
- For 5.1: a `.bwr` recording from [Unit 2](../unit-2/README.md) and a GitHub repository.
- For 5.2: Docker, plus a Linux host (real or VM) if you want to run the systemd walkthrough.
- For 5.3: a working OTLP collector locally (the lesson stands one up with Docker).

## Lessons

| Lesson | Topic | What You'll Build |
|--------|-------|-------------------|
| [5.1](lesson-1/README.md) | GitHub Actions integration | `bowire test` as an assertion suite + `bowire mock` as a job-level service container |
| [5.2](lesson-2/README.md) | Deployment patterns | Standalone `bowire` container + embedded host gated for production; reverse-proxy in front; layered config (`appsettings.json` → `BOWIRE_*` env → CLI flags) |
| [5.3](lesson-3/README.md) | Observability + operations | OTLP export wired against `BowireTelemetry`; runbook for plugin disable, workspace backup, plugin-health probe |
| [5.4](lesson-4/README.md) | `bowire test` in CI — Flow runs, JUnit XML, HTML | The v2.2 Flow-driven CI runner with `--report` / `--junit` / `--base-url` / `--env` wired into GitHub Actions |
| [5.5](lesson-5/README.md) | Workspace deletion — Soft vs Hard | The two deletion postures, Trash retention, and how Undo works across both |

## Why this unit

A workbench that only lives on a laptop is a dev tool. A workbench you can ship into staging, production, or a CI runner — and keep running across upgrades, plugin breakage, and operator handovers — is infrastructure.

Unit 5 covers the five things that turn the first into the second:

1. **CI integration** (5.1) — recordings as regression assertions; the mock server as a deterministic upstream for downstream test jobs.
2. **Deployment shapes** (5.2) — when to pick the standalone CLI (one workbench across many backends) vs the embedded mount (one workbench co-deployed with its service), and how to package each.
3. **Observability + ops** (5.3) — what Bowire emits (logs, traces, metrics under the `Kuestenlogik.Bowire` ActivitySource + Meter), how to point it at your collector, and the day-1 runbook (disable a misbehaving plugin, back up an operator's workspace, probe plugin health).
4. **Flow-driven CI** (5.4) — v2.2's Flow-runner mode of `bowire test` with JUnit XML + HTML report emission, wired into GitHub Actions.
5. **Workspace hygiene** (5.5) — soft vs hard deletion, Trash retention, and how the Undo layers (Trash + action-log) interact.

---

**Next:** → [Capstones](../../capstones/) (the [Administrator capstone](../../capstones/administrator/README.md) closes the loop: a `docker-compose.yml` + production runbook drawing on every lesson in this unit)
