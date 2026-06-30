# Bootcamp audit — v2.1 coverage + purpose-path restructure

*Date:* 2026-06-30
*Scope:* `Kuestenlogik/Bowire.Bootcamp` against `Kuestenlogik/Bowire` v2.1.0
*Status:* Draft — proposal for follow-on PRs, no curriculum edits applied yet.

The bootcamp shipped against Bowire v1.x discovery + invoke + record/mock + plugin
authoring. Bowire v2.1 expanded the workbench surface considerably (Compose rail,
pluggable workbench, interceptor middleware, MapLibre extension, MCP forwarder,
catalogue providers, workspaces UX, benchmarks). The curriculum still teaches the
v1.x shape — none of the v2.1 highlights appear in any unit. Separately, the five
learning paths conflate **purpose** (what role the operator plays) with **content
slice** (which units to walk), and the single capstone only fits one of those
roles. This audit covers three sections:

1. **v2.1 feature gap** — what Bowire v2.1 ships that the bootcamp doesn't teach.
2. **Deployment-shape coverage** — what the CLI vs Embedded split currently
   handles well and where v2.1 widens the gap.
3. **Purpose-path restructure** — re-cut the learning paths around the three
   audiences Bowire actually has, with one capstone per audience.

Sections 1–2 are summarised here for context; the substantive proposal is
Section 3.

---

## Section 1 — v2.1 feature gap

Bowire v2.1.0 (release notes:
`Kuestenlogik/Bowire/docs/release-notes/v2.1.0.md`) lists eighteen highlight
entries. The bootcamp covers exactly **zero** of them in any lesson body. The
only mention of any v2.1 concept across all nine lessons is a competitive-
positioning aside in `units/unit-0/lesson-1/README.md:114` that contrasts
Bowire's recording with Postman's browser Interceptor — and that's referencing
Postman's product, not Bowire's `Kuestenlogik.Bowire.Interceptor` package.

The gaps that matter for the curriculum (not exhaustive — picked by
"would a v2.1 operator notice the lesson is stale"):

| v2.1 feature | Bootcamp location it should land | Current state |
|---|---|---|
| **Compose rail** — Hoppscotch-style request builder, protocol picker, per-protocol layouts, history persistence, binary uploads, Collections + Presets integration | Unit 1 (both tracks) — this is the primary verb for "drive a single request" now, replacing the old Design rail | Not mentioned |
| **Pluggable workbench** — rails / modules / extensions ship as separate packages, `Bundle.Minimal` vs `Bundle.Workbench`, per-rail enable/disable, `[BowirePlugin]` + `[BowireExtension]` attributes | Unit 4 (extending Bowire) — the contribution model is the seam plugin authors actually target | Unit 4 still teaches `IBowireProtocol` only — no mention of `IBowireRailContribution`, UI extensions, modules, or `[BowireExtension]` |
| **Transparent in-process interceptor** — `app.UseBowireInterceptor()`, Intercepted rail, auto-append to recordings | Unit 1 Embedded track + Unit 2.1 (record/replay) | Not mentioned; recording lesson teaches only the in-workbench session-capture flow |
| **Workspaces** — `.bww` file, sort + manual ordering, environment variables under the workspace, soft-delete + aggregated Trash | Unit 0.3 or new Unit 1 lesson — workspaces are the operator's persistent state container | Not mentioned; bootcamp doesn't teach the operator how to save / share / load their work |
| **MapLibre extension + TacticalApi sample** — auto-mounted viewer when a response carries `coordinate.wgs84`, bi-directional JSON↔map sync | Unit 4 (a Map / spatial UI extension is the showcase example of a UI extension plugin) and/or new "domain extension" lesson | Not mentioned |
| **MCP-over-MCP forwarder** — `bowire mcp serve --attach`, bearer-auth, CI-runner ↔ workstation relay | Unit 3.1 | Unit 3.1 teaches only direct stdio `bowire mcp serve` — no relay, no `--token`, no `--attach-token` |
| **Catalogue providers** — `Kuestenlogik.Bowire.Catalogue` with local-file / HTTP / Consul providers | Unit 4 (plugin contribution surface) — and as an admin-deploy concern in a future Production path | Not mentioned |
| **REST auto-probing of well-known OpenAPI paths** | Unit 1 (REST first call) | Bootcamp REST lesson hand-writes the full `/openapi.json` URL; auto-probe would let it drop to a bare origin |
| **Discovery self-origin gate** | Documentation note in Unit 1 Embedded + Unit 3.1 (MCP adapter) — relevant for embedded hosts that also expose their own protocols | Not mentioned; embedded learners will hit phantom-service confusion if they enable the MCP adapter |
| **Benchmarks rail** — random targets, diff banner, CSV / k6 / OTLP exports | Either a new lesson under Production path or a Unit 2 sibling lesson | Not mentioned |
| **Interceptor / Welle 2 package rename** — `Rail.` prefix dropped, `Kuestenlogik.Bowire.Interceptor` consolidates the proxy / intercepted / traffic surface | Unit 4 (plugin packaging) + Unit 1 Embedded (PackageReference list) | Not mentioned; Unit 1 Embedded still uses the v2.0 package shape |
| **Guided tour engine** — `bowireStartCaptureRecordingTour` &c. | Cross-cutting — link from each relevant lesson as a "if you'd rather click through the tour, run this from the help menu" | Not mentioned |
| **AI discoverability** — `<link rel="mcp">`, Reader Mode suppression | Unit 3.1 background | Not mentioned |

