# Learning Paths

Six role-based paths through the Bowire Bootcamp. Or, complete all units in order for the full picture (~3 hours).

Every path lists its **path-level prerequisites** (the toolchain you need before starting any of its units). Individual lessons also list their own prerequisites — those are subsets of the path-level list.

> **First decision — pick your deployment shape.** Bowire ships in two: standalone **CLI** (point at any URL) and **embedded** (`AddBowire()` + `MapBowire()` in your own ASP.NET service). [Unit 0 → Lesson 0.1](units/unit-0/lesson-1/README.md) breaks the choice down with diagrams. Most paths below work with either shape; **Path #6 (Embedded Backend Workflow)** is the embedded-native walkthrough for backend devs mounting the workbench inside their service.

---

## 1. Workbench Fundamentals

**For:** All API developers picking up Bowire for the first time.
**Duration:** ~45 min
**Units:** 3

**Prerequisites:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download) — for the sample REST + gRPC backends.
- A modern web browser — workbench UI runs in-browser, opens automatically.

| # | Unit | Why it matters |
|---|------|----------------|
| 1 | [Unit 0: Introduction](units/unit-0/README.md) | What Bowire is, how it positions vs Postman/Insomnia/Bruno, install verification |
| 2 | [Unit 1: Workbench Basics](units/unit-1/README.md) | First REST call, multi-protocol session (REST + gRPC discovered side-by-side) |
| 3 | [Unit 2.1: Record & Replay](units/unit-2/lesson-1/README.md) | Capture a session, save as `.bwr`, replay through `bowire mock` |

---

## 2. Mock-as-Stand-In

**For:** Frontend developers, QA engineers, integration testers who need a self-contained mock backend.
**Duration:** ~30 min
**Units:** 2

**Prerequisites:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download) — the bootcamp's sample backend uses it; production frontend devs can swap in their own.
- Bowire CLI: `dotnet tool install --global Kuestenlogik.Bowire.Tool`.

| # | Unit | Why it matters |
|---|------|----------------|
| 1 | [Unit 2.1: Record & Replay](units/unit-2/lesson-1/README.md) | Capture once, replay forever — no live backend required |
| 2 | [Unit 2.2: Schema Export + Mock-as-Stand-In](units/unit-2/lesson-2/README.md) | The mock serves `/openapi.json` of the original contract, so peer Bowires discover the *full* surface, not just the replayed slice |

---

## 3. AI & Automation

**For:** Agent builders, LLM integrators, prompt engineers wiring Bowire into Claude Desktop / Cursor / a custom MCP host.
**Duration:** ~20 min
**Units:** 2

**Prerequisites:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download) + Bowire CLI.
- **Claude Desktop** or **Cursor** installed (any other MCP-aware host also works — the config snippet is portable).

| # | Unit | Why it matters |
|---|------|----------------|
| 1 | [Unit 1: Workbench Basics](units/unit-1/README.md) | Understand Bowire's discovery + invoke surface first (the toolset the agent will drive) |
| 2 | [Unit 3.1: AI-Agent Integration](units/unit-3/lesson-1/README.md) | `bowire mcp serve` over stdio, wire it into the agent's MCP config, drive REST + gRPC from a chat window |

---

## 4. Plugin Author

**For:** Protocol-plugin authors — anyone shipping a new protocol on top of Bowire's host.
**Duration:** ~50 min
**Units:** 3

**Prerequisites:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download) + Bowire CLI.
- **Python 3.10+** + `pip` — only for the Unit 4.2 sidecar lesson. Unit 4.1 (.NET plugin) needs nothing beyond the SDK.
- [`dotnet new bowire-plugin`](https://github.com/Kuestenlogik/Bowire.Templates) template installed: `dotnet new install Kuestenlogik.Bowire.Templates`.

| # | Unit | Why it matters |
|---|------|----------------|
| 1 | [Unit 1: Workbench Basics](units/unit-1/README.md) | Understand how the host discovers and renders a protocol (REST + gRPC reference) |
| 2 | [Unit 4.1: .NET Protocol Plugin](units/unit-4/lesson-1/README.md) | Implement `IBowireProtocol`, `dotnet pack`, `bowire plugin install` |
| 3 | [Unit 4.2: Python Sidecar Plugin](units/unit-4/lesson-2/README.md) | Same surface, polyglot — `BowirePlugin` subclass, JSON-RPC stdio, ship as a zip |

---

## 5. Production / CI

**For:** DevOps, SREs, platform engineers folding Bowire into CI pipelines, sidecar deployments, &c.
**Duration:** ~40 min
**Units:** 2

**Prerequisites:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download) + Bowire CLI.
- **Docker** (for the CI examples that run the mock as a service container — optional in local dev).
- A **GitHub repository** you can push the sample workflow to (any other CI runner works analogously; the lesson examples are GitHub Actions).

