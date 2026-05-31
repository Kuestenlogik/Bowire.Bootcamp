# Learning Paths

Five role-based paths through the Bowire Bootcamp. Or, complete all units in order for the full picture (~3 hours).

---

## 1. Workbench Fundamentals

**For:** All API developers picking up Bowire for the first time.
**Duration:** ~45 min
**Units:** 3

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

| # | Unit | Why it matters |
|---|------|----------------|
| 1 | [Unit 2.1: Record & Replay](units/unit-2/lesson-1/README.md) | Capture once, replay forever — no live backend required |
| 2 | [Unit 2.2: Schema Export + Mock-as-Stand-In](units/unit-2/lesson-2/README.md) | The mock serves `/openapi.json` of the original contract, so peer Bowires discover the *full* surface, not just the replayed slice |

---

## 3. AI & Automation

**For:** Agent builders, LLM integrators, prompt engineers wiring Bowire into Claude Desktop / Cursor / a custom MCP host.
**Duration:** ~20 min
**Units:** 2

| # | Unit | Why it matters |
|---|------|----------------|
| 1 | [Unit 1: Workbench Basics](units/unit-1/README.md) | Understand Bowire's discovery + invoke surface first (the toolset the agent will drive) |
| 2 | [Unit 3.1: AI-Agent Integration](units/unit-3/lesson-1/README.md) | `bowire mcp serve` over stdio, wire it into the agent's MCP config, drive REST + gRPC from a chat window |

---

## 4. Plugin Author

**For:** Protocol-plugin authors — anyone shipping a new protocol on top of Bowire's host.
**Duration:** ~50 min
**Units:** 3

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

| # | Unit | Why it matters |
|---|------|----------------|
| 1 | [Unit 2.1: Record & Replay](units/unit-2/lesson-1/README.md) | Recordings as portable test fixtures |
| 2 | [Unit 5.1: CI Integration](units/unit-5/lesson-1/README.md) | `bowire test` in GitHub Actions, mock-server as a Job service for downstream integration tests |

---

## Or: the full bootcamp

Complete every unit in order — ~3 hours, ~9 lessons, plus the capstone.

- [Unit 0](units/unit-0/README.md) → [Unit 1](units/unit-1/README.md) → [Unit 2](units/unit-2/README.md) → [Unit 3](units/unit-3/README.md) → [Unit 4](units/unit-4/README.md) → [Unit 5](units/unit-5/README.md) → [Capstone](capstone/README.md)
