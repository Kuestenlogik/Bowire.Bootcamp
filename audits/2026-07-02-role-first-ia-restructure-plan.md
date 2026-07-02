# Role-first IA restructure + cleanup plan (2026-07-02)

Supersedes the deployment-shape framing decided in the 2026-06-30 audit
(PRs #8–#13, merged). Those PRs made the deployment shape a **setup tab
inside each lesson**. This plan reverses that: the shape is no longer an
in-lesson alternate track — instead the **role/audience path is the
primary axis** and selects a chain of **single-modality units**.

## Vocabulary & physical layout

- **Course / track** *(role / use-case)* → **Unit** *(numbered, reusable)*
  → **Lesson** → **Steps**.
- **Folders exist only for units + lessons**:
  `units/unit-N/lesson-M/{README.md, start/, completed/}` and
  `units/unit-N/{README.md, toc.yml}`.
- **Courses / tracks are NOT folders** — they are pure Markdown curation
  (`LEARNING_PATHS.md`, plus the course tables in `index.md` /
  `units-overview.md`). A course is a freely-composed, **non-linear**
  selection of units; **the same unit is reused across courses**
  (e.g. the Workbench unit appears in the User, Developer and Admin
  courses).
- Numbering is the canonical "whole bootcamp" order; courses pick subsets
  in whatever order fits. The `U/C/E/X` letters used while planning were
  only modality tags — the on-disk units are numbered with descriptive
  titles.

## Guiding principle

**One unit = one modality.** UI, CLI/Ops, Embedded-coding and
Custom-Protocol/Extension-coding are never mixed *within* a unit.

Modality-switching is allowed **between** units inside a course (crossing
a unit boundary is explicit and each unit is self-contained), but never
**within** a unit. Units must therefore be self-contained — loosely
coupled via cross-links, not hard linear prerequisites — so they can be
dropped into any course.

**Cross-modality = reference only, never an in-unit alternate track.**
- A setup section may *mention* a variant and link it: "Bowire is also
  available as a global `dotnet tool` — see [the CLI unit]" — a link, not
  a second tab.
- A UI lesson that records traffic *links* to the CLI unit for the
  scriptable path (`bowire mock` / `bowire test`) instead of opening the
  CLI flow inline.
- This replaces the old "≈90% identical → tab" dedup argument: the truly
  shared material is described **once** and **linked**, not duplicated.

## Reference: Surgewave.Bootcamp conventions (the model to match)

`C:\Projekte\Kuestenlogik\Surgewave.Bootcamp` already solves this cleanly:

- **Many single-topic units**, each effectively one modality; grouped in
  `units-overview.md` sections (Foundations / Core Skills / Integration /
  …).
- **Per-lesson sample convention: `start/` + `completed/`** with its own
  `.csproj` (used uniformly — 336 projects). `bin/`/`obj/` are gitignored.
  - `start/` = prepared scaffold to build on (don't start from zero; the
    skeleton fits the task).
  - `completed/` = the reference/finished state (milestone achieved).
- **Role/goal paths** in `LEARNING_PATHS.md` — each path is a curated,
  ordered list of units (mermaid diagram + audience + duration). Paths
  combine freely.
- `toc.yml` per unit + top-level. One `capstone/` with `start/` +
  `completed/`.

Bowire is smaller in scope, so **modality = unit, topics = lessons
inside** (not dozens of micro-units).

## Target structure

`units-overview.md` sections & units:

### Foundations (modality-free)
- **Unit F — What is Bowire?** Positioning, concepts, the two deploy
  shapes *as a concept* (no setup mix).

### UI / Workbench operator (modality: UI)
- **Unit U — Workbench.** Lessons: Install + first call (UI),
  multi-protocol (Discover/Compose), Record & Replay, schema-backed
  mocks, Flow Assertions, Regression Coverage, Intercept rail (four
  postures), AI-agent from the desktop. Links to Unit C for scripting.

### CLI / Ops (modality: CLI)
- **Unit C — CLI & operations.** Lessons: Install + first call
  (`dotnet tool`, `bowire --url`), `bowire mock`, `bowire test` in CI
  (Flow / JUnit / HTML), deployment patterns, observability + ops,
  workspace hygiene (soft vs hard delete).

### Embedded coding (modality: coding/embedded)
- **Unit E — Embed Bowire.** Lessons: `AddBowire()` / `MapBowire()`
  (DI / auth / config), embedded MCP adapter, interceptor middleware
  (`UseBowireInterceptor`).

### Extension / custom-protocol coding (modality: coding/extension)
- **Unit X — Extend Bowire.** Lessons: .NET protocol plugin, Python
  sidecar plugin, UI extension (Map widget / semantic kinds), plugin
  lifecycle.

### Courses / tracks (`LEARNING_PATHS.md`, Surgewave-style — curation, not folders)
Numbered units (canonical order): `unit-0` Foundations · `unit-1`
Workbench: first contact (UI) · `unit-2` Workbench: record/mock/assert/
cover (UI) · `unit-3` CLI & operations (CLI) · `unit-4` Embed Bowire
(embedded coding) · `unit-5` Extend Bowire (extension coding).

Courses compose these freely (units reused across courses):

| Course | Units (example composition) | Cross-links (not inline) |
|---|---|---|
| User / operator | 0 → 1 → 2 | → 3 (`mock` / `test`) where scripting helps |
| Integrator / DevOps / Admin | 0 → 3 → (1 to inspect) | → 2 (Intercept rail) |
| Developer | 0 → 1 → 4 → 5 | → 3 (CLI as the verification tool) |
| *(later)* QA / tester | 0 → 2 → 3 (`bowire test`) | |

## Capstones

Keep the **three per-audience capstones** (`capstones/user`,
`capstones/developer`, `capstones/administrator`) — their deliverables
genuinely differ (`.bww` workspace + runbook / NuGet plugin /
`docker-compose` + runbook). Align each to the **`start/` + `completed/`**
convention.

## Migration mapping (old → new, coarse)
- Unit 0.1 → **F**. Unit 0.2/0.3 + 1.1/1.2 (shape tabs) → split by
  modality into **U** (UI), **C** (CLI), **E** (Embedded).
- Unit 2 (record/mock) → **U** (UI capture); `bowire mock` → **C**.
- Unit 3 (AI / Flow / Coverage) → **U**; AI-agent "Path B embedded" → **E**.
- Unit 4 (plugins / interceptor / map / lifecycle) → **X**; interceptor
  middleware → **E**.
- Unit 5 (CI / deploy / observability / workspace) → **C**.

## Sample sources (external repos — do not hand-roll toy samples)

- **Services the workbench drives** come from **`Bowire.Samples`**:
  - `harbor-demo/` — one Harbor Control Center domain across every
    protocol; the **Combined** host (`:5101`) *embeds Bowire* (workbench
    at `:5101/bowire`) and speaks REST + gRPC + SignalR + WebSocket + SSE
    at once — the go-to for UI units 1 & 2.
  - `protocols/` — per-plugin canonical demos (`Rest.PetStore`,
    `Grpc.Greeter`, `OData.Northwind`, `JsonRpc.Math`, …) for
    single-protocol lessons.
- **Custom-protocol authoring (Unit 5)** references the real, shipped
  **`Bowire.Protocol.Akka`** plugin (`src/`, `tests/`, `samples/`,
  README) instead of a toy `IBowireProtocol`.
- The bootcamp's own toy `units/unit-1-samples/` (HelloApi, HelloGrpc) is
  **retired** in favour of the above. Units point at these repos; only
  genuinely lesson-specific edits ship as `start/` + `completed/`.

**Sample progression:**
- **Intro (Unit 0–1):** keep it dead-simple — hello-world / echo demos
  from `protocols/` (`Grpc.Greeter` = SayHello, `WebSocket.Echo`,
  `Rest.PetStore`). Lowest ceremony for a first invoke.
- **Unit 2 onward:** carry the **Harbor Control Center domain**
  (`Ship`, `Dock`, `Crane`, `Container`, `PortCall` via the Combined
  host) so record/mock/assert/cover lessons work against one coherent
  business model.
- **Capstones:** *extend the Harbor domain* — the deliverable adds
  something to the domain (a new operation / entity / protocol / plugin),
  so the learner builds on a familiar model rather than a throwaway.

## Cleanup workstream
1. **DONE (2026-07-02):** removed the dead `capstone/` (singular) dir —
   untracked, only `bin/`/`obj/` build artefacts, superseded by
   `capstones/`.
2. Fold `units/unit-1-samples/` (HelloApi, HelloGrpc) into the per-lesson
   `start/` + `completed/` convention; remove the redundant separate dir
   and the parallel `unit-1/lesson-*/sample/` dirs.
3. Fix stale `ROADMAP.md` reference `capstone/solution/` → the
   `capstones/<audience>/solution/` locations.
4. Retire the landing / Unit-0 "Pick your deployment shape" path-level
   wording (`index.md`, `units/unit-0/README.md`) in favour of the
   role-first framing.
5. Add `units-overview.md`; ensure `toc.yml` per unit.

## PR sequencing (rebase-only; issues in English)
- **PR A — Cleanup** (item 1 done; items 2–5). Low risk, decoupled.
- **PR B — IA scaffold** — new single-modality unit dirs + rewritten
  `LEARNING_PATHS.md` / `index.md` / `units-overview.md` (structure only,
  no prose rewrite).
- **PR C…N — Unit rework** — per unit family: prose to one modality,
  move setup/first-call, `start/` + `completed/` samples, cross-links
  replacing tabs.
