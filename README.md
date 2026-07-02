# Bowire Bootcamp

A hands-on, self-paced tutorial that takes you from zero to "I can ship a new protocol plugin" with [Bowire](https://bowire.io) — Küstenlogik's multi-protocol API workbench. **Role-first courses** compose six **single-modality units**; each unit stays in one modality (UI, CLI, embedded coding, or extension coding) and never makes you switch mid-unit. The two deployment shapes (standalone CLI vs embedded host) are a **concept** you meet in Unit 0; setup lives in the shape's own unit.

## Goal

Learn to drive Bowire end-to-end — discover any wire, capture sessions, replay through `bowire mock`, integrate AI agents, deploy and operate it, and ship your own protocol plugin in .NET or any language.

## What is Bowire Bootcamp?

A self-contained learning experience. Each lesson runs in 5–15 minutes and includes concepts, copy-pasteable exercises, and — where a lesson needs code — a `start/` scaffold to build on plus a `completed/` reference. Runnable sample services come from the [`Bowire.Samples`](https://github.com/Kuestenlogik/Bowire.Samples) repo (the Harbor demo + per-plugin `protocols/`); the custom-protocol unit studies the real [`Bowire.Protocol.Akka`](https://github.com/Kuestenlogik/Bowire.Protocol.Akka) plugin.

The [reference docs](https://bowire.io/docs/) answer **what does X do**. This bootcamp answers **how do I go from zero to productive**.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- **One** of: the Bowire CLI (`dotnet tool install --global Kuestenlogik.Bowire.Tool`, Unit 3) or the `Kuestenlogik.Bowire` NuGet in your ASP.NET host (`dotnet add package Kuestenlogik.Bowire`, Unit 4)
- Sample services from [`Bowire.Samples`](https://github.com/Kuestenlogik/Bowire.Samples)
- Docker (Unit 3 — CI/deploy) · Python 3.10+ (Unit 5.2 — sidecar) · Claude Desktop or Cursor (Unit 3.3 — AI agents)

## Getting Started

```bash
git clone https://github.com/Kuestenlogik/Bowire.Bootcamp.git
cd Bowire.Bootcamp
```

Then open [Unit 0: Foundations](units/unit-0/README.md), or the DocFX learn site:

```bash
./scripts/build-learn.ps1 -Serve -Port 8082
```

## Courses

Pick one by role — a course is a curated selection of units (see **[Learning Paths →](LEARNING_PATHS.md)**):

| Course | Target audience | Units |
|--------|-----------------|-------|
| **Workbench & API operator** | Developers, frontend, QA, AI/agent operators | 0 → 1 → 2 |
| **Integrator / DevOps / Administrator** | Platform engineers, SREs, DevOps | 0 → 3 |
| **Developer — embed & extend** | Backend devs embedding Bowire, plugin authors, contributors | 0 → 1 → 4 → 5 |

Each course ends in a per-audience capstone that **extends the Harbor domain**. Or complete every unit in order for the full picture.

---

## Curriculum

Six single-modality units plus a per-audience capstone. See **[All Units](units-overview.md)** for the catalogue.

| Unit | Modality | Topic | Lessons |
|---|---|---|---|
| [Unit 0: Foundations & setup](units/unit-0/README.md) | onboarding | What Bowire is · the two deployment shapes · get it running · how the bootcamp works | 4 |
| [Unit 1: The Workbench — first contact](units/unit-1/README.md) | UI | Discover, invoke, multi-protocol, streaming | 2 |
| [Unit 2: The Workbench — record, mock, assert, cover](units/unit-2/README.md) | UI | Record/replay, schema mocks, Flow assertions, coverage, Intercept rail | 5 |
| [Unit 3: CLI & operations](units/unit-3/README.md) | CLI | Install, `mock`/`test`+CI, `mcp serve`, reverse-proxy, deploy, observe, workspaces | 7 |
| [Unit 4: Embed Bowire](units/unit-4/README.md) | embedded coding | `AddBowire`/`MapBowire`, embedded MCP, interceptor middleware | 3 |
| [Unit 5: Extend Bowire](units/unit-5/README.md) | extension coding | Protocol plugin, sidecar, UI extension, plugin lifecycle | 4 |
| [Capstones](capstones/) | — | [User](capstones/user/README.md) · [Developer](capstones/developer/README.md) · [Administrator](capstones/administrator/README.md) | 3 |

### Unit 0: Foundations & setup *(onboarding)*

| Lesson | Topic |
|--------|-------|
| [0.1](units/unit-0/lesson-1/README.md) | What is Bowire? — positioning vs Postman / Insomnia / Bruno |
| [0.2](units/unit-0/lesson-2/README.md) | The two deployment shapes — CLI vs embedded, as a concept |
| [0.3](units/unit-0/lesson-3/README.md) | Get Bowire running — least-ceremony path to an open workbench |
| [0.4](units/unit-0/lesson-4/README.md) | How this bootcamp works — course → unit → lesson |

### Unit 1: The Workbench — first contact *(UI)*

| Lesson | Topic |
|--------|-------|
| [1.1](units/unit-1/lesson-1/README.md) | First contact — Discover + the invoke pane (the canonical walkthrough) |
| [1.2](units/unit-1/lesson-2/README.md) | Multi-protocol in one workbench — gRPC unary + server-streaming |

### Unit 2: The Workbench — record, mock, assert, cover *(UI)*

| Lesson | Topic |
|--------|-------|
| [2.1](units/unit-2/lesson-1/README.md) | Record & Replay |
| [2.2](units/unit-2/lesson-2/README.md) | Schema-backed mocks |
| [2.3](units/unit-2/lesson-3/README.md) | Flow Assertions — the five-kind expectation model |
| [2.4](units/unit-2/lesson-4/README.md) | Regression Coverage — recent · stale · failing · uncovered |
| [2.5](units/unit-2/lesson-5/README.md) | Intercept rail — four postures |

### Unit 3: CLI & operations *(CLI)*

| Lesson | Topic |
|--------|-------|
| [3.1](units/unit-3/lesson-1/README.md) | Install & first call |
| [3.2](units/unit-3/lesson-2/README.md) | Mock & test in CI |
| [3.3](units/unit-3/lesson-3/README.md) | AI agents over MCP (`bowire mcp serve`) |
| [3.4](units/unit-3/lesson-4/README.md) | Reverse-proxy interception |
| [3.5](units/unit-3/lesson-5/README.md) | Deployment patterns |
| [3.6](units/unit-3/lesson-6/README.md) | Observability & operations |
| [3.7](units/unit-3/lesson-7/README.md) | Workspace hygiene |

### Unit 4: Embed Bowire *(embedded coding)*

| Lesson | Topic |
|--------|-------|
| [4.1](units/unit-4/lesson-1/README.md) | Embed the workbench (`AddBowire` / `MapBowire`) |
| [4.2](units/unit-4/lesson-2/README.md) | Embedded MCP adapter |
| [4.3](units/unit-4/lesson-3/README.md) | Interceptor middleware |

### Unit 5: Extend Bowire *(extension coding)*

| Lesson | Topic |
|--------|-------|
| [5.1](units/unit-5/lesson-1/README.md) | .NET protocol plugin (`IBowireProtocol`) |
| [5.2](units/unit-5/lesson-2/README.md) | Polyglot sidecar plugin |
| [5.3](units/unit-5/lesson-3/README.md) | UI extension — semantic kinds |
| [5.4](units/unit-5/lesson-4/README.md) | Plugin lifecycle |

### Capstones (per audience — each extends the Harbor domain)

| Capstone | Deliverable |
|---------|-------------|
| [User](capstones/user/README.md) | `.bww` workspace + diagnosis runbook |
| [Developer](capstones/developer/README.md) | NuGet plugin (protocol / extension / rail / module) |
| [Administrator](capstones/administrator/README.md) | `docker-compose.yml` / k8s + production runbook |

---

## Layout per lesson

```
units/unit-N/lesson-M/
├── README.md     # the walkthrough — open this first
├── start/        # scaffold to build on (coding lessons only)
└── completed/    # reference/finished state (coding lessons only)
```

UI and CLI lessons carry no code folders — they drive the shared samples from `Bowire.Samples`. One modality per unit; where another modality is relevant, a lesson **links** to its sibling unit instead of opening a second track inline.

## Hosting

The published Bootcamp lives at **https://bowire.io/bootcamp/**. The DocFX output is built by the main Bowire repo's `Documentation` workflow, which clones this repo and mounts `artifacts/docs-learn/` under `/bootcamp/`. `.github/workflows/notify-bowire.yml` fires a `bootcamp-updated` dispatch at `Kuestenlogik/Bowire` when lesson content lands on `main`, so bowire.io refreshes immediately.

## License

Apache 2.0 — same as the main Bowire repo. Lessons + samples can be lifted into your own training material; please link back when you do.