| # | Unit | Why it matters |
|---|------|----------------|
| 1 | [Unit 2.1: Record & Replay](units/unit-2/lesson-1/README.md) | Recordings as portable test fixtures |
| 2 | [Unit 5.1: CI Integration](units/unit-5/lesson-1/README.md) | `bowire test` in GitHub Actions, mock-server as a Job service for downstream integration tests |

---

## 6. Embedded Backend Workflow

**For:** Backend developers and internal-tools teams who mount the workbench *inside* their own ASP.NET service via `AddBowire()` + `MapBowire()` — workbench inherits the host's DI container, `[Authorize]` policies, `IOptions<T>` config, and logging providers, so what the workbench discovers is exactly what production exposes.
**Duration:** ~50 min
**Units:** 3

**Prerequisites:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download).
- An ASP.NET host to mount Bowire into — either your own service, or scaffold a fresh one with `dotnet new web` (Lesson 0.2 walks the scaffold).
- `Kuestenlogik.Bowire` NuGet referenced from the host (`dotnet add package Kuestenlogik.Bowire`). No separate `bowire` CLI install required for this path, though the CLI is handy to keep around for external targets.

| # | Unit | Why it matters |
|---|------|----------------|
| 1 | [Unit 0: Introduction](units/unit-0/README.md) | Pick Path B in Lesson 0.2; mount the workbench in two lines |
| 2 | [Unit 1: Workbench Basics](units/unit-1/README.md) | First call against your own in-process service — same workbench surface as the CLI path, no separate process |
| 3 | [Unit 2.1: Record & Replay](units/unit-2/lesson-1/README.md) | Capture sessions against your own service; replay through `bowire mock` (CLI) for downstream consumers |

**Where embedded beats CLI:**
- The workbench inherits your auth pipeline. A method behind `[Authorize(Policy="…")]` needs the same token; no parallel credentials store.
- Discovery reads gRPC reflection / OpenAPI document provider / SignalR hub registry directly through DI — faster, no schema-version drift.
- Ship a debug UI *with* your binary — no separate distribution, no companion container.

**Where CLI still beats embedded:**
- External targets you don't control.
- Air-gapped or CI environments where adding a NuGet to the host isn't an option.
- MCP agents driving many services simultaneously — one CLI instance, many `--url` targets.

> **Coverage status.** Unit 0 is the only one with full Path-A / Path-B coverage today. Other units still write samples for the CLI path by default; [Kuestenlogik/Bowire#55](https://github.com/Kuestenlogik/Bowire/issues/55) tracks bringing the embedded path to lesson-by-lesson parity. The embedded code path is fully supported in the product — the bootcamp just hasn't finished documenting every variant yet.

---

## Or: the full bootcamp

Complete every unit in order — ~3 hours, ~9 lessons, plus the capstone.

**Prerequisites (everything):**
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Bowire CLI: `dotnet tool install --global Kuestenlogik.Bowire.Tool`
- `dotnet new bowire-plugin` template: `dotnet new install Kuestenlogik.Bowire.Templates`
- **Python 3.10+** (Unit 4.2 only)
- **Claude Desktop** or **Cursor** (Unit 3.1 only)
- **Docker** (Unit 5.1 only — optional in local dev)
- A **GitHub repository** (Unit 5.1 only)

- [Unit 0](units/unit-0/README.md) → [Unit 1](units/unit-1/README.md) → [Unit 2](units/unit-2/README.md) → [Unit 3](units/unit-3/README.md) → [Unit 4](units/unit-4/README.md) → [Unit 5](units/unit-5/README.md) → [Capstone](capstone/README.md)
