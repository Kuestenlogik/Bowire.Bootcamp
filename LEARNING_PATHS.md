# Learning Paths

Five role-based paths through the Bowire Bootcamp. Or, complete all units in order for the full picture (~3 hours).

Every path lists its **path-level prerequisites** (the toolchain you need before starting any of its units). Individual lessons also list their own prerequisites — those are subsets of the path-level list.

> **First decision — pick your deployment shape.** Unit 0 introduces the two:
>
> - **CLI (two-process)** — `bowire --url <service>` from a terminal. Bowire is its own process; your service is untouched.
> - **Embedded (single-process)** — `AddBowire()` + `MapBowire()` inside your ASP.NET host. Workbench mounts at `/bowire` on the host's own port.
>
> Unit 1 splits into two parallel **setup tracks** — [CLI](units/unit-1-cli/README.md) or [Embedded](units/unit-1-embedded/README.md). Pick one based on the shape that matches your deployment.
>
> From Unit 2 onwards the tracks merge — the workbench UI is identical regardless of how Bowire got mounted, so recording, mocking, AI integration, plugin authoring, and CI work are shared.

---

## 1. Workbench Fundamentals

**For:** All API developers picking up Bowire for the first time.
**Duration:** ~45 min
**Units:** 3

**Prerequisites:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download) — for the sample REST + gRPC backends.
- A modern web browser — workbench UI runs in-browser, opens automatically.
- Either the `bowire` CLI installed (CLI track) **or** an ASP.NET host you can `dotnet add package Kuestenlogik.Bowire` into (Embedded track).

| # | Unit | Why it matters |
|---|------|----------------|
| 1 | [Unit 0: Introduction](units/unit-0/README.md) | What Bowire is, how it positions vs Postman/Insomnia/Bruno, install verification, **pick your setup track** |
| 2 | Unit 1 — [CLI track](units/unit-1-cli/README.md) **or** [Embedded track](units/unit-1-embedded/README.md) | First REST call + multi-protocol session (REST + gRPC), walked through your chosen setup track |
| 3 | [Unit 2.1: Record & Replay](units/unit-2/lesson-1/README.md) | Capture a session, save as `.bwr`, replay through `bowire mock` |

---

## 2. Mock-as-Stand-In

**For:** Frontend developers, QA engineers, integration testers who need a self-contained mock backend.
**Duration:** ~30 min
**Units:** 2

**Prerequisites:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download) — the bootcamp's sample backend uses it; production frontend devs can swap in their own.
- Bowire CLI: `dotnet tool install --global Kuestenlogik.Bowire.Tool`. (Mock-server is a CLI-only feature, so this path defaults to the CLI shape regardless of what your backend team uses.)

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
- [.NET 10 SDK](https://dotnet.microsoft.com/download) + Bowire CLI (or embedded host).
- **Claude Desktop** or **Cursor** installed (any other MCP-aware host also works — the config snippet is portable).

| # | Unit | Why it matters |
|---|------|----------------|
| 1 | Unit 1 — [CLI track](units/unit-1-cli/README.md) **or** [Embedded track](units/unit-1-embedded/README.md) | Understand Bowire's discovery + invoke surface first (the toolset the agent will drive) |
| 2 | [Unit 3.1: AI-Agent Integration](units/unit-3/lesson-1/README.md) | `bowire mcp serve` over stdio (CLI shape), or `MapBowireMcpAdapter()` HTTP MCP (embedded shape) — wire it into the agent's MCP config, drive REST + gRPC from a chat window |

---

## 4. Plugin Author

**For:** Protocol-plugin authors — anyone shipping a new protocol on top of Bowire's host.
**Duration:** ~50 min
**Units:** 3

**Prerequisites:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download) + Bowire CLI (or embedded host).
- **Python 3.10+** + `pip` — only for the Unit 4.2 sidecar lesson. Unit 4.1 (.NET plugin) needs nothing beyond the SDK.
- [`dotnet new bowire-plugin`](https://github.com/Kuestenlogik/Bowire.Templates) template installed: `dotnet new install Kuestenlogik.Bowire.Templates`.

| # | Unit | Why it matters |
|---|------|----------------|
| 1 | Unit 1 — [CLI track](units/unit-1-cli/README.md) **or** [Embedded track](units/unit-1-embedded/README.md) | Understand how the host discovers and renders a protocol (REST + gRPC reference) |
| 2 | [Unit 4.1: .NET Protocol Plugin](units/unit-4/lesson-1/README.md) | Implement `IBowireProtocol`, `dotnet pack`, install via `bowire plugin install` (CLI) or `PackageReference` (Embedded) |
| 3 | [Unit 4.2: Python Sidecar Plugin](units/unit-4/lesson-2/README.md) | Same surface, polyglot — `BowirePlugin` subclass, JSON-RPC stdio, ship as a zip |

---

## 5. Production / CI

**For:** DevOps, SREs, platform engineers folding Bowire into CI pipelines, sidecar deployments, &c.
**Duration:** ~40 min
**Units:** 2

**Prerequisites:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download) + Bowire CLI. (CI workflows always run the CLI shape — there's no in-process host to mount the workbench into at CI time.)
- **Docker** (for the CI examples that run the mock as a service container — optional in local dev).
- A **GitHub repository** you can push the sample workflow to (any other CI runner works analogously; the lesson examples are GitHub Actions).

| # | Unit | Why it matters |
|---|------|----------------|
| 1 | [Unit 2.1: Record & Replay](units/unit-2/lesson-1/README.md) | Recordings as portable test fixtures |
| 2 | [Unit 5.1: CI Integration](units/unit-5/lesson-1/README.md) | `bowire test` in GitHub Actions, mock-server as a Job service for downstream integration tests |

---

## Or: the full bootcamp

Complete every unit in order — ~3 hours, ~9 lessons, plus the capstone.

**Prerequisites (everything):**
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- For the **CLI track of Unit 1** *and* Unit 5 (always CLI): Bowire CLI — `dotnet tool install --global Kuestenlogik.Bowire.Tool`
- For the **Embedded track of Unit 1**: ASP.NET host (own, or scaffold via `dotnet new web`) + `dotnet add package Kuestenlogik.Bowire`
- `dotnet new bowire-plugin` template: `dotnet new install Kuestenlogik.Bowire.Templates`
- **Python 3.10+** (Unit 4.2 only)
- **Claude Desktop** or **Cursor** (Unit 3.1 only)
- **Docker** (Unit 5.1 only — optional in local dev)
- A **GitHub repository** (Unit 5.1 only)

[Unit 0](units/unit-0/README.md) → Unit 1 ([CLI](units/unit-1-cli/README.md) or [Embedded](units/unit-1-embedded/README.md)) → [Unit 2](units/unit-2/README.md) → [Unit 3](units/unit-3/README.md) → [Unit 4](units/unit-4/README.md) → [Unit 5](units/unit-5/README.md) → [Capstone](capstone/README.md)
