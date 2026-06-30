# Learning Paths

Three audience-bound paths through the Bowire Bootcamp plus an optional, per-audience capstone. Or, complete every unit in order for the full picture (~3 hours).

The unit sequence on disk (`units/unit-0/` → `units/unit-5/`) is **flat**. The paths below are a *curation* on top — same model Surgewave Bootcamp uses for its purpose-driven paths.

> **The deployment shape is a tab inside a lesson, not a path-level branch.** When you reach a lesson that genuinely depends on whether you're driving the standalone CLI (`bowire --url …`) or the embedded host (`AddBowire()` / `MapBowire()`), the lesson's setup section walks both. From the workbench-walkthrough step onward, the UI is identical regardless of which shape you mounted with — so there's nothing to pick at the path level.

---

## 1. Workbench & API operator (User audience)

**For:** Developers, frontend engineers, QA, AI/agent operators who *use* Bowire to drive APIs. The CLI is the daily driver; the embedded shape is something they meet when their backend team uses it.

**Audience entry:** "I want to call APIs (mine or someone else's) without the API-client churn."

**Duration:** ~90 min · **Units touched:** 0, 1, 2, 3

| # | Lesson | Why it matters |
|---|---|---|
| 1 | [Unit 0.1 — What is Bowire?](units/unit-0/lesson-1/README.md) | Positioning, two-shape mental model |
| 2 | [Unit 0.2 — Setup (CLI tab)](units/unit-0/lesson-2/README.md) | Install global tool, verify |
| 3 | [Unit 0.3 — Hello Bowire](units/unit-0/lesson-3/README.md) | First call against public Petstore |
| 4 | [Unit 1.1 — First call (CLI shape)](units/unit-1/lesson-1/README.md) | First call against your own service |
| 5 | [Unit 1.2 — Multi-protocol (CLI shape)](units/unit-1/lesson-2/README.md) | REST + gRPC side-by-side |
| 6 | [Unit 2.1 — Record & Replay](units/unit-2/lesson-1/README.md) | Capture, mock-as-tape |
| 7 | [Unit 2.2 — Schema export + mock-as-stand-in](units/unit-2/lesson-2/README.md) | Mock with the full contract attached |
| 8 | [Unit 3.1 — AI-Agent integration (Path A)](units/unit-3/lesson-1/README.md) | Drive Bowire from Claude Desktop |

→ Terminates in the [**User capstone**](capstones/user/README.md): a `.bww` workspace + runbook that diagnoses a flaky mixed-protocol endpoint.

---

## 2. Developer / embed & extend (Developer audience)

**For:** Backend developers embedding Bowire in their own ASP.NET host, plugin authors shipping new protocols, contributors to the Bowire core. The embedded shape is the daily driver; the CLI is the verification tool.

**Audience entry:** "I want Bowire *inside* my service, and/or I want to ship something *on top of* Bowire."

**Duration:** ~160 min · **Units touched:** 0, 1, 3, 4

| # | Lesson | Why it matters |
|---|---|---|
| 1 | [Unit 0.1 — What is Bowire?](units/unit-0/lesson-1/README.md) | Positioning, two-shape mental model |
| 2 | [Unit 0.2 — Setup (Embedded tab)](units/unit-0/lesson-2/README.md) | `AddBowire()` + `MapBowire()` |
| 3 | [Unit 1.1 — First call (Embedded shape)](units/unit-1/lesson-1/README.md) | First mount, REST |
| 4 | [Unit 1.2 — Multi-protocol (Embedded shape)](units/unit-1/lesson-2/README.md) | co-host gRPC |
| 5 | [Unit 3.1 — AI-Agent integration (Path B)](units/unit-3/lesson-1/README.md) | `MapBowireMcpAdapter()` embedded MCP |
| 6 | [Unit 4.1 — .NET protocol plugin](units/unit-4/lesson-1/README.md) | `IBowireProtocol`, nupkg, `PackageReference` tab |
| 7 | [Unit 4.2 — Python sidecar plugin](units/unit-4/lesson-2/README.md) | polyglot escape hatch, sidecar zip |
| 8 | [Unit 4.3 — Interceptor / middleware](units/unit-4/lesson-3/README.md) | `app.UseBowireInterceptor()`, the Intercepted rail, `BowireInterceptorOptions` |
| 9 | [Unit 4.4 — Map widget / semantic kinds](units/unit-4/lesson-4/README.md) | `IBowireUiExtension`, `[BowireExtension]`, `coordinate.wgs84` auto-mount |