The two gaps that are load-bearing for the next bootcamp PR are **Compose** and
**Workspaces** — both are first-touch surfaces that the operator sees within
30 seconds of opening the workbench, and the current Unit 1 walkthrough doesn't
match what they see.

---

## Section 2 — Deployment-shape coverage (CLI vs Embedded)

The CLI-vs-Embedded split that landed in commits `c8ade4f` and `b38db69` is the
right shape and largely works. v2.1 widens the gap in two specific places:

- **Embedded-only territory.** The interceptor middleware
  (`UseBowireInterceptor()`), the Catalogue HTTP provider when it's a sibling
  service in the same host, the per-host DI / `[Authorize]` / logging
  story — these all only exist on the Embedded track. Unit 1 Embedded today
  stops at "mount the workbench"; it never crosses into the interceptor, which
  is the embedded shape's headline v2.1 feature.
- **CLI-only territory.** The MCP forwarder (`--attach` / `--token` /
  `--attach-token`), plugin install via `bowire plugin install`, mock-server as
  a separate process, and the entire CI integration are CLI-only. Unit 3.1
  + Unit 5.1 + Unit 4.1 already split correctly. The MCP forwarder is the only
  net-new CLI capability that needs a lesson.

Where the existing split breaks down: **Workspaces** and **Compose** are
shape-agnostic (workbench-UI features), but the bootcamp's "Unit 1 splits, Unit
2+ merges" rhythm currently puts them on neither side. Either Unit 0.3
(workbench tour) needs to grow to cover them before the track-split, or a new
post-Unit-1 merge lesson needs to introduce them. The latter is cleaner — the
operator has already mounted Bowire by the time they hit "save this as a
workspace" or "compose a request from scratch".

No further detail in this draft — the v2.1 feature gap (Section 1) drives all
the concrete shape-level changes; Section 2 is just calling out which Unit 1
track each gap lands on.

---

## Section 3 — Purpose-path restructure with per-audience capstones

The current `LEARNING_PATHS.md` enumerates **five** paths:

1. Workbench Fundamentals
2. Mock-as-Stand-In
3. AI & Automation
4. Plugin Author
5. Production / CI

These conflate two axes:

- **Purpose** — what role the operator plays with Bowire long-term.
- **Content slice** — which unit subset is the shortest path to one outcome.

Workbench Fundamentals is a content slice ("start here"); Mock-as-Stand-In is a
single-outcome slice; AI & Automation is also single-outcome; Plugin Author is
both an outcome and a role; Production / CI mixes a role (ops) with one
outcome (CI). The result is that an Administrator role and a User role both
have to read between the lines to find their slice — the User reads
Fundamentals + Mock + AI, the Administrator reads Fundamentals + CI + parts of
Plugin Author (for the deployment-shape implications). Neither has a single
purpose-path entry-point.

### Proposed restructure: three audience paths, each with its own capstone

The three audiences Bowire actually has:

- **User** — operates the workbench and the API surface. Frontend dev, QA,
  backend dev driving an unfamiliar service, integration tester, SRE
  diagnosing a flaky endpoint.
- **Developer** — embeds Bowire into a host, extends it with plugins,
  contributes rails / UI extensions / modules.
- **Administrator** — deploys Bowire (CLI / embedded / sidecar / containerised)
  into a production / shared / multi-tenant environment, owns auth +
  observability + plugin allow-listing + workspace backup.

Each audience gets ONE path and ONE capstone. Existing single-outcome slices
(Mock-as-Stand-In, AI & Automation) stay listed in `LEARNING_PATHS.md` as
**quick-tour shortcuts** under the User path — they aren't audiences, they're
scenarios the User performs.

#### User path — Workbench & API operator

**Audience:** Anyone who opens Bowire to drive an API surface — frontend devs
mocking a backend for a sprint, QA writing assertions, backend devs probing a
service they didn't write, SREs diagnosing a flaky endpoint, integration
testers, AI-agent operators driving MCP from Claude Desktop.

**Lessons:** Unit 0 → Unit 1 (CLI track is the default — the User audience
rarely has the source code) → Unit 2.1 + 2.2 (record / mock) → Unit 3.1
(AI integration). Plus two **new** lessons:

