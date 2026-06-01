# Bowire Bootcamp

A hands-on, self-paced tutorial that takes you from zero to "I can ship a new protocol plugin" with [Bowire](https://bowire.io) — Küstenlogik's multi-protocol API workbench. 6 units, 9 lessons, 5 learning paths, 1 capstone.

## Goal

Learn to drive Bowire end-to-end — discover any wire, capture sessions, replay through `bowire mock`, integrate AI agents, and ship your own protocol plugin in .NET or Python.

## What is Bowire Bootcamp?

A linear, self-contained learning experience. Each lesson is runnable in 5–15 minutes and includes:

- **Concepts** — what you're learning and where it fits.
- **Exercises** — hands-on steps you can copy-paste and execute.
- **Sample code** — runnable .NET / Python / shell samples next to every lesson.
- **Knowledge assessment** — a short quiz at the end of each lesson to lock in the takeaways.

The [reference docs](https://bowire.io/docs/) answer **what does X do**. This bootcamp answers **how do I go from zero to productive**.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- Bowire CLI: `dotnet tool install --global Kuestenlogik.Bowire.Tool`
- Docker (only for Unit 5 — CI integration)
- Python 3.10+ (only for Unit 4.2 — sidecar plugin)
- Claude Desktop or Cursor (only for Unit 3.1 — AI agents)

## Getting Started

```bash
# Clone the repository
git clone https://github.com/Kuestenlogik/Bowire.Bootcamp.git
cd Bowire.Bootcamp

# Verify the bowire CLI
bowire --version

# Start with the introduction
cd units/unit-0/lesson-1
```

### DocFX Learn Site

The bootcamp ships a DocFX-powered learn site:

```bash
./scripts/build-learn.ps1 -Serve -Port 8082
```

## Learning Paths

Choose from 5 structured paths based on your role and goals:

| Path | Target Audience | Duration | Units |
|------|-----------------|----------|-------|
| **Workbench Fundamentals** | All API developers (start here) | ~45 min | 3 units |
| **Mock-as-Stand-In** | Frontend devs, QA engineers | ~30 min | 2 units |
| **AI & Automation** | Agent / LLM builders | ~20 min | 2 units |
| **Plugin Author** | Protocol-plugin authors (.NET + polyglot) | ~50 min | 3 units |
| **Production / CI** | DevOps, platform engineers | ~40 min | 2 units |

Or complete all units in order for the full picture (~3 hours).

**[View Detailed Learning Paths →](LEARNING_PATHS.md)**

---

## Curriculum

### Unit 0: Introduction
*Time: ~30 minutes*

Understand what Bowire is and verify your environment.

| Lesson | Topic | What You'll Learn |
|--------|-------|-------------------|
| [0.1](units/unit-0/lesson-1/README.md) | What is Bowire? | Multi-protocol API workbench, Bowire vs Postman / Insomnia / Bruno |
| [0.2](units/unit-0/lesson-2/README.md) | Setup | Install the bowire CLI, the bundled plugins, the workbench host |
| [0.3](units/unit-0/lesson-3/README.md) | Hello Bowire | Launch the workbench, point it at a public REST API, invoke your first method |

### Unit 1: Workbench Basics
*Time: ~15 minutes*

Drive Bowire's discovery and invoke surface across two protocols at once.

| Lesson | Topic | What You'll Build |
|--------|-------|-------------------|
| [1.1](units/unit-1/lesson-1/README.md) | First call (REST + OpenAPI) | A REST sample API, discovered through its OpenAPI doc, invoked from the workbench |
| [1.2](units/unit-1/lesson-2/README.md) | Multi-protocol session (REST + gRPC) | A gRPC sample side-by-side with REST, both discovered automatically, server-streaming demo |

### Unit 2: Record, Replay, Mock
*Time: ~20 minutes*

Capture sessions and run them back as local mock servers — with the original contract reattached.

| Lesson | Topic | What You'll Build |
|--------|-------|-------------------|
| [2.1](units/unit-2/lesson-1/README.md) | Record & replay | A `.bwr` recording, replayed through `bowire mock` so the mock answers without the real backend running |
| [2.2](units/unit-2/lesson-2/README.md) | Schema export + mock-as-stand-in | The recording carries the source OpenAPI, the mock serves `/openapi.json`, a peer Bowire discovers the *full* surface through it |

### Unit 3: AI-Agent Integration
*Time: ~10 minutes*

Hand the workbench to a language model.

| Lesson | Topic | What You'll Build |
|--------|-------|-------------------|
| [3.1](units/unit-3/lesson-1/README.md) | Claude Desktop + Cursor over MCP | `bowire mcp serve` wired into the agent's `mcpServers` config, drive REST + gRPC + recordings from the chat window |

### Unit 4: Extending Bowire
*Time: ~30 minutes*

Author your own protocol plugin — once in .NET, once in Python.

| Lesson | Topic | What You'll Build |
|--------|-------|-------------------|
| [4.1](units/unit-4/lesson-1/README.md) | .NET protocol plugin | A Pirate-Speak `IBowireProtocol` packaged via `dotnet pack` → `bowire plugin install` |
| [4.2](units/unit-4/lesson-2/README.md) | Python sidecar plugin | A Yoda-Speak `BowirePlugin` subclass packaged as a sidecar `.zip` → `bowire plugin install --file` |

### Unit 5: CI Integration
*Time: ~15 minutes*

Fold Bowire into your CI pipeline as a regression-test runner and mock-server fixture.

| Lesson | Topic | What You'll Build |
|--------|-------|-------------------|
| [5.1](units/unit-5/lesson-1/README.md) | GitHub Actions integration | `bowire test` step running recordings as assertions, mock-server as a job service for downstream integration tests |

### Capstone

| Project | Title |
|---------|-------|
| [Capstone](capstone/README.md) | Multi-Protocol API Tour — combine recording, mocking, plugins, CI into an end-to-end scenario |

---

## Layout per lesson

Every lesson directory looks the same:

```
units/unit-N/lesson-M/
├── README.md              # The walkthrough text — open this first
├── KNOWLEDGE_ASSESSMENT.md  # Short quiz that locks in the takeaways
└── sample/                  # Runnable .NET / Python / shell sample (when applicable)
```

So you can `cd units/unit-N/lesson-M/sample && dotnet run` (or `python -m sample`) and follow along without leaving the directory.

## Hosting

The published Bootcamp lives at **https://bowire.io/bootcamp/**. The DocFX
output is built by the main Bowire repo's `Documentation` workflow, which
clones this repo and mounts the `artifacts/docs-learn/` output under
`/bootcamp/` in its combined GitHub Pages bundle.

`.github/workflows/notify-bowire.yml` fires a `bootcamp-updated`
`repository_dispatch` at `Kuestenlogik/Bowire` whenever lesson content
lands on `main`, so the bowire.io deploy refreshes immediately — no daily
polling cron. The dispatch needs a PAT stored as `BOWIRE_DISPATCH_TOKEN`
in this repo's Actions secrets, scoped `repo` on `Kuestenlogik/Bowire`.
A path filter on the trigger keeps internal infra-edits (release scripts,
gitignore tweaks, &c.) from re-deploying bowire.io unnecessarily.

## License

Apache 2.0 — same as the main Bowire repo. Lessons + samples can be lifted into your own training material; please link back when you do.