→ Terminates in the [**Developer capstone**](capstones/developer/README.md): ship your own Bowire plugin as a NuGet package.

---

## 3. Administrator / deploy & run (Administrator audience)

**For:** Platform engineers, SREs, DevOps, anyone packaging Bowire into deploys — internal-tools containers, CI runners, sidecar deploys, multi-tenant gateways.

**Audience entry:** "I need to ship Bowire into a non-laptop environment and keep it running."

**Duration:** ~75 min once the new lessons land (~25 min today, Unit 5.1 only) · **Units touched:** 0, 5

| # | Lesson | Why it matters |
|---|---|---|
| 1 | [Unit 0.1 — What is Bowire?](units/unit-0/lesson-1/README.md) | Positioning, shapes — admins need to pick the deploy shape |
| 2 | [Unit 0.2 — Setup (Administrator tab)](units/unit-0/lesson-2/README.md) | **proposed for PR 4** — systemd / Docker / IIS host of the CLI; embedded shape gating with `IsDevelopment()` / `#if DEBUG` |
| 3 | [Unit 5.1 — CI integration](units/unit-5/lesson-1/README.md) | `bowire test`, mock-as-service-container |
| 4 | *Unit 5.2 — Deployment patterns* | **proposed for PR 5** — single-binary CLI sidecar; embedded gated behind `#if RELEASE`; Settings IA: System → Defaults; reverse-proxy mount paths |
| 5 | *Unit 5.3 — Observability + operations* | **proposed for PR 5** — logs, structured output, v2.1 Settings IA's System tree, plugin extension-point tree in prod |

→ Terminates in the [**Administrator capstone**](capstones/administrator/README.md): a `docker-compose.yml` / k8s stack + production runbook.

---

## 4. Capstone (optional, per audience)

Each path terminates in its own capstone with an audience-appropriate deliverable. A single cross-audience capstone only really fits the Developer audience — the other two audiences need their own artefact-shape.

| Capstone | Deliverable | Path |
|---|---|---|
| [User](capstones/user/README.md) | `.bww` workspace + diagnosis runbook | Workbench & API operator |
| [Developer](capstones/developer/README.md) | NuGet plugin (protocol / extension / rail / module) | Developer / embed & extend |
| [Administrator](capstones/administrator/README.md) | `docker-compose.yml` (or k8s) + production runbook | Administrator / deploy & run |

---

## Or: the full bootcamp

Complete every unit in order — ~3 hours, ~10 lessons today, plus your audience's capstone (or all three).

**Prerequisites (everything):**

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Bowire CLI: `dotnet tool install --global Kuestenlogik.Bowire.Tool` (for Unit 1.1 / 1.2 CLI shape + Unit 5 CI)
- ASP.NET host (own, or scaffold via `dotnet new web`) + `dotnet add package Kuestenlogik.Bowire` (for Unit 1.1 / 1.2 Embedded shape + Unit 3.1 Path B + Unit 4.1 PackageReference tab)
- `dotnet new bowire-plugin` template: `dotnet new install Kuestenlogik.Bowire.Templates`
- **Python 3.10+** (Unit 4.2 only)
- **Claude Desktop** or **Cursor** (Unit 3.1 only)
- **Docker** (Unit 5.1 only — optional in local dev)
- A **GitHub repository** (Unit 5.1 only)

[Unit 0](units/unit-0/README.md) → [Unit 1](units/unit-1/README.md) → [Unit 2](units/unit-2/README.md) → [Unit 3](units/unit-3/README.md) → [Unit 4](units/unit-4/README.md) → [Unit 5](units/unit-5/README.md) → [Capstones](capstones/)