- **Workspaces + Compose** (post-Unit-1 merge) — save your work as a `.bww`,
  share it, drive Compose for free-form requests, organise into collections
  + presets.
- **Interceptor + reverse-proxy capture** (optional, for users who run an
  embedded-Bowire backend locally) — point any client at the host, watch
  flows land in the Intercepted rail, auto-append to a recording.

**Capstone — "Diagnose a flaky API endpoint" runbook.**
Deliverable: a `.bww` workspace file + a runbook `README.md`. The runbook walks
a realistic mixed-protocol scenario (REST checkout endpoint + gRPC inventory
sub-call + WebSocket order-status stream); the workspace pins the source URLs,
recorded reproductions, mock-server config, AI-agent MCP config, and notes
captured during the diagnosis. The user proves they can drive the full
workbench (Workspaces + Compose + Recording + Mock-Server + AI integration
+ Intercepted rail) end-to-end on a representative target.

Alternative scenario for the same capstone shape: **"Mock backend for a
frontend sprint"** — same `.bww` deliverable, but the runbook documents how a
frontend team consumes the mock against a recorded contract, including how to
hand the workspace off to a teammate (workspace export / import / catalogue
pin).

#### Developer path — Embed + Extend

**Audience:** .NET developers wiring Bowire into their own ASP.NET host,
authors of protocol plugins, UI extensions, rail contributions, or modules,
maintainers of internal Bowire bundles.

**Lessons:** Unit 0 (background) → Unit 1 Embedded track (`AddBowire()` +
`MapBowire()` in a real host) → new **Pluggable workbench tour** lesson
(`[BowirePlugin]`, `[BowireExtension]`, `IBowireRailContribution`,
`IBowireServiceContribution`, `IBowireEndpointContribution`, `Bundle.Minimal`
vs `Bundle.Workbench`, the `Rail.`-prefix-dropped package list) → Unit 4.1
(.NET protocol plugin) → Unit 4.2 (Python sidecar) → new **UI extension**
lesson modelled on the v2.1 MapLibre extension (`[BowireExtension]`, the
response-pane tab/split contract, `preferredSplitExtensionForMethod`,
semantic-kind detectors).

**Capstone — Ship a NuGet plugin.**
Deliverable: a published (or publishable) NuGet package, choosing ONE shape:

- A **protocol plugin** (e.g. a new wire — the bootcamp's existing pirate-speak
  / yoda-speak samples are the warm-up; the capstone picks something real, like
  a Redis-protocol or a custom binary wire).
- A **UI extension** modelled on the MapLibre extension (response-pane widget
  + JSON detector + bi-directional sync).
- A **rail contribution** (a new sidebar rail with its own state, descriptor,
  CRUD endpoints, contribution-self-register).
- A **module** (cross-rail orchestration — e.g. a "compare two responses"
  module that adds menu items to JSON + Compose + Recordings panes).

The current `capstone/` directory in the repo (`Multi-Protocol API Tour`)
**already targets this audience** — its deliverables are "build a recording +
mock-server + MCP config + GitHub Actions workflow + (optional) protocol
plugin", which are developer-shape outputs. The proposal here is to **sharpen
its framing**: drop the "optional protocol plugin" line and make the plugin
the primary deliverable, demoting the multi-protocol-tour scenario to one of
the four plugin-shape options ("ship a TacticalApi-style protocol plugin").

#### Administrator path — Deploy + Run

**Audience:** DevOps, SREs, platform engineers, security / compliance
operators deploying Bowire into shared / multi-tenant / production
environments. Today's "Production / CI" path served this audience but only
for the CI sub-slice.

**Lessons:** Unit 0 (background) → Unit 1 CLI track (the canonical deploy
shape) → Unit 5.1 (CI) → new **Containerised deployment** lesson
(`docker-compose.yml` and / or Kubernetes manifests for the CLI shape behind
a reverse proxy) → new **Auth + observability** lesson (bearer-auth for the
MCP forwarder, structured-log shipping, Prometheus / Grafana hookup for the
benchmark exports, OTLP traces) → new **Plugin allow-listing + workspace
backup** lesson (the `--disable-plugin` and catalogue-pinning story for
locked-down deployments, plus how to back up `.bww` + mock + recording state).

**Capstone — Production-ready deployment stack.**
Deliverable: a `docker-compose.yml` OR Kubernetes manifests (operator's
choice) + a runbook `README.md` covering:

- Reverse-proxy config (nginx / Caddy / Traefik) in front of Bowire's HTTP
  endpoint, TLS termination, path prefix (`MapBowire("/bowire")` in embedded
  mode).
- Auth — bearer-token for the MCP forwarder, basic-auth or OIDC at the
  reverse-proxy for the workbench UI itself.
- Observability — Prometheus scrape of the benchmark exports, log shipping
  format, dashboards for request latency / error rate / active subscriptions.
- Plugin disable list — which bundled rails / extensions / modules to drop
  for the deployment's security posture (e.g. disable the Interceptor rail
  in a shared environment, disable the MCP forwarder if no agents need it).
- Workspace backup — periodic export of `.bww` + recordings + mocks, restore
  procedure, multi-operator workspace contention notes.

### Capstone folder layout

Right now the repo has a single `capstone/` directory:

```
capstone/
├── README.md           # Multi-Protocol API Tour
├── ARCHITECTURE.md
├── REQUIREMENTS.md
├── sample/             # TBD — Harbor Tour backend
└── solution/           # TBD — reference solution
```

If the proposal lands, the current `capstone/` matches the **Developer**
capstone (after the framing tweak above). The proposed layout:

```
capstones/
├── README.md           # Index — "pick the capstone matching your audience"
├── user/
│   ├── README.md       # Diagnose-flaky-endpoint runbook brief
│   ├── REQUIREMENTS.md
│   ├── scenario/       # Sample REST + gRPC + WS backend the operator targets
│   └── reference/      # Reference .bww + runbook the learner can diff against
├── developer/
│   ├── README.md       # Ship-a-plugin brief (replaces current capstone/README.md)
│   ├── ARCHITECTURE.md # From current capstone/
│   ├── REQUIREMENTS.md # From current capstone/, sharpened for plugin shape
│   ├── sample/         # Current capstone/sample (Harbor Tour or replacement)
│   └── solution/       # Current capstone/solution
└── administrator/
    ├── README.md       # Production-deployment brief
    ├── REQUIREMENTS.md
    ├── compose/        # docker-compose.yml + .env.example + reverse-proxy config
    ├── k8s/            # manifests + kustomization
    └── reference/      # Reference runbook + dashboards / alerts
```

Migration: `git mv capstone capstones/developer`, then `mkdir capstones/user
capstones/administrator` with their own `README.md` skeletons. The top-level
`capstones/README.md` is a thin index that maps audience → capstone. All
cross-links in `README.md`, `index.md`, `LEARNING_PATHS.md`, `toc.yml`,
`units-overview.md`, and the per-unit "Next steps" sections update from
`capstone/` → `capstones/<audience>/`.

### What survives, what changes, what gets deprecated

| Asset | Disposition |
|---|---|
| `LEARNING_PATHS.md` 5-path layout | Replaced by 3-audience layout. Mock-as-Stand-In + AI & Automation demoted to "User scenarios" sub-section. Plugin Author folds into Developer. Production / CI folds into Administrator. |
| `capstone/` directory | `git mv` to `capstones/developer/`. README sharpened to make plugin-shape the primary deliverable. |
| `capstones/user/` + `capstones/administrator/` | **New** — scaffolded with README + REQUIREMENTS, scenario / runbook / reference content tracked on the roadmap alongside the existing "Capstone reference solution" entry. |
| Unit 1–5 lesson bodies | Unchanged in this restructure. The v2.1 feature gap from Section 1 drives lesson-body edits in a separate PR. |
| `ROADMAP.md` | Adds the two new capstones + the three new audience-path lessons (Workspaces+Compose for User, Pluggable Workbench Tour + UI Extension for Developer, Containerised Deployment + Auth/Observability + Plugin Allow-listing for Administrator) under "Next up". |

---

## Recommended PR sequencing

1. **PR 1 — purpose-path restructure (this audit's Section 3).** `LEARNING_PATHS.md` rewrite + `capstone/` → `capstones/developer/` move + scaffold `capstones/user/` and `capstones/administrator/` READMEs + roadmap entries. No lesson-body edits.
2. **PR 2 — v2.1 first-touch surfaces.** Add the Workspaces + Compose lesson to the User path (also linked from Developer); update Unit 1 CLI lesson 1 to use the well-known OpenAPI auto-probe; update Unit 1 Embedded to drop `UseBowireInterceptor()` into the host and walk the Intercepted rail.
3. **PR 3 — Developer path expansion.** Pluggable workbench tour + UI extension lesson + Unit 4 framing update for the new contribution model.
4. **PR 4 — Administrator path expansion.** Containerised deployment + Auth/Observability + Plugin allow-listing lessons; backfill the Administrator capstone reference content.
5. **PR 5 — MCP-over-MCP forwarder + remaining v2.1 polish.** Unit 3.1 grows the `--attach` / `--token` story; MapLibre + TacticalApi gets a mention in the User path's Compose lesson; Benchmarks rail picked up as a sibling lesson in the Administrator path.

PRs 1–2 are the load-bearing pair — everything after that is incremental. PR 1
is purely structural and can land independently of any lesson-body work.
